using FrostbiteSdk;
using FrostbiteSdk.SdkGenerator;
using FrostyEditor.IO;
using FrostySdk;
using FrostySdk.IO;
using System;
using System.IO;

namespace SdkGenerator.BaseInfo
{

    public class ClassInfo : ISdkGenInfo
    {
        public TypeInfo typeInfo;

        public ushort id;

        public ushort isDataContainer;

        public byte[] padding;

        public long parentClass;

        public virtual void Read(MemoryReader reader)
        {
            long position = reader.Position;
            using (NativeWriter nativeWriter = new NativeWriter(new FileStream("DebugClassInfo.dat", FileMode.Create)))
            {
                nativeWriter.Write(reader.ReadBytes(100));
                reader.Position = position;
            }

            long position2 = reader.ReadLong();
                ClassesSdkCreator.offset = reader.ReadLong();
                Guid guid = Guid.Empty;
                if (
                    ProfilesLibrary.IsFIFA19DataVersion() || 
                    ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20171110 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180628)
                {
                    guid = reader.ReadGuid();
                }

                id = reader.ReadUShort();

            //if (ProfilesLibrary.IsFIFA19DataVersion())
            //    reader.ReadUShort();

            if (ProfilesLibrary.IsFIFA19DataVersion())
            {
                reader.ReadByte();
            }

            //isDataContainer = reader.ReadUShort();
            isDataContainer = ProfilesLibrary.IsFIFA19DataVersion() ? reader.ReadByte() : reader.ReadUShort();
                padding = new byte[4]
                {
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte()
                };
                parentClass = reader.ReadLong();
                reader.Position = position2;
                typeInfo = new TypeInfo();
                typeInfo.Read(reader);
                if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20171110 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180628)
                {
                    typeInfo.guid = guid;
                }
                if (typeInfo.parentClass != 0L)
                {
                    parentClass = typeInfo.parentClass;
                }
                reader.Position = parentClass;
                if (reader.Position == position)
                {
                    parentClass = 0L;
                }


        }
    }

}
