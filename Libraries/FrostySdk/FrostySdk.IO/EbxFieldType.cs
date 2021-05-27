namespace FrostySdk.IO
{
	public enum EbxFieldType : byte
	{
		Inherited = 0,
		DbObject = 1,
		Struct = 2,
		Pointer = 3,
		Array = 4,
		String = 6,
		CString = 7,
		Enum = 8,
		FileRef = 9,
		Boolean = 10,
		Int8 = 11,
		UInt8 = 12,
		Int16 = 13,
		UInt16 = 14,
		Int32 = 0xF,
		UInt32 = 0x10,
		Int64 = 18,
		UInt64 = 17,
		Float32 = 19,
		Float64 = 20,
		Guid = 21,
		Sha1 = 22,
		ResourceRef = 23,
		Delegate = 24,
		TypeRef = 25,
		BoxedValueRef = 26
	}
}
