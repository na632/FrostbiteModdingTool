using System.Collections.Generic;
using System.IO;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.Managers;

namespace Frostbite.FileManagers
{
	public interface IChunkFileManager
	{
		IEnumerable<AssetEntry> EnumerateAssets(bool modifiedOnly);

		void FlushCache();

		Stream GetAsset(AssetEntry entry);

		AssetEntry GetAssetEntry(string key);

		LegacyFileEntry GetLFEntry(string key);

		void Initialize(ILogger logger);

		void ModifyAsset(string key, byte[] data);

        void ModifyAsset(string key, byte[] data, bool rebuildChunk);

        void RevertAsset(AssetEntry assetEntry);

        List<LegacyFileEntry> ModifyAssets(Dictionary<string, byte[]> data, bool rebuildChunk);

        void SetCacheModeEnabled(bool enabled);
		void DuplicateAsset(string name, LegacyFileEntry originalAsset);

    }
}
