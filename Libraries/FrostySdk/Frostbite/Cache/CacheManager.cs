using Frosty.Hash;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FrostySdk.Frostbite
{
	/// <summary>
	/// A Build / Load Cache Data via FrostySDK
	/// </summary>
    public class CacheManager : ILogger
    {
		/// <summary>
		/// 
		/// </summary>
		/// <param name="GameVersion"></param>
		/// <param name="GameLocation"></param>
		/// <param name="logger">ILogger</param>
		/// <param name="forceDeleteOfOld">Force the deletion of old Cache to rebuild it again</param>
		/// <param name="loadSDK">If you have already built the SDK, then just use the one you have</param>
		/// <returns></returns>
		public bool LoadData(string GameVersion, string GameLocation, ILogger logger = null, bool forceDeleteOfOld = false, bool loadSDK = false)
		{
            if(AssetManager.Instance != null)
			{
				AssetManager.Instance.Dispose();
				AssetManager.Instance = null;
			}

			if(ResourceManager.Instance != null)
			{
				ResourceManager.Instance.Dispose();
			}

            var result = LoadDataAsync(GameVersion, GameLocation, logger, forceDeleteOfOld, loadSDK).Result;
			return result;
		}

		public async Task<bool> LoadDataAsync(string GameVersion, string GameLocation, ILogger logger = null, bool forceDeleteOfOld = false, bool loadSDK = false)
		{
			Debug.WriteLine($"[DEBUG] BuildCache::LoadDataAsync({GameVersion},{GameLocation})");
			if (ProfileManager.Initialize(GameVersion))
			{
				if (File.Exists(ProfileManager.CacheName + ".cache") && forceDeleteOfOld)
					File.Delete(ProfileManager.CacheName + ".cache");

				if (File.Exists(ProfileManager.CacheName + ".CachingSBData.cache") && forceDeleteOfOld)
					File.Delete(ProfileManager.CacheName + ".CachingSBData.cache");


				return await Task.Run(() => {

					if (ProfileManager.RequiresKey)
					{
						KeyManager.ReadInKeys();
					}

					Debug.WriteLine($"[DEBUG] LoadDataAsync::Initialising Type Library");

					if (TypeLibrary.Initialize(loadSDK))
					{
						if (logger == null)
							logger = this;

						logger.Log("Loaded Type Library SDK");
						new FileSystem(GameLocation);
						new ResourceManager(logger);
						logger.Log("Initialised File & Resource System");
						new AssetManager(logger);
						AssetManager.Instance.RegisterLegacyAssetManager();
                        AssetManager.Instance.SetLogger(logger);
                        AssetManager.Instance.Initialize(additionalStartup: true);

						logger.Log("Initialised Asset Manager");

						return true;
					}
					return false;
				});
			}
			else
            {
				logger.LogError("Profile does not exist");
				Debug.WriteLine($"[ERROR] Failed to initialise");
			}
			return false;
		}


		public static bool HasEbx(string name)
		{
			return GetEbx(name) != null;
		}

        public static EbxAssetEntry GetEbx(string name)
        {
            EbxAssetEntry entry = null;
            using (NativeReader nativeReader = new NativeReader(AssetManager.CacheDecompress()))
            {
                if (nativeReader.ReadLengthPrefixedString() != ProfileManager.ProfileName)
                    return null;

                _ = nativeReader.ReadULong();

                var EbxDataOffset = nativeReader.ReadULong();
                var ResDataOffset = nativeReader.ReadULong();
                var ChunkDataOffset = nativeReader.ReadULong();
                var NameToPositionOffset = nativeReader.ReadULong();


            }
            return entry;
        }

        protected IEnumerable<EbxAssetEntry> EnumerateEbx(string name, string type, bool modifiedOnly, bool includeLinked)
        {
            using (NativeReader nativeReader = new NativeReader(AssetManager.CacheDecompress()))
            {
                if (nativeReader.ReadLengthPrefixedString() != ProfileManager.ProfileName)
                    return null;

                _ = nativeReader.ReadULong();

                var EbxDataOffset = nativeReader.ReadULong();
                var ResDataOffset = nativeReader.ReadULong();
                var ChunkDataOffset = nativeReader.ReadULong();
                var NameToPositionOffset = nativeReader.ReadULong();

				nativeReader.Position = (long)EbxDataOffset;
				var ebxCount = nativeReader.ReadUInt();


            }
            return new List<EbxAssetEntry>();
        }

        private string LastMessage = null;

		public void Log(string text, params object[] vars)
        {
			if(!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(LastMessage))
			{
				Debug.WriteLine(text);
            }
			LastMessage = text;
        }

        public void LogError(string text, params object[] vars)
        {
        }

        public void LogWarning(string text, params object[] vars)
        {
        }
    }
}
