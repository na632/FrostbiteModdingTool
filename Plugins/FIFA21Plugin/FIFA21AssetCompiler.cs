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
