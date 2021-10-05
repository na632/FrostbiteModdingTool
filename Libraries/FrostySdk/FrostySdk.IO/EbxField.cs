namespace FrostySdk.IO
{
    public struct EbxField
    //public class EbxField
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

        public uint NameHash;
        //public ulong NameHash;

        public ushort Type;

		public ushort ClassRef;

		public uint DataOffset;

		public uint SecondOffset;

		public EbxFieldType DebugType => (EbxFieldType)((Type >> 4) & 0x1F);
		public EbxFieldType DebugType2 => (EbxFieldType)((Type >> 4));
        public EbxFieldType22 DebugType22 => (EbxFieldType22)Type;

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
            return base.ToString();
        }
    }
}
