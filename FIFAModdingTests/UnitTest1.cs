using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
