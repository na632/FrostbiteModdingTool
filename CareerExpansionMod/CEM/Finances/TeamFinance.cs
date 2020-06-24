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
        public List<Debt> Debts { get; set; }
        public List<Loan> Loans { get; set; }
        public Sponsor Sponsor { get; set; }

        public static string CMETeamFinanceDirectory
        {
            get
            {
                return CEMCore.CEMMyDocumentsDbSaveDirectory;

            }
        }

        public static string CMETeamFinanceSaveFile
        {
            get
            {
                return CEMCore.CEMMyDocumentsDbSaveDirectory + "\\TeamFinances.json";
            }
        }

        public static TeamFinance Load()
        {
            if (File.Exists(CMETeamFinanceSaveFile))
                return JsonConvert.DeserializeObject<TeamFinance>(File.ReadAllText(CMETeamFinanceSaveFile));
            else
                return new TeamFinance();
        }

        public void Save()
        {
            File.WriteAllText(CMETeamFinanceSaveFile, JsonConvert.SerializeObject(this));
        }
    }
}
