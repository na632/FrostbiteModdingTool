using CareerExpansionMod.CEM;
using FrostySdk.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModdingCL.MemHack.Core;

namespace FrostbiteModdingTests
{
    [TestClass]
    public class FIFA21CEMTests : ILogger
    {
        public void Log(string text, params object[] vars)
        {
            Debug.WriteLine(text);
        }

        public void LogError(string text, params object[] vars)
        {
            Debug.WriteLine(text);
        }

        public void LogWarning(string text, params object[] vars)
        {
            Debug.WriteLine(text);
        }

        [TestMethod]
        public void TestInitialise()
        {

            bool success = CEMCore.InitialStartupOfCEM().Result;

            var saveName = CEMCore.CEMCoreInstance.CoreHack.GetSaveName();

            var pid1 = 258406;
            var tid1 = 1960;

            var bytesOfPlayerId = BitConverter.GetBytes(258406);
            var bytesOfTeamIdd = BitConverter.GetBytes(112254);
            Debug.WriteLine(pid1.ToString("X8"));
            Debug.WriteLine(tid1.ToString("X8"));

            //List<FIFAPlayerStat> playerStats = FIFAPlayerStat.GetPlayerStats(258406, 112254).ToList();//.Where(x => x.Appereances > 0).ToList();
            //List<FIFAPlayerStat> playerStats2 = FIFAPlayerStat.GetPlayerStats(258460, 112254).ToList();//.Where(x => x.Appereances > 0).ToList();
            List<FIFAPlayerStat> playerStats = FIFAPlayerStat.GetTeamPlayerStats(112254).ToList();
        }
    }
}
