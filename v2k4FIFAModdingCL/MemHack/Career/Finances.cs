using Memory;
using System;
using System.Collections.Generic;
using System.Text;
using v2k4FIFAModdingCL.MemHack.Core;

namespace v2k4FIFAModdingCL.MemHack.Career
{
    

    public class Finances
    {

        //FIFA20.exe+10DA094C - 44 8B 56 08          

        private class POINTER_ADDRESSES
        {
            public string TRANSFER_BUDGET = "FIFA20.exe+072BC110,0x18,0x18,0x2A8,0x268,0x8";
            public string STARTING_BUDGET = "FIFA20.exe+06E01930,0x10,0x48,0x30,0x58,0x5E0";
        }

        public int TransferBudget
        {
            get
            {
                if (CoreHack.GetProcess(out Mem MemLib).HasValue) { 
                    var transferbudget = MemLib.readInt(new POINTER_ADDRESSES().TRANSFER_BUDGET);
                    return transferbudget;
                }
                return 0;
            }
            set
            {

                if (CoreHack.GetProcess(out Mem MemLib).HasValue)
                {
                    MemLib.writeMemory(new POINTER_ADDRESSES().TRANSFER_BUDGET, "int", value.ToString());
                }

            }
        }

        public int StartingBudget
        {
            get
            {
                if (CoreHack.GetProcess(out Mem MemLib).HasValue)
                {
                    var budget = MemLib.readInt(new POINTER_ADDRESSES().STARTING_BUDGET);
                    return budget;
                }
                return 0;
            }
        }

        public bool RequestAdditionalFunds(out string message)
        {
            if (TransferBudget < StartingBudget)
            {
                message = "yep";
                SetTransferBudget(TransferBudget + (StartingBudget - TransferBudget));
                return true;
            }
            else
            {
                message = "nope";
                return false;
            }
        }

        public static int GetTransferBudget()
        {
            return new Finances().TransferBudget;
        }

        public static void SetTransferBudget(int newBudget)
        {
            new Finances().TransferBudget = newBudget;
        }
    }
}
