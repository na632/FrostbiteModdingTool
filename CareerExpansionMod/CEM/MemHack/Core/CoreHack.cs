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

namespace v2k4FIFAModdingCL.MemHack.Core
{
    public delegate void EventGameDateChangedHandler(DateTime oldDate, DateTime newDate);

    public class CoreHack
    {
        private class POINTER_ADDRESSES
        {
            public string GAME_DATE = "FIFA20.exe+072ECAF8,0x8,0x3C0";
            public string GAME_SAVE_NAME = "FIFA20.exe+06E2CB40,0x10,0x78,0x8,0x28,0x35C";

            /// <summary>
            /// To get in Cheat Engine. Load a game where you know the saved date. Then load another with a different date. Then pointer search.
            /// </summary>
            public static string GAME_SAVE_FILE = "FIFA20.exe+06DE0688,0x20,0x18,0x7BC";


        }

        public static class AOB_ADDRESSES
        {
            // As this is not working in memory you can use this to scan in Cheat Engine to find some stuff quicker
            public static string GAME_SAVE_FILE = "43 61 72 65 65 72 32 30 32 30 30 34 30 37";

            public static string GAME_DATE = "?? ?? 34 01 00 00 00 00 ?? ?? ?? ?? FF FF FF FF ?? ?? 00 00 00 00 00 00 FF FF FF FF FF FF FF FF 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 3A B6 61 03 06 6C 38";
                
        }

        public static class SAVED_ADDRESSES
        {
            public static string GAME_DATE = string.Empty;
        }

        private static DateTime internal_gamedate = DateTime.Now;

        public static bool IsBGLoading = false;

        public DateTime? GameDate
        {
            get
            {
                if (GetProcess().HasValue)
                {
                    //var aobgDate = memory.Scanner.GetScannerAndReadAddress(AOB_ADDRESSES.GAME_DATE);
                    //var gDate = MemLib.readInt(aobgDate.ToString());

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
                            CEMCore.CEMCoreInstance.CoreHack = this;
                            if (d.Date != internal_gamedate.Date)
                            {
                                //EventGameDateChanged?.Invoke(internal_gamedate.Date, d.Date);
                                CEMCore.CEMCoreInstance.GameDateChanged(internal_gamedate.Date, d.Date);
                                internal_gamedate = d.Date;
                            }
                            return d;
                        }
                        else
                            return null;
                    }
                    else
                        return null;

                }
                return DateTime.Now;
            }
        }

        public string CareerSaveFileLocation { get; set; }

        public static string LastSaveName;

        private string _saveName;

        public static bool DBHandling;

        public string SaveName
        {
            get
            {

                if (GetProcess().HasValue)
                {
                        _saveName = MemLib.readString(new POINTER_ADDRESSES().GAME_SAVE_NAME);
                }
                return _saveName;
            }
            set
            {
                _saveName = value;
            }
        }

        public string SaveFileName
        {
            get
            {

                if (GetProcess().HasValue)
                {
                    //var aob = MemLib.AoBScan("?? ?? 00 00 32 7D A2 0A 00", true, true, true).Result.FirstOrDefault();
                    //var code = MemLib.get64bitCode(aob.ToString());
                    //if(code != null)
                    //{

                    //}
                    //Debug.WriteLine(aob);
                    //foreach (var abi in aob)
                    //{
                    //    var result = MemLib.readString(abi.ToString());
                    //    Debug.WriteLine(result);

                    return MemLib.readString(POINTER_ADDRESSES.GAME_SAVE_FILE);

                    //}
                    //return result;
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


    }
}
