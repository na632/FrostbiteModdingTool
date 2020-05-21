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
            public string GAME_DATE = "FIFA20.exe+072F2C60,0x8,0x3C0";
            public string GAME_SAVE_NAME = "FIFA20.exe+06DE6708,0x20,0x20,0x5CC";

            /// <summary>
            /// To get in Cheat Engine. Load a game where you know the saved date. Then load another with a different date. Then pointer search.
            /// </summary>
            public static string GAME_SAVE_FILE = "FIFA20.exe+06DE6708,0x20,0x18,0x7BC";


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

        public string SaveName
        {
            get
            {

                if (GetProcess().HasValue)
                {

                    _saveName = MemLib.readString(new POINTER_ADDRESSES().GAME_SAVE_NAME);

                    //if (!string.IsNullOrEmpty(LastSaveName) && LastSaveName != _saveName)
                    if (!string.IsNullOrEmpty(_saveName) && LastSaveName != _saveName)
                    {
                        // Reinitialise Core
                        var cemcore = new CEMCore();
                    }

                    if (_saveName.ToLower().Contains("autosave"))
                        _saveName = LastSaveName;

                    LastSaveName = _saveName;

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
                    return MemLib.readString(POINTER_ADDRESSES.GAME_SAVE_FILE);
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
