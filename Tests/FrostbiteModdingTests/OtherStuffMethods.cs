using FrostySdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrostbiteModdingTests
{
    [TestClass]
    public class OtherStuffMethods
    {
        [TestMethod]
        public void DumpFrostyProfiles()
        {
            ProfileManager.DumpFrostyProfile("FIFA20");
        }
    }
}
