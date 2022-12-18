using Frostbite.FileManagers;
using FrostySdk.Interfaces;
using System.Collections.Generic;
using System.IO;

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
