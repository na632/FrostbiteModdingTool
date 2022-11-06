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

namespace FIFA22Plugin
{

	public class AssetLoader_Fifa22 : IAssetLoader, IDisposable
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

		public void LoadData(AssetManager parent, BinarySbDataHelper helper, string folder = "native_data/")
		{
			if (parent != null && parent.fs.Catalogs != null && parent.fs.Catalogs.Count() > 0)
			{
				foreach (Catalog catalogInfoItem in parent.fs.EnumerateCatalogInfos().OrderBy(x=> x.PersistentIndex.HasValue ? x.PersistentIndex : 0))
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

						parent.logger.Log($"Loading data ({sbName})");
						string tocFile = sbName.Replace("win32", catalogInfoItem.Name).Replace("cs/", "");
						if (parent.fs.ResolvePath(folder + tocFile + ".toc") == "")
						{
							tocFile = sbName;
						}
						List<BaseBundleInfo> listOfBundles_Data = new List<BaseBundleInfo>();
						List<BaseBundleInfo> listOfBundles_Patch = new List<BaseBundleInfo>();
						var tocFileRAW = $"{folder}{tocFile}.toc";
						string tocFileLocation = parent.fs.ResolvePath(tocFileRAW);
						if (!string.IsNullOrEmpty(tocFileLocation) && File.Exists(tocFileLocation))
						{
							TOCFile tocFile2 = new TOCFile(tocFileRAW, true, true, false);// new MemoryStream(File.ReadAllBytes(tocFileLocation)), true, true);
                            tocFile2.Dispose();
                        }
					}
				}
			}
		}

		public void LoadPatch(AssetManager parent, BinarySbDataHelper helper)
		{
			LoadData(parent, helper, "native_patch/");
		}

		public void Load(AssetManager parent, BinarySbDataHelper helper)
		{
			LoadData(parent, helper);
			LoadPatch(parent, helper);

		}

		public void Dispose()
		{
			AllDbObjects.Clear();
        }
	}


}
