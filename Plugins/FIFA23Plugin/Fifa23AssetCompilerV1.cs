using Frostbite.FileManagers;
using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using paulv2k4ModdingExecuter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static paulv2k4ModdingExecuter.FrostyModExecutor;

namespace FIFA23Plugin
{

    /// <summary>
    /// FIFA 22 Version of the FIFA 21 Asset Compiler. Solid and works. Uses .cache file to determine what needs editing
    /// Linked to FIFA21BundleAction
    /// </summary>
    public class Fifa23AssetCompilerV1 : IAssetCompiler
    {
        public const string ModDirectory = "ModData";
        public const string PatchDirectory = "Patch";


        /// <summary>
        /// This is run AFTER the compilation of the fbmod into resource files ready for the Actions to TOC/SB/CAS to be taken
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="logger"></param>
        /// <param name="frostyModExecuter">Frosty Mod Executer object</param>
        /// <returns></returns>
        public bool Compile(FileSystem fs, ILogger logger, object frostyModExecuter)
        {
            DateTime dtStarted = DateTime.Now;
            if (!ProfilesLibrary.IsFIFA23DataVersion())
            {
                logger.Log("[ERROR] Wrong compiler used for Game");
                return false;
            }

           
            bool result = false;
            if (!FrostyModExecutor.UseModData)
            {
                result = RunEADesktopCompiler(fs, logger, frostyModExecuter);
                return result;
            }
            result = RunOriginCompiler(fs, logger, frostyModExecuter);

            logger.Log($"Compiler completed in {(DateTime.Now - dtStarted).ToString(@"mm\:ss")}");
            return result;
        }

        private bool RunOriginCompiler(FileSystem fs, ILogger logger, object frostyModExecuter)
        {
            if (!Directory.Exists(fs.BasePath))
                throw new DirectoryNotFoundException($"Unable to find the correct base path directory of {fs.BasePath}");

            Directory.CreateDirectory(fs.BasePath + ModDirectory + "\\Data");
            Directory.CreateDirectory(fs.BasePath + ModDirectory + "\\Patch");

            var fme = (FrostyModExecutor)frostyModExecuter;

            logger.Log("Copying files from Data to ModData/Data");
            CopyDataFolder(fs.BasePath + "\\Data\\", fs.BasePath + ModDirectory + "\\Data\\", logger);
            logger.Log("Copying files from Patch to ModData/Patch");
            CopyDataFolder(fs.BasePath + PatchDirectory, fs.BasePath + ModDirectory + "\\" + PatchDirectory, logger);

            Fifa22BundleAction fifaBundleAction = new Fifa22BundleAction(fme);
            return fifaBundleAction.Run();
        }

        private void MakeTOCOriginals(string dir)
        {
            foreach (var tFile in Directory.EnumerateFiles(dir, "*.toc"))
            {
                if (File.Exists(tFile + ".bak"))
                    File.Copy(tFile + ".bak", tFile, true);
            }

            foreach (var tFile in Directory.EnumerateFiles(dir, "*.sb"))
            {
                if (File.Exists(tFile + ".bak"))
                    File.Copy(tFile + ".bak", tFile, true);
            }

            foreach (var internalDir in Directory.EnumerateDirectories(dir))
            {
                MakeTOCOriginals(internalDir);
            }
        }

        private void MakeTOCBackups(string dir)
        {
            foreach (var tFile in Directory.EnumerateFiles(dir, "*.toc"))
            {
                File.Copy(tFile, tFile + ".bak", true);
            }

            foreach (var tFile in Directory.EnumerateFiles(dir, "*.sb"))
            {
                File.Copy(tFile, tFile + ".bak", true);
            }

            foreach (var internalDir in Directory.EnumerateDirectories(dir))
            {
                MakeTOCBackups(internalDir);
            }
        }

        FrostyModExecutor ModExecutor;

        private bool RunEADesktopCompiler(FileSystem fs, ILogger logger, object frostyModExecuter)
        {
            var fme = (FrostyModExecutor)frostyModExecuter;
            var parent = fme;
            ModExecutor = fme;

            fme.Logger.Log("Not using ModData. Starting EA Desktop Compiler.");

            if (!Directory.Exists(fs.BasePath))
                throw new DirectoryNotFoundException($"Unable to find the correct base path directory of {fs.BasePath}");
            var notAnyBackups = !Directory.EnumerateFiles(fme.GamePath + "\\Data\\", "*.toc.bak").Any();
            fme.GameWasPatched = fme.GameWasPatched || notAnyBackups;
            if (!fme.GameWasPatched)
            {
                fme.Logger.Log("Same Game Version detected. Using vanilla backups.");

                MakeTOCOriginals(fme.GamePath + "\\Data\\");
                MakeTOCOriginals(fme.GamePath + "\\Patch\\");

            }
            else
            {
                fme.Logger.Log("Game was patched. Creating backups.");

                MakeTOCBackups(fme.GamePath + "\\Data\\");
                MakeTOCBackups(fme.GamePath + "\\Patch\\");
            }
            Fifa22BundleAction fifaBundleAction = new Fifa22BundleAction(fme, false);
            return fifaBundleAction.Run();
        }

        private static void CopyDataFolder(string from_datafolderpath, string to_datafolderpath, ILogger logger)
        {
            Directory.CreateDirectory(to_datafolderpath);

            var dataFiles = Directory.EnumerateFiles(from_datafolderpath, "*.*", SearchOption.AllDirectories);
            var dataFileCount = dataFiles.Count();
            var indexOfDataFile = 0;
            //ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
            //Parallel.ForEach(dataFiles, (f) =>
            foreach (var originalFilePath in dataFiles)
            {
                var finalDestinationPath = originalFilePath.ToLower().Replace(from_datafolderpath.ToLower(), to_datafolderpath.ToLower());

                bool Copied = false;

                var lastIndexOf = finalDestinationPath.LastIndexOf("\\");
                var newDirectory = finalDestinationPath.Substring(0, lastIndexOf) + "\\";
                if (!Directory.Exists(newDirectory))
                {
                    Directory.CreateDirectory(newDirectory);
                }


                if (!finalDestinationPath.Contains("moddata", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Incorrect Copy of Files to ModData");
                }

                var fIDest = new FileInfo(finalDestinationPath);
                var fIOrig = new FileInfo(originalFilePath);

                if (fIDest.Exists && finalDestinationPath.Contains("moddata", StringComparison.OrdinalIgnoreCase))
                {
                    var isCas = fIDest.Extension.Contains("cas", StringComparison.OrdinalIgnoreCase);

                    if (
                        isCas
                        && fIDest.Length != fIOrig.Length
                        )
                    {
                        fIDest.Delete();
                    }
                    else if
                        (
                            !isCas
                            &&
                            (
                                fIDest.Length != fIOrig.Length
                                ||
                                    (
                                        //fIDest.LastWriteTime.Day != fIOrig.LastWriteTime.Day
                                        //&& fIDest.LastWriteTime.Hour != fIOrig.LastWriteTime.Hour
                                        //&& fIDest.LastWriteTime.Minute != fIOrig.LastWriteTime.Minute
                                        !File.ReadAllBytes(finalDestinationPath).SequenceEqual(File.ReadAllBytes(originalFilePath))
                                    )
                            )
                        )
                    {
                        File.Delete(finalDestinationPath);
                    }
                }

                if (!File.Exists(finalDestinationPath))
                {
                    // Quick Copy
                    if (fIOrig.Length < 1024 * 100)
                    {
                        using (var inputStream = new NativeReader(File.Open(originalFilePath, FileMode.Open)))
                        using (var outputStream = new NativeWriter(File.Open(finalDestinationPath, FileMode.Create)))
                        {
                            outputStream.Write(inputStream.ReadToEnd());
                        }
                    }
                    else
                    {
                        //File.Copy(f, finalDestination);
                        CopyFile(originalFilePath, finalDestinationPath);
                    }
                    Copied = true;
                }
                indexOfDataFile++;

                if (Copied)
                    logger.Log($"Data Setup - Copied ({indexOfDataFile}/{dataFileCount}) - {originalFilePath}");
                //});
            }
        }

        public static void CopyFile(string inputFilePath, string outputFilePath)
        {
            using (var inStream = new FileStream(inputFilePath, FileMode.Open))
            {
                int bufferSize = 1024 * 1024;

                using (FileStream fileStream = new FileStream(outputFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fileStream.SetLength(inStream.Length);
                    int bytesRead = -1;
                    byte[] bytes = new byte[bufferSize];

                    while ((bytesRead = inStream.Read(bytes, 0, bufferSize)) > 0)
                    {
                        fileStream.Write(bytes, 0, bytesRead);
                    }
                }
            }
        }


    }

    /// <summary>
    /// The actual builder/modifier of the files
    /// </summary>
    public class Fifa22BundleAction
    {
        public class BundleFileEntry
        {
            public int CasIndex;

            public int Offset;

            public int Size;

            public string Name;

            public BundleFileEntry(int inCasIndex, int inOffset, int inSize, string inName = null)
            {
                CasIndex = inCasIndex;
                Offset = inOffset;
                Size = inSize;
                Name = inName;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return $"CASIdx({CasIndex}), Offset({Offset}), Size({Size}), Name({Name})";

            }
        }

        private FrostyModExecutor parent;

        public static Dictionary<string, List<string>> CatalogCasFiles = new Dictionary<string, List<string>>();

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

        private readonly bool UseModData;

        public Fifa22BundleAction(FrostyModExecutor inParent, bool useModData = true)
        {
            parent = inParent;
            ErrorCounts.Add(ModType.EBX, 0);
            ErrorCounts.Add(ModType.RES, 0);
            ErrorCounts.Add(ModType.CHUNK, 0);
            UseModData = useModData;

            CatalogCasFiles.Clear();
            foreach (Catalog catalog in FileSystem.Instance.EnumerateCatalogInfos())
            {
                int casFileNumber = 1;
                List<string> casFileLocation = new List<string>();
                string path2 = Path.Combine(FileSystem.Instance.BasePath, "Patch", catalog.Name, $"cas_{casFileNumber:D2}.cas");
                while (File.Exists(path2))
                {
                    casFileLocation.Add(path2);
                    casFileNumber++;
                    path2 = Path.Combine(FileSystem.Instance.BasePath, "Patch", catalog.Name, $"cas_{casFileNumber:D2}.cas");
                }
                CatalogCasFiles.Add(catalog.Name, casFileLocation);
            }

        }

        public enum ModType
        {
            EBX,
            RES,
            CHUNK
        }

        public struct ModdedFile
        {
            public Sha1 Sha1 { get; set; }
            public string NamePath { get; set; }
            public ModType ModType { get; set; }
            public bool IsAdded { get; set; }
            public AssetEntry OriginalEntry { get; set; }

            public ModdedFile(Sha1 inSha1, string inNamePath, ModType inModType, bool inAdded)
            {
                Sha1 = inSha1;
                NamePath = inNamePath;
                ModType = inModType;
                IsAdded = inAdded;
                OriginalEntry = null;
            }

            public ModdedFile(Sha1 inSha1, string inNamePath, ModType inModType, bool inAdded, AssetEntry inOrigEntry)
            {
                Sha1 = inSha1;
                NamePath = inNamePath;
                ModType = inModType;
                IsAdded = inAdded;
                OriginalEntry = inOrigEntry;
            }


        }

        public Dictionary<ModType, int> ErrorCounts = new Dictionary<ModType, int>();

        public List<ChunkAssetEntry> AddedChunks = new List<ChunkAssetEntry>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns>cas to Tuple of Sha1, Name, Type, IsAdded</returns>
        private Dictionary<string, List<ModdedFile>> GetModdedCasFiles()
        {
            // Handle Legacy first to generate modified chunks
            ProcessLegacyMods();
            // ------ End of handling Legacy files ---------

            Dictionary<string, List<ModdedFile>> casToMods = new Dictionary<string, List<ModdedFile>>();
            foreach (var modEBX in parent.modifiedEbx)
            {
                var originalEntry = AssetManager.Instance.GetEbxEntry(modEBX.Value.Name);
                if (originalEntry != null && originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                {
                    var casPath = originalEntry.ExtraData.CasPath;
                    
                    if (!casToMods.ContainsKey(casPath))
                    {
                        casToMods.Add(casPath, new List<ModdedFile>() { new ModdedFile(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, false, originalEntry) });
                    }
                    else
                    {
                        casToMods[casPath].Add(new ModdedFile(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, false, originalEntry));
                    }
                }
                else
                {
                    ErrorCounts[ModType.EBX]++;
                }
            }
            foreach (var modRES in parent.modifiedRes)
            {
                var originalEntry = AssetManager.Instance.GetResEntry(modRES.Value.Name);
                if (originalEntry != null)
                {
                    if (originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                    {

                        var casPath = originalEntry.ExtraData.CasPath;
                        if (!casToMods.ContainsKey(casPath))
                        {
                            casToMods.Add(casPath, new List<ModdedFile>() { new ModdedFile(modRES.Value.Sha1, modRES.Value.Name, ModType.RES, false, originalEntry) });
                        }
                        else
                        {
                            casToMods[casPath].Add(new ModdedFile(modRES.Value.Sha1, modRES.Value.Name, ModType.RES, false, originalEntry));
                        }
                    }
                }
                else
                {
                    ErrorCounts[ModType.RES]++;
                }
            }
            foreach (var modChunks in parent.ModifiedChunks)
            {
                var originalEntry = AssetManager.Instance.GetChunkEntry(modChunks.Key);

                if ((modChunks.Value.ModifiedEntry == null || !modChunks.Value.ModifiedEntry.AddToChunkBundle)
                    && originalEntry != null && originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                {
                    if (originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
                    {
                        var casPath = originalEntry.ExtraData.CasPath;
                        if (!casToMods.ContainsKey(casPath))
                        {
                            casToMods.Add(casPath, new List<ModdedFile>() { new ModdedFile(modChunks.Value.Sha1, modChunks.Key.ToString(), ModType.CHUNK, false, originalEntry) });
                        }
                        else
                        {
                            casToMods[casPath].Add(new ModdedFile(modChunks.Value.Sha1, modChunks.Key.ToString(), ModType.CHUNK, false, originalEntry));
                        }
                    }
                }
               
            }



            return casToMods;
        }

        private void ProcessLegacyMods()
        {
            List<Guid> ChunksToRemove = new List<Guid>();
            // *.fifamod files do not put data into "modifiedLegacy" and instead embed into Chunks 
            // handling those chunks here
            //if (parent.AddedChunks.Count > 0)
            //{
                
            //    foreach(var mod in parent.AddedChunks)
            //    {
            //        ChunkAssetEntry cae = mod.Value;
            //        if (cae != null)
            //        {
            //            if(cae.ModifiedEntry != null)
            //            {
            //                if(cae.ModifiedEntry.IsLegacyFile)
            //                {
            //                    parent.modifiedLegacy.Add(cae.ModifiedEntry.LegacyFullName
            //                        , new LegacyFileEntry()
            //                        {
            //                            Sha1 = cae.ModifiedEntry.Sha1,
            //                            Name = cae.ModifiedEntry.LegacyFullName
            //                        });
            //                    ChunksToRemove.Add(mod.Key);

            //                }
            //            }
            //        }
            //    }
            //}
            //else 
            //if (parent.ModifiedChunks.Count > 0)
            //{
            //    foreach (var mod in parent.ModifiedChunks)
            //    {
            //        ChunkAssetEntry cae = mod.Value;
            //        if (cae != null)
            //        {
            //            if (cae.ModifiedEntry != null)
            //            {
            //                if (cae.ModifiedEntry.IsLegacyFile)
            //                {
            //                    parent.modifiedLegacy.Add(cae.ModifiedEntry.LegacyFullName
            //                        , new LegacyFileEntry()
            //                        {
            //                            Sha1 = cae.ModifiedEntry.Sha1,
            //                            Name = cae.ModifiedEntry.LegacyFullName,
            //                            ModifiedEntry = new ModifiedAssetEntry()
            //                            {
            //                                Name = cae.ModifiedEntry.LegacyFullName,
            //                                UserData = cae.ModifiedEntry.LegacyFullName,
            //                                Data = cae.ModifiedEntry.Data
            //                            }
            //                        });
            //                    ChunksToRemove.Add(mod.Key);
            //                }
            //            }
            //        }
            //    }
            //}

            //foreach (var mod in ChunksToRemove)
            //    parent.ModifiedChunks.Remove(mod);


            // -----------------------------------------------------------
            // process modified legacy chunks and make live changes
            if (parent.modifiedLegacy.Count > 0)
            {
                parent.Logger.Log($"Legacy :: {parent.modifiedLegacy.Count} Legacy files found. Modifying associated chunks");

                Dictionary<string, byte[]> legacyData = new Dictionary<string, byte[]>();
                var countLegacyChunksModified = 0;
                foreach (var modLegacy in parent.modifiedLegacy)
                {
                    //var originalEntry = AssetManager.Instance.GetCustomAssetEntry("legacy", modLegacy.Key);
                    byte[] data = null;
                    //if (modLegacy.Value.ModifiedEntry != null && modLegacy.Value.ModifiedEntry.Data != null)
                    //    data = new CasReader(new MemoryStream(modLegacy.Value.ModifiedEntry.Data)).Read();
                    //else if (parent.archiveData.ContainsKey(modLegacy.Value.Sha1))
                        data = parent.archiveData[modLegacy.Value.Sha1].Data;

                    if (data != null)
                    {
                        legacyData.Add(modLegacy.Key, data);
                    }
                }

                //AssetManager.Instance.ModifyLegacyAssets(legacyData, true);
                LegacyFileManager_FMTV2 legacyFileManager = AssetManager.Instance.GetLegacyAssetManager() as LegacyFileManager_FMTV2;
                if (legacyFileManager != null)
                {
                    legacyFileManager.ModifyAssets(legacyData, true);

                    var modifiedLegacyChunks = AssetManager.Instance.EnumerateChunks(true);
                    foreach (var modLegChunk in modifiedLegacyChunks.Where(x => !parent.ModifiedChunks.ContainsKey(x.Id)))
                    {
                        modLegChunk.Sha1 = modLegChunk.ModifiedEntry.Sha1;
                        parent.ModifiedChunks.Add(modLegChunk.Id, modLegChunk);
                        countLegacyChunksModified++;
                    }

                    var modifiedChunks = AssetManager.Instance.EnumerateChunks(true);
                    foreach (var chunk in modifiedChunks)
                    {
                        if (parent.archiveData.ContainsKey(chunk.Sha1))
                            parent.archiveData[chunk.Sha1] = new ArchiveInfo() { Data = chunk.ModifiedEntry.Data };
                        else
                            parent.archiveData.TryAdd(chunk.Sha1, new ArchiveInfo() { Data = chunk.ModifiedEntry.Data });
                    }
                    parent.Logger.Log($"Legacy :: Modified {countLegacyChunksModified} associated chunks");
                }
            }
        }

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
                BuildCache buildCache = new BuildCache();
                buildCache.LoadData(ProfilesLibrary.ProfileName, parent.GamePath, parent.Logger, false, true);
            }

            parent.Logger.Log("Loading Cached Super Bundles.");

            if (!UseModData && GameWasPatched)
            {
                DeleteBakFiles(parent.GamePath);
            }

            parent.Logger.Log("Finished loading files. Enumerating modified bundles.");

            var dictOfModsToCas = GetModdedCasFiles();
            if (dictOfModsToCas != null && dictOfModsToCas.Count > 0)
            {
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
                    var casPath = FileSystem.Instance.ResolvePath(item.Key, FrostyModExecutor.UseModData);

                    Debug.WriteLine($"Modifying CAS file - {casPath}");
                    parent.Logger.Log($"Modifying CAS file - {casPath}");

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
                                parent.Logger.LogError($"Unable to find original archive data for {modItem.NamePath}");
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

                            if (origSize == 0
                                || origSize == data.Length)
                            {
                                var out_data = new CasReader(new MemoryStream(data)).Read();
                                origSize = out_data.Length;
                            }

                            if(originalEntry.TOCFileLocation != null)
                                EntriesToNewPosition.Add(originalEntry, (positionOfData, data.Length, origSize, modItem.Sha1));
                        }

                    }
                }

                if (EntriesToNewPosition != null)
                {

                    var groupedBySB = EntriesToNewPosition
                        .GroupBy(x => x.Key.TOCFileLocation)
                        .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
                    /*
                    foreach (var sbGroup in groupedBySB)
                    {
                        var sbpath = sbGroup.Key;
                        if (string.IsNullOrEmpty(sbpath))
                            continue;

                        sbpath = parent.fs.ResolvePath(sbpath, FrostyModExecutor.UseModData);

                        if (FrostyModExecutor.UseModData && !sbpath.Contains("moddata", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception($"WRONG SB PATH GIVEN! {sbpath}");
                        }

                        var tocSbReader = new TocSbReader_Fifa22(false, false);

                        DbObject dboOriginal = null;
                        if (!SbToDbObject.ContainsKey(sbGroup.Key))
                        {
                            var timeStarted = DateTime.Now;

                            tocSbReader.Read(sbpath
                                , 0
                                , new BinarySbDataHelper(AssetManager.Instance)
                                , sbpath);

                            SbToDbObject.Add(sbGroup.Key, tocSbReader.TOCFile.TOCObjects);
                            Debug.WriteLine("Time Taken to Read TOC - " + (DateTime.Now - timeStarted).ToString(@"mm\:ss"));
                        }

                        if (SbToDbObject.ContainsKey(sbGroup.Key))
                            dboOriginal = SbToDbObject[sbGroup.Key];

                        //using (NativeWriter nw_sb = new NativeWriter(new FileStream(sbpath, FileMode.Open)))
                        //{
                        foreach (var assetBundle in sbGroup.Value)
                        {
                            if (dboOriginal != null)
                            {
                                switch (assetBundle.Key.AssetType)
                                {
                                    case "ebx":
                                        DbObject origEbxDbo = null;
                                        var origEbxBundles = dboOriginal.List.Where(x => ((DbObject)x).HasValue("ebx")).Select(x => ((DbObject)x).GetValue<DbObject>("ebx")).ToList();
                                        foreach (DbObject dbInBundle in origEbxBundles)
                                        {
                                            origEbxDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["name"].ToString() == assetBundle.Key.Name);
                                            if (origEbxDbo != null)
                                            {
                                                var cBundle = tocSbReader.TOCFile.CasBundles.FirstOrDefault(
                                                     x => x.Entries.Any(
                                                         y => y.locationOfSize == origEbxDbo.GetValue<long>("SB_CAS_Size_Position")
                                                         ));
                                                var cEntry = cBundle.Entries.FirstOrDefault(
                                                         y => y.locationOfSize == origEbxDbo.GetValue<long>("SB_CAS_Size_Position")
                                                         );
                                                if (cEntry != null)
                                                {
                                                    var bundlePath = FileSystem.Instance.GetFilePath(cBundle.Catalog, cBundle.Cas, cBundle.Patch);
                                                    var entryPath = FileSystem.Instance.GetFilePath(cEntry.catalog, cEntry.cas, cEntry.isInPatch);

                                                    cEntry.bundleOffsetInCas = (uint)assetBundle.Value.Item1;
                                                    cEntry.bundleSizeInCas = (uint)assetBundle.Value.Item2;

                                                    var casBundleLocation = FileSystem.Instance.ResolvePath(bundlePath, FrostyModExecutor.UseModData);
                                                    using (NativeWriter nwCasBundle = new NativeWriter(new FileStream(casBundleLocation, FileMode.Open)))
                                                    {
                                                        nwCasBundle.Position = origEbxDbo.GetValue<long>("SB_OriginalSize_Position");
                                                        nwCasBundle.Write((uint)assetBundle.Value.Item3, Endian.Little);

                                                        nwCasBundle.Position = origEbxDbo.GetValue<long>("SB_Sha1_Position");
                                                        nwCasBundle.Write(assetBundle.Value.Item4);
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                        break;
                                    case "res":
                                        var origResBundles = dboOriginal.List.Where(x => ((DbObject)x).HasValue("res")).Select(x => ((DbObject)x).GetValue<DbObject>("res")).ToList();
                                        DbObject origResDbo = null;
                                        foreach (DbObject dbInBundle in origResBundles)
                                        {
                                            origResDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["name"].ToString() == assetBundle.Key.Name);
                                            if (origResDbo != null)
                                            {
                                                var cBundle = tocSbReader.TOCFile.CasBundles.FirstOrDefault(
                                                     x => x.Entries.Any(
                                                         y => y.locationOfSize == origResDbo.GetValue<long>("SB_CAS_Size_Position")
                                                         ));
                                                var cEntry = cBundle.Entries.FirstOrDefault(
                                                         y => y.locationOfSize == origResDbo.GetValue<long>("SB_CAS_Size_Position")
                                                         );
                                                if (cEntry != null)
                                                {
                                                    var bundlePath = FileSystem.Instance.GetFilePath(cBundle.Catalog, cBundle.Cas, cBundle.Patch);
                                                    var entryPath = FileSystem.Instance.GetFilePath(cEntry.catalog, cEntry.cas, cEntry.isInPatch);

                                                    cEntry.bundleOffsetInCas = (uint)assetBundle.Value.Item1;
                                                    cEntry.bundleSizeInCas = (uint)assetBundle.Value.Item2;

                                                    var casBundleLocation = FileSystem.Instance.ResolvePath(bundlePath, FrostyModExecutor.UseModData);
                                                    using (NativeWriter nwCasBundle = new NativeWriter(new FileStream(casBundleLocation, FileMode.Open)))
                                                    {
                                                        nwCasBundle.Position = origResDbo.GetValue<long>("SB_OriginalSize_Position");
                                                        nwCasBundle.Write((uint)assetBundle.Value.Item3, Endian.Little);

                                                        nwCasBundle.Position = origResDbo.GetValue<long>("SB_Sha1_Position");
                                                        nwCasBundle.Write(assetBundle.Value.Item4);

                                                        nwCasBundle.BaseStream.Position = origResDbo.GetValue<int>("SB_ResMeta_Position");
                                                        nwCasBundle.WriteBytes(parent.modifiedRes[assetBundle.Key.Name].ResMeta);
                                                    }
                                                }
                                                break;
                                            }
                                        }

                                        break;
                                    case "chunk":

                                        // Needs to move to the CAS
                                        var origChunkBundles = dboOriginal.List.Where(x => ((DbObject)x).HasValue("chunks")).Select(x => ((DbObject)x).GetValue<DbObject>("chunks")).ToList();
                                        DbObject origChunkDbo = null;
                                        foreach (DbObject dbInBundle in origChunkBundles)
                                        {
                                            origChunkDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["id"].ToString() == assetBundle.Key.Name);
                                            if (origChunkDbo != null)
                                            {
                                                var cBundle = tocSbReader.TOCFile.CasBundles.FirstOrDefault(
                                                     x => x.Entries.Any(
                                                         y => y.locationOfSize == origChunkDbo.GetValue<long>("SB_CAS_Size_Position")
                                                         ));
                                                var cEntry = cBundle.Entries.FirstOrDefault(
                                                         y => y.locationOfSize == origChunkDbo.GetValue<long>("SB_CAS_Size_Position")
                                                         );
                                                if (cEntry != null)
                                                {
                                                    var bundlePath = FileSystem.Instance.GetFilePath(cBundle.Catalog, cBundle.Cas, cBundle.Patch);
                                                    var entryPath = FileSystem.Instance.GetFilePath(cEntry.catalog, cEntry.cas, cEntry.isInPatch);

                                                    cEntry.bundleOffsetInCas = (uint)assetBundle.Value.Item1;
                                                    cEntry.bundleSizeInCas = (uint)assetBundle.Value.Item2;

                                                    var casBundleLocation = FileSystem.Instance.ResolvePath(bundlePath, FrostyModExecutor.UseModData);
                                                    using (NativeWriter nwCasBundle = new NativeWriter(new FileStream(casBundleLocation, FileMode.Open)))
                                                    {
                                                        nwCasBundle.Position = origChunkDbo.GetValue<long>("SB_OriginalSize_Position");
                                                        nwCasBundle.Write((uint)assetBundle.Value.Item3, Endian.Little);

                                                        nwCasBundle.Position = origChunkDbo.GetValue<long>("SB_Sha1_Position");
                                                        nwCasBundle.Write(assetBundle.Value.Item4);

                                                        nwCasBundle.BaseStream.Position = origChunkDbo.GetValue<int>("SB_LogicalOffset_Position");
                                                        nwCasBundle.Write(parent.ModifiedChunks[origChunkDbo.GetValue<Guid>("id")].LogicalOffset);
                                                    }
                                                }
                                                break;
                                            }
                                        }

                                        break;
                                }
                            }

                        }
                    
                        tocSbReader.TOCFile.Write(new FileStream(sbpath, FileMode.Open));
                    }
                    */

                    List<Task> tasks = new List<Task>();

                    foreach (var sbGroup in groupedBySB)
                    {
                        var sbpath = sbGroup.Key;
                        if (string.IsNullOrEmpty(sbpath))
                            continue;

                        sbpath = parent.fs.ResolvePath(sbpath, FrostyModExecutor.UseModData);

                        if (UseModData && !sbpath.Contains("moddata", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception($"WRONG SB PATH GIVEN! {sbpath}");
                        }

                        var tocSbReader = new TocSbReader_Fifa22(false, false);

                        DbObject dboOriginal = null;
                        if (!SbToDbObject.ContainsKey(sbGroup.Key))
                        {
                            var timeStarted = DateTime.Now;

                            var dboOriginal2 = tocSbReader.Read(sbpath.Replace(".sb", ".toc", StringComparison.OrdinalIgnoreCase), 0, sbpath);

                            SbToDbObject.Add(sbGroup.Key, new DbObject(dboOriginal2));
                            Debug.WriteLine("Time Taken to Read SB: " + (DateTime.Now - timeStarted).ToString());
                        }

                        if (SbToDbObject.ContainsKey(sbGroup.Key))
                            dboOriginal = SbToDbObject[sbGroup.Key];


                        tasks.Add(Task.Run(() =>
                        {
                            if (dboOriginal != null)
                            {
                                parent.Logger.Log($"Processing: {sbpath}");
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

                                if (new FileInfo(sbpath).Extension.Contains(".sb"))
                                {
                                    using (NativeWriter nw_sb = new NativeWriter(new FileStream(sbpath, FileMode.Open)))
                                    {
                                        foreach (var assetBundle in sbGroup.Value)
                                        {
                                            if (assetBundle.Key is EbxAssetEntry)
                                                WriteEbxChangesToSuperBundle(origEbxBundles, nw_sb, assetBundle);
                                            if (assetBundle.Key is ResAssetEntry)
                                                WriteResChangesToSuperBundle(origResBundles, nw_sb, assetBundle);
                                            if (assetBundle.Key is ChunkAssetEntry)
                                                WriteChunkChangesToSuperBundle(origChunkBundles, nw_sb, assetBundle);

                                            var positionOfNewData = assetBundle.Value.Item1;
                                            var sizeOfData = assetBundle.Value.Item2;
                                            var originalSizeOfData = assetBundle.Value.Item3;
                                            var sha = assetBundle.Value.Item4;

                                            int sb_cas_size_position = assetBundle.Key.SB_CAS_Size_Position;
                                            var sb_cas_offset_position = assetBundle.Key.SB_CAS_Offset_Position;
                                            nw_sb.BaseStream.Position = sb_cas_offset_position;
                                            nw_sb.Write((uint)positionOfNewData, Endian.Big);
                                            nw_sb.Write((uint)sizeOfData, Endian.Big);

                                            if (nw_sb.Length > assetBundle.Key.SB_OriginalSize_Position
                                                &&
                                                assetBundle.Key.SB_OriginalSize_Position != 0 && originalSizeOfData != 0)
                                            {
                                                nw_sb.Position = assetBundle.Key.SB_OriginalSize_Position;
                                                nw_sb.Write((uint)originalSizeOfData, Endian.Little);
                                            }

                                            if (nw_sb.Length > assetBundle.Key.SB_Sha1_Position
                                                &&
                                                assetBundle.Key.SB_Sha1_Position != 0 && sha != Sha1.Zero)
                                            {
                                                nw_sb.Position = assetBundle.Key.SB_Sha1_Position;
                                                nw_sb.Write(sha);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    using (NativeWriter nw_toc = new NativeWriter(new FileStream(sbpath, FileMode.Open)))
                                    {
                                        var assetBundleToCAS = new Dictionary<string, List<AssetEntry>>();
                                        foreach (var assetBundle in sbGroup.Value)
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

                                            var casPath = string.Empty;
                                            if (assetBundle.Key is EbxAssetEntry)
                                            {
                                                DbObject origEbxDbo = null;
                                                foreach (DbObject dbInBundle in origEbxBundles)
                                                {
                                                    origEbxDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["name"].ToString() == assetBundle.Key.Name);
                                                    if (origEbxDbo != null)
                                                        break;
                                                }

                                                if (origEbxDbo != null)
                                                {
                                                    casPath = origEbxDbo.GetValue<string>("ParentCASBundleLocation");
                                                }
                                            }

                                            if (assetBundle.Key is ResAssetEntry)
                                            {
                                                DbObject origResDbo = null;
                                                foreach (DbObject dbInBundle in origResBundles)
                                                {
                                                    origResDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["name"].ToString() == assetBundle.Key.Name);
                                                    if (origResDbo != null)
                                                        break;
                                                }

                                                if (origResDbo != null)
                                                {
                                                    casPath = origResDbo.GetValue<string>("ParentCASBundleLocation");
                                                }
                                            }

                                            if (assetBundle.Key is ChunkAssetEntry)
                                            {
                                                DbObject origChunkDbo = null;
                                                foreach (DbObject dbInBundle in origChunkBundles)
                                                {
                                                    origChunkDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["id"].ToString() == assetBundle.Key.Name);
                                                    if (origChunkDbo != null)
                                                        break;
                                                }

                                                if (origChunkDbo != null)
                                                {
                                                    casPath = origChunkDbo.GetValue<string>("ParentCASBundleLocation");
                                                }
                                            }

                                            if (!string.IsNullOrEmpty(casPath))
                                            {
                                                if (!assetBundleToCAS.ContainsKey(casPath))
                                                    assetBundleToCAS.Add(casPath, new List<AssetEntry>());

                                                assetBundleToCAS[casPath].Add(assetBundle.Key);
                                            }
                                        }

                                        foreach (var abtc in assetBundleToCAS)
                                        {
                                            var resolvedCasPath = FileSystem.Instance.ResolvePath(abtc.Key, FrostyModExecutor.UseModData);
                                            using (var nwCas = new NativeWriter(new FileStream(resolvedCasPath, FileMode.Open)))
                                            {
                                                foreach (var assetEntry in abtc.Value)
                                                {
                                                    var assetBundle = sbGroup.Value.FirstOrDefault(x => x.Key == assetEntry);
                                                    if (assetBundle.Key is EbxAssetEntry)
                                                        WriteEbxChangesToSuperBundle(origEbxBundles, nwCas, assetBundle);
                                                    if (assetBundle.Key is ResAssetEntry)
                                                        WriteResChangesToSuperBundle(origResBundles, nwCas, assetBundle);
                                                    if (assetBundle.Key is ChunkAssetEntry)
                                                        WriteChunkChangesToSuperBundle(origChunkBundles, nwCas, assetBundle);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            parent.Logger.Log($"Processing Complete: {sbpath}");

                        }));
                    }

                    Task.WaitAll(tasks.ToArray());

                }

            }

            //ModifyTOCChunks();

            return true;
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

        }

        private void WriteEbxChangesToSuperBundle(List<DbObject> origEbxBundles, NativeWriter writer, KeyValuePair<AssetEntry, (long, int, int, Sha1)> assetBundle)
        {
            DbObject origEbxDbo = null;
            foreach (DbObject dbInBundle in origEbxBundles)
            {
                origEbxDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["name"].ToString() == assetBundle.Key.Name);
                if (origEbxDbo != null)
                    break;
            }

            if (origEbxDbo != null
                && parent.modifiedEbx.ContainsKey(assetBundle.Key.Name))
            {
                var originalSizeOfData = assetBundle.Value.Item3;

                if (origEbxDbo.HasValue("SB_OriginalSize_Position"))
                {
                    writer.Position = origEbxDbo.GetValue<long>("SB_OriginalSize_Position");
                    writer.Write((uint)originalSizeOfData, Endian.Little);
                }

                if (origEbxDbo.HasValue("SB_Sha1_Position") && assetBundle.Value.Item4 != Sha1.Zero)
                {
                    writer.Position = origEbxDbo.GetValue<long>("SB_Sha1_Position");
                    writer.Write(assetBundle.Value.Item4);
                }
            }
        }

        private void WriteResChangesToSuperBundle(List<DbObject> origResBundles, NativeWriter writer, KeyValuePair<AssetEntry, (long, int, int, Sha1)> assetBundle)
        {
            DbObject origResDbo = null;
            foreach (DbObject dbInBundle in origResBundles)
            {
                origResDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["name"].ToString() == assetBundle.Key.Name);
                if (origResDbo != null)
                    break;
            }

            if (origResDbo != null
                && parent.modifiedRes.ContainsKey(assetBundle.Key.Name)
                && (assetBundle.Key.Type == "SkinnedMeshAsset"
                || assetBundle.Key.Type == "MeshSet"
                || assetBundle.Key.Type == "Texture"))
            {
                writer.BaseStream.Position = origResDbo.GetValue<int>("SB_ResMeta_Position");
                writer.WriteBytes(parent.modifiedRes[assetBundle.Key.Name].ResMeta);

                if (parent.modifiedRes[assetBundle.Key.Name].ResRid != 0)
                {
                    writer.BaseStream.Position = origResDbo.GetValue<int>("SB_ReRid_Position");
                    writer.Write(parent.modifiedRes[assetBundle.Key.Name].ResRid);
                }
            }

            var originalSizeOfData = assetBundle.Value.Item3;

            if (origResDbo.HasValue("SB_OriginalSize_Position"))
            {
                writer.Position = origResDbo.GetValue<long>("SB_OriginalSize_Position");
                writer.Write((uint)originalSizeOfData, Endian.Little);
            }

            if (origResDbo.HasValue("SB_Sha1_Position") && assetBundle.Value.Item4 != Sha1.Zero)
            {
                writer.Position = origResDbo.GetValue<long>("SB_Sha1_Position");
                writer.Write(assetBundle.Value.Item4);
            }
        }

        private void WriteChunkChangesToSuperBundle(List<DbObject> origChunkBundles, NativeWriter writer, KeyValuePair<AssetEntry, (long, int, int, Sha1)> assetBundle)
        {
            DbObject origChunkDbo = null;
            foreach (DbObject dbInBundle in origChunkBundles)
            {
                origChunkDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["id"].ToString() == assetBundle.Key.Name);
                if (origChunkDbo != null)
                    break;
            }

            if (Guid.TryParse(assetBundle.Key.Name, out Guid bndleId))
            {
                if (origChunkDbo != null
                    && parent.ModifiedChunks.ContainsKey(bndleId)
                    )
                {
                    writer.BaseStream.Position = origChunkDbo.GetValue<int>("SB_LogicalOffset_Position");
                    writer.Write(parent.ModifiedChunks[bndleId].LogicalOffset);
                }
            }

            var originalSizeOfData = assetBundle.Value.Item3;

            if (origChunkDbo.HasValue("SB_OriginalSize_Position"))
            {
                writer.Position = origChunkDbo.GetValue<long>("SB_OriginalSize_Position");
                writer.Write((uint)originalSizeOfData, Endian.Little);
            }

            if (origChunkDbo.HasValue("SB_Sha1_Position") && assetBundle.Value.Item4 != Sha1.Zero)
            {
                writer.Position = origChunkDbo.GetValue<long>("SB_Sha1_Position");
                writer.Write(assetBundle.Value.Item4);
            }
        }

        private void ModifyTOCChunks(string directory = "native_patch")
        {
            foreach (var catalogInfo in FileSystem.Instance.EnumerateCatalogInfos())
            {
                foreach (string key3 in catalogInfo.SuperBundles.Keys)
                {
                    string tocFile = key3;
                    if (catalogInfo.SuperBundles[key3])
                    {
                        tocFile = key3.Replace("win32", catalogInfo.Name);
                    }

                    // Only handle Legacy stuff right now
                    if (!tocFile.Contains("globals", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var tocFileRAW = $"{directory}/{tocFile}.toc";
                    string location_toc_file = parent.fs.ResolvePath(tocFileRAW, FrostyModExecutor.UseModData);
                    TocSbReader_Fifa22 tocSb = new TocSbReader_Fifa22(false, false);

                    // read the changed toc file in ModData
                    tocSb.Read(location_toc_file
                        , 0
                        , tocFileRAW);
                    if (tocSb.TOCFile == null || !tocSb.TOCFile.TocChunks.Any())
                        continue;

                    var patch = true;
                    var catalog = tocSb.TOCFile.TocChunks.Max(x => x.ExtraData.Catalog.Value);
                    if (!tocSb.TOCFile.TocChunks.Any(x => x.ExtraData.IsPatch))
                        patch = false;
                    
                    var cas = tocSb.TOCFile.TocChunks.Where(x => x.ExtraData.Catalog == catalog).Max(x => x.ExtraData.Cas.Value);
                   
                    var nextCasPath = GetNextCasInCatalog(catalogInfo, cas, patch, out int newCas);

                    using (NativeWriter nw_toc = new NativeWriter(new FileStream(location_toc_file, FileMode.Open)))
                    {
                        foreach (var modChunk in parent.ModifiedChunks)
                        {
                            if (tocSb.TOCFile.TocChunkGuids.Contains(modChunk.Key))
                            {
                                var chunkIndex = tocSb.TOCFile.TocChunks.FindIndex(x => x.Id == modChunk.Key
                                    && modChunk.Value.ModifiedEntry != null
                                    && (modChunk.Value.ModifiedEntry.AddToTOCChunks || modChunk.Value.ModifiedEntry.AddToChunkBundle));
                                if(chunkIndex != -1)
                                {
                                    var data = parent.archiveData[modChunk.Value.Sha1].Data;

                                    var chunkGuid = tocSb.TOCFile.TocChunkGuids[chunkIndex];

                                    var chunk = tocSb.TOCFile.TocChunks[chunkIndex];
                                    DbObject dboChunk = tocSb.TOCFile.TocChunkInfo[modChunk.Key];

                                    using (NativeWriter nw_cas = new NativeWriter(new FileStream(nextCasPath, FileMode.OpenOrCreate)))
                                    {
                                        nw_cas.Position = nw_cas.Length;
                                        var newPosition = nw_cas.Position;

                                        
                                        nw_cas.WriteBytes(data);

                                        nw_toc.Position = dboChunk.GetValue<long>("patchPosition");
                                        nw_toc.Write(Convert.ToByte(patch ? 1 : 0));
                                        nw_toc.Write(Convert.ToByte(catalog));
                                        nw_toc.Write(Convert.ToByte(newCas));

                                        nw_toc.Position = chunk.SB_CAS_Offset_Position;
                                        nw_toc.Write((uint)newPosition, Endian.Big);

                                        nw_toc.Position = chunk.SB_CAS_Size_Position;
                                        nw_toc.Write((uint)data.Length, Endian.Big);
                                    }
                                }
                            }
                        }
                    }



                }
            }

            if(directory == "native_patch")
                ModifyTOCChunks("native_data");
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
                    TocSbReader_Fifa22 tocSb = new TocSbReader_Fifa22(false, false);

                    var location_toc_file_new = UseModData
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

        /*
        private void ProcessAddedTOCChunk(TocSbReader_FIFA21 tocSb, string location_toc_file_new, ushort catalog, ushort cas, bool patch)
        {
            foreach (var aChunk in AddedChunks)
            {
                //tocSb.TOCFile.TocChunkGuids.Add(aChunk.Id);
                //aChunk.ExtraData = new AssetExtraData() { Catalog = catalog, Cas = Convert.ToUInt16(cas), IsPatch = patch };

                tocSb.TOCFile.ListTocChunkFlags.Add(-1);
                //// need to do a "divisable by 3" on this one
                //var newChunkIndex = tocSb.TOCFile.TocChunks.Count * 3;
                //while (tocSb.TOCFile.ChunkIndexToChunkId.ContainsKey(newChunkIndex | 0xFFFFFF))
                //{
                //    newChunkIndex++;
                //}

                tocSb.TOCFile.ChunkIndexToChunkId.Add(TOCFile.CalculateChunkIndexFromListIndex(tocSb.TOCFile.ChunkIndexToChunkId.Count), aChunk.Id);

                var new_casPath = FileSystem.Instance.ResolvePath(FileSystem.Instance.GetFilePath(
                catalog
                , cas
                , patch), true, true);

                long dataPosition = 0;
                using (NativeWriter nativeWriter = new NativeWriter(new FileStream(new_casPath, FileMode.OpenOrCreate)))
                {
                    nativeWriter.Position = nativeWriter.Length;

                    dataPosition = nativeWriter.Position;
                    nativeWriter.Write(aChunk.ModifiedEntry.Data);

                }

                aChunk.ExtraData = new AssetExtraData() { DataOffset = (uint)dataPosition, Cas = (ushort)cas, Catalog = catalog };
                aChunk.ExtraData.DataOffset = (uint)dataPosition;
                aChunk.Size = aChunk.ModifiedEntry.Data.Length;
                tocSb.TOCFile.TocChunks.Add(aChunk);

            }
            AddedChunks.Clear();

            if (tocSb.TOCFile != null)
            {
                var msNewFile = new MemoryStream();
                tocSb.TOCFile.Write(msNewFile);
                File.WriteAllBytes(location_toc_file_new, msNewFile.ToArray());
                // the check
                tocSb.TOCFile.Read(new NativeReader(msNewFile));
            }
        }
        */
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

            if (!FrostyModExecutor.UseModData)
                text = text.Replace("ModData", "", StringComparison.OrdinalIgnoreCase);

            fiCas = null;
            return text;
        }
    }



}
