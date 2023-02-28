using Frostbite.FileManagers;
using System.Collections.Generic;

namespace FrostySdk.Managers
{
    public interface ICustomAssetManager : IChunkFileManager
    {
        public List<LegacyFileEntry> AddedFileEntries { get; set; }

        //void ModifyAsset(string key, byte[] data, bool rebuildChunk);

        void AddAsset(string key, LegacyFileEntry lfe);

        void OnCommand(string command, params object[] value);

        void Reset();

        void ResetAndDispose();
    }
}
