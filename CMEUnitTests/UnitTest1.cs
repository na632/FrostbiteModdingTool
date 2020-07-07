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
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection.Metadata.Ecma335;

namespace CMEUnitTests
{
    

    [TestClass]
    public class UnitTest1
    {
        //[DllImport("paulv2k4HackHelpers.dll", CallingConvention = CallingConvention.Cdecl)]
        //public static extern ulong ResolvePtr(UIntPtr address, int offsetpos);

        long ResolvePtr(UIntPtr address, int offsetpos)
        {
            var proc = CoreHack.GetProcess();
            if (proc.HasValue)
            {
                long offset = Convert.ToInt64((address + offsetpos).ToUInt64());
                Debug.WriteLine($"ResolvePtr:{offset.ToString("X8")}");
                var readint = CoreHack.MemLib.readInt(offset.ToString("X8"));
                Debug.WriteLine($"ResolvePtr:{readint.ToString("X8")}");

                var addOffsetToAddress = new UIntPtr((ulong)readint + (ulong)offset + 4);
                Debug.WriteLine($"ResolvePtr:{addOffsetToAddress.ToUInt64().ToString("X8")}");

                //long resolved = 0;
                //resolved = Convert.ToInt64((addOffsetToAddress + offsetpos + 4).ToUInt64());
                ////return resolved;
                //Debug.WriteLine($"ResolvePtr:{resolved.ToString("X8")}");

                return (long)addOffsetToAddress.ToUInt64();
            }
            return 0;
        }

        [TestMethod]
        public void LoadFIFAGameDBPointers()
        {
            var proc = CoreHack.GetProcess();
            if(proc.HasValue)
            {

                //var codeGameDB_addy = CoreHack.MemLib.AoBScan("44 8B 7A 34 44 8B 62 38", true, true).Result.FirstOrDefault();
                var codeGameDB_addy = CoreHack.MemLib.AoBScan(long.Parse(CoreHack.MemLib.getMinAddress().ToString()), 0x00007ffffffeffff, "4C 0F 44 35 ?? ?? ?? ?? 41 8B 4E 08", true, true, true).Result.FirstOrDefault();
                if (codeGameDB_addy > 0)
                {
                    var t2 = codeGameDB_addy.ToString("X8");
                    if(!string.IsNullOrEmpty(t2))
                    {
                        Debug.WriteLine(t2);
                        //ulong x = 0x1410FB844;
                        var ul = ulong.Parse(codeGameDB_addy.ToString());
                        var uintpr = new UIntPtr(ul);
                        var newptr = ResolvePtr(uintpr, 4).ToString("X8");
                        Debug.WriteLine(newptr);

                    }
                }


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
