using FrostyEditor.IO;
using FrostySdk;
using System;
using System.Diagnostics;
using FrostbiteSdk;
using FrostbiteSdk.SdkGenerator;

namespace SdkGenerator.FIFA21
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


		private FIFA21.TypeInfo parentTypeInfo { get; }
		public FieldInfo(FIFA21.TypeInfo parentType)
        {
			parentTypeInfo = parentType;

		}

		public void Read(MemoryReader reader)
		{
			var position = reader.Position;
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
