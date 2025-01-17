namespace FrostySdk.IO
{
    public enum EbxFieldType : byte
    {
        /*
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
		*/
        Inherited = 0,
        DbObject = 1,
        Struct = 2,
        Pointer = 3,
        Array = 4,
        FixedArray = 5,
        String = 6,
        CString = 7,
        Enum = 8,
        FileRef = 9,
        Boolean = 10,
        Int8 = 11,
        UInt8 = 12,
        Int16 = 13,
        UInt16 = 14,
        Int32 = 15,
        UInt32 = 16,
        Int64 = 17,
        UInt64 = 18,
        Float32 = 19,
        Float64 = 20,
        Guid = 21,
        Sha1 = 22,
        ResourceRef = 23,
        Function = 24,
        TypeRef = 25,
        BoxedValueRef = 26,
        Interface = 27,
        Delegate = 28
    }

    public enum EbxFieldType22 : short
    {
        Inherited = 1,
        Pointer = 49,
        Float32 = 307,
        ArrayOfCString = 116,
        ArrayOfStructs = 36,
        Array = 308,

        ArrayOfInt2 = 244,
        ArrayOfUInt = 260,

    }

    public enum EbxFieldCategory : short
    {
        NotApplicable,
        Class,
        Value,
        Primitive,
        Array,
        Enum,
        Function,
        Interface,
        Delegate

    }
}
