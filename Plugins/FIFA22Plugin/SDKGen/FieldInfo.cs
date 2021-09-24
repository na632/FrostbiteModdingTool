using FrostySdk;
using System;
using FrostbiteSdk;
using FrostbiteSdk.SdkGenerator;

namespace SdkGenerator.Fifa22
{
	public class FieldInfo : IFieldInfo
	{
		public uint nameHash;

		public static Random RandomEmpty = new Random();

		public bool ReadSuccessfully = false;

		public string name { get; set; }

		public ushort flags { get; set; }

		public uint offset { get; set; }

		public ushort padding1 { get; set; }

		public long typeOffset { get; set; }

		public int index { get; set; }


		private ITypeInfo parentTypeInfo { get; }
		public FieldInfo(ITypeInfo parentType)
		{
			parentTypeInfo = parentType;

		}

		public void Read(MemoryReader reader)
		{
			name = reader.ReadNullTerminatedString();
			if (string.IsNullOrEmpty(name))
			{
				name = parentTypeInfo.name + "_UnkField_" + RandomEmpty.Next().ToString();
			}
			nameHash = reader.ReadUInt();
			flags = reader.ReadUShort();
			offset = reader.ReadUShort();
			typeOffset = reader.ReadLong();
			ReadSuccessfully = true;
		}


		//public override void Read(MemoryReader reader)
		//{
		//	name = reader.ReadNullTerminatedString();
		//	nameHash = reader.ReadUInt();
		//	flags = reader.ReadUShort();
		//	offset = reader.ReadUShort();
		//	typeOffset = reader.ReadLong();
		//}

		public void Modify(DbObject fieldObj)
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
