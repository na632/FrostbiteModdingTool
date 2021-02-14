using FrostyEditor.IO;
using FrostySdk;
using System;
using System.Diagnostics;

namespace v2k4FIFASDKGenerator.FIFA21
{
	public class FieldInfo : BaseInfo.FieldInfo
	{
		private uint nameHash;

		public static Random RandomEmpty = new Random();


		private FIFA21.TypeInfo parentTypeInfo { get; }
		public FieldInfo(FIFA21.TypeInfo parentType)
        {
			parentTypeInfo = parentType;

		}

		public override void Read(MemoryReader reader)
		{
			var position = reader.Position;
			name = reader.ReadNullTerminatedString();
			if (string.IsNullOrEmpty(name))
			{
				//if (parentTypeInfo.array[1] > 5000000000)
				//{
				//	reader.Position = parentTypeInfo.array[1];
				//	name = reader.ReadNullTerminatedString();
				//}
				//else 
				if (parentTypeInfo.array[3] > 5000000000)
				{
					reader.Position = parentTypeInfo.array[3];
					name = reader.ReadNullTerminatedString();
				}

				if (string.IsNullOrEmpty(name))
				{
					//name = "Unk" + RandomEmpty.Next().ToString();
					//Debug.WriteLine($"[ERROR] {parentTypeInfo.name} (Type:{parentTypeInfo.Type.ToString()}) has unknown field with name {name}");
				}
			}

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
