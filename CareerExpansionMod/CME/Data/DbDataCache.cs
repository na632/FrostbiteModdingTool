using CareerExpansionMod.CME.Finances;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CareerExpansionMod.CME.Data
{
    public static class DbDataCache
    {
        public static List<Sponsor> Sponsors = new List<Sponsor>();
        public static List<SponsorsToTeam> SponsorsToTeams = new List<SponsorsToTeam>();

        public static void CreateAllSponsorFiles()
        {
            Sponsor sponsorNone = new Sponsor()
            {
                SponsorType = eSponsorType.General,
                SponsorLevels = new List<bool>() { true, true, true, true, true, true, true, true, true, true }.ToArray(),
                SponsorName = "none",
                SponsorPayoutPerYearMax = 0
            };
            sponsorNone.Save();

            Sponsor sponsorYobet = new Sponsor()
            {
                SponsorType = eSponsorType.Main,
                SponsorLevels = new List<bool>() { false, true, true, true, true, true, true, true, true, true }.ToArray(),
                SponsorName = "yobet",
                SponsorPayoutPerYearMax = 1000000
            };
            sponsorYobet.Save();

            Sponsor sponsorLCV = new Sponsor()
            {
                SponsorType = eSponsorType.Travel,
                SponsorLevels = new List<bool>() { false, false, true, true, true, true, true, true, true, true }.ToArray(),
                SponsorName = "lowcostvans",
                SponsorPayoutPerYearMax = 500000
            };
            sponsorLCV.Save();

            CreateKitSponsorFiles();

            Sponsor sponsorDrink = new Sponsor()
            {
                SponsorType = eSponsorType.Alcohol,
                SponsorLevels = new List<bool>() { true, true, true, true, true, true, true, true, true, true }.ToArray(),
                SponsorName = "carling",
                SponsorPayoutPerYearMax = 750000
            };
            sponsorDrink.Save();

            Sponsor sponsorTraining = new Sponsor()
            {
                SponsorType = eSponsorType.Training,
                SponsorLevels = new List<bool>() { true, true, true, true, true, true, true, true, true, true }.ToArray(),
                SponsorName = "swanseauniversity",
                SponsorPayoutPerYearMax = 250000,
                SpecificTeamId = 1960
            };
            sponsorTraining.Save();
        }

        static void CreateKitSponsorFiles()
        {
           

            // Adidas
            Sponsor sponsorKitAdidas = new Sponsor()
            {
                SponsorType = eSponsorType.Kit,
                SponsorLevels = new List<bool>() { true, true, true, true, true, true, true, true, true, true }.ToArray(),
                SponsorName = "Adidas",
                SponsorPayoutPerYearMax = 5000000
            };
            sponsorKitAdidas.Save();

            // Emirates
            Sponsor sponsorKitEmirates = new Sponsor()
            {
                SponsorType = eSponsorType.Kit,
                SponsorLevels = new List<bool>() { true, true, false, false, false, false, false, false, false, false }.ToArray(),
                SponsorName = "Emirates",
                SponsorPayoutPerYearMax = 5000000
            };
            sponsorKitEmirates.Save();

            new Sponsor()
            {
                SponsorType = eSponsorType.Kit,
                SponsorLevels = new List<bool>() { true, true, false, false, false, false, false, false, false, false }.ToArray(),
                SponsorName = "Etihad",
                SponsorPayoutPerYearMax = 5000000
            }.Save();

            new Sponsor()
            {
                SponsorType = eSponsorType.Kit,
                SponsorLevels = new List<bool>() { true, true, false, false, false, false, false, false, false, false }.ToArray(),
                SponsorName = "JDSports",
                SponsorPayoutPerYearMax = 5000000
            }.Save();

            new Sponsor()
            {
                SponsorType = eSponsorType.Kit,
                SponsorLevels = new List<bool>() { true, true, false, false, false, false, false, false, false, false }.ToArray(),
                SponsorName = "JDSports",
                SponsorPayoutPerYearMax = 5000000
            }.Save();

            new Sponsor()
            {
                SponsorType = eSponsorType.Kit,
                SponsorLevels = new List<bool>() { false, true, true, true, true, true, true, true, true, true }.ToArray(),
                SponsorName = "Joma",
                SponsorPayoutPerYearMax = 750000
            }.Save();

            new Sponsor()
            {
                SponsorType = eSponsorType.Kit,
                SponsorLevels = new List<bool>() { true, true, true, true, true, true, true, true, true, true }.ToArray(),
                SponsorName = "Nike",
                SponsorPayoutPerYearMax = 5000000
            }.Save();

        }
    }
}
