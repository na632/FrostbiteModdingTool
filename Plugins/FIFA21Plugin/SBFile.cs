using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
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

        List<BundleInfo> Bundles = new List<BundleInfo>();

        public struct EBX
        {

        }

        public struct RES
        {

        }

        public struct CHUNK
        {

        }

        public void Read(NativeReader nativeReader)
        {
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

            for(var i = 0; i < SBHeaderInformation.ebxCount; i++)
            {
                BundleInfo bundleInfo = new BundleInfo();
                bundleInfo.GuidId = nativeReader.ReadGuid();
                Bundles.Add(bundleInfo);
            }
            for (var i = 0; i < SBHeaderInformation.resCount; i++)
            {
                BundleInfo bundleInfo = new BundleInfo();
                bundleInfo.GuidId = nativeReader.ReadGuid();
                Bundles.Add(bundleInfo);
            }
            for (var i = 0; i < SBHeaderInformation.chunkCount; i++)
            {
                BundleInfo bundleInfo = new BundleInfo();
                bundleInfo.GuidId = nativeReader.ReadGuid();
                Bundles.Add(bundleInfo);
            }

            for (var i = 0; i < SBHeaderInformation.ebxCount; i++)
            {
                Bundles[i].StringOffset = nativeReader.ReadInt();
                Bundles[i].Offset = nativeReader.ReadInt();
            }
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
