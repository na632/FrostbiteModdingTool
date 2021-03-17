using FrostySdk.Frostbite;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using v2k4FIFAModding.Frosty;
using v2k4FIFASDKGenerator;

namespace FrostbiteModdingTests
{
    [TestClass]
    public class Madden21Tests : ILogger
    {

        public const string GamePath = @"F:\Origin Games\Madden NFL 21\";

        public void Log(string text, params object[] vars)
        {
            Debug.WriteLine(text);
        }

        public void LogError(string text, params object[] vars)
        {
            Debug.WriteLine(text);
        }

        public void LogWarning(string text, params object[] vars)
        {
            Debug.WriteLine(text);
        }

        [TestMethod]
        public void TestBuildCache()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("Madden21", GamePath, this, true);
        }

        [TestMethod]
        public void TestBuildSDK()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("Madden21", GamePath, this, false);

            var buildSDK = new BuildSDK();
            buildSDK.Build().Wait();
        }

        [TestMethod]
        public void TestSplashScreenMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePath + "\\Madden21.exe");
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            projectManagement.Project.Load(@"G:\\MaddenSplashProject.fbproject");

            var testfbmodname = "test-" + new Random().Next().ToString() + ".fbmod";

            projectManagement.Project.WriteToMod(testfbmodname, new FrostySdk.ModSettings());

            

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.Run(AssetManager.Instance.fs, this, "", "",
                new System.Collections.Generic.List<string>() {
                    testfbmodname
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestGPMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePath + "\\Madden21.exe");
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            projectManagement.Project.Load(@"G:\\MaddenGPProject.fbproject");

            var testfbmodname = "test-" + new Random().Next().ToString() + ".fbmod";

            projectManagement.Project.WriteToMod(testfbmodname, new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.Run(AssetManager.Instance.fs, this, "", "",
                new System.Collections.Generic.List<string>() {
                    testfbmodname
                }.ToArray()).Wait();

        }

    }
}
