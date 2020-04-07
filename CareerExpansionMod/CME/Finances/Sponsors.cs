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
        public int SponsorId { get; set; }

        public string SponsorName { get; set; }

        public List<int> SponsorLevels { get; set; }

        public eSponsorType SponsorType { get; set; }

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
    }

    public class SponsorsToTeam
    {
        public int SponsorId { get; set; }
        public int TeamId { get; set; }
        public bool IsUserTeam { get; set; }
        public eSponsorType SponsorType { get; set; }

    }
}
