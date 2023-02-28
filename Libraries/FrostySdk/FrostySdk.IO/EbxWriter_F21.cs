using FrostySdk.FrostySdk.IO;
using System.IO;

namespace FrostySdk.IO
{
    public class EbxWriter_F21 : EbxWriterV3
    {
        public EbxWriter_F21(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.IncludeTransient, bool leaveOpen = false)
            : base(inStream, inFlags)
        {
        }
    }
}