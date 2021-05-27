using Frosty.Hash;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FrostySdk.Managers
{
	public class ResourceManager : ILoggable
	{
		private FileSystem fs;

		private ILogger logger;

		private Dictionary<Sha1, CatResourceEntry> resourceEntries = new Dictionary<Sha1, CatResourceEntry>();

		private Dictionary<Sha1, CatPatchEntry> patchEntries = new Dictionary<Sha1, CatPatchEntry>();

		private Dictionary<int, string> casFiles = new Dictionary<int, string>();

		public ResourceManager(FileSystem inFs)
		{
			fs = inFs;
		}

		public void Initialize()
		{
			ZStd.Bind();
			Oodle.Bind(fs.BasePath);
			////if (ProfilesLibrary.DataVersion == 20171210)
			////{
			////	LoadDas();
			////}
			//if (!ProfilesLibrary.IsMaddenDataVersion() && !ProfilesLibrary.IsFIFA21DataVersion())
			//{
			//	WriteToLog("Loading catalogs");
			//	foreach (string catalog in fs.Catalogs)
			//	{
			//		LoadCatalog("native_data/" + catalog + "/cas.cat");
			//		LoadCatalog("native_patch/" + catalog + "/cas.cat");
			//	}
			//}

			if ((ProfilesLibrary.DataVersion == 20170929 
				|| ProfilesLibrary.DataVersion == 20180914 
				|| ProfilesLibrary.IsFIFADataVersion()
				|| ProfilesLibrary.IsFIFA21DataVersion()
				//|| ProfilesLibrary.IsMadden21DataVersion()
				)
				&& fs.HasFileInMemoryFs("Dictionaries/ebx.dict")
				)
			{
				var ebxDict = fs.GetFileFromMemoryFs("Dictionaries/ebx.dict");
				ZStd.SetDictionary(ebxDict);
			}
		}

		public ManifestFileInfo GetResourceInfo(Sha1 sha1)
		{
			if (!resourceEntries.ContainsKey(sha1))
			{
				return null;
			}
			CatResourceEntry catResourceEntry = resourceEntries[sha1];
			string text = casFiles[catResourceEntry.ArchiveIndex];
			ManifestFileRef fileRef = fs.GetFileRef(text);
			bool inPatch = false;
			int inCasIndex = int.Parse(text.Remove(0, text.Length - 6).Remove(2));
			if (text.Contains("\\patch\\"))
			{
				inPatch = true;
			}
			return new ManifestFileInfo
			{
				file = new ManifestFileRef(fileRef.CatalogIndex, inPatch, inCasIndex),
				offset = catResourceEntry.Offset,
				size = catResourceEntry.Size
			};
		}

		public Stream GetResourceData(Sha1 sha1)
		{
			if (patchEntries.ContainsKey(sha1))
			{
				CatPatchEntry catPatchEntry = patchEntries[sha1];
				return GetResourceData(catPatchEntry.BaseSha1, catPatchEntry.DeltaSha1);
			}
			if (!resourceEntries.ContainsKey(sha1))
			{
				return null;
			}
			CatResourceEntry catResourceEntry = resourceEntries[sha1];
			byte[] array = null;
			if (catResourceEntry.IsEncrypted && !KeyManager.Instance.HasKey(catResourceEntry.KeyId))
			{
				return null;
			}
			using (NativeReader nativeReader = new NativeReader(new FileStream(casFiles[catResourceEntry.ArchiveIndex], FileMode.Open, FileAccess.Read)))
			{
				using (CasReader casReader = new CasReader(nativeReader.CreateViewStream(catResourceEntry.Offset, catResourceEntry.Size), catResourceEntry.IsEncrypted ? KeyManager.Instance.GetKey(catResourceEntry.KeyId) : null, catResourceEntry.EncryptedSize))
				{
					array = casReader.Read();
				}
			}
			if (array == null)
			{
				return null;
			}
			return new MemoryStream(array);
		}

		public Stream GetResourceData(Sha1 baseSha1, Sha1 deltaSha1)
		{
			if (!resourceEntries.ContainsKey(baseSha1))
			{
				return null;
			}
			if (!resourceEntries.ContainsKey(deltaSha1))
			{
				return null;
			}
			CatResourceEntry catResourceEntry = resourceEntries[baseSha1];
			CatResourceEntry catResourceEntry2 = resourceEntries[deltaSha1];
			byte[] array = null;
			using (NativeReader nativeReader = new NativeReader(new FileStream(casFiles[catResourceEntry.ArchiveIndex], FileMode.Open, FileAccess.Read)))
			{
				using (NativeReader nativeReader2 = (catResourceEntry2.ArchiveIndex == catResourceEntry.ArchiveIndex) ? nativeReader : new NativeReader(new FileStream(casFiles[catResourceEntry2.ArchiveIndex], FileMode.Open, FileAccess.Read)))
				{
					byte[] array2 = (catResourceEntry.IsEncrypted && KeyManager.Instance.HasKey(catResourceEntry.KeyId)) ? KeyManager.Instance.GetKey(catResourceEntry.KeyId) : null;
					byte[] array3 = (catResourceEntry2.IsEncrypted && KeyManager.Instance.HasKey(catResourceEntry2.KeyId)) ? KeyManager.Instance.GetKey(catResourceEntry2.KeyId) : null;
					if (catResourceEntry.IsEncrypted && array2 == null)
					{
						return null;
					}
					if (catResourceEntry2.IsEncrypted && array3 == null)
					{
						return null;
					}
					using (CasReader casReader = new CasReader(nativeReader.CreateViewStream(catResourceEntry.Offset, catResourceEntry.Size), array2, catResourceEntry.EncryptedSize, nativeReader2.CreateViewStream(catResourceEntry2.Offset, catResourceEntry2.Size), array3, catResourceEntry2.EncryptedSize))
					{
						array = casReader.Read();
					}
				}
			}
			if (array == null)
			{
				return null;
			}
			return new MemoryStream(array);
		}

		public static NativeReader LastNativeReader;
		private static Stream LastFileStream
        {
			get
            {
				if(LastNativeReader != null)
                {
					return LastNativeReader.BaseStream as Stream;
                }
				return null;
            }
        }

		public static string LastCasPath;
		public static MemoryStream LastCasPathInMemory;
		public static bool UseLastCasPath = false;

		public MemoryStream GetResourceData(string superBundleName, long offset, long size, AssetEntry entry = null)
		{
			if (UseLastCasPath)
				return GetResourceDataUseLastCas(superBundleName, offset, size);

			superBundleName = superBundleName.Replace("/cs/", "/");

			try
			{
				var path = fs.ResolvePath($"{superBundleName}");
				if (!string.IsNullOrEmpty(path))
				{

					using (var f = new FileStream(path, FileMode.Open, FileAccess.Read))
					{
						using (var nr = new NativeReader(f))
						{
							byte[] array = null;
							using (CasReader casReader = new CasReader(nr.CreateViewStream(offset, size)))
							{
								casReader.AssociatedAssetEntry = entry;
								array = casReader.Read();
							}
							if (array == null)
							{
								return null;
							}
							return new MemoryStream(array);
						}
					}
                }
			}
			catch (Exception e)
            {
				Debug.WriteLine("[DEBUG] [ERROR] " + e.Message);
            }
			return null;
		}

		public MemoryStream GetResourceDataUseLastCas(string superBundleName, long offset, long size)
		{
			superBundleName = superBundleName.Replace("/cs/", "/");

			try
			{
				var path = fs.ResolvePath($"{superBundleName}");
				if (LastCasPath != path && LastCasPathInMemory != null)
				{
					LastCasPathInMemory.Close();
					LastCasPathInMemory.Dispose();
					LastCasPathInMemory = null;
					GC.Collect();
					GC.WaitForPendingFinalizers();
				}

				LastCasPath = path;
				if (!string.IsNullOrEmpty(LastCasPath))
				{
					if (LastCasPathInMemory == null)
					{
						using (var f = new FileStream(LastCasPath, FileMode.Open, FileAccess.Read))
						{
							using (NativeReader reader = new NativeReader(f))
							{
								LastCasPathInMemory = new MemoryStream(reader.ReadToEnd());
								LastCasPathInMemory.Position = 0;
							}
						}
					}
					//using (var f = new FileStream(LastCasPath, FileMode.Open, FileAccess.Read))
					//{
					NativeReader mR = new NativeReader(LastCasPathInMemory);
					{
						byte[] array = null;
						using (CasReader casReader = new CasReader(mR.CreateViewStream(offset, size)))
						{
							array = casReader.Read();
						}
						if (array == null)
						{
							return null;
						}
						return new MemoryStream(array);
					}
					//}
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("[DEBUG] [ERROR] " + e.Message);
			}
			return null;
		}

		public Stream GetResourceData(long offset, long size)
		{
			byte[] array = null;
			using (NativeReader nativeReader = new NativeReader(new FileStream(fs.CacheName + "_sbdata.cas", FileMode.Open, FileAccess.Read)))
			{
				using (CasReader casReader = new CasReader(nativeReader.CreateViewStream(offset, size)))
				{
					array = casReader.Read();
				}
			}
			if (array == null)
			{
				return null;
			}
			return new MemoryStream(array);
		}

		public Stream GetResourceData(byte[] buffer)
		{
			byte[] array = null;
			using (MemoryStream inBaseStream = new MemoryStream(buffer))
			{
				using (CasReader casReader = new CasReader(inBaseStream))
				{
					array = casReader.Read();
				}
			}
			if (array == null)
			{
				return null;
			}
			return new MemoryStream(array);
		}

		public Sha1 GetBaseSha1(Sha1 sha1)
		{
			if (patchEntries.ContainsKey(sha1))
			{
				return patchEntries[sha1].BaseSha1;
			}
			return sha1;
		}

		public bool IsEncrypted(Sha1 sha1)
		{
			if (resourceEntries.ContainsKey(sha1) && resourceEntries[sha1].IsEncrypted)
			{
				return true;
			}
			if (patchEntries.ContainsKey(sha1))
			{
				CatPatchEntry catPatchEntry = patchEntries[sha1];
				if (resourceEntries.ContainsKey(catPatchEntry.BaseSha1) && resourceEntries[catPatchEntry.BaseSha1].IsEncrypted)
				{
					return true;
				}
				if (resourceEntries.ContainsKey(catPatchEntry.DeltaSha1) && resourceEntries[catPatchEntry.DeltaSha1].IsEncrypted)
				{
					return true;
				}
			}
			return false;
		}

		public void SetLogger(ILogger inLogger)
		{
			logger = inLogger;
		}

		public void ClearLogger()
		{
			logger = null;
		}

		private void LoadCatalog(string filename)
		{
			string path = fs.ResolvePath(filename);
			if (File.Exists(path))
			{
				using (CatReader catReader = new CatReader(new FileStream(path, FileMode.Open, FileAccess.Read), fs.CreateDeobfuscator()))
				{
					for (int i = 0; i < catReader.ResourceCount; i++)
					{
						CatResourceEntry value = catReader.ReadResourceEntry();
						value.ArchiveIndex = AddCas(filename, value.ArchiveIndex);
						if (value.LogicalOffset == 0 && !resourceEntries.ContainsKey(value.Sha1))
						{
							resourceEntries.Add(value.Sha1, value);
						}
					}
					for (int j = 0; j < catReader.EncryptedCount; j++)
					{
						CatResourceEntry value2 = catReader.ReadEncryptedEntry();
						value2.ArchiveIndex = AddCas(filename, value2.ArchiveIndex);
						if (value2.LogicalOffset == 0 && !resourceEntries.ContainsKey(value2.Sha1))
						{
							resourceEntries.Add(value2.Sha1, value2);
						}
					}
					for (int k = 0; k < catReader.PatchCount; k++)
					{
						CatPatchEntry value3 = catReader.ReadPatchEntry();
						if (!patchEntries.ContainsKey(value3.Sha1))
						{
							patchEntries.Add(value3.Sha1, value3);
						}
					}
				}
			}
		}

		private void LoadDas()
		{
			string path = fs.ResolvePath("das.dal");
			new List<Tuple<string, int>>();
			using (NativeReader nativeReader = new NativeReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
			{
				int num = nativeReader.ReadByte();
				for (int i = 0; i < num; i++)
				{
					string str = nativeReader.ReadSizedString(64);
					int num2 = nativeReader.ReadInt();
					string text = fs.ResolvePath("das_" + str + ".das");
					Fnv1.HashString(text);
					casFiles.Add(i, text);
					using (NativeReader nativeReader2 = new NativeReader(new FileStream(text, FileMode.Open, FileAccess.Read)))
					{
						long num3 = num2 * 24;
						for (int j = 0; j < num2; j++)
						{
							Sha1 sha = nativeReader2.ReadSha1();
							uint num4 = nativeReader2.ReadUInt();
							long num5 = num3;
							num3 += num4;
							CatResourceEntry catResourceEntry = default(CatResourceEntry);
							catResourceEntry.Sha1 = sha;
							catResourceEntry.Offset = (uint)num5;
							catResourceEntry.Size = num4;
							catResourceEntry.ArchiveIndex = casFiles.Count - 1;
							CatResourceEntry value = catResourceEntry;
							resourceEntries.Add(sha, value);
						}
					}
				}
			}
		}

		private int AddCas(string catPath, int archiveIndex)
		{
			string text = catPath.Substring(0, catPath.Length - 7) + "cas_" + archiveIndex.ToString("d2") + ".cas";
			int num = Fnv1.HashString(text);
			if (!casFiles.ContainsKey(num))
			{
				casFiles.Add(num, fs.ResolvePath(text));
			}
			return num;
		}

		private void WriteToLog(string text, params object[] vars)
		{
			if (logger != null)
			{
				logger.Log(text, vars);
			}
		}
	}
}
