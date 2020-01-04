using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml.Serialization;
using v2k4FIFAModdingCL.Career.ObjectiveHelpers;
using v2k4FIFAModdingCL.Career.StoryAssetHelpers;
using FifaLibrary;
using System.Text;
using System.Web;

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

            CareerFile careerFile = new CareerFile(@"C:\Users\paula\Documents\FIFA 20\settings\Career20191126231940A", "fifa_ng_db-meta.xml");
            if(careerFile!=null)
            {
                var dsets = careerFile.ConvertToDataSet();
                if(dsets != null)
                {

                }
            }

        }

        [TestMethod]
        public void TestEncryptDecrypt()
        {
            var readin = System.IO.File.ReadAllText(@"E:\Origin Games\FIFA 20\Patch\initfs_Win32");
            var key = System.IO.File.ReadAllText(@"E:\Origin Games\FIFA 20\Patch\key.txt");
            var readout = XORdecrypt(readin, key);
            Assert.IsNotNull(readout);
            System.IO.File.WriteAllText(@"E:\Origin Games\FIFA 20\Patch\initfs_Win32_decrypt", readout);


        }

        private static string xor(string text, string key)
        {
            var result = new StringBuilder();
            for (int c = 0; c < text.Length; c++)
                result.Append((char)((uint)text[c] ^ (uint)key[c % key.Length]));
            return result.ToString();
        }

        public static string XORencrypt(string text, string key)
        {
            byte[] decrypted = Encoding.UTF8.GetBytes(text);
            byte[] encrypted = new byte[decrypted.Length];

            for (int i = 0; i < decrypted.Length; i++)
            {
                encrypted[i] = (byte)(decrypted[i] ^ key[i % key.Length]);
            }

            string xored = System.Convert.ToBase64String(encrypted);

            return xored;
        }

        public static string XORdecrypt(string text, string key)
        {
            var decoded = System.Convert.FromBase64String(HttpUtility.UrlDecode(text));

            byte[] result = new byte[decoded.Length];

            for (int c = 0; c < decoded.Length; c++)
            {
                result[c] = (byte)((uint)decoded[c] ^ (uint)key[c % key.Length]);
            }

            string dexored = Encoding.UTF8.GetString(result);

            return dexored;
        }
    }
}
