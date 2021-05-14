using CareerExpansionMod.CEM;
using CareerExpansionMod.CEM.FIFA;
using FrostbiteModdingUI.CEM;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModdingCL;
using v2k4FIFAModdingCL.MemHack.Core;

namespace FrostbiteModdingTests
{
    [TestClass]
    public class FIFA21CEMTests : ILogger
    {
        public const string GamePath = @"F:\Origin Games\FIFA 21";
        public const string GamePathEXE = @"F:\Origin Games\FIFA 21\FIFA21.exe";

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
        public void LoadStatsFromLatestCareerSave()
        {
            GameInstanceSingleton.InitializeSingleton(GamePathEXE);

            var cem = new CEMCore2("FIFA21");
            var ps = cem.GetPlayerStats();
            using (var nw = new NativeWriter(new FileStream("_TestExportCSV.csv", FileMode.Create), wide: true))
            {
                nw.WriteLine("Player Id,Player Name,Season Year,Competition,Appereances,Goals,Assists,Clean Sheets,Average Rating");
                for (var i = 0; i < ps.Count; i++)
                {
                    nw.WriteLine(
                        ps[i].PlayerId
                        + ",\"" + ps[i].PlayerName + "\""
                        + "," + ps[i].SeasonYear
                        + ",\"" + ps[i].CompName + "\""
                        + "," + ps[i].Apps
                        + "," + ps[i].Goals
                        + "," + ps[i].Assists
                        + "," + ps[i].CleanSheets
                        + "," + ps[i].AverageRating);
                }
            }
        }

        [TestMethod]
        public void LoadStatsFromJaysArsenalSave()
        {
            var cem = new CEMCore2("FIFA21");
            var newFile = CEMCore2.SetupCareerFile(@"C:\Users\paula\Documents\FIFA 21\settings\Career20210511171702");
            var ps = cem.GetPlayerStats();
        }

        [TestMethod]
        public void LoadStatsFromCEMTest()
        {
            var cem = new CEMCore2("FIFA21");
            var newFile = CEMCore2.SetupCareerFile(@"C:\Users\paula\Documents\FIFA 21\settings\Career20210514142807");
            var ps = cem.GetPlayerStats();
        }

        //[TestMethod]
        //public void LoadStatsFromSelectedCareer()
        //{
        //    var cem = new CEMCore2("FIFA21");
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    var result = openFileDialog.ShowDialog();
        //    if (result.HasValue && result.Value)
        //    {
        //        var newFile = CEMCore2.SetupCareerFile(openFileDialog.FileName);
        //        var ps = cem.GetPlayerStats();
        //    }
        //}

        public byte[] HexStringToByte(string param1, string param2)
        {
            return new byte[] { 
                Convert.ToByte("0x" + param1.Substring(6, 2))
                , Convert.ToByte("0x" + param1.Substring(4, 2))
                , Convert.ToByte("0x" + param1.Substring(2, 2))
                , Convert.ToByte("0x" + param1.Substring(0, 2))
                , Convert.ToByte("0x" + param2.Substring(6, 2))
                , Convert.ToByte("0x" + param2.Substring(4, 2))
                , Convert.ToByte("0x" + param2.Substring(2, 2))
                , Convert.ToByte("0x" + param2.Substring(0, 2))
            };
        }

        public string FlipHexString(string innerHex)
        {
            return innerHex.Substring(6, 2) + innerHex.Substring(4, 2) + innerHex.Substring(2, 2) + innerHex.Substring(0, 2);
        }

        public string HexStringLittleEndian(int number)
        {
            return FlipHexString(number.ToString("X8"));
        }


    }
}
