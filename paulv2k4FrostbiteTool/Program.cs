using CommandLine;
using FrostyEditor.Controls;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;

namespace paulv2k4FrostbiteTool
{
    class Program
    {
        public class Options
        {
            [Option('i', "import", Required = false, HelpText = "Import")]
            public bool? Import { get; set; }

            [Option('e', "export", Required = false, HelpText = "Export")]
            public bool? Export { get; set; }

            [Option('m', "importFiles", Required = false, HelpText = "Imported files locations, use an = sign to determine the EBX/RES/Chunk they import into", Separator = ',')]
            public IEnumerable<string> ImportFiles { get; set; }

            [Option('x', "exportFiles", Required = false, HelpText = "Export files search wildcard", Separator = ',')]
            public IEnumerable<string> ExportFiles { get; set; }

            //[Option(longName: "ExtractGameplayOnly")]
            //public bool ExtractGameplayOnly { get; set; }

            [Option('p', "GamePath", Required = true, HelpText = "The path to your game EXE (e.g. C:/FIFA20/FIFA20.exe)")]
            public string GamePath { get; set; }

            [Option(longName: "listEBX", Required = false, HelpText = "Exports a list of EBX in the Game")]
            public bool listEBX { get; set; }

            [Option(longName: "listRES", Required = false, HelpText = "Exports a list of RES in the Game")]
            public bool listRES { get; set; }

        }

        static void Main(string[] args)
        {

            Console.WriteLine("Welcome to paulv2k4's Frostbite Import and Export Tool");
            Console.WriteLine("This tool is in early alpha, please aware you might break your game using this tool.");

            Console.WriteLine("");
            if (args.Length == 0)
            {
                Console.WriteLine("Please supply some arguments for the tool to work!");
                return;
            }



            Parser.Default.ParseArguments<Options>(args)
                .WithNotParsed(err => {

                    foreach (var e in err) 
                    {
                        Console.WriteLine(e);
                        Console.WriteLine(e.Tag);
                    }
                    Console.ReadLine();
                
                })
                .WithParsed(RunOptions);

        }




        private static void InitializeOfSelectedGame(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var FIFADirectory = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                GameInstanceSingleton.GAMERootPath = FIFADirectory;
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                GameInstanceSingleton.GAMEVERSION = fileName.Replace(".exe", "");
                if (!ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
                {
                    throw new Exception("Unable to Initialize Profile");
                }
            }
        }

        private static ProjectManagement projectManagement;

        private static void RunOptions(Options obj)
        {
            if (!Directory.Exists("Export"))
                Directory.CreateDirectory("Export");

            if (!Directory.Exists("Export/EBX"))
                Directory.CreateDirectory("Export/EBX");

            if (!Directory.Exists("Export/EBX/Readable"))
                Directory.CreateDirectory("Export/EBX/Readable");

            if (!Directory.Exists("Export/RES"))
                Directory.CreateDirectory("Export/RES");

            if (!Directory.Exists("Export/Chunk"))
                Directory.CreateDirectory("Export/Chunk");

            if (!Directory.Exists("Export/Legacy"))
                Directory.CreateDirectory("Export/Legacy");

            if (!Directory.Exists("Export/Texture"))
                Directory.CreateDirectory("Export/Texture");

            if (!Directory.Exists("Import"))
                Directory.CreateDirectory("Import");

            if(!obj.Export.HasValue && !obj.Import.HasValue)
            {
                Console.WriteLine("Please use --Import or --Export for the tool to know what to do!");
                Console.ReadLine();
                return;
            }

            InitializeOfSelectedGame(obj.GamePath);
            projectManagement = new ProjectManagement();
            var project = projectManagement.StartNewProject();

            //if (obj.ExtractGameplayOnly)
            //{

            //}

            if (obj.Export.HasValue)
            {
                if (obj.ExportFiles.Count() == 0)
                {
                    Console.WriteLine("Please use --exportFiles for the tool to know what files to search for");
                    Console.ReadLine();
                    return;
                }
                else
                {
                    Export(obj);

                    return;
                }
            }

            if (obj.Import.HasValue)
            {

            }


        }

        private static void Export(Options options)
        {
            var allEBX = projectManagement.FrostyProject.AssetManager.EnumerateEbx().ToList();
            var searchEBX = allEBX.Where(x =>
            {
                return SearchMatchExportFilesToAssetEntry(options, x);
            }
            ).ToList();
            if (searchEBX != null && searchEBX.Count() > 0)
            {
                searchEBX.ForEach(r =>
                {
                    ExportRawData(r, "EBX");
                    var ebx = projectManagement.FrostyProject.AssetManager.GetEbx(r);
                    var json = JsonConvert.SerializeObject(ebx.RootObject);
                    File.WriteAllText("Export\\EBX\\Readable\\" + r.Filename + ".json", json);

                });
            }

            var allRES = projectManagement.FrostyProject.AssetManager.EnumerateRes().ToList();
            var searchRES = allRES.Where(x =>
            {
                return SearchMatchExportFilesToAssetEntry(options, x);
            }
            ).ToList();
            if (searchRES != null && searchRES.Count() > 0)
            {
               searchRES.ForEach(r =>
               {
                   ExportRawData(r, "RES");
                   ExportTexture(r);

               });
            }

            //var allChunks = projectManagement.FrostyProject.AssetManager.EnumerateChunks().ToList();
            //var searchChunks = allChunks.Where(x =>
            //{
            //    return SearchMatchExportFilesToAssetEntry(options, x);
            //}
            //).ToList();
            //if (searchChunks != null && searchChunks.Count() > 0)
            //{
            //    searchChunks.ForEach(r => ExportRawData(r, "Chunk"));
            //}

            //var allLegacy = projectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy").ToList();
            //var searchLegacy = allLegacy.Where(x =>
            //{
            //    return SearchMatchExportFilesToAssetEntry(options, x);
            //}
            //).ToList();
            //if (allLegacy != null && allLegacy.Count() > 0)
            //{
            //    allLegacy.ForEach(r => ExportRawData(r, "Legacy"));
            //}

            Console.WriteLine("Export Complete");
            Console.ReadLine();
        }

        private static void ExportTexture(ResAssetEntry r)
        {
            if (r.Type == "Texture")
            {
                var resStream = projectManagement.FrostyProject.AssetManager.GetRes(r);
                Texture textureAsset = new Texture(resStream, projectManagement.FrostyProject.AssetManager);
                new TextureExporter().Export(textureAsset, $"Export\\Texture\\{r.Filename}.DDS", "*.dds");
            }
        }

        private static void ExportRawData(AssetEntry r, string type)
        {
            if (r.ExtraData != null && !string.IsNullOrEmpty(r.ExtraData.CasPath))
            {
                var path = projectManagement.FrostyProject.FileSystem.ResolvePath(r.ExtraData.CasPath);
                using (var fileStream = TryOpen(path, FileMode.Open, maximumAttempts: 10, attemptWaitMS: 50))
                {
                    using (NativeReader reader = new NativeReader(fileStream))
                    {
                        reader.BaseStream.Seek(r.ExtraData.DataOffset, SeekOrigin.Begin);

                        using (NativeReader viewStreamReader = new NativeReader(reader.CreateViewStream(r.ExtraData.DataOffset, r.Size)))
                        {
                            using (NativeWriter writer = new NativeWriter(new FileStream($"Export\\{type}\\{r.Filename}", FileMode.OpenOrCreate)))
                            {
                                writer.Write(viewStreamReader.ReadToEnd());
                            }
                            viewStreamReader.BaseStream.Close();
                        }
                    }
                }
            }
        }

        private static bool SearchMatchExportFilesToAssetEntry(Options options, AssetEntry x)
        {
            foreach (var i in options.ExportFiles)
            {
                return x.Path.ToLower().Contains(i.ToLower());// x.Filename.Contains(i);
            }

            return false;
        }


        /// <summary>
        /// Tries to open a file, with a user defined number of attempt and Sleep delay between attempts.
        /// </summary>
        /// <param name="filePath">The full file path to be opened</param>
        /// <param name="fileMode">Required file mode enum value(see MSDN documentation)</param>
        /// <param name="fileAccess">Required file access enum value(see MSDN documentation)</param>
        /// <param name="fileShare">Required file share enum value(see MSDN documentation)</param>
        /// <param name="maximumAttempts">The total number of attempts to make (multiply by attemptWaitMS for the maximum time the function with Try opening the file)</param>
        /// <param name="attemptWaitMS">The delay in Milliseconds between each attempt.</param>
        /// <returns>A valid FileStream object for the opened file, or null if the File could not be opened after the required attempts</returns>
        private static FileStream TryOpen(string filePath, FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.Read, FileShare fileShare = FileShare.Read, int maximumAttempts = 1, int attemptWaitMS = 10)
        {
            FileStream fs = null;
            int attempts = 0;

            // Loop allow multiple attempts
            while (true)
            {
                try
                {
                    fs = File.Open(filePath, fileMode, fileAccess, fileShare);

                    //If we get here, the File.Open succeeded, so break out of the loop and return the FileStream
                    break;
                }
                catch (IOException ioEx)
                {
                    // IOExcception is thrown if the file is in use by another process.

                    // Check the numbere of attempts to ensure no infinite loop
                    attempts++;
                    if (attempts > maximumAttempts)
                    {
                        // Too many attempts,cannot Open File, break and return null 
                        fs = null;
                        break;
                    }
                    else
                    {
                        // Sleep before making another attempt
                        Thread.Sleep(attemptWaitMS);

                    }

                }

            }
            // Reutn the filestream, may be valid or null
            return fs;
        }
    }
}
