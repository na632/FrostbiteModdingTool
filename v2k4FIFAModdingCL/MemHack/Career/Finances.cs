using Memory;
using System;
using System.Collections.Generic;
using System.Text;
using v2k4FIFAModdingCL.MemHack.Core;

namespace v2k4FIFAModdingCL.MemHack.Career
{
    

    public class Finances : IDisposable
    {

        private class POINTER_ADDRESSES
        {
            /// Will need to redo this for every EA Update
            public static string TRANSFER_BUDGET = "FIFA20.exe+072E6980,0x18,0x18,0x2A8,0x268,0x8";
            /// Will need to redo this for every EA Update
            public static string STARTING_BUDGET = "FIFA20.exe+06E2D988,0x538,0x0,0x20,0x238,0x5E0";
        }

        

        public int TransferBudget
        {
            get
            {
                if (CoreHack.GetProcess(out Mem MemLib).HasValue) { 
                    var transferbudget = MemLib.readInt(POINTER_ADDRESSES.TRANSFER_BUDGET);
                    CoreHack.GameSaveStateInstance.Finances = this;
                    return transferbudget;
                }
                return -1;
            }
            set
            {

                if (CoreHack.GetProcess(out Mem MemLib).HasValue)
                {
                    MemLib.writeMemory(POINTER_ADDRESSES.TRANSFER_BUDGET, "int", value.ToString());
                    CoreHack.GameSaveStateInstance.Finances = this;
                }

            }
        }

        public int StartingBudget
        {
            get
            {
                if (CoreHack.GetProcess(out Mem MemLib).HasValue)
                {
                    var budget = MemLib.readInt(POINTER_ADDRESSES.STARTING_BUDGET);
                    CoreHack.GameSaveStateInstance.Finances = this;
                    return budget;
                }
                return -1;
            }
        }

        public bool RequestAdditionalFunds(out string message)
        {
            bool success = false;
            if (TransferBudget < StartingBudget)
            {
                message = "yep";
                SetTransferBudget(TransferBudget + (StartingBudget - TransferBudget));
                success = true;
            }
            else
            {
                message = "nope";
                success = false;
            }

            CoreHack.GameSaveStateInstance.Finances = this;
            return success;
        }

        public static int GetTransferBudget()
        {
            return new Finances().TransferBudget;
        }

        public static void SetTransferBudget(int newBudget)
        {
            new Finances().TransferBudget = newBudget;
        }

        public void Dispose()
        {
            //~Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
