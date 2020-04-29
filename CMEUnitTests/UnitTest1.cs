using CareerExpansionMod.CEM.FIFA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using v2k4FIFAModding.Career.CME.FIFA;
using CareerExpansionMod.CEM.Finances;
using v2k4FIFAModdingCL.MemHack.Core;
using CareerExpansionMod.CEM.Data;
using CareerExpansionMod.CEM;
using System.Threading;

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
            CareerDB1.FIFAUser = new FIFAUsers();
            CareerDB1.FIFAUser.clubteamid = 1960;
            CoreHack coreHack = new CoreHack();
            coreHack.SaveName = "Test";

            DbDataCache.CreateAllSponsorFiles();

            DbDataCache.SponsorsToTeams.AddRange(
                new List<SponsorsToTeam>() {
                new SponsorsToTeam()
                {
                    TeamId = 1960,
                     IsUserTeam = false,
                      SponsorName = "yobet",
                       SponsorType = eSponsorType.Main,
                        ContractLengthInYears = 12,
                         GameDateStarted = DateTime.Now
                }
                , new SponsorsToTeam()
                {
                    TeamId = 1960,
                    IsUserTeam = false,
                    SponsorName = "lowcostvans",
                    SponsorType = eSponsorType.Travel,
                        ContractLengthInYears = 12,
                         GameDateStarted = DateTime.Now
                }
                , new SponsorsToTeam()
                {
                    TeamId = 1960,
                    IsUserTeam = false,
                    SponsorName = "carling",
                    SponsorType = eSponsorType.Alcohol,
                        ContractLengthInYears = 12,
                         GameDateStarted = DateTime.Now
                }
                , new SponsorsToTeam()
                {
                    TeamId = 1960,
                    IsUserTeam = false,
                    SponsorName = "swanseauniversity",
                    SponsorType = eSponsorType.Training,
                        ContractLengthInYears = 12,
                         GameDateStarted = DateTime.Now
                }
                }
                );

            SponsorsToTeam.SaveAll();
        }
    
    
        [TestMethod]
        public void TestChangeOfSave()
        {
            CEMCore core = new CEMCore();
            for(var i = 0; i < 3; i++)
            {
                var getGameDate = core.CoreHack.GameDate;
                Thread.Sleep(1000);

            }

        }
    }
}
