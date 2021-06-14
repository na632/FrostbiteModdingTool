using Frosty.Hash;
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
                    AssetManager.Instance.logger.Log($"Loading data from {FileLocation} {percentDone}%");
                }

                DbObject dbObject = new DbObject(new Dictionary<string, object>());

                BundleEntry bundleEntry = new BundleEntry
                {
                    Name = Guid.NewGuid().ToString(),
                    SuperBundleId = SuperBundleIndex,
                };
                //using (NativeReader binarySbReader2 = new NativeReader(nativeReader.CreateViewStream(BaseBundleItem.Offset, nativeReader.Length - BaseBundleItem.Offset)))
                using (NativeReader binarySbReader2 = new NativeReader(nativeReader.CreateViewStream(BaseBundleItem.Offset, BaseBundleItem.Size)))
                {
                    CachingSBData.Bundle cachingSBDataBundle = ReadInternalBundle((int)BaseBundleItem.Offset, ref dbObject, binarySbReader2);
                    cachingSBDataBundle.BaseBundleItem = BaseBundleItem;
                    //cachingSBData.Bundles.Add(cachingSBDataBundle);

                }

                AssetManager.Instance.bundles.Add(bundleEntry);
                dbObjects.Add(dbObject);
                index++;
                FIFA21AssetLoader.BaseBundleInfo.BundleItemIndex++;
            }

            //CachingSB.CachingSBs.Add(cachingSBData);
            //CachingSB.Save();
            return dbObjects;
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
            //if (File.Exists("debugSBViewStream.dat"))
            //    File.Delete("debugSBViewStream.dat");
            //using (NativeWriter writer = new NativeWriter(new FileStream("debugSBViewStream.dat", FileMode.OpenOrCreate)))
            //{
            //    writer.Write(binarySbReader2.ReadToEnd());
            //}
            //binarySbReader2.Position = 0;

            CachedBundle = new CachingSBData.Bundle();
            CachedBundle.StartOffset = (int)bundleOffset;
            dbObject.SetValue("BundleStartOffset", CachedBundle.StartOffset);

            uint CatalogOffset = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("CatalogOffset", CatalogOffset);

            uint EndOfMeta = binarySbReader2.ReadUInt(Endian.Big) + CatalogOffset - 4; // end of META
            dbObject.SetValue("EndOfMeta", EndOfMeta);

            uint casFileForGroupOffset = binarySbReader2.ReadUInt(Endian.Big);
            dbObject.SetValue("CasFileForGroupOffset", casFileForGroupOffset);

            var unk2 = binarySbReader2.ReadUInt(Endian.Big);

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
                unk2 = unk2
                 ,
                CatalogAndCASOffset = CatalogAndCASOffset
            };

            // read 3 unknowns 
            CachedBundle.BundleHeader.unk3 = binarySbReader2.ReadUInt(Endian.Big);
            CachedBundle.BundleHeader.unk4 = binarySbReader2.ReadUInt(Endian.Big);
            CachedBundle.BundleHeader.unk5 = binarySbReader2.ReadUInt(Endian.Big);

            // ---------------------------------------------------------------------------------------------------------------------
            // This is where it hits the Binary SB Reader. FIFA 21 is more like MADDEN 21 in this section

            CachedBundle.BinaryDataOffset = (int)binarySbReader2.Position;
            //SBHeaderInformation SBHeaderInformation = BinaryRead_FIFA21(nativeReader, BaseBundleItem, dbObject, binarySbReader2);
            SBHeaderInformation SBHeaderInformation = new BinaryReader_FIFA21().BinaryRead_FIFA21(bundleOffset, ref dbObject, binarySbReader2, true);
            CachedBundle.BinaryDataOffsetEnd = (int)binarySbReader2.Position;

            //binarySbReader2.Position = CachedBundle.BinaryDataOffset;
            //cachingSBDataBundle.BinaryDataData = binarySbReader2.ReadBytes((int)cachingSBDataBundle.BinaryDataOffsetEnd - (int)cachingSBDataBundle.BinaryDataOffset);

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

        /*
        public void WriteBundleFETWay(Stream stream, DbObject bundle)
        {
            long sbStartingPosition = stream.Position;
            NativeWriter writer = new NativeWriter(stream);
            int entriesCount = checked(bundle.GetValue<DbObject>("ebx").List.Count + bundle.GetValue<DbObject>("res").List.Count + bundle.GetValue<DbObject>("chunks").List.Count);
            writer.WriteInt32BigEndian(32);
            long headerStartPosition = stream.Position;
            writer.WriteInt32BigEndian(0);
            writer.WriteInt32BigEndian(0);
            writer.WriteInt32BigEndian(entriesCount);
            writer.WriteInt32BigEndian(0);
            writer.WriteInt32BigEndian(0);
            writer.WriteInt32BigEndian(0);
            writer.WriteInt32BigEndian(0);
            writer.WriteInt32BigEndian(0);
            long bundleEndPosition = new BundleWriter_F21().Write(stream, bundle);
            byte[] flags = ArrayPool<byte>.Shared.Rent(entriesCount);
            int currentCasIdentifier = -1;
            long startOfEntryDataOffset = writer.Position - 32 - sbStartingPosition;
            int entryIndex = 0;
            foreach (DbObject entry in bundle.GetValue<DbObject>("ebx").List.Concat(bundle.GetValue<DbObject>("res").List).Concat(bundle.GetValue<DbObject>("chunks").List))
            {
                int entryCasIdentifier = CreateCasIdentifier(entry.GetValue<byte>("unk"), entry.HasValue("patch"), entry.GetValue<byte>("catalog"), entry.GetValue<byte>("cas"));
                _ = 0;
                if (entryCasIdentifier != currentCasIdentifier)
                {
                    writer.WriteInt32BigEndian(entryCasIdentifier);
                    flags[entryIndex] = 1;
                    currentCasIdentifier = entryCasIdentifier;
                }
                else
                {
                    flags[entryIndex] = 0;
                }
                writer.WriteUInt32BigEndian(entry.GetValue<uint>("offset"));
                writer.WriteInt32BigEndian(entry.GetValue<int>("size"));
                entryIndex++;
            }
            long startOfFlagsOffset = writer.Position - sbStartingPosition;
            //writer.WriteBytes(flags, 0, entriesCount);
            writer.WriteBytes(flags);
            ArrayPool<byte>.Shared.Return(flags);
            long endPosition = writer.Position;
            writer.Position = headerStartPosition;
            if (startOfEntryDataOffset <= int.MaxValue && startOfFlagsOffset <= int.MaxValue && startOfEntryDataOffset + 32 <= int.MaxValue)
            {
                _ = bundleEndPosition - 36;
                _ = int.MaxValue;
            }
            writer.WriteInt32BigEndian((int)startOfEntryDataOffset);
            writer.WriteInt32BigEndian((int)startOfFlagsOffset);
            writer.WriteInt32BigEndian(entriesCount);
            writer.WriteInt32BigEndian((int)(startOfEntryDataOffset + 32));
            writer.WriteInt32BigEndian((int)(startOfEntryDataOffset + 32));
            writer.WriteInt32BigEndian((int)(startOfEntryDataOffset + 32));
            writer.WriteInt32BigEndian(0);
            writer.WriteInt32BigEndian((int)bundleEndPosition - 36);
            writer.Position = endPosition;
        }
        */

        public static int CreateCasIdentifier(byte unk, bool isPatch, byte packageIndex, byte casIndex)
        {
            return (unk << 24) | ((isPatch ? 1 : 0) << 16) | (packageIndex << 8) | casIndex;
        }

    }


    /*
    public class BundleWriter_F21
    {
        public long Write(Stream stream, DbObject bundle, bool padEnd = true)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (!stream.CanWrite)
            {
                throw new ArgumentException("Stream must support writing.", "stream");
            }
            if (!stream.CanSeek)
            {
                throw new ArgumentException("Stream must support seeking.", "stream");
            }
            if (bundle == null)
            {
                throw new ArgumentNullException("bundle");
            }
            NativeWriter writer = new NativeWriter(stream);
            long bundleStartOffset = writer.Position;
            int ebxEntries = bundle.GetValue<DbObject>("ebx").List.Count;
            int resEntries = bundle.GetValue<DbObject>("res").List.Count;
            int chunkEntries = bundle.GetValue<DbObject>("chunks").List.Count;
            int totalEntries = checked(ebxEntries + resEntries + chunkEntries);
            writer.WriteUInt32BigEndian(3599661469u);
            writer.WriteInt32LittleEndian(totalEntries);
            writer.WriteInt32LittleEndian(ebxEntries);
            writer.WriteInt32LittleEndian(resEntries);
            writer.WriteInt32LittleEndian(chunkEntries);
            long placeholderPosition = writer.Position;
            writer.WriteUInt32LittleEndian(0u);
            writer.WriteUInt32LittleEndian(0u);
            writer.WriteUInt32LittleEndian(0u);
            foreach (DbObject item in bundle.GetValue<DbObject>("ebx").List.Concat(bundle.GetValue<DbObject>("res").List).Concat(bundle.GetValue<DbObject>("chunks").List))
            {
                Sha1 hash = item.GetValue<Sha1>("sha1");
                writer.Write(hash);
            }
            uint entryNamesOffset = 0u;
            WriteEbx(writer, bundle.GetValue<DbObject>("ebx").List.Cast<DbObject>(), ref entryNamesOffset);
            WriteRes(writer, bundle.GetValue<DbObject>("res").List.Cast<DbObject>(), ref entryNamesOffset);
            WriteChunks(writer, bundle.GetValue<DbObject>("chunks").List.Cast<DbObject>());
            long stringsOffset = writer.Position - bundleStartOffset;
            foreach (DbObject entry in bundle.GetValue<DbObject>("ebx").List.Concat(bundle.GetValue<DbObject>("res").List))
            {
                writer.WriteNullTerminatedString(entry.GetValue<string>("name"));
            }
            long chunkMetaOffset = 0L;
            long chunkMetaSize = 0L;
            if (chunkEntries > 0)
            {
                chunkMetaOffset = writer.Position - bundleStartOffset;
                var chunkMetaBytes = new DbWriter(stream).WriteDbObject("chunkMeta", bundle.GetValue<DbObject>("chunkMeta"));
                writer.WriteBytes(chunkMetaBytes);
                chunkMetaSize = writer.Position - bundleStartOffset - chunkMetaOffset;
            }
            long endPosition = writer.Position;
            writer.Position = placeholderPosition;
            _ = uint.MaxValue;
            writer.WriteUInt32LittleEndian((uint)stringsOffset);
            if (chunkMetaOffset != 0L)
            {
                if (chunkMetaOffset <= uint.MaxValue)
                {
                    _ = uint.MaxValue;
                }
                writer.WriteUInt32LittleEndian((uint)chunkMetaOffset);
                writer.WriteUInt32LittleEndian((uint)chunkMetaSize);
            }
            else
            {
                writer.WriteUInt64LittleEndian(0uL);
            }
            writer.Position = endPosition;
            if (padEnd)
            {
                writer.WritePadding(4);
            }
            return endPosition;
        }

        private void WriteEbx(NativeWriter writer, IEnumerable<DbObject> ebxEntries, ref uint stringsOffset)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (ebxEntries == null)
            {
                throw new ArgumentNullException("ebxEntries");
            }
            checked
            {
                foreach (DbObject ebxEntry in ebxEntries)
                {
                    writer.WriteUInt32LittleEndian(stringsOffset);
                    stringsOffset += (uint)Encoding.ASCII.GetByteCount(ebxEntry.GetValue<string>("name")) + 1u;
                    writer.WriteUInt32LittleEndian(ebxEntry.GetValue<uint>("originalSize"));
                }
            }
        }

        private void WriteRes(NativeWriter writer, IEnumerable<DbObject> resEntries, ref uint stringsOffset)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (resEntries == null)
            {
                throw new ArgumentNullException("resEntries");
            }
            checked
            {
                foreach (DbObject resEntry4 in resEntries)
                {
                    writer.WriteUInt32LittleEndian(stringsOffset);
                    stringsOffset += (uint)Encoding.ASCII.GetByteCount(resEntry4.GetValue<string>("name")) + 1u;
                    writer.WriteUInt32LittleEndian(resEntry4.GetValue<uint>("originalSize"));
                }
            }
            foreach (DbObject resEntry3 in resEntries)
            {
                writer.WriteUInt32LittleEndian((uint)resEntry3.GetValue<long>("resType"));
            }
            foreach (DbObject resEntry2 in resEntries)
            {
                writer.WriteBytes(resEntry2.GetValue<byte[]>("resMeta"));
            }
            foreach (DbObject resEntry in resEntries)
            {
                writer.WriteInt64LittleEndian(resEntry.GetValue<long>("resRid"));
            }
        }

        private void WriteChunks(NativeWriter writer, IEnumerable<DbObject> chunkEntries)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            if (chunkEntries == null)
            {
                throw new ArgumentNullException("chunkEntries");
            }
            foreach (DbObject chunkEntry in chunkEntries)
            {
                writer.WriteGuid(chunkEntry.GetValue<Guid>("id"));
                writer.WriteUInt32LittleEndian(chunkEntry.GetValue<uint>("logicalOffset"));
                writer.WriteUInt32LittleEndian(chunkEntry.GetValue<uint>("logicalSize"));
            }
        }
    }
    */




}
