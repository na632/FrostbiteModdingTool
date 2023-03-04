using FMT.FileTools;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.Managers;
using System;

namespace FrostySdk.Frostbite.Cache
{
    public class BaseCacheReader : ICacheReader
    {
        public ulong EbxDataOffset { get; set; }
        public ulong ResDataOffset { get; set; }
        public ulong ChunkDataOffset { get; set; }
        public ulong NameToPositionOffset { get; set; }

        public virtual bool Read()
        {
            throw new NotImplementedException();
        }

        public EbxAssetEntry ReadEbxAssetEntry(NativeReader nativeReader)
        {
            throw new NotImplementedException();
        }
    }
}
