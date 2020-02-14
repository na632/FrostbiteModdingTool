using Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace v2k4FIFAModdingCL.MemHack.Core
{
    public class CoreHack
    {
        private class POINTER_ADDRESSES
        {
            public string GAME_DATE = "FIFA20.exe+072BE688,0x8,0x698";
            public string GAME_SAVE_NAME = "FIFA20.exe+06DFF960,0xA0,0x5CC";
        }

        private const int CALL_POLL_IN_SECONDS = 5;

        private static DateTime internal_gamedate = DateTime.Now;

        public event EventHandler GameDateHasChanged;

        public DateTime GameDate
        {
            get
            {
                if (GetProcess(out Mem MemLib).HasValue)
                {
                    var gDate = MemLib.readInt(new POINTER_ADDRESSES().GAME_DATE);
                    var isoDate = gDate.ToString().Substring(0, 4)
                        + "-"
                        + gDate.ToString().Substring(4, 2)
                        + "-"
                        + gDate.ToString().Substring(6, 2);
                    if (DateTime.TryParse(
                        isoDate
                        , out DateTime d))
                    {
                        new TaskFactory().StartNew(async() => { 
                            while(true)
                            {
                                if (internal_gamedate != d)
                                {
                                    internal_gamedate = d;
                                    GameDateHasChanged.Invoke(this, null);
                                }

                                await Task.Delay(CALL_POLL_IN_SECONDS * 1000);
                            }
                        });


                        return d;
                    }
                }
                return DateTime.Now;
            }
        }

        public string SaveName
        {
            get
            {
                if (GetProcess(out Mem MemLib).HasValue)
                {
                    var saveName = MemLib.readString(new POINTER_ADDRESSES().GAME_SAVE_NAME);
                    return saveName;
                }
                return "";
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
