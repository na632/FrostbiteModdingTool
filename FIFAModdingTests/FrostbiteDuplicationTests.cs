using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml.Serialization;
using v2k4FIFAModdingCL.Career.ObjectiveHelpers;
using v2k4FIFAModdingCL.Career.StoryAssetHelpers;
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
using System.Diagnostics;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;
//using FifaLibrary;

namespace FIFAModdingTests
{
    // THIS DOESNT WORK WHILE FrostySDK remains in .NET 4.8
    //[TestClass]
    //public class FrostbiteDuplicationTests
    //{
    //    [TestMethod]
    //    public void TestDuplicationOfHead()
    //    {
    //        ProjectManagement projectManagement = new ProjectManagement();
    //        FIFAInstanceSingleton.FIFARootPath = @"E:\Origin Games\FIFA 20";
    //        FIFAInstanceSingleton.FIFAVERSION = "FIFA20";
    //        var FrostyProject = projectManagement.StartNewProject();
    //        var ebxItems = FrostyProject.AssetManager.EnumerateEbx();

    //        foreach (var i in ebxItems.Where(x => x.Path.Contains("olivier_giroud")).OrderBy(x => x.Path).ThenBy(x => x.Filename))
    //        {
    //            Debug.WriteLine(i.Path);
    //            //FrostyProject.AssetManager.GetEbxEntry("");
    //        }
    //    }
    //}
}