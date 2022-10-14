using FrostySdk.IO;

namespace FrostyEditor
{
	internal struct IMAGE_SECTION_HEADER
	{
		public string Name;

		public uint Address;

		public uint VirtualAddress;

		public uint SizeOfRawData;

		public uint PointerToRawData;

		public uint PointerToRelocations;

		public uint PointerToLinenumbers;

		public ushort NumberOfRelocations;

		public ushort NumberOfLinenumbers;

		public uint Characteristics;

		public uint PhysicalAddress => Address;

		public uint VirtualSize => Address;

		public void Read(NativeReader reader)
		{
			Name = reader.ReadSizedString(8);
			Address = reader.ReadUInt();
			VirtualAddress = reader.ReadUInt();
			SizeOfRawData = reader.ReadUInt();
			PointerToRawData = reader.ReadUInt();
			PointerToRelocations = reader.ReadUInt();
			PointerToLinenumbers = reader.ReadUInt();
			NumberOfRelocations = reader.ReadUShort();
			NumberOfLinenumbers = reader.ReadUShort();
			Characteristics = reader.ReadUInt();
		}
	}
}
