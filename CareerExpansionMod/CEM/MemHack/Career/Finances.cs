using CareerExpansionMod.CEM;
using Memory;
using System;
using System.Collections.Generic;
using System.Text;
using v2k4FIFAModdingCL.MemHack.Core;

namespace v2k4FIFAModdingCL.MemHack.Career
{
    

    public class Finances : IDisposable
    {

        private static class AOB_ADDRESSES
        {
            // DOESN'T WORK!
            public static string TRANSFER_BUDGET = "?? D9 39 01 70 A3 23 00 AC FC 28 01 78 29 20 69 E9 1A 02 00 8D 16 F4 00";
        }

        //private static int? PreviousInGameTransferBudget;

        private static CoreHack.POINTER_ADDRESSES _ADDRESSES;

        public static CoreHack.POINTER_ADDRESSES POINTER_ADDRESSES
        {
            get {
                if (_ADDRESSES == null)
                    _ADDRESSES = CoreHack.POINTER_ADDRESSES.LoadPointerAddresses();

                return _ADDRESSES; 
            }
            set { _ADDRESSES = value; }
        }

        /// <summary>
        /// Actual transfer budget
        /// </summary>
        public static int TransferBudget
        {
            get
            {
                if (CoreHack.GetProcess().HasValue) { 
                    var transferbudget = CoreHack.MemLib.readInt(POINTER_ADDRESSES.FinancePointers["FINANCES_TRANSFER_BUDGET"]);
                    return transferbudget;
                }
                return -1;
            }
            set
            {

                if (CoreHack.GetProcess().HasValue)
                {
                    CoreHack.MemLib.writeMemory(POINTER_ADDRESSES.FinancePointers["FINANCES_TRANSFER_BUDGET"], "int", value.ToString());
                }

            }
        }

        /// <summary>
        /// Starting budget / CEM Balance
        /// </summary>
        public static int StartingBudget
        {
            get
            {
                if (CoreHack.GetProcess().HasValue)
                {
                    var budget = CoreHack.MemLib.readInt(POINTER_ADDRESSES.FinancePointers["FINANCES_STARTING_BUDGET"]);
                    return budget;
                }
                return -1;
            }
            set
            {

                if (CoreHack.GetProcess().HasValue)
                {
                    CoreHack.MemLib.writeMemory(POINTER_ADDRESSES.FinancePointers["FINANCES_STARTING_BUDGET"], "int", value.ToString());
                }

            }
        }

        public static int TeamId
        {
            get
            {
                if (CoreHack.GetProcess().HasValue)
                {
                    var budget = CoreHack.MemLib.readInt(POINTER_ADDRESSES.FinancePointers["FINANCES_TEAMID"]);
                    return budget;
                }
                return -1;
            }
        }

        public bool RequestAdditionalFunds(out string message)
        {
            bool success = false;
            var ratio = ((TransferBudget > 0 ? (double)TransferBudget : 1) / (StartingBudget > 0 ? (double)StartingBudget : 1));
            ratio = Math.Round(ratio, 2);
            var desiredRatioOfClub = 0.85;
            if (ratio < desiredRatioOfClub)
            {
                message = "Board: OK. We have provided you with some extra funds.";
                SetTransferBudget(Convert.ToInt32(Math.Round(TransferBudget + ((StartingBudget - TransferBudget) * desiredRatioOfClub))));
                success = true;
            }
            else
            {
                message = "<p>Board: Sorry we are unable to provide you with additional funds at this time.<br /> You currently have a ratio of " + (ratio*100).ToString() + "%.</p>";
                success = false;
            }

            CEMCore.CEMCoreInstance.Finances = this;
            return success;
        }

        public static int GetTransferBudget()
        {
            return TransferBudget;
        }

        public static void SetTransferBudget(int newBudget)
        {
            TransferBudget = newBudget;
        }

        public static int GetBalance()
        {
            return StartingBudget;
        }

        public static void SetBalance(int newBalance)
        {
            StartingBudget = newBalance;
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
