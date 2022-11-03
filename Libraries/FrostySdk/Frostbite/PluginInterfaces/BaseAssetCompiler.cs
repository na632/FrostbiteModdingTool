using Frostbite.FileManagers;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using ModdingSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

                var legacyFileManager = AssetManager.Instance.GetLegacyAssetManager() as ChunkFileManager2022;
                if (legacyFileManager != null)
                {
                    legacyFileManager.ModifyAssets(legacyData, true);

                    var modifiedLegacyChunks = legacyFileManager.ModifiedChunks;
                    foreach (var modLegChunk in modifiedLegacyChunks)
                    {
                        ModExecuter.ModifiedChunks.Add(modLegChunk.Id, modLegChunk);
                        countLegacyChunksModified++;
                    }

                    foreach (var chunk in modifiedLegacyChunks)
                    {
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

        public void WriteChangesToSuperBundle(DbObject origDbo, NativeWriter writer, KeyValuePair<AssetEntry, (long, int, int, Sha1)> assetBundle)
        {
            if (assetBundle.Key is EbxAssetEntry)
                WriteEbxChangesToSuperBundle(origDbo, writer, assetBundle);
            else if (assetBundle.Key is ResAssetEntry)
                WriteResChangesToSuperBundle(origDbo, writer, assetBundle);
            else if (assetBundle.Key is ChunkAssetEntry)
                WriteChunkChangesToSuperBundle(origDbo, writer, assetBundle);
        }

        private void WriteEbxChangesToSuperBundle(DbObject origEbxDbo, NativeWriter writer, KeyValuePair<AssetEntry, (long, int, int, Sha1)> assetBundle)
        {
            //DbObject origEbxDbo = null;
            //foreach (DbObject dbInBundle in origEbxBundles)
            //{
            //    origEbxDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["name"].ToString() == assetBundle.Key.Name);
            //    if (origEbxDbo != null)
            //        break;
            //}

            if (origEbxDbo != null)
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

        private void WriteResChangesToSuperBundle(DbObject origResDbo, NativeWriter writer, KeyValuePair<AssetEntry, (long, int, int, Sha1)> assetBundle)
        {
            //DbObject origResDbo = null;
            //foreach (DbObject dbInBundle in origResBundles)
            //{
            //    origResDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["name"].ToString() == assetBundle.Key.Name);
            //    if (origResDbo != null)
            //        break;
            //}

            if (origResDbo != null
                && ModExecuter.modifiedRes.ContainsKey(assetBundle.Key.Name)
                && (assetBundle.Key.Type == "SkinnedMeshAsset"
                || assetBundle.Key.Type == "MeshSet"
                || assetBundle.Key.Type == "Texture"))
            {
                writer.BaseStream.Position = origResDbo.GetValue<int>("SB_ResMeta_Position");
                writer.WriteBytes(ModExecuter.modifiedRes[assetBundle.Key.Name].ResMeta);

                if (ModExecuter.modifiedRes[assetBundle.Key.Name].ResRid != 0)
                {
                    writer.BaseStream.Position = origResDbo.GetValue<int>("SB_ReRid_Position");
                    writer.Write(ModExecuter.modifiedRes[assetBundle.Key.Name].ResRid);
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

        private void WriteChunkChangesToSuperBundle(DbObject origChunkDbo, NativeWriter writer, KeyValuePair<AssetEntry, (long, int, int, Sha1)> assetBundle)
        {
            //DbObject origChunkDbo = null;
            //foreach (DbObject dbInBundle in origChunkBundles)
            //{
            //    origChunkDbo = (DbObject)dbInBundle.List.FirstOrDefault(z => ((DbObject)z)["id"].ToString() == assetBundle.Key.Name);
            //    if (origChunkDbo != null)
            //        break;
            //}

            if (Guid.TryParse(assetBundle.Key.Name, out Guid bndleId))
            {
                if (origChunkDbo != null
                    && ModExecuter.ModifiedChunks.ContainsKey(bndleId)
                    )
                {
                    writer.BaseStream.Position = origChunkDbo.GetValue<int>("SB_LogicalOffset_Position");
                    writer.Write(ModExecuter.ModifiedChunks[bndleId].LogicalOffset);
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

        public void ModifyTOCChunks(string directory = "native_patch")
        {
            int sbIndex = -1;
            foreach (var catalogInfo in FileSystem.Instance.EnumerateCatalogInfos())
            {
                foreach (string sbKey in catalogInfo.SuperBundles.Keys)
                {
                    sbIndex++;
                    string tocFile = sbKey;
                    if (catalogInfo.SuperBundles[sbKey])
                    {
                        tocFile = sbKey.Replace("win32", catalogInfo.Name);
                    }

                    // Only handle Legacy stuff right now
                    if (!tocFile.Contains("globals", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var pathToTOCFileRAW = $"{directory}/{tocFile}.toc";
                    string location_toc_file = FileSystem.Instance.ResolvePath(pathToTOCFileRAW);

                    var pathToTOCFile = ModExecutor.UseModData
                        ? location_toc_file
                        .Replace("Data", "ModData\\Data", StringComparison.OrdinalIgnoreCase)
                        .Replace("Patch", "ModData\\Patch", StringComparison.OrdinalIgnoreCase)
                        : location_toc_file;

                    using TOCFile tocFile2 = new TOCFile(new FileStream(location_toc_file, FileMode.Open), false, false);

                    // read the changed toc file in ModData
                    if (!tocFile2.TocChunks.Any())
                        continue;

                    var patch = true;
                    var catalog = tocFile2.TocChunks.Max(x => x.ExtraData.Catalog.Value);
                    if (!tocFile2.TocChunks.Any(x => x.ExtraData.IsPatch))
                        patch = false;

                    var cas = tocFile2.TocChunks.Where(x => x.ExtraData.Catalog == catalog).Max(x => x.ExtraData.Cas.Value);

                    var newCas = cas;
                    //var nextCasPath = GetNextCasInCatalog(catalogInfo, cas, patch, out int newCas);
                    var nextCasPath = FileSystem.Instance.ResolvePath(FileSystem.Instance.GetFilePath(catalog, cas, patch), ModExecutor.UseModData);
                    if (!File.Exists(nextCasPath))
                    {
                        nextCasPath = FileSystem.Instance.ResolvePath(FileSystem.Instance.GetFilePath(catalog, cas, false), ModExecutor.UseModData);
                        patch = false;
                    }
                    if (string.IsNullOrEmpty(nextCasPath))
                    {
                        Debug.WriteLine("Error finding nextCasPath in BaseAssetCompiler.ModifyTOCChunks!");
                        return;
                    }

                    using (NativeWriter nw_cas = new NativeWriter(new FileStream(nextCasPath, FileMode.OpenOrCreate)))
                    {
                        using (NativeWriter nw_toc = new NativeWriter(new FileStream(pathToTOCFile, FileMode.Open)))
                        {
                            foreach (var modChunk in ModExecuter.ModifiedChunks)
                            {
                                if (tocFile2.TocChunkGuids.Contains(modChunk.Key))
                                {
                                    var chunkIndex = tocFile2.TocChunks.FindIndex(x => x.Id == modChunk.Key
                                        && modChunk.Value.ModifiedEntry != null
                                        && (modChunk.Value.ModifiedEntry.AddToTOCChunks || modChunk.Value.ModifiedEntry.AddToChunkBundle));
                                    if (chunkIndex != -1)
                                    {
                                        //var data = parent.archiveData[modChunk.Value.Sha1].Data;
                                        byte[] data = null;
                                        if (ModExecuter.archiveData.ContainsKey(modChunk.Value.ModifiedEntry.Sha1))
                                            data = ModExecuter.archiveData[modChunk.Value.ModifiedEntry.Sha1].Data;
                                        //else if (ModExecuter.archiveData.ContainsKey(modChunk.Value.Sha1))
                                        //    data = ModExecuter.archiveData[modChunk.Value.Sha1].Data;

                                        if (data == null)
                                            continue;

                                        var chunkGuid = tocFile2.TocChunkGuids[chunkIndex];

                                        var chunk = tocFile2.TocChunks[chunkIndex];
                                        DbObject dboChunk = tocFile2.TocChunkInfo[modChunk.Key];

                                        nw_cas.Position = nw_cas.Length;
                                        var newPosition = nw_cas.Position;
                                        nw_cas.WriteBytes(data);
                                        modChunk.Value.Size = data.Length;
                                        modChunk.Value.ExtraData = new AssetExtraData()
                                        {
                                            DataOffset = (uint)newPosition,
                                            Cas = newCas,
                                            Catalog = catalog,
                                            IsPatch = patch,
                                        };

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

                                // Added / Duplicate chunk -- Does nothing at the moment
                                if (modChunk.Value.ExtraData == null && tocFile == "win32/globalsfull")
                                {
                                    var data = ModExecuter.archiveData[modChunk.Value.Sha1].Data;
                                    nw_cas.Position = nw_cas.Length;
                                    var newPosition = nw_cas.Position;
                                    //nw_cas.WriteBytes(data);
                                    modChunk.Value.Size = data.Length;
                                    modChunk.Value.ExtraData = new AssetExtraData()
                                    {
                                        DataOffset = (uint)newPosition,
                                        Cas = newCas,
                                        Catalog = catalog,
                                        IsPatch = patch,
                                    };
                                    //tocSb.TOCFile.TocChunks.Add(modChunk.Value);
                                }
                            }
                        }
                    }

                    //using (var fsToc = new FileStream(pathToTOCFile, FileMode.Open))
                    //{
                    //    tocSb.TOCFile.Write(fsToc);
                    //}
                    TOCFile.RebuildTOCSignatureOnly(pathToTOCFile);
                }
            }

            if (directory == "native_patch")
                ModifyTOCChunks("native_data");
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
