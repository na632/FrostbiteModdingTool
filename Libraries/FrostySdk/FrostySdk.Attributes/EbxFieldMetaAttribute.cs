using FrostySdk.IO;
using System;

namespace FrostySdk.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class EbxFieldMetaAttribute : Attribute
	{
		public EbxFieldType Type => (EbxFieldType)((Flags >> 4) & 0x1F);

		public EbxFieldType ArrayType => (EbxFieldType)((ArrayFlags >> 4) & 0x1F);

		public ushort Flags
		{
			get;
			set;
		}

		public uint Offset
		{
			get;
			set;
		}

		public Type BaseType
		{
			get;
			set;
		}

		public ushort ArrayFlags
		{
			get;
			set;
		}

		public bool IsArray
		{
			get;
			set;
		}

		public EbxFieldMetaAttribute(ushort flags, uint offset, Type baseType, bool isArray, ushort arrayFlags)
		{
			Flags = flags;
			Offset = offset;
			BaseType = baseType;
			IsArray = isArray;
			ArrayFlags = arrayFlags;
		}

		public EbxFieldMetaAttribute(EbxFieldType type, string baseType = "", EbxFieldType arrayType = EbxFieldType.Inherited)
		{
			BaseType = typeof(object);
			if (baseType != "")
			{
				BaseType = TypeLibrary.GetType(baseType);
			}
			Flags = (ushort)((uint)type << 4);
			if (arrayType != 0)
			{
				IsArray = true;
				ArrayFlags = (ushort)((uint)arrayType << 4);
			}
		}
	}
}
