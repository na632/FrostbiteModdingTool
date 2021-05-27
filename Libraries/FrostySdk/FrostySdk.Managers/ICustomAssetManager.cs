using FrostySdk.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace FrostySdk.Managers
{
	public interface ICustomAssetManager
	{
		void Initialize(ILogger logger);

		AssetEntry GetAssetEntry(string key);

		Stream GetAsset(AssetEntry entry);

		void ModifyAsset(string key, byte[] data);

		void ModifyAsset(string key, byte[] data, bool rebuildChunk);

		IEnumerable<AssetEntry> EnumerateAssets(bool modifiedOnly);

		void OnCommand(string command, params object[] value);
	}
}
