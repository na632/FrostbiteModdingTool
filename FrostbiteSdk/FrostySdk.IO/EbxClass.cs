namespace FrostySdk.IO
{
	public struct EbxClass
	{
		public string Name;

		public uint NameHash;

		public int FieldIndex;

		public byte FieldCount;

		public byte Alignment;

		public ushort Type;

		public ushort Size;

		public ushort SecondSize;

		public string Namespace;

		public int Index;

		public EbxFieldType DebugType => (EbxFieldType)((Type >> 4) & 0x1F);
	}
}
