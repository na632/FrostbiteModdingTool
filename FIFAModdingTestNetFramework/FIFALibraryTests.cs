using FifaLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIFAModdingTestNetFramework
{
    [TestClass]
    public class FIFALibraryTests
    {
        [TestMethod]
        public void LoadCareerFileTest()
        {

            var careerFile = new CareerFile(@"C:\Users\paula\Documents\FIFA 20\settings\Career20190930235655", @"C:\Program Files (x86)\RDBM 20\RDBM20\Templates\FIFA 20\fifa_ng_db-meta.XML");

            if (careerFile != null)
            {

                foreach (var dbf in careerFile.Databases)
                {
                    foreach (var t in dbf.Table)
                    {
                    }
                }
            }
        }
    }
}
