using FrostySdk.FrostySdk.Managers;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrostbiteSdk.FrostbiteSdk.Managers
{
    public sealed class EmbeddedFileEntry : AssetEntry
    {
        public string ExportedRelativePath { get; set; }

        public string ImportedFileLocation { get; set; }

        public byte[] Data { get; set; }

        public override bool Equals(object obj)
        {
            if(obj is EmbeddedFileEntry other)
            {
                if(other != null)
                {
                    return other.ImportedFileLocation == this.ImportedFileLocation
                        || other.ExportedRelativePath == this.ExportedRelativePath
                        ;
                }
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
