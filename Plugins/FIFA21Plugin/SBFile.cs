using Frosty.Hash;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public DbObject Read(NativeReader nativeReader)
        {
            DbObject dbObject = new DbObject(new Dictionary<string, object>());

            var startOffset = nativeReader.Position;
            if (File.Exists("debugSB.dat"))
                File.Delete("debugSB.dat");
            using (NativeWriter writer = new NativeWriter(new FileStream("debugSB.dat", FileMode.OpenOrCreate)))
            {
                writer.Write(nativeReader.ReadToEnd());
            }
            nativeReader.Position = startOffset;
            foreach (BaseBundleInfo BaseBundleItem in AssociatedTOCFile.Bundles)
            {
                BundleEntry item4 = new BundleEntry
                {
                    Name = BaseBundleItem.Name,
                    SuperBundleId = SuperBundleIndex
                };
                AssetManager.Instance.bundles.Add(item4);
                //using (NativeReader binarySbReader2 = new NativeReader(nativeReader.CreateViewStream(0, BaseBundleItem.Size)))
                using (NativeReader binarySbReader2 = new NativeReader(nativeReader.CreateViewStream(BaseBundleItem.Offset, nativeReader.Length - BaseBundleItem.Offset)))
                {
                    uint num35 = binarySbReader2.ReadUInt(Endian.Big);
                    uint num36 = binarySbReader2.ReadUInt(Endian.Big) + num35;
                    uint num37 = binarySbReader2.ReadUInt(Endian.Big);

                    binarySbReader2.Position += 20;
                    // This is where it hits the Binary SB Reader. FIFA 21 is more like MADDEN 21 in this section

                    // Read out the Header Info
                    var SBHeaderInformation = new SBHeaderInformation(binarySbReader2);
                    //
                    List<Sha1> sha1 = new List<Sha1>();
                    for (int i = 0; i < SBHeaderInformation.totalCount; i++)
                    {
                        sha1.Add(binarySbReader2.ReadSha1());
                    }
                    //dbObject.AddValue("ebx", new DbObject(ReadEbx(dbReader)));
                    //dbObject.AddValue("res", new DbObject(ReadRes(dbReader)));
                    //dbObject.AddValue("chunks", new DbObject(ReadChunks(dbReader)));
                    dbObject.AddValue("dataOffset", (int)(SBHeaderInformation.size));
                    dbObject.AddValue("stringsOffset", (int)(SBHeaderInformation.stringOffset));
                    dbObject.AddValue("metaOffset", (int)(SBHeaderInformation.metaOffset));
                    dbObject.AddValue("metaSize", (int)(SBHeaderInformation.metaSize));
                }

            }

            /*
            // Initial Header data
            for (var i = 0; i < 8; i++)
            {
                ArrayOfInitialHeaderData[i] = nativeReader.ReadInt(Endian.Big);
            }
           
            ArrayOfInitialHeaderData[1] = ArrayOfInitialHeaderData[1] + ArrayOfInitialHeaderData[0];

            // Header Information (Counts offsets etc)
            SBHeaderInformation = new SBHeaderInformation(nativeReader);

            //List<Guid> ShaGuids = new List<Guid>();
            //for(var i = 0; i < SBHeaderInformation.shaCount; i++)
            //{
            //    ShaGuids.Add(nativeReader.ReadGuid());
            //}

            var currentPostion = nativeReader.Position;
            nativeReader.Position = SBHeaderInformation.stringOffset;
            for(var i = 0; i < SBHeaderInformation.ebxCount; i++)
            {
                BundleEntryInfo bundleInfo = new BundleEntryInfo() { Type = "EBX" };
                bundleInfo.Name = nativeReader.ReadNullTerminatedString();
                Bundles.Add(bundleInfo);
            }
            for (var i = 0; i < SBHeaderInformation.resCount; i++)
            {
                BundleEntryInfo bundleInfo = new BundleEntryInfo() { Type = "RES" };
                bundleInfo.Name = nativeReader.ReadNullTerminatedString();
                Bundles.Add(bundleInfo);
            }
            nativeReader.Position = currentPostion;
            for (var i = 0; i < SBHeaderInformation.ebxCount; i++)
            {
                //Bundles[i].Sha = nativeReader.ReadGuid();
                Bundles[i].Sha = nativeReader.ReadSha1();
            }
            for (var i = SBHeaderInformation.ebxCount-1; i < SBHeaderInformation.ebxCount+SBHeaderInformation.resCount; i++)
            {
                //Bundles[i].Sha = nativeReader.ReadGuid();
                Bundles[i].Sha = nativeReader.ReadSha1();
            }

            for (var i = 0; i < SBHeaderInformation.chunkCount-1; i++)
            {
                BundleEntryInfo bundleInfo = new BundleEntryInfo() { Type = "CHUNK" };
                //bundleInfo.Sha = nativeReader.ReadGuid();
                bundleInfo.Sha = nativeReader.ReadSha1();
                Bundles.Add(bundleInfo);
            }

            while (nativeReader.ReadInt(Endian.Little) != 0) ;
            nativeReader.Position -= 4;

            for (var i = 0; i < SBHeaderInformation.ebxCount; i++)
            {
                Bundles[i].StringOffset = nativeReader.ReadInt();
                Bundles[i].OriginalSize = nativeReader.ReadInt();
            }
            for (var i = 0; i < SBHeaderInformation.resCount; i++)
            {
                var index = (i + SBHeaderInformation.ebxCount);
                Bundles[index].StringOffset = nativeReader.ReadInt();
                Bundles[index].OriginalSize = nativeReader.ReadInt();
            }

            dbObject.AddValue("ebx", new DbObject(ReadToDBObject(Bundles.Where(x=> x.Type == "EBX").ToList(), nativeReader)));
            dbObject.AddValue("res", new DbObject(ReadToDBObject(Bundles.Where(x => x.Type == "RES").ToList(), nativeReader)));
            dbObject.AddValue("chunks", new DbObject(ReadToDBObject(Bundles.Where(x => x.Type == "CHUNK").ToList(), nativeReader)));
            dbObject.AddValue("dataOffset", (int)(SBHeaderInformation.size));
            dbObject.AddValue("stringsOffset", (int)(SBHeaderInformation.stringOffset));
            dbObject.AddValue("metaOffset", (int)(SBHeaderInformation.metaOffset));
            dbObject.AddValue("metaSize", (int)(SBHeaderInformation.metaSize));

            // Fill out the Boolean Array for all Objects
            nativeReader.Position = ArrayOfInitialHeaderData[2];
            bool[] boolArray = new bool[SBHeaderInformation.totalCount];
            for (uint index = 0u; index < SBHeaderInformation.totalCount; index++)
            {
                boolArray[index] = nativeReader.ReadBoolean();
            }

            // Get Catalog and CAS data
            nativeReader.Position = ArrayOfInitialHeaderData[1];
            bool flag = false;
            int catalog = 0;
            int cas = 0;
            int arrayIndex = 0;
            for (int ebxIndex = 0; ebxIndex < dbObject.GetValue<DbObject>("ebx").Count; ebxIndex++)
            {
                if (boolArray[arrayIndex++])
                {
                    nativeReader.ReadByte();
                    flag = nativeReader.ReadBoolean();
                    catalog = nativeReader.ReadByte();
                    cas = nativeReader.ReadByte();
                }
                DbObject dbObjRef = dbObject.GetValue<DbObject>("ebx")[ebxIndex] as DbObject;
                int offset = nativeReader.ReadInt(Endian.Big);
                int size = nativeReader.ReadInt(Endian.Big);
                dbObjRef.SetValue("catalog", catalog);
                dbObjRef.SetValue("cas", cas);
                dbObjRef.SetValue("offset", offset);
                dbObjRef.SetValue("size", size);
                if (flag)
                {
                    dbObjRef.SetValue("patch", true);
                }
            }
            for (int resIndex = 0; resIndex < dbObject.GetValue<DbObject>("res").Count; resIndex++)
            {
                if (boolArray[arrayIndex++])
                {
                    nativeReader.ReadByte();
                    flag = nativeReader.ReadBoolean();
                    catalog = nativeReader.ReadByte();
                    cas = nativeReader.ReadByte();
                }
                DbObject dbObjRef = dbObject.GetValue<DbObject>("res")[resIndex] as DbObject;
                int offset = nativeReader.ReadInt(Endian.Big);
                int size = nativeReader.ReadInt(Endian.Big);
                dbObjRef.SetValue("catalog", catalog);
                dbObjRef.SetValue("cas", cas);
                dbObjRef.SetValue("offset", offset);
                dbObjRef.SetValue("size", size);
                if (flag)
                {
                    dbObjRef.SetValue("patch", true);
                }
            }
            for (int chunkIndex = 0; chunkIndex < dbObject.GetValue<DbObject>("chunks").Count; chunkIndex++)
            {
                if (boolArray[arrayIndex++])
                {
                    nativeReader.ReadByte();
                    flag = nativeReader.ReadBoolean();
                    catalog = nativeReader.ReadByte();
                    cas = nativeReader.ReadByte();
                }
                DbObject dbObjRef = dbObject.GetValue<DbObject>("chunks")[chunkIndex] as DbObject;
                int offset = nativeReader.ReadInt(Endian.Big);
                int size = nativeReader.ReadInt(Endian.Big);
                dbObjRef.SetValue("catalog", catalog);
                dbObjRef.SetValue("cas", cas);
                dbObjRef.SetValue("offset", offset);
                dbObjRef.SetValue("size", size);
                if (flag)
                {
                    dbObjRef.SetValue("patch", true);
                }
            }
            */

            return dbObject;
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
