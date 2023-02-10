using FrostySdk;
using FrostySdk.FrostbiteSdk.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMT.FileTools.AssetEntry
{
    public class AssetEntryStub : IAssetEntry
    {
        public string Filename { get; set; } = null;

        public string Path { get; set; } = null;

        public string Name { get; set; } = null;

        public string DisplayName { get; set; } = null;

        public bool IsModified { get; set; }

        public Sha1 Sha1 { get; set; }

        public IModifiedAssetEntry ModifiedEntry { get; set; } = null;

        public enum EntryStubType
        {
            Frostbite_Ebx = 1,
            Frostbite_Resource = 2,
            Frostbite_Chunk = 3,
            Frostbite_ChunkFile = 4,
            Frostbite_File = 5,
        }

        public EntryStubType StubType { get; set; }

        public long OriginalSize { get; set; }

        public List<int> Bundles { get; set; }
        public string ExtraInformation { get; set; }
    }
}
