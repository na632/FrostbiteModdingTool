namespace FrostySdk.IO
{
    public struct EbxField
    //public class EbxField
    {
		public string Name;

		//public uint NameHash;
		public ulong NameHash;

		public ushort Type;

		public ushort ClassRef;

		public uint DataOffset;

		public uint SecondOffset;

		public EbxFieldType DebugType => (EbxFieldType)((Type >> 4) & 0x1F);

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
