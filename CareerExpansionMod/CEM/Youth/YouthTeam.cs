using CareerExpansionMod.CEM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using v2k4FIFAModding.Career.CME.FIFA;

namespace v2k4FIFAModdingCL.Career.CME.Youth
{
    public class YouthTeam
    {
        public static string YouthTeamDirectory { get
            {
                var s = CEMCore.CEMMyDocumentsDbSaveDirectory + "YouthTeam\\";
                if (!Directory.Exists(s))
                    Directory.CreateDirectory(s);

                return s;
                
            } }

        public string YouthTeamFileNameLocation
        {
            get
            {
                return YouthTeamDirectory + $"{YouthTeamId}.json";

            }
        }

        /// <summary>
        /// The Youth Team Id for CEM
        /// </summary>
        public int YouthTeamId { get; set; }

        public string TeamName { get; set; }
        /// <summary>
        /// The team id in FIFA
        /// </summary>
        public int ParentTeamId { get; set; }
        /// <summary>
        /// CEM Youth League Id
        /// </summary>
        public int LeagueId { get; set; }
        public int NationId { get; set; }

        public List<FIFAPlayer> Players { get; set; }

        public void Save()
        {
            var raw = JsonConvert.SerializeObject(this);
            File.WriteAllText(YouthTeamFileNameLocation, raw);
        }

        public static YouthTeam Load(int id)
        {
            return JsonConvert.DeserializeObject<YouthTeam>(YouthTeamDirectory + $"{id}.json");
        }

    }
}
