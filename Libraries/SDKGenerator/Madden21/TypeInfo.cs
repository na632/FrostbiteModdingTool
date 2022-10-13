using System.Text.RegularExpressions;
using FrostbiteSdk;
using FrostbiteSdk.SdkGenerator;
using System.Collections.Generic;
using System;
using FrostySdk;

namespace SdkGenerator.Madden21
{
	public class TypeInfo : ITypeInfo
	{
		public int nameHash { get; set; }

		public long[] array;

		public string name { get; set; }

		public ushort flags { get; set; }

		public uint size { get; set; }

		public Guid guid { get; set; }

		public ushort padding1 { get; set; }

		public string nameSpace { get; set; }

		public ushort alignment { get; set; }

		public uint fieldCount { get; set; }

		public uint padding3 { get; set; }

		public long parentClass { get; set; }


		public int Type => (flags >> 4) & 0x1F;

        public List<IFieldInfo> fields { get; set; }

        public void Read(MemoryReader reader)
		{
			fields = new List<IFieldInfo>();
			name = reader.ReadNullTerminatedString();
			if(name.Equals("TextureAsset", System.StringComparison.OrdinalIgnoreCase))
            {

            }
			//nameHash = reader.ReadUInt();
			var h = reader.ReadInt();
			if(h == -237252713)
            {

            }
			nameHash = h;
			if (nameHash == 4057714583)
			{

			}
			flags = reader.ReadUShort();
			flags >>= 1;
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

			if (Type == 2)
			{
				reader.Position = array[6];
				flag = true;
			}
            else if (Type == 3)
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
			else if (Type == 8)
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


				foreach(var f  in fields)
                {
                }
			}
		}


		public void Modify(DbObject classObj)
		{
			classObj.SetValue("nameHash", nameHash);
		}
	}
}
