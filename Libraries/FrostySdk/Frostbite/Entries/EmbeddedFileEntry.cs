﻿using FrostySdk.Managers;

namespace FrostbiteSdk.FrostbiteSdk.Managers
{
    public sealed class EmbeddedFileEntry : AssetEntry
    {
        private string exportedRelativePath;

        public string ExportedRelativePath
        {
            get { return exportedRelativePath != null ? exportedRelativePath : Name; }
            set { exportedRelativePath = value; }
        }

        public string ImportedFileLocation { get; set; }

        public byte[] Data { get; set; }

        public bool IsAppended { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is EmbeddedFileEntry other)
            {
                if (other != null)
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
