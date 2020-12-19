using Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace v2k4FIFAModdingCL
{
    public static class GameInstanceSingleton
    {
        public static bool INITIALIZED = false;

        private static string fifaVersion;
        public static string GAMEVERSION { set { fifaVersion = value; INITIALIZED = !string.IsNullOrEmpty(value); } get { return fifaVersion; } }
        public static string FIFAVERSION_NODEMO { get { return !string.IsNullOrEmpty(GAMEVERSION) ? GAMEVERSION.Replace("_demo", "") : null; } }

        public static string GAMERootPath = "";

        public static string FIFADataPath { get { return GAMERootPath + "\\Data\\"; } }

        public static string FIFALocaleINIPath { get { return GAMERootPath + "\\Data\\locale.ini"; } }
        public static string FIFAPatchPath { get { return GAMERootPath + "\\Patch\\"; } }
        public static string FIFA_INITFS_Win32 { get { return FIFAPatchPath + "\\initfs_Win32"; } }

        public static string LegacyModsPath { get { return GAMERootPath + "\\LegacyMods\\Legacy\\"; } }

        public static bool InitializeSingleton(string filePath)
        {
            if(!string.IsNullOrEmpty(filePath))
            {
                var FIFADirectory = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                GAMERootPath = FIFADirectory;
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                if (!string.IsNullOrEmpty(fileName) && GameInstanceSingleton.CompatibleGameVersions.Contains(fileName))
                {
                    GAMEVERSION = fileName.Replace(".exe", "");
                    return true;
                }
            }

            return false;
        }

        public static List<string> CompatibleGameVersions = new List<string>()
        {
            //"FIFA19.exe",
            //"FIFA20_demo.exe",
            "FIFA20.exe",
            "FIFA21.exe",
            "MADDEN20.exe",
            "MADDEN21.exe",
        };

        public static List<string> CompatibleGameFBModVersions = new List<string>()
        {
            "FIFA20.exe"
        };

        public static bool IsCompatibleWithFbMod()
        {
            return CompatibleGameFBModVersions.Any(x => x.Contains(GAMEVERSION));
        }

        public static List<string> CompatibleGameLegacyModVersions = new List<string>()
        {
            "FIFA20.exe",
            "FIFA21.exe"
        };

        public static bool IsCompatibleWithLegacyMod()
        {
            return CompatibleGameLegacyModVersions.Any(x => x.Contains(GAMEVERSION));
        }

        public static int? GetProcIDFromName(string name) //new 1.0.2 function
        {
            Process[] processlist = Process.GetProcesses();

            if (name.Contains(".exe"))
                name = name.Replace(".exe", "");

            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName.Equals(name, StringComparison.CurrentCultureIgnoreCase)) //find (name).exe in the process list (use task manager to find the name)
                    return theprocess.Id;
            }

            return null; //if we fail to find it
        }

        public static void InjectDLL(string dllpath)
        {
            Thread.Sleep(5000);

            dllpath = dllpath.Replace(@"\\\\", @"\");
            dllpath = dllpath.Replace(@"\\", @"\");
            if (File.Exists(dllpath))
            {
                int? proc = GetProcIDFromName(GAMEVERSION);
                while (!proc.HasValue || proc == 0)
                {
                    Debug.WriteLine($"Waiting for {GAMEVERSION} to appear");
                    proc = GetProcIDFromName(GAMEVERSION);
                    Thread.Sleep(5000);
                }
                if (proc.HasValue)
                {
                    Debug.WriteLine($"About to inject: {dllpath}");
                    Thread.Sleep(1000);
                    var bl = new Bleak.Injector(Bleak.InjectionMethod.CreateThread, proc.Value, @dllpath, false);
                    bl.InjectDll();
                    Debug.WriteLine($"Injected: {dllpath}");
                }
            }
            else
            {
                Debug.WriteLine("[ERROR] InjectDLL :: " + dllpath + " not found");
            }
        }

        public static async void InjectDLLAsync(string dllpath)
        {
            await Task.Run(() =>
            {
                InjectDLL(dllpath);
            });
        }
    }
}
