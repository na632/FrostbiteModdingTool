using FrostyModManager;
using FrostySdk;
using FrostySdk.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using paulv2k4ModdingExecuter;
using System.Threading.Tasks;
using FrostySdk.Managers;

namespace FIFAModdingUI
{
    public static class LaunchFIFA
    {
        public static void Launch(string FIFARootPath, string ModDirectory, List<string> OrderedListOfMods, ILogger logger = null, string FIFAVERSION = "FIFA20", bool buildMods = true, bool useSymbolicLink = true)
        {
            LaunchAsync(FIFARootPath, ModDirectory, OrderedListOfMods, logger, FIFAVERSION, buildMods).Wait();
        }

        public static async Task<int> LaunchAsync(string FIFARootPath, string ModDirectory, List<string> OrderedListOfMods, ILogger logger = null, string FIFAVERSION = "FIFA20", bool buildMods = true, bool useSymbolicLink = true)
        {

            

            if (logger == null)
                logger = new TestLog();

            if (!ProfilesLibrary.Initialize(FIFAVERSION))
            {
                throw new Exception("Unable to Initialize Profile");
            }
            FileSystem fileSystem = new FileSystem(FIFARootPath);
            foreach (FileSystemSource source in ProfilesLibrary.Sources)
            {
                fileSystem.AddSource(source.Path, source.SubDirs);
            }
            fileSystem.Initialize();
            _ = new FrostyProfile("Default");
            var fme = new FrostyModExecutor();
            fme.UseSymbolicLinks = useSymbolicLink;
            return await fme.Run(fileSystem, logger, ModDirectory, "-DrawStatsEnable 1", OrderedListOfMods.ToArray());
        }
    }

    public class TestLog : ILogger
    {
        public void Log(string text, params object[] vars)
        {
            TextWriterTraceListener tr1 = new TextWriterTraceListener(System.Console.Out);

            Debug.WriteLine(text);
        }

        public void LogError(string text, params object[] vars)
        {
            TextWriterTraceListener tr1 = new TextWriterTraceListener(System.Console.Out);

            Debug.WriteLine(text);
        }

        public void LogWarning(string text, params object[] vars)
        {
            TextWriterTraceListener tr1 = new TextWriterTraceListener(System.Console.Out);

            Debug.WriteLine(text);
        }
    }
}
