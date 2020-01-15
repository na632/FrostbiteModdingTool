using Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace v2k4FIFAModdingCL.MemHack.Core
{
    public class CoreHack
    {
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
