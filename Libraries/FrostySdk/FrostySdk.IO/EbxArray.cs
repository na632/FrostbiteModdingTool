namespace FrostySdk.IO
{
    public struct EbxArray
    {
        public int ClassRef;

        public uint Offset;

        public uint Count;

        public uint PathDepth { get; internal set; }
        public int TypeFlags { get; internal set; }
    }
}
