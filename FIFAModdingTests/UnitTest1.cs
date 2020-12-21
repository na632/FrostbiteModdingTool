using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Web;
using v2k4FIFAModdingCL.CGFE;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using v2k4FIFAModding.Career;
using System.Windows;
using System.Threading;
using FrostySdk;
using FrostySdk.Managers;
using FIFAModdingUI;
//using FifaLibrary;

namespace FIFAModdingTests
{
    [TestClass]
    public class CareerFileTests
    {
        [TestMethod]
        public void LoadCareerFileTest()
        {

            var careerFile = new CareerFile(@"C:\Users\paula\Documents\FIFA 20\settings\Career20190930235655", @"C:\Program Files (x86)\RDBM 20\RDBM20\Templates\FIFA 20\fifa_ng_db-meta.XML");

            List<DataSet> dataSets = new List<DataSet>();
            if (careerFile != null)
            {
                foreach (var dbf in careerFile.Databases.Where(x => x != null))
                {
                    dataSets.Add(dbf.ConvertToDataSet());
                }
            }



            if (dataSets.Count > 0)
            {

            }

            //BinaryReader r = new BinaryReader(new FileStream(@"C:\Users\paula\Documents\FIFA 20\settings\Career20190930235655", FileMode.Open), Encoding.UTF8);

            //var m_HeaderSignature = r.ReadChars(16);
            //var m_CrcEaHeaderPosition = r.BaseStream.Position;
            //var m_CrcEaHeader = r.ReadUInt32();
            //var m_CryptArea = r.ReadBytes(24);

            //var m_Database[this.m_NDatabases] = new DbFile();
            //this.m_Database[this.m_NDatabases].DescriptorDataSet = this.m_DescriptorDataSet;
        }

        [TestMethod]
        public void LoadSquadFileTest()
        {

            var careerFile = new CareerFile(@"C:\Users\paula\Documents\FIFA 20\settings\Squads20200124104151", @"C:\Program Files (x86)\RDBM 20\RDBM20\Templates\FIFA 20\fifa_ng_db-meta.XML");

            List<DataSet> dataSets = new List<DataSet>();
            if (careerFile != null)
            {
                foreach (var dbf in careerFile.Databases.Where(x => x != null))
                {
                    dataSets.Add(dbf.ConvertToDataSet());
                }
            }



            if (dataSets.Count > 0)
            {

            }

            //BinaryReader r = new BinaryReader(new FileStream(@"C:\Users\paula\Documents\FIFA 20\settings\Career20190930235655", FileMode.Open), Encoding.UTF8);

            //var m_HeaderSignature = r.ReadChars(16);
            //var m_CrcEaHeaderPosition = r.BaseStream.Position;
            //var m_CrcEaHeader = r.ReadUInt32();
            //var m_CryptArea = r.ReadBytes(24);

            //var m_Database[this.m_NDatabases] = new DbFile();
            //this.m_Database[this.m_NDatabases].DescriptorDataSet = this.m_DescriptorDataSet;
        }


        //[TestMethod]
        //public void LoadCareerFileIntoCareerDb2()
        //{

        //    var careerFile = new CareerFile(@"C:\Users\paula\Documents\FIFA 20\settings\Career20200216121303A", @"C:\Program Files (x86)\RDBM 20\RDBM20\Templates\FIFA 20\fifa_ng_db-meta.XML");
        //    List<DataSet> dataSets = new List<DataSet>();
        //    if (careerFile != null)
        //    {
        //        int indexOfFiles = 0;
        //        foreach (var dbf in careerFile.Databases.Where(x => x != null))
        //        {
        //            var convertedDS = dbf.ConvertToDataSet();
        //            string json = JsonConvert.SerializeObject(convertedDS, Formatting.Indented);
        //            File.WriteAllText(careerFile.InGameName + "_" + indexOfFiles + ".json", json);
        //            indexOfFiles++;
        //            dataSets.Add(convertedDS);
        //        }
        //        if (dataSets.Count > 0)
        //        {
        //            var d = JsonConvert.DeserializeObject<CareerDB2>(File.ReadAllText(careerFile.InGameName + "_1.json"));
        //            Assert.IsNotNull(d);
        //        }
        //    }

        //    //BinaryReader r = new BinaryReader(new FileStream(@"C:\Users\paula\Documents\FIFA 20\settings\Career20190930235655", FileMode.Open), Encoding.UTF8);

        //    //var m_HeaderSignature = r.ReadChars(16);
        //    //var m_CrcEaHeaderPosition = r.BaseStream.Position;
        //    //var m_CrcEaHeader = r.ReadUInt32();
        //    //var m_CryptArea = r.ReadBytes(24);

        //    //var m_Database[this.m_NDatabases] = new DbFile();
        //    //this.m_Database[this.m_NDatabases].DescriptorDataSet = this.m_DescriptorDataSet;
        //}

        [TestMethod]
        public void TestLaunchFIFAWithMods()
        {
            var r = LaunchFIFA.LaunchAsync(@"E:\Origin Games\FIFA 20", "", new List<string>() { @"C:\Users\paula\Downloads\Other Faces.zip" }).Result;

            //FIFAModdingUI.LaunchFIFA.Launch(@"E:\Origin Games\FIFA 20", "Mods/", new FIFAModdingUI.Mods.ModList().ModListItems);

        }

        [TestMethod]
        public void LoadFrostyGameplayProject()
        {
            var FileSystem = new FrostySdk.FileSystem(@"E:\Origin Games\FIFA 20");
            var ResourceManager = new ResourceManager(FileSystem);
            var AssetManager = new AssetManager(FileSystem, ResourceManager);
            var FrostyProject = new FrostbiteProject(AssetManager, FileSystem);
            FrostyProject.Load(@"J:\Work\Modding\FIFA Modding\Gameplay mod\Version TC 2 Alpha 10\paulv2k4 FIFA 20 Gameplay Mod - TC 2 Alpha 10.fbproject");
            if (FrostyProject != null)
            {

            }
        }
    }
}
