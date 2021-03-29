using FrostyEditor.IO;
using FrostySdk;

namespace SdkGenerator.FIFA18
{
	public class FieldInfo : BaseInfo.FieldInfo
	{
		private uint nameHash;

		public override void Read(MemoryReader reader)
		{
			name = reader.ReadNullTerminatedString();
			nameHash = reader.ReadUInt();
			flags = reader.ReadUShort();
			offset = reader.ReadUShort();
			typeOffset = reader.ReadLong();
		}

		public override void Modify(DbObject fieldObj)
		{
			fieldObj.SetValue("nameHash", nameHash);
		}
	}
}
