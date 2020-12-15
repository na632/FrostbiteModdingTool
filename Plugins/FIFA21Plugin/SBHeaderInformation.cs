using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FIFA21Plugin
{
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
                return stringOffset > AdditionalHeaderLength;
            }
        }

        public bool HasMeta
        {
            get
            {
                return metaSize > AdditionalHeaderLength && metaOffset > AdditionalHeaderLength;
            }
        }

        int AdditionalHeaderLength = 32;

        //public int shaCount;

        public SBHeaderInformation(NativeReader nr, int additionalHeaderLength = 36)
        {
            AdditionalHeaderLength = additionalHeaderLength;
            var pos = nr.Position;

            size = nr.ReadInt(Endian.Big) + AdditionalHeaderLength;
            magicStuff = nr.ReadUInt(Endian.Big);
            if (magicStuff != 3599661469)
                throw new Exception("Magic/Hash is not right, expecting 3599661469");

            totalCount = nr.ReadInt(Endian.Little);
            ebxCount = nr.ReadInt(Endian.Little);
            resCount = nr.ReadInt(Endian.Little);
            chunkCount = nr.ReadInt(Endian.Little);
            stringOffset = nr.ReadInt(Endian.Little) + AdditionalHeaderLength;
            metaOffset = nr.ReadInt(Endian.Little) + AdditionalHeaderLength;
            metaSize = nr.ReadInt(Endian.Little) + AdditionalHeaderLength;
        }

        public byte[] Write()
        {
            MemoryStream memoryStream = new MemoryStream();
            NativeWriter nw = new NativeWriter(memoryStream);
            nw.Write((int)size, Endian.Big);
            nw.Write(3599661469, Endian.Big);
            nw.Write(totalCount, Endian.Little);
            nw.Write(ebxCount, Endian.Little);
            nw.Write(resCount, Endian.Little);
            nw.Write(chunkCount, Endian.Little);
            nw.Write(stringOffset, Endian.Little);
            nw.Write(metaOffset, Endian.Little);
            nw.Write(metaSize, Endian.Little);
            return memoryStream.ToArray();
        }
    }
}
