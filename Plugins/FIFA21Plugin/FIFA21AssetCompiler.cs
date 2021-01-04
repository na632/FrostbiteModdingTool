using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using paulv2k4ModdingExecuter;
using System;
using System.Collections.Generic;
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

        private static void DirectoryCopy(string sourceBasePath, string destinationBasePath, bool recursive = true)
        {
            if (!Directory.Exists(sourceBasePath))
                throw new DirectoryNotFoundException($"Directory '{sourceBasePath}' not found");

            var directoriesToProcess = new Queue<(string sourcePath, string destinationPath)>();
            directoriesToProcess.Enqueue((sourcePath: sourceBasePath, destinationPath: destinationBasePath));
            while (directoriesToProcess.Any())
            {
                (string sourcePath, string destinationPath) = directoriesToProcess.Dequeue();

                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);

                var sourceDirectoryInfo = new DirectoryInfo(sourcePath);
                    foreach (FileInfo sourceFileInfo in sourceDirectoryInfo.EnumerateFiles())
                        sourceFileInfo.CopyTo(Path.Combine(destinationPath, sourceFileInfo.Name), true);
                if (!recursive)
                    continue;

                foreach (DirectoryInfo sourceSubDirectoryInfo in sourceDirectoryInfo.EnumerateDirectories())
                    directoriesToProcess.Enqueue((
                        sourcePath: sourceSubDirectoryInfo.FullName,
                        destinationPath: Path.Combine(destinationPath, sourceSubDirectoryInfo.Name)));
            }
        }

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

            // ------------------------------------------------------------------------------------------
            // You will need to change this to ProfilesLibrary.DataVersion if you change the Profile.json DataVersion field
            //if (ProfilesLibrary.IsFIFA21DataVersion())
            //{
                DbObject layoutToc = null;

                // Read the original Layout TOC into a DB Object
                //using (DbReader dbReaderOfLayoutTOC = new DbReader(new FileStream(fs.BasePath + PatchDirectory + "/layout.toc", FileMode.Open, FileAccess.Read), fs.CreateDeobfuscator()))
                //{
                //    layoutToc = dbReaderOfLayoutTOC.ReadDbObject();
                //}

                // Notify the Bundle Action of the Cas File Count
                FIFA21BundleAction.CasFileCount = fs.CasFileCount;
                List<FIFA21BundleAction> madden21BundleActions = new List<FIFA21BundleAction>();

                var numberOfCatalogs = fs.Catalogs.Count();
                var numberOfCatalogsCompleted = 0;

            //if (!((FrostyModExecutor)frostyModExecuter).UseSymbolicLinks)
            //{
            //logger.Log("No Symbolic Link - Copying files from Data to ModData");
            //CopyDataFolder(fs.BasePath + "\\Data\\", fs.BasePath + ModDirectory + "\\Data\\", logger);

            //Task.WaitAll(tasks);
            //}
            Directory.CreateDirectory(fs.BasePath + ModDirectory + "\\Data");
            Directory.CreateDirectory(fs.BasePath + ModDirectory + "\\Patch");

            //if (Directory.Exists(fs.BasePath + ModDirectory + "\\Data"))
            //    {
            //        logger.Log("Deleting TOC/SB files from ModData/Data");
            //        foreach (string sbFileLocation in Directory.EnumerateFiles(fs.BasePath + ModDirectory + "\\Data\\", "*.sb", SearchOption.AllDirectories))
            //        {
            //            File.Delete(sbFileLocation);
            //        }
            //        foreach (string tocFileLocation in Directory.EnumerateFiles(fs.BasePath + ModDirectory + "\\Data\\", "*.toc", SearchOption.AllDirectories))
            //        {
            //            File.Delete(tocFileLocation);
            //        }

            //    }

            //    if (Directory.Exists(fs.BasePath + ModDirectory + "\\" + PatchDirectory))
            //    {
            //        logger.Log("Deleting CAS files from ModData/Patch");
            //        //foreach (string casFileLocation in Directory.EnumerateFiles(fs.BasePath + ModDirectory + "\\" + PatchDirectory, "*.cas", SearchOption.AllDirectories))
            //        //{
            //        //    File.Delete(casFileLocation);
            //        //}
            //        foreach (string sbFileLocation in Directory.EnumerateFiles(fs.BasePath + ModDirectory + "\\" + PatchDirectory, "*.sb", SearchOption.AllDirectories))
            //        {
            //            File.Delete(sbFileLocation);
            //        }
            //        foreach (string tocFileLocation in Directory.EnumerateFiles(fs.BasePath + ModDirectory + "\\" + PatchDirectory, "*.toc", SearchOption.AllDirectories))
            //        {
            //            File.Delete(tocFileLocation);
            //        }

            //    }
                logger.Log("Copying files from Patch to ModData/Patch");
                // Copied Patch CAS files from Patch to Mod Data Patch
                //DirectoryCopy(fs.BasePath + PatchDirectory, fs.BasePath + ModDirectory + "//" + PatchDirectory, true);
                CopyDataFolder(fs.BasePath + "\\Data\\", fs.BasePath + ModDirectory + "\\Data\\", logger);
                CopyDataFolder(fs.BasePath + PatchDirectory, fs.BasePath + ModDirectory + "\\" + PatchDirectory, logger);

            //foreach (CatalogInfo catalogItem in fs.EnumerateCatalogInfos())
            //{
            var fme = (FrostyModExecutor)frostyModExecuter;

            FIFA21BundleAction fifaBundleAction = new FIFA21BundleAction(fme);
            return fifaBundleAction.Run();

            //Plugin2.Execution.Plugin2Executer plugin2Executer = new Plugin2.Execution.Plugin2Executer(AssetManager.Instance.fs, KeyManager.Instance, "ModData");
            //plugin2Executer.Logger = logger;
            //plugin2Executer.modifiedEbx = fme.modifiedEbx;
            //plugin2Executer.modifiedRes = fme.modifiedRes;
            //plugin2Executer.modifiedChunks = fme.modifiedChunks;
            //plugin2Executer.archiveData = fme.archiveData;
            //return plugin2Executer.BuildFIFA21Mods("Patch", "ModData").Result;

                //FIFA21ContentPatchBuilder contentPatchBuilder = new FIFA21ContentPatchBuilder((FrostyModExecutor)frostyModExecuter);
                //contentPatchBuilder.TransferDataToPatch();

                //numberOfCatalogsCompleted++;
                //logger.Log($"Compiling Mod Progress: { Math.Round((double)numberOfCatalogsCompleted / numberOfCatalogs, 2) * 100} %");
                //}
                // --------------------------------------------------------------------------------------


                // --------------------------------------------------------------------------------------
                // From the new bundles that have been created that has generated new CAS files, add these new CAS files to the Layout TOC
                //foreach (FIFA21BundleAction bundleAction in madden21BundleActions)
                //{
                //    if (bundleAction.HasErrored)
                //    {
                //        throw bundleAction.Exception;
                //    }
                //    if (bundleAction.CasFiles.Count > 0)
                //    {
                //        var installManifest = layoutToc.GetValue<DbObject>("installManifest");
                //        var installChunks = installManifest.GetValue<DbObject>("installChunks");
                //        foreach (DbObject installChunk in installChunks)
                //        {
                //            if (bundleAction.CatalogInfo.Name.Equals("win32/" + installChunk.GetValue<string>("name")))
                //            {
                //                foreach (int key in bundleAction.CasFiles.Keys)
                //                {
                //                    DbObject newFile = DbObject.CreateObject();
                //                    newFile.SetValue("id", key);
                //                    newFile.SetValue("path", bundleAction.CasFiles[key]);

                //                    var installChunkFiles = installChunk.GetValue<DbObject>("files");
                //                    installChunkFiles.Add(newFile);


                //                }
                //                break;
                //            }
                //        }
                //    }
                //}


                // --------------------------------------------------------------------------------------
                // Write a new Layout file
                //logger.Log("Writing new Layout file to Game");
                //using (DbWriter dbWriter = new DbWriter(new FileStream(ModDirectory + PatchDirectory + "/layout.toc", FileMode.Create), inWriteHeader: true))
                //{
                //    dbWriter.Write(layoutToc);
                //}
                // --------------------------------------------------------------------------------------

            //}
            //return false;
        }

        private static void CopyDataFolder(string from_datafolderpath, string to_datafolderpath, ILogger logger)
        {
            Directory.CreateDirectory(to_datafolderpath);

            var dataFiles = Directory.EnumerateFiles(from_datafolderpath, "*.*", SearchOption.AllDirectories);
            var dataFileCount = dataFiles.Count();
            var indexOfDataFile = 0;
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            Parallel.ForEach(dataFiles, (f) =>
            {
                var finalDestination = f.ToLower().Replace(from_datafolderpath.ToLower(), to_datafolderpath.ToLower());

                bool Copied = false;

                var lastIndexOf = finalDestination.LastIndexOf("\\");
                var newDirectory = finalDestination.Substring(0, lastIndexOf) + "\\";
                if (!Directory.Exists(newDirectory))
                {
                    Directory.CreateDirectory(newDirectory);
                }
                if (File.Exists(finalDestination) && finalDestination.Contains("moddata"))
                {
                    if (
                        File.GetLastWriteTime(finalDestination).Ticks != File.GetLastWriteTime(f).Ticks
                        || new FileInfo(finalDestination).Length != new FileInfo(f).Length
                        )
                    {
                        File.Delete(finalDestination);
                    }
                }
                if (!File.Exists(finalDestination))
                {
                    //using (var inputStream = new NativeReader(File.Open(f, FileMode.Open)))
                    //using (var outputStream = new NativeWriter(File.Open(finalDestination, FileMode.Create)))
                    //{
                    //    outputStream.Write(inputStream.ReadToEnd());
                    //}

                    File.Copy(f, finalDestination);
                    Copied = true;
                }
                indexOfDataFile++;

                if (Copied)
                    logger.Log($"Data Setup - Copied ({indexOfDataFile}/{dataFileCount}) - {f}");
            });
        }
    }
}
