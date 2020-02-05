using System;
using System.Collections.Generic;
using System.Text;

namespace v2k4FIFAModdingCL
{
    public static class FIFAInstanceSingleton
    {
        public static string FIFAVERSION = "FIFA19";
        public static string FIFAVERSION_NODEMO { get { return FIFAVERSION.Replace("_demo", ""); } }

        public static string FIFARootPath = "";

        public static string FIFADataPath { get{return FIFARootPath + "\\Data\\"; } }
        public static string FIFAPatchPath { get{return FIFARootPath + "\\Patch\\"; } }
        public static string FIFA_INITFS_Win32 { get { return FIFAPatchPath + "\\initfs_Win32"; } }
    }
}
