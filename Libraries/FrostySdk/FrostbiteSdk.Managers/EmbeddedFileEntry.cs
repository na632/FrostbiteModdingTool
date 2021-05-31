using FrostySdk.FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrostbiteSdk.FrostbiteSdk.Managers
{
    public sealed class EmbeddedFileEntry
    {
        public string Name { get; set; }

        public string ExportedRelativePath { get; set; }

        public byte[] Data { get; set; }
    }
}
