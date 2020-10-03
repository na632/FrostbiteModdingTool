using CommandLine;
using Frostbite.Textures;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Newtonsoft.Json;
using paulv2k4ModdingExecuter;
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
    internal class Program : ILogger
    {
        public class Options
        {
            [Option('i', "import", Required = false, HelpText = "Import (DOESNT WORK)")]
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

            [Option(longName: "listLegacy", Required = false, HelpText = "Exports a list of Legacy files in the Game")]
            public bool listLegacy { get; set; }

            [Option(longName: "project", Required = false, HelpText = "Path to the project file you'd like to use. If this doesn't exist, it will start a new project.")]
            public string project { get; set; }

            [Option(longName: "startProject", Required = false, HelpText = "Start a new Project - Allows lots of search and export functions.")]
            public bool? startProject { get; set; }

        }

        static void Main(string[] args)
        {

            Console.WriteLine("Welcome to paulv2k4's Frostbite Import and Export Tool");
            Console.WriteLine("This tool is in early alpha, please be aware you might break your game using this tool.");

            Console.WriteLine("");
            if (args.Length == 0)
            {
                Console.WriteLine("Please supply some arguments for the tool to work!");
                Console.ReadLine();
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

            //if(!obj.Export.HasValue && !obj.Import.HasValue)
            //{
            //    Console.WriteLine("Please use --Import or --Export for the tool to know what to do!");
            //    Console.ReadLine();
            //    return;
            //}

            if(string.IsNullOrEmpty(obj.GamePath) || !File.Exists(obj.GamePath))
            {
                Console.WriteLine("Invalid or no Game Path provided");
                Console.ReadLine();
                return;
            }

            InitializeOfSelectedGame(obj.GamePath);
            projectManagement = new ProjectManagement(obj.GamePath);
            var project = projectManagement.StartNewProject();

            //if (obj.ExtractGameplayOnly)
            //{

            //}

            if(obj.listEBX)
            {
                var allEBX = projectManagement.FrostyProject.AssetManager.EnumerateEbx().ToList().Select(x => new { x.Path, x.Name, x.Type, CasPath = x.ExtraData != null ? x.ExtraData.CasPath : "" });
                File.WriteAllText("Export/EBX/__ListOfEbx.json", JsonConvert.SerializeObject(allEBX));
            }

            if (obj.listLegacy)
            {
                var allLegacy = projectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy").ToList().Select(x => new { x.Path, x.Name, x.Type });
                File.WriteAllText("Export/Legacy/__ListOfLegacy.json", JsonConvert.SerializeObject(allLegacy));
            }

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
                    Export(obj.ExportFiles, "all");

                    return;
                }
            }

            if (obj.Import.HasValue)
            {
                Import(obj);
            }

            if (!string.IsNullOrEmpty(obj.project) || obj.startProject.HasValue)
            {
                StartProjectManagementSteps(obj);
            }

        }

        private static FrostyProject FrostyProject;
        private static string FrostyProjectFileLocation;

        private static void StartProjectManagementSteps(Options options)
        {
            ProjectManagement projectManagement = new ProjectManagement(options.GamePath);
            if (!string.IsNullOrEmpty(options.project) && File.Exists(options.project))
            {
                FrostyProject = projectManagement.StartNewProject();
                FrostyProject.Load(options.project);
                Console.WriteLine("Project loaded");
            }
            else
            {
                FrostyProject = projectManagement.StartNewProject();
                if (string.IsNullOrEmpty(options.project))
                {
                    provide_filename_step:
                    Console.WriteLine("Please provide a filename to save your project.");
                    options.project = Console.ReadLine();
                    if (string.IsNullOrEmpty(options.project))
                    {
                        goto provide_filename_step;
                    }
                }
                FrostyProject.Save(options.project);
                Console.WriteLine("Project created and saved");
            }

            FrostyProjectFileLocation = options.project;

            new Program().ImportExportSearchStep();
            

        }

        private void ImportExportSearchStep()
        {
            import_export_step:
            Console.WriteLine("");
            Console.WriteLine("What would you like to do? Import, Export, Search or Launch?");
            switch (Console.ReadLine().ToLower())
            {
                case "import":
                    ImportStep();
                    break;
                case "export":
                    ExportStep();
                    break;
                case "search":
                    SearchStep();
                    break;
                case "launch":
                    Launch();
                    break;
                default:
                    goto import_export_step;
            }


            goto import_export_step;
        }

        private void Launch()
        {
            FrostyProject.Save(FrostyProjectFileLocation);
            FrostyProject.WriteToMod("projectTest.fbmod", new ModSettings() { Title = "Test" });

            if (!ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
            {
                throw new Exception("Unable to Initialize Profile");
            }
            FileSystem fileSystem = new FileSystem(GameInstanceSingleton.GAMERootPath);
            foreach (FileSystemSource source in ProfilesLibrary.Sources)
            {
                fileSystem.AddSource(source.Path, source.SubDirs);
            }
            fileSystem.Initialize();
            var fme = new FrostyModExecutor();
            var result = fme.Run(fileSystem, this, "", "", new System.Collections.Generic.List<string>() { @"projectTest.fbmod" }.ToArray()).Result;
        }

        private void ImportStep()
        {
            import_step_1:
            Console.WriteLine("Please provide the path to the file to import from your PC.");
            var pathToFile_to_import = Console.ReadLine();
            if(!File.Exists(pathToFile_to_import))
            {
                Console.WriteLine("Unable to find file.");
                goto import_step_1;
            }

            Console.WriteLine("Please provide the path to the file to import to in Frostbite.");
            var pathToFileToImportInFrostbite = Console.ReadLine();
            Console.WriteLine("Is this file a texture? (ONLY TEXTURES ARE SUPPORTED!)");
            switch (Console.ReadLine())
            {
                case "yes":
                case "y":
                    ImportTexture(pathToFile_to_import, pathToFileToImportInFrostbite);
                    break;
                case "no":
                case "n":
                    goto import_step_1;
            }


            projectManagement.FrostyProject.Save(FrostyProjectFileLocation);
            ImportExportSearchStep();
        }

        private void ExportStep()
        {
            Console.WriteLine("This is currently unavailable!");
            ImportExportSearchStep();
        }

        private void SearchStep()
        {
            string searchPattern = "";
            SearchStep:
            Console.WriteLine("Which section would you like to search? EBX, RES or Legacy? Or back?");
            switch (Console.ReadLine().ToLower())
            {
                case "ebx":
                    Console.WriteLine("Please enter your search pattern.");
                    searchPattern = Console.ReadLine();
                    goto ebx;
                case "res":
                    Console.WriteLine("Please enter your search pattern.");
                    searchPattern = Console.ReadLine();
                    goto res;
                case "legacy":
                    Console.WriteLine("Please enter your search pattern.");
                    searchPattern = Console.ReadLine();
                    goto legacy;
                case "back":
                    ImportExportSearchStep();
                    break;
                default:
                    goto SearchStep;
            }

            ebx:
            var allEBX = projectManagement.FrostyProject.AssetManager.EnumerateEbx().ToList();
            var searchEBX = allEBX.Where(x =>
            {
                return SearchMatchExportFilesToAssetEntry(new List<string>() { searchPattern }, x);
            }
            ).ToList();
            foreach (var i in searchEBX)
            {
                Console.WriteLine(i.Name);
            }
            Console.WriteLine("Would you like to Export this list?");
            switch (Console.ReadLine().ToLower())
            {
                case "yes":
                case "y":
                    Export(new List<string>() { searchPattern }, "ebx");
                    break;
                case "no":
                case "n":
                    goto SearchStep;
            }

            res:
            var allRES = projectManagement.FrostyProject.AssetManager.EnumerateRes().ToList();
            var searchRES = allRES.Where(x =>
            {
                return SearchMatchExportFilesToAssetEntry(new List<string>() { searchPattern }, x);
            }
            ).ToList();
            foreach (var i in searchRES)
            {
                Console.WriteLine(i.Name);
            }
            Console.WriteLine("Would you like to Export this list?");
            switch (Console.ReadLine().ToLower())
            {
                case "yes":
                case "y":
                    Export(new List<string>() { searchPattern }, "res");
                    break;
                case "no":
                case "n":
                    goto SearchStep;
            }

        legacy:
            var allLegacy = projectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy").ToList();
            var searchLegacy = allLegacy.Where(x =>
            {
                return SearchMatchExportFilesToAssetEntry(new List<string>() { searchPattern }, x);
            }
            ).ToList();
            foreach(var i in searchLegacy)
            {
                Console.WriteLine(i.Name);
            }
            Console.WriteLine("Would you like to Export this list?");
            switch (Console.ReadLine().ToLower())
            {
                case "yes":
                case "y":
                    Export(new List<string>() { searchPattern }, "legacy");
                    break;
                case "no":
                case "n":
                    goto SearchStep;
            }

            goto SearchStep;

        }

        private static void Export(IEnumerable<string> pattern, string type)
        {
            if (type == "all" || type == "ebx")
            {
                var allEBX = projectManagement.FrostyProject.AssetManager.EnumerateEbx().ToList();
                var searchEBX = allEBX.Where(x =>
                {
                    return SearchMatchExportFilesToAssetEntry(pattern, x);
                }
                ).ToList();
                if (searchEBX != null && searchEBX.Count() > 0)
                {
                    searchEBX.ForEach(r =>
                    {
                        ExportRawData(r, "EBX");
                        var ebx = projectManagement.FrostyProject.AssetManager.GetEbx(r);
                        var json = JsonConvert.SerializeObject(ebx.RootObject, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        File.WriteAllText("Export\\EBX\\Readable\\" + r.Filename + ".json", json);

                    });
                }
            }

            if (type == "all" || type == "res")
            {

                var allRES = projectManagement.FrostyProject.AssetManager.EnumerateRes().ToList();
                var searchRES = allRES.Where(x =>
                {
                    return SearchMatchExportFilesToAssetEntry(pattern, x);
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
            }

            if (type == "all" || type == "legacy")
            {
                var allLegacy = projectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy").ToList();
                var searchLegacy = allLegacy.Where(x =>
                {
                    return SearchMatchExportFilesToAssetEntry(pattern, x);
                }
                ).ToList();
                if (searchLegacy != null && searchLegacy.Count() > 0)
                {
                    searchLegacy.ForEach(r =>
                    {
                        ExportRawData(r, "Legacy");
                    });
                }
            }
            
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
            else
            {
                using (NativeWriter nativeWriter = new NativeWriter(new FileStream($"Export\\{type}\\{r.Filename}.{r.Type}", FileMode.Create)))
                {
                    nativeWriter.Write(new NativeReader(projectManagement.FrostyProject.AssetManager.GetCustomAsset("legacy", r)).ReadToEnd());
                }
            }

        }

        private static bool SearchMatchExportFilesToAssetEntry(IEnumerable<string> patterns, AssetEntry x)
        {
            foreach (var i in patterns)
            {
                return x.Path.ToLower().Contains(i.ToLower());
            }

            return false;
        }




        private static void Import(Options options)
        {
            //using (FileStream fileStream = new FileStream(@"G:\splashscreen_v2k4.DDS", FileMode.Open))
            //{
            var allEBX = projectManagement.FrostyProject.AssetManager.EnumerateEbx().Where(x=>x.Filename.ToLower() == "splashscreen").ToList();
            var allRes = projectManagement.FrostyProject.AssetManager.EnumerateRes().ToList();
            var ebxofres = allRes.Where(x => x.DisplayName.ToLower().Contains("splashscreen")).ToList();

            Texture textureAsset = null;
            foreach (var res in projectManagement.FrostyProject.AssetManager.EnumerateRes().Where(x => x.Name.ToLower() == "content/ui/splashscreen/splashscreen").ToList())
            {
                //File.WriteAllText($"Debugging/RES/{res.DisplayName}", JsonConvert.SerializeObject(res));
                //var resStream = projectManagement.FrostyProject.AssetManager.GetRes(res);
                if (res.Type == "Texture")
                {
                    using (var resStream = projectManagement.FrostyProject.AssetManager.GetRes(res))
                    {
                        textureAsset = new Texture(resStream, projectManagement.FrostyProject.AssetManager);
                    }
                }
            }

            var dataOffset = 0;
            byte[] c2 = null;
            var path = "";
            TextureImporter importer = new TextureImporter();
            importer.ImportTextureFromFileToTextureAsset_Original(
                 @"G:\splashscreen_v2k4.DDS"
                 , allEBX.FirstOrDefault()
                 , projectManagement.FrostyProject.AssetManager
                 , ref textureAsset
                 , out string message);
            //{
            //    //reader.Position = 0;
            //    //var c0 = Utils.CompressTexture(reader.ReadBytes((int)reader.Length), textureAsset);
            //    //reader.Position = 0;
            //    //var c1 = Utils.CompressTexture(reader.ReadBytes((int)reader.Length), textureAsset, CompressionType.Default);
            //    reader.Position = 0;
            //    c2 = Utils.CompressTexture(reader.ReadBytes((int)reader.Length), textureAsset, CompressionType.Oodle);

            //    dataOffset = (int)textureAsset.ChunkEntry.ExtraData.DataOffset;
            //    path = projectManagement.FrostyProject.FileSystem.ResolvePath(textureAsset.ChunkEntry.ExtraData.CasPath);
            //    textureAsset.Dispose();
            //    textureAsset = null;
            //    GC.Collect();
            //    GC.WaitForPendingFinalizers();

            //}

            //using (var fs = TryOpen(path, FileMode.Open, FileAccess.ReadWrite, maximumAttempts: 5, attemptWaitMS: 500))
            //{
            //    using (NativeWriter writer = new NativeWriter(fs))
            //    {
            //        //writer.Seek(dataOffset, SeekOrigin.Begin);
            //        //writer.Write(c2);
            //    }
            //}
                //}
        }

        private static void ImportTexture(string pathOnDisk, string ResPath)
        {
            var allEBX = projectManagement.FrostyProject.AssetManager.EnumerateEbx().Where(x => x.Name.ToLower() == ResPath).ToList();

            Texture textureAsset = null;
            foreach (var res in projectManagement.FrostyProject.AssetManager.EnumerateRes().Where(x => x.Name.ToLower() == ResPath).ToList())
            {
                if (res.Type == "Texture")
                {
                    using (var resStream = projectManagement.FrostyProject.AssetManager.GetRes(res))
                    {
                        textureAsset = new Texture(resStream, projectManagement.FrostyProject.AssetManager);
                    }
                }
            }

            if (textureAsset != null)
            {
                TextureImporter importer = new TextureImporter();
                importer.ImportTextureFromFileToTextureAsset_Original(
                     @"G:\splashscreen_v2k4.DDS"
                     , allEBX.FirstOrDefault()
                     , projectManagement.FrostyProject.AssetManager
                     , ref textureAsset
                     , out string message);
            }
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

        public void Log(string text, params object[] vars)
        {
            Console.WriteLine(text);
        }

        public void LogWarning(string text, params object[] vars)
        {
        }

        public void LogError(string text, params object[] vars)
        {
        }
    }
}
