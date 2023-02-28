namespace FrostySdk.IO
{
    public struct CatResourceEntry
    {
        public FMT.FileTools.Sha1 Sha1;

        public uint Offset;

        public uint Size;

        public uint LogicalOffset;

        public int ArchiveIndex;

        public bool IsEncrypted;

        public uint Unknown;

        public string KeyId;

        public byte[] UnknownData;

        public uint EncryptedSize;
    }
}
