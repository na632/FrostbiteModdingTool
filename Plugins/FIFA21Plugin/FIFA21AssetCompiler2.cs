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
using static FIFA21Plugin.FIFA21AssetLoader;
using static paulv2k4ModdingExecuter.FrostyModExecutor;

namespace FIFA21Plugin
{

    /// <summary>
    /// FIFA 21 Asset Compiler. Solid and works. Uses .cache file to determine what needs editing
    /// Linked to FIFA21BundleAction
    /// </summary>
    public class FIFA21AssetCompiler2 : IAssetCompiler
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

            parent = (FrostyModExecutor)frostyModExecuter;
            UseModData = FrostyModExecutor.UseModData;
            //if (!FrostyModExecutor.UseModData)
            //{
            //    return RunEADesktopCompiler(fs, logger, frostyModExecuter);
            //}
            //return RunOriginCompiler(fs, logger, frostyModExecuter);

            if (!Directory.Exists(fs.BasePath))
                throw new DirectoryNotFoundException($"Unable to find the correct base path directory of {fs.BasePath}");

            Directory.CreateDirectory(fs.BasePath + ModDirectory + "\\Data");
            Directory.CreateDirectory(fs.BasePath + ModDirectory + "\\Patch");

            var fme = (FrostyModExecutor)frostyModExecuter;

            logger.Log("Copying files from Data to ModData/Data");
            CopyDataFolder(fs.BasePath + "\\Data\\", fs.BasePath + ModDirectory + "\\Data\\", logger);
            logger.Log("Copying files from Patch to ModData/Patch");
            CopyDataFolder(fs.BasePath + PatchDirectory, fs.BasePath + ModDirectory + "\\" + PatchDirectory, logger);



            // Handle Legacy first to generate modified chunks
            ProcessLegacyMods();
            // ------ End of handling Legacy files ---------

            return RunBundleCompiler(fs, logger, frostyModExecuter);
        }

        Catalog currentCatalog;

        private bool RunBundleCompiler(FileSystem fs, ILogger logger, object frostyModExecuter, string directory = "native_data")
        {
            if (!UseModData)
                throw new Exception("Must use ModData for this method");

            string LastCasPath = null;
            NativeReader nrCas = null;


            NativeWriter writer_new_cas_file = null;
            int casFileIndex = 1;


            DateTime startTime = DateTime.Now;
            var ModExecuter = (FrostyModExecutor)frostyModExecuter;
            foreach (var catalogInfo in FileSystem.Instance.EnumerateCatalogInfos())
            {
                currentCatalog = catalogInfo;


                byte[] key_2_from_key_manager = KeyManager.Instance.GetKey("Key2");
                foreach (string key3 in catalogInfo.SuperBundles.Keys)
                {
                    string tocFile = key3;
                    if (catalogInfo.SuperBundles[key3])
                    {
                        tocFile = key3.Replace("win32", catalogInfo.Name);
                    }

                    var tocFileRAW = $"{directory}/{tocFile}.toc";
                    string location_toc_file = ModExecuter.fs.ResolvePath(tocFileRAW).ToLower();
                    TocSbReader_FIFA21 tocSb = new TocSbReader_FIFA21();
                    tocSb.DoLogging = false;
                    tocSb.ProcessData = false;
                    tocSb.Read(location_toc_file, 0, tocFileRAW);

                    var location_toc_file_new = UseModData 
                        ? location_toc_file
                        .Replace("Data", "ModData\\Data", StringComparison.OrdinalIgnoreCase)
                        .Replace("Patch", "ModData\\Patch", StringComparison.OrdinalIgnoreCase)
                        : location_toc_file;

                    if (tocSb.TOCFile != null)
                    {
                        var location_sb_file = location_toc_file.Replace(".toc", ".sb", StringComparison.OrdinalIgnoreCase);
                        if (tocSb.SBFile != null)
                        {
                            bool modified = false;
                            Dictionary<DbObject, (int, int)> newSBFileWithPositions = new Dictionary<DbObject, (int, int)>();
                            MemoryStream msNewFile = new MemoryStream();
                            using (NativeReader nativeReader = new NativeReader(new FileStream(location_sb_file, FileMode.Open, FileAccess.Read)))
                            {
                                if (nativeReader.Length > 1)
                                {
                                    var SBFile = new SBFile(tocSb, tocSb.TOCFile, 0);
                                    SBFile.DoLogging = false;
                                    var dboSBFile = SBFile.Read(nativeReader);
                                    if (dboSBFile != null)
                                    {
                                        foreach (var b in dboSBFile)
                                        {
                                            if (b.HasValue("ebx"))
                                            {
                                                foreach (DbObject ebx in b.GetValue<DbObject>("ebx"))
                                                {
                                                    var ebxName = ebx.GetValue<string>("name");
                                                    if (parent.modifiedEbx.ContainsKey(ebxName))
                                                    {
                                                        modified = true;
                                                        EbxAssetEntry ebxAssetEntry = ModExecuter.modifiedEbx[ebxName];
                                                        if (writer_new_cas_file == null)
                                                        {
                                                            writer_new_cas_file?.Close();
                                                            writer_new_cas_file = GetNextCas(ebx.GetValue<int>("catalog"), out casFileIndex);
                                                        }
                                                        ebx.SetValue("originalSize", ebxAssetEntry.OriginalSize);
                                                        ebx.SetValue("size", ModExecuter.archiveData[ebxAssetEntry.Sha1].Data.Length);
                                                        ebx.SetValue("cas", casFileIndex);
                                                        ebx.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                                        var ebxData = ModExecuter.archiveData[ebxAssetEntry.Sha1].Data;

                                                        writer_new_cas_file.Write(ebxData);
                                                    }
                                                }
                                                
                                            }

                                            if (b.HasValue("res"))
                                            {
                                                foreach (DbObject res in b.GetValue<DbObject>("res"))
                                                {
                                                    var resName = res.GetValue<string>("name");
                                                    if (parent.modifiedRes.ContainsKey(resName))
                                                    {
                                                        modified = true;
                                                        EbxAssetEntry ebxAssetEntry = ModExecuter.modifiedEbx[resName];
                                                        if (writer_new_cas_file == null)
                                                        {
                                                            writer_new_cas_file?.Close();
                                                            writer_new_cas_file = GetNextCas(res.GetValue<int>("catalog"), out casFileIndex);
                                                        }
                                                        res.SetValue("originalSize", ebxAssetEntry.OriginalSize);
                                                        res.SetValue("size", ModExecuter.archiveData[ebxAssetEntry.Sha1].Data.Length);
                                                        res.SetValue("cas", casFileIndex);
                                                        res.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                                        var ebxData = ModExecuter.archiveData[ebxAssetEntry.Sha1].Data;

                                                        writer_new_cas_file.Write(ebxData);
                                                    }
                                                }

                                            }

                                            if (b.HasValue("chunks"))
                                            {
                                                foreach (DbObject chunk in b.GetValue<DbObject>("chunks"))
                                                {
                                                    var chunkId = chunk.GetValue<Guid>("id");
                                                    if (parent.ModifiedChunks.ContainsKey(chunkId))
                                                    {
                                                        ChunkAssetEntry chunkAssetEntry = parent.ModifiedChunks[chunkId];
                                                        if (writer_new_cas_file == null)
                                                        {
                                                            writer_new_cas_file?.Close();
                                                            writer_new_cas_file = GetNextCas(chunk.GetValue<int>("catalog"), out casFileIndex);
                                                        }
                                                        chunk.SetValue("originalSize", chunkAssetEntry.OriginalSize);
                                                        chunk.SetValue("size", ModExecuter.archiveData[chunkAssetEntry.Sha1].Data.Length);
                                                        chunk.SetValue("cas", casFileIndex);
                                                        chunk.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                                        chunk.SetValue("logicalOffset", chunkAssetEntry.LogicalOffset);
                                                        chunk.SetValue("logicalSize", chunkAssetEntry.LogicalSize);
                                                        writer_new_cas_file.Write(ModExecuter.archiveData[chunkAssetEntry.Sha1].Data);
                                                    }
                                                }
                                            }
                                        }

                                        if(modified)
                                            newSBFileWithPositions = SBFile.Write(dboSBFile, out msNewFile);
                                       
                                    }

                                }
                            }
                            if (modified)
                            {
                                var location_sb_file_new = location_toc_file_new.Replace(".toc", ".sb", StringComparison.OrdinalIgnoreCase);
                                File.WriteAllBytes(location_sb_file_new, msNewFile.ToArray());

                                foreach(var kvp in newSBFileWithPositions)
                                {
                                    var bbi = kvp.Key.GetValue<BaseBundleInfo>("Bundle");
                                    var tocBundle = tocSb.TOCFile.Bundles.FirstOrDefault(x => x == bbi);
                                    if(tocBundle != null)
                                    {
                                        tocBundle.Offset = kvp.Value.Item1;
                                        tocBundle.Size = kvp.Value.Item2;
                                    }
                                }
                            }
                        }

                        // Read Out stuff from Original TOC

                        foreach (var b in tocSb.TOCFile.CasBundles)
                        {
                            DbObject dbo = new DbObject();
                            BinaryReader_FIFA21 binaryReader = new BinaryReader_FIFA21();
                            var resolvedPath = FileSystem.Instance.ResolvePath(FileSystem.Instance.GetCasFilePath(b.Catalog, b.Cas, b.Patch));
                            if (LastCasPath != resolvedPath)
                            {
                                if (nrCas != null)
                                {
                                    nrCas.Dispose();
                                    nrCas = null;
                                    GC.Collect();
                                    GC.WaitForPendingFinalizers();
                                }
                                LastCasPath = resolvedPath;
                                nrCas = new NativeReader(new FileStream(resolvedPath, FileMode.Open));
                            }
                            binaryReader.BinaryRead_FIFA21(b.BundleOffset, ref dbo, nrCas, false);
                            if (dbo.HasValue("ebx"))
                            {
                                if(dbo.GetValue<DbObject>("ebx").list.Any(x => ((DbObject)x).GetValue<string>("name").Contains("splash", StringComparison.OrdinalIgnoreCase)))
                                {

                                }
                                foreach (DbObject ebx in dbo.GetValue<DbObject>("ebx"))
                                {
                                    var ebxName = ebx.GetValue<string>("name");
                                    if (parent.modifiedEbx.ContainsKey(ebxName))
                                    {
                                        EbxAssetEntry ebxAssetEntry = ModExecuter.modifiedEbx[ebxName];
                                        if (writer_new_cas_file == null)
                                        {
                                            writer_new_cas_file?.Close();
                                            writer_new_cas_file = GetNextCas(ebx.GetValue<int>("catalog"), out casFileIndex);
                                        }
                                        ebx.SetValue("originalSize", ebxAssetEntry.OriginalSize);
                                        ebx.SetValue("size", ModExecuter.archiveData[ebxAssetEntry.Sha1].Data.Length);
                                        ebx.SetValue("cas", casFileIndex);
                                        ebx.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                        var ebxData = ModExecuter.archiveData[ebxAssetEntry.Sha1].Data;

                                        writer_new_cas_file.Write(ebxData);
                                    }
                                }
                                
                            }

                            if (dbo.HasValue("res"))
                            {
                                foreach (DbObject res in dbo.GetValue<DbObject>("res"))
                                {
                                    var resName = res.GetValue<string>("name");
                                    if (parent.modifiedRes.ContainsKey(resName))
                                    {
                                        EbxAssetEntry ebxAssetEntry = ModExecuter.modifiedEbx[resName];
                                        if (writer_new_cas_file == null)
                                        {
                                            writer_new_cas_file?.Close();
                                            writer_new_cas_file = GetNextCas(res.GetValue<int>("catalog"), out casFileIndex);
                                        }
                                        res.SetValue("originalSize", ebxAssetEntry.OriginalSize);
                                        res.SetValue("size", ModExecuter.archiveData[ebxAssetEntry.Sha1].Data.Length);
                                        res.SetValue("cas", casFileIndex);
                                        res.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                        var ebxData = ModExecuter.archiveData[ebxAssetEntry.Sha1].Data;

                                        writer_new_cas_file.Write(ebxData);
                                    }
                                }

                            }   

                            if (dbo.HasValue("chunks"))
                            {
                                foreach (DbObject chunk in dbo.GetValue<DbObject>("chunks"))
                                {
                                    var chunkId = chunk.GetValue<Guid>("id");
                                    if (parent.ModifiedChunks.ContainsKey(chunkId))
                                    {
                                        ChunkAssetEntry chunkAssetEntry = parent.ModifiedChunks[chunkId];  // ModExecuter.ModifiedChunks[chunkId];
                                                                                                           //if (writer_new_cas_file == null || writer_new_cas_file.BaseStream.Length + ModExecuter.archiveData[chunkAssetEntry.Sha1].Data.Length > 1073741824)
                                        if (writer_new_cas_file == null)
                                        {
                                            writer_new_cas_file?.Close();
                                            writer_new_cas_file = GetNextCas(chunk.GetValue<int>("catalog"), out casFileIndex);
                                        }
                                        chunk.SetValue("originalSize", chunkAssetEntry.OriginalSize);
                                        chunk.SetValue("size", ModExecuter.archiveData[chunkAssetEntry.Sha1].Data.Length);
                                        chunk.SetValue("cas", casFileIndex);
                                        chunk.SetValue("offset", (int)writer_new_cas_file.BaseStream.Position);
                                        chunk.SetValue("logicalOffset", chunkAssetEntry.LogicalOffset);
                                        chunk.SetValue("logicalSize", chunkAssetEntry.LogicalSize);
                                        writer_new_cas_file.Write(ModExecuter.archiveData[chunkAssetEntry.Sha1].Data);
                                    }
                                }
                            }
                        }

                        // Modifying toc chunks
                        foreach (var c in tocSb.TOCFile.TocChunks)
                        {
                            if (parent.ModifiedChunks.ContainsKey(c.Id))
                            {
                            }
                        }

                        // Write new TOC
                        MemoryStream memoryStream = new MemoryStream();
                        tocSb.TOCFile.Write(memoryStream);
                        byte[] newTocData = memoryStream.ToArray();
                        memoryStream.Dispose();
                        File.WriteAllBytes(location_toc_file_new, newTocData);
                    }

                    //if (File.Exists(tocSb.SbPath))
                    //{
                    //    File.Copy(tocSb.SbPath
                    //        , tocSb.SbPath
                    //        .Replace("Data", "ModData\\Data", StringComparison.OrdinalIgnoreCase)
                    //        .Replace("Patch", "ModData\\Patch", StringComparison.OrdinalIgnoreCase), true);
                    //}

                    
                }
            }

            if (nrCas != null)
            {
                nrCas.Dispose();
                nrCas = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            if (writer_new_cas_file == null)
            {
                writer_new_cas_file?.Close();
                writer_new_cas_file?.Dispose();
            }

            if (directory == "native_data")
            {
                logger.Log("RunBundleCompiler TimeTaken:: " + (DateTime.Now - startTime).ToString());

                return RunBundleCompiler(fs, logger, frostyModExecuter, "native_patch");
            }

            return true;


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


        DbObject layoutToc = null;

        private NativeWriter GetNextCas(int catalog, out int casFileIndex)
        {
            casFileIndex = 1;
            string text = FileSystem.Instance.ResolvePath(FileSystem.Instance.GetCasFilePath(catalog, casFileIndex, true));

            if (text.Contains("/native_data/"))
                text = text.Replace("/native_data/", "ModData\\", StringComparison.OrdinalIgnoreCase);
            if (text.Contains("/native_patch/"))
                text = text.Replace("/native_patch/", "ModData\\", StringComparison.OrdinalIgnoreCase);
            text = text.Replace("/", "\\");

            var parentDirectory = Directory.GetParent(text).FullName;
            var allFiles = Directory.GetFiles(parentDirectory, "*.cas");
            text = allFiles[allFiles.Length - 1];



            var lastDigit = int.Parse(text.Split("_")[text.Split("_").Length - 1].Replace(".cas", ""));
            //if(new FileInfo(text).Length > 1073741824)
            //    casFileIndex = lastDigit + 1;
            //else
                casFileIndex = lastDigit;

            var nw = new NativeWriter(new FileStream(text, FileMode.OpenOrCreate));
            nw.Position = nw.BaseStream.Length;
            return nw;
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


        public FrostyModExecutor parent;


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

        private bool UseModData;

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

        private void ProcessLegacyMods()
        {
            if (parent.modifiedLegacy.Count > 0)
            {
                parent.Logger.Log($"Legacy :: {parent.modifiedLegacy.Count} Legacy files found. Modifying associated chunks");

                Dictionary<string, byte[]> legacyData = new Dictionary<string, byte[]>();
                var countLegacyChunksModified = 0;
                foreach (var modLegacy in parent.modifiedLegacy)
                {
                    var originalEntry = AssetManager.Instance.GetCustomAssetEntry("legacy", modLegacy.Key);
                    var data = parent.archiveData[modLegacy.Value.Sha1].Data;
                    legacyData.Add(modLegacy.Key, data);

                }

                AssetManager.Instance.ModifyLegacyAssets(legacyData, true);

                var modifiedLegacyChunks = AssetManager.Instance.EnumerateChunks(true);
                foreach (var modLegChunk in modifiedLegacyChunks)
                {
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


    }





}
