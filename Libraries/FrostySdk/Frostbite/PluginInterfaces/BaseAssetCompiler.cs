using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FrostySdk.Frostbite.PluginInterfaces.BaseAssetCompiler;

namespace FrostySdk.Frostbite.PluginInterfaces
{
    public abstract class BaseAssetCompiler
    {
        public string ModDirectory { get; } = "ModData";
        public string PatchDirectory { get; } = "Patch";

        public Dictionary<ModType, int> ErrorCounts = new Dictionary<ModType, int>();

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


        protected static void CopyDataFolder(string from_datafolderpath, string to_datafolderpath, ILogger logger)
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

                var isPatch = fIOrig.FullName.Contains("Patch", StringComparison.OrdinalIgnoreCase);

                if (fIDest.Exists && finalDestinationPath.Contains("moddata", StringComparison.OrdinalIgnoreCase))
                {
                    var isCas = fIDest.Extension.Contains("cas", StringComparison.OrdinalIgnoreCase);
                    var twogbinsize = 2L * (1048576L * 1024L);
                    if (
                        isCas
                        && fIDest.Length != fIOrig.Length
                        && (isPatch || (fIDest.Length >= twogbinsize))
                        )
                    {
                        fIDest.Delete();
                    }
                    else if
                        (
                            !isCas
                        //&&
                        //(
                        //    fIDest.Length != fIOrig.Length
                        //    ||
                        //        (
                        //            //fIDest.LastWriteTime.Day != fIOrig.LastWriteTime.Day
                        //            //&& fIDest.LastWriteTime.Hour != fIOrig.LastWriteTime.Hour
                        //            //&& fIDest.LastWriteTime.Minute != fIOrig.LastWriteTime.Minute
                        //            !File.ReadAllBytes(finalDestinationPath).SequenceEqual(File.ReadAllBytes(originalFilePath))
                        //        )
                        //)
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

        protected static void CopyFile(string inputFilePath, string outputFilePath)
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
