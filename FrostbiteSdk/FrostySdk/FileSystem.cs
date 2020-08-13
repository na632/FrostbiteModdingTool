using Frosty.Hash;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

namespace FrostySdk
{
	public class FileSystem
	{
		private List<string> paths = new List<string>();

		private List<string> superBundles = new List<string>();

		private List<CatalogInfo> catalogs = new List<CatalogInfo>();

		private Dictionary<string, byte[]> memoryFs = new Dictionary<string, byte[]>();

		private List<string> casFiles = new List<string>();

		private string basePath;

		private string cacheName;

		private Type deobfuscatorType;

		private uint baseNum;

		private uint headNum;

		private List<ManifestBundleInfo> manifestBundles = new List<ManifestBundleInfo>();

		private List<ManifestChunkInfo> manifestChunks = new List<ManifestChunkInfo>();

		public int SuperBundleCount => superBundles.Count;

		public IEnumerable<string> SuperBundles
		{
			get
			{
				for (int i = 0; i < superBundles.Count; i++)
				{
					yield return superBundles[i];
				}
			}
		}

		public int CatalogCount => catalogs.Count;

		public IEnumerable<string> Catalogs
		{
			get
			{
				for (int i = 0; i < catalogs.Count; i++)
				{
					yield return catalogs[i].Name;
				}
			}
		}

		public int CasFileCount => casFiles.Count;

		public uint Base => baseNum;

		public uint Head => headNum;

		public string CacheName => cacheName;

		public string BasePath => basePath;

        public static FileSystem Instance;

		public FileSystem(string inBasePath)
		{
			basePath = inBasePath;
			if (!basePath.EndsWith("\\") || !basePath.EndsWith("/"))
			{
				basePath += "\\";
			}
			cacheName = ProfilesLibrary.CacheName;
			deobfuscatorType = ProfilesLibrary.Deobfuscator;

            Instance = this;
		}


		/// <summary>
		/// Needs to first 16 bytes of the key
		/// </summary>
		/// <param name="key"></param>
		public void Initialize(byte[] key = null)
		{
			ProcessLayouts();
			LoadInitfs(key);
		}

		public IDeobfuscator CreateDeobfuscator()
		{
			return (IDeobfuscator)Activator.CreateInstance(deobfuscatorType);
		}

		public void AddSource(string path, bool iterateSubPaths = false)
		{
			if (Directory.Exists(basePath + path))
			{
				if (iterateSubPaths)
				{
					foreach (string item in Directory.EnumerateDirectories(basePath + path, "*", SearchOption.AllDirectories))
					{
						if (!item.ToLower().Contains("\\patch") && File.Exists(item + "\\package.mft"))
						{
							paths.Add("\\" + item.Replace(basePath, "").ToLower() + "\\data\\");
						}
					}
				}
				else
				{
					paths.Add("\\" + path.ToLower() + "\\");
				}
			}
		}

		public string ResolvePath(string filename)
		{
			//Debug.WriteLine(JsonConvert.SerializeObject(paths));
			if (filename.StartsWith("native_patch/") && paths.Count == 1)
			{
				return "";
			}
			int num = 0;
			int num2 = paths.Count;
			if (filename.StartsWith("native_data/") && paths.Count > 1)
			{
				num = 1;
			}
			else if (filename.StartsWith("native_patch/"))
			{
				num2 = 1;
			}
			filename = filename.Replace("native_data/", "");
			filename = filename.Replace("native_patch/", "");
			filename = filename.Trim('/');
            if (paths.Count > 0)
            {
                for (int i = num; i < num2; i++)
                {
                    if (File.Exists(basePath + paths[i] + filename) || Directory.Exists(basePath + paths[i] + filename))
                    {
                        return basePath + paths[i] + filename;
                    }
                }
            }
			return "";
		}

		public string ResolvePath(ManifestFileRef fileRef)
		{
			string filename = (fileRef.IsInPatch ? "native_patch/" : "native_data/") + catalogs[fileRef.CatalogIndex].Name + "/cas_" + fileRef.CasIndex.ToString("D2") + ".cas";
			return ResolvePath(filename);
		}

		public string GetCatalogFromSuperBundle(string sbName)
		{
			foreach (CatalogInfo catalog in catalogs)
			{
				if (catalog.SuperBundles.ContainsKey(sbName))
				{
					return catalog.Name;
				}
			}
			if (ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20160927)
			{
				foreach (CatalogInfo catalog2 in catalogs)
				{
					if (catalog2.Name.EndsWith("installpackage_00", StringComparison.OrdinalIgnoreCase))
					{
						return catalog2.Name;
					}
				}
			}
			else
			{
				foreach (CatalogInfo catalog3 in catalogs)
				{
					if (catalog3.SuperBundles.Count != 0)
					{
						return catalog3.Name;
					}
				}
			}
			return catalogs[0].Name;
		}

		public string GetCatalog(ManifestFileRef fileRef)
		{
			return catalogs[fileRef.CatalogIndex].Name;
		}

		public IEnumerable<CatalogInfo> EnumerateCatalogInfos()
		{
			foreach (CatalogInfo catalog in catalogs)
			{
				yield return catalog;
			}
		}

		public ManifestFileRef GetFileRef(string path)
		{
			path = path.Replace(BasePath, "");
			foreach (string path2 in paths)
			{
				path = path.Replace(path2, "");
			}
			if (path.EndsWith("cat"))
			{
				path = path.Remove(path.Length - 8);
			}
			else if (path.EndsWith("cas"))
			{
				path = path.Remove(path.Length - 11);
			}
			foreach (CatalogInfo catalog in catalogs)
			{
				if (catalog.Name.Equals(path, StringComparison.OrdinalIgnoreCase))
				{
					return new ManifestFileRef(catalogs.IndexOf(catalog), inPatch: false, 0);
				}
			}
			return default(ManifestFileRef);
		}

		public bool HasFileInMemoryFs(string name)
		{
			return memoryFs.ContainsKey(name);
		}

		public byte[] GetFileFromMemoryFs(string name)
		{
			if (!memoryFs.ContainsKey(name))
			{
				return null;
			}
			return memoryFs[name];
		}

		public string GetFilePath(int index)
		{
			if (index < casFiles.Count)
			{
				return casFiles[index];
			}
			return "";
		}

		public string GetFilePath(int catalog, int cas, bool patch)
		{
			CatalogInfo catalogInfo = catalogs[catalog];
			return (patch ? "native_patch/" : "native_data/") + catalogInfo.Name + "/cas_" + cas.ToString("D2") + ".cas";
		}

		public void LoadInitfs(byte[] key, bool patched = true)
		{
			//if (ProfilesLibrary.DataVersion != 20170321 && ProfilesLibrary.DataVersion != 20170929 && ProfilesLibrary.DataVersion != 20180914 && ProfilesLibrary.DataVersion != 20181207 && ProfilesLibrary.DataVersion != 20190911 && ProfilesLibrary.DataVersion != 20190905)
			//{
			//	return;
			//}
			string text = ResolvePath((patched ? "native_patch/" : "native_data/") + "initfs_win32");
			if (text == "")
			{
				return;
			}
			using (DbReader dbReader = new DbReader(new FileStream(text, FileMode.Open, FileAccess.Read), CreateDeobfuscator()))
			{
				DbObject dbObject = dbReader.ReadDbObject();
				//if (ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905)
				//{
					byte[] value = dbObject.GetValue<byte[]>("encrypted");
				if (value != null)
				{
					if (key == null)
					{
						Debug.WriteLine("[DEBUG] LoadInitfs()::Key is not available");
						return;
					}
					using (Aes aes = Aes.Create())
					{
						aes.Key = key;
						aes.IV = key;
						ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
						using (MemoryStream stream = new MemoryStream(value))
						{
							using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read))
							{
								cryptoStream.Read(value, 0, value.Length);
								// test write out
								using (FileStream fileStream = new FileStream("fsCrypto.txt", FileMode.OpenOrCreate))
								{
									stream.Position = 0;
									stream.CopyTo(fileStream);
								}
							}

							

						}
					}
					using (DbReader dbReader2 = new DbReader(new MemoryStream(value), CreateDeobfuscator()))
					{
						dbObject = dbReader2.ReadDbObject();
					}

					// ----------- TEST WRITE OUT AND RE-ENCRYPT of INITFS --------------------
					string s = System.Text.Encoding.UTF8.GetString(value, 0, value.Length);
					File.WriteAllText("test.txt", s);
					using (Aes aes = Aes.Create())
					{
						aes.Key = key;
						aes.IV = key;
						ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);
						using (FileStream fileStream = new FileStream("reencrpyted_initfs", FileMode.OpenOrCreate))
						{
							using (CryptoStream cryptoStream = new CryptoStream(fileStream, transform, CryptoStreamMode.Write))
							{
								var bytesOfTest = File.ReadAllBytes("test.txt");
								cryptoStream.Write(bytesOfTest, 0, bytesOfTest.Length);
							}
							//fileStream.Flush();
						}
					}
					// ----------- TEST WRITE OUT AND RE-ENCRYPT of INITFS --------------------
				}
				//}
				foreach (DbObject item in dbObject)
				{
					DbObject value2 = item.GetValue<DbObject>("$file");
					//string payload = System.Text.Encoding.Default.GetString(value2.GetValue<byte[]>("payload")); 
					string value3 = value2.GetValue<string>("name");
					if (!memoryFs.ContainsKey(value3))
					{
						memoryFs.Add(value3, value2.GetValue<byte[]>("payload"));
					}
				}
			}
			if (memoryFs.ContainsKey("__fsinternal__"))
			{
				DbObject dbObject2 = null;
				using (DbReader dbReader3 = new DbReader(new MemoryStream(memoryFs["__fsinternal__"]), null))
				{
					dbObject2 = dbReader3.ReadDbObject();
				}
				memoryFs.Remove("__fsinternal__");
				if (dbObject2.GetValue("inheritContent", defaultValue: false))
				{
					LoadInitfs(key, patched: false);
				}
			}
		}

		private void ProcessLayouts()
		{
			string path = ResolvePath("native_data/layout.toc");
			string text = ResolvePath("native_patch/layout.toc");
			DbObject dbObject = null;
			using (DbReader dbReader = new DbReader(new FileStream(path, FileMode.Open, FileAccess.Read), CreateDeobfuscator()))
			{
				dbObject = dbReader.ReadDbObject();
			}
			foreach (DbObject item2 in dbObject.GetValue<DbObject>("superBundles"))
			{
				superBundles.Add(item2.GetValue<string>("name").ToLower());
			}
			if (text != "")
			{
				DbObject dbObject3 = null;
				using (DbReader dbReader2 = new DbReader(new FileStream(text, FileMode.Open, FileAccess.Read), CreateDeobfuscator()))
				{
					dbObject3 = dbReader2.ReadDbObject();
				}
				foreach (DbObject item3 in dbObject3.GetValue<DbObject>("superBundles"))
				{
					string item = item3.GetValue<string>("name").ToLower();
					if (!superBundles.Contains(item))
					{
						superBundles.Add(item);
					}
				}
				baseNum = dbObject3.GetValue("base", 0u);
				headNum = dbObject3.GetValue("head", 0u);
				ProcessCatalogs(dbObject3);
				ProcessManifest(dbObject3);
			}
			else
			{
				baseNum = dbObject.GetValue("base", 0u);
				headNum = dbObject.GetValue("head", 0u);
				ProcessCatalogs(dbObject);
				ProcessManifest(dbObject);
			}
		}

		private void ProcessCatalogs(DbObject patchLayout)
		{
			DbObject value = patchLayout.GetValue<DbObject>("installManifest");
			if (value != null)
			{
				foreach (DbObject item in value.GetValue<DbObject>("installChunks"))
				{
					if (!item.GetValue("testDLC", defaultValue: false))
					{
						bool value2 = item.GetValue("alwaysInstalled", defaultValue: false);
						string text = "win32/" + item.GetValue<string>("name");
						if ((ProfilesLibrary.DataVersion != 20180628 || !(text == "win32/installation/default")) && (File.Exists(ResolvePath(text + "/cas.cat")) || (item.HasValue("files") && item.GetValue<DbObject>("files").Count != 0) || ProfilesLibrary.DataVersion == 20181207 || ProfilesLibrary.DataVersion == 20190905 || ProfilesLibrary.DataVersion == 20180628))
						{
							CatalogInfo catalogInfo = null;
							Guid catalogId = item.GetValue<Guid>("id");
							catalogInfo = catalogs.Find((CatalogInfo ci) => ci.Id == catalogId);
							if (catalogInfo == null)
							{
								catalogInfo = new CatalogInfo();
								catalogInfo.Id = item.GetValue<Guid>("id");
								catalogInfo.Name = text;
								catalogInfo.AlwaysInstalled = value2;
								foreach (string item2 in item.GetValue<DbObject>("superBundles"))
								{
									catalogInfo.SuperBundles.Add(item2.ToLower(), value: false);
								}
							}
							catalogs.Add(catalogInfo);
							if (item.HasValue("files"))
							{
								foreach (DbObject item3 in item.GetValue<DbObject>("files"))
								{
									int value3 = item3.GetValue("id", 0);
									while (casFiles.Count <= value3)
									{
										casFiles.Add("");
									}
									string text3 = item3.GetValue<string>("path").Trim('/');
									text3 = text3.Replace("native_data/Data", "native_data");
									text3 = text3.Replace("native_data/Patch", "native_patch");
									casFiles[value3] = text3;
								}
							}
							if (item.HasValue("splitSuperBundles"))
							{
								foreach (DbObject item4 in item.GetValue<DbObject>("splitSuperBundles"))
								{
									string key = item4.GetValue<string>("superBundle").ToLower();
									if (!catalogInfo.SuperBundles.ContainsKey(key))
									{
										catalogInfo.SuperBundles.Add(key, value: true);
									}
								}
							}
							if (item.HasValue("splitTocs"))
							{
								foreach (DbObject item5 in item.GetValue<DbObject>("splitTocs"))
								{
									string key2 = "win32/" + item5.GetValue<string>("superbundle").ToLower();
									if (!catalogInfo.SuperBundles.ContainsKey(key2))
									{
										catalogInfo.SuperBundles.Add(key2, value: true);
									}
								}
							}
						}
					}
				}
				return;
			}
			CatalogInfo catalogInfo2 = new CatalogInfo
			{
				Name = ""
			};
			foreach (string superBundle in superBundles)
			{
				catalogInfo2.SuperBundles.Add(superBundle, value: false);
			}
			catalogs.Add(catalogInfo2);
		}

		public IEnumerable<DbObject> EnumerateBundles()
		{
			foreach (ManifestBundleInfo manifestBundle in manifestBundles)
			{
				ManifestFileInfo manifestFileInfo = manifestBundle.files[0];
				CatalogInfo catalogInfo = catalogs[manifestFileInfo.file.CatalogIndex];
				string path = ResolvePath(manifestFileInfo.file);
				if (File.Exists(path))
				{
					using (NativeReader reader = new NativeReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
					{
						using (BinarySbReader sbReader = new BinarySbReader(reader.CreateViewStream(manifestFileInfo.offset, manifestFileInfo.size), 0L, null))
						{
							DbObject dbObject = sbReader.ReadDbObject();
							string newValue = manifestBundle.hash.ToString("x8");
							if (ProfilesLibrary.SharedBundles.ContainsKey(manifestBundle.hash))
							{
								newValue = ProfilesLibrary.SharedBundles[manifestBundle.hash];
							}
							dbObject.SetValue("name", newValue);
							dbObject.SetValue("catalog", catalogInfo.Name);
							yield return dbObject;
						}
					}
				}
			}
		}

		public List<ChunkAssetEntry> ProcessManifestChunks()
		{
			List<ChunkAssetEntry> list = new List<ChunkAssetEntry>();
			foreach (ManifestChunkInfo manifestChunk in manifestChunks)
			{
				ManifestFileInfo file = manifestChunk.file;
				ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
				chunkAssetEntry.Id = manifestChunk.guid;
				string casPath = (file.file.IsInPatch ? "native_patch/" : "native_data/") + catalogs[file.file.CatalogIndex].Name + "/cas_" + file.file.CasIndex.ToString("D2") + ".cas";
				chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
				chunkAssetEntry.Size = file.size;
				chunkAssetEntry.ExtraData = new AssetExtraData();
				chunkAssetEntry.ExtraData.DataOffset = file.offset;
				chunkAssetEntry.ExtraData.CasPath = casPath;
				list.Add(chunkAssetEntry);
			}
			return list;
		}

		public ManifestBundleInfo GetManifestBundle(string name)
		{
			int result = 0;
			if (name.Length != 8 || !int.TryParse(name, NumberStyles.HexNumber, null, out result))
			{
				result = Fnv1.HashString(name);
			}
			foreach (ManifestBundleInfo manifestBundle in manifestBundles)
			{
				if (manifestBundle.hash == result)
				{
					return manifestBundle;
				}
			}
			return null;
		}

		public ManifestBundleInfo GetManifestBundle(int nameHash)
		{
			foreach (ManifestBundleInfo manifestBundle in manifestBundles)
			{
				if (manifestBundle.hash == nameHash)
				{
					return manifestBundle;
				}
			}
			return null;
		}

		public ManifestChunkInfo GetManifestChunk(Guid id)
		{
			return manifestChunks.Find((ManifestChunkInfo a) => a.guid == id);
		}

		public void AddManifestBundle(ManifestBundleInfo bi)
		{
			manifestBundles.Add(bi);
		}

		public void AddManifestChunk(ManifestChunkInfo ci)
		{
			manifestChunks.Add(ci);
		}

		public void ResetManifest()
		{
			manifestBundles.Clear();
			manifestChunks.Clear();
			catalogs.Clear();
			superBundles.Clear();
			ProcessLayouts();
		}

		public byte[] WriteManifest()
		{
			using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream()))
			{
				List<ManifestFileInfo> list = new List<ManifestFileInfo>();
				foreach (ManifestBundleInfo manifestBundle in manifestBundles)
				{
					for (int i = 0; i < manifestBundle.files.Count; i++)
					{
						ManifestFileInfo item = manifestBundle.files[i];
						list.Add(item);
					}
				}
				foreach (ManifestChunkInfo manifestChunk in manifestChunks)
				{
					list.Add(manifestChunk.file);
					manifestChunk.fileIndex = list.Count - 1;
				}
				nativeWriter.Write(list.Count);
				nativeWriter.Write(manifestBundles.Count);
				nativeWriter.Write(manifestChunks.Count);
				foreach (ManifestFileInfo item2 in list)
				{
					nativeWriter.Write(item2.file);
					nativeWriter.Write(item2.offset);
					nativeWriter.Write(item2.size);
				}
				foreach (ManifestBundleInfo manifestBundle2 in manifestBundles)
				{
					nativeWriter.Write(manifestBundle2.hash);
					nativeWriter.Write(list.IndexOf(manifestBundle2.files[0]));
					nativeWriter.Write(manifestBundle2.files.Count);
					nativeWriter.Write(0);
					nativeWriter.Write(0);
				}
				foreach (ManifestChunkInfo manifestChunk2 in manifestChunks)
				{
					nativeWriter.Write(manifestChunk2.guid);
					nativeWriter.Write(manifestChunk2.fileIndex);
				}
				return ((MemoryStream)nativeWriter.BaseStream).ToArray();
			}
		}

		private void ProcessManifest(DbObject patchLayout)
		{
			DbObject value = patchLayout.GetValue<DbObject>("manifest");
			if (value != null)
			{
				List<ManifestFileInfo> list = new List<ManifestFileInfo>();
				ManifestFileRef fileRef = value.GetValue("file", 0);
				_ = catalogs[fileRef.CatalogIndex];
				using (NativeReader nativeReader = new NativeReader(new FileStream(ResolvePath(fileRef), FileMode.Open, FileAccess.Read)))
				{
					long position = value.GetValue("offset", 0);
					value.GetValue("size", 0);
					nativeReader.Position = position;
					uint num = nativeReader.ReadUInt();
					uint num2 = nativeReader.ReadUInt();
					uint num3 = nativeReader.ReadUInt();
					for (uint num4 = 0u; num4 < num; num4++)
					{
						ManifestFileInfo item = new ManifestFileInfo
						{
							file = nativeReader.ReadInt(),
							offset = nativeReader.ReadUInt(),
							size = nativeReader.ReadLong(),
							isChunk = false
						};
						list.Add(item);
					}
					for (uint num5 = 0u; num5 < num2; num5++)
					{
						ManifestBundleInfo manifestBundleInfo = new ManifestBundleInfo();
						manifestBundleInfo.hash = nativeReader.ReadInt();
						int num6 = nativeReader.ReadInt();
						int num7 = nativeReader.ReadInt();
						nativeReader.ReadLong();
						for (int i = 0; i < num7; i++)
						{
							manifestBundleInfo.files.Add(list[num6 + i]);
						}
						manifestBundles.Add(manifestBundleInfo);
					}
					for (uint num8 = 0u; num8 < num3; num8++)
					{
						ManifestChunkInfo manifestChunkInfo = new ManifestChunkInfo();
						manifestChunkInfo.guid = nativeReader.ReadGuid();
						manifestChunkInfo.fileIndex = nativeReader.ReadInt();
						manifestChunkInfo.file = list[manifestChunkInfo.fileIndex];
						manifestChunkInfo.file.isChunk = true;
						manifestChunks.Add(manifestChunkInfo);
					}
				}
			}
		}
	}
}
