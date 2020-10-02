using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static FrostySdk.Managers.AssetManager;

namespace FIFA21Plugin
{

	public class FIFA21AssetLoader : IAssetLoader
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
			foreach (CatalogInfo item5 in parent.fs.EnumerateCatalogInfos())
			{
				foreach (string sbName in item5.SuperBundles.Keys)
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
					parent.logger.Log($"Loading data ({sbName})");
					string tocFile = sbName.Replace("win32", item5.Name).Replace("cs/", "");
					if (parent.fs.ResolvePath("native_data/" + tocFile + ".toc") == "")
					{
						tocFile = sbName;
					}
					List<BaseBundleInfo> listOfBundles_Data = new List<BaseBundleInfo>();
					List<BaseBundleInfo> listOfBundles_Patch = new List<BaseBundleInfo>();
					string tocFileLocation = parent.fs.ResolvePath($"native_data/{tocFile}.toc");

					TocSbReader_FIFA21 tocSbReader_FIFA21 = new TocSbReader_FIFA21();
					tocSbReader_FIFA21.Read(tocFileLocation, 0, new BinarySbDataHelper(parent));
				}
			}
		}
	}


}
