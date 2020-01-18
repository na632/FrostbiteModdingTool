using Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace v2k4FIFAModdingCL.MemHack.Core
{
    public class CoreHack
    {
        private class POINTER_ADDRESSES
        {
            public string GAME_DATE = "FIFA20.exe+072BC1A0,0x8,0x698";
        }

        public int GameDate
        {
            get
            {
                if (GetProcess(out Mem MemLib).HasValue)
                {
                    var gDate = MemLib.readInt(new POINTER_ADDRESSES().GAME_DATE);
                    return gDate;
                }
                return 0;
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
