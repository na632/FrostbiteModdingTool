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
        public int NameHash { get; set; }   

        public uint NameHashU => (uint)NameHash;
        //public ulong NameHash;

        public ushort Type { get; set; }

		public ushort ClassRef;

		public uint DataOffset;

		public uint SecondOffset;

		public uint ThirdOffset;

        //public EbxFieldType DebugType => (EbxFieldType)((Type >> 4) & 0x1Fu);
        public EbxFieldType DebugType => DebugTypeOverride.HasValue ? DebugTypeOverride.Value : (EbxFieldType)((Type >> 4) & 0x1F);
        public EbxFieldType? DebugTypeOverride { get; set; }
        public EbxFieldType InternalType => (EbxFieldType)((Type >> 4) & 0x1F);
        public EbxFieldType22 DebugType22 => (EbxFieldType22)Type;
        public EbxFieldCategory Category => (EbxFieldCategory)(Type & 0xFu);

        public uint Unk3 { get; internal set; }
        public ushort Flags { get; internal set; }
        public EbxTypeCategory TypeCategory => (EbxTypeCategory)(this.Flags & 0xFu);

        public override bool Equals(object obj)
        {
            if(obj is EbxField)
            {
                EbxField other = (EbxField)obj;
                return other.NameHash == NameHash;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            if(!string.IsNullOrEmpty(Name))
            {
                return Name;
            }
            return base.ToString();
        }
    }
}
