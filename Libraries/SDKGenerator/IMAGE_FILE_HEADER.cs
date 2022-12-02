using FMT.FileTools;
using FrostySdk.IO;

namespace FrostyEditor
{
	internal class IMAGE_FILE_HEADER
	{
		public ushort Machine;

		public ushort NumberOfSections;

		public uint TimeDateStamp;

		public uint PointerToSymbolTable;

		public uint NumberOfSymbols;

		public ushort SizeOfOptionalHeader;

		public ushort Characteristics;

		public void Read(NativeReader reader)
		{
			Machine = reader.ReadUShort();
			NumberOfSections = reader.ReadUShort();
			TimeDateStamp = reader.ReadUInt();
			PointerToSymbolTable = reader.ReadUInt();
			NumberOfSymbols = reader.ReadUInt();
			SizeOfOptionalHeader = reader.ReadUShort();
			Characteristics = reader.ReadUShort();
		}
	}
}
