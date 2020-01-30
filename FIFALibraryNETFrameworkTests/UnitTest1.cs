using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FifaLibrary;

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
    }
}
