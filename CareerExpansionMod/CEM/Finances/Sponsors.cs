using CareerExpansionMod.CEM.Data;
using CareerExpansionMod.CEM.FIFA;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModding.Career.CME.FIFA;

namespace CareerExpansionMod.CEM.Finances
{
    public enum eSponsorType : int
    {
        General,
        Main,
        Kit,
        Alcohol,
        Drinks,
        Food,
        Travel,
        Gym,
        Training,
        Legal,
        Nutritional,
        Hospitality
    }

    public class Sponsor
    {
        public string SponsorName { get; set; }

        public bool[] SponsorLevels = new bool[10];

        public eSponsorType SponsorType { get; set; }

        public double SponsorPayoutPerYearMax { get; set; }


        public int? SpecificTeamId { get; set; }

        public static string CMESponsorDirectory
        { 
            get
            {
                var datalocation = CEMCore.CEMMyDocumentsDirectory + "\\Data\\CEM\\Sponsors\\";
                Directory.CreateDirectory(datalocation);
                return datalocation;

            } 
        }

        public string GetSponsorImageUrl()
        {
            var dlllocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            //Directory.EnumerateFiles(dlllocation + "Career/CME/Data/")

            var finalValue = Directory.EnumerateFiles(dlllocation + "/wwwroot/images/sponsors/").ToList().FirstOrDefault(x=>x.Contains(SponsorName.ToLower()));
            if (finalValue == null)
            {
                finalValue = "/images/sponsors/noimage.jpg";
            }
            else
            {
                finalValue = finalValue.Split("wwwroot/")[1];
            }
            return finalValue;
        }

        public void Save(bool saveToFile = true)
        {
            var finalLocation = CMESponsorDirectory + SponsorName + ".json";
            DbDataCache.Sponsors.RemoveAll(x => x.SponsorName == SponsorName);
            DbDataCache.Sponsors.Add(this);
            if(saveToFile)
                File.WriteAllText(finalLocation, JsonConvert.SerializeObject(this));
        }

        public static Sponsor Load(string sponsorName)
        {
            var existingRecord = DbDataCache.Sponsors.Find(x => x.SponsorName == sponsorName);
            if (existingRecord != null)
                return existingRecord;

            var finalLocation = CMESponsorDirectory + sponsorName + ".json";
            if (File.Exists(finalLocation))
            {
                var fileRecord = JsonConvert.DeserializeObject<Sponsor>(File.ReadAllText(finalLocation));
                DbDataCache.Sponsors.Add(fileRecord);
                return fileRecord;
            }
            else
                return null;
        }

        public static IEnumerable<Sponsor> LoadAll()
        {
            var datalocation = CMESponsorDirectory;
            var files = Directory.EnumerateFiles(datalocation);

            if (DbDataCache.Sponsors.Count == 0)
            {
                foreach (var f in files)
                {
                    DbDataCache.Sponsors.Add(JsonConvert.DeserializeObject<Sponsor>(File.ReadAllText(f)));
                }
            }
            return DbDataCache.Sponsors;
        }
    }


    public class SponsorsToTeam
    {
        public string SponsorName { get; set; }
        public int TeamId { get; set; }
        public bool IsUserTeam { get; set; }
        public eSponsorType SponsorType { get; set; }
        public int ContractLengthInYears { get; set; }
        public DateTime GameDateStarted { get; set; }
        public int PayoutPerYear { get; set; }
        public int Confidence { get; set; }

        public Sponsor GetSponsor()
        {
            return Sponsor.Load(SponsorName);
        }

        public static string CMESponsorsToTeamDirectory
        {
            get
            {
                return CEMCore.CEMMyDocumentsDbSaveDirectory;

            }
        }


        public static void Save()
        {
            var finalLocation = CMESponsorsToTeamDirectory + "SponsorsToTeams.json";
            File.WriteAllText(finalLocation, JsonConvert.SerializeObject(DbDataCache.SponsorsToTeams));
        }

        public static List<SponsorsToTeam> Load()
        {
            var finalLocation = CMESponsorsToTeamDirectory + "SponsorsToTeams.json";
            if (File.Exists(finalLocation) && DbDataCache.SponsorsToTeams.Count == 0)
                DbDataCache.SponsorsToTeams = JsonConvert.DeserializeObject<List<SponsorsToTeam>>(File.ReadAllText(finalLocation));

            return DbDataCache.SponsorsToTeams;
        }

        public static List<SponsorsToTeam> LoadSponsorsForTeam(int teamId)
        {
            return Load().Where(x => x.TeamId == teamId).ToList();
        }

        public static int GetCalculatedContractLengthInMonths(int teamid, string sponsor)
        {
            return 12;
        }

        public static int GetCalculatedContractLengthInYears(int teamid, string sponsor)
        {
            var calcMonths = GetCalculatedContractLengthInMonths(teamid, sponsor);
            return calcMonths > 0 ? Convert.ToInt32(Math.Ceiling((double)calcMonths / 12.0)) : 1;
        }

        public static int GetCaclulatedPayoutAmountPerYear(int teamid, string sponsorName)
        {
            var sponsor = Sponsor.Load(sponsorName);
            if (sponsor != null && CareerDB2.Current.ParentDataSet != null) 
            {
                var team = CareerDB1.UserTeam;
                if (team != null)
                {
                    var prestige = Convert.ToInt32(team.domesticprestige);
                    var leagueRank = (from ltl in CareerDB2.Current.ParentDataSet.Tables["leagueteamlinks"].AsEnumerable()
                                      join l in CareerDB2.Current.ParentDataSet.Tables["leagues"].AsEnumerable() on ltl.Field<int>("leagueid") equals l.Field<int>("leagueid")
                                      where Convert.ToInt32(ltl["teamid"]) == teamid
                                      select l.Field<int>("level")).FirstOrDefault();
                    var rankRatio = Convert.ToDouble(11 - leagueRank);
                    if (team != null)
                    {
                        return Convert.ToInt32(Math.Ceiling(sponsor.SponsorPayoutPerYearMax * (rankRatio * 0.1d) * (prestige * 0.1)));
                    }
                }
            }

            return 0;
        }
    }
}
