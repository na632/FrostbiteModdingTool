namespace FrostySdk.IO
{
	public struct EbxClass
	{
        private string name;

        public string Name
        {
            get 
            {
                if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(EbxSharedTypeDescriptors.GetClassName(NameHash)))
                {
                    name = EbxSharedTypeDescriptors.GetClassName(NameHash);
                }
    
                return name; 
            
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

		public EbxFieldType DebugType => (EbxFieldType)((Type >> 4) & 0x1F);

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
            return base.ToString();
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
