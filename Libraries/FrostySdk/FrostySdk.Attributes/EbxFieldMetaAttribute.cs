using FrostySdk.IO;
using System;
using System.Linq;
using System.Reflection;

namespace FrostySdk.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public partial class EbxFieldMetaAttribute : Attribute
    {
        public EbxFieldType Type => (EbxFieldType)((uint)(Flags >> 4) & 0x1Fu);

        public EbxFieldType ArrayType => (EbxFieldType)((uint)(ArrayFlags >> 4) & 0x1Fu);

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
                //BaseType = GetType(baseType);
            }
            Flags = (ushort)((uint)type << 4);
            if (arrayType != 0)
            {
                IsArray = true;
                ArrayFlags = (ushort)((uint)arrayType << 4);
            }
        }

        public static Type GetType(string name)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = a.GetTypes().FirstOrDefault(x => x.Name == name);
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }
    }
}
