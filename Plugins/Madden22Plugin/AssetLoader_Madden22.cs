﻿using FrostySdk;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Madden22Plugin
{

    public class AssetLoader_Madden22 : IAssetLoader
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
            if (parent != null && parent.FileSystem.Catalogs != null && parent.FileSystem.Catalogs.Count() > 0)
            {
                foreach (Catalog catalogInfoItem in parent.FileSystem.EnumerateCatalogInfos())
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

                        parent.Logger.Log($"Loading data ({sbName})");
                        string tocFile = sbName.Replace("win32", catalogInfoItem.Name).Replace("cs/", "");
                        if (parent.FileSystem.ResolvePath(folder + tocFile + ".toc") == "")
                        {
                            tocFile = sbName;
                        }
                        List<BaseBundleInfo> listOfBundles_Data = new List<BaseBundleInfo>();
                        List<BaseBundleInfo> listOfBundles_Patch = new List<BaseBundleInfo>();
                        var tocFileRAW = $"{folder}{tocFile}.toc";
                        string tocFileLocation = parent.FileSystem.ResolvePath(tocFileRAW);
                        if (!string.IsNullOrEmpty(tocFileLocation) && File.Exists(tocFileLocation))
                        {
                            TocSbReader_Madden22 tocSbReader = new TocSbReader_Madden22();
                            var dbObjectsToProcess = tocSbReader.Read(tocFileLocation, sbIndex, new BinarySbDataHelper(parent), sbName, true, tocFileRAW);
                            if (dbObjectsToProcess != null)
                            {
                                foreach (DbObject @object in dbObjectsToProcess.Where(x => x != null))
                                {
                                    parent.ProcessBundleEbx(@object, parent.Bundles.Count - 1, helper);
                                    parent.ProcessBundleRes(@object, parent.Bundles.Count - 1, helper);
                                    parent.ProcessBundleChunks(@object, parent.Bundles.Count - 1, helper);
                                }
                            }
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
