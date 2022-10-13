using FrostySdk.IO;

namespace FrostyEditor
{
	internal class IMAGE_OPTIONAL_HEADER
	{
		public ushort Magic;

		public byte MajorLinkerVersion;

		public byte MinorLinkerVersion;

		public uint SizeOfCode;

		public uint SizeOfInitializedData;

		public uint SizeOfUninitializedData;

		public uint AddressOfEntryPoint;

		public uint BaseOfCode;

		public ulong ImageBase;

		public uint SectionAlignment;

		public uint FileAlignment;

		public ushort MajorOperatingSystemVersion;

		public ushort MinorOperatingSystemVersion;

		public ushort MajorImageVersion;

		public ushort MinorImageVersion;

		public ushort MajorSubsystemVersion;

		public ushort MinorSubsystemVersion;

		public uint Win32VersionValue;

		public uint SizeOfImage;

		public uint SizeOfHeaders;

		public uint CheckSum;

		public ushort Subsystem;

		public ushort DllCharacteristics;

		public ulong SizeOfStackReserve;

		public ulong SizeOfStackCommit;

		public ulong SizeOfHeapReserve;

		public ulong SizeOfHeapCommit;

		public uint LoaderFlags;

		public uint NumberOfRvaAndSizes;

		private IMAGE_DATA_DIRECTORY[] DataDirectory = new IMAGE_DATA_DIRECTORY[16];

		public void Read(NativeReader reader)
		{
			Magic = reader.ReadUShort();
			MajorLinkerVersion = reader.ReadByte();
			MinorLinkerVersion = reader.ReadByte();
			SizeOfCode = reader.ReadUInt();
			SizeOfInitializedData = reader.ReadUInt();
			SizeOfUninitializedData = reader.ReadUInt();
			AddressOfEntryPoint = reader.ReadUInt();
			BaseOfCode = reader.ReadUInt();
			ImageBase = reader.ReadULong();
			SectionAlignment = reader.ReadUInt();
			FileAlignment = reader.ReadUInt();
			MajorOperatingSystemVersion = reader.ReadUShort();
			MinorOperatingSystemVersion = reader.ReadUShort();
			MajorImageVersion = reader.ReadUShort();
			MinorImageVersion = reader.ReadUShort();
			MajorSubsystemVersion = reader.ReadUShort();
			MinorSubsystemVersion = reader.ReadUShort();
			Win32VersionValue = reader.ReadUInt();
			SizeOfImage = reader.ReadUInt();
			SizeOfHeaders = reader.ReadUInt();
			CheckSum = reader.ReadUInt();
			Subsystem = reader.ReadUShort();
			DllCharacteristics = reader.ReadUShort();
			SizeOfStackReserve = reader.ReadULong();
			SizeOfStackCommit = reader.ReadULong();
			SizeOfHeapReserve = reader.ReadULong();
			SizeOfHeapCommit = reader.ReadULong();
			LoaderFlags = reader.ReadUInt();
			NumberOfRvaAndSizes = reader.ReadUInt();
			for (int i = 0; i < 16; i++)
			{
				DataDirectory[i].VirtualAddress = reader.ReadUInt();
				DataDirectory[i].Size = reader.ReadUInt();
			}
		}
	}
}
