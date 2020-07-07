using Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModding.Career;
using v2k4FIFAModdingCL.MemHack.Career;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using CareerExpansionMod.CEM;
using Microsoft.AspNetCore.Http;
using System.Net;
using ElectronNET.API.Entities;
using System.Runtime.Serialization.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using CareerExpansionMod;
using CareerExpansionMod.CEM.MemHack;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace v2k4FIFAModdingCL.MemHack.Core
{
    public delegate void EventGameDateChangedHandler(DateTime oldDate, DateTime newDate);
    public delegate void EventGameSaveChangedHandler();

    public class CoreHack
    {
        public static IHubContext<CareerExpansionMod.Hubs.InfoHub> HubContext;

        public CoreHack()
        {

        }

        /// <summary>
        /// Initialise with a Notification SignalR Hub
        /// </summary>
        /// <param name="hubContext"></param>
        public CoreHack(IHubContext<CareerExpansionMod.Hubs.InfoHub> hubContext)
        {
            HubContext = hubContext;
        }

        public class POINTER_ADDRESSES
        {
            public string GAME_DATE = "FIFA20.exe+072F6B10,0x8,0x3C0";

            /// <summary>
            /// To get in Cheat Engine. Load a game where you know the saved date. Then load another with a different date. Then pointer search.
            /// </summary>
            public string GAME_SAVE_FILE = "FIFA20.exe+06dea808,0x20,0x20,0x54C";

            public string GAME_SAVE_NAME = "FIFA20.exe+06dea808,0x20,0x20,0x5cc";

            // This works but its VERY slow
            public string GAME_SAVE_NAME_AOB = "? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? ? 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 08 6D 1F AE 12 B2 F8 45 A6 14 2F E0 3B A9 2B 64 F8 7D A4 45 01 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 D6 A2 D1 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 53 61 76 65 54 79 70 65 5F 43 61 72 65 65 72 00 43 61 72 65 65 72";

            public Dictionary<string, string> FinancePointers = new Dictionary<string, string>();
           
            public POINTER_ADDRESSES()
            {
                // General default addresses
                FinancePointers.Add("FINANCES_STARTING_POINTER_ADDRESS", "FIFA20.exe+06E3a228,0x10,0x48,0x30,0x58,0x5E0");
                FinancePointers.Add("FINANCES_STARTING_BUDGET", "FIFA20.exe+06E3a228,0x10,0x48,0x30,0x58,0x5E0");
                FinancePointers.Add("FINANCES_TRANSFER_BUDGET", "FIFA20.exe+06E3a228,0x10,0x48,0x30,0x58,0x5F8");
                FinancePointers.Add("FINANCES_TEAMID", "FIFA20.exe+06E3a228,0x10,0x48,0x30,0x58,0x618");
            }

            private static POINTER_ADDRESSES POINTER_ADDRESSES_REF;

            public static POINTER_ADDRESSES LoadPointerAddresses()
            {
                if (POINTER_ADDRESSES_REF == null)
                {
                    POINTER_ADDRESSES_REF = new POINTER_ADDRESSES();

                    try
                    {
                        //if (CoreHack.HubContext != null && CoreHack.HubContext.Clients != null)
                        //    CoreHack.HubContext.Clients.All.SendAsync("SendMessage", "CEM", "Hello");

                        //WebRequest request = WebRequest.Create("https://www.fifa-mods.com/CEM/DownloadClientUpdate/FIFA20/");
                        //request.Method = "GET";
                        //request.Timeout = 2000;
                        ////request.ContentType = "application/json;";
                        //using (var response = request.GetResponse())
                        //{
                        //    //(HttpWebResponse)
                        //    using (var sr = new StreamReader(response.GetResponseStream()))
                        //    {
                        //        var jsonR = sr.ReadToEnd().Replace("\\r\\n", "").Replace("\\", "").Remove(0, 1);
                        //        jsonR = jsonR.Remove(jsonR.Length - 1, 1);
                        //        try
                        //        {
                        //            POINTER_ADDRESSES_REF = JsonConvert.DeserializeObject<POINTER_ADDRESSES>(jsonR);
                        //        }
                        //        catch (Exception)
                        //        {
                        //            Debug.WriteLine("Unable to get POINTER_ADDRESSES from web");
                        //        }
                        //    }
                        //}
                    }
                    catch (WebException)
                    {
                        //if (Startup.InfoHubContext != null)
                        //{
                        //    var context = Startup.InfoHubContext;
                        //    context.Clients.All.SendAsync("SendMessage", "CEM", "Unable to connect to FIFA-Mods Server");
                        //}
                    }
                    catch (Exception)
                    {
                    }

                    var serialized = JsonConvert.SerializeObject(POINTER_ADDRESSES_REF);
                    Debug.WriteLine(serialized);







                }
                return POINTER_ADDRESSES_REF;
            }

        }

        public static class SAVED_ADDRESSES
        {
            public static string GAME_DATE = string.Empty;
        }

        private static DateTime internal_gamedate = DateTime.Now.Date;

        public static bool IsBGLoading = false;

        public static bool IsSettingDate = false;

        private DateTime? _gamedate;

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

        public DateTime? GetInGameDate()
        {
            try
            {

            
                if (GetProcess().HasValue)
                {
                    if (!IsSettingDate && !IsBGLoading) 
                    {
                        IsSettingDate = true;
                        var gDate = MemLib.readInt(new POINTER_ADDRESSES().GAME_DATE);
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

                                CEMCore.CEMCoreInstance.CoreHack = this;
                                if (d.Date != internal_gamedate.Date)
                                {
                                    //EventGameDateChanged?.Invoke(internal_gamedate.Date, d.Date);
                                    if(d.Date.AddDays(-1).Date == internal_gamedate.Date || internal_gamedate.Date == DateTime.Now.Date)
                                        CEMCore.CEMCoreInstance.GameDateChanged(internal_gamedate.Date, d.Date);
                                    else
                                    {
                                        var diffDays = (d.Date - internal_gamedate.Date).Days;
                                        for(var i = 0; i<diffDays; i++)
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
            return await new TaskFactory().StartNew(() =>
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
                //while(searching)
                //{
                //    Thread.Sleep(1000);
                //}    
                //long num = Startup.FIFAProcess.MainModule.BaseAddress.ToInt64();
                //MemoryReader memoryReader = new MemoryReader(Startup.FIFAProcess, num);
                //if (!savenameaddress.HasValue)
                //{
                //    searching = true;
                //    savenameaddress = memoryReader.scan(new POINTER_ADDRESSES().GAME_SAVE_NAME_AOB).FirstOrDefault();
                //}

                _saveName = MemLib.readString(new POINTER_ADDRESSES().GAME_SAVE_NAME);
                SaveNameChanged = false;

                //if (!string.IsNullOrEmpty(LastSaveName) && LastSaveName != _saveName)
                if (!string.IsNullOrEmpty(_saveName) && LastSaveName != _saveName)
                {
                    // Reinitialise Core
                    //var cemcore = new CEMCore(this);
                    SaveNameChanged = true;
                    GameSaveNameChanged?.Invoke(this, null);
                }

                if (_saveName.ToLower().Contains("autosave"))
                    _saveName = LastSaveName;

                LastSaveName = _saveName;

            }
            return _saveName;

        }

        public void SetSaveName(string newname)
        {

        }

        public string SaveFileName
        {
            get
            {

                if (GetProcess().HasValue)
                {
                    return MemLib.readString(POINTER_ADDRESSES.LoadPointerAddresses().GAME_SAVE_FILE);
                }
                return null;
            }
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

        public static int? GetProcess()
        {
            //MemLib = StaticMemLib;
            //if (StaticMemLib == null)
            //{
            if (MemLib != null && gameProcId.HasValue && gameProcId > 0)
                return gameProcId;

            MemLib = new Mem();
            gameProcId = MemLib.getProcIDFromName("FIFA20");

            if (gameProcId.HasValue && gameProcId > 0)
            {
                MemLib.OpenProcess(gameProcId.Value);
                return gameProcId;
            }
            //}

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
}
