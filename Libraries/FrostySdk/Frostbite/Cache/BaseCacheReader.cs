using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
