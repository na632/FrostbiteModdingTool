using FrostyModManager;
using FrostySdk;
using FrostySdk.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using paulv2k4ModdingExecuter;

namespace FIFAModdingUI
{
    public static class LaunchFIFA
    {
        public static void Launch(string FIFARootPath, string ModDirectory, List<string> OrderedListOfMods)
        {
            TestLog testLog = new TestLog();
            if (!ProfilesLibrary.Initialize("FIFA20"))
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

            var tsk = new FrostyModExecutor().Run(fileSystem, testLog, ModDirectory, "-DrawStatsEnable 1", OrderedListOfMods.ToArray());
            tsk.Wait();
        }
    }

    public class TestLog : ILogger
    {
        public void Log(string text, params object[] vars)
        {
            TextWriterTraceListener tr1 = new TextWriterTraceListener(System.Console.Out);

            Console.WriteLine(text);
            Trace.WriteLine(text);
        }

        public void LogError(string text, params object[] vars)
        {
            TextWriterTraceListener tr1 = new TextWriterTraceListener(System.Console.Out);

            Console.WriteLine(text);
            Trace.WriteLine(text);
        }

        public void LogWarning(string text, params object[] vars)
        {
            TextWriterTraceListener tr1 = new TextWriterTraceListener(System.Console.Out);

            Console.WriteLine(text);
            Trace.WriteLine(text);
        }
    }
}
