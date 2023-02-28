using FrostySdk.IO;
using System.IO;

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
