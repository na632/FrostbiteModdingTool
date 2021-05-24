using FrostySdk;
using FrostySdk.Interfaces;
using Lunar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
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

        public static string GameDataPath { get { return GAMERootPath + "\\Data\\"; } }

        public static string FIFALocaleINIPath { get { return GAMERootPath + "\\Data\\locale.ini"; } }
        public static string FIFALocaleINIModDataPath { get { return ModDataPath + "\\Data\\locale.ini"; } }
        public static string GamePatchPath { get { return GAMERootPath + "\\Patch\\"; } }
        public static string FIFA_INITFS_Win32 { get { return GamePatchPath + "\\initfs_Win32"; } }

        public static string LegacyModsPath { get { return GAMERootPath + "\\LegacyMods\\Legacy\\"; } }

        public static string GameEXE { get { return GAMERootPath + "\\" + GAMEVERSION + ".exe"; } }

        public static string ModDataPath { get { return GAMERootPath + "\\ModData\\"; } }

        public static bool InitializeSingleton(string filePath)
        {
            if(!string.IsNullOrEmpty(filePath))
            {
                var GameDirectory = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                GAMERootPath = GameDirectory;
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                if (!string.IsNullOrEmpty(fileName) && GameInstanceSingleton.CompatibleGameVersions.Contains(fileName))
                {
                    GAMEVERSION = fileName.Replace(".exe", "");
                    INITIALIZED = true;
                }
                if(ProfilesLibrary.ProfileName == null)
                {
                    ProfilesLibrary.Initialize(GAMEVERSION);
                }
            }

            return INITIALIZED;
        }

        public static IEnumerable<string> CompatibleGameVersions
        {
            get
            {
                var lstOfComp = new List<string>();
                lstOfComp.AddRange(CompatibleGameFBModVersions);
                lstOfComp.AddRange(CompatibleGameLegacyModVersions);
                return lstOfComp.Distinct();
            }
        }

        public static List<string> CompatibleGameFBModVersions = new List<string>()
        {
            //"FIFA20.exe",
            "FIFA21.exe",
            "Madden21.exe"
        };

        public static bool IsCompatibleWithFbMod()
        {
            return CompatibleGameFBModVersions.Any(x => x.Contains(GAMEVERSION, StringComparison.OrdinalIgnoreCase));
        }

        public static List<string> CompatibleGameLegacyModVersions = new List<string>()
        {
            "FIFA20.exe",
            "FIFA21.exe"
        };

        public static bool IsCompatibleWithLegacyMod()
        {
            return CompatibleGameLegacyModVersions.Any(x => x.Contains(GAMEVERSION, StringComparison.OrdinalIgnoreCase));
        }

        public static ILogger Logger;

        public static bool LegacyInjectionExtraAssertions = true;

        public static int? GetProcIDFromName(string name) //new 1.0.2 function
        {

            if (name.Contains(".exe"))
                name = name.Replace(".exe", "");

            int? id = null;
            int attempts = 0;
            while (!id.HasValue && attempts < 120)
            {
                Process[] processlist = Process.GetProcesses();

                Thread.Sleep(500);
                foreach (Process theprocess in processlist)
                {
                    if (theprocess.ProcessName.Equals(name, StringComparison.CurrentCultureIgnoreCase)) //find (name).exe in the process list (use task manager to find the name)
                        id = theprocess.Id;
                }

                attempts++;
            }

            if (id.HasValue)
                return id;

            throw new ArgumentException($"Unable to find Process {name} running on your PC");
        }

        public static int InjectDLL_GetProcess()
        {
            int attempts = 0;

            bool ModuleLoaded = !LegacyInjectionExtraAssertions;
            int? proc = GetProcIDFromName(GAMEVERSION);
            while ((!proc.HasValue || proc == 0 || !ModuleLoaded) && attempts < 60)
            {
                Debug.WriteLine($"Waiting for {GAMEVERSION} to appear");
                Thread.Sleep(1000);
                proc = GetProcIDFromName(GAMEVERSION);
                if (proc.HasValue)
                {
                    if (LegacyInjectionExtraAssertions)
                    {
                        Process actualProcess = null;
                        try
                        {
                            //var processes = Process.GetProcessesByName(GAMEVERSION);
                            actualProcess = Process.GetProcessById(proc.Value);
                            foreach (ProcessModule m in actualProcess.Modules)
                            {
                                if (m.FileName.Contains("powdll_Win64_retail", StringComparison.OrdinalIgnoreCase))
                                {
                                    ModuleLoaded = true;
                                }
                                if (m.FileName.Contains("sysdll_Win64_retail", StringComparison.OrdinalIgnoreCase))
                                {
                                    ModuleLoaded = true;
                                }
                            }
                        }
                        catch
                        {
                            throw new Exception("Injector: Unable to access process");
                        }
                    }

                    if(ModuleLoaded)
                        return proc.Value;
                }
                attempts++;
            }

            throw new Exception("Injector has failed to inject");

        }

        public static bool InjectDLL_Bleak(int proc, string dllpath)
        {
            using (var bl = new Bleak.Injector(Bleak.InjectionMethod.CreateThread, proc, @dllpath, false))
            {
                var ptr = bl.InjectDll();
                return (ptr != null); // Bleak Injected
            }
        }

        public static bool InjectDLL_Lunar(int proc, string dllpath)
        {
            try
            {
                LibraryMapper libraryMapper = new LibraryMapper(Process.GetProcessById(proc), dllpath);
                libraryMapper.MapLibrary();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool InjectDLL_Own(int proc, string dllpath)
        {
            return InjectDLLIntoProcessFromPath(dllpath, proc);
        }

        public static async Task<bool> InjectDLL(string dllpath)
        {
            dllpath = dllpath.Replace(@"\\\\", @"\");
            dllpath = dllpath.Replace(@"\\", @"\");
            if (File.Exists(dllpath))
            {
                int proc = InjectDLL_GetProcess();
                if (!LegacyInjectionExtraAssertions)
                {
                    // Waiting for process to fully awake
                    //Thread.Sleep(2750);
                    await Task.Delay(2750);
                }

                

                var dll_File = dllpath.Split('\\')[dllpath.Split('\\').Length - 1];
                dll_File = dll_File.Replace(".dll", "");
                try
                {
                    // Use Bleak first
                    bool injected = InjectDLL_Bleak(proc, dllpath);
                    if (!injected)
                    {
                        // Own second
                        injected = InjectDLL_Own(proc, dllpath);
                        if (!injected)
                        {
                            // Last resort Lunar
                            injected = InjectDLL_Lunar(proc, dllpath);
                        }
                    }

                    return injected;
                }
                catch (Exception ex)
                {
                    if (Logger != null)
                    {
                        Logger.LogError($"DLL Injector Failed with Exception: {ex.ToString()}");
                    }
                    else
                    {
                        Debug.WriteLine($"DLL Injector Failed with Exception: {ex.ToString()}");
                    }
                }
            }
            else
            {
                Debug.WriteLine("[ERROR] InjectDLL :: " + dllpath + " not found");
            }

            return false;
        }

        public static async Task<bool> InjectDLLAsync(string dllpath)
        {
            return await InjectDLL(dllpath);
        }

        public static bool InjectDLLSync(string dllpath)
        {
            return InjectDLL(dllpath).Result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DllPath"></param>
        /// <param name="ProcId"></param>
        /// <returns></returns>
        private static bool InjectDLLIntoProcessFromPath(string DllPath, int ProcId)
        {
            // Open handle to the target process
            IntPtr ProcHandle = OpenProcess(
                ProcessAccessFlags.All,
                false,
                ProcId);
            if (ProcHandle == null)
            {
                Logger.LogError("[!] Handle to target process could not be obtained!");
                return false;
            }
            else
            {
                Debug.WriteLine("[+] Handle (0x" + ProcHandle + ") to target process has been be obtained.");
            }

            IntPtr Size = (IntPtr)DllPath.Length;

            // Allocate DLL space
            IntPtr DllSpace = VirtualAllocEx(
                ProcHandle,
                IntPtr.Zero,
                Size,
                AllocationType.Reserve | AllocationType.Commit,
                MemoryProtection.ExecuteReadWrite);

            if (DllSpace == null)
            {
                Logger.LogError("[!] DLL space allocation failed.");
                return false;
            }
            else
            {
                Debug.WriteLine("[+] DLL space (0x" + DllSpace + ") allocation is successful.");
            }

            // Write DLL content to VAS of target process
            byte[] bytes = Encoding.ASCII.GetBytes(DllPath);
            bool DllWrite = WriteProcessMemory(
                ProcHandle,
                DllSpace,
                bytes,
                (int)bytes.Length,
                out var bytesread
                );

            if (DllWrite == false)
            {
                Logger.LogError("[!] Writing DLL content to target process failed.");
                return false;
            }
            else
            {
                Debug.WriteLine("[+] Writing DLL content to target process is successful.");
            }

            // Get handle to Kernel32.dll and get address for LoadLibraryA
            IntPtr Kernel32Handle = GetModuleHandle("Kernel32.dll");
            IntPtr LoadLibraryAAddress = GetProcAddress(Kernel32Handle, "LoadLibraryA");

            if (LoadLibraryAAddress == null)
            {
                Logger.LogError("[!] Obtaining an addess to LoadLibraryA function has failed.");
                return false;
            }
            else
            {
                Debug.WriteLine("[+] LoadLibraryA function address (0x" + LoadLibraryAAddress + ") has been obtained.");
            }

            // Create remote thread in the target process
            IntPtr RemoteThreadHandle = CreateRemoteThread(
                ProcHandle,
                IntPtr.Zero,
                0,
                LoadLibraryAAddress,
                DllSpace,
                0,
                IntPtr.Zero
                );

            if (RemoteThreadHandle == null)
            {
                Logger.LogError("[!] Obtaining a handle to remote thread in target process failed.");
                return false;
            }
            else
            {
                Debug.WriteLine("[+] Obtaining a handle to remote thread (0x" + RemoteThreadHandle + ") in target process is successful.");
            }

            return true;

        }


        // OpenProcess signture https://www.pinvoke.net/default.aspx/kernel32.openprocess
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
        ProcessAccessFlags processAccess,
        bool bInheritHandle,
        int processId);
        public static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags)
        {
            return OpenProcess(flags, false, proc.Id);
        }

        // VirtualAllocEx signture https://www.pinvoke.net/default.aspx/kernel32.virtualallocex
        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        // VirtualFreeEx signture  https://www.pinvoke.net/default.aspx/kernel32.virtualfreeex
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress,
        int dwSize, AllocationType dwFreeType);

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            IntPtr dwSize,
            AllocationType flAllocationType,
            MemoryProtection flProtect);

        // WriteProcessMemory signture https://www.pinvoke.net/default.aspx/kernel32/WriteProcessMemory.html
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        [MarshalAs(UnmanagedType.AsAny)] object lpBuffer,
        int dwSize,
        out IntPtr lpNumberOfBytesWritten);

        // GetProcAddress signture https://www.pinvoke.net/default.aspx/kernel32.getprocaddress
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        // GetModuleHandle signture http://pinvoke.net/default.aspx/kernel32.GetModuleHandle
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        // CreateRemoteThread signture https://www.pinvoke.net/default.aspx/kernel32.createremotethread
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(
        IntPtr hProcess,
        IntPtr lpThreadAttributes,
        uint dwStackSize,
        IntPtr lpStartAddress,
        IntPtr lpParameter,
        uint dwCreationFlags,
        IntPtr lpThreadId);

        // CloseHandle signture https://www.pinvoke.net/default.aspx/kernel32.closehandle
        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);
    }
}
