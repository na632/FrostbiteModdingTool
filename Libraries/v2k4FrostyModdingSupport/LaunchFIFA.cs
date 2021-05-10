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

        public static async Task<bool> LaunchAsync(string FIFARootPath, string ModDirectory, List<string> OrderedListOfMods, ILogger logger = null, string FIFAVERSION = "FIFA20", bool forceReinstallMods = false, bool useSymbolicLink = true)
        {
            if (logger == null)
            {
                logger = new TestLog();
                logger.Log("[ERROR] No logger provided for launching service");
            }

            //if(AssetManager.Instance.fs == null)
            //{
            //    throw new Exception("Asset Manager is not initialised");
            //}

            logger.Log("Running Mod Executer");
            var fme = new FrostyModExecutor();
            fme.ForceRebuildOfMods = forceReinstallMods;
            fme.UseSymbolicLinks = useSymbolicLink;


            return await fme.Run(logger, FIFARootPath, ModDirectory, OrderedListOfMods.ToArray());
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
