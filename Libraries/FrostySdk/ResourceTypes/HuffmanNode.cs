namespace FrostySdk.Resources
{
    internal class HuffmanNode
    {
        public uint value;

        public HuffmanNode left;

        public HuffmanNode right;

        public char Letter => (char)(~value);

        public uint Value => ~value;

        public object Data
        {
            get;
            set;
        }
    }
}
