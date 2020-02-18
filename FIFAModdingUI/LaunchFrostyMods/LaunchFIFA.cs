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

namespace FIFAModdingUI
{
    public class LaunchFIFA
    {
        public static void Launch()
        {
            TestLog testLog = new TestLog();
            if (!ProfilesLibrary.Initialize("FIFA20"))
            {
                throw new Exception("Unable to Initialize Profile");
            }
            FileSystem fileSystem = new FileSystem(FIFAInstanceSingleton.FIFARootPath);
            foreach (FileSystemSource source in ProfilesLibrary.Sources)
            {
                fileSystem.AddSource(source.Path, source.SubDirs);
            }
            fileSystem.Initialize();
            var files = Directory.GetFiles(
                Directory.GetParent(Assembly.GetExecutingAssembly().Location)
                + "\\Mods\\")
                .Where(x => x.ToLower().Contains(".fbmod"));
            var tsk = new FrostyModExecutor().Run(fileSystem, testLog, "Mods/", "", files.ToArray());
            tsk.Wait();
        }
    }

    public class TestLog : ILogger
    {
        public void Log(string text, params object[] vars)
        {
            Console.WriteLine(text);
        }

        public void LogError(string text, params object[] vars)
        {
            Console.WriteLine(text);
        }

        public void LogWarning(string text, params object[] vars)
        {
            Console.WriteLine(text);
        }
    }
}
