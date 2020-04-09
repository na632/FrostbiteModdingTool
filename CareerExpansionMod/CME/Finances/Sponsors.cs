using CareerExpansionMod.CME;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace v2k4FIFAModding.Career.CME.Finances
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

        public List<int> SponsorLevels { get; set; }

        public eSponsorType SponsorType { get; set; }

        public decimal SponsorPayoutPerYearMax { get; set; }

        public static string CMESponsorDirectory
        { 
            get
            {
                var datalocation = CMECore.CMEMyDocumentsDirectory + "\\Data\\CME\\Sponsors\\";
                Directory.CreateDirectory(datalocation);
                return datalocation;

            } 
        }

        public string GetSponsorImageUrl()
        {
            var dlllocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            //Directory.EnumerateFiles(dlllocation + "Career/CME/Data/")

            var finalValue = Directory.EnumerateFiles(dlllocation + "/wwwroot/images/sponsors/").ToList().FirstOrDefault(x=>x.Contains(SponsorName));
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

        public void Save()
        {
            var finalLocation = CMESponsorDirectory + SponsorName + ".json";
            File.WriteAllText(finalLocation, JsonConvert.SerializeObject(this));
        }

        public static Sponsor Load(string sponsorName)
        {
            var finalLocation = CMESponsorDirectory + sponsorName + ".json";
            if (File.Exists(finalLocation))
                return JsonConvert.DeserializeObject<Sponsor>(File.ReadAllText(finalLocation));
            else
                return null;
        }

        public static IEnumerable<Sponsor> LoadAll()
        {
            var datalocation = CMESponsorDirectory;
            var files = Directory.EnumerateFiles(datalocation);

            foreach(var f in files)
            {
                yield return JsonConvert.DeserializeObject<Sponsor>(File.ReadAllText(f));
            }
        }
    }

    public class SponsorsToTeam
    {
        public string SponsorName { get; set; }
        public int TeamId { get; set; }
        public bool IsUserTeam { get; set; }
        public eSponsorType SponsorType { get; set; }

        public static string CMESponsorsToTeamDirectory
        {
            get
            {
                var datalocation = CMECore.CMEMyDocumentsDirectory + "\\Data\\CME\\DB\\";
                Directory.CreateDirectory(datalocation);
                return datalocation;

            }
        }

        public static List<SponsorsToTeam> SponsorsToTeams = new List<SponsorsToTeam>();

        public static void Save()
        {
            var finalLocation = CMESponsorsToTeamDirectory + "SponsorsToTeams.json";
            File.WriteAllText(finalLocation, JsonConvert.SerializeObject(SponsorsToTeams));
        }

        public static List<SponsorsToTeam> Load()
        {
            var finalLocation = CMESponsorsToTeamDirectory + "SponsorsToTeams.json";
            if (File.Exists(finalLocation))
                SponsorsToTeams = JsonConvert.DeserializeObject<List<SponsorsToTeam>>(File.ReadAllText(finalLocation));

            return SponsorsToTeams;
        }

        public static List<SponsorsToTeam> LoadSponsorsForTeam(int teamId)
        {
            return Load().Where(x => x.TeamId == teamId).ToList();
        }
    }
}
