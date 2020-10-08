using Frosty.Hash;
using FrostySdk;
using FrostySdk.Deobfuscators;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
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
        public string FileLocation { get; set; }

        public static readonly int SBInitialHeaderLength = 32;
        public static readonly int SBInformationHeaderLength = 36;

        public int[] ArrayOfInitialHeaderData = new int[8];

        public SBHeaderInformation SBHeaderInformation { get; set; }

        List<BundleEntryInfo> Bundles = new List<BundleEntryInfo>();

        private int SuperBundleIndex = 0;

        private TocSbReader_FIFA21 ParentReader;
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

        public List<DbObject> Read(NativeReader nativeReader)
        {
            List<DbObject> dbObjects = new List<DbObject>();

            var startOffset = nativeReader.Position;
            if (File.Exists("debugSB.dat"))
                File.Delete("debugSB.dat");
            using (NativeWriter writer = new NativeWriter(new FileStream("debugSB.dat", FileMode.OpenOrCreate)))
            {
                writer.Write(nativeReader.ReadToEnd());
            }
            nativeReader.Position = startOffset;
            var index = 0;
            foreach (BaseBundleInfo BaseBundleItem in AssociatedTOCFile.Bundles)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());

                BundleEntry bundleEntry = new BundleEntry
                {
                    Name = AssociatedTOCFile.SuperBundleName + "_" + index.ToString(), 
                    SuperBundleId = SuperBundleIndex
                };
                using (NativeReader binarySbReader2 = new NativeReader(nativeReader.CreateViewStream(BaseBundleItem.Offset, nativeReader.Length - BaseBundleItem.Offset)))
                {
                    if (File.Exists("debugSBViewStream.dat"))
                        File.Delete("debugSBViewStream.dat");
                    using (NativeWriter writer = new NativeWriter(new FileStream("debugSBViewStream.dat", FileMode.OpenOrCreate)))
                    {
                        writer.Write(binarySbReader2.ReadToEnd());
                    }
                    binarySbReader2.Position = 0;


                    uint num35 = binarySbReader2.ReadUInt(Endian.Big);
                    uint CatalogAndCASOffset = binarySbReader2.ReadUInt(Endian.Big) + num35;
                    uint num37 = binarySbReader2.ReadUInt(Endian.Big);

                    binarySbReader2.Position += 20;

                    // ---------------------------------------------------------------------------------------------------------------------
                    // This is where it hits the Binary SB Reader. FIFA 21 is more like MADDEN 21 in this section

                    // Read out the Header Info
                    var SBHeaderInformation = new SBHeaderInformation(binarySbReader2);
                    //
                    List<Sha1> sha1 = new List<Sha1>();
                    for (int i = 0; i < SBHeaderInformation.totalCount; i++)
                    {
                        sha1.Add(binarySbReader2.ReadSha1());
                    }
                    dbObject.AddValue("ebx", new DbObject(ReadEbx(SBHeaderInformation, sha1, binarySbReader2)));
                    dbObject.AddValue("res", new DbObject(ReadRes(SBHeaderInformation, sha1, binarySbReader2)));
                    dbObject.AddValue("chunks", new DbObject(ReadChunks(SBHeaderInformation, sha1, binarySbReader2)));
                    dbObject.AddValue("dataOffset", (int)(SBHeaderInformation.size));
                    dbObject.AddValue("stringsOffset", (int)(SBHeaderInformation.stringOffset));
                    dbObject.AddValue("metaOffset", (int)(SBHeaderInformation.metaOffset));
                    dbObject.AddValue("metaSize", (int)(SBHeaderInformation.metaSize));

                    if (SBHeaderInformation.chunkCount != 0)
                    {
                        using (DbReader dbReader = new DbReader(nativeReader.CreateViewStream(SBHeaderInformation.metaOffset + BaseBundleItem.Offset, nativeReader.Length - binarySbReader2.Position), new NullDeobfuscator()))
                        {
                            var o = dbReader.ReadDbObject();
                            dbObject.AddValue("chunkMeta", o);
                        }
                    }

                    binarySbReader2.Position = SBHeaderInformation.size;
                    //if (binarySbReader2.Position != binarySbReader2.Length)
                    if (SBHeaderInformation.size > binarySbReader2.Position)
                    {
                        //ReadDataBlock(dbObject.GetValue<DbObject>("ebx"), (int)BaseBundleItem.Offset, (int)binarySbReader2.Position, binarySbReader2);
                        //ReadDataBlock(dbObject.GetValue<DbObject>("res"), (int)BaseBundleItem.Offset, (int)binarySbReader2.Position, binarySbReader2);
                        //ReadDataBlock(dbObject.GetValue<DbObject>("chunks"), (int)BaseBundleItem.Offset, (int)binarySbReader2.Position, binarySbReader2);
                    }

                    // END OF BINARY READER
                    // ---------------------------------------------------------------------------------------------------------------------


                    binarySbReader2.Position = num37;
                    bool[] booleanChangeOfCas = new bool[SBHeaderInformation.totalCount];
                    for (uint booleanIndex = 0u; booleanIndex < SBHeaderInformation.totalCount; booleanIndex++)
                    {
                        booleanChangeOfCas[booleanIndex] = binarySbReader2.ReadBoolean();
                    }
                    binarySbReader2.Position = CatalogAndCASOffset;

                    int movedPositionByteCount = 0;
                    while (binarySbReader2.ReadByte() == 0) { movedPositionByteCount++; }
                    binarySbReader2.Position -= 2;
                    movedPositionByteCount--;


                    bool patchFlag = false;
                    int unk1 = 0;
                    int catalog = 0;
                    int cas = 0;
                    int flagIndex = 0;

                    var ebxCount = dbObject.GetValue<DbObject>("ebx").Count;
                    for (int ebxIndex = 0; ebxIndex < ebxCount; ebxIndex++)
                    {
                        if (booleanChangeOfCas[flagIndex++])
                        {
                            unk1 = binarySbReader2.ReadByte();
                            patchFlag = binarySbReader2.ReadBoolean();
                            catalog = binarySbReader2.ReadByte();
                            cas = binarySbReader2.ReadByte();
                        }
                        DbObject ebxObject = dbObject.GetValue<DbObject>("ebx")[ebxIndex] as DbObject;
                        int offset = binarySbReader2.ReadInt(Endian.Big);
                        int size = binarySbReader2.ReadInt(Endian.Big);

                        if(catalog >= 0 && catalog < AssetManager.Instance.fs.CatalogCount - 1)
                            ebxObject.SetValue("catalog", catalog);
                        if(cas > 0)
                            ebxObject.SetValue("cas", cas);
                        ebxObject.SetValue("offset", offset);
                        ebxObject.SetValue("size", size);
                        if (patchFlag)
                        {
                            ebxObject.SetValue("patch", true);
                        }
                    }

                    binarySbReader2.Position += movedPositionByteCount;
                    var positionBeforeRes = binarySbReader2.Position;
                  
                    for (int indexRes = 0; indexRes < dbObject.GetValue<DbObject>("res").Count; indexRes++)
                    {
                        if (booleanChangeOfCas[flagIndex++])
                        {
                            var b = binarySbReader2.ReadByte();
                            patchFlag = binarySbReader2.ReadBoolean();
                            catalog = binarySbReader2.ReadByte();
                            cas = binarySbReader2.ReadByte();
                        }
                        DbObject resObject = dbObject.GetValue<DbObject>("res")[indexRes] as DbObject;
                        int offset = binarySbReader2.ReadInt(Endian.Big);
                        int size = binarySbReader2.ReadInt(Endian.Big);
                        if (catalog < 0 || catalog > AssetManager.Instance.fs.CatalogCount - 1)
                        {

                        }
                        if (catalog >= 0 && catalog < AssetManager.Instance.fs.CatalogCount - 1)
                            resObject.SetValue("catalog", catalog);
                        if(cas > 0)
                            resObject.SetValue("cas", cas);
                        resObject.SetValue("offset", offset);
                        resObject.SetValue("size", size);
                        if (patchFlag)
                        {
                            resObject.SetValue("patch", true);
                        }
                    }
                    for (int indexChunk = 0; indexChunk < dbObject.GetValue<DbObject>("chunks").Count; indexChunk++)
                    {
                        if (booleanChangeOfCas[flagIndex++])
                        {
                            var b = binarySbReader2.ReadByte();
                            patchFlag = binarySbReader2.ReadBoolean();
                            catalog = binarySbReader2.ReadByte();
                            cas = binarySbReader2.ReadByte();
                        }
                        DbObject chnkObj = dbObject.GetValue<DbObject>("chunks")[indexChunk] as DbObject;
                        int offset = binarySbReader2.ReadInt(Endian.Big);
                        int size = binarySbReader2.ReadInt(Endian.Big);
                        if (catalog < 0 || catalog > AssetManager.Instance.fs.CatalogCount - 1)
                        {

                        }
                        if(catalog >= 0 && catalog < AssetManager.Instance.fs.CatalogCount - 1)
                            chnkObj.SetValue("catalog", catalog);
                        if(cas > 0)
                            chnkObj.SetValue("cas", cas);
                        chnkObj.SetValue("offset", offset);
                        chnkObj.SetValue("size", size);
                        if (patchFlag)
                        {
                            chnkObj.SetValue("patch", true);
                        }
                    }




                }

                AssetManager.Instance.bundles.Add(bundleEntry);
                dbObjects.Add(dbObject);
                index++;
            }

            return dbObjects;
        }

        private List<object> ReadEbx(SBHeaderInformation information, List<Sha1> sha1, NativeReader reader)
        {
            List<object> list = new List<object>();
            for (int i = 0; i < information.ebxCount; i++)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());
                uint num = reader.ReadUInt(Endian.Little);
                uint num2 = reader.ReadUInt(Endian.Little);
                long position = reader.Position;
                reader.Position = information.stringOffset + num;
                //System.Diagnostics.Debug.WriteLine($"EBX::Position::{reader.Position}");
                dbObject.AddValue("sha1", sha1[i]);
                var name = reader.ReadNullTerminatedString();
                //System.Diagnostics.Debug.WriteLine("EBX:: " + name);
                dbObject.AddValue("name", name);
                dbObject.AddValue("nameHash", Fnv1.HashString(dbObject.GetValue<string>("name")));
                dbObject.AddValue("originalSize", num2);
                list.Add(dbObject);
                reader.Position = position;
            }
            return list;
        }
        private List<object> ReadRes(SBHeaderInformation information, List<Sha1> sha1, NativeReader reader)
        {
            List<object> list = new List<object>();
            int num = (int)information.ebxCount;
            for (int i = 0; i < information.resCount; i++)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());
                uint num2 = reader.ReadUInt(Endian.Little);
                uint num3 = reader.ReadUInt(Endian.Little);
                long position = reader.Position;
                reader.Position = information.stringOffset + num2;
                dbObject.AddValue("sha1", sha1[num++]);
                var name = reader.ReadNullTerminatedString();
                //System.Diagnostics.Debug.WriteLine("RES:: " + name);
                dbObject.AddValue("name", name);
                dbObject.AddValue("nameHash", Fnv1.HashString(name));
                dbObject.AddValue("originalSize", num3);
                list.Add(dbObject);
                reader.Position = position;
            }
            foreach (DbObject item in list)
            {
                //var type = reader.ReadUInt(Endian.Big);
                var type = reader.ReadUInt(Endian.Little);
                var resType = (ResourceType)type;
                item.AddValue("resType", type);
            }
            foreach (DbObject item2 in list)
            {
                var resMeta = reader.ReadBytes(16);
                item2.AddValue("resMeta", resMeta);
            }
            foreach (DbObject item3 in list)
            {
                var resRid = reader.ReadLong(Endian.Little);
                item3.AddValue("resRid", resRid);
            }
            return list;
        }

        private List<object> ReadChunks(SBHeaderInformation information, List<Sha1> sha1, NativeReader reader)
        {
            var currentPostion = reader.Position;

            List<object> list = new List<object>();
            int num = (int)(information.ebxCount + information.resCount);
            for (int i = 0; i < information.chunkCount; i++)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());
                Guid guid = reader.ReadGuid(Endian.Little);
                uint num2 = reader.ReadUInt(Endian.Little);
                uint num3 = reader.ReadUInt(Endian.Little);
                long num4 = (num2 & 0xFFFF) | num3;
                dbObject.AddValue("id", guid);
                dbObject.AddValue("sha1", sha1[num + i]);
                dbObject.AddValue("logicalOffset", num2);
                dbObject.AddValue("logicalSize", num3);
                dbObject.AddValue("originalSize", num4);
                list.Add(dbObject);
            }
            return list;
        }

        private void ReadDataBlock( DbObject list, int bundleOffset, int Position, NativeReader reader)
        {
            if (list == null)
            {
                return;
            }
            var currentPostion = reader.Position;


            foreach (DbObject item in list)
            {
                item.AddValue("offset", bundleOffset + Position);
                long num = item.GetValue("originalSize", 0L);
                //item.AddValue("size", num);
                long num2 = 0L;
                while (num > 0)
                {
                    int num3 = reader.ReadInt(Endian.Little);
                    ushort bitShift = reader.ReadUShort(Endian.Little);
                    int num5 = reader.ReadUShort(Endian.Little);
                    int num6 = (bitShift & 0xFF00) >> 8;
                    if ((num6 & 0xF) != 0)
                    {
                        num5 = ((num6 & 0xF) << 16) + num5;
                    }
                    if ((num3 & 4278190080u) != 0L)
                    {
                        num3 &= 0xFFFFFF;
                    }
                    num -= num3;
                    if ((ushort)(bitShift & 0x7F) == 0)
                    {
                        num5 = num3;
                    }
                    num2 += num5 + 8;
                    Position += num5;
                }
                var origSize = item.GetValue("originalSize", 0L);
                item.AddValue("size", num2);
                item.AddValue("sb", true);
            }
        }

        List<object> ReadToDBObject(List<BundleEntryInfo> bundleEntries, NativeReader reader)
        {
            var currentPosition = reader.Position;

            List<object> list = new List<object>();
            var index = 0;
            foreach (BundleEntryInfo bundleEntryInfo in bundleEntries)
            {
                DbObject dbObject = new DbObject(new Dictionary<string, object>());
                dbObject.AddValue("sha1", bundleEntryInfo.Sha);
                if (!string.IsNullOrEmpty(bundleEntryInfo.Name))
                {
                    dbObject.AddValue("name", bundleEntryInfo.Name);
                    dbObject.AddValue("nameHash", Fnv1.HashString(dbObject.GetValue<string>("name")));
                }
                dbObject.AddValue("originalSize", bundleEntryInfo.OriginalSize);

                reader.Position = SBHeaderInformation.stringOffset - (SBHeaderInformation.chunkCount * 24) - (SBHeaderInformation.resCount * 28) + (index * 28);
                if (bundleEntryInfo.Type == "RES")
                {
                    //var type = reader.ReadUInt(Endian.Big);
                    var type = reader.ReadUInt();
                    var resType = (ResourceType)type;
                    dbObject.AddValue("resType", type);
                    var resMeta = reader.ReadBytes(16);
                    dbObject.AddValue("resMeta", resMeta);
                    var resRid = reader.ReadLong(Endian.Little);
                    dbObject.AddValue("resRid", resRid);
                }

                reader.Position = SBHeaderInformation.stringOffset - (SBHeaderInformation.chunkCount * 24) + (index * 24);
                if(bundleEntryInfo.Type == "CHUNK")
                {
                    Guid guid = reader.ReadGuid(Endian.Little);
                    uint num2 = reader.ReadUInt(Endian.Little);
                    uint num3 = reader.ReadUInt(Endian.Little);
                    long num4 = (num2 & 0xFFFF) | num3;
                    dbObject.AddValue("id", guid);
                    dbObject.AddValue("logicalOffset", num2);
                    dbObject.AddValue("logicalSize", num3);
                    dbObject.AddValue("originalSize", num4);
                }
                index++;
                list.Add(dbObject);
            }
            return list;
        }
    }

   

    public class SBHeaderInformation
    {
        public int size;
        public uint magicStuff;
        public int totalCount;
        public int ebxCount;
        public int resCount;
        public int chunkCount;
        public int stringOffset;
        public int metaOffset;
        public int metaSize;


        public bool HasStrings
        {
            get
            {
                return stringOffset > SBFile.SBInformationHeaderLength;
            }
        }

        public bool HasMeta
        { 
            get
            {
                return metaSize > SBFile.SBInformationHeaderLength && metaOffset > SBFile.SBInformationHeaderLength;
            } 
        }

        //public int shaCount;

        public SBHeaderInformation(NativeReader nr)
        {
            var pos = nr.Position;

            size = nr.ReadInt(Endian.Big) + SBFile.SBInformationHeaderLength;
            magicStuff = nr.ReadUInt(Endian.Big);
            if (magicStuff != 3599661469)
                throw new Exception("Magic/Hash is not right, expecting 3599661469");

            totalCount = nr.ReadInt(Endian.Little);
            ebxCount = nr.ReadInt(Endian.Little);
            resCount = nr.ReadInt(Endian.Little);
            chunkCount = nr.ReadInt(Endian.Little);
            stringOffset = nr.ReadInt(Endian.Little) + SBFile.SBInformationHeaderLength;
            metaOffset = nr.ReadInt(Endian.Little) + SBFile.SBInformationHeaderLength;
            metaSize = nr.ReadInt(Endian.Little) + SBFile.SBInformationHeaderLength;
        }
    }
}
