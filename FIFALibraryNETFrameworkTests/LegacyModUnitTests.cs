using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using FIFAModdingUI;
using FrostySdk.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using v2k4FIFAModding;
using v2k4FIFAModdingCL;

namespace FIFALibraryNETFrameworkTests
{
  
    [TestClass]
    public class LegacyModUnitTests : ILogger
    {
       
        public int? getProcIDFromName(string name) //new 1.0.2 function
        {
            Process[] processlist = Process.GetProcesses();

            if (name.Contains(".exe"))
                name = name.Replace(".exe", "");

            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName.Equals(name, StringComparison.CurrentCultureIgnoreCase)) //find (name).exe in the process list (use task manager to find the name)
                    return theprocess.Id;
            }

            return null; //if we fail to find it
        }

        public void Log(string text, params object[] vars)
        {
            Debug.WriteLine("[LOG] " + text);
        }

        public void LogError(string text, params object[] vars)
        {
            Debug.WriteLine("[ERROR] " + text);
        }

        public void LogWarning(string text, params object[] vars)
        {
            Debug.WriteLine("[WARNING] " + text);
        }

        [TestMethod]
        public void TestLegacyModSupportInjection()
        {
            int? proc = getProcIDFromName("FIFA20");
            //if (!proc.HasValue || proc == 0)
            //{
            //    var r = LaunchFIFA.LaunchAsync(
            //        FIFAInstanceSingleton.FIFARootPath
            //        , ""
            //        , new System.Collections.Generic.List<string>() { }
            //        , this
            //        , FIFAInstanceSingleton.FIFAVERSION
            //        , true).Result;
            //    Thread.Sleep(4000);
            //}
            

            proc = getProcIDFromName("FIFA20");
            while (!proc.HasValue || proc == 0)
            {
                Debug.WriteLine($"Waiting for FIFA to appear");
                proc = getProcIDFromName("FIFA20");
                Thread.Sleep(1000);
            }
            if (proc.HasValue)
            {
                //var dllpath = @"E:\Origin Games\FIFA 20\FIFALiveEditor.dll";
                //Debug.WriteLine($"About to inject: {dllpath}");
                //var bl = new Bleak.Injector(Bleak.InjectionMethod.CreateThread, proc.Value, dllpath, false);
                //bl.InjectDll();
                //Debug.WriteLine($"Injected: {dllpath}");


                //var dllpath = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\v2k4LegacyFileModSupport.dll";
                if (File.Exists(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA.dll"))
                    File.Copy(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA.dll", @"E:\Origin Games\FIFA 20\v2k4LegacyModSupport.dll", true);

                var dllpath = @"E:\Origin Games\FIFA 20\v2k4LegacyModSupport.dll";
                var bl = new Bleak.Injector(Bleak.InjectionMethod.CreateThread, proc.Value, dllpath, false);
                bl.InjectDll();
                Debug.WriteLine($"Injected: {dllpath}");
            }
        }

        [TestMethod]
        public void TestLegacyModSupportInjection_Madden21()
        {
            int? proc = getProcIDFromName("MADDEN21");
            proc = getProcIDFromName("MADDEN21");
            while (!proc.HasValue || proc == 0)
            {
                Debug.WriteLine($"Waiting for Madden 21 to appear");
                proc = getProcIDFromName("MADDEN21");
                Thread.Sleep(1000);
            }
            if (proc.HasValue)
            {
                if (File.Exists(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA.dll"))
                    File.Copy(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA.dll", @"E:\Origin Games\Madden NFL 21\v2k4LegacyModSupport.dll", true);

                var dllpath = @"E:\Origin Games\Madden NFL 21\v2k4LegacyModSupport.dll";
                var bl = new Bleak.Injector(Bleak.InjectionMethod.CreateThread, proc.Value, dllpath, false);
                bl.InjectDll();
                Debug.WriteLine($"Injected: {dllpath}");
            }
        }

        [TestMethod]
        public void TestLegacyModSupportInjection_FIFA21()
        {
            int? proc = getProcIDFromName("FIFA21");
            proc = getProcIDFromName("FIFA21");
            while (!proc.HasValue || proc == 0)
            {
                Debug.WriteLine($"Waiting for FIFA 21 to appear");
                proc = getProcIDFromName("FIFA21");
                Thread.Sleep(1000);
            }
            if (proc.HasValue)
            {
                if (File.Exists(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA.dll"))
                    File.Copy(Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA.dll", @"E:\Origin Games\FIFA 21\v2k4LegacyModSupport.dll", true);

                var dllpath = @"E:\Origin Games\FIFA 21\v2k4LegacyModSupport.dll";
                var bl = new Bleak.Injector(Bleak.InjectionMethod.CreateThread, proc.Value, dllpath, false);
                bl.InjectDll();
                Debug.WriteLine($"Injected: {dllpath}");
            }
        }

        [TestMethod]
        [HandleProcessCorruptedStateExceptionsAttribute]
        public void TestEncryptModFile()
        {
            //var zip = File.ReadAllText(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc.zip");
            //    if (File.Exists(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc.mod"))
            //        File.Delete(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc.mod");

            v2k4Encryption.encryptFile(@"E:\Origin Games\FIFA 20\LegacyMods\root\Legacy\dlc\dlc_FootballCompEng\dlc\FootballCompEng\data\playervalues.ini", @"E:\Origin Games\FIFA 20\LegacyMods\root\Legacy\dlc\dlc_FootballCompEng\dlc\FootballCompEng\data\test.mod");
            v2k4Encryption.decryptFile(@"E:\Origin Games\FIFA 20\LegacyMods\root\Legacy\dlc\dlc_FootballCompEng\dlc\FootballCompEng\data\test.mod", @"E:\Origin Games\FIFA 20\LegacyMods\root\Legacy\dlc\dlc_FootballCompEng\dlc\FootballCompEng\data\playervalues_dec.ini");

            // Test encrypted file
            v2k4Encryption.test_load_mod_file(@"E:\Origin Games\FIFA 20\LegacyMods\root\Legacy\dlc\dlc_FootballCompEng\dlc\FootballCompEng\data\test.mod");

            // Test normal file
            v2k4Encryption.test_load_mod_file(@"E:\Origin Games\FIFA 20\LegacyMods\root\Legacy\dlc\dlc_FootballCompEng\dlc\FootballCompEng\data\playervalues_dec.ini");

            //v2k4Encryption.encryptFile(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc.mod", @"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc.zip.bk.zip");

            //File.WriteAllText(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc.mod", e);

            //using (FileStream fsOrig = new FileStream(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc.zip", FileMode.OpenOrCreate))
            //{
            //    using (FileStream fsmod = new FileStream(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc.mod", FileMode.OpenOrCreate))
            //    {
            //        var fiferisatheifkey = "FIFERISATHEIF";
            //        foreach (var c in fiferisatheifkey)
            //        {
            //            fsmod.Write(BitConverter.GetBytes(c), 0, BitConverter.GetBytes(c).Length);
            //        }
            //        while (fsOrig.Position != fsOrig.Length)
            //        {
            //            var b = (byte)fsOrig.ReadByte();
            //            fsmod.WriteByte(b);
            //        }
            //    }
            //}



            // Convert it back
            //using (FileStream fs = new FileStream(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc.mod", FileMode.Open))
            //{
            //    using (FileStream fszip = new FileStream(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\dlc_back.zip", FileMode.OpenOrCreate))
            //    {
            //        while (fs.Position != fs.Length)
            //        {
            //            var c = Convert.ToChar(fs.ReadByte());
            //            var s = encryptChar(c);
            //            fszip.Write(BitConverter.GetBytes(s), 0, BitConverter.GetBytes(s).Length);
            //        }
            //    }

            //}

        }
    }
}
