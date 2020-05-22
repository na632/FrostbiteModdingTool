using CareerExpansionMod.CEM.Data;
using CareerExpansionMod.CEM.FIFA;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
        [Display(Name = "Kit Sleeve")]
        Kit_Sleeve,
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

    public static class SponsorFactory 
    {
        public static Sponsor CreateSponsor(string name)
        {
            return new Sponsor(name);
        }
    }

    public class Sponsor
    {
        public Sponsor()
        {

        }

        public bool AllowOverwrite = true;

        public Sponsor(string name)
        {
            SponsorName = name;
            var finalLocation = CEMSponsorDirectory + SponsorName + ".json";
            if (File.Exists(finalLocation))
            {
                AllowOverwrite = false;
                return;
            }

        }

        public string SponsorName { get; set; }

        public int[] SponsorLevels = new int[10];

        public eSponsorType SponsorType { get; set; }

        public double SponsorPayoutPerYearMax { get; set; }

        /// <summary>
        /// If NULL or Empty searches the CEM Directory of images
        /// </summary>
        [Display(Name = "Image Url - Leave blank to use internal image")]
        public string SponsorImageUrl { get; set; }

        public int? SpecificTeamId { get; set; }

        public string ParentSponsorName { get; set; }

        public static string CEMSponsorDirectory
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
            // External Link
            if (!string.IsNullOrEmpty(SponsorImageUrl))
                return SponsorImageUrl;


            // Internal file
            var dlllocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);

            var allFiles = Directory.EnumerateFiles(dlllocation + "/wwwroot/images/sponsors/");
            var finalValue = allFiles
                .FirstOrDefault(x=>
                    x.ToLower().Contains(SponsorName.ToLower())
                    || x.ToLower().Replace("_", " ").Contains(SponsorName.ToLower()));
            if (finalValue == null && string.IsNullOrEmpty(ParentSponsorName))
            {
                finalValue = "/images/sponsors/none.jtif";
            }
            else if(!string.IsNullOrEmpty(ParentSponsorName))
            {
                finalValue = Sponsor.Load(ParentSponsorName).GetSponsorImageUrl();
            }
            else
            {
                finalValue = finalValue.Split("wwwroot/")[1];
            }
            return finalValue;
        }

        public void Save(bool saveToFile = true)
        {
            if (AllowOverwrite)
            {
                var finalLocation = CEMSponsorDirectory + SponsorName + ".json";
                DbDataCache.Sponsors.RemoveAll(x => x.SponsorName == SponsorName);
                DbDataCache.Sponsors.Add(this);
                if (saveToFile)
                {
                    File.WriteAllText(finalLocation, JsonConvert.SerializeObject(this));

                    if (File.Exists(CEMCore.CEMInternalDataDirectory + "BaseData\\Sponsors.zip"))
                        File.Delete(CEMCore.CEMInternalDataDirectory + "BaseData\\Sponsors.zip");

                    ZipFile.CreateFromDirectory(Sponsor.CEMSponsorDirectory, CEMCore.CEMInternalDataDirectory + "BaseData\\Sponsors.zip", CompressionLevel.Optimal, false);
                }
            }
            else
            {
                Debug.WriteLine("We haven't overwritten " + SponsorName);
                Trace.WriteLine("We haven't overwritten " + SponsorName);
            }
        }

        public static Sponsor Load(string sponsorName)
        {
            var existingRecord = DbDataCache.Sponsors.Find(x => x.SponsorName == sponsorName);
            if (existingRecord != null)
                return existingRecord;

            var finalLocation = CEMSponsorDirectory + sponsorName + ".json";
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
            var datalocation = CEMSponsorDirectory;
            var files = Directory.EnumerateFiles(datalocation);

            DbDataCache.Sponsors = new List<Sponsor>();
            //if (DbDataCache.Sponsors.Count == 0)
            //{
            foreach (var f in files)
            {
                DbDataCache.Sponsors.Add(JsonConvert.DeserializeObject<Sponsor>(File.ReadAllText(f)));
            }
            //}
            return DbDataCache.Sponsors;
        }

        public void Delete()
        {
            var finalLocation = CEMSponsorDirectory + SponsorName + ".json";
            DbDataCache.Sponsors.RemoveAll(x => x.SponsorName == SponsorName);
            File.Delete(finalLocation);
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
        public double PayoutPerYear { get; set; }
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

        public void Save()
        {
            DbDataCache.SponsorsToTeams.Add(this);
            SaveAll();
        }


        public static void SaveAll()
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

        public static List<Sponsor> GetAvailableSponsorsForTeamAndType(int teamid, eSponsorType type)
        {
            if (CareerDB1.UserTeam != null && CareerDB2.Current.ParentDataSet != null)
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

                    var leagueRankSponsors = Sponsor.LoadAll().Where(x => x.SponsorType == type && x.SponsorLevels[leagueRank] == 1);
                    return leagueRankSponsors.ToList();
                    
                }
            }
            throw new NotImplementedException();
        }

        public static int GetCalculatedContractLengthInMonths(int teamid, string sponsorName)
        {
            Sponsor s = Sponsor.Load(sponsorName);
            s.SponsorPayoutPerYearMax = s.SponsorPayoutPerYearMax > 0 ? s.SponsorPayoutPerYearMax : 5000000;
            var maxCalcValue = 50000000d;
            switch(s.SponsorType)
            {
                case eSponsorType.Kit:
                    maxCalcValue = 25000000d;
                    break;
                case eSponsorType.Training:
                    maxCalcValue = 10000000d;
                    break;
            }

            var firstCalc = Math.Round(maxCalcValue / Math.Max(10000000, Math.Min(s.SponsorPayoutPerYearMax, 25000000d)));
            var months = Math.Min(12 * 5, Math.Max(12, Convert.ToInt32(Math.Round(firstCalc * 12))));
            
            return months;
        }

        public static int GetCalculatedContractLengthInYears(int teamid, string sponsor)
        {
            var calcMonths = GetCalculatedContractLengthInMonths(teamid, sponsor);
            return calcMonths > 0 ? Convert.ToInt32(Math.Ceiling((double)calcMonths / 12.0)) : 1;
        }

        public static int GetCaclulatedPayoutAmountPerYear(int teamid, string sponsorName)
        {
            var sponsor = Sponsor.Load(sponsorName);
            if (sponsor != null && CareerDB2.Current != null && CareerDB2.Current.ParentDataSet != null) 
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
