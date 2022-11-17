using Frostbite.FileManagers;
using FrostbiteSdk.Frostbite.FileManagers;
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

namespace FIFA22Plugin
{

    /// <summary>
    /// FIFA 23 Asset Compiler. Solid and works. Uses .cache file to determine what needs editing
    /// Linked to FIFA21BundleAction
    /// </summary>
    public class Fifa22AssetCompilerV1 : BaseAssetCompiler, IAssetCompiler
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
            if (!ProfileManager.IsFIFA22DataVersion())
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

            //if (!FrostyModExecutor.UseModData)
            //{
            //    result = RunEADesktopCompiler(fs, logger, frostyModExecuter);
            //    return result;
            //}
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


        //}

        ///// <summary>
        ///// The actual builder/modifier of the files
        ///// </summary>
        //public class FIFA23BundleAction
        //{
        private ModExecutor parent => ModExecuter;

        //public static Dictionary<string, List<string>> CatalogCasFiles { get; } = new Dictionary<string, List<string>>();

        public bool GameWasPatched => parent.GameWasPatched;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inCatalogInfo"></param>
        /// <param name="inDoneEvent"></param>
        /// <param name="inParent"></param>
        //public FIFA21BundleAction(Catalog inCatalogInfo, FrostyModExecutor inParent)
        //{
        //    catalogInfo = inCatalogInfo;
        //    parent = inParent;
        //}

        //private readonly bool UseModData;

        //public FIFA23BundleAction(FrostyModExecutor inParent, bool useModData = true)
        //{
        //    parent = inParent;
        //    ErrorCounts.Add(ModType.EBX, 0);
        //    ErrorCounts.Add(ModType.RES, 0);
        //    ErrorCounts.Add(ModType.CHUNK, 0);
        //    UseModData = useModData;

        //    CatalogCasFiles.Clear();
        //    foreach (Catalog catalog in FileSystem.Instance.EnumerateCatalogInfos())
        //    {
        //        int casFileNumber = 1;
        //        List<string> casFileLocation = new List<string>();
        //        string path2 = Path.Combine(FileSystem.Instance.BasePath, "Patch", catalog.Name, $"cas_{casFileNumber:D2}.cas");
        //        while (File.Exists(path2))
        //        {
        //            casFileLocation.Add(path2);
        //            casFileNumber++;
        //            path2 = Path.Combine(FileSystem.Instance.BasePath, "Patch", catalog.Name, $"cas_{casFileNumber:D2}.cas");
        //        }
        //        CatalogCasFiles.Add(catalog.Name, casFileLocation);
        //    }

        //}

        

        


        public List<ChunkAssetEntry> AddedChunks = new List<ChunkAssetEntry>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>cas to Tuple of Sha1, Name, Type, IsAdded</returns>
        //private Dictionary<string, List<ModdedFile>> GetModdedCasFiles()
        //{
        //    // Handle Legacy first to generate modified chunks
        //    ProcessLegacyMods();
        //    // ------ End of handling Legacy files ---------

        //    Dictionary<string, List<ModdedFile>> casToMods = new Dictionary<string, List<ModdedFile>>();
        //    foreach(var mod in parent.ModifiedAssets)
        //    {
        //        AssetEntry originalEntry = null;
        //        if (mod.Value is EbxAssetEntry)
        //            originalEntry = AssetManager.Instance.GetEbxEntry(mod.Value.Name);
        //        else if (mod.Value is ResAssetEntry)
        //            originalEntry = AssetManager.Instance.GetResEntry(mod.Value.Name);
        //        else if (mod.Value is ChunkAssetEntry)
        //            originalEntry = AssetManager.Instance.GetChunkEntry(Guid.Parse(mod.Value.Name));

        //        if (originalEntry == null)
        //            continue;

        //        var casPath = originalEntry.ExtraData.CasPath;
        //        if (!casToMods.ContainsKey(casPath))
        //            casToMods.Add(casPath, new List<ModdedFile>());

        //        casToMods[casPath].Add(new ModdedFile(mod.Value.Sha1, mod.Value.Name, false, mod.Value, originalEntry));

        //    }

        //    return casToMods;
        //}

        //private void ProcessLegacyMods()
        //{
        //    List<Guid> ChunksToRemove = new List<Guid>();

        //    // -----------------------------------------------------------
        //    // process modified legacy chunks and make live changes
        //    if (parent.modifiedLegacy.Count > 0)
        //    {
        //        parent.Logger.Log($"Legacy :: {parent.modifiedLegacy.Count} Legacy files found. Modifying associated chunks");

        //        Dictionary<string, byte[]> legacyData = new Dictionary<string, byte[]>();
        //        var countLegacyChunksModified = 0;
        //        foreach (var modLegacy in parent.modifiedLegacy)
        //        {
        //            byte[] data = null;
        //            //if (modLegacy.Value.ModifiedEntry != null && modLegacy.Value.ModifiedEntry.Data != null)
        //            //    data = new CasReader(new MemoryStream(modLegacy.Value.ModifiedEntry.Data)).Read();
        //            //else 
        //            if (parent.archiveData.ContainsKey(modLegacy.Value.Sha1))
        //                data = parent.archiveData[modLegacy.Value.Sha1].Data;

        //            if (data != null)
        //            {
        //                legacyData.Add(modLegacy.Key, data);
        //            }
        //        }

        //        LegacyFileManager_FMTV2 legacyFileManager = AssetManager.Instance.GetLegacyAssetManager() as LegacyFileManager_FMTV2;
        //        if (legacyFileManager != null)
        //        {
        //            legacyFileManager.ModifyAssets(legacyData, true);

        //            var modifiedLegacyChunks = AssetManager.Instance.EnumerateChunks(true);
        //            foreach (var modLegChunk in modifiedLegacyChunks.Where(x => !parent.ModifiedChunks.ContainsKey(x.Id)))
        //            {
        //                if (modLegChunk.Id.ToString() == "f0ca4187-b95e-5153-a1eb-1e0a7fff6371")
        //                {

        //                }
        //                if (modLegChunk.Id.ToString() == "3e3ea546-1d18-6ed0-c3e4-2af56e6e8b6d")
        //                {

        //                }
        //                modLegChunk.Sha1 = modLegChunk.ModifiedEntry.Sha1;
        //                parent.ModifiedChunks.Add(modLegChunk.Id, modLegChunk);
        //                countLegacyChunksModified++;
        //            }

        //            var modifiedChunks = AssetManager.Instance.EnumerateChunks(true);
        //            foreach (var chunk in modifiedChunks)
        //            {
        //                if (parent.archiveData.ContainsKey(chunk.Sha1))
        //                    parent.archiveData[chunk.Sha1] = new ArchiveInfo() { Data = chunk.ModifiedEntry.Data };
        //                else
        //                    parent.archiveData.TryAdd(chunk.Sha1, new ArchiveInfo() { Data = chunk.ModifiedEntry.Data });
        //            }
        //            parent.Logger.Log($"Legacy :: Modified {countLegacyChunksModified} associated chunks");
        //        }
        //    }
        //}

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
            //try
            //{
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

            //ProcessLegacyMods();
            parent.Logger.Log("Modifying TOC Chunks.");
            ModifyTOCChunks();

            if (dictOfModsToCas == null || dictOfModsToCas.Count == 0)
                return true;

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
                    //parent.Logger.Log($"Modifying CAS file - {casPath}");


                    using (NativeWriter nwCas = new NativeWriter(new FileStream(casPath, FileMode.Open)))
                    {
                        foreach (var modItem in item.Value.OrderBy(x => x.NamePath))
                        {
                            nwCas.Position = nwCas.Length;
                            byte[] data = new byte[0];
                            AssetEntry originalEntry = modItem.OriginalEntry;
                            if (originalEntry == null)
                                continue;

                            if (originalEntry.SBFileLocation != null &&
                                (
                                    //originalEntry.SBFileLocation.Contains("story", StringComparison.OrdinalIgnoreCase)
                                    //|| 
                                    originalEntry.SBFileLocation.Contains("storycharsb", StringComparison.OrdinalIgnoreCase)
                                    || originalEntry.SBFileLocation.Contains("careersba", StringComparison.OrdinalIgnoreCase)
                                    )
                                )
                                continue;

                            if (originalEntry.TOCFileLocation != null &&
                                (
                                    //originalEntry.TOCFileLocation.Contains("story", StringComparison.OrdinalIgnoreCase)
                                    //|| 
                                    originalEntry.TOCFileLocation.Contains("storycharsb", StringComparison.OrdinalIgnoreCase)
                                    || originalEntry.TOCFileLocation.Contains("careersba", StringComparison.OrdinalIgnoreCase)
                                    )
                                )
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

                if (EntriesToNewPosition == null)
                {
                    parent.Logger.LogError($"Unable to find any entries to process");
                    return false;
                }

                var groupedByTOCSB = EntriesToNewPosition.GroupBy(x =>
                            !string.IsNullOrEmpty(x.Key.SBFileLocation)
                            ? x.Key.SBFileLocation
                            : x.Key.TOCFileLocation
                            )
                    .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

                List<Task> tasks = new List<Task>();

                foreach (var tocGroup in groupedByTOCSB)
                {
                    var tocPath = tocGroup.Key;
                    if (string.IsNullOrEmpty(tocPath))
                        continue;

                    tasks.Add(Task.Run(() =>
                    {
                        tocPath = parent.fs.ResolvePath(tocPath, ModExecutor.UseModData);

                        if (ModExecutor.UseModData && !tocPath.Contains("moddata", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception($"WRONG SB PATH GIVEN! {tocPath}");
                        }

                        using (var tocSbReader = new TocSbReader_Fifa22(false, false))
                        {
                            DbObject dboOriginal = null;
                            List<DbObject> dboOriginal2 = null;
                            if (!SbToDbObject.ContainsKey(tocGroup.Key))
                            {
                                var timeStarted = DateTime.Now;

                                dboOriginal2 = tocSbReader.Read(tocPath, 0, tocGroup.Key, nativePath: tocGroup.Key);

                                SbToDbObject.Add(tocGroup.Key, new DbObject(dboOriginal2));
                                Debug.WriteLine("Time Taken to Read SB: " + (DateTime.Now - timeStarted).ToString());
                            }

                            if (SbToDbObject.ContainsKey(tocGroup.Key))
                                dboOriginal = SbToDbObject[tocGroup.Key];
                            if (dboOriginal != null)
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

                                using (NativeWriter nw_toc = new NativeWriter(new FileStream(tocPath, FileMode.Open)))
                                    {
                                        var assetBundleToCAS = new Dictionary<string, List<(AssetEntry, DbObject)>>();
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

                                        foreach (var abtc in assetBundleToCAS)
                                        {

                                            var resolvedCasPath = FileSystem.Instance.ResolvePath(abtc.Key, ModExecutor.UseModData);
                                            using (var nwCas = new NativeWriter(new FileStream(resolvedCasPath, FileMode.Open)))
                                            {
                                                foreach (var assetEntry in abtc.Value)
                                                {
                                                    var assetBundle = tocGroup.Value.LastOrDefault(x => x.Key.Equals(assetEntry.Item1));
                                                    //if (assetBundle.Key is EbxAssetEntry)
                                                    //    WriteEbxChangesToSuperBundle(assetEntry.Item2, nwCas, assetBundle);
                                                    //else if (assetBundle.Key is ResAssetEntry)
                                                    //    WriteResChangesToSuperBundle(assetEntry.Item2, nwCas, assetBundle);
                                                    //else if (assetBundle.Key is ChunkAssetEntry)
                                                    //    WriteChunkChangesToSuperBundle(assetEntry.Item2, nwCas, assetBundle);
                                                    WriteChangesToSuperBundle(assetEntry.Item2, nwCas, assetBundle);
                                            }
                                            }
                                        }



                                    }

                                using (var fsTocSig = new FileStream(tocPath, FileMode.Open))
                                    TOCFile.RebuildTOCSignatureOnly(fsTocSig);
                            }
                            parent.Logger.Log($"Processing Complete: {tocPath}");
                            if (dboOriginal != null)
                            {
                                dboOriginal = null;
                            }
                            if (dboOriginal2 != null)
                            {
                                dboOriginal2.Clear();
                                dboOriginal2 = null;
                            }
                            if (SbToDbObject != null)
                            {
                                //SbToDbObject.Clear();
                                //SbToDbObject = null;
                            }
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }


                    }));


                }

                Task.WaitAll(tasks.ToArray());





            return true;
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

        }

       


        private void ModifyTOCCasBundles(string directory = "native_patch")
        {
            foreach (var catalogInfo in FileSystem.Instance.EnumerateCatalogInfos())
            {
                byte[] key_2_from_key_manager = KeyManager.Instance.GetKey("Key2");
                foreach (string key3 in catalogInfo.SuperBundles.Keys)
                {

                    string tocFile = key3;
                    if (catalogInfo.SuperBundles[key3])
                    {
                        tocFile = key3.Replace("win32", catalogInfo.Name);
                    }

                    var tocFileRAW = $"{directory}/{tocFile}.toc";
                    string location_toc_file = parent.fs.ResolvePath(tocFileRAW);
                    using TocSbReader_Fifa22 tocSb = new TocSbReader_Fifa22();
                    tocSb.DoLogging = false;
                    tocSb.ProcessData = false;

                    var location_toc_file_new = ModExecutor.UseModData
                        ? location_toc_file
                        .Replace("Data", "ModData\\Data", StringComparison.OrdinalIgnoreCase)
                        .Replace("Patch", "ModData\\Patch", StringComparison.OrdinalIgnoreCase)
                        : location_toc_file;

                    // read the changed toc file in ModData
                    tocSb.Read(location_toc_file_new, 0, tocFileRAW);
                    if (tocSb.TOCFile == null || !tocSb.TOCFile.CasBundles.Any())
                        continue;

                }
            }

            if (directory == "native_patch")
                ModifyTOCCasBundles("native_data");
        }

        private string GetNextCasInCatalog(Catalog catalogInfo, int lastCas, bool patch, out int newCas)
        {
            newCas = lastCas + 1;
            //newCas = lastCas;
            string stub = parent.fs.BasePath + $"ModData\\{(patch ? "Patch" : "Data")}\\" + catalogInfo.Name + "\\cas_";

            string text = stub + (newCas).ToString("D2") + ".cas";

            var fiCas = new FileInfo(text);
            while (fiCas.Exists && fiCas.Length > 1073741824)
            {
                newCas++;
                text = stub + (newCas).ToString("D2") + ".cas";
                fiCas = new FileInfo(text);
            }

            if (!ModExecutor.UseModData)
                text = text.Replace("ModData", "", StringComparison.OrdinalIgnoreCase);

            fiCas = null;
            return text;
        }

        public override bool Cleanup(FileSystem fs, ILogger logger, ModExecutor modExecuter)
        {
            var r = base.Cleanup(fs, logger, modExecuter);

            return r;
        }

        public void GetChunkAssetForEbx(EbxAssetEntry ebxAssetEntry, out ChunkAssetEntry chunkAssetEntry, out EbxAsset ebxAsset)
        {
            chunkAssetEntry = null;
            ebxAsset = AssetManager.Instance.GetEbx(ebxAssetEntry);
            if (ebxAsset != null)
            {
                dynamic rootObject = ebxAsset.RootObject;
                if (rootObject != null)
                {
                    dynamic val = rootObject.Manifest;
                    chunkAssetEntry = AssetManager.Instance.GetChunkEntry(val.ChunkId);
                }
            }
        }
    }



}
