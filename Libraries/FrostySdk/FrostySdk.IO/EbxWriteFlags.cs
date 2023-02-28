using System;

namespace FrostySdk.IO
{
    [Flags]
    public enum EbxWriteFlags
    {
        None = 0x0,
        IncludeTransient = 0x1,
        DoNotSort = 0x2
    }
}
