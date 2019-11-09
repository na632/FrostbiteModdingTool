using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml.Serialization;
using v2k4FIFAModdingCL.Career.ObjectiveHelpers;
using v2k4FIFAModdingCL.Career.StoryAssetHelpers;
using FifaLibrary;

namespace FIFAModdingTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var cfile = new FifaLibrary.CareerFile(@"C:\Users\paula\Documents\FIFA 19\settings\Career20190828115121A"
                    , @"C:\Program Files (x86)\Fifa Master\RDBM 19\Templates\FIFA 19\fifa_ng_db-meta.xml");

            if(cfile != null)
            {

            }

        }

        [TestMethod]
        public void TestLoadStoryAssetFile()
        {

            XmlSerializer serializer = new XmlSerializer(typeof(StoryAsset));

            StreamReader reader = new StreamReader(@"J:\Work\Modding\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc\dlc_Saga\dlc\Saga\data\StoryAsset\Playtime Requests\9096113e-dbba-46b7-b7f1-6803e7de3bdd.XML");
            var r = (StoryAsset)serializer.Deserialize(reader);
            reader.Close();
            if(r!=null)
            {

            }

        }

        [TestMethod]
        public void TestLoadObjectivesFile()
        {

            XmlSerializer serializer = new XmlSerializer(typeof(objectives));

            StreamReader reader = new StreamReader(@"J:\Work\Modding\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc\dlc_FootballCompEng\dlc\FootballCompEng\data\Objectives\objectivesProfitability.XML");
            var r = (objectives)serializer.Deserialize(reader);
            reader.Close();
            if (r != null)
            {

            }

        }


        [TestMethod]
        public void TestLoadCareerSaveFile()
        {

            CareerFile careerFile = new CareerFile(@"C:\Users\paula\Documents\FIFA 20\settings\Career20191107114315A", "fifa_ng_db-meta.xml");
            if(careerFile!=null)
            {
                var dsets = careerFile.ConvertToDataSet();
                if(dsets != null)
                {

                }
            }

        }
    }
}
