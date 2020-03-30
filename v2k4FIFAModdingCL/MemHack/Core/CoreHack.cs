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

namespace v2k4FIFAModdingCL.MemHack.Core
{
    public class CoreHack
    {


        private class POINTER_ADDRESSES
        {
            public string GAME_DATE = "FIFA20.exe+072E6A58,0x8,0x688";
            public string GAME_SAVE_NAME = "FIFA20.exe+06DDA5F0,0x20,0x20,0x5CC";
        }

        private const int CALL_POLL_IN_SECONDS = 5;

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
                            GameSaveStateInstance.CoreHack = this;
                            SaveToFile();
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
        Thread DBHandlingTask;

        public string SaveName
        {
            get
            {

                if (GetProcess(out Mem MemLib).HasValue)
                {
                    _saveName = MemLib.readString(new POINTER_ADDRESSES().GAME_SAVE_NAME);
                    if(_saveName != LastSaveName)
                    {
                        if (!DBHandling)
                        {
                            DBHandlingTask = new Thread(() =>
                            {
                                DBHandling = true;
                                //var kvp = CareerUtil.GetCareerSaves().FirstOrDefault(x => x.Value.Contains(_saveName));
                                //CareerSaveFileLocation = kvp.Key;
                                // load db
                                //if (File.Exists(kvp.Value))
                                //    File.Delete(kvp.Value);

                                //File.Copy(CareerSaveFileLocation, kvp.Value);
                                SaveToFile();
                                DBHandling = false;
                            });
                            DBHandlingTask.Start();
                        }
                    }
                    GameSaveStateInstance.CoreHack = this;
                }
                return _saveName;
            }
        }

        public class GameSaveState
        {
            public CoreHack CoreHack { get; set; }
            public Finances Finances { get; set; }
            public Manager Manager { get; set; }
        }

        public static GameSaveState GameSaveStateInstance = new GameSaveState();

        static bool SavingToFile;
        public void SaveToFile()
        {
            if (!SavingToFile)
            {
                var SavingToFileThread = new Thread(() =>
                {
                    SavingToFile = true;
                    File.WriteAllText(GameSaveStateInstance.CoreHack.SaveName + "-" + Guid.NewGuid() + ".json", JsonConvert.SerializeObject("test"));
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
