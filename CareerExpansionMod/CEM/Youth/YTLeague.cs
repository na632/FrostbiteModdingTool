using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

                var s = CEMCore.CEMMyDocumentsDbSaveDirectory + "YouthTeam\\League\\";
                if (!Directory.Exists(s))
                    Directory.CreateDirectory(s);

                return s;

            }
        }

        public string YouthTeamFileNameLocation
        {
            get
            {
                return YouthTeamLeagueDirectory + $"{YTLeagueId}.json";

            }
        }

        public int YTLeagueId { get; set; }

        public string LeagueName { get; set; }

        public int Level { get; set; }

        public int MaxAge { get; set; }

        public int MaxAmountOfOverage { get; set; }

        public string NationId { get; set; }

        public void Save()
        {
            var objSerialized = JsonConvert.SerializeObject(this);
            File.WriteAllText(YouthTeamFileNameLocation, objSerialized);
        }

        public static YTLeague Load(int id)
        {
            return JsonConvert.DeserializeObject<YTLeague>(YouthTeamLeagueDirectory + $"{id}.json");
        }
    }
}
