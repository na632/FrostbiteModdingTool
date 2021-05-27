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

namespace FrostySdk.Frostbite
{
	/// <summary>
	/// A Build / Load Cache Data via FrostySDK
	/// </summary>
    public class BuildCache : ILogger
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
			var result = LoadDataAsync(GameVersion, GameLocation, logger, forceDeleteOfOld, loadSDK).Result;
			return result;
		}
		public async Task<bool> LoadDataAsync(string GameVersion, string GameLocation, ILogger logger = null, bool forceDeleteOfOld = false, bool loadSDK = false)
		{
			Debug.WriteLine($"[DEBUG] BuildCache::LoadDataAsync({GameVersion},{GameLocation})");
			if (ProfilesLibrary.Initialize(GameVersion))
			{
				if (File.Exists(ProfilesLibrary.CacheName + ".cache") && forceDeleteOfOld)
					File.Delete(ProfilesLibrary.CacheName + ".cache");

				if (File.Exists(ProfilesLibrary.CacheName + ".CachingSBData.cache") && forceDeleteOfOld)
					File.Delete(ProfilesLibrary.CacheName + ".CachingSBData.cache");


				return await Task.Run(() => { 
				
						if (ProfilesLibrary.RequiresKey)
						{
							byte[] array;

							//array = NativeReader.ReadInStream(new FileStream(ProfilesLibrary.CacheName + ".key", FileMode.Open, FileAccess.Read));
							// change this so it reads the easy version of the key
							// 0B0E04030409080C010708010E0B0B02﻿
			Debug.WriteLine($"[DEBUG] LoadDataAsync::Reading the Key");


							array = NativeReader.ReadInStream(new FileStream("fifa20.key", FileMode.Open, FileAccess.Read));
							byte[] array2 = new byte[16];
							Array.Copy(array, array2, 16);
							KeyManager.Instance.AddKey("Key1", array2);
							if (array.Length > 16)
							{
								array2 = new byte[16];
								Array.Copy(array, 16, array2, 0, 16);
								KeyManager.Instance.AddKey("Key2", array2);
								array2 = new byte[16384];
								Array.Copy(array, 32, array2, 0, 16384);
								KeyManager.Instance.AddKey("Key3", array2);
							}

						Debug.WriteLine($"[DEBUG] LoadDataAsync::Initialising Type Library");

						if (TypeLibrary.Initialize(loadSDK))
						{
							if (logger == null)
								logger = this;

							logger.Log("Loaded Type Library SDK");

							AssetManagerImportResult result = new AssetManagerImportResult();

							var FileSystem = new FileSystem(GameLocation);

							bool patched = false;

							foreach (FileSystemSource source in ProfilesLibrary.Sources)
							{
								FileSystem.AddSource(source.Path, source.SubDirs);
								if (source.Path.ToLower().Contains("patch"))
									patched = true;
							}
							byte[] key = KeyManager.Instance.GetKey("Key1");
							FileSystem.Initialize(key, patched);

							logger.Log("Initialised File System");

							var ResourceManager = new ResourceManager(FileSystem);
							ResourceManager.SetLogger(logger);
							ResourceManager.Initialize();

							logger.Log("Initialised Resource System");

							if (AssetManager.Instance == null)
							{
								var assetManager = new AssetManager(FileSystem, ResourceManager);
								assetManager.RegisterLegacyAssetManager();
								assetManager.SetLogger(logger);
								assetManager.Initialize(additionalStartup: true, result);

								logger.Log("Initialised Asset Manager");

							}
							else
                            {
								AssetManager.Instance.SetLogger(logger);
							}
							return true;
						}
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
