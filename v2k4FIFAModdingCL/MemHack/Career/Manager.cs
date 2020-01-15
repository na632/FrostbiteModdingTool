using Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModdingCL.MemHack.Core;

namespace v2k4FIFAModdingCL.MemHack.Career
{
    public class Manager
    {
        private class POINTER_ADDRESSES
        {
            public string MANAGER_RATING = "FIFA20.exe+06E045B0,0x538,0x0,0x20,0x240,0x574";
        }

        public int ManagerRating
        {
            get
            {
                if (CoreHack.GetProcess(out Mem MemLib).HasValue)
                {
                    var transferbudget = MemLib.readInt(new POINTER_ADDRESSES().MANAGER_RATING);
                    return transferbudget;
                }
                return 0;
            }
            set
            {

                if (CoreHack.GetProcess(out Mem MemLib).HasValue)
                {
                    MemLib.writeMemory(new POINTER_ADDRESSES().MANAGER_RATING, "int", value.ToString());
                }

            }
        }

        public static int GetManagerRating()
        {
            return new Manager().ManagerRating;
        }

        public static void SetManagerRating(int rating)
        {
            new Manager().ManagerRating = rating;
        }

        public static bool Unsackable = false;
        public static Task UnsackableTask;

        public static void MakeMeUnsackable(bool yes)
        {
            Unsackable = yes;
            if(Unsackable)
            {
                UnsackableTask = new TaskFactory().StartNew(async () =>
                {
                    while (Unsackable)
                    {
                        Manager.SetManagerRating(99);
                        await Task.Delay(60 * 100);
                    }
                });
            }
            else
            {
                UnsackableTask = null;
            }
        }
    }
}
