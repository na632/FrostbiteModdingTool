using FrostySdk.Frostbite.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModding.Frosty;

namespace FIFALibraryNETFrameworkTests
{
    [TestClass]
    public class Madden21ReaderTests
    {
        [TestMethod]
        public void ReadVanillaTocFile()
        {

            ProjectManagement projectManagement = new ProjectManagement(@"E:\Origin Games\Madden NFL 21\Madden21.exe");
            projectManagement.StartNewProject();

            //TocCasReader_M21 tocCasReader_M21 = new TocCasReader_M21();
            //tocCasReader_M21.Read(@"E:\Origin Games\Madden NFL 21\Patch\Win32\splash.toc", 0, new FrostySdk.Managers.BinarySbDataHelper(projectManagement.FrostyProject.AssetManager));

        }
    }
}
