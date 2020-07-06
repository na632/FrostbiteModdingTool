using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using v2k4FIFAModdingCL.MemHack.Career;

namespace CareerExpansionMod.CEM.Finances
{
    public class TeamFinance
    {
        public static TeamFinance TeamFinanceInstance;
        public List<Debt> Debts { get; set; }
        public List<Loan> Loans { get; set; }

        public List<Payment> Payments { get; set; }

        public static string CEMTeamFinanceDirectory
        {
            get
            {
                return CEMCore.CEMMyDocumentsDbSaveDirectory;

            }
        }

        public static string CEMTeamFinanceSaveFile
        {
            get
            {
                return CEMCore.CEMMyDocumentsDbSaveDirectory + "\\TeamFinances.json";
            }
        }

        public static TeamFinance Load()
        {
            if (File.Exists(CEMTeamFinanceSaveFile))
            {
                TeamFinanceInstance = JsonConvert.DeserializeObject<TeamFinance>(File.ReadAllText(CEMTeamFinanceSaveFile));
            }
            else
            {
                TeamFinanceInstance = new TeamFinance() { Debts = new List<Debt>(), Loans = new List<Loan>(), Payments = new List<Payment>() };
            }

            return TeamFinanceInstance;
        }

        public void Save()
        {
            TeamFinanceInstance = this;
            lock (TeamFinanceInstance)
            {
                //using (FileStream fileStream = new FileStream(CMETeamFinanceSaveFile, FileMode.OpenOrCreate))
                //{
                //    fileStream.Write()
                //}
                //    Stream s = this.GetStream();
                //IAsyncResult ar = s.BeginWrite(data, 0, data.Length, SendAsync, state);
                //if (!ar.IsCompleted)
                //    ar.AsyncWaitHandle.WaitOne();

                File.WriteAllText(CEMTeamFinanceSaveFile, JsonConvert.SerializeObject(TeamFinanceInstance));
            }
        }
    }
}
