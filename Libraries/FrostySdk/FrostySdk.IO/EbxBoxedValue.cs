using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostySdk.IO
{
    public struct EbxBoxedValue
    {
        public uint Offset;

        public ushort ClassRef;

        public ushort Type;
    }
}
