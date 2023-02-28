using FMT.FifaLibrary.CEM;
using System;
using System.Diagnostics;
using System.IO;

namespace FIFACareerDBTests
{
    [TestClass]
    public class Fifa23Tests : IDisposable
    {

        //public string FIFA23TestCareerSaveFilePath { get
        //    {
        //        return Path.Combine(AppContext.BaseDirectory, "TestCareerSaveFile", "Career20220930102999");
        //    } }

        public Stream FIFA23TestCareerResource
        {
            get
            {
                return FMT.FileTools.EmbeddedResourceHelper.GetEmbeddedResourceByName("Career20220930102999");
            }
        }

        public Stream FIFA23ResourceDBMeta
        {
            get
            {
                return FMT.FileTools.EmbeddedResourceHelper.GetEmbeddedResourceByName("fifa_ng_db-meta.XML");
            }
        }


        public Stream FIFA23ResourceLOCMeta
        {
            get
            {
                return FMT.FileTools.EmbeddedResourceHelper.GetEmbeddedResourceByName("eng_us-meta.XML");
            }
        }

        private CEMCore2 CEM => new CEMCore2("FIFA23", FIFA23TestCareerResource, FIFA23ResourceDBMeta);


        [TestMethod]
        public void ReadStatsFromCareerFile()
        {
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            var stats = CEM.GetPlayerStatsAsync().Result;
            sw1.Stop();
            Debug.WriteLine($"Time taken to get stats : {sw1.Elapsed}");
            var statsDoncaster = CEM.GetPlayerStatsAsync(142).Result;
        }


        [TestMethod]
        public void ReadFinancesFromCareerFile()
        {
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            var finances = CEM.GetFinances().Result;
            sw1.Stop();
            Debug.WriteLine($"Time taken to get finances : {sw1.Elapsed}");
        }

        public void Dispose()
        {
            CEM.Dispose();
        }
    }
}