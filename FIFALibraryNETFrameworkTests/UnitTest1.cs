using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FifaLibrary;
using FrostySdk;
using FrostySdk.Managers;

namespace FIFALibraryNETFrameworkTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()

        {
            var careerFile = new CareerFile(@"C:\Users\paula\Documents\FIFA 20\settings\Career20190930235655", @"C:\Program Files (x86)\RDBM 20\RDBM20\Templates\FIFA 20\fifa_ng_db-meta.XML");

            if (careerFile != null)
            {

            }
        }

        [TestMethod]
        public void LoadFrostyGameplayProject()
        {
            var FileSystem = new FrostySdk.FileSystem(@"E:\Origin Games\FIFA 20");
            var ResourceManager = new ResourceManager(FileSystem);
            var AssetManager = new AssetManager(FileSystem, ResourceManager);
            var FrostyProject = new FrostyProject(AssetManager, FileSystem);
            FrostyProject.Load(@"J:\Work\Modding\FIFA Modding\Gameplay mod\Version TC 2 Alpha 10\paulv2k4 FIFA 20 Gameplay Mod - TC 2 Alpha 10.fbproject");
            if (FrostyProject != null)
            {

            }
        }
    }
}
