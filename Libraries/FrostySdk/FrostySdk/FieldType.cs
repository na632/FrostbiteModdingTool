using FrostySdk.IO;
using System;

namespace FrostySdk
{
    public struct FieldType
    {
        private string name;

        private Type type;

        private Type baseType;

        private EbxField? fieldType;

        private EbxField? arrayType;

        private MetaDataType? metaData;

        public string Name => name;

        public Type Type => type;

        public Type BaseType => baseType;

        public EbxField? FieldInfo => fieldType;

        public EbxField? ArrayInfo => arrayType;

        public MetaDataType? MetaData => metaData;

        public FieldType(string inName, Type inType, Type inBaseType, EbxField? inFieldType, EbxField? inArrayType = null, MetaDataType? inMetaData = null)
        {
            name = inName;
            type = inType;
            baseType = inBaseType;
            fieldType = inFieldType;
            arrayType = inArrayType;
            metaData = inMetaData;
        }
    }
}
