using FrostySdk.IO;
using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false)]
	public class EbxClassMetaAttribute : Attribute
	{
		//public EbxFieldType Type => (EbxFieldType)((Flags >> 4) & 0x1F);
		public EbxFieldType Type => (EbxFieldType)((Flags >> 4) & 0x1Fu);

        public ushort Flags
		{
			get;
			set;
		}

		public byte Alignment
		{
			get;
			set;
		}

		public ushort Size
		{
			get;
			set;
		}

		public string Namespace
		{
			get;
			set;
		}

		public EbxClassMetaAttribute(ushort flags, byte alignment, ushort size, string nameSpace)
		{
			Flags = flags;
			Alignment = alignment;
			Size = size;
			Namespace = nameSpace;
		}

		public EbxClassMetaAttribute(EbxFieldType type)
		{
			Flags = (ushort)((uint)type << 4);
		}
	}
}
