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


            // Handle Legacy first to generate modified chunks
            ProcessLegacyMods();
            // ------ End of handling Legacy files ---------

            return RunBundleCompiler(fs, logger, frostyModExecuter);
        }

        private bool RunBundleCompiler(FileSystem fs, ILogger logger, object frostyModExecuter, string directory = "native_data")
        {
            if (!UseModData)
                throw new Exception("Must use ModData for this method");

            DateTime startTime = DateTime.Now;
            var ModExecuter = (FrostyModExecutor)frostyModExecuter;
            foreach (Catalog catalogInfo in FileSystem.Instance.EnumerateCatalogInfos())
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
                    string location_toc_file = ModExecuter.fs.ResolvePath(tocFileRAW).ToLower();
                    TocSbReader_FIFA21 tocSb = new TocSbReader_FIFA21();
                    tocSb.DoLogging = false;
                    tocSb.ProcessData = false;
                    tocSb.Read(location_toc_file, 0, new BinarySbDataHelper(AssetManager.Instance), tocFileRAW);

                    var location_toc_file_new = UseModData 
                        ? location_toc_file
                        .Replace("Data", "ModData/Data", StringComparison.OrdinalIgnoreCase)
                        .Replace("Patch", "ModData/Patch", StringComparison.OrdinalIgnoreCase)
                        : location_toc_file;

                    if (tocSb.TOCFile != null)
                    {
                        foreach (var b in tocSb.TOCFile.Bundles)
                        {
                            DbObject dbo = new DbObject();
                            //BinaryReader_FIFA21 binaryReader = new BinaryReader_FIFA21();
                            //using(var nr = new NativeReader(tocSb.SBFile))
                            //binaryReader.BinaryRead_FIFA21(b.Offset, ref dbo, )
                        }

                        // Read Out stuff from Original TOC

                        NativeReader nrCas = null;
                        string LastCasPath = null;
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
                        }


                        // Write new TOC
                        MemoryStream memoryStream = new MemoryStream();
                        tocSb.TOCFile.Write(memoryStream);
                        byte[] newTocData = memoryStream.ToArray();
                        memoryStream.Dispose();

                        using (NativeWriter writer = new NativeWriter(new FileStream(location_toc_file_new, FileMode.Create)))
                        {
                            writer.Write(newTocData);
                        }

                        if (nrCas != null)
                        {
                            nrCas.Dispose();
                            nrCas = null;
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                }
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
                        parent.archiveData.Add(chunk.Sha1, new ArchiveInfo() { Data = chunk.ModifiedEntry.Data });
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

    }



}
