using FrostyEditor.IO;
using FrostySdk;
using System;
using v2k4FIFASDKGenerator;

namespace v2k4FIFASDKGenerator.BaseInfo
{

    public class ClassInfo
    {
        public TypeInfo typeInfo;

        public ushort id;

        public ushort isDataContainer;

        public byte[] padding;

        public long parentClass;

        public virtual void Read(MemoryReader reader)
        {
            long position = reader.Position;
            long position2 = reader.ReadLong();
            ClassesSdkCreator.offset = reader.ReadLong();
            Guid guid = Guid.Empty;
            if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20171110 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180628)
            {
                guid = reader.ReadGuid();
            }
            id = reader.ReadUShort();
            isDataContainer = reader.ReadUShort();
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
