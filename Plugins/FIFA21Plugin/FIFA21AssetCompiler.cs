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

namespace FIFA21Plugin
{

    /// <summary>
    /// FIFA 21 Asset Compiler. Solid and works. Uses .cache file to determine what needs editing
    /// Linked to FIFA21BundleAction
    /// </summary>
    public class FIFA21AssetCompiler : IAssetCompiler
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
            if (!ProfilesLibrary.IsFIFA21DataVersion())
            {
                logger.Log("[ERROR] Wrong compiler used for Game");
                return false;
            }

           

            if (!FrostyModExecutor.UseModData)
            {
                return RunEADesktopCompiler(fs, logger, frostyModExecuter);
            }
            return RunOriginCompiler(fs, logger, frostyModExecuter);
        }

        private bool RunOriginCompiler(FileSystem fs, ILogger logger, object frostyModExecuter)
        {
            // Notify the Bundle Action of the Cas File Count
            FIFA21BundleAction.CasFileCount = fs.CasFileCount;

            if (!Directory.Exists(fs.BasePath))
                throw new DirectoryNotFoundException($"Unable to find the correct base path directory of {fs.BasePath}");

            Directory.CreateDirectory(fs.BasePath + ModDirectory + "\\Data");
            Directory.CreateDirectory(fs.BasePath + ModDirectory + "\\Patch");

            var fme = (FrostyModExecutor)frostyModExecuter;

            logger.Log("Copying files from Data to ModData/Data");
            CopyDataFolder(fs.BasePath + "\\Data\\", fs.BasePath + ModDirectory + "\\Data\\", logger);
            logger.Log("Copying files from Patch to ModData/Patch");
            CopyDataFolder(fs.BasePath + PatchDirectory, fs.BasePath + ModDirectory + "\\" + PatchDirectory, logger);

            FIFA21BundleAction fifaBundleAction = new FIFA21BundleAction(fme);
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
            FIFA21BundleAction fifaBundleAction = new FIFA21BundleAction(fme, false);
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
    public class FIFA21BundleAction
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

        private static readonly object locker = new object();

        public static int CasFileCount = 0;

        private Exception errorException;

        private FrostyModExecutor parent;

        private Catalog catalogInfo;

        private Dictionary<int, string> casFiles = new Dictionary<int, string>();

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

        public FIFA21BundleAction(FrostyModExecutor inParent, bool useModData = true)
        {
            parent = inParent;
            ErrorCounts.Add(ModType.EBX, 0);
            ErrorCounts.Add(ModType.RES, 0);
            ErrorCounts.Add(ModType.CHUNK, 0);
            UseModData = useModData;
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
                    if (casPath.Contains("native_patch"))
                    {

                    }

                    if (!casToMods.ContainsKey(casPath))
                    {
                        casToMods.Add(casPath, new List<ModdedFile>() { new ModdedFile(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, false, originalEntry) });
                    }
                    else
                    {
                        casToMods[casPath].Add(new ModdedFile(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, false, originalEntry));
                    }
                    //// Is Added
                    //else
                    //{
                    //    if (!casToMods.ContainsKey(string.Empty))
                    //    {
                    //        casToMods.Add(string.Empty, new List<ModdedFile>() { new ModdedFile(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, true) });
                    //    }
                    //    else
                    //    {
                    //        casToMods[string.Empty].Add(new ModdedFile(modEBX.Value.Sha1, modEBX.Value.Name, ModType.EBX, true));
                    //    }
                    //}
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
                    //// Is Added
                    //else
                    //{
                    //    if (!casToMods.ContainsKey(string.Empty))
                    //    {
                    //        casToMods.Add(string.Empty, new List<ModdedFile>() { new ModdedFile(modRES.Value.Sha1, modRES.Value.Name, ModType.EBX, true) });
                    //    }
                    //    else
                    //    {
                    //        casToMods[string.Empty].Add(new ModdedFile(modRES.Value.Sha1, modRES.Value.Name, ModType.EBX, true));
                    //    }
                    //}
                }
                else
                {
                    ErrorCounts[ModType.RES]++;
                }
            }
            foreach (var modChunks in parent.ModifiedChunks)
            {
                var originalEntry = AssetManager.Instance.GetChunkEntry(modChunks.Key);

                if (originalEntry != null && originalEntry.ExtraData != null && originalEntry.ExtraData.CasPath != null)
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
                else
                {
                    AddedChunks.Add(modChunks.Value);
                    //parent.Logger.LogWarning($"This mod compiler cannot handle Added Chunks. {modChunks.Key} will be ignored.");
                    //ErrorCounts[ModType.CHUNK]++;

                    //throw new Exception($"Unable to find CAS file to edit for Chunk {originalEntry.Id}");
                    //parent.Logger.LogWarning($"Unable to find CAS file to edit for Chunk {modChunks.Key}");
                    //parent.Logger.LogWarning("Unable to apply Chunk Entry for mod");
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

                AssetManager.Instance.ModifyLegacyAssets(legacyData, true);

                var modifiedLegacyChunks = AssetManager.Instance.EnumerateChunks(true);
                foreach (var modLegChunk in modifiedLegacyChunks)
                {
                    if(modLegChunk.Id.ToString() == "f0ca4187-b95e-5153-a1eb-1e0a7fff6371")
                    {

                    }
                    modLegChunk.Sha1 = modLegChunk.ModifiedEntry.Sha1;
                    if (!parent.ModifiedChunks.ContainsKey(modLegChunk.Id))
                    {
                        parent.ModifiedChunks.Add(modLegChunk.Id, modLegChunk);
                    }
                    else
                    {
                        parent.ModifiedChunks[modLegChunk.Id] = modLegChunk;
                    }
                    countLegacyChunksModified++;
                }

                var modifiedChunks = AssetManager.Instance.EnumerateChunks(true);
                foreach (var chunk in modifiedChunks)
                {
                    if (chunk.Id.ToString() == "f0ca4187-b95e-5153-a1eb-1e0a7fff6371")
                    {

                    }

                    if (parent.archiveData.ContainsKey(chunk.Sha1))
                        parent.archiveData[chunk.Sha1] = new ArchiveInfo() { Data = chunk.ModifiedEntry.Data };
                    else
                        parent.archiveData.TryAdd(chunk.Sha1, new ArchiveInfo() { Data = chunk.ModifiedEntry.Data });
                }
                parent.Logger.Log($"Legacy :: Modified {countLegacyChunksModified} associated chunks");
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

            BuildCache buildCache = new BuildCache();
            buildCache.LoadData(ProfilesLibrary.ProfileName, parent.GamePath, parent.Logger, false, true);

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

                foreach (var item in dictOfModsToCas)
                {
                    var casPath = string.Empty;

                    if (!string.IsNullOrEmpty(item.Key))
                    {
                        casPath = item.Key.Replace("native_data"
                            , AssetManager.Instance.fs.BasePath + "ModData\\Data", StringComparison.OrdinalIgnoreCase);
                    }

                    casPath = casPath.Replace("native_patch"
                        , AssetManager.Instance.fs.BasePath + "ModData\\Patch", StringComparison.OrdinalIgnoreCase);

                    if (!casPath.Contains("ModData"))
                    {
                        throw new Exception($"WRONG CAS PATH GIVEN! {casPath}");
                    }

                    if (!UseModData)
                    {
                        casPath = casPath.Replace("ModData\\", "", StringComparison.OrdinalIgnoreCase);
                    }

                    Debug.WriteLine($"Modifying CAS file - {casPath}");
                    parent.Logger.Log($"Modifying CAS file - {casPath}");

                    Dictionary<AssetEntry, (long, int, int, Sha1)> EntriesToNewPosition = new Dictionary<AssetEntry, (long, int, int, Sha1)>();

                    using (NativeWriter nwCas = new NativeWriter(new FileStream(casPath, FileMode.Open)))
                    {
                        foreach (var modItem in item.Value.OrderBy(x => x.NamePath))
                        {
                            nwCas.Position = nwCas.Length;
                            byte[] data = new byte[0];
                            AssetEntry originalEntry = modItem.OriginalEntry;

                            if (modItem.NamePath.Contains("3e3ea546-1d18-6ed0-c3e4-2af56e6e8b6d"))
                            {

                            }

                            if (modItem.NamePath.Contains("f0ca4187-b95e-5153-a1eb-1e0a7fff6371"))
                            {

                            }

                            if (originalEntry != null && parent.archiveData.ContainsKey(modItem.Sha1))
                            {
                                data = parent.archiveData[modItem.Sha1].Data;
                            }
                            else
                            {
                                parent.Logger.LogError($"Unable to find original archive data for {modItem.NamePath}");
                                continue;
                                //throw new Exception()
                            }

                            if (data.Length == 0)
                            {
                                parent.Logger.LogError($"Unable to find any data for {modItem.NamePath}");
                                continue;
                            }

                            var origSize = 0;
                            var positionOfData = nwCas.Position;
                            // write the new data to end of the file (this should be fine)
                            nwCas.Write(data);

                            switch (modItem.ModType)
                            {
                                case ModType.EBX:
                                    origSize = Convert.ToInt32(parent.modifiedEbx[modItem.NamePath].OriginalSize);
                                    break;
                                case ModType.RES:
                                    origSize = Convert.ToInt32(parent.modifiedRes[modItem.NamePath].OriginalSize);
                                    break;
                                case ModType.CHUNK:
                                    origSize = Convert.ToInt32(parent.ModifiedChunks[Guid.Parse(modItem.NamePath)].OriginalSize);
                                    break;
                            }
                            if (origSize == 0
                                || origSize == data.Length)
                            {
                                var out_data = new CasReader(new MemoryStream(data)).Read();
                                origSize = out_data.Length;
                            }

                            var useCas = string.IsNullOrEmpty(originalEntry.SBFileLocation);
                            if (useCas && (originalEntry is EbxAssetEntry || originalEntry is ResAssetEntry))
                            {
                                if (originalEntry.SB_OriginalSize_Position != 0 && origSize != 0)
                                {
                                    nwCas.Position = originalEntry.SB_OriginalSize_Position;
                                    nwCas.Write((uint)origSize, Endian.Little);
                                }

                                if (originalEntry.SB_Sha1_Position != 0 && modItem.Sha1 != Sha1.Zero)
                                {
                                    nwCas.Position = originalEntry.SB_Sha1_Position;
                                    nwCas.Write(modItem.Sha1);
                                }
                            }

                            EntriesToNewPosition.Add(originalEntry, (positionOfData, data.Length, origSize, modItem.Sha1));
                        }

                    }

                    var groupedBySB = EntriesToNewPosition.GroupBy(x =>
                                !string.IsNullOrEmpty(x.Key.SBFileLocation)
                                ? x.Key.SBFileLocation
                                : x.Key.TOCFileLocation
                                )
                        .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());

                    foreach (var sbGroup in groupedBySB)
                    {
                        var sbpath = sbGroup.Key;
                        sbpath = parent.fs.ResolvePath(sbpath).ToLower();
                        if (UseModData)
                        {
                            sbpath = sbpath.ToLower().Replace("\\patch", "\\ModData\\Patch".ToLower(), StringComparison.OrdinalIgnoreCase);
                            sbpath = sbpath.ToLower().Replace("\\data", "\\ModData\\Data".ToLower(), StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            var originalFile = sbpath + ".bak";

                            // First run. Create backup of original file
                            if (!File.Exists(originalFile))
                                File.Copy(sbpath, originalFile);

                            // Later runs. Copy back vanilla before changes.
                            if (File.Exists(originalFile))
                                File.Copy(originalFile, sbpath, true);
                        }
                        if (UseModData && !sbpath.ToLower().Contains("moddata", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new Exception($"WRONG SB PATH GIVEN! {sbpath}");
                        }

                        var tocSbReader = new TocSbReader_FIFA21();
                        tocSbReader.DoLogging = false;
                        tocSbReader.ProcessData = false;


                        DbObject dboOriginal = null;
                        if (!SbToDbObject.ContainsKey(sbGroup.Key)//)
                            // This needs to go. Probably be replaced by the new compiler anyway
                            && !sbpath.Contains(".toc", StringComparison.OrdinalIgnoreCase))
                        {
                            var timeStarted = DateTime.Now;
                            
                            var dboOriginal2 = tocSbReader.Read(sbpath.Replace(".sb", ".toc", StringComparison.OrdinalIgnoreCase), 0, new BinarySbDataHelper(AssetManager.Instance), sbpath);

                            SbToDbObject.Add(sbGroup.Key, new DbObject(dboOriginal2));
                            Debug.WriteLine("Time Taken to Read SB: " + (DateTime.Now - timeStarted).ToString());
                        }
                        //else if (!SbToDbObject.ContainsKey(sbGroup.Key) && sbpath.Contains(".toc", StringComparison.OrdinalIgnoreCase))
                        //{

                        //}



                        if (SbToDbObject.ContainsKey(sbGroup.Key))
                            dboOriginal = SbToDbObject[sbGroup.Key];

                        using (NativeWriter nw_sb = new NativeWriter(new FileStream(sbpath, FileMode.Open)))
                        {
                            foreach (var assetBundle in sbGroup.Value)
                            {
                                if (dboOriginal != null)
                                {
                                    var origEbxBundles = dboOriginal.List.Where(x => ((DbObject)x).HasValue("ebx")).Select(x => ((DbObject)x).GetValue<DbObject>("ebx")).ToList();

                                    var origResBundles = dboOriginal.List.Where(x => ((DbObject)x).HasValue("res")).Select(x => ((DbObject)x).GetValue<DbObject>("res")).ToList();
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
                                        nw_sb.BaseStream.Position = origResDbo.GetValue<int>("SB_ResMeta_Position");
                                        nw_sb.WriteBytes(parent.modifiedRes[assetBundle.Key.Name].ResMeta);
                                    }
                                    else if (origResDbo != null)
                                    {

                                    }

                                    var origChunkBundles = dboOriginal.List.Where(x => ((DbObject)x).HasValue("chunks")).Select(x => ((DbObject)x).GetValue<DbObject>("chunks")).ToList();
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
                                            nw_sb.BaseStream.Position = origChunkDbo.GetValue<int>("SB_LogicalOffset_Position");
                                            nw_sb.Write(parent.ModifiedChunks[bndleId].LogicalOffset);
                                        }
                                    }
                                }

                                var positionOfNewData = assetBundle.Value.Item1;
                                var sizeOfData = assetBundle.Value.Item2;
                                var originalSizeOfData = assetBundle.Value.Item3;
                                var sha = assetBundle.Value.Item4;

                                var sb_cas_size_position = assetBundle.Key.SB_CAS_Size_Position;
                                var sb_cas_offset_position = assetBundle.Key.SB_CAS_Offset_Position;
                                nw_sb.BaseStream.Position = sb_cas_offset_position;
                                nw_sb.Write((uint)positionOfNewData, Endian.Big);
                                nw_sb.Write((uint)sizeOfData, Endian.Big);

                                if (!sbpath.Contains(".toc", StringComparison.OrdinalIgnoreCase))
                                {
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

                        //if (tocSbReader.TOCFile != null)
                        //{
                        //    // TOC Chunks
                        //    using (NativeWriter nw_toc = new NativeWriter(new FileStream(tocSbReader.TOCFile.FileLocation, FileMode.Open)))
                        //    {
                        //        if (tocSbReader != null && tocSbReader.TOCFile != null)
                        //        {
                        //            foreach (var assetBundle in sbGroup.Value)
                        //            {
                        //                if (Guid.TryParse(assetBundle.Key.Name, out Guid chunkId))
                        //                {
                        //                    if (tocSbReader.TOCFile.tocChunkGuids.Contains(chunkId))
                        //                    {
                        //                        var chunk = tocSbReader.TOCFile.TocChunks.FirstOrDefault(x => x.Id == chunkId);
                        //                        if (chunk != null)
                        //                        {
                        //                            var positionOfNewData = assetBundle.Value.Item1;
                        //                            var sizeOfData = assetBundle.Value.Item2;
                        //                            var originalSizeOfData = assetBundle.Value.Item3;
                        //                            var sha = assetBundle.Value.Item4;

                        //                            nw_toc.Position = chunk.SB_CAS_Offset_Position;
                        //                            nw_toc.Write((uint)positionOfNewData, Endian.Big);

                        //                            nw_toc.Position = chunk.SB_CAS_Size_Position;
                        //                            nw_toc.Write((uint)sizeOfData, Endian.Big);
                        //                        }
                        //                    }
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                    
                    }


                }

                // Add chunks to globals

            }


            ModifyTOCChunks();

            return true;
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        private void ModifyTOCChunks(string directory = "native_data")
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

                    // Only handle Legacy stuff right now
                    if(!tocFile.Contains("globals", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var tocFileRAW = $"{directory}/{tocFile}.toc";
                    string location_toc_file = parent.fs.ResolvePath(tocFileRAW).ToLower();
                    TocSbReader_FIFA21 tocSb = new TocSbReader_FIFA21();
                    tocSb.DoLogging = false;
                    tocSb.ProcessData = false;

                    var location_toc_file_new = UseModData
                        ? location_toc_file
                        .Replace("Data", "ModData\\Data", StringComparison.OrdinalIgnoreCase)
                        .Replace("Patch", "ModData\\Patch", StringComparison.OrdinalIgnoreCase)
                        : location_toc_file;

                    // read the changed toc file in ModData
                    tocSb.Read(location_toc_file_new, 0, new BinarySbDataHelper(AssetManager.Instance), tocFileRAW);

                    // Full Proper Rewrite
                    //if (tocSb.TOCFile != null)
                    //{
                    //    var msNewFile = new MemoryStream();
                    //    tocSb.TOCFile.Write(msNewFile);
                    //    File.WriteAllBytes(location_toc_file_new, msNewFile.ToArray());
                    //}

                    var catalog = tocSb.TOCFile.TocChunks[0].ExtraData.Catalog.Value;
                    var cas = tocSb.TOCFile.TocChunks[0].ExtraData.Cas.Value;
                    var patch = true;
                    var firstCasInData = FileSystem.Instance.GetFilePath(
                        catalog
                        , cas
                        , patch);

                    using (NativeWriter nw_toc = new NativeWriter(new FileStream(location_toc_file_new, FileMode.Open)))
                    {
                        foreach (var modChunk in parent.ModifiedChunks)
                        {
                            if (tocSb.TOCFile.tocChunkGuids.Contains(modChunk.Key))
                            {
                                var chunk = tocSb.TOCFile.TocChunks.FirstOrDefault(x => x.Id == modChunk.Key);
                                if (chunk != null)
                                {
                                    DbObject dboChunk = tocSb.TOCFile.TocChunkInfo[modChunk.Key];
                                    var resolvedCasPath = FileSystem.Instance.ResolvePath(chunk.ExtraData.CasPath, UseModData);

                                    using (NativeWriter nw_cas = new NativeWriter(new FileStream(resolvedCasPath, FileMode.Open)))
                                    {
                                        nw_cas.Position = nw_cas.Length;
                                        var newPosition = nw_cas.Position;

                                        var data = parent.archiveData[modChunk.Value.Sha1].Data;
                                        nw_cas.WriteBytes(data);

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

            if(directory == "native_data")
                ModifyTOCChunks("native_patch");
        }
    }



}
