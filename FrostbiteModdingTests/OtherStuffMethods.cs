using FrostySdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace FrostbiteModdingTests
{
    [TestClass]
    public class OtherStuffMethods
    {
        [TestMethod]
        public void DumpFrostyProfiles()
        {
            ProfilesLibrary.DumpFrostyProfile("FIFA20");
        }
    }
}
