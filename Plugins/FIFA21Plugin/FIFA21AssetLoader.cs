using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static FrostySdk.Managers.AssetManager;

namespace FIFA21Plugin
{

	public class FIFA21AssetLoader : IAssetLoader
	{
		public List<DbObject> AllDbObjects = new List<DbObject>();
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

		public class BaseBundleInfo
		{
			public string Name;

			public long Offset;

			public long Size;

			public long TocOffset;

            public int CasIndex { get; internal set; }
        }

		public void Load(AssetManager parent, BinarySbDataHelper helper)
		{
			if (parent != null && parent.fs.Catalogs != null && parent.fs.Catalogs.Count() > 0)
			{
				foreach (CatalogInfo catalogInfoItem in parent.fs.EnumerateCatalogInfos())
				{
					foreach (string sbName in catalogInfoItem.SuperBundles.Where(x=>!x.Value).Select(x=>x.Key))
					{
						SuperBundleEntry superBundleEntry = parent.superBundles.Find((SuperBundleEntry a) => a.Name == sbName);
						int sbIndex = -1;
						if (superBundleEntry != null)
						{
							sbIndex = parent.superBundles.IndexOf(superBundleEntry);
						}
						else
						{
							parent.superBundles.Add(new SuperBundleEntry
							{
								Name = sbName
								, CatalogInfo = catalogInfoItem
							});
							sbIndex = parent.superBundles.Count - 1;
						}
						parent.logger.Log($"Loading data ({sbName})");
						string tocFile = sbName.Replace("win32", catalogInfoItem.Name).Replace("cs/", "");
						if (parent.fs.ResolvePath("native_data/" + tocFile + ".toc") == "")
						{
							tocFile = sbName;
						}
						List<BaseBundleInfo> listOfBundles_Data = new List<BaseBundleInfo>();
						List<BaseBundleInfo> listOfBundles_Patch = new List<BaseBundleInfo>();
						string tocFileLocation = parent.fs.ResolvePath($"native_data/{tocFile}.toc");
						if (!string.IsNullOrEmpty(tocFileLocation))
						{
							TocSbReader_FIFA21 tocSbReader_FIFA21 = new TocSbReader_FIFA21();
							var dbObjects = tocSbReader_FIFA21.Read(tocFileLocation, sbIndex, new BinarySbDataHelper(parent), sbName, true);
							if (dbObjects != null)
							{



								foreach (DbObject @object in dbObjects.Where(x => x != null))
								{
									parent.ProcessBundleEbx(@object, parent.bundles.Count - 1, helper);
									parent.ProcessBundleRes(@object, parent.bundles.Count - 1, helper);
									parent.ProcessBundleChunks(@object, parent.bundles.Count - 1, helper);
								}


							}
						}


						tocFileLocation = parent.fs.ResolvePath($"native_patch/{tocFile}.toc");
						if (!string.IsNullOrEmpty(tocFileLocation))
						{

							TocSbReader_FIFA21 tocSbReader_FIFA21 = new TocSbReader_FIFA21();
							var dbObjects = tocSbReader_FIFA21.Read(tocFileLocation, sbIndex, new BinarySbDataHelper(parent), sbName, false);
							if (dbObjects != null)
							{

								foreach (DbObject @object in dbObjects.Where(x => x != null))
								{
									parent.ProcessBundleEbx(@object, parent.bundles.Count - 1, helper);
									parent.ProcessBundleRes(@object, parent.bundles.Count - 1, helper);
									parent.ProcessBundleChunks(@object, parent.bundles.Count - 1, helper);
								}

								AllDbObjects.AddRange(dbObjects);
							}
						}

					}
				}


				// CAS SEARCHING

				/*
				List<string> casFilesToSearch = new List<string>()
				{
					parent.fs.BasePath + @"\Data\Win32\superbundlelayout\fifa_installpackage_00\cas_01.cas",
					parent.fs.BasePath + @"\Data\Win32\superbundlelayout\fifa_installpackage_03\cas_03.cas"

				};
				foreach (var path in casFilesToSearch) 
				{

					using (NativeReader nr_cas = new NativeReader(
						new FileStream(path, FileMode.Open, FileAccess.Read)
						)
						)
					{

						List<int> PositionOfReadableItems = SearchBytePattern(new byte[] { 0xD6, 0x8E, 0x79, 0x9D }, nr_cas.ReadToEnd()).ToList();

						List<DbObject> dbObjects = new List<DbObject>();
						nr_cas.Position = 0;
						foreach (var pos in PositionOfReadableItems)
						{
							// go back 4 from the magic
							var actualPos = pos - 4;

							using (
								NativeReader inner_reader = new NativeReader(
								nr_cas.CreateViewStream(actualPos, nr_cas.Length - actualPos)
								))
							{
								SBFile sbFile = new SBFile();
								DbObject obj = new DbObject();
								sbFile.BinaryRead_FIFA21(new FIFA21AssetLoader.BaseBundleInfo()
									, ref obj, inner_reader, false);
								foreach (DbObject ebx in obj.GetValue<DbObject>("ebx"))
								{
								}

								dbObjects.Add(obj);
							}
						}

						if (dbObjects != null)
						{

							foreach (DbObject @object in dbObjects.Where(x => x != null))
							{
								parent.ProcessBundleEbx(@object, parent.bundles.Count - 1, helper);
								parent.ProcessBundleRes(@object, parent.bundles.Count - 1, helper);
								parent.ProcessBundleChunks(@object, parent.bundles.Count - 1, helper);
							}

							AllDbObjects.AddRange(dbObjects);
						}
					}
				}
				*/

			}

			
		}
		static public List<int> SearchBytePattern(byte[] pattern, byte[] bytes)
		{
			List<int> positions = new List<int>();
			int patternLength = pattern.Length;
			int totalLength = bytes.Length;
			byte firstMatchByte = pattern[0];
			for (int i = 0; i < totalLength; i++)
			{
				if (firstMatchByte == bytes[i] && totalLength - i >= patternLength)
				{
					byte[] match = new byte[patternLength];
					Array.Copy(bytes, i, match, 0, patternLength);
					if (match.SequenceEqual<byte>(pattern))
					{
						positions.Add(i);
						i += patternLength - 1;
					}
				}
			}
			return positions;
		}
	}


}
