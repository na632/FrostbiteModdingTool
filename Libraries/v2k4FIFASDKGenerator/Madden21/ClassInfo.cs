using FrostyEditor.IO;
using System;
using v2k4FIFASDKGenerator;

namespace v2k4FIFASDKGenerator.Madden21
{
	public class ClassInfo : BaseInfo.ClassInfo
	{
		public override void Read(MemoryReader reader)
		{
			long position = reader.Position;
			long position2 = reader.ReadLong();
			ClassesSdkCreator.offset = reader.ReadLong();
			//if(ClassesSdkCreator.offset == 0)
				ClassesSdkCreator.offset = reader.ReadLong();

			id = reader.ReadUShort();
			//
			//reader.ReadUShort();
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
			if (typeInfo.parentClass != 0L)
			{
				parentClass = typeInfo.parentClass;
			}
			if (parentClass == position)
			{
				parentClass = 0L;
			}
		}

        public override string ToString()
        {
			if(typeInfo!=null && !string.IsNullOrEmpty(typeInfo.name))
				return typeInfo.name;

            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
