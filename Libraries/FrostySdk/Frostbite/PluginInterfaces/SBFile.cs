using FMT.FileTools;
using FrostySdk;
using FrostySdk.Deobfuscators;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using ModdingSupport;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace FrostySdk.Frostbite.PluginInterfaces
{
    public class SBFile : IDisposable
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

        //private TocSbReader_FIFA21 ParentReader;

        public bool DoLogging = true;

        public bool ProcessData = true;

        /// <summary>
        /// Reads the TOC data via the stream provided with optional logging and processing
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="log"></param>
        /// <param name="process"></param>
        public SBFile(string nativePath, TOCFile tocFile, bool log = true, bool process = true, bool useModDataPath = false)
        {
            NativeFileLocation = nativePath;
            FileLocation = FileSystem.Instance.ResolvePath(nativePath, useModDataPath);

            AssociatedTOCFile = tocFile;
            DoLogging = log;
            ProcessData = process;
            using (NativeReader reader = new NativeReader(new FileStream(FileLocation, FileMode.Open)))
                Read(reader);
        }

        public List<DbObject> DbObjects { get; } = new List<DbObject>();

        /// <summary>
        /// Reads the entire SBFile from the Associated TOC Bundles
        /// </summary>
        /// <param name="nativeReader"></param>
        /// <returns></returns>
        public List<DbObject> Read(NativeReader nativeReader)
        {
            var startOffset = nativeReader.Position;
            
            nativeReader.Position = startOffset;
            var index = 0;
            foreach (BaseBundleInfo BaseBundleItem in AssociatedTOCFile.Bundles)
            {

                if (DoLogging)
                {
                    var percentDone = Math.Round(((double)index / AssociatedTOCFile.Bundles.Count()) * 100).ToString();
                    if (AssetManager.Instance != null)
                        AssetManager.Instance.Logger.Log($"Loading data from {FileLocation} {percentDone}%");
                }

                DbObject dbObject = new DbObject(new Dictionary<string, object>());

                BundleEntry bundleEntry = new BundleEntry
                {
                    Name = AssociatedTOCFile.NativeFileLocation + "-" + BundleEntry.PersistedIndexCount,
                    SuperBundleId = SuperBundleIndex,
                    PersistedIndex = BundleEntry.PersistedIndexCount
                };
                using (NativeReader binarySbReader2 = new NativeReader(nativeReader.CreateViewStream(BaseBundleItem.Offset, BaseBundleItem.Size)))
                {
                    dbObject.SetValue("Bundle", BaseBundleItem);
                    dbObject.SetValue("BundleEntry", bundleEntry);
                    ReadInternalBundle((int)BaseBundleItem.Offset, ref dbObject, binarySbReader2);
                }

                if (AssetManager.Instance != null && ProcessData)
                    AssetManager.Instance.Bundles.Add(bundleEntry);

                BundleEntry.PersistedIndexCount++;

                DbObjects.Add(dbObject);
                index++;
                BaseBundleInfo.BundleItemIndex++;
            }

            return DbObjects;
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

        /// <summary>
        /// Reads the reader from a viewstream of the internal bundle
        /// </summary>
        /// <param name="BaseBundleItem"></param>
        /// <param name="dbObject"></param>
        /// <param name="binarySbReader2"></param>
        /// <returns></returns>
        public void ReadInternalBundle(int bundleOffset, ref DbObject dbObject, NativeReader binarySbReader2)
        {
            //CachedBundle = new CachingSBData.Bundle();
            //CachedBundle.StartOffset = (int)bundleOffset;
            //dbObject.SetValue("BundleStartOffset", CachedBundle.StartOffset);

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


            var bundleHeader = new BundleHeader()
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
            bundleHeader.unk3 = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("unk3", bundleHeader.unk3);

            bundleHeader.unk4 = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("unk4", bundleHeader.unk4);

            bundleHeader.unk5 = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("unk5", bundleHeader.unk5);


            // ---------------------------------------------------------------------------------------------------------------------
            // This is where it hits the Binary SB Reader. FIFA 21 is more like MADDEN 21 in this section

            var BinaryDataOffset = (int)binarySbReader2.Position;
            SBHeaderInformation SBHeaderInformation = new BinaryReader21().BinaryRead21(bundleOffset, ref dbObject, binarySbReader2, true);
            var BinaryDataOffsetEnd = (int)binarySbReader2.Position;
            dbObject.SetValue("BinarySize", BinaryDataOffsetEnd - 32);

            // END OF BINARY READER
            // ---------------------------------------------------------------------------------------------------------------------

            binarySbReader2.Position = casFileForGroupOffset;
            var BooleanOfCasGroupOffset = (int)binarySbReader2.Position;
            byte[] boolChangeOfCasData = new byte[SBHeaderInformation.totalCount];

            bool[] booleanChangeOfCas = new bool[SBHeaderInformation.totalCount];
            for (uint booleanIndex = 0u; booleanIndex < SBHeaderInformation.totalCount; booleanIndex++)
            {
                booleanChangeOfCas[booleanIndex] = binarySbReader2.ReadBoolean();
                boolChangeOfCasData[booleanIndex] = Convert.ToByte(booleanChangeOfCas[booleanIndex]);
            }
            dbObject.SetValue("BoolChangeOfCasData", boolChangeOfCasData);

            var BooleanOfCasGroupOffsetEnd = binarySbReader2.Position;

            binarySbReader2.Position = CatalogAndCASOffset;
            var CatalogCasGroupOffset = binarySbReader2.Position;

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
                if (AssociatedTOCFile != null)
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
                }


                o.SetValue("BundleIndex", BaseBundleInfo.BundleItemIndex);

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
                }
                o.SetValue("BundleIndex", BaseBundleInfo.BundleItemIndex);
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
                }
                o.SetValue("BundleIndex", BaseBundleInfo.BundleItemIndex);
            }

            for (int i = 0; i < ebxCount; i++)
            {
                DbObject obj = dbObject.GetValue<DbObject>("ebx")[i] as DbObject;
                EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
                ebxAssetEntry = (EbxAssetEntry)AssetLoaderHelpers.ConvertDbObjectToAssetEntry(obj, ebxAssetEntry);
                ebxAssetEntry.CASFileLocation = NativeFileLocation;
                ebxAssetEntry.TOCFileLocation = AssociatedTOCFile.NativeFileLocation;
                ebxAssetEntry.AddToBundle(Fnv1a.HashString(NativeFileLocation));
                if (AssociatedTOCFile.ProcessData)
                    AssetManager.Instance.AddEbx(ebxAssetEntry);
            }
            for (int i = 0; i < resCount; i++)
            {
                DbObject obj = dbObject.GetValue<DbObject>("res")[i] as DbObject;
            }
            for (int i = 0; i < chunkCount; i++)
            {
                DbObject obj = dbObject.GetValue<DbObject>("chunks")[i] as DbObject;
                ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
                chunkAssetEntry = (ChunkAssetEntry)AssetLoaderHelpers.ConvertDbObjectToAssetEntry(obj, chunkAssetEntry);
                chunkAssetEntry.CASFileLocation = NativeFileLocation;
                chunkAssetEntry.TOCFileLocation = AssociatedTOCFile.NativeFileLocation;
                chunkAssetEntry.AddToBundle(Fnv1a.HashString(NativeFileLocation));
                if (AssociatedTOCFile.ProcessData)
                    AssetManager.Instance.AddChunk(chunkAssetEntry);
            }

        }

        public void Dispose()
        {
            //if (AssociatedTOCFile != null)
            //    AssociatedTOCFile.Dispose();

            AssociatedTOCFile = null;
        }

    }

}
