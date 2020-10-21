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
					foreach (string sbName in catalogInfoItem.SuperBundles.Keys)
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
			}
		}
	}


}
