using FrostyEditor.IO;
using FrostySdk;
using System.Text.RegularExpressions;

namespace SdkGenerator.Madden21
{
	public class TypeInfo : BaseInfo.TypeInfo
	{
		private uint nameHash;

		public long[] array;


		public override void Read(MemoryReader reader)
		{
			name = reader.ReadNullTerminatedString();
			if(name.Contains("blocking"))
            {

            }
			//nameHash = reader.ReadUInt();
			nameHash = (uint)reader.ReadInt();
			flags = reader.ReadUShort();
			flags >>= 1;
			//size = reader.ReadUInt();
			//reader.Position -= 4L;
			size = reader.ReadUShort();

			guid = reader.ReadGuid();
			if (!Regex.IsMatch(guid.ToString(), @"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$"))
			{
				throw new System.FormatException("Guid is not valid");
			}

			// Module 
			long namespaceNamePosition = reader.ReadLong();

			// Unknown ?
			long nextTypeInfo = reader.ReadLong();

			// 
			alignment = reader.ReadUShort();
			fieldCount = reader.ReadUShort();

			// Padding
			padding3 = reader.ReadUInt();

			array = new long[7];
			for (int i = 0; i < 7; i++)
			{
				array[i] = reader.ReadLong();
			}
			reader.Position = namespaceNamePosition;
			nameSpace = reader.ReadNullTerminatedString();
			bool flag = false;

			reader.Position = nextTypeInfo;

			parentClass = array[0];

			if (base.Type == 2)
			{
				reader.Position = array[6];
				flag = true;
			}
            else if (base.Type == 3)
            {
                reader.Position = array[1];

                //if (reader.Position != 0)
                flag = true;
            }
            else if (Type == 4)
			{
				//parentClass = 0L;
				//reader.Position = array[0];
				//if (reader.Position != 0)
				//	flag = true;
			}
			else if (base.Type == 8)
			{
				//parentClass = 0L;
				reader.Position = array[0];
				if (reader.Position != 0)
					flag = true;
			}

			if (flag)
			{
				for (int j = 0; j < fieldCount; j++)
				{
					FieldInfo fieldInfo = new FieldInfo(this);
					fieldInfo.Read(reader);
					fieldInfo.index = j;
					if (fieldInfo.ReadSuccessfully)
					{
						fields.Add(fieldInfo);
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
