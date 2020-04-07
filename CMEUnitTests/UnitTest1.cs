using Microsoft.VisualStudio.TestTools.UnitTesting;
using v2k4FIFAModding.Career.CME.FIFA;

namespace CMEUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void LoadFIFALeaguesFromCSV()
        {
            var fifaleagues = FIFALeague.GetFIFALeagues();
        }
    }
}
