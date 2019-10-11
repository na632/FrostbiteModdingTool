using System;
using System.Collections.Generic;
using System.Text;

namespace v2k4FIFAModdingCL.Interfaces
{
    interface IFIFAFile
    {
        string FileLocation { get; }
        bool IsLegacyFile { get; }
    }
}
