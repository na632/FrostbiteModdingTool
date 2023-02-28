using FrostbiteSdk;
using System;


namespace SdkGenerator.FIFA18
{
    public class ClassInfo : BaseInfo.ClassInfo
    {
        public override void Read(MemoryReader reader)
        {
            long position = reader.Position;
            long position2 = reader.ReadLong();
            ClassesSdkCreator.offset = reader.ReadLong();
            Guid guid = reader.ReadGuid();
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
            typeInfo.guid = guid;
            if (typeInfo.parentClass != 0L)
            {
                parentClass = typeInfo.parentClass;
            }
            if (parentClass == position)
            {
                parentClass = 0L;
            }
        }
    }
}
