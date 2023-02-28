using FrostySdk.FrostySdk.IO._2022.Readers;

namespace FrostySdk.IO
{
    public struct EbxField
    {
        private string name;

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(EbxSharedTypeDescriptors.GetPropertyName(NameHash)))
                {
                    name = EbxSharedTypeDescriptors.GetPropertyName(NameHash);
                }

                return name;

            }
            set { name = value; }
        }

        //public uint NameHash;
        public uint NameHash { get; set; }

        public uint NameHashU => NameHash;
        //public ulong NameHash;

        public ushort Type { get; set; }

        public ushort ClassRef;

        public uint DataOffset;

        public uint SecondOffset;

        public uint ThirdOffset;

        public EbxFieldType DebugType => DebugTypeOverride.HasValue ? DebugTypeOverride.Value : (EbxFieldType)((uint)(Type >> 4) & 0x1Fu);
        public EbxFieldType? DebugTypeOverride { get; set; }
        public EbxFieldType InternalType => (EbxFieldType)((Type >> 4) & 0x1F);
        public EbxFieldCategory Category => (EbxFieldCategory)(Type & 0xFu);

        public uint Unk3 { get; internal set; }
        public EbxTypeCategory TypeCategory => (EbxTypeCategory)(this.Type & 0xFu);

        public override bool Equals(object obj)
        {
            if (obj is EbxField)
            {
                EbxField other = (EbxField)obj;
                return other.NameHash == NameHash && (other.DebugType == this.DebugType || other.DebugType == this.InternalType);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }
            return base.ToString();
        }
    }
}
