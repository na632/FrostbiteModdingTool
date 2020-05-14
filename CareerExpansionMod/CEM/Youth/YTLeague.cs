using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareerExpansionMod.CEM.Youth
{
    public class YTLeague
    {
        public static string YouthTeamLeagueDirectory
        {
            get
            {
                return CEMCore.CEMMyDocumentsDbSaveDirectory + "YouthTeam\\League\\";

            }
        }

        public string YouthTeamFileNameLocation
        {
            get
            {
                return YouthTeamLeagueDirectory + $"{YTLeagueId}-{LeagueName}.json";

            }
        }

        public int YTLeagueId { get; set; }

        public string LeagueName { get; set; }

        public int Level { get; set; }

        public int MaxAge { get; set; }

        public int MaxAmountOfOverage { get; set; }
    }
}
