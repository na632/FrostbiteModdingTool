using System.Collections.Generic;
using System.IO;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.Managers;

namespace Frostbite.FileManagers
{
	public interface ILegacyFileManager
	{
		IEnumerable<AssetEntry> EnumerateAssets(bool modifiedOnly);

		void FlushCache();

		Stream GetAsset(AssetEntry entry);

		AssetEntry GetAssetEntry(string key);

		LegacyFileEntry GetLFEntry(string key);

		void Initialize(ILogger logger);

		void ModifyAsset(string key, byte[] data);

		void SetCacheModeEnabled(bool enabled);
	}
}
