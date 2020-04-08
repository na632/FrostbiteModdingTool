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

        public string GetSponsorImageUrl()
        {
            var dlllocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            //Directory.EnumerateFiles(dlllocation + "Career/CME/Data/")

            var finalValue = Directory.EnumerateFiles(dlllocation + "/wwwroot/images/sponsors/").ToList().FirstOrDefault(x=>x.Contains(SponsorName));
            if (finalValue == null)
            {
                finalValue = dlllocation + "/wwwroot/images/sponsors/noimage.jpg";
            }
            return finalValue;
        }

        public void Save()
        {
            var dlllocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            var datalocation = dlllocation + "\\Data\\CME\\Sponsors\\";
            if (!Directory.Exists(datalocation))
                Directory.CreateDirectory(datalocation);

            var finalLocation = datalocation + SponsorName + ".json";
            File.WriteAllText(finalLocation, JsonConvert.SerializeObject(this));
        }

        public static Sponsor Load(string sponsorName)
        {
            var dlllocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            var datalocation = dlllocation + "\\Data\\CME\\Sponsors\\";
            var finalLocation = datalocation + sponsorName + ".json";
            if (File.Exists(finalLocation))
                return JsonConvert.DeserializeObject<Sponsor>(File.ReadAllText(finalLocation));
            else
                return null;
        }
    }

    public class SponsorsToTeam
    {
        public string SponsorName { get; set; }
        public int TeamId { get; set; }
        public bool IsUserTeam { get; set; }
        public eSponsorType SponsorType { get; set; }

        public static List<SponsorsToTeam> SponsorsToTeams = new List<SponsorsToTeam>();

        public static void Save()
        {
            var dlllocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            var datalocation = dlllocation + "\\Data\\CME\\DB\\";
            if (!Directory.Exists(datalocation))
                Directory.CreateDirectory(datalocation);

            var finalLocation = datalocation + "SponsorsToTeams.json";
            File.WriteAllText(finalLocation, JsonConvert.SerializeObject(SponsorsToTeams));
        }

        public static List<SponsorsToTeam> Load()
        {
            var dlllocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            var datalocation = dlllocation + "\\Data\\CME\\DB\\";
            var finalLocation = datalocation + "SponsorsToTeams.json";
            if (File.Exists(finalLocation))
                SponsorsToTeams = JsonConvert.DeserializeObject<List<SponsorsToTeam>>(File.ReadAllText(finalLocation));

            return SponsorsToTeams;
        }
    }
}
