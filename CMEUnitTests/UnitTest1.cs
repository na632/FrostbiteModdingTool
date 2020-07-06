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
        public void LoadFIFAGameDBPointers()
        {
            var proc = CoreHack.GetProcess();
            if(proc.HasValue)
            {

                var codeGameDB_addy = CoreHack.MemLib.AoBScan("44 8B 7A 34 44 8B 62 38", true, true).Result;
                //if(codeGameDB_addy > 0)
                //{
                //}


            }

    //        var codeGameDB = tonumber(get_validated_address('AOB_codeGameDB'), 16)
    //local base_ptr = readPointer(byteTableToDword(readBytes(codeGameDB + 4, 4, true)) + codeGameDB + 8)
    //if DEBUG_MODE then
    //    do_log(string.format("codeGameDB base_ptr %X", base_ptr))
    //end
        }


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
            //coreHack.SaveName = "Test";

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
