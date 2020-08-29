using Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace v2k4FIFAModdingCL
{
    public static class FIFAInstanceSingleton
    {
        public static bool INITIALIZED = false;

        private static string fifaVersion;
        public static string FIFAVERSION { set { fifaVersion = value; INITIALIZED = !string.IsNullOrEmpty(value); } get { return fifaVersion; } }
        public static string FIFAVERSION_NODEMO { get { return !string.IsNullOrEmpty(FIFAVERSION) ? FIFAVERSION.Replace("_demo", "") : null; } }

        public static string FIFARootPath = "";

        public static string FIFADataPath { get { return FIFARootPath + "\\Data\\"; } }

        public static string FIFALocaleINIPath { get { return FIFARootPath + "\\Data\\locale.ini"; } }
        public static string FIFAPatchPath { get { return FIFARootPath + "\\Patch\\"; } }
        public static string FIFA_INITFS_Win32 { get { return FIFAPatchPath + "\\initfs_Win32"; } }

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
            dllpath = dllpath.Replace(@"\\\\", @"\");
            dllpath = dllpath.Replace(@"\\", @"\");
            if (File.Exists(dllpath))
            {
                int? proc = GetProcIDFromName(FIFAVERSION);
                while (!proc.HasValue || proc == 0)
                {
                    Debug.WriteLine($"Waiting for {FIFAVERSION} to appear");
                    proc = GetProcIDFromName(FIFAVERSION);
                    Thread.Sleep(4000);
                }
                if (proc.HasValue)
                {
                    Debug.WriteLine($"About to inject: {dllpath}");
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
