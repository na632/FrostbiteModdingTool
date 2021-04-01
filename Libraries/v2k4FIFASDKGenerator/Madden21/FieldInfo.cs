using FrostyEditor.IO;
using FrostySdk;
using System;

namespace SdkGenerator.Madden21
{
	public class FieldInfo : BaseInfo.FieldInfo
	{
		private uint nameHash;

		public static Random RandomEmpty = new Random();

		public bool ReadSuccessfully = false;


		private Madden21.TypeInfo parentTypeInfo { get; }
		public FieldInfo(Madden21.TypeInfo parentType)
		{
			parentTypeInfo = parentType;

		}

		public override void Read(MemoryReader reader)
		{
			var position = reader.Position;
			name = reader.ReadNullTerminatedString();
			if (string.IsNullOrEmpty(name))
			{
				//if (parentTypeInfo.array[0] > 5000000000)
				//{
				//	reader.Position = parentTypeInfo.array[0];
				//	name = reader.ReadNullTerminatedString();
				//}
				//if (parentTypeInfo.array[1] > 5000000000)
    //            {
    //                reader.Position = parentTypeInfo.array[1];
    //                name = reader.ReadNullTerminatedString();
    //            }
				//if (parentTypeInfo.array[2] > 5000000000)
				//{
				//	reader.Position = parentTypeInfo.array[2];
				//	name = reader.ReadNullTerminatedString();
				//}
				//if (parentTypeInfo.array[3] > 5000000000)
				//{
				//	reader.Position = parentTypeInfo.array[3];
				//	name = reader.ReadNullTerminatedString();
				//}
				//if (parentTypeInfo.array[4] > 5000000000)
				//{
				//	reader.Position = parentTypeInfo.array[4];
				//	name = reader.ReadNullTerminatedString();
				//}
				//if (parentTypeInfo.array[5] > 5000000000)
				//{
				//	reader.Position = parentTypeInfo.array[5];
				//	name = reader.ReadNullTerminatedString();
				//}
				//if (parentTypeInfo.array[6] > 5000000000)
				//{
				//	reader.Position = parentTypeInfo.array[6];
				//	name = reader.ReadNullTerminatedString();
				//}
				

				if (string.IsNullOrEmpty(name))
				{
					name = parentTypeInfo.name + "_UnkField_" + RandomEmpty.Next().ToString();
					//Debug.WriteLine($"[ERROR] {parentTypeInfo.name} (Type:{parentTypeInfo.Type.ToString()}) has unknown field with name {name}");
				}
			}
			//else
            //{
				ReadSuccessfully = true;
			//}

			//var index = 1;
			//for(index = 1; string.IsNullOrEmpty(name) && index < 7; index++)
			//         {
			//	reader.Position = parentTypeInfo.array[index];
			//	name = reader.ReadNullTerminatedString();
			//}

			nameHash = reader.ReadUInt();
			flags = reader.ReadUShort();
			offset = reader.ReadUShort();
			typeOffset = reader.ReadLong();
		}

		//public override void Read(MemoryReader reader)
		//{
		//	name = reader.ReadNullTerminatedString();
		//	nameHash = reader.ReadUInt();
		//	flags = reader.ReadUShort();
		//	offset = reader.ReadUShort();
		//	typeOffset = reader.ReadLong();
		//}

		public override void Modify(DbObject fieldObj)
		{
			fieldObj.SetValue("nameHash", nameHash);
		}

        public override string ToString()
        {
			return name;
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
