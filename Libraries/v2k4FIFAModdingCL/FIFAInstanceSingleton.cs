using Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lunar;
using FrostySdk.Interfaces;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Security;

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

        public static ILogger Logger;

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
            Thread.Sleep(3000);

            dllpath = dllpath.Replace(@"\\\\", @"\");
            dllpath = dllpath.Replace(@"\\", @"\");
            if (File.Exists(dllpath))
            {
                int? proc = GetProcIDFromName(GAMEVERSION);
                while (!proc.HasValue || proc == 0)
                {
                    Debug.WriteLine($"Waiting for {GAMEVERSION} to appear");
                    Thread.Sleep(1000);
                    proc = GetProcIDFromName(GAMEVERSION);
                }
                if (proc.HasValue)
                {
                    //Logger.Log($"About to inject: {dllpath}");
                    Thread.Sleep(100);
                    bool alreadyExists = false;

                    var dll_File = dllpath.Split('\\')[dllpath.Split('\\').Length - 1];
                    dll_File = dll_File.Replace(".dll", "");
                    // Seems to be breaking for some users
                    //foreach (ProcessModule m in Process.GetProcessById(proc.Value).Modules)
                    //{
                    //    if (m.FileName.Contains(dll_File, StringComparison.OrdinalIgnoreCase))
                    //    {
                    //        alreadyExists = true;
                    //        break;
                    //    }
                    //}
                    if (!alreadyExists) {

                        try
                        {
                            InjectDLLIntoProcessFromPath(dllpath, proc.Value);
                            //var bl = new Bleak.Injector(Bleak.InjectionMethod.CreateThread, proc.Value, @dllpath, false);
                            //bl.InjectDll();

                            //var mapper = new LibraryMapper(Process.GetProcessById(proc.Value), dllpath);
                            //mapper.MapLibrary();

                            Debug.WriteLine($"Injected: {dllpath}");
                        }
                        catch(Exception ex)
                        {
                            if(Logger != null)
                            {
                                Logger.LogError($"DLL Injector Failed with Exception: {ex.ToString()}");
                            }
                            else
                            {
                                Debug.WriteLine($"DLL Injector Failed with Exception: {ex.ToString()}");
                            }
                        }
                    }
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


        /**/

        private static void InjectDLLIntoProcessFromPath(string DllPath, int ProcId)
        {
            // Open handle to the target process
            IntPtr ProcHandle = OpenProcess(
                ProcessAccessFlags.All,
                false,
                ProcId);
            if (ProcHandle == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Handle to target process could not be obtained!");
                Logger.LogError("[!] Handle to target process could not be obtained!");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Handle (0x" + ProcHandle + ") to target process has been be obtained.");

                Console.ForegroundColor = ConsoleColor.White;
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] DLL space allocation failed.");
                Logger.LogError("[!] DLL space allocation failed.");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] DLL space (0x" + DllSpace + ") allocation is successful.");
                Console.ForegroundColor = ConsoleColor.White;
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Writing DLL content to target process failed.");
                Logger.LogError("[!] Writing DLL content to target process failed.");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Writing DLL content to target process is successful.");
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Get handle to Kernel32.dll and get address for LoadLibraryA
            IntPtr Kernel32Handle = GetModuleHandle("Kernel32.dll");
            IntPtr LoadLibraryAAddress = GetProcAddress(Kernel32Handle, "LoadLibraryA");

            if (LoadLibraryAAddress == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Obtaining an addess to LoadLibraryA function has failed.");
                Logger.LogError("[!] Obtaining an addess to LoadLibraryA function has failed.");

                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] LoadLibraryA function address (0x" + LoadLibraryAAddress + ") has been obtained.");
                Console.ForegroundColor = ConsoleColor.White;
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Obtaining a handle to remote thread in target process failed.");
                Logger.LogError("[!] Obtaining a handle to remote thread in target process failed.");
                Console.ForegroundColor = ConsoleColor.White;
                System.Environment.Exit(1);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Obtaining a handle to remote thread (0x" + RemoteThreadHandle + ") in target process is successful.");
                Console.ForegroundColor = ConsoleColor.White;
            }
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
