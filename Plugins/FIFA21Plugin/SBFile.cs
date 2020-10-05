using Frosty.Hash;
using FrostySdk;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            using (NativeWriter writer = new NativeWriter(new FileStream("debugSB.dat", FileMode.OpenOrCreate)))
            {
                writer.Write(nativeReader.ReadToEnd());
            }
            nativeReader.Position = startOffset;

            // Initial Header data
            for (var i = 0; i < 8; i++)
            {
                ArrayOfInitialHeaderData[i] = nativeReader.ReadInt(Endian.Big);
            }

            // Header Information (Counts offsets etc)
            SBHeaderInformation = new SBHeaderInformation(nativeReader);

            List<Guid> ShaGuids = new List<Guid>();
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
                Bundles[i].Sha = nativeReader.ReadGuid();
            }
            for (var i = SBHeaderInformation.ebxCount-1; i < SBHeaderInformation.ebxCount+SBHeaderInformation.resCount; i++)
            {
                Bundles[i].Sha = nativeReader.ReadGuid();
            }

            for (var i = 0; i < SBHeaderInformation.chunkCount; i++)
            {
                BundleEntryInfo bundleInfo = new BundleEntryInfo() { Type = "CHUNK" };
                bundleInfo.Sha = nativeReader.ReadGuid();
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

            dbObject.AddValue("ebx", new DbObject(BundlesToDBObject(Bundles.Where(x=> x.Type == "EBX").ToList(), nativeReader)));
            dbObject.AddValue("res", new DbObject(BundlesToDBObject(Bundles.Where(x => x.Type == "RES").ToList(), nativeReader)));
            dbObject.AddValue("chunks", new DbObject(BundlesToDBObject(Bundles.Where(x => x.Type == "CHUNK").ToList(), nativeReader)));
            dbObject.AddValue("dataOffset", (int)(SBHeaderInformation.size));
            dbObject.AddValue("stringsOffset", (int)(SBHeaderInformation.stringOffset));
            dbObject.AddValue("metaOffset", (int)(SBHeaderInformation.metaOffset));
            dbObject.AddValue("metaSize", (int)(SBHeaderInformation.metaSize));
            return dbObject;
        }

        List<object> BundlesToDBObject(List<BundleEntryInfo> bundleEntries, NativeReader reader)
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
