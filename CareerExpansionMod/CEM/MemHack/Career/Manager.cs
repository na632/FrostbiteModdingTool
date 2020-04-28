using Memory;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using v2k4FIFAModdingCL.MemHack.Core;

namespace v2k4FIFAModdingCL.MemHack.Career
{
    public class Manager
    {
        private class POINTER_ADDRESSES
        {
            public string MANAGER_RATING = "FIFA20.exe+05712C7C,0x38,0x10,0x8,0x5F8";
        }

        public int ManagerRating
        {
            get
            {
                if (CoreHack.GetProcess().HasValue)
                {
                    var transferbudget = CoreHack.MemLib.readInt(new POINTER_ADDRESSES().MANAGER_RATING);
                    return transferbudget;
                }
                return 0;
            }
            set
            {

                if (CoreHack.GetProcess().HasValue)
                {
                    CoreHack.MemLib.writeMemory(new POINTER_ADDRESSES().MANAGER_RATING, "int", value.ToString());
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
        public static Thread UnsackableThread;

        public static void MakeMeUnsackable(bool yes)
        {
            Unsackable = yes;
            if(Unsackable)
            {
                UnsackableThread = new Thread(() =>
                {
                    while (Unsackable)
                    {
                        Manager.SetManagerRating(99);
                        Thread.Sleep(10000);
                    }
                });
            }
            else
            {

            }
        }
    }
}
