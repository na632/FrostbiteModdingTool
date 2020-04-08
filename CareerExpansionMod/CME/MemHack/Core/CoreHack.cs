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
using CareerExpansionMod.CME;

namespace v2k4FIFAModdingCL.MemHack.Core
{
    public delegate void EventGameDateChangedHandler(DateTime oldDate, DateTime newDate);

    public class CoreHack
    {
        private class POINTER_ADDRESSES
        {
            public string GAME_DATE = "FIFA20.exe+072E79F8,0x8,0x3C0";
            public string GAME_SAVE_NAME = "FIFA20.exe+06DDB5F0,0x20,0x20,0x5CC";

            /// <summary>
            /// To get in Cheat Engine. Load a game where you know the saved date. Then load another with a different date. Then pointer search.
            /// </summary>
            public static string GAME_SAVE_FILE = "FIFA20.exe+06DDB5F0,0x20,0x18,0x7BC";
        }

        public static class AOB_ADDRESSES
        {
            // As this is not working in memory you can use this to scan in Cheat Engine to find some stuff quicker
            public static string GAME_SAVE_FILE = "43 61 72 65 65 72 32 30 32 30 30 34 30 37";
        }

        private static DateTime internal_gamedate = DateTime.Now;

        public static bool IsBGLoading = false;

        public DateTime? GameDate
        {
            get
            {
                if (GetProcess(out Mem MemLib).HasValue)
                {
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
                            CMECore.CMECoreInstance.CoreHack = this;
                            if (d.Date != internal_gamedate.Date)
                            {
                                //EventGameDateChanged?.Invoke(internal_gamedate.Date, d.Date);
                                CMECore.CMECoreInstance.GameDateChanged(internal_gamedate.Date, d.Date);
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

                if (GetProcess(out Mem MemLib).HasValue)
                {
                        _saveName = MemLib.readString(new POINTER_ADDRESSES().GAME_SAVE_NAME);
                }
                return _saveName;
            }
        }

        public string SaveFileName
        {
            get
            {

                if (GetProcess(out Mem MemLib).HasValue)
                {
                    //var aob = MemLib.AoBScan("43 61 72 65 65 72 32 30 32 30 30 34 30 37", true, true).Result.FirstOrDefault();
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

        public static int? GetProcess(out Mem MemLib)
        {
            MemLib = new Mem();
            int gameProcId = MemLib.getProcIDFromName("FIFA20");

            if (gameProcId != 0)
            {
                MemLib.OpenProcess(gameProcId);
                return gameProcId;
            }
            return null;
        }


    }
}
