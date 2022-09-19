namespace FrostySdk.IO
{
	public struct EbxClass
	{
        private string name;

        public string Name
        {
            get 
            {
                if(!string.IsNullOrEmpty(name))
                    return name;

                //name = this.ToString();
                return !string.IsNullOrEmpty(name) && !name.Contains("EbxClass") ? name : string.Empty;
            
            }
            set { name = value; }
        }


        public uint NameHash;

		public int FieldIndex;

		public byte FieldCount;

		public byte Alignment;

		public ushort Type;

		public ushort Size;

		public ushort SecondSize;

		public string Namespace;

		public int Index;

        //public EbxFieldType DebugType => (EbxFieldType)((uint)(Type >> 4) & 0x1Fu);
        public EbxFieldType DebugType => DebugTypeOverride.HasValue ? DebugTypeOverride.Value : (EbxFieldType)((Type >> 4) & 0x1Fu);
        public EbxFieldType? DebugTypeOverride { get; set; }
        public EbxFieldCategory Category => (EbxFieldCategory)((uint)(Type >> 4) & 0x1Fu);

        public override string ToString()
        {
            if(!string.IsNullOrEmpty(EbxSharedTypeDescriptors.GetClassName(NameHash)))
            {
                if(NameHash > 0)
                {
                    return EbxSharedTypeDescriptors.GetClassName(NameHash) + " - " + NameHash;
                }
                else
                {
                    return EbxSharedTypeDescriptors.GetClassName(NameHash);
                }
            }
            return Name.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
