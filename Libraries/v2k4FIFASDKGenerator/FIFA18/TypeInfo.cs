using FrostyEditor.IO;
using FrostySdk;

namespace v2k4FIFASDKGenerator.FIFA18
{
	public class TypeInfo : BaseInfo.TypeInfo
	{
		private uint nameHash;

		public override void Read(MemoryReader reader)
		{
            bool flag = false;
            name = reader.ReadNullTerminatedString();
            if (!string.IsNullOrEmpty(name) && name.Length > 0)
            {
                flags = reader.ReadUShort();
                if (ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20171110 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20190729 || ProfilesLibrary.DataVersion == 20180628)
                {
                    flags >>= 1;
                }
                size = reader.ReadUInt();
                if (ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20190729)
                {
                    reader.Position -= 4L;
                    size = reader.ReadUShort();
                    guid = reader.ReadGuid();
                    reader.ReadUShort();
                }
                padding1 = reader.ReadUShort();
                long position = reader.ReadLong();
                if (ProfilesLibrary.DataVersion == 20170321 || ProfilesLibrary.DataVersion == 20160927 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20171110 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20190729 || ProfilesLibrary.DataVersion == 20180628)
                {
                    reader.ReadLong();
                }
                alignment = (flag ? reader.ReadByte() : reader.ReadUShort());
                fieldCount = (flag ? reader.ReadByte() : reader.ReadUShort());
                if (flag)
                {
                    padding3 = reader.ReadUShort();
                }
                padding3 = reader.ReadUInt();
                long[] array = new long[7];
                for (int i = 0; i < 7; i++)
                {
                    array[i] = reader.ReadLong();
                }
                reader.Position = position;
                nameSpace = reader.ReadNullTerminatedString();
                bool flag2 = false;
                if (ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20171110 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180628)
                {
                    parentClass = array[0];
                    if (Type == 2)
                    {
                        reader.Position = array[5];
                        flag2 = true;
                    }
                    else if (Type == 3)
                    {
                        reader.Position = array[1];
                        flag2 = true;
                    }
                    else if (Type == 8)
                    {
                        reader.Position = array[0];
                        flag2 = true;
                        parentClass = 0L;
                    }
                }
                else if (Type == 2)
                {
                    reader.Position = array[1];
                    flag2 = true;
                }
                else if (Type == 3)
                {
                    reader.Position = array[2];
                    flag2 = true;
                }
                else if (Type == 8)
                {
                    reader.Position = array[0];
                    flag2 = true;
                    parentClass = 0L;
                }
                else if (Type == 4)
                {
                    parentClass = array[0];
                }
                if (flag2)
                {
                    for (int j = 0; j < fieldCount; j++)
                    {
                        FieldInfo fieldInfo2 = new FieldInfo();
                        fieldInfo2.Read(reader);
                        fieldInfo2.index = j;
                        fields.Add(fieldInfo2);
                    }
                }
            }
        }

		public override void Modify(DbObject classObj)
		{
			classObj.SetValue("nameHash", nameHash);
		}
	}
}
