using System;
using System.Collections.Generic;
using System.Text;

namespace v2k4FIFAModdingCL
{
    public static class FIFAInstanceSingleton
    {
        public static bool INITIALIZED = false;

        private static string fifaVersion;
        public static string FIFAVERSION { set { fifaVersion = value; INITIALIZED = !string.IsNullOrEmpty(value); } get { return fifaVersion; } }
        public static string FIFAVERSION_NODEMO { get { return !string.IsNullOrEmpty(FIFAVERSION) ? FIFAVERSION.Replace("_demo", "") : null; } }

        public static string FIFARootPath = "";

        public static string FIFADataPath { get{return FIFARootPath + "\\Data\\"; } }

        public static string FIFALocaleINIPath { get { return FIFARootPath + "\\Data\\locale.ini"; } }


        public static string FIFAPatchPath { get{return FIFARootPath + "\\Patch\\"; } }
        public static string FIFA_INITFS_Win32 { get { return FIFAPatchPath + "\\initfs_Win32"; } }
    }
}
