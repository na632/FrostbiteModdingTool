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

                        //if (sbName.Contains("story", StringComparison.OrdinalIgnoreCase))
                        //    continue;

                        if (sbName.Contains("storysba", StringComparison.OrdinalIgnoreCase))
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
							TocSbReader_FIFA21 tocSbReader_FIFA21 = new TocSbReader_FIFA21();
							var dbObjects = tocSbReader_FIFA21.Read(tocFileLocation, sbIndex, sbName, true, tocFileRAW);
							if (dbObjects != null)
							{
								foreach (DbObject @object in dbObjects.Where(x => x != null))
								{
									parent.ProcessBundleEbx(@object, parent.Bundles.Count - 1, helper);
									parent.ProcessBundleRes(@object, parent.Bundles.Count - 1, helper);
									parent.ProcessBundleChunks(@object, parent.Bundles.Count - 1, helper);
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
                        //if (sbName.Contains("careersba", StringComparison.OrdinalIgnoreCase))
                        //	continue;

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
							TocSbReader_FIFA21 tocSbReader_FIFA21 = new TocSbReader_FIFA21();
							var dbObjects = tocSbReader_FIFA21.Read(tocFileLocation, sbIndex, sbName, false, tocFileRAW);
							if (dbObjects != null)
							{

								foreach (DbObject @object in dbObjects.Where(x => x != null))
								{
									parent.ProcessBundleEbx(@object, parent.Bundles.Count - 1, helper);
									parent.ProcessBundleRes(@object, parent.Bundles.Count - 1, helper);
									parent.ProcessBundleChunks(@object, parent.Bundles.Count - 1, helper);
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
