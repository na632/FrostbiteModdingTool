using Memory;
using System;
using System.Collections.Generic;
using System.Text;
using v2k4FIFAModdingCL.MemHack.Core;

namespace v2k4FIFAModdingCL.MemHack.Career
{
    

    public class Finances : IDisposable
    {

        private static class POINTER_ADDRESSES
        {
            /// Will need to redo this for every EA Update
            public static string TRANSFER_BUDGET = "FIFA20.exe+072E7900,0x10,0x18,0x2B0,0x8,0x8";
            /// Will need to redo this for every EA Update
            public static string STARTING_BUDGET = "FIFA20.exe+06E2B010,0x10,0x48,0x30,0x58,0x5E0";
        }

        private static class AOB_ADDRESSES
        {
            // DOESN'T WORK!
            public static string TRANSFER_BUDGET = "?? D9 39 01 70 A3 23 00 AC FC 28 01 78 29 20 69 E9 1A 02 00 8D 16 F4 00";
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

        private static Finances _financeInstance;
        public static Finances FinanceInstance
        {
            get
            {
                if (_financeInstance == null)
                    _financeInstance = new Finances();

                return _financeInstance;
            }
        }

        public static int GetTransferBudget()
        {
            return FinanceInstance.TransferBudget;
        }

        public static void SetTransferBudget(int newBudget)
        {
            FinanceInstance.TransferBudget = newBudget;
        }

        public void Dispose()
        {
            //~Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Finances()
        {

        }
    }
}
