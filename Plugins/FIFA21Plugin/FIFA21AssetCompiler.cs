using FrostySdk;
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

namespace FIFA21Plugin
{

    /// <summary>
    /// Currently. The Madden 21 Compiler does not work in game.
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

//#if DEBUG
//            //RunBundleCompiler(fs, logger, frostyModExecuter);
//#endif

            if(!FrostyModExecutor.UseModData)
            {
                return RunEADesktopCompiler(fs, logger, frostyModExecuter);
            }
            return RunOriginCompiler(fs, logger, frostyModExecuter);
        }

        private void RunBundleCompiler(FileSystem fs, ILogger logger, object frostyModExecuter, string directory = "native_data")
        {
            DateTime startTime = DateTime.Now;
            var ModExecuter = (FrostyModExecutor)frostyModExecuter;
            foreach (Catalog catalogInfo in FileSystem.Instance.EnumerateCatalogInfos())
            {
                NativeWriter writer_new_cas_file = null;
                int casFileIndex = 0;
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
                    if (tocSb.TOCFile != null)
                    {
                        foreach (var b in tocSb.TOCFile.Bundles)
                        {
                            DbObject dbo = new DbObject();
                            //BinaryReader_FIFA21 binaryReader = new BinaryReader_FIFA21();
                            //using(var nr = new NativeReader(tocSb.SBFile))
                            //binaryReader.BinaryRead_FIFA21(b.Offset, ref dbo, )
                        }

                        NativeReader nrCas = null;
                        string LastCasPath = null;
                        foreach (var b in tocSb.TOCFile.CasBundles)
                        {
                            DbObject dbo = new DbObject();
                            BinaryReader_FIFA21 binaryReader = new BinaryReader_FIFA21();
                            var resolvedPath = FileSystem.Instance.ResolvePath(FileSystem.Instance.GetCasFilePath(b.Catalog, b.Cas, b.Patch));
                            if(LastCasPath != resolvedPath)
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
                        if(nrCas != null)
                        {
                            nrCas.Dispose();
                            nrCas = null;
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                    }
                }
            }

            if(directory == "native_data")
            {
                RunBundleCompiler(fs, logger, frostyModExecuter, "native_patch");

                logger.Log("RunBundleCompiler TimeTaken:: " + (DateTime.Now - startTime).ToString());
            }

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
}
