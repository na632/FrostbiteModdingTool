using Frosty.ModSupport;
using FrostyModManager;
using FrostySdk;
using FrostySdk.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using paulv2k4ModdingExecuter;
using System.IO;
using v2k4FIFAModdingCL;
using System.Reflection;
using System.Diagnostics;


namespace FIFAModdingUI
{
    public static class LaunchFIFA
    {
        public static void Launch()
        {
            

            TestLog testLog = new TestLog();
            if (!ProfilesLibrary.Initialize("FIFA20"))
            {
                throw new Exception("Unable to Initialize Profile");
            }
            FIFAInstanceSingleton.FIFARootPath = @"E:\Origin Games\FIFA 20";
            FileSystem fileSystem = new FileSystem(FIFAInstanceSingleton.FIFARootPath);
            foreach (FileSystemSource source in ProfilesLibrary.Sources)
            {
                fileSystem.AddSource(source.Path, source.SubDirs);
            }
            fileSystem.Initialize();
            _ = new FrostyProfile("Default");
            //var files = Directory.GetFiles(
            //    Directory.GetParent(Assembly.GetExecutingAssembly().Location)
            //    + "\\Mods\\")
            //    .Where(x => x.ToLower().Contains(".fbmod"));

            var files = new List<string>();
            //Directory.GetFiles(
            //    Directory.GetParent(Assembly.GetExecutingAssembly().Location)
            //    + "\\Mods\\")
            //    .Where(x => x.ToLower().Contains(".fbmod"));
            files = Directory.EnumerateFiles(
                Directory.GetParent(Assembly.GetExecutingAssembly().Location)
                + "\\Mods\\").Where(x => x.ToLower().Contains(".fbmod")).Select(
                f => new FileInfo(f).Name).ToList();

            //var files = new List<string>() { "REMOVEME.fbmod" };
            var tsk = new FrostyModExecutor().Run(fileSystem, testLog, "Mods/", "", files.ToArray());
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
