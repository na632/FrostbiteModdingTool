using System;
using System.Collections.Generic;
using System.Text;

namespace v2k4FIFAModdingCL
{
    public static class FIFAInstanceSingleton
    {
        public static string FIFAVERSION = "FIFA19";
        public static string FIFAVERSION_NODEMO { get { return FIFAVERSION.Replace("_demo", ""); } }
    }
}
