using FrostySdk.Frostbite;
using FrostySdk.Frostbite.IO.Output;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SdkGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;

namespace FrostbiteModdingTests
{
    [TestClass]
    public class Madden21Tests : ILogger
    {

        public const string GamePath = @"F:\Origin Games\Madden NFL 21\";
        public const string GamePathExe = GamePath + "\\Madden21.exe";

        private string lastLog;
        public void Log(string text, params object[] vars)
        {
            if (lastLog != text)
            {
                Debug.WriteLine(text);
                lastLog = text;
            }
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
        public void TestBuildCacheWithLoadedSDK()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("Madden21", GamePath, this, true, true);
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

            var oldFiles = Directory.GetFiles(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "*.fbmod");
            foreach (var oFile in oldFiles) File.Delete(oFile);
            var testfbmodname = "test-" + new Random().Next().ToString() + ".fbmod";

            projectManagement.Project.WriteToMod(testfbmodname, new FrostySdk.ModSettings());

            

            ModdingSupport.FrostyModExecutor frostyModExecutor = new ModdingSupport.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testfbmodname
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestMainMenuSplashScreenMod()
        {
            //ProjectManagement projectManagement = new ProjectManagement(GamePath + "\\Madden21.exe");
            GameInstanceSingleton.InitializeSingleton(GamePathExe);


            var oldFiles = Directory.GetFiles(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "*.fbmod");
            foreach (var oFile in oldFiles) File.Delete(oFile);
            var testfbmodname = @"G:\Work\MADDEN Modding\Paulv2k4 Main Menu splash mod.fbmod";

            ModdingSupport.FrostyModExecutor frostyModExecutor = new ModdingSupport.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testfbmodname
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestTeamWipeMod()
        {
            //ProjectManagement projectManagement = new ProjectManagement(GamePath + "\\Madden21.exe");
            GameInstanceSingleton.InitializeSingleton(GamePathExe);


            var oldFiles = Directory.GetFiles(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "*.fbmod");
            foreach (var oFile in oldFiles) File.Delete(oFile);
            var testfbmodname = @"G:\Work\MADDEN Modding\Paulv2k4 Team Wipe Mod.fbmod";

            ModdingSupport.FrostyModExecutor frostyModExecutor = new ModdingSupport.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testfbmodname
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestColtKitMod()
        {
            //ProjectManagement projectManagement = new ProjectManagement(GamePath + "\\Madden21.exe");
            GameInstanceSingleton.InitializeSingleton(GamePathExe);

            var oldFiles = Directory.GetFiles(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "*.fbmod");
            foreach (var oFile in oldFiles) File.Delete(oFile);
            var testfbmodname = @"G:\Work\MADDEN Modding\Paulv2k4 Colt kit mod 2.fbmod";

            ModdingSupport.FrostyModExecutor frostyModExecutor = new ModdingSupport.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GamePath, "",
                new System.Collections.Generic.List<string>() {
                    testfbmodname
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestColtKitModProject()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePath + "\\Madden21.exe");
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            projectManagement.Project.Load(@"G:\\Work\MADDEN Modding\Paulv2k4 Colt mod.fbproject");
            projectManagement.Project.WriteToMod(@"G:\\Work\MADDEN Modding\Paulv2k4 Colt kit mod 2.fbmod", new FrostySdk.ModSettings() { });

            var oldFiles = Directory.GetFiles(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "*.fbmod");
            foreach (var oFile in oldFiles) File.Delete(oFile);
            var testfbmodname = @"G:\Work\MADDEN Modding\Paulv2k4 Colt kit mod 2.fbmod";

            ModdingSupport.FrostyModExecutor frostyModExecutor = new ModdingSupport.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GamePath, "",
                new System.Collections.Generic.List<string>() {
                    testfbmodname
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestGPMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePath + "\\Madden21.exe");
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            projectManagement.Project.Load(@"G:\\MaddenGPProject2.fbproject");

            var oldFiles = Directory.GetFiles(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "*.fbmod");
            foreach (var oFile in oldFiles) File.Delete(oFile);
            var testfbmodname = "test-" + new Random().Next().ToString() + ".fbmod";

            projectManagement.Project.WriteToMod(testfbmodname, new FrostySdk.ModSettings());

            ModdingSupport.FrostyModExecutor frostyModExecutor = new ModdingSupport.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testfbmodname
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestLegacyMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathExe);
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            projectManagement.Project.Load(@"G:\\MaddenLegacyProject.fbproject");

            var oldFiles = Directory.GetFiles(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "*.fbmod");
            foreach (var oFile in oldFiles) File.Delete(oFile);
            var testfbmodname = "test-" + new Random().Next().ToString() + ".fbmod";

            projectManagement.Project.WriteToMod(testfbmodname, new FrostySdk.ModSettings());

            ModdingSupport.FrostyModExecutor frostyModExecutor = new ModdingSupport.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testfbmodname
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void ExportFaceMesh()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathExe);
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            var project = projectManagement.Project;

            var skinnedMeshEntry = project.AssetManager.EnumerateEbx("SkinnedMeshAsset").Where(x => x.Name.ToLower().Contains("hameer_2402_mesh")).FirstOrDefault();
            if (skinnedMeshEntry != null)
            {
                var skinnedMeshEbx = project.AssetManager.GetEbx(skinnedMeshEntry);
                if (skinnedMeshEbx != null)
                {
                    var resentry = project.AssetManager.GetResEntry(skinnedMeshEntry.Name);
                    var res = project.AssetManager.GetRes(resentry);

                    var exporter1 = new MeshSetToFbxExport();
                    MeshSet meshSet = exporter1.LoadMeshSet(skinnedMeshEntry);

                    exporter1.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test.fbx", "FBX_2012", "Meters", true, "content/characters/rig/skeleton/player/maddenfb_hero_skeleton", "fbx", meshSet);
                }
            }
        }


        [TestMethod]
        public ProjectManagement TestImportFaceMesh()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathExe);
            var project = projectManagement.StartNewProject();
            //var skinnedMeshEntry = project.AssetManager.EnumerateEbx("SkinnedMeshAsset").Where(x => x.Name.ToLower().Contains("head_192563_0_0_mesh")).FirstOrDefault();
            var skinnedMeshEntry = project.AssetManager.EnumerateEbx("SkinnedMeshAsset").Where(x => x.Name.ToLower().Contains("mahomesiipatrick_12635_mesh")).FirstOrDefault();
            if (skinnedMeshEntry != null)
            {
                var skinnedMeshEbx = project.AssetManager.GetEbx(skinnedMeshEntry);
                if (skinnedMeshEbx != null)
                {
                    var resentry = project.AssetManager.GetResEntry(skinnedMeshEntry.Name);
                    var res = project.AssetManager.GetRes(resentry);
                    MeshSet meshSet = new MeshSet(res);
                    using(NativeWriter nwTest = new NativeWriter(new FileStream("MeshSet-" + skinnedMeshEntry.Filename + ".dat", FileMode.Create)))
                    {
                        nwTest.WriteBytes(((MemoryStream)res).ToArray());
                    }


                    FrostySdk.Frostbite.IO.Input.FBXImporter importer = new FrostySdk.Frostbite.IO.Input.FBXImporter();
                    //FrostySdk.Frostbite.IO.Input.FT.FBXImporter2 importer = new FrostySdk.Frostbite.IO.Input.FT.FBXImporter2(AssetManager.Instance);
                    var exporter = new MeshSetToFbxExport();
                    exporter.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test.fbx", "FBX_2012", "Meters", true, "content/characters/rig/skeleton/player/maddenfb_hero_skeleton", "*.fbx", meshSet);
                    //importer.ImportFBX("test.fbx", meshSet, skinnedMeshEbx, skinnedMeshEntry, new FrostySdk.Frostbite.IO.Input.MeshImportSettings()
                    importer.ImportFBX(@"C:\Users\paula\Desktop\mackkhalil_12344_mesh.fbx", meshSet, skinnedMeshEbx, skinnedMeshEntry, new FrostySdk.Frostbite.IO.Input.MeshImportSettings()
                    {
                        SkeletonAsset = "content/characters/rig/skeleton/player/maddenfb_hero_skeleton"
                    });

                }
            }
            return projectManagement;
        }

        [TestMethod]
        public void TestImportFaceMeshAndRun()
        {
            var projectManagement = TestImportFaceMesh();
            projectManagement.Project.WriteToMod("test.fbmod", new FrostySdk.ModSettings());
            ModdingSupport.FrostyModExecutor frostyModExecutor = new ModdingSupport.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    @"test.fbmod"
                }.ToArray()).Wait();

        }

    }
}
