using Frostbite.FileManagers;
using FrostbiteSdk.Frostbite.FileManagers;
using FrostbiteSdk.FrostbiteSdk.Managers;
using Frosty.Hash;
using FrostySdk.Frostbite.IO;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Resources;
using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace FrostySdk.Managers
{
	public interface IAssetLoader
	{
		void Load(AssetManager parent, BinarySbDataHelper helper);
	}
	public interface IAssetCompiler
	{
		bool Compile(FileSystem fs, ILogger logger, object frostyModExecuter);
	}

	public class BinarySbDataHelper
	{
		protected Dictionary<string, byte[]> ebxDataFiles = new Dictionary<string, byte[]>();

		protected Dictionary<string, byte[]> resDataFiles = new Dictionary<string, byte[]>();

		protected Dictionary<string, byte[]> chunkDataFiles = new Dictionary<string, byte[]>();

		private AssetManager am;

		public BinarySbDataHelper(AssetManager inParent)
		{
			am = inParent;
		}

		public void FilterAndAddBundleData(DbObject baseList, DbObject deltaList)
		{
			FilterBinaryBundleData(baseList, deltaList, "ebx", ebxDataFiles);
			FilterBinaryBundleData(baseList, deltaList, "res", resDataFiles);
			FilterBinaryBundleData(baseList, deltaList, "chunks", chunkDataFiles);
		}

		public void RemoveEbxData(string name)
		{
			ebxDataFiles.Remove(name);
		}

		public void RemoveResData(string name)
		{
			resDataFiles.Remove(name);
		}

		public void RemoveChunkData(string name)
		{
			chunkDataFiles.Remove(name);
		}

		//public void WriteToCache(AssetManager am)
		//{
		//	if (ebxDataFiles.Count + resDataFiles.Count + chunkDataFiles.Count != 0)
		//	{
		//		using (BinaryWriter binaryWriter = new BinaryWriter(new FileStream(am.fs.CacheName + "_sbdata.cas", FileMode.Create)))
		//		{
		//			foreach (KeyValuePair<string, byte[]> ebxDataFile in ebxDataFiles)
		//			{
		//				am.EbxList[ebxDataFile.Key].ExtraData.DataOffset = (uint)binaryWriter.BaseStream.Position;
		//				binaryWriter.Write(ebxDataFile.Value);
		//			}
		//			foreach (KeyValuePair<string, byte[]> resDataFile in resDataFiles)
		//			{
		//				am.resList[resDataFile.Key].ExtraData.DataOffset = (uint)binaryWriter.BaseStream.Position;
		//				binaryWriter.Write(resDataFile.Value);
		//			}
		//			foreach (KeyValuePair<string, byte[]> chunkDataFile in chunkDataFiles)
		//			{
		//				Guid key = new Guid(chunkDataFile.Key);
		//				am.chunkList[key].ExtraData.DataOffset = (uint)binaryWriter.BaseStream.Position;
		//				binaryWriter.Write(chunkDataFile.Value);
		//			}
		//		}
		//		ebxDataFiles.Clear();
		//		resDataFiles.Clear();
		//		chunkDataFiles.Clear();
		//	}
		//}

		private void FilterBinaryBundleData(DbObject baseList, DbObject deltaList, string listName, Dictionary<string, byte[]> dataFiles)
		{
			foreach (DbObject item in deltaList.GetValue<DbObject>(listName))
			{
				Sha1 value = item.GetValue<Sha1>("sha1");
				string text = item.GetValue<string>("name");
				if (text == null)
				{
					text = item.GetValue<Guid>("id").ToString();
				}
				if (!dataFiles.ContainsKey(text))
				{
					bool flag = false;
					if (baseList != null)
					{
						foreach (DbObject item2 in baseList.GetValue<DbObject>(listName))
						{
							if (item2.GetValue<Sha1>("sha1") == value)
							{
								item.SetValue("size", item2.GetValue("size", 0L));
								item.SetValue("originalSize", item2.GetValue("originalSize", 0L));
								item.SetValue("offset", item2.GetValue("offset", 0L));
								item.RemoveValue("data");
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						byte[] array = Utils.CompressFile(item.GetValue<byte[]>("data"));
						dataFiles.Add(text, array);
						item.SetValue("size", array.Length);
						item.AddValue("cache", true);
						item.RemoveValue("sb");
					}
				}
			}
		}
	}


	public class AssetManager : ILoggable, IDisposable
	{
		private static AssetManager _Instance;
        public static AssetManager Instance 
		{ 
			get 
			{ 
				if(_Instance == null)
                {
                }
				return _Instance;
			
			} 
			set 
			{ 
				_Instance = value; 
			} 
		}

		public Dictionary<int, string> ModCASFiles = new Dictionary<int, string>();


		public class BaseBundleInfo
		{
			public string Name;

			public long Offset;

			public long Size;
		}

		internal class FifaAssetLoader : IAssetLoader
		{
			internal struct BundleFileInfo
			{
				public int Index;

				public int Offset;

				public int Size;

				public BundleFileInfo(int index, int offset, int size)
				{
					Index = index;
					Offset = offset;
					Size = size;
				}
			}

			public void Load(AssetManager parent, BinarySbDataHelper helper)
			{
				byte[] key = KeyManager.Instance.GetKey("Key2");
				foreach (Catalog item2 in parent.fs.EnumerateCatalogInfos())
				{
					foreach (string sbName in item2.SuperBundles.Keys)
					{
						SuperBundleEntry superBundleEntry = parent.superBundles.Find((SuperBundleEntry a) => a.Name == sbName);
						int num = -1;
						if (superBundleEntry != null)
						{
							num = parent.superBundles.IndexOf(superBundleEntry);
						}
						else
						{
							parent.superBundles.Add(new SuperBundleEntry
							{
								Name = sbName
							});
							num = parent.superBundles.Count - 1;
						}
						parent.WriteToLog($"Loading data ({sbName})");
						parent.superBundles.Add(new SuperBundleEntry
						{
							Name = sbName
						});
						string arg = sbName;
						if (item2.SuperBundles[sbName])
						{
							arg = sbName.Replace("win32", item2.Name);
						}
						string text = parent.fs.ResolvePath($"{arg}.toc");
						if (text != "")
						{
							//if(ProfilesLibrary.IsMadden21DataVersion())
       //                     {
							//	if (!text.Contains("playercontent_sb") // Player uniforms / kits
					  // && !text.Contains("playercontentlaunch_sb") // Player uniforms / kits
							//		)
							//		continue;
							//}
							int num2 = 0;
							int num3 = 0;
							byte[] array = null;
							using (NativeReader nativeReader = new NativeReader(new FileStream(text, FileMode.Open, FileAccess.Read), parent.fs.CreateDeobfuscator()))
							{
								uint num4 = nativeReader.ReadUInt();
								num2 = nativeReader.ReadInt() - 12;
								num3 = nativeReader.ReadInt() - 12;
								array = nativeReader.ReadToEnd();
								if (num4 == 3286619587u)
								{
									using (Aes aes = Aes.Create())
									{
										aes.Key = key;
										aes.IV = key;
										aes.Padding = PaddingMode.None;
										ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
										using (MemoryStream stream = new MemoryStream(array))
										{
											using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read))
											{
												cryptoStream.Read(array, 0, array.Length);
											}
										}
									}
								}
							}
							if (array.Length != 0)
							{
								using (NativeReader nativeReader2 = new NativeReader(new MemoryStream(array)))
								{
									List<int> list = new List<int>();
									if (num2 > 0)
									{
										nativeReader2.Position = num2;
										int numberOfBundles = nativeReader2.ReadInt();
										for (int i = 0; i < numberOfBundles; i++)
										{
											list.Add(nativeReader2.ReadInt());
										}
										DateTime lastLogTime = DateTime.Now;

										for (int j = 0; j < numberOfBundles; j++)
										{
											if (lastLogTime.AddSeconds(15) < DateTime.Now)
											{
												var percentDone = Math.Round((double)j / (double)numberOfBundles * 100.0);
												parent.logger.Log($"{arg} Progress: {percentDone}");
												lastLogTime = DateTime.Now;
											}


											int num6 = nativeReader2.ReadInt() - 12;
											long position = nativeReader2.Position;
											nativeReader2.Position = num6;
											int num7 = nativeReader2.ReadInt() - 1;
											List<BundleFileInfo> list2 = new List<BundleFileInfo>();
											MemoryStream memoryStream = new MemoryStream();
											int num8;
											do
											{
												num8 = nativeReader2.ReadInt();
												int num9 = nativeReader2.ReadInt();
												int num10 = nativeReader2.ReadInt();
												using (NativeReader nativeReader3 = new NativeReader(new FileStream(parent.fs.ResolvePath(parent.fs.GetFilePath(num8 & int.MaxValue)), FileMode.Open, FileAccess.Read)))
												{
													nativeReader3.Position = num9;
													memoryStream.Write(nativeReader3.ReadBytes(num10), 0, num10);
												}
												list2.Add(new BundleFileInfo(num8 & int.MaxValue, num9, num10));
											}
											while ((num8 & 2147483648u) != 0L);
											nativeReader2.Position = num7 - 12;
											int num11 = 0;
											string text2 = "";
											do
											{
												string str = nativeReader2.ReadNullTerminatedString();
												num11 = nativeReader2.ReadInt() - 1;
												text2 += str;
												if (num11 != -1)
												{
													nativeReader2.Position = num11 - 12;
												}
											}
											while (num11 != -1);
											text2 = Utils.ReverseString(text2);
											nativeReader2.Position = position;
											BundleEntry item = new BundleEntry
											{
												Name = text2,
												SuperBundleId = num
											};
											parent.bundles.Add(item);
											BinarySbReader binarySbReader = null;
											if (ProfilesLibrary.IsMadden21DataVersion())
												binarySbReader = new BinarySbReaderV2(memoryStream, 0L, parent.fs.CreateDeobfuscator());
											else
												binarySbReader = new BinarySbReader(memoryStream, 0L, parent.fs.CreateDeobfuscator());

											using (binarySbReader)
											{
												DbObject dbObject = binarySbReader.ReadDbObject();
												BundleFileInfo bundleFileInfo = list2[0];
												long offset = bundleFileInfo.Offset + (dbObject.GetValue("dataOffset", 0L) + 4);
												long currentSize = bundleFileInfo.Size - (dbObject.GetValue("dataOffset", 0L) + 4);
												int num14 = 0;
												foreach (DbObject item3 in dbObject.GetValue<DbObject>("ebx"))
												{
													if (currentSize == 0L)
													{
														bundleFileInfo = list2[++num14];
														currentSize = bundleFileInfo.Size;
														offset = bundleFileInfo.Offset;
													}
													int value = item3.GetValue("size", 0);
													item3.SetValue("offset", offset);
													item3.SetValue("cas", bundleFileInfo.Index);
													offset += value;
													currentSize -= value;
												}
												foreach (DbObject item4 in dbObject.GetValue<DbObject>("res"))
												{
													if (currentSize == 0L)
													{
														bundleFileInfo = list2[++num14];
														currentSize = bundleFileInfo.Size;
														offset = bundleFileInfo.Offset;
													}
													int value2 = item4.GetValue("size", 0);
													item4.SetValue("offset", offset);
													item4.SetValue("cas", bundleFileInfo.Index);
													offset += value2;
													currentSize -= value2;
												}
												foreach (DbObject item5 in dbObject.GetValue<DbObject>("chunks"))
												{
													if (currentSize == 0L)
													{
														bundleFileInfo = list2[++num14];
														currentSize = bundleFileInfo.Size;
														offset = bundleFileInfo.Offset;
													}
													int value3 = item5.GetValue("size", 0);
													item5.SetValue("offset", offset);
													item5.SetValue("cas", bundleFileInfo.Index);
													offset += value3;
													currentSize -= value3;
												}
												parent.ProcessBundleEbx(dbObject, parent.bundles.Count - 1, helper);
												parent.ProcessBundleRes(dbObject, parent.bundles.Count - 1, helper);
												parent.ProcessBundleChunks(dbObject, parent.bundles.Count - 1, helper);
											}
										}
									}
									if (num3 > 0)
									{
										nativeReader2.Position = num3;
										int num15 = nativeReader2.ReadInt();
										list = new List<int>();
										for (int k = 0; k < num15; k++)
										{
											list.Add(nativeReader2.ReadInt());
										}
										for (int l = 0; l < num15; l++)
										{
											int num16 = nativeReader2.ReadInt();
											long position2 = nativeReader2.Position;
											nativeReader2.Position = num16 - 12;
											Guid guid = nativeReader2.ReadGuid();
											int index = nativeReader2.ReadInt();
											int offset = nativeReader2.ReadInt();
											int num18 = nativeReader2.ReadInt();
											if (!parent.Chunks.ContainsKey(guid))
											{
												//parent.chunkList.Add(guid, new ChunkAssetEntry());
												parent.Chunks.TryAdd(guid, new ChunkAssetEntry());
											}
											ChunkAssetEntry chunkAssetEntry = parent.Chunks[guid];
											chunkAssetEntry.Id = guid;
											chunkAssetEntry.Size = num18;
											chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
											chunkAssetEntry.ExtraData = new AssetExtraData();
											chunkAssetEntry.ExtraData.CasPath = parent.fs.GetFilePath(index);
											chunkAssetEntry.ExtraData.DataOffset = (uint)offset;
											parent.Chunks[guid].IsTocChunk = true;
											nativeReader2.Position = position2;
										}
									}
								}
							}
						}
						num++;
					}
				}
			}
		}

		private const ulong CacheMagic = 144213406785688134uL;

		private const uint CacheVersion = 2u;

		public FileSystem fs;

		public FileSystem FileSystem => fs;

		public ResourceManager rm;

		public ResourceManager ResourceManager => rm;


		public ILogger logger;

		public List<SuperBundleEntry> superBundles = new List<SuperBundleEntry>(500);

		public List<BundleEntry> bundles = new List<BundleEntry>(999999);

        public ConcurrentDictionary<string, EbxAssetEntry> EBX = new ConcurrentDictionary<string, EbxAssetEntry>(8, 999999, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// NameHash to EbxAssetEntry
        /// </summary>
        //public ConcurrentDictionary<int, EbxAssetEntry> EbxList = new ConcurrentDictionary<int, EbxAssetEntry>();

        public Dictionary<string, ResAssetEntry> RES = new Dictionary<string, ResAssetEntry>(999999);

        public ConcurrentDictionary<Guid, ChunkAssetEntry> Chunks = new ConcurrentDictionary<Guid, ChunkAssetEntry>(8, 999999);

        public ConcurrentDictionary<(string, Guid), ChunkAssetEntry> BundleChunks = new ConcurrentDictionary<(string, Guid), ChunkAssetEntry>(8, 999999);

        //public ConcurrentDictionary<Guid, EbxAssetEntry> ebxGuidList = new ConcurrentDictionary<Guid, EbxAssetEntry>();

        public Dictionary<ulong, ResAssetEntry> resRidList = new Dictionary<ulong, ResAssetEntry>(999999);

		public Dictionary<string, ICustomAssetManager> CustomAssetManagers = new Dictionary<string, ICustomAssetManager>(1);

		public List<EmbeddedFileEntry> EmbeddedFileEntries = new List<EmbeddedFileEntry>();



		public AssetManager(FileSystem inFs, ResourceManager inRm)
		{
			fs = inFs;
			rm = inRm;

            Instance = this;
		}

		// To detect redundant calls
		private bool _disposed = false;

		~AssetManager() => Dispose(false);

		public void Dispose()
		{
			// Dispose of unmanaged resources.
			Dispose(true);
			// Suppress finalization.
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			// Managed Resources
			if (disposing)
			{
				bundles.Clear();
				bundles = null;
				EBX.Clear();
				EBX = null;
				RES.Clear();
				RES = null;
				resRidList.Clear();
				resRidList = null;
				Chunks.Clear();
				Chunks = null;

				TypeLibrary.ExistingAssembly = null;
			}
		}

		public void RegisterCustomAssetManager(string type, Type managerType)
		{
			CustomAssetManagers.Add(type, (ICustomAssetManager)Activator.CreateInstance(managerType));
		}

        public void RegisterLegacyAssetManager()
        {
			if (ProfilesLibrary.IsMadden21DataVersion() || ProfilesLibrary.IsFIFA21DataVersion())
			{
				CustomAssetManagers.Add("legacy", new LegacyFileManager_M21());
			}
			else
			{
				CustomAssetManagers.Add("legacy", new LegacyFileManager(this));
			}
			
        }

		private static List<string> PluginAssemblies = new List<string>();

		public static bool InitialisePlugins()
        {

			if (Directory.Exists("Plugins"))
			{
				foreach (var p in Directory.EnumerateFiles("Plugins"))
				{
					if (p.ToLower().EndsWith(".dll") && p.ToLower().Contains(ProfilesLibrary.ProfileName.ToLower()))
					{
                        if (Assembly.UnsafeLoadFrom(p) != null)
                        //if (Assembly.LoadFrom(p) != null)
                        {
							if (!PluginAssemblies.Contains(p))
								PluginAssemblies.Add(p);

							return true;
						}
					}
				}
			}

			return false;
		}

		public static bool CacheUpdate = false;

		public static object LoadTypeFromPlugin(string className)
		{
			foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Contains("Plugin")))
			{
				var t = a.GetTypes().FirstOrDefault(x => x.Name == className);
				if (t != null)
					return Activator.CreateInstance(t);
			}
			throw new ArgumentNullException("Unable to find Plugin or Class");
		}


        public void Initialize(bool additionalStartup = true, AssetManagerImportResult result = null)
		{
			logger.Log("Initialising Plugins");
			if(!InitialisePlugins())
            {
				throw new Exception("Plugins could not be initialised!");
            }				
			TypeLibrary.Initialize(TypeLibrary.RequestLoadSDK);
			if (TypeLibrary.RequestLoadSDK && File.Exists("SDK/" + ProfilesLibrary.SDKFilename + ".dll"))
			{
				logger.Log($"Plugins and SDK {"SDK/" + ProfilesLibrary.SDKFilename + ".dll"} Initialised");
			}

			DateTime now = DateTime.Now;
			List<EbxAssetEntry> prePatchCache = new List<EbxAssetEntry>();

			if (!ReadFromCache(out prePatchCache))
			{
				logger.Log($"Cache Needs to Built/Updated");

				BinarySbDataHelper binarySbDataHelper = new BinarySbDataHelper(this);
				if (ProfilesLibrary.AssetLoader != null)
					((IAssetLoader)Activator.CreateInstance(ProfilesLibrary.AssetLoader)).Load(this, binarySbDataHelper);
				else
				{
					foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies().Where(x=>x.FullName.Contains("Plugin")))
					{
						var t = a.GetTypes().FirstOrDefault(x => x.Name == ProfilesLibrary.AssetLoaderName);
						if(t != null)
							((IAssetLoader)Activator.CreateInstance(t)).Load(this, binarySbDataHelper);
					}
				}
				//binarySbDataHelper.WriteToCache(this);
				GC.Collect();
				WriteToCache();
			}
			
			DoEbxIndexing();

			if (!additionalStartup || TypeLibrary.ExistingAssembly == null)
			{
				return;
			}
			foreach (BundleEntry bundle in bundles)
			{
				bundle.Type = BundleType.SharedBundle;
				bundle.Blueprint = GetEbxEntry(bundle.Name.Remove(0, 6));
				if (bundle.Blueprint == null)
				{
					bundle.Blueprint = GetEbxEntry(bundle.Name);
				}
				if (bundle.Blueprint != null)
				{
					bundle.Type = BundleType.SubLevel;
					if (TypeLibrary.IsSubClassOf(bundle.Blueprint.Type, "BlueprintBundle"))
					{
						bundle.Type = BundleType.BlueprintBundle;
					}
				}
			}
			foreach (ICustomAssetManager value in CustomAssetManagers.Values)
			{
				value.Initialize(logger);
			}
			
			//if (ProfilesLibrary.IsFIFADataVersion() 
			//	|| ProfilesLibrary.IsMaddenDataVersion()
			//	|| ProfilesLibrary.IsFIFA21DataVersion()
			//	)
			//{
			//	TypeLibrary.Reflection.LoadClassInfoAssets(this);
			//}

			TimeSpan timeSpan = DateTime.Now - now;
			logger.Log($"Loading complete {timeSpan.ToString()}");

		}

		public void SetLogger(ILogger inLogger)
		{
			logger = inLogger;
		}

		public void ClearLogger()
		{
			logger = null;
		}


		private List<Type> AllSdkAssemblyTypes { get; set; }

		private List<EbxAssetEntry> _EbxItemsWithNoType;
		private List<EbxAssetEntry> EbxItemsWithNoType
		{
			get
			{
				if(_EbxItemsWithNoType == null)
					_EbxItemsWithNoType = EBX.Values.Where(x => string.IsNullOrEmpty(x.Type)).OrderBy(x=>x.ExtraData.CasPath).ToList();

				return _EbxItemsWithNoType;
			}

		}

        public bool ForceChunkRemoval { get; set; }

        public void UpdateEbxListItem(EbxAssetEntry ebx)
        {
			


			if (string.IsNullOrEmpty(ebx.Type))
			{
				//var t = AllSdkAssemblyTypes.FirstOrDefault(x => x.Name.ToLower().Contains(ebx.Filename.Replace("_runtime", "")));
				//if (t != null)
				//{
				//	var splitString = t.ToString().Split('.');
				//	// Expects final class name
				//	ebxList[ebx.Name].Type = splitString[splitString.Length - 1];
				//	return;
				//}
				//else
				//{
				//var e = GetEbx(ebx);
				//var root = e.RootObject;
				//ebxList[ebx.Name].Type = root.GetType().Name;
				//if (string.IsNullOrEmpty(ebxList[ebx.Name].Type))
				//{
				using (Stream ebxStream = GetEbxStream(ebx))
				{
					if (ebxStream != null && ebxStream.Length > 0)
					{
						//EbxReader_F21 ebxReader = new EbxReader_F21(ebxStream, true, ebx.Filename);
						if (ProfilesLibrary.IsFIFA19DataVersion())
						{
							EbxReader ebxReader = new EbxReader(ebxStream, true);
							EBX[ebx.Name].Type = ebxReader.RootType;
							EBX[ebx.Name].Guid = ebxReader.FileGuid;
						}
						else
						{
							EbxReaderV2 ebxReader = new EbxReaderV2(ebxStream, true);
							EBX[ebx.Name].Type = ebxReader.RootType;
							EBX[ebx.Name].Guid = ebxReader.FileGuid;
						}
						return;
					}
				}

				//}
                    //}
            }


			if (string.IsNullOrEmpty(ebx.Type))
			{
				EBX.TryRemove(ebx.Name, out _);
			}
		}

		

		public void DoEbxIndexing()
		{
			if (TypeLibrary.ExistingAssembly == null)
			{
				WriteToLog($"Unable to index data until SDK exists");
				return;
			}

			if (AllSdkAssemblyTypes == null)
				AllSdkAssemblyTypes = TypeLibrary.ExistingAssembly.GetTypes().ToList();

			ResourceManager.UseLastCasPath = true;

			var ebxListValues = EBX.Values.ToList();
            if (ProfilesLibrary.IsMadden21DataVersion()
                || ProfilesLibrary.IsFIFA21DataVersion()
                || ProfilesLibrary.IsFIFA20DataVersion()
                || ProfilesLibrary.IsFIFA19DataVersion()
                )
            {

				int ebxProgress = 0;

				var count = EbxItemsWithNoType.Count;
				if (count > 0)
				{
					WriteToLog($"Initial load - Indexing data - This will take some time");

					EbxItemsWithNoType.ForEach(x =>
					{
						UpdateEbxListItem(x);
						ebxProgress++;
						WriteToLog($"Initial load - Indexing data ({Math.Round(((double)ebxProgress / (double)count * 100.0), 1)}%)");
					});

					WriteToCache();
					WriteToLog("Initial load - Indexing complete");

				}

            }

			ResourceManager.UseLastCasPath = false;

		}

		public uint GetModifiedCount()
		{
			uint num = (uint)EBX.Values.Count((EbxAssetEntry entry) => entry.IsModified);
			uint num2 = (uint)RES.Values.Count((ResAssetEntry entry) => entry.IsModified);
			uint num3 = (uint)Chunks.Values.Count((ChunkAssetEntry entry) => entry.IsModified);
			uint num4 = 0u;
			foreach (ICustomAssetManager value in CustomAssetManagers.Values)
			{
				num4 = (uint)((int)num4 + value.EnumerateAssets(modifiedOnly: true).Count());
			}
			return num + num2 + num3 + num4;
		}

		public uint GetDirtyCount()
		{
			uint num = (uint)EBX.Values.Count((EbxAssetEntry entry) => entry.IsDirty);
			uint num2 = (uint)RES.Values.Count((ResAssetEntry entry) => entry.IsDirty);
			uint num3 = (uint)Chunks.Values.Count((ChunkAssetEntry entry) => entry.IsDirty);
			uint num4 = 0u;
			foreach (ICustomAssetManager value in CustomAssetManagers.Values)
			{
				num4 = (uint)((int)num4 + value.EnumerateAssets(modifiedOnly: true).Count((AssetEntry a) => a.IsDirty));
			}
			return num + num2 + num3 + num4;
		}

		public uint GetEbxCount(string ebxType)
		{
			return (uint)EBX.Values.Count((EbxAssetEntry entry) => entry.Type != null && entry.Type.Equals(ebxType));
		}

		public uint GetEbxCount()
		{
			return (uint)EBX.Count;
		}

		public uint GetResCount(uint resType)
		{
			return (uint)RES.Values.Count((ResAssetEntry entry) => entry.ResType == resType);
		}

		public uint GetEmbeddedCount(uint resType)
		{
			return (uint)EmbeddedFileEntries.Count();
		}

		public void Reset()
		{
			List<EbxAssetEntry> list = EBX.Values.ToList();
			List<ResAssetEntry> list2 = RES.Values.ToList();
			List<ChunkAssetEntry> list3 = Chunks.Values.ToList();
			foreach (EbxAssetEntry item in list)
			{
				RevertAsset(item, dataOnly: false, suppressOnModify: false);
			}
			foreach (ResAssetEntry item2 in list2)
			{
				RevertAsset(item2, dataOnly: false, suppressOnModify: false);
			}
			foreach (ChunkAssetEntry item3 in list3)
			{
				RevertAsset(item3, dataOnly: false, suppressOnModify: false);
			}
			foreach (ICustomAssetManager value in CustomAssetManagers.Values)
			{
				foreach (AssetEntry item4 in value.EnumerateAssets(modifiedOnly: true))
				{
					RevertAsset(item4, dataOnly: false, suppressOnModify: false);
				}
			}
			EmbeddedFileEntries = new List<EmbeddedFileEntry>();

			LegacyFileManager_M21.CleanUpChunks();

		}

		public void RevertAsset(AssetEntry entry, bool dataOnly = false, bool suppressOnModify = true)
		{
			if (!entry.IsModified)
			{
				return;
			}
			foreach (AssetEntry linkedAsset in entry.LinkedAssets)
			{
				RevertAsset(linkedAsset, dataOnly, suppressOnModify);
			}
			
			entry.ClearModifications();
			if (dataOnly)
			{
				return;
			}
			entry.LinkedAssets.Clear();
			entry.AddBundles.Clear();
			entry.RemBundles.Clear();
			//if (entry.IsAdded)
			//{
			//	if (entry is EbxAssetEntry)
			//	{
			//		EbxAssetEntry ebxAssetEntry = entry as EbxAssetEntry;
			//		EBX.TryRemove(ebxAssetEntry.Name, out _);
			//	}
			//	else if (entry is ResAssetEntry)
			//	{
			//		ResAssetEntry resAssetEntry = entry as ResAssetEntry;
			//		resRidList.Remove(resAssetEntry.ResRid);
			//		resList.Remove(resAssetEntry.Name);
			//	}
			//	else if (entry is ChunkAssetEntry)
			//	{
			//		ChunkAssetEntry chunkAssetEntry = entry as ChunkAssetEntry;
			//		//chunkList.Remove(chunkAssetEntry.Id);
			//		chunkList.TryRemove(chunkAssetEntry.Id, out _);
			//	}
			//}

			var m21LAM = GetLegacyAssetManager() as LegacyFileManager_M21;
			if (m21LAM != null)
			{
				m21LAM.RevertAsset(entry);
			}

			entry.IsDirty = false;
			if (!entry.IsAdded && !suppressOnModify)
			{
				entry.OnModified();
			}
		}

		public ICustomAssetManager GetLegacyAssetManager()
        {
			return CustomAssetManagers["legacy"];
        }

		public void AddEmbeddedFile(EmbeddedFileEntry entry)
        {
			if (EmbeddedFileEntries.Contains(entry))
				EmbeddedFileEntries.Remove(entry);

			EmbeddedFileEntries.Add(entry);
        }

		public void AddChunk(ChunkAssetEntry entry)
		{
            if (Chunks.ContainsKey(entry.Id))
                Chunks.TryRemove(entry.Id, out _);

			Chunks.TryAdd(entry.Id, entry);

			// ------------------------- SEPERATE LIST FOR BUNDLE ID ------------------------
			if (BundleChunks.ContainsKey((entry.Bundle, entry.Id)))
				BundleChunks.TryRemove((entry.Bundle, entry.Id), out _);

			BundleChunks.TryAdd((entry.Bundle, entry.Id), entry);
		}

		public Dictionary<Guid, List<ChunkAssetEntry>> ChunkListDuplicates = new Dictionary<Guid, List<ChunkAssetEntry>>();

		public void AddRes(ResAssetEntry entry)
		{
			entry.IsAdded = true;

			if (RES.ContainsKey(entry.Name.ToLower()))
				RES.Remove(entry.Name.ToLower());

			RES.Add(entry.Name.ToLower(), entry);

			if (resRidList.ContainsKey(entry.ResRid))
				resRidList.Remove(entry.ResRid);

			resRidList.Add(entry.ResRid, entry);
		}

		public bool AddEbx(EbxAssetEntry entry)
		{
			entry.IsAdded = true;
			//var intFnv1 = Fnv1.HashString(entry.Name);
			bool result = EBX.TryAdd(entry.Name.ToLower(), entry);
			//ebxGuidList.TryAdd(entry.Guid, entry);

			return result;
		}

		public BundleEntry AddBundle(string name, BundleType type, int sbIndex)
		{
			int num = bundles.FindIndex((BundleEntry be) => be.Name == name);
			if (num != -1)
			{
				return bundles[num];
			}
			BundleEntry bundleEntry = new BundleEntry();
			bundleEntry.Name = name;
			bundleEntry.SuperBundleId = sbIndex;
			bundleEntry.Type = type;
			bundleEntry.Added = true;
			bundles.Add(bundleEntry);
			return bundleEntry;
		}

		public SuperBundleEntry AddSuperBundle(string name)
		{
			int num = superBundles.FindIndex((SuperBundleEntry sbe) => sbe.Name.Equals(name));
			if (num != -1)
			{
				return superBundles[num];
			}
			SuperBundleEntry superBundleEntry = new SuperBundleEntry();
			superBundleEntry.Name = name;
			superBundleEntry.Added = true;
			superBundles.Add(superBundleEntry);
			return superBundleEntry;
		}

		public EbxAssetEntry AddEbx(string name, EbxAsset asset, params int[] bundles)
		{
			string key = name.ToLower();
			if (EBX.ContainsKey(key))
			{
				return EBX[key];
			}
			EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
			ebxAssetEntry.Name = name;
			ebxAssetEntry.Guid = asset.FileGuid;
			ebxAssetEntry.Type = asset.RootObject.GetType().Name;
			ebxAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
			using (EbxWriter ebxWriter = new EbxWriter(new MemoryStream()))
			{
				ebxWriter.WriteAsset(asset);
				((MemoryStream)ebxWriter.BaseStream).ToArray();
			}
			ebxAssetEntry.ModifiedEntry.DataObject = asset;
			ebxAssetEntry.ModifiedEntry.OriginalSize = 0L;
			ebxAssetEntry.ModifiedEntry.Sha1 = Sha1.Zero;
			ebxAssetEntry.ModifiedEntry.IsInline = false;
			ebxAssetEntry.IsDirty = true;
			ebxAssetEntry.IsAdded = true;
			//EBX.Add(key, ebxAssetEntry);
			EBX.TryAdd(ebxAssetEntry.Name, ebxAssetEntry);

			//ebxGuidList.TryAdd(ebxAssetEntry.Guid, ebxAssetEntry);
			return ebxAssetEntry;
		}

		public ResAssetEntry AddRes(string name, ResourceType resType, byte[] resMeta, byte[] buffer, params int[] bundles)
		{
			name = name.ToLower();
			if (RES.ContainsKey(name))
			{
				return RES[name];
			}
			ResAssetEntry resAssetEntry = new ResAssetEntry();
			resAssetEntry.Name = name;
			resAssetEntry.ResRid = Utils.GenerateResourceId();
			resAssetEntry.ResType = (uint)resType;
			resAssetEntry.ResMeta = resMeta;
			resAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
			resAssetEntry.ModifiedEntry.Data = Utils.CompressFile(buffer);
			resAssetEntry.ModifiedEntry.OriginalSize = buffer.Length;
			resAssetEntry.ModifiedEntry.Sha1 = GenerateSha1(resAssetEntry.ModifiedEntry.Data);
			resAssetEntry.ModifiedEntry.IsInline = false;
			resAssetEntry.ModifiedEntry.ResMeta = resAssetEntry.ResMeta;
			resAssetEntry.IsAdded = true;
			resAssetEntry.IsDirty = true;
			RES.Add(resAssetEntry.Name, resAssetEntry);
			resRidList.Add(resAssetEntry.ResRid, resAssetEntry);
			return resAssetEntry;
		}

		public Guid AddChunk(byte[] buffer, Guid? overrideGuid = null, Texture texture = null, params int[] bundles)
		{
			ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
			CompressionType compressionOverride = (ProfilesLibrary.DataVersion == 20170929) ? CompressionType.Oodle : CompressionType.Default;
			chunkAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
			chunkAssetEntry.ModifiedEntry.Data = ((texture != null) ? Utils.CompressTexture(buffer, texture, compressionOverride) : Utils.CompressFile(buffer, null, ResourceType.Invalid, compressionOverride));
			chunkAssetEntry.ModifiedEntry.Sha1 = GenerateSha1(chunkAssetEntry.ModifiedEntry.Data);
			chunkAssetEntry.ModifiedEntry.LogicalSize = (uint)buffer.Length;
			chunkAssetEntry.ModifiedEntry.FirstMip = -1;
			chunkAssetEntry.AddBundles.AddRange(bundles);
			if (texture != null)
			{
				chunkAssetEntry.ModifiedEntry.LogicalOffset = texture.LogicalOffset;
				chunkAssetEntry.ModifiedEntry.LogicalSize = texture.LogicalSize;
				chunkAssetEntry.ModifiedEntry.RangeStart = texture.RangeStart;
				chunkAssetEntry.ModifiedEntry.RangeEnd = texture.RangeEnd;
				chunkAssetEntry.ModifiedEntry.FirstMip = texture.FirstMip;
			}
			chunkAssetEntry.IsAdded = true;
			chunkAssetEntry.IsDirty = true;
			if (overrideGuid.HasValue)
			{
				chunkAssetEntry.Id = overrideGuid.Value;
			}
			else
			{
				byte[] array = Guid.NewGuid().ToByteArray();
				array[15] |= 1;
				chunkAssetEntry.Id = new Guid(array);
			}
			//chunkList.Add(chunkAssetEntry.Id, chunkAssetEntry);
			Chunks.TryAdd(chunkAssetEntry.Id, chunkAssetEntry);
			return chunkAssetEntry.Id;
		}

		public bool ModifyChunk((string bundleName, Guid chunkId) chunk
			, byte[] buffer
			, Texture texture = null
			, CompressionType compressionOverride = CompressionType.Default)
		{
			if (!BundleChunks.ContainsKey(chunk))
			{
				return false;
			}
			ChunkAssetEntry chunkAssetEntry = BundleChunks[chunk];
			// fifa 21 chunk is oodle
			// madden 21 chunk is oodle
			if ((ProfilesLibrary.IsFIFADataVersion()
				|| ProfilesLibrary.IsMadden21DataVersion()
				|| ProfilesLibrary.IsFIFA21DataVersion()
				)
				&& texture != null)
			{
				compressionOverride = CompressionType.Oodle;
			}

			if (chunkAssetEntry.ModifiedEntry == null)
			{
				chunkAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
			}
			chunkAssetEntry.ModifiedEntry.Data = ((texture != null) ? Utils.CompressTexture(buffer, texture, compressionOverride) : Utils.CompressFile(buffer, null, ResourceType.Invalid, compressionOverride));
			chunkAssetEntry.ModifiedEntry.Sha1 = GenerateSha1(chunkAssetEntry.ModifiedEntry.Data);
			chunkAssetEntry.ModifiedEntry.LogicalSize = (uint)buffer.Length;
			if (texture != null)
			{
				chunkAssetEntry.ModifiedEntry.LogicalOffset = texture.LogicalOffset;
				chunkAssetEntry.ModifiedEntry.LogicalSize = texture.LogicalSize;
				chunkAssetEntry.ModifiedEntry.RangeStart = texture.RangeStart;
				chunkAssetEntry.ModifiedEntry.RangeEnd = (uint)chunkAssetEntry.ModifiedEntry.Data.Length;
				chunkAssetEntry.ModifiedEntry.FirstMip = texture.FirstMip;
			}
			chunkAssetEntry.IsDirty = true;
			return true;
		}

		public bool ModifyChunk(Guid chunkId
			, byte[] buffer
			, Texture texture = null
			, CompressionType compressionOverride = CompressionType.Default)
		{
			if (!Chunks.ContainsKey(chunkId))
			{
				return false;
			}
			ChunkAssetEntry chunkAssetEntry = Chunks[chunkId];
			// fifa 21 chunk is oodle
			// madden 21 chunk is oodle
			if ((ProfilesLibrary.IsFIFADataVersion() 
				|| ProfilesLibrary.IsMadden21DataVersion()
				|| ProfilesLibrary.IsFIFA21DataVersion()
				) 
				&& texture != null)
			{
				compressionOverride = CompressionType.Oodle;
			}

			if (chunkAssetEntry.ModifiedEntry == null)
			{
				chunkAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
			}
			chunkAssetEntry.ModifiedEntry.Data = ((texture != null) ? Utils.CompressTexture(buffer, texture, compressionOverride) : Utils.CompressFile(buffer, null, ResourceType.Invalid, compressionOverride));
			chunkAssetEntry.ModifiedEntry.Sha1 = GenerateSha1(chunkAssetEntry.ModifiedEntry.Data);
			chunkAssetEntry.ModifiedEntry.LogicalSize = (uint)buffer.Length;
			if (texture != null)
			{
				chunkAssetEntry.ModifiedEntry.LogicalOffset = texture.LogicalOffset;
				chunkAssetEntry.ModifiedEntry.LogicalSize = texture.LogicalSize;
				chunkAssetEntry.ModifiedEntry.RangeStart = texture.RangeStart;
				chunkAssetEntry.ModifiedEntry.RangeEnd = (uint)chunkAssetEntry.ModifiedEntry.Data.Length;
				chunkAssetEntry.ModifiedEntry.FirstMip = texture.FirstMip;
			}
			chunkAssetEntry.IsDirty = true;
			return true;
		}

		public void ModifyRes(ulong resRid, byte[] buffer, byte[] meta = null)
		{
			if (resRidList.ContainsKey(resRid))
			{
				ResAssetEntry resAssetEntry = resRidList[resRid];
				//CompressionType compressionOverride = (ProfilesLibrary.DataVersion == 20170929) ? CompressionType.Oodle : CompressionType.Default;
				CompressionType compressionOverride = CompressionType.Default;
				if (ProfilesLibrary.IsMadden21DataVersion()) compressionOverride = CompressionType.Oodle;
				if (resAssetEntry.ModifiedEntry == null)
				{
					resAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
				}
				resAssetEntry.ModifiedEntry.Data = Utils.CompressFile(buffer, null, (ResourceType)resAssetEntry.ResType, compressionOverride);
				resAssetEntry.ModifiedEntry.OriginalSize = buffer.Length;
				resAssetEntry.ModifiedEntry.Sha1 = GenerateSha1(resAssetEntry.ModifiedEntry.Data);
				if (meta != null)
				{
					resAssetEntry.ModifiedEntry.ResMeta = meta;
				}
				resAssetEntry.IsDirty = true;
			}
		}

		public void ModifyRes(ulong resRid, Resource resource)
		{
			if (resRidList.ContainsKey(resRid))
			{
				ResAssetEntry resAssetEntry = resRidList[resRid];
				if (resAssetEntry.ModifiedEntry == null)
				{
					resAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
				}
				resAssetEntry.ModifiedEntry.DataObject = resource.Save();
				resAssetEntry.IsDirty = true;
			}
		}

		public void ModifyRes(string resName, byte[] buffer, byte[] meta = null, CompressionType compressionOverride = CompressionType.Default)
		{
			if (RES.ContainsKey(resName))
			{
				ResAssetEntry resAssetEntry = RES[resName];
				// Madden 21 Res is oodle
				// FIFA 21 Res for meshes is oodle
				compressionOverride = (ProfilesLibrary.DataVersion == 20170929 
					|| ProfilesLibrary.IsMadden21DataVersion()
					|| ProfilesLibrary.IsFIFA21DataVersion()
					) ? CompressionType.Oodle : CompressionType.Default;

				resAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
				resAssetEntry.ModifiedEntry.Data = Utils.CompressFile(buffer, null, (ResourceType)resAssetEntry.ResType, compressionOverride);
				resAssetEntry.ModifiedEntry.OriginalSize = buffer.Length;
				resAssetEntry.ModifiedEntry.Sha1 = GenerateSha1(resAssetEntry.ModifiedEntry.Data);
				if (meta != null)
				{
					resAssetEntry.ModifiedEntry.ResMeta = meta;
				}
				resAssetEntry.IsDirty = true;
			}
		}

		public void ModifyRes(string resName, Resource resource, byte[] meta = null)
		{
			if (RES.ContainsKey(resName))
			{
				ResAssetEntry resAssetEntry = RES[resName];
				if (resAssetEntry.ModifiedEntry == null)
				{
					resAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
				}
				resAssetEntry.ModifiedEntry.DataObject = resource.Save();
				if (meta != null)
				{
					resAssetEntry.ModifiedEntry.ResMeta = meta;
				}
				resAssetEntry.IsDirty = true;
			}
		}

		public void ModifyEbx(string name, EbxAsset asset)
		{
			name = name.ToLower();
			if (EBX.ContainsKey(name))
			{
				EbxAssetEntry ebxAssetEntry = EBX[name];
				if (ebxAssetEntry.ModifiedEntry == null)
				{
					ebxAssetEntry.ModifiedEntry = new ModifiedAssetEntry();
				}
				ebxAssetEntry.ModifiedEntry.DataObject = asset;
				ebxAssetEntry.ModifiedEntry.OriginalSize = 0L;
				ebxAssetEntry.ModifiedEntry.Sha1 = Sha1.Zero;
				ebxAssetEntry.ModifiedEntry.IsTransientModified = asset.TransientEdit;
				ebxAssetEntry.ModifiedEntry.DependentAssets.Clear();
				ebxAssetEntry.ModifiedEntry.DependentAssets.AddRange(asset.Dependencies);
				ebxAssetEntry.IsDirty = true;
				ebxAssetEntry.IsBinary = false;
			}
		}

		public void ModifyEbxBinary(string name, byte[] data)
		{
			name = name.ToLower();
			if (EBX.ContainsKey(name))
			{
				var ebxEntry = EBX[name];
				var patch = ebxEntry.IsInPatch || ebxEntry.ExtraData.IsPatch;
				var ebxAsset = GetEbxAssetFromStream(new MemoryStream(data), patch);
				ModifyEbx(name, ebxAsset);
			}
		}



		public void ModifyLegacyAsset(string name, byte[] data, bool rebuildChunk = true)
		{
			if (CustomAssetManagers.ContainsKey("legacy"))
			{
				CustomAssetManagers["legacy"].ModifyAsset(name, data, rebuildChunk);
			}
		}

		public void ModifyLegacyAssets(Dictionary<string, byte[]> data, bool rebuildChunk = true)
		{
			var lm = CustomAssetManagers["legacy"] as LegacyFileManager_M21;
			if(lm != null)
            {
				lm.ModifyAssets(data, true);
            }
		}

		public void ModifyCustomAsset(string type, string name, byte[] data)
		{
			if (CustomAssetManagers.ContainsKey(type))
			{
				CustomAssetManagers[type].ModifyAsset(name, data);
			}
		}

		public IEnumerable<SuperBundleEntry> EnumerateSuperBundles(bool modifiedOnly = false)
		{
			foreach (SuperBundleEntry superBundle in superBundles)
			{
				if (!modifiedOnly || superBundle.Added)
				{
					yield return superBundle;
				}
			}
		}

		public IEnumerable<BundleEntry> EnumerateBundles(BundleType type = BundleType.None, bool modifiedOnly = false)
		{
			foreach (BundleEntry bundle in bundles)
			{
				if ((type == BundleType.None || bundle.Type == type) && (!modifiedOnly || bundle.Added))
				{
					yield return bundle;
				}
			}
		}

		public IEnumerable<EbxAssetEntry> EnumerateEbx(BundleEntry bentry)
		{
			int num = bundles.IndexOf(bentry);
			return EnumerateEbx("", false, false, true, num);
		}

		public IEnumerable<EbxAssetEntry> EnumerateEbx(string type = "", bool modifiedOnly = false, bool includeLinked = false, bool includeHidden = true, string bundleSubPath = "")
		{
			List<int> list = new List<int>();
			if (bundleSubPath != "")
			{
				bundleSubPath = bundleSubPath.ToLower();
				for (int i = 0; i < bundles.Count; i++)
				{
					if (bundles[i].Name.Equals(bundleSubPath) || bundles[i].Name.StartsWith(bundleSubPath + "/"))
					{
						list.Add(i);
					}
				}
			}

			return EnumerateEbx(type, modifiedOnly, includeLinked, includeHidden, list.ToArray());
		}

		protected IEnumerable<EbxAssetEntry> EnumerateEbx(string type, bool modifiedOnly, bool includeLinked, bool includeHidden, params int[] bundles)
		{
			foreach (EbxAssetEntry value in EBX.Values)
			{
				if ((!modifiedOnly || (value.IsModified && (!value.IsIndirectlyModified || includeLinked || value.IsDirectlyModified))) && (!(type != "") || (value.Type != null && TypeLibrary.IsSubClassOf(value.Type, type))))
				{
					if (bundles.Length != 0)
					{
						bool flag = false;
						foreach (int item in bundles)
						{
							if (value.Bundles.Contains(item))
							{
								flag = true;
								break;
							}
							if (value.AddBundles.Contains(item))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							continue;
						}
					}
					yield return value;
				}
			}
		}

		public IEnumerable<ResAssetEntry> EnumerateRes(BundleEntry bentry)
		{
			int num = bundles.IndexOf(bentry);
			if (num != -1)
			{
				foreach (ResAssetEntry item in EnumerateRes(0u, false, num))
				{
					yield return item;
				}
			}
		}

		public IEnumerable<ResAssetEntry> EnumerateRes(uint resType = 0u, bool modifiedOnly = false, string bundleSubPath = "")
		{
			List<int> list = new List<int>();
			if (bundleSubPath != "")
			{
				bundleSubPath = bundleSubPath.ToLower();
				for (int i = 0; i < bundles.Count; i++)
				{
					if (bundles[i].Name.Equals(bundleSubPath) || bundles[i].Name.StartsWith(bundleSubPath + "/"))
					{
						list.Add(i);
					}
				}
				if (list.Count == 0)
				{
					yield break;
				}
			}
			foreach (ResAssetEntry item in EnumerateRes(resType, modifiedOnly, list.ToArray()))
			{
				yield return item;
			}
		}

		protected IEnumerable<ResAssetEntry> EnumerateRes(uint resType, bool modifiedOnly, params int[] bundles)
		{
			foreach (ResAssetEntry value in RES.Values)
			{
				if ((!modifiedOnly || value.IsDirectlyModified) && (resType == 0 || value.ResType == resType))
				{
					if (bundles.Length != 0)
					{
						bool flag = false;
						foreach (int item in bundles)
						{
							if (value.Bundles.Contains(item))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							continue;
						}
					}
					yield return value;
				}
			}
		}

		public IEnumerable<ChunkAssetEntry> EnumerateChunks(BundleEntry bentry)
		{
			int bindex = bundles.IndexOf(bentry);
			if (bindex != -1)
			{
				foreach (ChunkAssetEntry value in Chunks.Values.OrderBy(x => x.ExtraData != null ? x.ExtraData.CasPath : string.Empty))
				{
					if (value.Bundles.Contains(bindex))
					{
						yield return value;
					}
				}
			}
		}

		public IEnumerable<ChunkAssetEntry> EnumerateChunks(bool modifiedOnly = false)
		{
			foreach (ChunkAssetEntry value in Chunks.Values.OrderBy(x=> x.ExtraData != null ? x.ExtraData.CasPath : string.Empty))
			{
				if (!modifiedOnly || value.IsDirectlyModified)
				{
					yield return value;
				}
			}
		}

		public IEnumerable<AssetEntry> EnumerateCustomAssets(string type, bool modifiedOnly = false)
		{
			if (CustomAssetManagers.ContainsKey(type))
			{
				foreach (AssetEntry item in CustomAssetManagers[type].EnumerateAssets(modifiedOnly))
				{
					yield return item;
				}
			}
		}

		public int GetSuperBundleId(SuperBundleEntry sbentry)
		{
			return superBundles.FindIndex((SuperBundleEntry sbe) => sbe.Name.Equals(sbentry.Name));
		}

		public int GetSuperBundleId(string sbname)
		{
			return superBundles.FindIndex((SuperBundleEntry sbe) => sbe.Name.Equals(sbname, StringComparison.OrdinalIgnoreCase));
		}

		public SuperBundleEntry GetSuperBundle(int id)
		{
			if (id >= superBundles.Count)
			{
				return null;
			}
			return superBundles[id];
		}

		public int GetBundleId(BundleEntry bentry)
		{
			return bundles.FindIndex((BundleEntry be) => be.Name.Equals(bentry.Name));
		}

		public int GetBundleId(string name)
		{
			return bundles.FindIndex((BundleEntry be) => be.Name.Equals(name));
		}

		public BundleEntry GetBundleEntry(int bundleId)
		{
			if (bundleId >= bundles.Count)
			{
				return null;
			}
			return bundles[bundleId];
		}

		public AssetEntry GetCustomAssetEntry(string type, string key)
		{
			if (!CustomAssetManagers.ContainsKey(type))
			{
				return null;
			}
			return CustomAssetManagers[type].GetAssetEntry(key);
		}

		public T GetCustomAssetEntry<T>(string type, string key) where T : AssetEntry
		{
			return (T)GetCustomAssetEntry(type, key);
		}

		//public EbxAssetEntry GetEbxEntry(Guid ebxGuid)
		//{
		//	if (!ebxGuidList.ContainsKey(ebxGuid))
		//	{
		//		return null;
		//	}
		//	return ebxGuidList[ebxGuid];
		//}

		public EbxAssetEntry GetEbxEntry(string name)
		{
			name = name.ToLower();
			if (!EBX.ContainsKey(name))
			{
				return null;
			}
			return EBX[name];
		}

		public EbxAssetEntry GetEbxEntry(Guid id)
		{
			var ebxGuids = EBX.Values.Where(x => x.Guid != null);
			return ebxGuids.FirstOrDefault(x => x.Guid == id);
		}

		public ResAssetEntry GetResEntry(ulong resRid)
		{
			if (!resRidList.ContainsKey(resRid))
			{
				return null;
			}
			return resRidList[resRid];
		}

		public ResAssetEntry GetResEntry(string name)
		{
			name = name.ToLower();
			if (!RES.ContainsKey(name))
			{
				return null;
			}
			return RES[name];
		}

		public ChunkAssetEntry GetChunkEntry(Guid id, string bundle)
		{
			if (!BundleChunks.ContainsKey((bundle, id)))
			{
				//if (GetChunkEntry(id) != null)
				//	return GetChunkEntry(id);

				return null;
			}
			var entry = BundleChunks[(bundle, id)];
			return entry;
		}

		public ChunkAssetEntry GetChunkEntry(Guid id)
		{
			if (!Chunks.ContainsKey(id))
			{
				return null;
			}
			var entry = Chunks[id];
			return entry;
		}

		public Stream GetCompressedChunk(ChunkAssetEntry entry)
		{
			throw new NotImplementedException("Not finished!!");
			//if (entry == null)
			//{
			//	throw new ArgumentNullException("entry");
			//}
			//return GetAsset(entry, decompress: false);
		}

		public List<ChunkAssetEntry> GetChunkEntries(Guid id)
		{
			var allChunks = ChunkListDuplicates.Where(x => x.Key == id).SelectMany(x => x.Value).ToList();
			//if(chunkList.ContainsKey(id))
			//	allChunks.Add(chunkList[id]);
			return allChunks;

				
		}

		public Stream GetCustomAsset(string type, AssetEntry entry)
		{
			if (!CustomAssetManagers.ContainsKey(type))
			{
				return null;
			}
			return CustomAssetManagers[type].GetAsset(entry);
		}

		public EbxAsset GetEbx(EbxAssetEntry entry, bool getModified = true)
		{
			Stream assetStream = null;
			if (getModified)
			{
				if (entry != null && entry.ModifiedEntry != null && entry.ModifiedEntry.DataObject != null)
				{
					if (entry.IsBinary || entry.ModifiedEntry.Data != null)
					{
						assetStream = new MemoryStream(entry.ModifiedEntry.Data);
					}
					else
					{
						return entry.ModifiedEntry.DataObject as EbxAsset;
					}
				}
			}

			if(assetStream == null)
			{
				assetStream = GetAsset(entry);
				if (assetStream == null)
				{
					return null;
				}
			}
            bool inPatched = false;
			if ( (ProfilesLibrary.IsFIFADataVersion() 
				|| ProfilesLibrary.IsMadden21DataVersion() 
				|| ProfilesLibrary.IsFIFA21DataVersion()
				)
				&& entry.ExtraData.CasPath.StartsWith("native_patch"))
			{
				inPatched = true;
			}

            return GetEbxAssetFromStream(assetStream, inPatched);
        }
		public async Task<EbxAsset> GetEbxAsync(EbxAssetEntry entry, bool getModified = true)
		{
			return await Task.Run(() =>
			{
				return GetEbx(entry, getModified);
			});

		}


			public EbxAsset GetEbxAssetFromStream(Stream asset, bool inPatched = true)
        //public EbxAsset GetEbxAssetFromStream(Stream asset, bool inPatched = false)
        {
			EbxReader ebxReader = null;
			if (ProfilesLibrary.IsFIFA21DataVersion())
			{
                //ebxReader = new EbxReader_F21(asset, inPatched);
                //ebxReader = new EbxReaderV2(asset, inPatched);
                ebxReader = new EbxReaderV3(asset, inPatched);

            }
			else if (ProfilesLibrary.IsMadden21DataVersion())
			{
				//ebxReader = new EbxReader_F21(asset, inPatched);
				//ebxReader = new EbxReaderV2(asset, inPatched);
				ebxReader = new EbxReaderV3(asset, inPatched);

			}
			else if (ProfilesLibrary.DataVersion == 20181207
				|| ProfilesLibrary.IsFIFA20DataVersion()
				|| ProfilesLibrary.DataVersion == 20190905)
			{
				ebxReader = new EbxReaderV2(asset, inPatched);
			}
            else
            {
                ebxReader = new EbxReader(asset);
            }

            return ebxReader.ReadAsset();
		}

		/// <summary>
		/// Only gathers the ORIGINAL/VANILLA stream from the system. DO NOT USE
		/// </summary>
		/// <param name="entry"></param>
		/// <returns></returns>
		public Stream GetEbxStream(EbxAssetEntry entry)
		{
			return GetAsset(entry);
		}

		public Stream GetRes(ResAssetEntry entry)
		{
			return GetAsset(entry);
		}

		public async Task<Stream> GetResAsync(ResAssetEntry entry)
		{
			return await Task.Run(() =>
			{
				return GetRes(entry);
			});
		}

		public T GetResAs<T>(ResAssetEntry entry) where T : Resource, new()
		{
			using (NativeReader reader = new NativeReader(GetAsset(entry)))
			{
				ModifiedResource modifiedData = null;
				if (entry.ModifiedEntry != null && entry.ModifiedEntry.DataObject != null)
				{
					modifiedData = (entry.ModifiedEntry.DataObject as ModifiedResource);
				}
				T val = new T();
				val.Read(reader, this, entry, modifiedData);
				return val;
			}
		}

		public Stream GetChunk(ChunkAssetEntry entry)
		{
			return GetAsset(entry);
		}

		public byte[] GetChunkData(ChunkAssetEntry entry)
		{
			return ((MemoryStream)GetAsset(entry)).ToArray();
		}

		private Stream GetAsset(AssetEntry entry)
		{
			if(entry == null)
            {
				throw new Exception("Failed to find Asset Entry");
            }

			if (entry.ModifiedEntry != null && entry.ModifiedEntry.Data != null)
			{
				return rm.GetResourceData(entry.ModifiedEntry.Data);
			}
			switch (entry.Location)
			{
			case AssetDataLocation.Cas:
				if (entry.ExtraData == null)
				{
					return rm.GetResourceData(entry.Sha1);
				}
				return rm.GetResourceData(entry.ExtraData.BaseSha1, entry.ExtraData.DeltaSha1);
			case AssetDataLocation.SuperBundle:
				return rm.GetResourceData((entry.ExtraData.IsPatch ? "native_patch/" : "native_data/") + superBundles[entry.ExtraData.SuperBundleId].Name + ".sb", entry.ExtraData.DataOffset, entry.Size);
			case AssetDataLocation.Cache:
				return rm.GetResourceData(entry.ExtraData.DataOffset, entry.Size);
			case AssetDataLocation.CasNonIndexed:
				return rm.GetResourceData(entry.ExtraData.CasPath, entry.ExtraData.DataOffset, entry.Size, entry);
			default:
				return null;
			}
		}

		public void ProcessBundleEbx(DbObject sb, int bundleId, BinarySbDataHelper helper)
		{
			if (sb.GetValue<DbObject>("ebx") != null)
			{
				foreach (DbObject item in sb.GetValue<DbObject>("ebx"))
				{
					EbxAssetEntry ebxAssetEntry = AddEbx(item, ProfilesLibrary.IsMadden21DataVersion());
					if (ebxAssetEntry.Sha1 != item.GetValue<Sha1>("sha1") && item.GetValue("casPatchType", 0) != 0)
					{
						ebxAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
						ebxAssetEntry.Size = item.GetValue("size", 0L);
						ebxAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
						ebxAssetEntry.IsInline = item.HasValue("idata");

					}

					if (item.GetValue("cache", defaultValue: false) && ebxAssetEntry.Location != AssetDataLocation.Cache)
					{
						helper.RemoveEbxData(ebxAssetEntry.Name);
					}

					if (item.HasValue("SBFileLocation"))
					{
						ebxAssetEntry.SBFileLocation = item.GetValue<string>("SBFileLocation");
					}

					if (item.HasValue("TOCFileLocation"))
					{
						ebxAssetEntry.TOCFileLocation = item.GetValue<string>("TOCFileLocation");
					}

					if (item.HasValue("SB_CAS_Offset_Position"))
					{
						ebxAssetEntry.SB_CAS_Offset_Position = item.GetValue<int>("SB_CAS_Offset_Position");
					}

					if (item.HasValue("SB_CAS_Size_Position"))
					{
						ebxAssetEntry.SB_CAS_Size_Position = item.GetValue<int>("SB_CAS_Size_Position");
					}

					if(item.HasValue("SB_OriginalSize_Position"))
						ebxAssetEntry.SB_OriginalSize_Position = item.GetValue<int>("SB_OriginalSize_Position");

					if (item.HasValue("SB_Sha1_Position"))
						ebxAssetEntry.SB_Sha1_Position = item.GetValue<int>("SB_Sha1_Position");

					if(item.HasValue("ParentBundleOffset"))
						ebxAssetEntry.ParentBundleOffset = item.GetValue<int>("ParentBundleOffset");

					if (item.HasValue("ParentBundleSize"))
						ebxAssetEntry.ParentBundleSize = item.GetValue<int>("ParentBundleSize");

					ebxAssetEntry.Bundles.Add(bundleId);

					if (item.HasValue("Bundle"))
					{
						ebxAssetEntry.Bundle = item.GetValue<string>("Bundle");
					}
					else
					{
						ebxAssetEntry.Bundle = AssetManager.Instance.bundles[bundleId].Name;
					}
				}
			}
		}

		public void ProcessBundleRes(DbObject sb, int bundleId, BinarySbDataHelper helper)
		{
			if (sb.GetValue<DbObject>("res") != null)
			{
				foreach (DbObject item in sb.GetValue<DbObject>("res"))
				{
					if (!ProfilesLibrary.IsResTypeIgnored((ResourceType)item.GetValue("resType", 0L)))
					{
						ResAssetEntry resAssetEntry = AddRes(item);
						if (resAssetEntry.Sha1 != item.GetValue<Sha1>("sha1") && item.GetValue("casPatchType", 0) != 0)
						{
							resRidList.Remove(resAssetEntry.ResRid);
							resAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
							resAssetEntry.Size = item.GetValue("size", 0L);
							//resAssetEntry.ResRid = (ulong)item.GetValue("resRid", 0L);
							resAssetEntry.ResRid = item.GetValue("resRid", 0UL);
							resAssetEntry.ResMeta = item.GetValue<byte[]>("resMeta");
							resAssetEntry.IsInline = item.HasValue("idata");
							resAssetEntry.OriginalSize = item.GetValue("originalSize", 0L);
							resRidList.Add(resAssetEntry.ResRid, resAssetEntry);
						}
						if (item.GetValue("cache", defaultValue: false) && resAssetEntry.Location != AssetDataLocation.Cache)
						{
							helper.RemoveResData(resAssetEntry.Name);
						}


						if (item.HasValue("SBFileLocation"))
						{
							resAssetEntry.SBFileLocation = item.GetValue<string>("SBFileLocation");
						}

						if (item.HasValue("TOCFileLocation"))
						{
							resAssetEntry.TOCFileLocation = item.GetValue<string>("TOCFileLocation");
						}

						if (item.HasValue("SB_CAS_Offset_Position"))
						{
							resAssetEntry.SB_CAS_Offset_Position = item.GetValue<int>("SB_CAS_Offset_Position");
						}

						if (item.HasValue("SB_CAS_Size_Position"))
						{
							resAssetEntry.SB_CAS_Size_Position = item.GetValue<int>("SB_CAS_Size_Position");
						}

						if (item.HasValue("SB_OriginalSize_Position"))
							resAssetEntry.SB_OriginalSize_Position = item.GetValue<int>("SB_OriginalSize_Position");

						if (item.HasValue("SB_Sha1_Position"))
							resAssetEntry.SB_Sha1_Position = item.GetValue<int>("SB_Sha1_Position");

						if (item.HasValue("ParentBundleOffset"))
							resAssetEntry.ParentBundleOffset = item.GetValue<int>("ParentBundleOffset");

						if (item.HasValue("ParentBundleSize"))
							resAssetEntry.ParentBundleSize = item.GetValue<int>("ParentBundleSize");

						resAssetEntry.Bundles.Add(bundleId);

						if (item.HasValue("Bundle"))
						{
							resAssetEntry.Bundle = item.GetValue<string>("Bundle");
						}
						else
						{
							resAssetEntry.Bundle = AssetManager.Instance.bundles[bundleId].Name;
						}
					}
				}
			}
		}

		public void ProcessBundleChunks(DbObject sb, int bundleId, BinarySbDataHelper helper)
		{
			if (sb.GetValue<DbObject>("chunks") != null)
			{
				foreach (DbObject item in sb.GetValue<DbObject>("chunks"))
				{

					// colts uniform
					if (item.GetValue<Guid>("name").ToString() == "e35237c0-ebbe-bc31-1504-aaf3eb947c9b")
					{

					}


					if (item.GetValue<Guid>("name").ToString() == "56aeabb4-505a-58f4-77a6-f3b3c5fda185")
					{

					}

					var otherBundles = new List<int>();
					// last bundles 
                    if (Chunks.ContainsKey(item.GetValue<Guid>("name")))
					{
						otherBundles.AddRange(Chunks[item.GetValue<Guid>("name")].Bundles);
                    }

					item.SetValue("bundleIndex", bundleId);
					ChunkAssetEntry chunkAssetEntry = AddChunk(item, ProfilesLibrary.IsMadden21DataVersion());
					
					//if (item.GetValue("cache", defaultValue: false) && chunkAssetEntry.Location != AssetDataLocation.Cache)
					//{
					//	helper.RemoveChunkData(chunkAssetEntry.Id.ToString());
					//}
					if (chunkAssetEntry.Size == 0L)
					{
						chunkAssetEntry.Size = item.GetValue("size", 0L);
						chunkAssetEntry.LogicalOffset = item.GetValue("logicalOffset", 0u);
						chunkAssetEntry.LogicalSize = item.GetValue("logicalSize", 0u);
						chunkAssetEntry.RangeStart = item.GetValue("rangeStart", 0u);
						chunkAssetEntry.RangeEnd = item.GetValue("rangeEnd", 0u);
						chunkAssetEntry.BundledSize = item.GetValue("bundledSize", 0u);
						chunkAssetEntry.IsInline = item.HasValue("idata");
					}
					if (item.HasValue("SBFileLocation"))
					{
						chunkAssetEntry.SBFileLocation = item.GetValue<string>("SBFileLocation");
					}

					if (item.HasValue("TOCFileLocation"))
					{
						chunkAssetEntry.TOCFileLocation = item.GetValue<string>("TOCFileLocation");
					}

					if (item.HasValue("SB_CAS_Offset_Position"))
					{
						chunkAssetEntry.SB_CAS_Offset_Position = item.GetValue<int>("SB_CAS_Offset_Position");
					}

					if (item.HasValue("SB_CAS_Size_Position"))
					{
						chunkAssetEntry.SB_CAS_Size_Position = item.GetValue<int>("SB_CAS_Size_Position");
					}

					if (item.HasValue("SB_OriginalSize_Position"))
						chunkAssetEntry.SB_OriginalSize_Position = item.GetValue<int>("SB_OriginalSize_Position");

					if (item.HasValue("SB_Sha1_Position"))
						chunkAssetEntry.SB_Sha1_Position = item.GetValue<int>("SB_Sha1_Position");

					if (item.HasValue("ParentBundleOffset"))
						chunkAssetEntry.ParentBundleOffset = item.GetValue<int>("ParentBundleOffset");

					if (item.HasValue("ParentBundleSize"))
						chunkAssetEntry.ParentBundleSize = item.GetValue<int>("ParentBundleSize");

					chunkAssetEntry.Bundles.AddRange(otherBundles);
					chunkAssetEntry.Bundles.Add(bundleId);
					chunkAssetEntry.Bundles = chunkAssetEntry.Bundles.Distinct().ToList();
					if (item.HasValue("Bundle") && !string.IsNullOrEmpty(item.GetValue<string>("Bundle")))
					{
						chunkAssetEntry.Bundle = item.GetValue<string>("Bundle");
					}
					else
					{
						chunkAssetEntry.Bundle = AssetManager.Instance.bundles[bundleId].Name;
					}

					Chunks[chunkAssetEntry.Id] = chunkAssetEntry;

				}
			}
		}

		public DbObject ProcessTocChunks(string superBundleName, BinarySbDataHelper helper, bool isBase = false)
		{
			string text = fs.ResolvePath(superBundleName);
			if (text == "")
			{
				return null;
			}
			DbObject dbObject = null;
			using (DbReader dbReader = new DbReader(new FileStream(text, FileMode.Open, FileAccess.Read), fs.CreateDeobfuscator()))
			{
				dbObject = dbReader.ReadDbObject();
			}
			if (isBase && ProfilesLibrary.DataVersion != 20141118 && ProfilesLibrary.DataVersion != 20141117 && ProfilesLibrary.DataVersion != 20151103 && ProfilesLibrary.DataVersion != 20150223 && ProfilesLibrary.DataVersion != 20131115 && ProfilesLibrary.DataVersion != 20140225)
			{
				return dbObject;
			}
			if (dbObject.GetValue<DbObject>("chunks") != null)
			{
				foreach (DbObject item in dbObject.GetValue<DbObject>("chunks"))
				{
					Guid value = item.GetValue<Guid>("id");
					ChunkAssetEntry chunkAssetEntry = null;
					if (Chunks.ContainsKey(value))
					{
						chunkAssetEntry = Chunks[value];
						//chunkList.Remove(value);
						Chunks.TryRemove(value, out _);
						helper.RemoveChunkData(chunkAssetEntry.Id.ToString());
					}
					else
					{
						chunkAssetEntry = new ChunkAssetEntry();
					}
					chunkAssetEntry.Id = item.GetValue<Guid>("id");
					chunkAssetEntry.Sha1 = item.GetValue<Sha1>("sha1");
					if (item.GetValue("size", 0L) != 0L)
					{
						chunkAssetEntry.Location = AssetDataLocation.SuperBundle;
						chunkAssetEntry.Size = item.GetValue("size", 0L);
						chunkAssetEntry.ExtraData = new AssetExtraData();
						chunkAssetEntry.ExtraData.DataOffset = (uint)item.GetValue("offset", 0L);
						chunkAssetEntry.ExtraData.SuperBundleId = superBundles.Count - 1;
						chunkAssetEntry.ExtraData.IsPatch = superBundleName.StartsWith("native_patch");
					}

					chunkAssetEntry.SB_LogicalOffset_Position = item.GetValue("SB_LogicalOffset_Position", 0u);
					chunkAssetEntry.SB_LogicalSize_Position = item.GetValue("SB_LogicalSize_Position", 0u);
					Chunks.TryAdd(chunkAssetEntry.Id, chunkAssetEntry);
					//chunkList.Add(chunkAssetEntry.Id, chunkAssetEntry);
				}
				return dbObject;
			}
			return dbObject;
		}

		private EbxAssetEntry AddEbx(DbObject ebx, bool returnExisting = false)
		{
			string text = ebx.GetValue<string>("name").ToLower();
			if (EBX.ContainsKey(text))
			{
				//EBX.Remove(text);
				if(returnExisting)
					return EBX[text];
				else
					EBX.TryRemove(text, out _);
				//return ebxList[text];
			}
			EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
			ebxAssetEntry.Name = text;
			ebxAssetEntry.Sha1 = ebx.GetValue<Sha1>("sha1");
			ebxAssetEntry.BaseSha1 = rm.GetBaseSha1(ebxAssetEntry.Sha1);
			ebxAssetEntry.Size = ebx.GetValue("size", 0L);
			ebxAssetEntry.OriginalSize = ebx.GetValue("originalSize", 0L);
			ebxAssetEntry.IsInline = ebx.HasValue("idata");
			ebxAssetEntry.Location = AssetDataLocation.Cas;
			if (ebx.HasValue("cas"))
			{
				ebxAssetEntry.Location = AssetDataLocation.CasNonIndexed;
				ebxAssetEntry.ExtraData = new AssetExtraData();
				ebxAssetEntry.ExtraData.DataOffset = (uint)ebx.GetValue("offset", 0L);
				ebxAssetEntry.ExtraData.CasPath = (ebx.HasValue("catalog") ? fs.GetFilePath(ebx.GetValue("catalog", 0), ebx.GetValue("cas", 0), ebx.HasValue("patch")) : fs.GetFilePath(ebx.GetValue("cas", 0)));
				ebxAssetEntry.ExtraData.IsPatch = ebx.HasValue("patch") ? ebx.GetValue<bool>("patch") : false;
			}
			else if (ebx.GetValue("sb", defaultValue: false))
			{
				ebxAssetEntry.Location = AssetDataLocation.SuperBundle;
				ebxAssetEntry.ExtraData = new AssetExtraData();
				ebxAssetEntry.ExtraData.DataOffset = (uint)ebx.GetValue("offset", 0L);
				ebxAssetEntry.ExtraData.SuperBundleId = superBundles.Count - 1;
			}
			else if (ebx.GetValue("cache", defaultValue: false))
			{
				ebxAssetEntry.Location = AssetDataLocation.Cache;
				ebxAssetEntry.ExtraData = new AssetExtraData();
				//ebxAssetEntry.ExtraData.DataOffset = 3735928559L;
			}
			else if (ebx.GetValue("casPatchType", 0) == 2)
			{
				ebxAssetEntry.ExtraData = new AssetExtraData();
				ebxAssetEntry.ExtraData.BaseSha1 = ebx.GetValue<Sha1>("baseSha1");
				ebxAssetEntry.ExtraData.DeltaSha1 = ebx.GetValue<Sha1>("deltaSha1");
			}

			ebxAssetEntry.SBFileLocation = ebx.GetValue<string>("SBFileLocation");
			ebxAssetEntry.TOCFileLocation = ebx.GetValue<string>("TOCFileLocation");
			ebxAssetEntry.SB_CAS_Offset_Position = ebx.GetValue<int>("SB_CAS_Offset_Position");
			ebxAssetEntry.SB_CAS_Size_Position = ebx.GetValue<int>("SB_CAS_Size_Position");


			//EBX.Add(text, ebxAssetEntry);
				EBX.TryAdd(text, ebxAssetEntry);
			return ebxAssetEntry;
		}

		private ResAssetEntry AddRes(DbObject res, bool returnExisting = false)
		{
			string value = res.GetValue<string>("name");
			if (RES.ContainsKey(value))
			{
				if(returnExisting)
					return RES[value];

				RES.Remove(value);
				//return resList[value];
			}
			ResAssetEntry resAssetEntry = new ResAssetEntry();
			resAssetEntry.Name = value;
			resAssetEntry.Sha1 = res.GetValue<Sha1>("sha1");
			resAssetEntry.BaseSha1 = rm.GetBaseSha1(resAssetEntry.Sha1);
			resAssetEntry.Size = res.GetValue("size", 0L);
			resAssetEntry.OriginalSize = res.GetValue("originalSize", 0L);
			var rrid = res.GetValue<string>("resRid");
			//if (rrid < 0) rrid *= -1;
			resAssetEntry.ResRid = Convert.ToUInt64(rrid);
			resAssetEntry.ResType = (uint)res.GetValue("resType", 0L);
			resAssetEntry.ResMeta = res.GetValue<byte[]>("resMeta");
			resAssetEntry.IsInline = res.HasValue("idata");
			resAssetEntry.Location = AssetDataLocation.Cas;
			if (res.HasValue("cas"))
			{
				resAssetEntry.Location = AssetDataLocation.CasNonIndexed;
				resAssetEntry.ExtraData = new AssetExtraData();
				resAssetEntry.ExtraData.DataOffset = (uint)res.GetValue("offset", 0L);
				resAssetEntry.ExtraData.CasPath = (res.HasValue("catalog") ? fs.GetFilePath(res.GetValue("catalog", 0), res.GetValue("cas", 0), res.HasValue("patch")) : fs.GetFilePath(res.GetValue("cas", 0)));
				resAssetEntry.ExtraData.IsPatch = res.HasValue("patch") ? res.GetValue<bool>("patch") : false;
			}
			else if (res.GetValue("sb", defaultValue: false))
			{
				resAssetEntry.Location = AssetDataLocation.SuperBundle;
				resAssetEntry.ExtraData = new AssetExtraData();
				resAssetEntry.ExtraData.DataOffset = (uint)res.GetValue("offset", 0L);
				resAssetEntry.ExtraData.SuperBundleId = superBundles.Count - 1;
			}
			else if (res.GetValue("cache", defaultValue: false))
			{
				resAssetEntry.Location = AssetDataLocation.Cache;
				resAssetEntry.ExtraData = new AssetExtraData();
				//resAssetEntry.ExtraData.DataOffset = 3735928559L;
			}
			else if (res.GetValue("casPatchType", 0) == 2)
			{
				resAssetEntry.ExtraData = new AssetExtraData();
				resAssetEntry.ExtraData.BaseSha1 = res.GetValue<Sha1>("baseSha1");
				resAssetEntry.ExtraData.DeltaSha1 = res.GetValue<Sha1>("deltaSha1");
			}

			resAssetEntry.SBFileLocation = res.GetValue<string>("SBFileLocation");
			resAssetEntry.TOCFileLocation = res.GetValue<string>("TOCFileLocation");
			resAssetEntry.SB_CAS_Offset_Position = res.GetValue<int>("SB_CAS_Offset_Position");
			resAssetEntry.SB_CAS_Size_Position = res.GetValue<int>("SB_CAS_Size_Position");

			RES.Add(value, resAssetEntry);
			if (resAssetEntry.ResRid != 0L)
			{
				if (resRidList.ContainsKey(resAssetEntry.ResRid))
					resRidList.Remove(resAssetEntry.ResRid);

				resRidList.Add(resAssetEntry.ResRid, resAssetEntry);
			}
			return resAssetEntry;
		}

		private ChunkAssetEntry AddChunk(DbObject chunk, bool returnExisting = false)
		{
			Guid value = chunk.GetValue<Guid>("id");

            //if (chunkList.ContainsKey(value) && returnExisting)
   //         {
			//	return chunkList[value];
			//}

			if (value.ToString() == "e35237c0-ebbe-bc31-1504-aaf3eb947c9b")
			{

			}

			if (value.ToString() == "56aeabb4-505a-58f4-77a6-f3b3c5fda185")
			{

			}

			

			ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
			chunkAssetEntry.Id = value;
			chunkAssetEntry.Sha1 = chunk.GetValue<Sha1>("sha1");
			chunkAssetEntry.Size = chunk.GetValue("size", 0L);
			chunkAssetEntry.LogicalOffset = chunk.GetValue("logicalOffset", 0u);
			chunkAssetEntry.LogicalSize = chunk.GetValue("logicalSize", 0u);
			chunkAssetEntry.RangeStart = chunk.GetValue("rangeStart", 0u);
			chunkAssetEntry.RangeEnd = chunk.GetValue("rangeEnd", 0u);
			chunkAssetEntry.BundledSize = chunk.GetValue("bundledSize", 0u);
			chunkAssetEntry.IsInline = chunk.HasValue("idata");
			chunkAssetEntry.Location = AssetDataLocation.Cas;
			if (chunk.HasValue("cas"))
			{
				chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
				chunkAssetEntry.ExtraData = new AssetExtraData();
				chunkAssetEntry.ExtraData.DataOffset = (uint)chunk.GetValue("offset", 0L);
				chunkAssetEntry.ExtraData.CasPath = (chunk.HasValue("catalog") ? fs.GetFilePath(chunk.GetValue("catalog", 0), chunk.GetValue("cas", 0), chunk.HasValue("patch")) : fs.GetFilePath(chunk.GetValue("cas", 0)));
				chunkAssetEntry.ExtraData.IsPatch = chunk.HasValue("patch") ? chunk.GetValue<bool>("patch") : false;
			}
			else if (chunk.GetValue("sb", defaultValue: false))
			{
				chunkAssetEntry.Location = AssetDataLocation.SuperBundle;
				chunkAssetEntry.ExtraData = new AssetExtraData();
				chunkAssetEntry.ExtraData.DataOffset = (uint)chunk.GetValue("offset", 0L);
				chunkAssetEntry.ExtraData.SuperBundleId = superBundles.Count - 1;
			}
			else if (chunk.GetValue("cache", defaultValue: false))
			{
				chunkAssetEntry.Location = AssetDataLocation.Cache;
				chunkAssetEntry.ExtraData = new AssetExtraData();
				//chunkAssetEntry.ExtraData.DataOffset = 3735928559L;
			}

			chunkAssetEntry.SBFileLocation = chunk.GetValue<string>("SBFileLocation");
			chunkAssetEntry.TOCFileLocation = chunk.GetValue<string>("TOCFileLocation");
			chunkAssetEntry.SB_CAS_Offset_Position = chunk.GetValue<int>("SB_CAS_Offset_Position");
			chunkAssetEntry.SB_CAS_Size_Position = chunk.GetValue<int>("SB_CAS_Size_Position");

			//chunkAssetEntry.Bundles.Add(chunk.GetValue<int>("bundleIndex"));
			//chunkList.Add(value, chunkAssetEntry);
			AddChunk(chunkAssetEntry);

			return chunkAssetEntry;
		}

		public void SendManagerCommand(string type, string command, params object[] value)
		{
			if (CustomAssetManagers.ContainsKey(type))
			{
				CustomAssetManagers[type].OnCommand(command, value);
			}
		}

		private bool ReadFromCache(out List<EbxAssetEntry> prePatchCache)
		{
			prePatchCache = null;
			if (!File.Exists(fs.CacheName + ".cache"))
			{
				return false;
			}
			WriteToLog("Loading data (" + fs.CacheName + ".cache)");

			if (!string.IsNullOrEmpty(ProfilesLibrary.CacheReader))
			{
				var resultFromPlugin = ((ICacheReader)LoadTypeFromPlugin(ProfilesLibrary.CacheReader)).Read();
				AssetManager.Instance = this;
				return resultFromPlugin;
			}

			bool flag = false;
			using (NativeReader nativeReader = new NativeReader(new FileStream(fs.CacheName + ".cache", FileMode.Open, FileAccess.Read)))
			{
				if (nativeReader.ReadLengthPrefixedString() != ProfilesLibrary.ProfileName)
					return false;

				var cacheHead = nativeReader.ReadUInt();
				if (cacheHead != fs.Head)
				{
					flag = true;
					prePatchCache = new List<EbxAssetEntry>();
					//CacheUpdate = true;
				}
				int count = nativeReader.ReadInt();
				if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
				{
					superBundles.Add(new SuperBundleEntry
					{
						Name = "<none>"
					});
				}
				else
				{
					for (int i = 0; i < count; i++)
					{
						SuperBundleEntry superBundleEntry = new SuperBundleEntry();
						superBundleEntry.Name = nativeReader.ReadNullTerminatedString();
						superBundles.Add(superBundleEntry);
					}
				}
				count = nativeReader.ReadInt();
                if (!ProfilesLibrary.IsFIFA21DataVersion() && count == 0)
                {
                    return false;
                }
                for (int j = 0; j < count; j++)
				{
					BundleEntry bundleEntry = new BundleEntry();
					bundleEntry.Name = nativeReader.ReadNullTerminatedString();
					bundleEntry.SuperBundleId = nativeReader.ReadInt();
					if (!flag)
					{
						bundles.Add(bundleEntry);
					}
				}
				count = nativeReader.ReadInt();
				EBX = new ConcurrentDictionary<string, EbxAssetEntry>(1, count + 100);
				for (int k = 0; k < count; k++)
				{
					EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
					ebxAssetEntry.Name = nativeReader.ReadLengthPrefixedString();
					ebxAssetEntry.Sha1 = nativeReader.ReadSha1();
					ebxAssetEntry.BaseSha1 = rm.GetBaseSha1(ebxAssetEntry.Sha1);
					ebxAssetEntry.Size = nativeReader.ReadLong();
					ebxAssetEntry.OriginalSize = nativeReader.ReadLong();
					ebxAssetEntry.Location = (AssetDataLocation)nativeReader.ReadInt();
					ebxAssetEntry.IsInline = nativeReader.ReadBoolean();
					ebxAssetEntry.Type = nativeReader.ReadLengthPrefixedString();
					ebxAssetEntry.Guid = nativeReader.ReadGuid();
					if (nativeReader.ReadBoolean())
					{
						ebxAssetEntry.ExtraData = new AssetExtraData();
						ebxAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
						ebxAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
						ebxAssetEntry.ExtraData.DataOffset = nativeReader.ReadUInt();
						ebxAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
						ebxAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
						ebxAssetEntry.ExtraData.CasPath = nativeReader.ReadLengthPrefixedString();
					}
					int bundleCount = nativeReader.ReadInt();
					for (int l = 0; l < bundleCount; l++)
					{
						ebxAssetEntry.Bundles.Add(nativeReader.ReadInt());
					}
					bundleCount = nativeReader.ReadInt();
					for (int m = 0; m < bundleCount; m++)
					{
						ebxAssetEntry.DependentAssets.Add(nativeReader.ReadGuid());
					}
					if (ProfilesLibrary.IsFIFA21DataVersion())
					{
						if (nativeReader.ReadBoolean())
							ebxAssetEntry.SBFileLocation = nativeReader.ReadLengthPrefixedString();
						if (nativeReader.ReadBoolean())
							ebxAssetEntry.TOCFileLocation = nativeReader.ReadLengthPrefixedString();
						if (nativeReader.ReadBoolean())
							ebxAssetEntry.CASFileLocation = nativeReader.ReadLengthPrefixedString();

						ebxAssetEntry.SB_CAS_Offset_Position = nativeReader.ReadInt();
						ebxAssetEntry.SB_CAS_Size_Position = nativeReader.ReadInt();
						ebxAssetEntry.SB_Sha1_Position = nativeReader.ReadInt();
						ebxAssetEntry.SB_OriginalSize_Position = nativeReader.ReadInt();
					}
     //               if (ProfilesLibrary.IsMadden21DataVersion())
     //               {
					//	ebxAssetEntry.ParentBundleOffset = nativeReader.ReadInt();
					//	ebxAssetEntry.ParentBundleSize = nativeReader.ReadInt();
					//}
					if (flag)
					{
						prePatchCache.Add(ebxAssetEntry);
					}
					else
					{
						EBX.TryAdd(ebxAssetEntry.Name, ebxAssetEntry);
					}
				}
				count = nativeReader.ReadInt();
				for (int n = 0; n < count; n++)
				{
					ResAssetEntry resAssetEntry = new ResAssetEntry();
					resAssetEntry.Name = nativeReader.ReadLengthPrefixedString();
					resAssetEntry.Sha1 = nativeReader.ReadSha1();
					resAssetEntry.BaseSha1 = rm.GetBaseSha1(resAssetEntry.Sha1);
					resAssetEntry.Size = nativeReader.ReadLong();
					resAssetEntry.OriginalSize = nativeReader.ReadLong();
					resAssetEntry.Location = (AssetDataLocation)nativeReader.ReadInt();
					resAssetEntry.IsInline = nativeReader.ReadBoolean();
					resAssetEntry.ResRid = nativeReader.ReadULong();
					resAssetEntry.ResType = nativeReader.ReadUInt();
					resAssetEntry.ResMeta = nativeReader.ReadBytes(nativeReader.ReadInt());
					if (nativeReader.ReadBoolean())
					{
						resAssetEntry.ExtraData = new AssetExtraData();
						resAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
						resAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
						resAssetEntry.ExtraData.DataOffset = nativeReader.ReadUInt();
						resAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
						resAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
						resAssetEntry.ExtraData.CasPath = nativeReader.ReadLengthPrefixedString();
					}
                    if (ProfilesLibrary.IsFIFA21DataVersion() || ProfilesLibrary.IsMadden21DataVersion())
                    {
						if (nativeReader.ReadBoolean())
							resAssetEntry.SBFileLocation = nativeReader.ReadLengthPrefixedString();
						if (nativeReader.ReadBoolean())
							resAssetEntry.TOCFileLocation = nativeReader.ReadLengthPrefixedString();
						if (nativeReader.ReadBoolean())
							resAssetEntry.CASFileLocation = nativeReader.ReadLengthPrefixedString();

						resAssetEntry.SB_CAS_Offset_Position = nativeReader.ReadInt();
                        resAssetEntry.SB_CAS_Size_Position = nativeReader.ReadInt();
                        resAssetEntry.SB_Sha1_Position = nativeReader.ReadInt();
                        resAssetEntry.SB_OriginalSize_Position = nativeReader.ReadInt();
                    }
					if (ProfilesLibrary.IsMadden21DataVersion())
					{
						resAssetEntry.ParentBundleOffset = nativeReader.ReadInt();
						resAssetEntry.ParentBundleSize = nativeReader.ReadInt();
					}



					int num3 = nativeReader.ReadInt();
					for (int num4 = 0; num4 < num3; num4++)
					{
						resAssetEntry.Bundles.Add(nativeReader.ReadInt());
					}
					
					if (!flag || (CacheUpdate && resAssetEntry.ExtraData.CasPath.Contains("native_data", StringComparison.OrdinalIgnoreCase)))
					{
						RES.Add(resAssetEntry.Name, resAssetEntry);
						if (resAssetEntry.ResRid != 0L)
						{
							if(!resRidList.ContainsKey(resAssetEntry.ResRid))
								resRidList.Add(resAssetEntry.ResRid, resAssetEntry);
						}
					}
				}
				count = nativeReader.ReadInt();
				for (int num5 = 0; num5 < count; num5++)
                {
                    ChunkAssetEntry chunkAssetEntry = ReadChunkFromCache(nativeReader);
                    if (!flag)
					{
                        if (Chunks.ContainsKey(chunkAssetEntry.Id))
                        {
                            //chunkList.Remove(chunkAssetEntry.Id);
                            Chunks.TryRemove(chunkAssetEntry.Id, out _);
                        }
                        //chunkList.Add(chunkAssetEntry.Id, chunkAssetEntry);
                        Chunks.TryAdd(chunkAssetEntry.Id, chunkAssetEntry);
						var entryAfter = Chunks[chunkAssetEntry.Id];
						//if(chunkAssetEntry.Id.ToString() == "3e0a186b-c286-1dff-455b-7eb097c3e8f9")
      //                  {

      //                  }

						if (chunkAssetEntry.Id.ToString() == "dbb8c69e-38fa-eeff-3dd5-cebb88ca6df9")
						{

						}

						//{7ba5f499-d244-0c9b-cadd-1a351fec88cb}
						if (chunkAssetEntry.Id.ToString() == "7ba5f499-d244-0c9b-cadd-1a351fec88cb")
						{

						}

						if(chunkAssetEntry.Id.ToString() == "2912b9ae-d22c-ac5a-9ff8-f81cf792c23d")
                        {

                        }
					}
				}

				var ChunkListDupCount = nativeReader.ReadInt();
                for (int cnklistindex = 0; cnklistindex < ChunkListDupCount; cnklistindex++)
                {
                    var chunkDupGuid = nativeReader.ReadGuid();
                    var chunkDupCount = nativeReader.ReadInt();

					var lstOfChunks = new List<ChunkAssetEntry>();
                    for (var chunkDupValueIndex = 0; chunkDupValueIndex < chunkDupCount; chunkDupValueIndex++)
                    {
						lstOfChunks.Add(ReadChunkFromCache(nativeReader));
                    }
					ChunkListDuplicates.Add(chunkDupGuid, lstOfChunks);

				}
            }

			AssetManager.Instance = this;
			return !flag;
		}

        private ChunkAssetEntry ReadChunkFromCache(NativeReader nativeReader)
        {
            ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
            chunkAssetEntry.Id = nativeReader.ReadGuid();
            chunkAssetEntry.Sha1 = nativeReader.ReadSha1();
            chunkAssetEntry.BaseSha1 = rm.GetBaseSha1(chunkAssetEntry.Sha1);
            chunkAssetEntry.Size = nativeReader.ReadLong();
            chunkAssetEntry.Location = (AssetDataLocation)nativeReader.ReadInt();
            chunkAssetEntry.IsInline = nativeReader.ReadBoolean();
            chunkAssetEntry.BundledSize = nativeReader.ReadUInt();
            chunkAssetEntry.RangeStart = nativeReader.ReadUInt();
            chunkAssetEntry.RangeEnd = nativeReader.ReadUInt();
            chunkAssetEntry.LogicalOffset = nativeReader.ReadUInt();
            chunkAssetEntry.LogicalSize = nativeReader.ReadUInt();
            chunkAssetEntry.H32 = nativeReader.ReadInt();
            chunkAssetEntry.FirstMip = nativeReader.ReadInt();
            if (nativeReader.ReadBoolean())
            {
                chunkAssetEntry.ExtraData = new AssetExtraData();
                chunkAssetEntry.ExtraData.BaseSha1 = nativeReader.ReadSha1();
                chunkAssetEntry.ExtraData.DeltaSha1 = nativeReader.ReadSha1();
                chunkAssetEntry.ExtraData.DataOffset = nativeReader.ReadUInt();
                chunkAssetEntry.ExtraData.SuperBundleId = nativeReader.ReadInt();
                chunkAssetEntry.ExtraData.IsPatch = nativeReader.ReadBoolean();
                chunkAssetEntry.ExtraData.CasPath = nativeReader.ReadLengthPrefixedString();
            }
			//else
			//{
			//	throw new Exception("No Extra Data!");
			//}
			if (ProfilesLibrary.IsFIFA21DataVersion() || ProfilesLibrary.IsMadden21DataVersion())
            {
                if (nativeReader.ReadBoolean())
                    chunkAssetEntry.SBFileLocation = nativeReader.ReadLengthPrefixedString();
                if (nativeReader.ReadBoolean())
                    chunkAssetEntry.TOCFileLocation = nativeReader.ReadLengthPrefixedString();
                if (nativeReader.ReadBoolean())
                    chunkAssetEntry.CASFileLocation = nativeReader.ReadLengthPrefixedString();


                chunkAssetEntry.SB_CAS_Offset_Position = nativeReader.ReadInt();
                chunkAssetEntry.SB_CAS_Size_Position = nativeReader.ReadInt();
                chunkAssetEntry.SB_Sha1_Position = nativeReader.ReadInt();
                chunkAssetEntry.SB_OriginalSize_Position = nativeReader.ReadInt();

                chunkAssetEntry.SB_LogicalOffset_Position = nativeReader.ReadUInt();
                chunkAssetEntry.SB_LogicalSize_Position = nativeReader.ReadUInt();
            }
			if (ProfilesLibrary.IsMadden21DataVersion())
			{
				chunkAssetEntry.ParentBundleOffset = nativeReader.ReadInt();
				chunkAssetEntry.ParentBundleSize = nativeReader.ReadInt();
			}
			int chunkBundleCount = nativeReader.ReadInt();
            for (int chunkIndez = 0; chunkIndez < chunkBundleCount; chunkIndez++)
            {
                chunkAssetEntry.Bundles.Add(nativeReader.ReadInt());
            }

            return chunkAssetEntry;
        }



        private void WriteToCache()
		{
			if(!string.IsNullOrEmpty(ProfilesLibrary.CacheWriter))
            {
				((ICacheWriter)LoadTypeFromPlugin(ProfilesLibrary.CacheWriter)).Write();
				return;
			}

			var msCache = new MemoryStream();
			//using (NativeWriter nativeWriter = new NativeWriter(new FileStream(fs.CacheName + ".cache", FileMode.Create)))
			using (NativeWriter nativeWriter = new NativeWriter(msCache, leaveOpen: true))
			{
				nativeWriter.WriteLengthPrefixedString(ProfilesLibrary.ProfileName);
				nativeWriter.Write(fs.Head);
				if (ProfilesLibrary.DataVersion == 20171117 || ProfilesLibrary.DataVersion == 20180628)
				{
					nativeWriter.Write(0);
				}
				else
				{
					nativeWriter.Write(superBundles.Count);
					foreach (SuperBundleEntry superBundle in superBundles)
					{
						nativeWriter.WriteNullTerminatedString(superBundle.Name);
					}
				}
				nativeWriter.Write(bundles.Count);
				foreach (BundleEntry bundle in bundles)
				{
					nativeWriter.WriteNullTerminatedString(bundle.Name);
					nativeWriter.Write(bundle.SuperBundleId);
				}
				nativeWriter.Write(EBX.Values.Count);
				foreach (EbxAssetEntry ebxEntry in EBX.Values)
				{
                    //var ebxJson = JsonConvert.SerializeObject(ebxEntry);
                    //nativeWriter.WriteLengthPrefixedString(ebxJson);
                    nativeWriter.WriteSizedString(ebxEntry.Name);
                    nativeWriter.Write(ebxEntry.Sha1);
                    nativeWriter.Write(ebxEntry.Size);
                    nativeWriter.Write(ebxEntry.OriginalSize);
                    nativeWriter.Write((int)ebxEntry.Location);
                    nativeWriter.Write(ebxEntry.IsInline);
                    nativeWriter.WriteLengthPrefixedString((ebxEntry.Type != null) ? ebxEntry.Type : "");
                    nativeWriter.Write(ebxEntry.Guid);
                    nativeWriter.Write(ebxEntry.ExtraData != null);
                    if (ebxEntry.ExtraData != null)
                    {
                        nativeWriter.Write(ebxEntry.ExtraData.BaseSha1);
                        nativeWriter.Write(ebxEntry.ExtraData.DeltaSha1);
                        nativeWriter.Write(ebxEntry.ExtraData.DataOffset);
                        nativeWriter.Write(ebxEntry.ExtraData.SuperBundleId);
                        nativeWriter.Write(ebxEntry.ExtraData.IsPatch);
                        nativeWriter.WriteLengthPrefixedString(ebxEntry.ExtraData.CasPath);
                    }
                    nativeWriter.Write(ebxEntry.Bundles.Count);
                    foreach (int bundle2 in ebxEntry.Bundles)
                    {
                        nativeWriter.Write(bundle2);
                    }
                    nativeWriter.Write(ebxEntry.DependentAssets.Count);
                    foreach (Guid item in ebxEntry.EnumerateDependencies())
                    {
                        nativeWriter.Write(item);
                    }

                    if (ProfilesLibrary.IsFIFA21DataVersion() || ProfilesLibrary.IsMadden21DataVersion())
                    {
                        nativeWriter.Write(!string.IsNullOrEmpty(ebxEntry.SBFileLocation));
                        if (!string.IsNullOrEmpty(ebxEntry.SBFileLocation))
                            nativeWriter.WriteLengthPrefixedString(ebxEntry.SBFileLocation);
                        nativeWriter.Write(!string.IsNullOrEmpty(ebxEntry.TOCFileLocation));
                        if (!string.IsNullOrEmpty(ebxEntry.TOCFileLocation))
                            nativeWriter.WriteLengthPrefixedString(ebxEntry.TOCFileLocation);

                        nativeWriter.Write(!string.IsNullOrEmpty(ebxEntry.CASFileLocation));
                        if (!string.IsNullOrEmpty(ebxEntry.CASFileLocation))
                            nativeWriter.WriteLengthPrefixedString(ebxEntry.CASFileLocation);

                        nativeWriter.Write(ebxEntry.SB_CAS_Offset_Position);
                        nativeWriter.Write(ebxEntry.SB_CAS_Size_Position);
                        nativeWriter.Write(ebxEntry.SB_Sha1_Position);
                        nativeWriter.Write(ebxEntry.SB_OriginalSize_Position);
                    }
                    if (ProfilesLibrary.IsMadden21DataVersion())
                    {
                        nativeWriter.Write(ebxEntry.ParentBundleOffset);
                        nativeWriter.Write(ebxEntry.ParentBundleSize);
                    }

                }
				nativeWriter.Write(RES.Values.Count);
				foreach (ResAssetEntry resEntry in RES.Values)
				{

                    nativeWriter.WriteLengthPrefixedString(resEntry.Name);
                    nativeWriter.Write(resEntry.Sha1);
                    nativeWriter.Write(resEntry.Size);
                    nativeWriter.Write(resEntry.OriginalSize);
                    nativeWriter.Write((int)resEntry.Location);
                    nativeWriter.Write(resEntry.IsInline);
                    nativeWriter.Write(resEntry.ResRid);
                    nativeWriter.Write(resEntry.ResType);
                    nativeWriter.Write(resEntry.ResMeta.Length);
                    nativeWriter.Write(resEntry.ResMeta);
                    nativeWriter.Write(resEntry.ExtraData != null);
                    if (resEntry.ExtraData != null)
                    {
                        nativeWriter.Write(resEntry.ExtraData.BaseSha1);
                        nativeWriter.Write(resEntry.ExtraData.DeltaSha1);
                        nativeWriter.Write(resEntry.ExtraData.DataOffset);
                        nativeWriter.Write(resEntry.ExtraData.SuperBundleId);
                        nativeWriter.Write(resEntry.ExtraData.IsPatch);
                        nativeWriter.WriteLengthPrefixedString(resEntry.ExtraData.CasPath);
                    }
                    if (ProfilesLibrary.IsFIFA21DataVersion() || ProfilesLibrary.IsMadden21DataVersion())
                    {
                        nativeWriter.Write(!string.IsNullOrEmpty(resEntry.SBFileLocation));
                        if (!string.IsNullOrEmpty(resEntry.SBFileLocation))
                            nativeWriter.WriteLengthPrefixedString(resEntry.SBFileLocation);
                        nativeWriter.Write(!string.IsNullOrEmpty(resEntry.TOCFileLocation));
                        if (!string.IsNullOrEmpty(resEntry.TOCFileLocation))
                            nativeWriter.WriteLengthPrefixedString(resEntry.TOCFileLocation);

                        nativeWriter.Write(!string.IsNullOrEmpty(resEntry.CASFileLocation));
                        if (!string.IsNullOrEmpty(resEntry.CASFileLocation))
                            nativeWriter.WriteLengthPrefixedString(resEntry.CASFileLocation);

                        nativeWriter.Write(resEntry.SB_CAS_Offset_Position);
                        nativeWriter.Write(resEntry.SB_CAS_Size_Position);
                        nativeWriter.Write(resEntry.SB_Sha1_Position);
                        nativeWriter.Write(resEntry.SB_OriginalSize_Position);
                    }
                    if (ProfilesLibrary.IsMadden21DataVersion())
                    {
                        nativeWriter.Write(resEntry.ParentBundleOffset);
                        nativeWriter.Write(resEntry.ParentBundleSize);
                    }

                    nativeWriter.Write(resEntry.Bundles.Count);
                    foreach (int bundle3 in resEntry.Bundles)
                    {
                        nativeWriter.Write(bundle3);
                    }
                }
				nativeWriter.Write(Chunks.Count);
				foreach (ChunkAssetEntry chunkEntry in Chunks.Values)
                {
                    WriteChunkToCache(nativeWriter, chunkEntry);

                }
                nativeWriter.Write(ChunkListDuplicates.Count);
                foreach (var grp in ChunkListDuplicates)
                {
                    nativeWriter.Write(grp.Key);
                    nativeWriter.Write(grp.Value.Count);
                    foreach (var chunkEntry in grp.Value)
                    {
						WriteChunkToCache(nativeWriter, chunkEntry);
                    }
                }
            }

			using (NativeWriter nativeWriter = new NativeWriter(new FileStream(ApplicationDirectory + fs.CacheName + ".cache", FileMode.Create)))
            {
				nativeWriter.WriteBytes(msCache.ToArray());
				nativeWriter.Flush();
            }

			msCache.Close();
			msCache.Dispose();

		}

		public static string ApplicationDirectory
		{
			get
			{
				return System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\";
			}
		}

		private static void WriteChunkToCache(NativeWriter nativeWriter, ChunkAssetEntry chunkEntry)
        {
            //var chunkJson = JsonConvert.SerializeObject(chunkEntry);
            //nativeWriter.WriteLengthPrefixedString(chunkJson);
            nativeWriter.Write(chunkEntry.Id);
            nativeWriter.Write(chunkEntry.Sha1);
            nativeWriter.Write(chunkEntry.Size);
            nativeWriter.Write((int)chunkEntry.Location);
            nativeWriter.Write(chunkEntry.IsInline);
            nativeWriter.Write(chunkEntry.BundledSize);
            nativeWriter.Write(chunkEntry.RangeStart);
            nativeWriter.Write(chunkEntry.RangeEnd);
            nativeWriter.Write(chunkEntry.LogicalOffset);
            nativeWriter.Write(chunkEntry.LogicalSize);
            nativeWriter.Write(chunkEntry.H32);
            nativeWriter.Write(chunkEntry.FirstMip);
            nativeWriter.Write(chunkEntry.ExtraData != null);
            if (chunkEntry.ExtraData != null)
            {
                nativeWriter.Write(chunkEntry.ExtraData.BaseSha1);
                nativeWriter.Write(chunkEntry.ExtraData.DeltaSha1);
                nativeWriter.Write(chunkEntry.ExtraData.DataOffset);
                nativeWriter.Write(chunkEntry.ExtraData.SuperBundleId);
                nativeWriter.Write(chunkEntry.ExtraData.IsPatch);
                nativeWriter.WriteLengthPrefixedString(chunkEntry.ExtraData.CasPath);
            }
			//else
   //         {
			//	throw new Exception("No Extra Data!");
   //         }
            if (ProfilesLibrary.IsFIFA21DataVersion() || ProfilesLibrary.IsMadden21DataVersion())
            {
                nativeWriter.Write(!string.IsNullOrEmpty(chunkEntry.SBFileLocation));
                if (!string.IsNullOrEmpty(chunkEntry.SBFileLocation))
                    nativeWriter.WriteLengthPrefixedString(chunkEntry.SBFileLocation);
                nativeWriter.Write(!string.IsNullOrEmpty(chunkEntry.TOCFileLocation));
                if (!string.IsNullOrEmpty(chunkEntry.TOCFileLocation))
                    nativeWriter.WriteLengthPrefixedString(chunkEntry.TOCFileLocation);

                nativeWriter.Write(!string.IsNullOrEmpty(chunkEntry.CASFileLocation));
                if (!string.IsNullOrEmpty(chunkEntry.CASFileLocation))
                    nativeWriter.WriteLengthPrefixedString(chunkEntry.CASFileLocation);

                nativeWriter.Write(chunkEntry.SB_CAS_Offset_Position);
                nativeWriter.Write(chunkEntry.SB_CAS_Size_Position);
                nativeWriter.Write(chunkEntry.SB_Sha1_Position);
                nativeWriter.Write(chunkEntry.SB_OriginalSize_Position);

                nativeWriter.Write(chunkEntry.SB_LogicalOffset_Position);
                nativeWriter.Write(chunkEntry.SB_LogicalSize_Position);
            }
            if (ProfilesLibrary.IsMadden21DataVersion())
            {
                nativeWriter.Write(chunkEntry.ParentBundleOffset);
                nativeWriter.Write(chunkEntry.ParentBundleSize);
            }
			if(chunkEntry.Id.ToString() == "e35237c0-ebbe-bc31-1504-aaf3eb947c9b")
            {

            }
            nativeWriter.Write(chunkEntry.Bundles.Count);
            foreach (int bundle in chunkEntry.Bundles)
            {
                nativeWriter.Write(bundle);
            }
        }

        private string lastLogMessage = null;
		private void WriteToLog(string text, params object[] vars)
		{
			if (logger != null)
			{
				if (lastLogMessage == text)
					return;

				lastLogMessage = text;
				logger.Log(text, vars);
			}
		}

		private Sha1 GenerateSha1(byte[] buffer)
		{
			using (SHA1Managed sHA1Managed = new SHA1Managed())
			{
				return new Sha1(sHA1Managed.ComputeHash(buffer));
			}
		}

		public Guid GenerateChunkId(AssetEntry ae)
		{
			ulong num = Murmur2.HashString64(ae.Filename, 18532uL);
			ulong value = Murmur2.HashString64(ae.Path, 18532uL);
			int num2 = 1;
			Guid guid = Guid.Empty;
			do
			{
				using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream()))
				{
					nativeWriter.Write(value);
					nativeWriter.Write((ulong)((long)num ^ (long)num2));
					byte[] array = ((MemoryStream)nativeWriter.BaseStream).ToArray();
					array[15] = 1;
					guid = new Guid(array);
				}
				num2++;
			}
			while (AssetManager.Instance.GetChunkEntry(guid) != null);
			return guid;
		}
	}
}
