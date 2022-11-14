using FrostySdk.FrostySdk.IO;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostySdk.IO
{
    public class EbxWriterRiff : EbxWriter2023
    {
        public EbxWriterRiff(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.None, bool leaveOpen = false)
           : base(inStream, inFlags, true)
        {
        }
    }
}
