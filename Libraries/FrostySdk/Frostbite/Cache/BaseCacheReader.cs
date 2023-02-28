using FrostySdk.Frostbite.PluginInterfaces;
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


    }
}
