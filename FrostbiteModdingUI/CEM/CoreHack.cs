using Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModding.Career;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using CareerExpansionMod.CEM;
using System.Net;
using System.Runtime.Serialization.Json;
using CareerExpansionMod;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace v2k4FIFAModdingCL.MemHack.Core
{
    public delegate void EventGameDateChangedHandler(DateTime oldDate, DateTime newDate);
    public delegate void EventGameSaveChangedHandler();

    public class CoreHack
    {
        public CoreHack()
        {

        }

        public static bool IsInCM()
        {
            if (GetProcess().HasValue)
            {
                if (MemLib != null)
                {
                    if (MemLib.theProc != null)
                    {
                        foreach (ProcessModule ProcessMod in MemLib.theProc.Modules)
                        {
                            if (ProcessMod.FileName.Contains("FootballCompEng_Win64_retail"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        // you need to add 32 to this address
        public static string GAME_DATE_AOB = "?? ?? ?? ?? 00 00 00 00 D0 41 ?? 77 00 00 00 00 01 00 00 00 ?? ?? ?? ?? 58 04 E7 45 01 00 00 00";

        // you need to add 40 to this address
        public static string SEASON_START_DATE_AOB = "?? ?? ?? ?? 00 00 00 00 D0 41 ?? 77 00 00 00 00 01 00 00 00 ?? ?? ?? ?? 58 04 E7 45 01 00 00 00";


        private static string _GAME_DATE_ADDR_20 = "FIFA20.exe+072F6D08,0x8,0x3C0";
        public static string GAME_DATE_ADDR
        {
            get
            {
                if (FIFAProcessName == "FIFA20")
                {
                    // Automatically overwriting with Pointer Address for now
                    if (!string.IsNullOrEmpty(_GAME_DATE_ADDR_20))
                        return _GAME_DATE_ADDR_20;

                    var aobAddy = CoreHack.AOBScan(GAME_DATE_AOB, "GAME_DATE");
                    if (aobAddy.HasValue && aobAddy != 0)
                    {
                        _GAME_DATE_ADDR_20 = CoreHack.ResolveOffset(aobAddy.Value, 32).ToString("X8");

                        return _GAME_DATE_ADDR_20;
                    }
                }
                return null;
            }
        }

        private static string seasonStartDate;
        public static string SeasonStartDate
        {
            get
            {
                if (!string.IsNullOrEmpty(seasonStartDate))
                    return seasonStartDate;

                var aobAddy = CoreHack.AOBScan(SEASON_START_DATE_AOB, "SEASON_START_DATE");
                if (aobAddy.HasValue && aobAddy.Value != 0)
                {
                    seasonStartDate = CoreHack.ResolveOffset(aobAddy.Value, 40).ToString("X8");

                    return seasonStartDate;
                }
                return null;
            }
        }

        /// <summary>
        /// To get in Cheat Engine. Load a game where you know the saved date. Then load another with a different date. Then pointer search.
        /// </summary>
        public string GAME_SAVE_FILE_POINTER_ADDR_20 = "FIFA20.exe+06dea808,0x20,0x20,0x54C";

        public static string GAME_SAVE_NAME_POINTER_ADDR_20 = "FIFA20.exe+06dea808,0x20,0x20,0x5cc";
        public static long GAME_SAVE_NAME_ADDR;

        // This works but its VERY slow
        public static string GAME_SAVE_NAME_AOB = "43 61 72 65 65 72 ? ? ? ? ? ? ? ? ? ? ? ? ? ? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ?? 00 00 00 08 6D 1F AE 12 B2 F8 45 A6 14 2F E0 3B A9 2B 64 20 7E A4 45 01 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 D6 A2 D1 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 53 61 76 65 54 79 70 65 5F 43 61 72 65 65 72 00 43 61 72 65 65 72";

        //public static string GAME_SAVE_NAME_AOB_F21 = "?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 46 81 FF 14 66 76 C7 C8 2A 02 4F A2 98 6B EF 0B 6E 79 A5 E8 12 CF 46 01 00 00 00 02 00 00 00 F6 46 81 FF 00 00 00 00 00 00 00 00 71 AE CE 00 00 00 00 00 00 00 00 00 F6 46 81 FF 00 00 00 00 00 00 00 00 00 00 00 00 F6 46 81 FF 00 00 00 00 00 00 00 00 53 61 76 65 54 79 70 65 5F 43 61 72 65 65 72 00 43 61 72 65 65 72 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 43 61 72 65 65 72";
        public static string GAME_SAVE_NAME_AOB_F21 =   "01 ?? ?? FF 14 66 76 C7 C8 2A 02 4F A2 98 6B EF 0B 6E 79 A5 E8 12 CF 46 01 00 00 00 02 00 00 00 ?? ?? ?? FF 00 00 00 00 00 00 00 00 71 AE CE 00 00 00 00 00 00 00 00 00 ?? ?? ?? FF 00 00 00 00 00 00 00 00 00 00 00 00 ?? ?? ?? FF 00 00 00 00 00 00 00 00 53 61 76 65 54 79 70 65 5F 43 61 72 65 65 72 00 43 61 72 65 65 72";
        //                                              "01 42 7E FF 14 66 76 C7 C8 2A 02 4F A2 98 6B EF 0B 6E 79 A5 E8 12 CF 46 01 00 00 00 02 00 00 00 F5 42 7E FF 00 00 00 00 00 00 00 00 71 AE CE 00 00 00 00 00 00 00 00 00 F5 42 7E FF 00 00 00 00 00 00 00 00 00 00 00 00 F5 42 7E FF 00 00 00 00 00 00 00 00 53 61 76 65 54 79 70 65 5F 43 61 72 65 65 72 00 43 61 72 65 65 72"
        //                                              "01 42 7E FF 14 66 76 C7 C8 2A 02 4F A2 98 6B EF 0B 6E 79 A5 E8 12 CF 46 01 00 00 00 02 00 00 00 F5 42 7E FF 00 00 00 00 00 00 00 00 71 AE CE 00 00 00 00 00 00 00 00 00 F5 42 7E FF 00 00 00 00 00 00 00 00 00 00 00 00 F5 42 7E FF 00 00 00 00 00 00 00 00 53 61 76 65 54 79 70 65 5F 43 61 72 65 65 72 00 43 61 72 65 65 72"
        private static DateTime internal_gamedate = DateTime.Now.Date;

        public static bool IsBGLoading = false;

        public static bool IsSettingDate = false;

        private static DateTime? _gamedate;

        public DateTime? GameDate
        {
            get
            {
                if (GetProcess().HasValue)
                    _gamedate = GetInGameDate();

                return _gamedate;
            }
            set
            {
                _gamedate = value;
            }
        }
        public async Task<DateTime?> GetInGameDateAsync()
        {
            return await new TaskFactory().StartNew(() => {

                return GetInGameDate();
            });
        }

        public DateTime? GetInGameDate()
        {
            try
            {

            
                if (GetProcess().HasValue)
                {
                    if (!IsSettingDate && !IsBGLoading) 
                    {
                        IsSettingDate = true;
                        var gmeDateAddy = GAME_DATE_ADDR;
                        if (!string.IsNullOrEmpty(gmeDateAddy))
                        {
                            var gDate = MemLib.readInt(gmeDateAddy);
                            if (gDate > 0 && gDate.ToString().Length == 8)
                            {
                                var isoDate = gDate.ToString().Substring(0, 4)
                                    + "-"
                                    + gDate.ToString().Substring(4, 2)
                                    + "-"
                                    + gDate.ToString().Substring(6, 2);
                                if (DateTime.TryParse(
                                    isoDate
                                    , out DateTime d))
                                {
                                    if (CEMCore.CEMCoreInstance == null)
                                    {
                                        IsSettingDate = false;
                                        return null;
                                    }

                                    if (d.Date != internal_gamedate.Date)
                                    {
                                        //EventGameDateChanged?.Invoke(internal_gamedate.Date, d.Date);
                                        if (d.Date.AddDays(-1).Date == internal_gamedate.Date || internal_gamedate.Date == DateTime.Now.Date)
                                            CEMCore.CEMCoreInstance.GameDateChanged(internal_gamedate.Date, d.Date);
                                        else
                                        {
                                            var diffDays = (d.Date - internal_gamedate.Date).Days;
                                            for (var i = 0; i < diffDays; i++)
                                            {
                                                CEMCore.CEMCoreInstance.GameDateChanged(internal_gamedate.Date, internal_gamedate.Date.AddDays(i));
                                            }
                                        }

                                        internal_gamedate = d.Date;
                                    }

                                    IsSettingDate = false;
                                    return d;
                                }
                            }
                        }

                        IsSettingDate = false;
                        return null;

                    }

                }
                return DateTime.Now;
            }
            finally
            {
                IsSettingDate = false;
            }
        }

        public string CareerSaveFileLocation { get; set; }

        public static string LastSaveName;
        public static string NonAutosaveName;

        private string _saveName;

        public static bool DBHandling;

        //private long? savenameaddress = null;
        //private static bool searching = false;

        public async Task<string> GetSaveNameAsync()
        {
            return await Task.Run(() =>
            {
                return GetSaveName();
            });
            
        }

        public static bool SaveNameChanged;
        public event EventHandler GameSaveNameChanged;
        public string GetSaveName()
        {
            if (GetProcess().HasValue)
            {
                CEMCore.CEMCoreInstance.Logger.Log("Getting Save Name...");
                var addr = AOBScan(GAME_SAVE_NAME_AOB_F21, "GAME_SAVE_NAME");
                addr -= 64;
                if (addr.HasValue)
                {
                    var addrX8 = addr.Value.ToString("X8");
                    _saveName = MemLib.readString(addrX8);

                    SaveNameChanged = false;

                    //if (!string.IsNullOrEmpty(LastSaveName) && LastSaveName != _saveName)
                    if (!string.IsNullOrEmpty(_saveName) && LastSaveName != _saveName)
                    {
                        CEMCore.CEMCoreInstance.Logger.Log("Save Name found - " + _saveName);

                        // Reinitialise Core
                        //var cemcore = new CEMCore(this);
                        SaveNameChanged = true;
                        GameSaveNameChanged?.Invoke(this, null);
                    }

                    if (_saveName.ToLower().Contains("autosave"))
                        _saveName = LastSaveName;

                    LastSaveName = _saveName;
                }

            }
            return _saveName;

        }

        static bool SavingToFile;
        static DateTime TimeLastSaved = DateTime.Now;
        public void SaveToFile()
        {
            if (!SavingToFile && TimeLastSaved.AddMinutes(1) < DateTime.Today)
            {
                var SavingToFileThread = new Thread(() =>
                {
                    SavingToFile = true;
                    Debug.WriteLine("Saving to file");
                    Trace.WriteLine("Saving to file");
                    //File.WriteAllText(GameSaveStateInstance.CoreHack.SaveName + "-" + Guid.NewGuid() + ".json", JsonConvert.SerializeObject("test"));
                    SavingToFile = false;
                });
                SavingToFileThread.Start();
            }
        }

        private static int? gameProcId;
        public static Mem MemLib;

        public static string FIFAProcessName;

        public static int? GetProcess(bool CMCheck = true)
        {
            //MemLib = StaticMemLib;
            //if (StaticMemLib == null)
            //{

            if (MemLib != null && gameProcId.HasValue && gameProcId > 0)
                return gameProcId;

            if (!GetProcessFIFA21(CMCheck).HasValue && !gameProcId.HasValue)
            {

                MemLib = new Mem();

                gameProcId = MemLib.getProcIDFromName("FIFA20");

                if (gameProcId.HasValue && gameProcId > 0)
                {
                    MemLib.OpenProcess(gameProcId.Value);
                    if (MemLib.theProc != null)
                    {
                        if (CMCheck)
                        {
                            foreach (ProcessModule ProcessMod in MemLib.theProc.Modules)
                            {
                                if (ProcessMod.FileName.Contains("FootballCompEng_Win64_retail"))
                                {
                                    FIFAProcessName = "FIFA20";
                                    return gameProcId;
                                }
                            }
                        }
                        else
                        {
                            return gameProcId;
                        }
                    }

                    MemLib = null;
                    gameProcId = null;
                }
            }
            //}

            return null;


        }

        public static int? GetProcessFIFA21(bool CMCheck = true)
        {

            MemLib = new Mem();

            gameProcId = MemLib.getProcIDFromName("FIFA21");

            if (gameProcId.HasValue && gameProcId > 0)
            {
                MemLib.OpenProcess(gameProcId.Value);
                if (MemLib.theProc != null)
                {
                    if (CMCheck)
                    {
                        foreach (ProcessModule ProcessMod in MemLib.theProc.Modules)
                        {
                            if (ProcessMod.FileName.Contains("FootballCompEng_Win64_retail"))
                            {
                                FIFAProcessName = "FIFA21";
                                return gameProcId;
                            }
                        }
                    }
                    else
                    {
                        return gameProcId;
                    }
                }

                MemLib = null;
                gameProcId = null;
            }
            //}

            return null;

        }

        public static List<string> SCANNING_FOR_LIST = new List<string>();
        public static string SCANNING_FOR = null;


        public static Dictionary<string, long> AOBtoAddress = new Dictionary<string, long>();
        public static Dictionary<string, List<long>> AOBtoAddresses = new Dictionary<string, List<long>>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="aob"></param>
        /// <param name="FRIENDLY_SCAN_NAME"></param>
        /// <returns></returns>
        public static long? AOBScan(string aob, string FRIENDLY_SCAN_NAME = "SOMETHING")
        {
            if (AOBtoAddress.ContainsKey(aob))
                return AOBtoAddress[aob];


            if (GetProcess().HasValue)
            {
                if (MemLib == null)
                    return null;

                if (SCANNING_FOR_LIST.Contains(FRIENDLY_SCAN_NAME))
                {
                    return null;
                }

                Debug.WriteLine("[DEBUG] CoreHack.AOBScan " + FRIENDLY_SCAN_NAME);

                SCANNING_FOR_LIST.Add(FRIENDLY_SCAN_NAME);
                SCANNING_FOR = FRIENDLY_SCAN_NAME;

                
                long baseAddress = MemLib.theProc.MainModule.BaseAddress.ToInt64();
                //long baseSize = MemLib.theProc.Modules[0].ModuleMemorySize;

                //long lastAddress = MemLib.theProc.Modules[MemLib.theProc.Modules.Count - 1].BaseAddress.ToInt64();
                //long lastSize = MemLib.theProc.Modules[MemLib.theProc.Modules.Count - 1].ModuleMemorySize;
                
                var longAddr = MemLib.AoBScan(0, baseAddress, aob, true, true).Result.FirstOrDefault();
                AOBtoAddress.Add(aob, longAddr);
                SCANNING_FOR = null;
                SCANNING_FOR_LIST.Remove(FRIENDLY_SCAN_NAME);
                return longAddr;
            }
            return null;
        }

        public static List<long> AOBScanList(string aob, string FRIENDLY_SCAN_NAME = "SOMETHING")
        {

            if (AOBtoAddresses.ContainsKey(aob))
                return AOBtoAddresses[aob];


            if (GetProcess().HasValue)
            {
                if (SCANNING_FOR_LIST.Contains(FRIENDLY_SCAN_NAME))
                {
                    return null;
                }
                Debug.WriteLine("[DEBUG] CoreHack.AOBScanList " + FRIENDLY_SCAN_NAME);

                SCANNING_FOR_LIST.Add(FRIENDLY_SCAN_NAME);
                SCANNING_FOR = FRIENDLY_SCAN_NAME;

                long baseAddress = MemLib.theProc.MainModule.BaseAddress.ToInt64();
                //long baseSize = MemLib.theProc.Modules[0].ModuleMemorySize;

                long lastAddress = MemLib.theProc.Modules[MemLib.theProc.Modules.Count - 1].BaseAddress.ToInt64();
                long lastSize = MemLib.theProc.Modules[MemLib.theProc.Modules.Count - 1].ModuleMemorySize;
                var longAddresses = MemLib.AoBScan(0, lastAddress + lastSize, aob, true, true).Result;
                AOBtoAddresses.Add(aob, longAddresses.ToList());

                SCANNING_FOR = null;
                SCANNING_FOR_LIST.Remove(FRIENDLY_SCAN_NAME);
                return longAddresses.ToList();
            }
            return null;
        }

        public static UIntPtr ResolveMultipointer(UIntPtr base_addr, List<int> offsets)
        {
            List<UIntPtr> lst = new List<UIntPtr>();
            for(var i = 1; i < offsets.Count(); i++ )
            {
                if (base_addr.ToUInt32() == 0 || base_addr == null)
                {
                    for(var j = 1; j < offsets.Count(); j++)
                    {
                        Debug.WriteLine(ResolvePtr(base_addr, offsets[j]).ToString("X8"));
                    }
                }

                base_addr = ResolvePtr(base_addr, offsets[i]).ToUIntPtr();
                Debug.WriteLine(ResolvePtr(base_addr, offsets[i]).ToString("X8"));

            }
            return base_addr;
        }

        public static long ResolvePtr(string aobaddress, int offsetpos)
        {
            var addr = CoreHack.MemLib.AoBScan(aobaddress, true, true, true).Result.FirstOrDefault();
            if (addr > 0)
            {
                UIntPtr finalPtr = CoreHack.ResolvePtr(addr, offsetpos).ToUIntPtr();
                return (long)finalPtr.ToUInt64();
            }

            return 0;
        }

       
        public static long ResolvePtr(long address, int offsetpos)
        {
            var uintpr = new UIntPtr(Convert.ToUInt64(address));
            return ResolvePtr(uintpr, offsetpos);
        }

        public static long ResolvePtr(UIntPtr address, int offsetpos)
        {
            var proc = CoreHack.GetProcess();
            if (proc.HasValue)
            {
                long offset = Convert.ToInt64((address + offsetpos).ToUInt64());
                Debug.WriteLine($"ResolvePtr:{offset.ToString("X8")}");
                var readint = CoreHack.MemLib.readInt(offset.ToString("X8"));
                Debug.WriteLine($"ResolvePtr:{readint.ToString("X8")}");

                var addOffsetToAddress = new UIntPtr((ulong)readint + (ulong)offset + 4);
                Debug.WriteLine($"ResolvePtr:{addOffsetToAddress.ToUInt64().ToString("X8")}");

                return (long)addOffsetToAddress.ToUInt64();
            }
            return 0;
        }

        public static long ResolveOffset(long address, int addedOffset)
        {
            var proc = CoreHack.GetProcess();
            if (proc.HasValue)
            {
                long offset = Convert.ToInt64((address + addedOffset));
                return offset;
            }
            return address;
        }
    }

    public static class HelpfulExtensionForUIntPtr
    {
        public static UIntPtr ToUIntPtr(this long v)
        {
            return new UIntPtr(Convert.ToUInt64(v));
        }

        public static UIntPtr ToUIntPtr(this ulong v)
        {
            return new UIntPtr(v);
        }
    }


    public static class Cast
    {

        private static class ThreadLocalType<T>
        {

            [ThreadStatic]
            private static T[] buffer;

            public static T[] Buffer
            {
                get
                {
                    if (buffer == null)
                    {
                        buffer = new T[1];
                    }
                    return buffer;
                }
            }
        }

        public static TTarget Reinterpret<TTarget, TSource>(TSource source)
        {
            TSource[] sourceBuffer = ThreadLocalType<TSource>.Buffer;
            TTarget[] targetBuffer = ThreadLocalType<TTarget>.Buffer;

            int sourceSize = Buffer.ByteLength(sourceBuffer);
            int destSize = Buffer.ByteLength(targetBuffer);
            if (sourceSize != destSize)
            {
                var errorText = "Cannot convert " + typeof(TSource).FullName + " to " + typeof(TTarget).FullName + ". Data types are of different sizes.";
                Debug.WriteLine(errorText);
                throw new ArgumentException(errorText);
            }

            sourceBuffer[0] = source;
            Buffer.BlockCopy(sourceBuffer, 0, targetBuffer, 0, sourceSize);
            return targetBuffer[0];
        }
    }
}


