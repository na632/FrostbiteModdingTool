using CommandLine;
using FrostySdk;
using System;
using System.Collections.Generic;
using System.IO;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;

namespace paulv2k4FrostbiteImportExport
{
    class Program
    {

        public class Options
        {
            [Option('i', "import", Required = false, HelpText = "Import")]
            public bool Import { get; set; }

            [Option('e', "export", Required = false, HelpText = "Export")]
            public bool Export { get; set; }

            [Option('m', "importFiles", Required = false, HelpText = "Imported files locations, use an = sign to determine the EBX/RES/Chunk they import into", Separator = ',')]
            public IEnumerable<string> ImportFiles { get; set; }

            [Option('x', "exportFiles", Required = false, HelpText = "Export files search wildcard", Separator = ',')]
            public IEnumerable<string> ExportFiles { get; set; }

            [Option(longName: "ExtractGameplayOnly")]
            public bool ExtractGameplayOnly { get; set; }

            [Option('p', "GamePath", Required = true, HelpText = "The path to your game EXE (e.g. C:/FIFA20/FIFA20.exe)")]
            public string GamePath { get; set; }

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

        private static void RunOptions(Options obj)
        {
            if (!Directory.Exists("Extract"))
                Directory.CreateDirectory("Extract");

            if (!Directory.Exists("Import"))
                Directory.CreateDirectory("Import");

            InitializeOfSelectedGame(obj.GamePath);
            ProjectManagement projectManagement = new ProjectManagement();
            var project = projectManagement.StartNewProject();

            if (obj.ExtractGameplayOnly)
            {

            }


        }
    }
}
