using Frosty.Hash;
using FrostySdk;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static FrostySdk.Managers.AssetManager;

namespace BF2042Plugin
{

	public class AssetLoader_BF2042 : IAssetLoader
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
			public static int BundleItemIndex = 0;

			public string Name { get; set; }

			public long Offset { get; set; }

			public long TOCSizePosition { get; set; }

			public long Size { get; set; }

			public long TocOffset { get; set; }

			public int CasIndex { get; internal set; }
			public int Unk { get; internal set; }

			public int TocBundleIndex { get; set; }

			public override string ToString()
            {
				return $"Offset:{Offset}-Size:{Size}-Index:{TocBundleIndex}";
			}

        }

		public void LoadData(AssetManager parent, BinarySbDataHelper helper)
		{
			if (parent != null && parent.fs.Catalogs != null && parent.fs.Catalogs.Count() > 0)
			{
				foreach (Catalog catalogInfoItem in parent.fs.EnumerateCatalogInfos())
				{
					foreach (string sbName in catalogInfoItem.SuperBundles.Where(x => !x.Value).Select(x => x.Key))
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
								,
								CatalogInfo = catalogInfoItem
							});
							sbIndex = parent.superBundles.Count - 1;
						}
						// Test to fix Arsenal Kit -- CareerSBA is useless anyway
						if (sbName.Contains("careersba", StringComparison.OrdinalIgnoreCase))
							continue;

						if (sbName.Contains("storycharsb", StringComparison.OrdinalIgnoreCase))
							continue;

						if (sbName.Contains("story", StringComparison.OrdinalIgnoreCase))
							continue;

						parent.logger.Log($"Loading data ({sbName})");
						string tocFile = sbName.Replace("win32", catalogInfoItem.Name).Replace("cs/", "");
						if (parent.fs.ResolvePath("native_data/" + tocFile + ".toc") == "")
						{
							tocFile = sbName;
						}
						List<BaseBundleInfo> listOfBundles_Data = new List<BaseBundleInfo>();
						List<BaseBundleInfo> listOfBundles_Patch = new List<BaseBundleInfo>();
						var tocFileRAW = $"native_data/{tocFile}.toc";
						string tocFileLocation = parent.fs.ResolvePath(tocFileRAW);
						if (!string.IsNullOrEmpty(tocFileLocation) && File.Exists(tocFileLocation))
						{
							TocSbReader_BF2042 tocSbReader = new TocSbReader_BF2042();
							var dbObjects = tocSbReader.Read(tocFileLocation, sbIndex, new BinarySbDataHelper(parent), sbName, true, tocFileRAW);
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
						//else
						//{
						//	parent.logger.LogError($"Unable to find {tocFileLocation}");
						//}
					}
				}
			}
		}

		public void LoadPatch(AssetManager parent, BinarySbDataHelper helper)
		{
			if (parent != null && parent.fs.Catalogs != null && parent.fs.Catalogs.Count() > 0)
			{
				foreach (Catalog catalogInfoItem in parent.fs.EnumerateCatalogInfos())
				{
					foreach (string sbName in catalogInfoItem.SuperBundles.Where(x => !x.Value).Select(x => x.Key))
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
								,
								CatalogInfo = catalogInfoItem
							});
							sbIndex = parent.superBundles.Count - 1;
						}

						// Test to fix Arsenal Kit -- CareerSBA is useless anyway
						if (sbName.Contains("careersba", StringComparison.OrdinalIgnoreCase))
							continue;

						if (sbName.Contains("storycharsb", StringComparison.OrdinalIgnoreCase))
							continue;

						if (sbName.Contains("story", StringComparison.OrdinalIgnoreCase))
							continue;

						parent.logger.Log($"Loading data ({sbName})");
						string tocFile = sbName.Replace("win32", catalogInfoItem.Name).Replace("cs/", "");
						if (parent.fs.ResolvePath("native_patch/" + tocFile + ".toc") == "")
						{
							tocFile = sbName;
						}

						var tocFileRAW = $"native_patch/{tocFile}.toc";
						var tocFileLocation = parent.fs.ResolvePath(tocFileRAW);
						if (!string.IsNullOrEmpty(tocFileLocation) && File.Exists(tocFileLocation))
						{
							TocSbReader_BF2042 tocSbReader = new TocSbReader_BF2042();
							var dbObjects = tocSbReader.Read(tocFileLocation, sbIndex, new BinarySbDataHelper(parent), sbName, false, tocFileRAW);
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
						else
						{
							parent.logger.LogError($"Unable to find {tocFileLocation}");
						}

					}
				}
			}
		}

		public void Load(AssetManager parent, BinarySbDataHelper helper)
		{
			LoadData(parent, helper);
			LoadPatch(parent, helper);
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
