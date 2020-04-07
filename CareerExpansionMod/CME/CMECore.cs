using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using v2k4FIFAModding.Career.CME.FIFA;
using v2k4FIFAModdingCL.MemHack.Career;
using v2k4FIFAModdingCL.MemHack.Core;

namespace CareerExpansionMod.CME
{
    public class CMECore
    {
        public static CMECore CMECoreInstance;
        public CMECore()
        {

            // Initialize FIFA Leagues CSV to Enumerable
            FIFALeague.GetFIFALeagues();


            // Setup Singleton
            CMECoreInstance = this;
            CMECoreInstance.CoreHack.EventGameDateChanged += CoreHack_EventGameDateChanged;
        }

        public void CoreHack_EventGameDateChanged(DateTime oldDate, DateTime newDate)
        {
            GameDateChanged(oldDate, newDate);
        }

        public CoreHack CoreHack = new CoreHack();
        public Finances Finances = new Finances();
        public Manager Manager = new Manager();

        internal void GameDateChanged(DateTime oldDate, DateTime newDate)
        {
            // Do day on day processing


            // Do a Save of the Stuff thats needed
            var myDocuments = Microsoft.VisualBasic.FileIO.SpecialDirectories.MyDocuments + "\\FIFA 20\\settings\\";
            //File.Copy( , "");

            // Refresh
            Finances.GetTransferBudget();

        }
    }
}
