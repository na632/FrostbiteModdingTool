﻿using Frosty.Hash;
using FrostySdk;
using FrostySdk.Deobfuscators;
using FrostySdk.IO;
using FrostySdk.Managers;
using paulv2k4ModdingExecuter;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static FIFA21Plugin.FIFA21AssetLoader;

namespace FIFA21Plugin
{


    public class SBFile
    {
        public TOCFile AssociatedTOCFile { get; set; }
        public string NativeFileLocation { get; set; }
        public string FileLocation { get; set; }

        public int SBInitialHeaderLength = 32;
        public int SBInformationHeaderLength = 36;

        public int[] ArrayOfInitialHeaderData = new int[8];

        public SBHeaderInformation SBHeaderInformation { get; set; }

        List<BundleEntryInfo> Bundles = new List<BundleEntryInfo>();

        private int SuperBundleIndex = 0;

        private TocSbReader_FIFA21 ParentReader;

        public SBFile() { }

        public SBFile(TOCFile parentTOC)
        {
            AssociatedTOCFile = parentTOC;
        }

        public SBFile(TocSbReader_FIFA21 parent, TOCFile parentTOC, int sbIndex)
        {
            ParentReader = parent;
            AssociatedTOCFile = parentTOC;
            SuperBundleIndex = sbIndex;
        }

        public struct EBX
        {

        }

        public struct RES
        {

        }

        public struct CHUNK
        {

        }

        public bool DoLogging = true;


        public List<DbObject> Read()
        {
            if (AssociatedTOCFile == null)
                throw new FileNotFoundException("Unable to process SB file without knowing its location");

            var sbLocation = AssociatedTOCFile.FileLocation.Replace(".toc", ".sb", StringComparison.OrdinalIgnoreCase);
            return Read(new NativeReader(File.ReadAllBytes(sbLocation)));
        }

        /// <summary>
        /// Reads the entire SBFile from the Associated TOC Bundles
        /// </summary>
        /// <param name="nativeReader"></param>
        /// <returns></returns>
        public List<DbObject> Read(NativeReader nativeReader)
        {
            //AssetManager.Instance.logger.Log($"Loading data from {FileLocation}");

            //CachingSBData cachingSBData = new CachingSBData();
            //cachingSBData.SBFile = NativeFileLocation;


            List<DbObject> dbObjects = new List<DbObject>();

            var startOffset = nativeReader.Position;
            //#if DEBUG
            //            if (File.Exists("debugSB.dat"))
            //                File.Delete("debugSB.dat");
            //            using (NativeWriter writer = new NativeWriter(new FileStream("debugSB.dat", FileMode.OpenOrCreate)))
            //            {
            //                writer.Write(nativeReader.ReadToEnd());
            //            }
            //#endif
            nativeReader.Position = startOffset;
            var index = 0;
            foreach (BaseBundleInfo BaseBundleItem in AssociatedTOCFile.Bundles)
            {

                if (DoLogging)
                {
                    var percentDone = Math.Round(((double)index / AssociatedTOCFile.Bundles.Count) * 100).ToString();
                    if (AssetManager.Instance != null)
                        AssetManager.Instance.logger.Log($"Loading data from {FileLocation} {percentDone}%");
                }

                DbObject dbObject = new DbObject(new Dictionary<string, object>());

                BundleEntry bundleEntry = new BundleEntry
                {
                    Name = AssociatedTOCFile.NativeFileLocation + "-" + BundleEntry.PersistedIndexCount,
                    SuperBundleId = SuperBundleIndex,
                    PersistedIndex = BundleEntry.PersistedIndexCount
                };
                //using (NativeReader binarySbReader2 = new NativeReader(nativeReader.CreateViewStream(BaseBundleItem.Offset, nativeReader.Length - BaseBundleItem.Offset)))
                using (NativeReader binarySbReader2 = new NativeReader(nativeReader.CreateViewStream(BaseBundleItem.Offset, BaseBundleItem.Size)))
                {
                    dbObject.SetValue("Bundle", BaseBundleItem);
                    dbObject.SetValue("BundleEntry", bundleEntry);
                    CachingSBData.Bundle cachingSBDataBundle = ReadInternalBundle((int)BaseBundleItem.Offset, ref dbObject, binarySbReader2);
                    cachingSBDataBundle.BaseBundleItem = BaseBundleItem;
                    //cachingSBData.Bundles.Add(cachingSBDataBundle);
                }

                if (AssetManager.Instance != null && ParentReader.ProcessData)
                    AssetManager.Instance.bundles.Add(bundleEntry);

                BundleEntry.PersistedIndexCount++;

                dbObjects.Add(dbObject);
                index++;
                FIFA21AssetLoader.BaseBundleInfo.BundleItemIndex++;
            }

            //CachingSB.CachingSBs.Add(cachingSBData);
            //CachingSB.Save();
            return dbObjects;
        }

        public Dictionary<DbObject, (int, int)> Write(List<DbObject> objs, out MemoryStream msNewFile)
        {
            msNewFile = new MemoryStream();

            var newBundleOffsets = new Dictionary<DbObject, (int, int)>();

            using (NativeWriter nw = new NativeWriter(msNewFile, true))
            {
                foreach (DbObject obj in objs)
                {
                    MemoryStream msNewInternalBundle = new MemoryStream();

                    var newBundlePosition = msNewFile.Position;
                    WriteInternalBundle(msNewInternalBundle, obj);
                    nw.Write(msNewInternalBundle.ToArray());

                    newBundleOffsets.Add(obj, ((int)newBundlePosition, msNewInternalBundle.ToArray().Length));
                }
            }
            return newBundleOffsets;
        }

        public class BundleHeader
        {
            public uint CatalogOffset { get; set; }
            public uint unk1 { get; set; }
            public uint casFileForGroupOffset { get; set; }
            public uint unk2 { get; set; }
            public uint CatalogAndCASOffset { get; set; }
            public uint unk3 { get; set; }
            public uint unk4 { get; set; }
            public uint unk5 { get; set; }

            public byte[] Write()
            {
                MemoryStream memoryStream = new MemoryStream();
                NativeWriter nw = new NativeWriter(memoryStream);
                nw.Write((int)CatalogOffset, Endian.Big);
                nw.Write((int)unk1, Endian.Big);
                nw.Write((int)casFileForGroupOffset, Endian.Big);
                nw.Write((int)unk2, Endian.Big);
                nw.Write((int)CatalogAndCASOffset, Endian.Big);
                nw.Write((int)unk3, Endian.Big);
                nw.Write((int)unk4, Endian.Big);
                nw.Write((int)unk5, Endian.Big);
                return memoryStream.ToArray();
            }

        }

        public CachingSBData.Bundle CachedBundle { get; set; }

        /// <summary>
        /// Reads the reader from a viewstream of the internal bundle
        /// </summary>
        /// <param name="BaseBundleItem"></param>
        /// <param name="dbObject"></param>
        /// <param name="binarySbReader2"></param>
        /// <returns></returns>
        public CachingSBData.Bundle ReadInternalBundle(int bundleOffset, ref DbObject dbObject, NativeReader binarySbReader2)
        {
            CachedBundle = new CachingSBData.Bundle();
            CachedBundle.StartOffset = (int)bundleOffset;
            dbObject.SetValue("BundleStartOffset", CachedBundle.StartOffset);

            uint CatalogOffset = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("CatalogOffset", CatalogOffset);

            uint EndOfMeta = binarySbReader2.ReadUInt(Endian.Big) + CatalogOffset - 4; // end of META
            dbObject.SetValue("EndOfMeta", EndOfMeta);

            uint casFileForGroupOffset = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("CasFileForGroupOffset", casFileForGroupOffset);

            var totalCount = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("unk2", totalCount);

            uint CatalogAndCASOffset = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("CatalogAndCASOffset", CatalogAndCASOffset);


            CachedBundle.BundleHeader = new BundleHeader()
            {
                CatalogOffset = CatalogOffset
                 ,
                unk1 = EndOfMeta
                 ,
                casFileForGroupOffset = casFileForGroupOffset
                 ,
                unk2 = totalCount
                 ,
                CatalogAndCASOffset = CatalogAndCASOffset
            };

            // read 3 unknowns 
            CachedBundle.BundleHeader.unk3 = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("unk3", CachedBundle.BundleHeader.unk3);

            CachedBundle.BundleHeader.unk4 = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("unk4", CachedBundle.BundleHeader.unk4);

            CachedBundle.BundleHeader.unk5 = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("unk5", CachedBundle.BundleHeader.unk5);


            // ---------------------------------------------------------------------------------------------------------------------
            // This is where it hits the Binary SB Reader. FIFA 21 is more like MADDEN 21 in this section

            CachedBundle.BinaryDataOffset = (int)binarySbReader2.Position;
            SBHeaderInformation SBHeaderInformation = new BinaryReader_FIFA21().BinaryRead_FIFA21(bundleOffset, ref dbObject, binarySbReader2, true);
            CachedBundle.BinaryDataOffsetEnd = (int)binarySbReader2.Position;
            dbObject.SetValue("BinarySize", CachedBundle.BinaryDataOffsetEnd - 32);

            // END OF BINARY READER
            // ---------------------------------------------------------------------------------------------------------------------

            binarySbReader2.Position = casFileForGroupOffset;
            CachedBundle.BooleanOfCasGroupOffset = (int)binarySbReader2.Position;
            byte[] boolChangeOfCasData = new byte[SBHeaderInformation.totalCount];

            bool[] booleanChangeOfCas = new bool[SBHeaderInformation.totalCount];
            for (uint booleanIndex = 0u; booleanIndex < SBHeaderInformation.totalCount; booleanIndex++)
            {
                booleanChangeOfCas[booleanIndex] = binarySbReader2.ReadBoolean();
                boolChangeOfCasData[booleanIndex] = Convert.ToByte(booleanChangeOfCas[booleanIndex]);
            }
            dbObject.SetValue("BoolChangeOfCasData", boolChangeOfCasData);

            CachedBundle.BooleanOfCasGroupOffsetEnd = binarySbReader2.Position;

            binarySbReader2.Position = CatalogAndCASOffset;
            CachedBundle.CatalogCasGroupOffset = binarySbReader2.Position;

            bool patchFlag = false;
            int unkInBatch1 = 0;
            int catalog = 0;
            int cas = 0;
            int flagIndex = 0;

            var ebxCount = dbObject.GetValue<DbObject>("ebx").Count;
            for (int ebxIndex = 0; ebxIndex < ebxCount; ebxIndex++)
            {
                if (booleanChangeOfCas[flagIndex++])
                {
                    unkInBatch1 = binarySbReader2.ReadByte();
                    patchFlag = binarySbReader2.ReadBoolean();
                    catalog = binarySbReader2.ReadByte();
                    cas = binarySbReader2.ReadByte();
                }
                DbObject o = dbObject.GetValue<DbObject>("ebx")[ebxIndex] as DbObject;

                o.SetValue("SBFileLocation", NativeFileLocation);
                if(AssociatedTOCFile != null)
                    o.SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);

                o.SetValue("SB_CAS_Offset_Position", binarySbReader2.Position + bundleOffset);
                int offset = binarySbReader2.ReadInt(Endian.Big);

                o.SetValue("SB_CAS_Size_Position", binarySbReader2.Position + bundleOffset);
                int size = binarySbReader2.ReadInt(Endian.Big);

                o.SetValue("catalog", catalog);
                o.SetValue("cas", cas);
                o.SetValue("offset", offset);
                o.SetValue("size", size);
                if (patchFlag)
                {
                    o.SetValue("patch", true);

                    CachedBundle.LastCAS = cas;
                    CachedBundle.LastCatalogId = catalog;
                }


                o.SetValue("BundleIndex", FIFA21AssetLoader.BaseBundleInfo.BundleItemIndex);

            }

            var resCount = dbObject.GetValue<DbObject>("res").Count;
            for (int indexRes = 0; indexRes < resCount; indexRes++)
            {

                if (booleanChangeOfCas[flagIndex++])
                {
                    var b = binarySbReader2.ReadByte();
                    patchFlag = binarySbReader2.ReadBoolean();
                    catalog = binarySbReader2.ReadByte();
                    cas = binarySbReader2.ReadByte();
                }
                DbObject o = dbObject.GetValue<DbObject>("res")[indexRes] as DbObject;
                o.SetValue("SB_CAS_Offset_Position", binarySbReader2.Position + bundleOffset);
                int offset = binarySbReader2.ReadInt(Endian.Big);
                o.SetValue("SB_CAS_Size_Position", binarySbReader2.Position + bundleOffset);
                int size = binarySbReader2.ReadInt(Endian.Big);

                o.SetValue("SBFileLocation", NativeFileLocation);
                if (AssociatedTOCFile != null)
                    o.SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);

                o.SetValue("catalog", catalog);
                o.SetValue("cas", cas);
                o.SetValue("offset", offset);
                o.SetValue("size", size);
                if (patchFlag)
                {
                    o.SetValue("patch", true);

                    CachedBundle.LastCAS = cas;
                    CachedBundle.LastCatalogId = catalog;
                }
                o.SetValue("BundleIndex", FIFA21AssetLoader.BaseBundleInfo.BundleItemIndex);
            }

            var chunkCount = dbObject.GetValue<DbObject>("chunks").Count;
            for (int indexChunk = 0; indexChunk < chunkCount; indexChunk++)
            {
                if (booleanChangeOfCas[flagIndex++])
                {
                    var b = binarySbReader2.ReadByte();
                    patchFlag = binarySbReader2.ReadBoolean();
                    catalog = binarySbReader2.ReadByte();
                    cas = binarySbReader2.ReadByte();
                }
                DbObject o = dbObject.GetValue<DbObject>("chunks")[indexChunk] as DbObject;

                o.SetValue("SB_CAS_Offset_Position", binarySbReader2.Position + bundleOffset);
                int offset = binarySbReader2.ReadInt(Endian.Big);
                o.SetValue("SB_CAS_Size_Position", binarySbReader2.Position + bundleOffset);
                int size = binarySbReader2.ReadInt(Endian.Big);

                o.SetValue("SBFileLocation", NativeFileLocation);
                if (AssociatedTOCFile != null)
                    o.SetValue("TOCFileLocation", AssociatedTOCFile.NativeFileLocation);
                o.SetValue("catalog", catalog);
                o.SetValue("cas", cas);
                o.SetValue("offset", offset);
                o.SetValue("size", size);
                if (patchFlag)
                {
                    o.SetValue("patch", true);

                    CachedBundle.LastCAS = cas;
                    CachedBundle.LastCatalogId = catalog;
                }
                o.SetValue("BundleIndex", FIFA21AssetLoader.BaseBundleInfo.BundleItemIndex);
            }

            for (int i = 0; i < ebxCount; i++)
            {
                DbObject obj = dbObject.GetValue<DbObject>("ebx")[i] as DbObject;
                CachedBundle.ListOfItems["ebx"].Add(obj["name"].ToString());
            }
            for (int i = 0; i < resCount; i++)
            {
                DbObject obj = dbObject.GetValue<DbObject>("res")[i] as DbObject;
                CachedBundle.ListOfItems["res"].Add(obj["name"].ToString());
            }
            for (int i = 0; i < chunkCount; i++)
            {
                DbObject obj = dbObject.GetValue<DbObject>("chunks")[i] as DbObject;
                CachedBundle.ListOfItems["chunk"].Add(obj["id"].ToString());
            }

            CachedBundle.CatalogCasGroupOffsetEnd = (int)binarySbReader2.Position;
            if(CachedBundle.CatalogCasGroupOffsetEnd == 0)
            {

            }
            binarySbReader2.Position = CachedBundle.CatalogCasGroupOffset;
            //cachingSBDataBundle.CatalogCasGroupData = binarySbReader2.ReadBytes((int)cachingSBDataBundle.CatalogCasGroupOffsetEnd - (int)cachingSBDataBundle.CatalogCasGroupOffset);


            return CachedBundle;
        }

        
        public void WriteInternalBundle(MemoryStream stream, DbObject bundle)
        {
            long sbStartingPosition = stream.Position;
            NativeWriter writer = new NativeWriter(stream);
            int totalCount = bundle.GetValue<DbObject>("ebx").List.Count + bundle.GetValue<DbObject>("res").List.Count + bundle.GetValue<DbObject>("chunks").List.Count;
            writer.Write((int)bundle.GetValue<int>("CatalogOffset"), Endian.Big);
            long headerStartPosition = stream.Position;
            writer.Write((int)bundle.GetValue<int>("EndOfMeta"), Endian.Big);
            writer.Write((int)bundle.GetValue<int>("CasFileForGroupOffset"), Endian.Big);
            writer.Write((int)totalCount, Endian.Big);
            writer.Write((int)bundle.GetValue<int>("CatalogAndCASOffset"), Endian.Big);
            writer.Write((int)bundle.GetValue<int>("unk3"), Endian.Big);
            writer.Write((int)bundle.GetValue<int>("unk4"), Endian.Big);
            writer.Write((int)bundle.GetValue<int>("unk5"), Endian.Big);
            //writer.Write((int)0, Endian.Big);
            //long bundleEndPosition = new BundleWriter_F21().Write(stream, bundle);


            new BinaryWriter_FIFA21().Write(bundle, stream);
            long bundleEndPosition = stream.Position;
            long binarySize = stream.Position - 32;

            byte[] flags = ArrayPool<byte>.Shared.Rent(totalCount);
            int currentCasIdentifier = -1;
            long startOfEntryDataOffset = writer.Position - 32 - sbStartingPosition;
            int entryIndex = 0;
            foreach (DbObject entry in bundle.GetValue<DbObject>("ebx").List.Concat(bundle.GetValue<DbObject>("res").List).Concat(bundle.GetValue<DbObject>("chunks").List))
            {
                int entryCasIdentifier = CreateCasIdentifier(entry.GetValue<byte>("unk"), entry.HasValue("patch"), entry.GetValue<byte>("catalog"), entry.GetValue<byte>("cas"));
                _ = 0;
                if (entryCasIdentifier != currentCasIdentifier)
                {
                    writer.Write((int)entryCasIdentifier);
                    flags[entryIndex] = 1;
                    currentCasIdentifier = entryCasIdentifier;
                }
                else
                {
                    flags[entryIndex] = 0;
                }
                writer.Write((uint)entry.GetValue<uint>("offset"), Endian.Big);
                writer.Write((uint)entry.GetValue<int>("size"), Endian.Big);
                entryIndex++;
            }
            long startOfFlagsOffset = writer.Position - sbStartingPosition;
            writer.WriteBytes(flags);
            ArrayPool<byte>.Shared.Return(flags);
            long endPosition = writer.Position;
            writer.Position = headerStartPosition;
            writer.Write((int)(int)startOfEntryDataOffset, Endian.Big);
            writer.Write((int)(int)startOfFlagsOffset, Endian.Big);
            writer.Write((int)totalCount, Endian.Big);
            writer.Write((int)(int)startOfEntryDataOffset + 32, Endian.Big);
            writer.Write((int)(int)startOfEntryDataOffset + 32, Endian.Big);
            writer.Write((int)(int)startOfEntryDataOffset + 32, Endian.Big);
            //writer.Write((int)(int)bundleEndPosition - 36, Endian.Big);
            writer.Write(0, Endian.Big);
            writer.Position = endPosition;
        }
        

        public static int CreateCasIdentifier(byte unk, bool isPatch, byte packageIndex, byte casIndex)
        {
            return (unk << 24) | ((isPatch ? 1 : 0) << 16) | (packageIndex << 8) | casIndex;
        }

    }

}
