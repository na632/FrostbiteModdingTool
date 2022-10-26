using Frostbite.FileManagers;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using ModdingSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static FrostySdk.Frostbite.PluginInterfaces.BaseAssetCompiler;

namespace FrostySdk.Frostbite.PluginInterfaces
{
    public abstract class BaseAssetCompiler : IAssetCompiler
    {
        public string ModDirectory { get; } = "ModData";
        public string PatchDirectory { get; } = "Patch";

        public Dictionary<ModType, int> ErrorCounts = new Dictionary<ModType, int>();

        public ModExecutor ModExecuter { get; set; }

        public void MakeTOCOriginals(string dir)
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

        public void MakeTOCBackups(string dir)
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

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, ILogger logger)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            var filesToCopy = new List<(FileInfo, FileInfo)>();

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                if (file.Extension.Contains("bak", StringComparison.OrdinalIgnoreCase))
                    continue;

                var targetFile = new FileInfo(targetFilePath);
                if (!file.Extension.Contains("cas", StringComparison.OrdinalIgnoreCase))
                {
                    filesToCopy.Add((file, targetFile));
                }
                else
                {
                    if (!targetFile.Exists)
                    {
                        filesToCopy.Add((file, targetFile));
                        continue;
                    }

                    if(targetFile.Length != file.Length)
                    {
                        filesToCopy.Add((file, targetFile));
                        continue;
                    }

                    if (targetFile.LastWriteTime != file.LastWriteTime)
                    {
                        filesToCopy.Add((file, targetFile));
                        continue;
                    }

                    //if (targetFile.LastAccessTime != file.LastAccessTime)
                    //{
                    //    filesToCopy.Add((file, targetFile));
                    //    continue;
                    //}
                }
            }

            //var index = 1;
            logger.Log($"Data Setup - Copying {sourceDir}");
            foreach(var ftc in filesToCopy)
            {
                ftc.Item1.CopyTo(ftc.Item2.FullName, true);
                //logger.Log($"Data Setup - Copied ({index}/{filesToCopy.Count}) - {ftc.Item1.FullName}");
                //index++;
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true, logger);
                }
            }
        }


        protected static void CopyDataFolder(string from_datafolderpath, string to_datafolderpath, ILogger logger)
        {
            CopyDirectory(from_datafolderpath, to_datafolderpath, true, logger);
            //Directory.CreateDirectory(to_datafolderpath);

            
            //var dataFiles = Directory.EnumerateFiles(from_datafolderpath, "*.*", SearchOption.AllDirectories);
            //var dataFileCount = dataFiles.Count();
            //var indexOfDataFile = 0;
            ////ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
            ////Parallel.ForEach(dataFiles, (f) =>
            //foreach (var originalFilePath in dataFiles)
            //{
            //    var finalDestinationPath = originalFilePath.ToLower().Replace(from_datafolderpath.ToLower(), to_datafolderpath.ToLower());

            //    bool Copied = false;

            //    var lastIndexOf = finalDestinationPath.LastIndexOf("\\");
            //    var newDirectory = finalDestinationPath.Substring(0, lastIndexOf) + "\\";
            //    if (!Directory.Exists(newDirectory))
            //    {
            //        Directory.CreateDirectory(newDirectory);
            //    }


            //    if (!finalDestinationPath.Contains("moddata", StringComparison.OrdinalIgnoreCase))
            //    {
            //        throw new Exception("Incorrect Copy of Files to ModData");
            //    }

            //    var fIDest = new FileInfo(finalDestinationPath);
            //    var fIOrig = new FileInfo(originalFilePath);

            //    var isPatch = fIOrig.FullName.Contains("Patch", StringComparison.OrdinalIgnoreCase);

            //    if (fIDest.Exists && finalDestinationPath.Contains("moddata", StringComparison.OrdinalIgnoreCase))
            //    {
            //        var isCas = fIDest.Extension.Contains("cas", StringComparison.OrdinalIgnoreCase);
            //        var twogbinsize = 2L * (1048576L * 1024L);
            //        if (
            //            isCas
            //            && fIDest.Length != fIOrig.Length
            //            && (isPatch || (fIDest.Length >= twogbinsize))
            //            )
            //        {
            //            fIDest.Delete();
            //        }
            //        else if
            //            (
            //                !isCas
            //            //&&
            //            //(
            //            //    fIDest.Length != fIOrig.Length
            //            //    ||
            //            //        (
            //            //            //fIDest.LastWriteTime.Day != fIOrig.LastWriteTime.Day
            //            //            //&& fIDest.LastWriteTime.Hour != fIOrig.LastWriteTime.Hour
            //            //            //&& fIDest.LastWriteTime.Minute != fIOrig.LastWriteTime.Minute
            //            //            !File.ReadAllBytes(finalDestinationPath).SequenceEqual(File.ReadAllBytes(originalFilePath))
            //            //        )
            //            //)
            //            )
            //        {
            //            File.Delete(finalDestinationPath);
            //        }
            //    }

            //    if (!File.Exists(finalDestinationPath))
            //    {
            //        // Quick Copy
            //        if (fIOrig.Length < 1024 * 100)
            //        {
            //            using (var inputStream = new NativeReader(File.Open(originalFilePath, FileMode.Open)))
            //            using (var outputStream = new NativeWriter(File.Open(finalDestinationPath, FileMode.Create)))
            //            {
            //                outputStream.Write(inputStream.ReadToEnd());
            //            }
            //        }
            //        else
            //        {
            //            //File.Copy(f, finalDestination);
            //            CopyFile(originalFilePath, finalDestinationPath);
            //        }
            //        Copied = true;
            //    }
            //    indexOfDataFile++;

            //    if (Copied)
            //        logger.Log($"Data Setup - Copied ({indexOfDataFile}/{dataFileCount}) - {originalFilePath}");
            //    //});
            //}
        }

        protected Dictionary<string, List<ModdedFile>> GetModdedCasFiles()
        {
            // Handle Legacy first to generate modified chunks
            ProcessLegacyMods();
            // ------ End of handling Legacy files ---------

            Dictionary<string, List<ModdedFile>> casToMods = new Dictionary<string, List<ModdedFile>>();
            foreach (var mod in ModExecuter.ModifiedAssets)
            {
                AssetEntry originalEntry = null;
                if (mod.Value is EbxAssetEntry)
                    originalEntry = AssetManager.Instance.GetEbxEntry(mod.Value.Name);
                else if (mod.Value is ResAssetEntry)
                    originalEntry = AssetManager.Instance.GetResEntry(mod.Value.Name);
                else if (mod.Value is ChunkAssetEntry)
                    originalEntry = AssetManager.Instance.GetChunkEntry(Guid.Parse(mod.Value.Name));

                if (originalEntry == null)
                    continue;

                if (originalEntry.ExtraData == null || string.IsNullOrEmpty(originalEntry.ExtraData.CasPath))
                    continue;

                if (mod.Value is ChunkAssetEntry chunkAssetEntry)
                {

                }

                var casPath = originalEntry.ExtraData.CasPath;
                if (!casToMods.ContainsKey(casPath))
                    casToMods.Add(casPath, new List<ModdedFile>());

                casToMods[casPath].Add(new ModdedFile(mod.Value.Sha1, mod.Value.Name, false, mod.Value, originalEntry));

            }

            return casToMods;
        }

        protected void ProcessLegacyMods()
        {
            List<Guid> ChunksToRemove = new List<Guid>();

            // -----------------------------------------------------------
            // process modified legacy chunks and make live changes
            if (ModExecuter.modifiedLegacy.Count > 0)
            {
                ModExecuter.Logger.Log($"Legacy :: {ModExecuter.modifiedLegacy.Count} Legacy files found. Modifying associated chunks");

                Dictionary<string, byte[]> legacyData = ModExecuter.modifiedLegacy.ToDictionary(x => x.Key, x => x.Value.ModifiedEntry.Data);
                var countLegacyChunksModified = 0;
                //foreach (var modLegacy in ModExecuter.modifiedLegacy)
                //{
                //    byte[] data = null;
                    
                //    if (ModExecuter.archiveData.ContainsKey(modLegacy.Value.Sha1))
                //        data = ModExecuter.archiveData[modLegacy.Value.Sha1].Data;

                //    if (data != null)
                //    {
                //        legacyData.Add(modLegacy.Key, data);
                //    }
                //}

                var legacyFileManager = AssetManager.Instance.GetLegacyAssetManager() as LegacyFileManager_FMTV2;
                if (legacyFileManager != null)
                {
                    legacyFileManager.ModifyAssets(legacyData, true);

                    //var modifiedLegacyChunks = AssetManager.Instance.EnumerateChunks(true);
                    var modifiedLegacyChunks = legacyFileManager.ModifiedChunks;
                    foreach (var modLegChunk in modifiedLegacyChunks)
                    {
                        //modLegChunk.Sha1 = modLegChunk.ModifiedEntry.Sha1;
                        //if(ModExecuter.ModifiedChunks.ContainsKey(modLegChunk.Id))
                        //    ModExecuter.ModifiedChunks.Remove(modLegChunk.Id);

                        ModExecuter.ModifiedChunks.Add(modLegChunk.Id, modLegChunk);
                        countLegacyChunksModified++;
                    }

                    //var modifiedChunks = AssetManager.Instance.EnumerateChunks(true);
                    //foreach (var chunk in ModExecuter.ModifiedChunks)
                    foreach (var chunk in modifiedLegacyChunks)
                        //foreach (var chunk in AssetManager.Instance.EnumerateChunks(true))
                    {
                        //if (ModExecuter.archiveData.ContainsKey(chunk.Sha1))
                        //    ModExecuter.archiveData.Remove(chunk.Sha1);
                        //ModExecuter.archiveData[chunk.Value.ModifiedEntry.Sha1] = new ArchiveInfo() { Data = chunk.Value.ModifiedEntry.Data };
                        //else

                        //if (!ModExecuter.archiveData.ContainsKey(chunk.Sha1))
                            ModExecuter.archiveData.Add(chunk.ModifiedEntry.Sha1, new ArchiveInfo() { Data = chunk.ModifiedEntry.Data });
                    }
                    ModExecuter.Logger.Log($"Legacy :: Modified {countLegacyChunksModified} associated chunks");
                }
            }
        }

        public virtual bool Compile(FileSystem fs, ILogger logger, ModExecutor modExecuter)
        {
            ModExecuter = modExecuter;
            return false;
        }

        public virtual bool Cleanup(FileSystem fs, ILogger logger, ModExecutor modExecuter)
        {
            return false;
        }

        public struct ModdedFile
        {
            public Sha1 Sha1 { get; set; }
            public string NamePath { get; set; }
            public ModType ModType { get; set; }
            public bool IsAdded { get; set; }
            public AssetEntry ModEntry { get; set; }
            public AssetEntry OriginalEntry { get; set; }

            public ModdedFile(Sha1 inSha1, string inNamePath, bool inAdded)
            {
                ModType = ModType.EBX;
                Sha1 = inSha1;
                NamePath = inNamePath;
                IsAdded = inAdded;
                OriginalEntry = null;
                ModEntry = null;
            }

            public ModdedFile(Sha1 inSha1, string inNamePath, bool inAdded, AssetEntry inModEntry, AssetEntry inOrigEntry)
            {
                Sha1 = inSha1;
                NamePath = inNamePath;

                ModType = ModType.EBX;
                if(inOrigEntry is EbxAssetEntry)
                    ModType = ModType.EBX;
                else if (inOrigEntry is ResAssetEntry)
                    ModType = ModType.RES;
                else if (inOrigEntry is ChunkAssetEntry)
                    ModType = ModType.CHUNK;

                IsAdded = inAdded;
                OriginalEntry = inOrigEntry;
                ModEntry = inModEntry;
            }

            public ModdedFile(Sha1 inSha1, string inNamePath, ModType inModType, bool inAdded)
            {
                Sha1 = inSha1;
                NamePath = inNamePath;
                ModType = inModType;
                IsAdded = inAdded;
                OriginalEntry = null;
                ModEntry = null;
            }

            public ModdedFile(Sha1 inSha1, string inNamePath, ModType inModType, bool inAdded, AssetEntry inOrigEntry)
            {
                Sha1 = inSha1;
                NamePath = inNamePath;
                ModType = inModType;
                IsAdded = inAdded;
                OriginalEntry = inOrigEntry;
                ModEntry = null;
            }

            public override string ToString()
            {
                return $"[{ModType.ToString()}]({NamePath})";
            }

        }

        public enum ModType
        {
            EBX,
            RES,
            CHUNK
        }

    }

    

}
