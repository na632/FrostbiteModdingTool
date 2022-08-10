using FrostySdk.Interfaces;
using System.Collections.Generic;
using System.IO;

namespace FrostySdk.Managers
{
	public interface ICustomAssetManager
	{
		public List<LegacyFileEntry> AddedFileEntries { get; set; }

		void Initialize(ILogger logger);

		AssetEntry GetAssetEntry(string key);

		Stream GetAsset(AssetEntry entry);

		void ModifyAsset(string key, byte[] data);

		void ModifyAsset(string key, byte[] data, bool rebuildChunk);

		void AddAsset(string key, LegacyFileEntry lfe);

		IEnumerable<AssetEntry> EnumerateAssets(bool modifiedOnly);

		void OnCommand(string command, params object[] value);

		void Reset();

		void ResetAndDispose();
	}
}
