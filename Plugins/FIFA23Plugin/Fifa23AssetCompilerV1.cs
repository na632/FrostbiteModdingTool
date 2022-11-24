using Frostbite.FileManagers;
using FrostbiteSdk.Frostbite.FileManagers;
using Frosty.Hash;
using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Frostbite.Compilers;
using FrostySdk.Frostbite.PluginInterfaces;
using FrostySdk.FrostySdk.Managers;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using ModdingSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Xps.Serialization;

namespace FIFA23Plugin
{

    /// <summary>
    /// FIFA 23 Asset Compiler. Solid and works. Uses .cache file to determine what needs editing
    /// Linked to FIFA21BundleAction
    /// </summary>
    public class Fifa23AssetCompilerV1 : BaseAssetCompiler, IAssetCompiler
    {
        /// <summary>
        /// This is run AFTER the compilation of the fbmod into resource files ready for the Actions to TOC/SB/CAS to be taken
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="logger"></param>
        /// <param name="frostyModExecuter">Frosty Mod Executer object</param>
        /// <returns></returns>
        public override bool Compile(FileSystem fs, ILogger logger, ModExecutor modExecuter)
        {
            base.Compile(fs, logger, modExecuter);

            DateTime dtStarted = DateTime.Now;
            if (!ProfileManager.IsFIFA23DataVersion())
            {
                logger.Log("[ERROR] Wrong compiler used for Game");
                return false;
            }

            bool result = false;
            ErrorCounts.Clear();
            ErrorCounts.Add(ModType.EBX, 0);
            ErrorCounts.Add(ModType.RES, 0);
            ErrorCounts.Add(ModType.CHUNK, 0);
            ModExecutor.UseModData = true;

            result = RunModDataCompiler(logger);

            logger.Log($"Compiler completed in {(DateTime.Now - dtStarted).ToString(@"mm\:ss")}");
            return result;
        }

        private bool RunModDataCompiler(ILogger logger)
        {
            if (!Directory.Exists(FileSystem.Instance.BasePath))
                throw new DirectoryNotFoundException($"Unable to find the correct base path directory of {FileSystem.Instance.BasePath}");

            Directory.CreateDirectory(FileSystem.Instance.BasePath + ModDirectory + "\\Data");
            Directory.CreateDirectory(FileSystem.Instance.BasePath + ModDirectory + "\\Patch");

            logger.Log("Copying files from Data to ModData/Data");
            CopyDataFolder(FileSystem.Instance.BasePath + "\\Data\\", FileSystem.Instance.BasePath + ModDirectory + "\\Data\\", logger);
            logger.Log("Copying files from Patch to ModData/Patch");
            CopyDataFolder(FileSystem.Instance.BasePath + "\\Patch\\", FileSystem.Instance.BasePath + ModDirectory + "\\Patch\\", logger);

            return Run();
        }


        private ModExecutor parent => ModExecuter;

        public bool GameWasPatched => parent.GameWasPatched;

        public List<ChunkAssetEntry> AddedChunks = new List<ChunkAssetEntry>();

        Dictionary<string, DbObject> SbToDbObject = new Dictionary<string, DbObject>();

        private void DeleteBakFiles(string path)
        {
            foreach (var file in Directory.EnumerateFiles(path))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.FullName.Contains(".bak"))
                {
                    fileInfo.Delete();
                }
            }

            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                DeleteBakFiles(dir);
            }
        }

        public bool Run()
        {
            parent.Logger.Log("Loading files to know what to change.");
            if (parent.modifiedEbx.Count == 0 && parent.modifiedRes.Count == 0 && parent.ModifiedChunks.Count == 0 && parent.modifiedLegacy.Count == 0)
                return true;

            if (AssetManager.Instance == null)
            {
                CacheManager buildCache = new CacheManager();
                buildCache.LoadData(ProfileManager.ProfileName, parent.GamePath, parent.Logger, false, true);
            }

            if (!ModExecutor.UseModData && GameWasPatched)
            {
                DeleteBakFiles(parent.GamePath);
            }


            parent.Logger.Log("Retreiving list of Modified CAS Files.");
            var dictOfModsToCas = GetModdedCasFiles();

            // Force delete of Live Tuning Updates
            if (dictOfModsToCas.Any(x => x.Value.Any(y => y.NamePath.Contains("gp_", StringComparison.OrdinalIgnoreCase))))
                parent.DeleteLiveUpdates = true;

            parent.Logger.Log("Modifying TOC Chunks.");
            ModifyTOCChunks();

            var entriesToNewPosition = WriteNewDataToCasFiles(dictOfModsToCas);
            if (entriesToNewPosition == null || entriesToNewPosition.Count == 0)
                return true;

            bool result = WriteNewDataChangesToSuperBundles(entriesToNewPosition);
            result = WriteNewDataChangesToSuperBundles(entriesToNewPosition, "native_data");
            return result;

        }

        /// <summary>
        /// Writes mod data to the CAS files the Asset Entry originally belongs to.
        /// </summary>
        /// <param name="dictOfModsToCas"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected Dictionary<AssetEntry, (long, int, int, Sha1)> WriteNewDataToCasFiles(Dictionary<string, List<ModdedFile>> dictOfModsToCas)
        {
            if (dictOfModsToCas == null || dictOfModsToCas.Count == 0)
                return null;

            if (ErrorCounts.Count > 0)
            {
                if (ErrorCounts[ModType.EBX] > 0)
                    parent.Logger.Log("EBX ERRORS:: " + ErrorCounts[ModType.EBX]);
                if (ErrorCounts[ModType.RES] > 0)
                    parent.Logger.Log("RES ERRORS:: " + ErrorCounts[ModType.RES]);
                if (ErrorCounts[ModType.CHUNK] > 0)
                    parent.Logger.Log("Chunk ERRORS:: " + ErrorCounts[ModType.CHUNK]);
            }

            Dictionary<AssetEntry, (long, int, int, Sha1)> EntriesToNewPosition = new Dictionary<AssetEntry, (long, int, int, Sha1)>();

            foreach (var item in dictOfModsToCas)
            {

                string casPath = FileSystem.Instance.ResolvePath(item.Key, ModExecutor.UseModData);


                if (ModExecutor.UseModData && !casPath.Contains("ModData"))
                {
                    throw new Exception($"WRONG CAS PATH GIVEN! {casPath}");
                }

                if (!ModExecutor.UseModData)
                {
                    casPath = casPath.Replace("ModData\\", "", StringComparison.OrdinalIgnoreCase);
                }

                Debug.WriteLine($"Modifying CAS file - {casPath}");

                using (NativeWriter nwCas = new NativeWriter(new FileStream(casPath, FileMode.Open)))
                {
                    foreach (var modItem in item.Value.OrderBy(x => x.NamePath))
                    {
                        nwCas.Position = nwCas.Length;
                        byte[] data = new byte[0];
                        AssetEntry originalEntry = modItem.OriginalEntry;
                        if (originalEntry == null)
                            continue;

                        if (originalEntry != null && parent.archiveData.ContainsKey(modItem.Sha1))
                        {
                            data = parent.archiveData[modItem.Sha1].Data;
                        }
                        else
                        {
                            //parent.Logger.LogError($"Unable to find original archive data for {modItem.NamePath}");
                            continue;
                        }

                        if (data.Length == 0)
                        {
                            parent.Logger.LogError($"Unable to find any data for {modItem.NamePath}");
                            continue;
                        }

                        AssetEntry modifiedAsset = null;

                        var origSize = 0;
                        var positionOfData = nwCas.Position;
                        // write the new data to end of the file (this should be fine)
                        nwCas.Write(data);

                        switch (modItem.ModType)
                        {
                            case ModType.EBX:
                                modifiedAsset = parent.modifiedEbx[modItem.NamePath];
                                break;
                            case ModType.RES:
                                modifiedAsset = parent.modifiedRes[modItem.NamePath];
                                break;
                            case ModType.CHUNK:
                                modifiedAsset = parent.ModifiedChunks[Guid.Parse(modItem.NamePath)];
                                break;
                        }

                        if (modifiedAsset != null && modifiedAsset is ChunkAssetEntry)
                        {
                            var chunkModAsset = modifiedAsset as ChunkAssetEntry;
                            if (chunkModAsset.ModifiedEntry != null && chunkModAsset.ModifiedEntry.AddToChunkBundle)
                                continue;
                        }

                        origSize = Convert.ToInt32(modifiedAsset.OriginalSize);

                        if (origSize == 0)
                        {
                            if (modifiedAsset is ChunkAssetEntry cae && cae.LogicalSize > 0)
                            {
                                origSize = (int)cae.LogicalSize;
                                modifiedAsset.OriginalSize = origSize;
                            }
                            else
                            {
                                //parent.Logger.LogWarning($"OriginalSize is missing or 0 on {modItem.NamePath}, attempting calculation by reading it.");
                                using (var stream = new MemoryStream(data))
                                {
                                    var out_data = new CasReader(new MemoryStream(data)).Read();
                                    origSize = out_data.Length;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(originalEntry.TOCFileLocation))
                            continue;

                        EntriesToNewPosition.Add(originalEntry, (positionOfData, data.Length, origSize, modItem.Sha1));
                    }

                }
            }

            return EntriesToNewPosition;
        }

       
        protected bool WriteNewDataChangesToSuperBundles(Dictionary<AssetEntry, (long, int, int, Sha1)> EntriesToNewPosition, string directory = "native_patch") 
        {
            if (EntriesToNewPosition == null)
            {
                parent.Logger.LogError($"Unable to find any entries to process");
                return false;
            }

            // ------------------------------------------------------------------------------
            // Step 1. Discovery phase. Find the Edited Bundles and what TOC/SB they affect
            //
            var editedBundles = EntriesToNewPosition.SelectMany(x => x.Key.Bundles).Distinct();
            var groupedByTOCSB = new Dictionary<string, List<KeyValuePair<AssetEntry, (long, int, int, Sha1)>>>();
            int sbIndex = -1;
            foreach (var catalogInfo in FileSystem.Instance.EnumerateCatalogInfos())
            {
                foreach (
                    string sbKey in catalogInfo.SuperBundles.Keys
                    )
                {
                    sbIndex++;
                    string tocFileKey = sbKey;
                    if (catalogInfo.SuperBundles[sbKey])
                    {
                        tocFileKey = sbKey.Replace("win32", catalogInfo.Name);
                    }

                    var nativePathToTOCFile = $"{directory}/{tocFileKey}.toc";
                    var actualPathToTOCFile = FileSystem.Instance.ResolvePath(nativePathToTOCFile, ModExecutor.UseModData);
                    using TOCFile tocFile = new TOCFile(nativePathToTOCFile, false, false, true, sbIndex, true);

                    var hashedEntries = tocFile.BundleEntries.Select(x => Fnv1a.HashString(x.Name));
                    if (hashedEntries.Any(x => editedBundles.Contains(x)))
                    {
                        if (!groupedByTOCSB.ContainsKey(nativePathToTOCFile))
                            groupedByTOCSB.Add(nativePathToTOCFile, new List<KeyValuePair<AssetEntry, (long, int, int, Sha1)>>());

                        var editedBundleEntries = EntriesToNewPosition.Where(x => x.Key.Bundles.Any(y => hashedEntries.Contains(y))).ToArray();
                        foreach (var item in editedBundleEntries)
                            groupedByTOCSB[nativePathToTOCFile].Add(item);
                    }
                }
            }

            List<Task> tasks = new List<Task>();

            var assetBundleToCAS = new Dictionary<string, List<(AssetEntry, DbObject)>>();

            // ------------------------------------------------------------------------------
            // Step 2. Apply bundle changes to TOC Files
            //
            foreach (var tocGroup in groupedByTOCSB)
            {
                var tocPath = tocGroup.Key;
                if (string.IsNullOrEmpty(tocPath))
                    continue;

                //tasks.Add(Task.Run(() =>
                //{
                    using TOCFile tocFile = new TOCFile(tocPath, false, false, true, sbIndex, false);
                    {
                        DbObject dboOriginal = tocFile.TOCObjects;
                    if (dboOriginal == null)
                        continue;


                        {
                            parent.Logger.Log($"Processing: {tocPath}");
                            var origEbxBundles = dboOriginal.List
                            .Where(x => ((DbObject)x).HasValue("ebx"))
                            .Select(x => ((DbObject)x).GetValue<DbObject>("ebx"))
                            .Where(x => x.List != null && x.List.Any(y => parent.modifiedEbx.ContainsKey(((DbObject)y).GetValue<string>("name"))))
                            .ToList();

                            var origResBundles = dboOriginal.List
                            .Where(x => ((DbObject)x).HasValue("res"))
                            .Select(x => ((DbObject)x).GetValue<DbObject>("res"))
                            .Where(x => x.List != null && x.List.Any(y => parent.modifiedRes.ContainsKey(((DbObject)y).GetValue<string>("name"))))
                            .ToList();

                            var origChunkBundles = dboOriginal.List
                            .Where(x => ((DbObject)x).HasValue("chunks"))
                            .Select(x => ((DbObject)x).GetValue<DbObject>("chunks"))
                            .Where(x => x.List != null && x.List.Any(y => parent.ModifiedChunks.ContainsKey(((DbObject)y).GetValue<Guid>("id"))))
                            .ToList();

                            if (origEbxBundles.Count == 0 && origResBundles.Count == 0 && origChunkBundles.Count == 0)
                                continue;
                                //return;

                        var resolvedTocPath = FileSystem.Instance.ResolvePath(tocPath, true);
                            using (NativeWriter nw_toc = new NativeWriter(new FileStream(resolvedTocPath, FileMode.Open)))
                            {
                                foreach (var assetBundle in tocGroup.Value)
                                {
                                    DbObject origDbo = null;
                                    var casPath = string.Empty;
                                    if (assetBundle.Key is EbxAssetEntry)
                                    {
                                        foreach (DbObject dbInBundle in origEbxBundles)
                                        {
                                            origDbo = (DbObject)dbInBundle.List.SingleOrDefault(z => ((DbObject)z)["name"].ToString() == assetBundle.Key.Name);
                                            if (origDbo != null)
                                                break;
                                        }

                                        if (origDbo != null)
                                        {
                                            casPath = origDbo.GetValue<string>("ParentCASBundleLocation");
                                        }
                                    }

                                    if (assetBundle.Key is ResAssetEntry)
                                    {

                                        foreach (DbObject dbInBundle in origResBundles)
                                        {
                                            origDbo = (DbObject)dbInBundle.List.SingleOrDefault(z => ((DbObject)z)["name"].ToString() == assetBundle.Key.Name);
                                            if (origDbo != null)
                                                break;
                                        }

                                        if (origDbo != null)
                                        {
                                            casPath = origDbo.GetValue<string>("ParentCASBundleLocation");
                                        }
                                    }

                                    if (assetBundle.Key is ChunkAssetEntry)
                                    {

                                        foreach (DbObject dbInBundle in origChunkBundles)
                                        {
                                            origDbo = (DbObject)dbInBundle.List.SingleOrDefault(z => ((DbObject)z)["id"].ToString() == assetBundle.Key.Name);
                                            if (origDbo != null)
                                                break;
                                        }

                                        if (origDbo != null)
                                        {
                                            casPath = origDbo.GetValue<string>("ParentCASBundleLocation");
                                        }
                                    }

                                    if (origDbo != null && !string.IsNullOrEmpty(casPath))
                                    {
                                        var positionOfNewData = assetBundle.Value.Item1;
                                        var sizeOfData = assetBundle.Value.Item2;
                                        var originalSizeOfData = assetBundle.Value.Item3;
                                        var sha = assetBundle.Value.Item4;

                                        int sb_cas_size_position = assetBundle.Key.SB_CAS_Size_Position;
                                        var sb_cas_offset_position = assetBundle.Key.SB_CAS_Offset_Position;
                                        nw_toc.BaseStream.Position = sb_cas_offset_position;
                                        nw_toc.Write((uint)positionOfNewData, Endian.Big);
                                        nw_toc.Write((uint)sizeOfData, Endian.Big);

                                        if (!assetBundleToCAS.ContainsKey(casPath))
                                            assetBundleToCAS.Add(casPath, new List<(AssetEntry, DbObject)>());

                                        assetBundleToCAS[casPath].Add((assetBundle.Key, origDbo));
                                    }
                                }

                                



                            }

                            using (var fsTocSig = new FileStream(resolvedTocPath, FileMode.Open))
                                TOCFile.RebuildTOCSignatureOnly(fsTocSig);
                        }
                        parent.Logger.Log($"Processing Complete: {tocPath}");
                        if (dboOriginal != null)
                        {
                            dboOriginal = null;
                        }

                        //GC.Collect();
                        //GC.WaitForPendingFinalizers();
                    }


                //}));
            }
            // Wait for above tasks to complete
            Task.WaitAll(tasks.ToArray());
            //
            // ------------------------------------------------------------------------------


            // ------------------------------------------------------------------------------
            // Step 3. Apply bundle changes to CAS Files
            //
            foreach (var tocGroup in groupedByTOCSB)
            {
                foreach (var abtc in assetBundleToCAS)
                {
                    var resolvedCasPath = FileSystem.Instance.ResolvePath(abtc.Key, ModExecutor.UseModData);
                    using (var nwCas = new NativeWriter(new FileStream(resolvedCasPath, FileMode.Open)))
                    {
                        foreach (var assetEntry in abtc.Value)
                        {
                            var assetBundle = tocGroup.Value.LastOrDefault(x => x.Key.Equals(assetEntry.Item1));

                            WriteChangesToSuperBundle(assetEntry.Item2, nwCas, assetBundle);
                        }
                    }
                }
            }

            return true;
        }

    }



}
