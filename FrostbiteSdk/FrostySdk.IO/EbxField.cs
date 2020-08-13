namespace FrostySdk.IO
{
	public struct EbxField
	{
		public string Name;

		public uint NameHash;

		public ushort Type;

		public ushort ClassRef;

		public uint DataOffset;

		public uint SecondOffset;

		public EbxFieldType DebugType => (EbxFieldType)((Type >> 4) & 0x1F);
	}
}
