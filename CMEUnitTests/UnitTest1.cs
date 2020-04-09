using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using v2k4FIFAModding.Career.CME.FIFA;
using v2k4FIFAModding.Career.CME.Finances;

namespace CMEUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LoadFIFALeaguesFromCSV()
        {
            var fifaleagues = FIFALeague.GetFIFALeagues();
        }

        [TestMethod]
        public void SaveSwanseaSponsorsToFile()
        {
            Sponsor sponsorNone = new Sponsor()
            {
                SponsorType = eSponsorType.General,
                SponsorLevels = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },
                SponsorName = "none",
                SponsorPayoutPerYearMax = 0
            };
            sponsorNone.Save();

            Sponsor sponsorYobet = new Sponsor()
            {
                SponsorType = eSponsorType.Main,
                SponsorLevels = new List<int>() { 2, 3 },
                SponsorName = "yobet",
                SponsorPayoutPerYearMax = 1000000
            };
            sponsorYobet.Save();

            Sponsor sponsorLCV = new Sponsor()
            {
                SponsorType = eSponsorType.Travel,
                SponsorLevels = new List<int>() { 3 },
                SponsorName = "lowcostvans",
                SponsorPayoutPerYearMax = 500000
            };
            sponsorLCV.Save();

            Sponsor sponsorKit = new Sponsor()
            {
                SponsorType = eSponsorType.Kit,
                SponsorLevels = new List<int>() { 2,3,4 },
                SponsorName = "joma",
                SponsorPayoutPerYearMax = 750000
            };
            sponsorKit.Save();

            Sponsor sponsorDrink = new Sponsor()
            {
                SponsorType = eSponsorType.Alcohol,
                SponsorLevels = new List<int>() { 1, 2, 3, 4 },
                SponsorName = "carling",
                SponsorPayoutPerYearMax = 750000
            };
            sponsorDrink.Save();

            Sponsor sponsorTraining = new Sponsor()
            {
                SponsorType = eSponsorType.Training,
                SponsorLevels = new List<int>() { 1, 2, 3, 4 },
                SponsorName = "swanseauniversity",
                SponsorPayoutPerYearMax = 250000
            };
            sponsorTraining.Save();

            SponsorsToTeam.SponsorsToTeams.AddRange(
                new List<SponsorsToTeam>() {
                new SponsorsToTeam()
                {
                    TeamId = 1960,
                     IsUserTeam = false,
                      SponsorName = "yobet",
                       SponsorType = eSponsorType.Main
                }
                , new SponsorsToTeam()
                {
                    TeamId = 1960,
                    IsUserTeam = false,
                    SponsorName = "lowcostvans",
                    SponsorType = eSponsorType.Travel
                }
                , new SponsorsToTeam()
                {
                    TeamId = 1960,
                    IsUserTeam = false,
                    SponsorName = "carling",
                    SponsorType = eSponsorType.Alcohol
                }
                , new SponsorsToTeam()
                {
                    TeamId = 1960,
                    IsUserTeam = false,
                    SponsorName = "swanseauniversity",
                    SponsorType = eSponsorType.Training
                }
                }
                );

            SponsorsToTeam.Save();
        }
    }
}
