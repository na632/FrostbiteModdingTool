using FrostyEditor.IO;
using FrostySdk;
using System;

namespace v2k4FIFASDKGenerator.FIFA21
{
	public class FieldInfo : BaseInfo.FieldInfo
	{
		private uint nameHash;

		public static Random RandomEmpty = new Random();

		public override void Read(MemoryReader reader)
		{
			name = reader.ReadNullTerminatedString();
			if(string.IsNullOrEmpty(name))
            {
				name = "Unknown" + RandomEmpty.Next().ToString();
            }
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
