using FIFAModdingUI;
using FrostbiteSdk.Import;
using FrostySdk.Frostbite;
using FrostySdk.Frostbite.IO.Output;
using FrostySdk.Frosty.FET;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;
using SdkGenerator;
using FrostySdk.FrostySdk.Resources.Mesh2;

namespace FrostbiteModdingTests
{
    [TestClass]
    public class FIFA21Tests : ILogger
    {
        private string prevText = string.Empty;

        public const string GamePath = @"F:\Origin Games\FIFA 21";
        public const string GamePathEXE = @"F:\Origin Games\FIFA 21\FIFA21.exe";
        public void Log(string text, params object[] vars)
        {
            if (prevText != text)
            {
                Debug.WriteLine("[LOGGER] [DEBUG] " + text);
                prevText = text;
            }
        }

        public void LogError(string text, params object[] vars)
        {
            if (prevText != text)
            {
                Debug.WriteLine("[LOGGER] [ERROR] " + text);
                prevText = text;
            }
        }

        public void LogWarning(string text, params object[] vars)
        {
            if (prevText != text)
            {
                Debug.WriteLine("[LOGGER] [WARNING] " + text);
                prevText = text;
            }
        }

        [TestMethod]
        public void TestBuildCache()
        {
            var buildCache = new BuildCache();
            //buildCache.LoadData("FIFA21", @"E:\Origin Games\FIFA 21", this, false);
            buildCache.LoadData("FIFA21", GamePath, this, true, true);
        }

        [TestMethod]
        public void TestBuildCacheIndexing()
        {
            var buildCache = new BuildCache();
            //buildCache.LoadData("FIFA21", @"E:\Origin Games\FIFA 21", this, false);
            buildCache.LoadData("FIFA21", GamePath, this, false);
            //AssetManager.Instance.ebxGuidList.Clear();
            AssetManager.Instance.DoEbxIndexing();
        }

        [TestMethod]
        public void TestBuildSDK()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("FIFA21", GamePath, this, false);

            var buildSDK = new BuildSDK();
            buildSDK.Build().Wait();
        }

        [TestMethod]
        public void TestLiveLegacySystem()
        {
            GameInstanceSingleton.InitializeSingleton(GamePathEXE);
            var legacyModSupportFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA.dll";

            if (!string.IsNullOrEmpty(legacyModSupportFile))
            {
                File.Copy(legacyModSupportFile, @GameInstanceSingleton.GAMERootPath + "v2k4LegacyModSupport.dll", true);

                var legmodsupportdllpath = @GameInstanceSingleton.GAMERootPath + @"v2k4LegacyModSupport.dll";
                GameInstanceSingleton.InjectDLL(legmodsupportdllpath);
            }

    }

    [TestMethod]
        public void TestExportFaceMesh()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            var project = projectManagement.StartNewProject();
            var skinnedMeshEntry = project.AssetManager.EnumerateEbx("SkinnedMeshAsset").Where(x => x.Name.ToLower().Contains("head_10264_0_0_mesh")).FirstOrDefault();
            if (skinnedMeshEntry != null)
            {
                var skinnedMeshEbx = project.AssetManager.GetEbx(skinnedMeshEntry);
                if (skinnedMeshEbx != null)
                {
                    var resentry = project.AssetManager.GetResEntry(skinnedMeshEntry.Name);
                    var res = project.AssetManager.GetRes(resentry);

                    var exporter1 = new MeshSetToFbxExport();
                    var exporter2 = new MeshSetToFbxExport2();
                    MeshSet meshSet = exporter1.LoadMeshSet(skinnedMeshEntry);

                    exporter1.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test.fbx", "FBX_2012", "Meters", true, "content/character/rig/skeleton/player/skeleton_player", "fbx", meshSet);

                    //exporter2.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test.fbx", "2012", "Meters", true, "content/character/rig/skeleton/player/skeleton_player", "fbx", meshSet);
                    //meshToFbxExporter.Export(base.AssetManager, base.Asset, outputFile, "2020", "Meters", false, skeleton, "obj", meshSet);
                    //exporter.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test_noSkel.fbx", "FBX_2012", "Centimeters", true, null, "*.fbx", meshSet);

                    //exporter.OnlyFirstLOD = true;
                    //exporter.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test_noSkel.obj", "2016", "Meters", true, null, "*.obj", meshSet);
                }
            }
        }



        [TestMethod]
        public ProjectManagement TestImportFaceMesh()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            var project = projectManagement.StartNewProject();
            //var skinnedMeshEntry = project.AssetManager.EnumerateEbx("SkinnedMeshAsset").Where(x => x.Name.ToLower().Contains("head_192563_0_0_mesh")).FirstOrDefault();
            var skinnedMeshEntry = project.AssetManager.EnumerateEbx("SkinnedMeshAsset").Where(x => x.Name.ToLower().Contains("head_192563_0_0_mesh")).FirstOrDefault();
            if (skinnedMeshEntry != null)
            {
                var skinnedMeshEbx = project.AssetManager.GetEbx(skinnedMeshEntry);
                if (skinnedMeshEbx != null)
                {
                    var resentry = project.AssetManager.GetResEntry(skinnedMeshEntry.Name);
                    var res = project.AssetManager.GetRes(resentry);
                    MeshSet meshSet = new MeshSet(res);

                    FrostySdk.Frostbite.IO.Input.FBXImporter importer = new FrostySdk.Frostbite.IO.Input.FBXImporter();
                    //FrostySdk.Frostbite.IO.Input.FT.FBXImporter2 importer = new FrostySdk.Frostbite.IO.Input.FT.FBXImporter2(AssetManager.Instance);
                    var exporter = new MeshSetToFbxExport();
                    exporter.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test.fbx", "FBX_2012", "Meters", true, "content/character/rig/skeleton/player/skeleton_player", "*.fbx", meshSet);
                    //importer.ImportFBX("test.fbx", meshSet, skinnedMeshEbx, skinnedMeshEntry, new FrostySdk.Frostbite.IO.Input.MeshImportSettings()
                    importer.ImportFBX(@"C:\Users\paula\Desktop\head_250_0_0_mesh.fbx", meshSet, skinnedMeshEbx, skinnedMeshEntry, new FrostySdk.Frostbite.IO.Input.MeshImportSettings()
                    {
                         SkeletonAsset = "content/character/rig/skeleton/player/skeleton_player"
                    });

                    res = project.AssetManager.GetRes(resentry);
                    meshSet = new MeshSet(res);
                    exporter.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test2.fbx", "FBX_2012", "Meters", true, "content/character/rig/skeleton/player/skeleton_player", "*.fbx", meshSet);


                }
            }
            return projectManagement;
        }

        [TestMethod]
        public void TestImportFaceMeshAndRun()
        {
            var projectManagement = TestImportFaceMesh();
            projectManagement.Project.WriteToMod("test.fbmod", new FrostySdk.ModSettings());
            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    @"test.fbmod"
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void InjectSDKGeneratorIntoFIFA()
        {

            int? proc = GameInstanceSingleton.GetProcIDFromName("FIFA21").Result;
            proc = GameInstanceSingleton.GetProcIDFromName("FIFA21").Result;
            while (!proc.HasValue || proc == 0)
            {
                Debug.WriteLine($"Waiting for FIFA to appear");
                proc = GameInstanceSingleton.GetProcIDFromName("FIFA21").Result;
                Thread.Sleep(1000);
            }
            if (proc.HasValue)
            {
                var dllpath = @"G:\Work\FIFA Modding\SDKGenerator\x64\Debug\Publish\Generator.dll";
                if (File.Exists(dllpath))
                {
                    Debug.WriteLine($"Injecting: {dllpath}");
                    var bl = new Bleak.Injector(Bleak.InjectionMethod.CreateThread, proc.Value, dllpath, false);
                    //var bl = new Bleak.Injector(proc.Value, dllpath, Bleak.InjectionMethod.CreateThread);
                    bl.InjectDll();
                    Debug.WriteLine($"Injected: {dllpath}");
                }
            }
        }

        [TestMethod]
        public void TestFETSaveFETProjectAndLoad()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            projectManagement.Project.Load(@"G:\Work\FIFA Modding\Gameplay mod\FIFA 21\Paulv2k4 FIFA 21 Gameplay Version 3 Final.fbproject");

            FIFAEditorProject editorProject = FIFAEditorProject.ConvertFromFbProject(projectManagement.Project, @"G:\testgpfromfbproject.fifaproject");
            //FIFAEditorProject editorProject = new FIFAEditorProject("FIFA21", AssetManager.Instance, AssetManager.Instance.fs);
            //editorProject.Save(@"G:\testgpfromfbproject.fifaproject");
            //editorProject.Load(@"G:\testgpfromfbproject.fifaproject");


        }

        [TestMethod]
        public void TestFETFIFAMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.GAMERootPath, "", 
                new System.Collections.Generic.List<string>() {
                    //@"C:\Users\paula\Downloads\Villalibre Molina.fifamod"
                    //@"E:\Origin Games\FIFA 21\fet gp change.fifamod"
                    //@"G:\Work\FIFA Modding\Gameplay mod\FIFA 21\Paulv2k4 FIFA 21 Gameplay Version 3 Patch 1 FET.fifamod"
                    //@"G:\Work\FIFA Modding\Sky_Sports_Football_TV_Logo_for_the_English_Premier_League.fifamod"
                    //@"G:\Work\FIFA Modding\GraphicMod\FIFA 21\FCB17 Facepack #5 (fix).fifamod"
                    @"G:\Work\FIFA Modding\GraphicMod\FIFA 21\Mason Greenwood by AlieFFR.fifamod"


                }.ToArray()).Wait();

        }

        [TestMethod]
        public void OpenFETEditorProject()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            FIFAEditorProject editorProject = new FIFAEditorProject("FIFA21", AssetManager.Instance, AssetManager.Instance.fs);
            editorProject.Load(@"C:\Users\paula\Downloads\JAYS BOOTPACK.fifaproject");
            var count = AssetManager.Instance.EnumerateEbx(modifiedOnly: true).Count();
            var res = AssetManager.Instance.EnumerateRes(modifiedOnly: true).ToList();
            var count2 = res.Count();
            var count3 = AssetManager.Instance.EnumerateChunks(modifiedOnly: true).Count();


        }

        [TestMethod]
        public void TestGPMod()
        {
            GameInstanceSingleton.InitializeSingleton(GamePathEXE);
            //ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            //projectManagement.Project = new FrostySdk.FrostbiteProject();
            //projectManagement.Project.Load(@"G:\Work\FIFA Modding\Gameplay mod\FIFA 21\Paulv2k4 FIFA 21 Gameplay Version 2 Alpha 12.fbproject");
            //projectManagement.Project.WriteToMod("test.fbmod", new FrostySdk.ModSettings());
            
            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    @"G:\Work\FIFA Modding\Gameplay mod\FIFA 21\Paulv2k4 FIFA 21 Gameplay Version 2 Alpha 12.fbmod"
                    //"test.fbmod"
                }.ToArray()).Wait();

        }



        [TestMethod]
        public void TestLegacyMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            projectManagement.Project.Load(@"G:\Work\FIFA Modding\Career Mod\Paulv2k4 Customize and Dynamic - Alpha 1.fbproject");
            //projectManagement.Project.Load(@"G:\Work\FIFA Modding\Career Mod\Paulv2k4 Dark Theme.fbproject");

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.Run(this, GameInstanceSingleton.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testR
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestLegacyMod_PlayerLUA()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            projectManagement.Project = new FrostySdk.FrostbiteProject();

            AssetManager.Instance.ModifyLegacyAsset("data/fifarna/lua/assets/player.lua"
                , new NativeReader(new FileStream(@"G:\Work\FIFA Modding\Career Mod\FIFA-21-Career-Mod\Source fbmod\fifarna\lua\assets\player.LUA", FileMode.Open)).ReadToEnd()
                , false);

            var testModFile = $"test{DateTime.Now.Ticks}.fbmod";
            projectManagement.Project.WriteToMod(testModFile, new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.Run(this, GameInstanceSingleton.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testModFile
                }.ToArray()).Wait();

        }

        /// <summary>
        /// This tests a more complex "compressed" asset
        /// </summary>
        [TestMethod]
        public void TestLegacyMod_CompressedAsset()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            projectManagement.Project = new FrostySdk.FrostbiteProject();

            var ca = AssetManager.Instance.GetCustomAsset("legacy", AssetManager.Instance.GetCustomAssetEntry("legacy", @"dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/Finance/ProfCWRelation.csv"));
            byte[] data = null;
            using (NativeReader nr = new NativeReader(ca))
            {
                nr.Position = 0;
                data = nr.ReadToEnd();
            }

            AssetManager.Instance.ModifyLegacyAsset(@"dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/Finance/ProfCWRelation.csv"
                , new NativeReader(new FileStream(@"G:\Work\FIFA Modding\Career Mod\FIFA-21-Career-Mod\Source lmod\dlc\dlc_FootballCompEng\dlc\FootballCompEng\data\Finance\ProfCWRelation.csv", FileMode.Open)).ReadToEnd()
                , false
                );

            projectManagement.Project.WriteToMod("test.fbmod", new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    "test.fbmod"
                }.ToArray()).Wait();

        }

        /// <summary>
        /// This tests a more complex "compressed" asset
        /// </summary>
        [TestMethod]
        public void TestLegacyMod_PlayerValues()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            projectManagement.Project = new FrostySdk.FrostbiteProject();

            
            AssetManager.Instance.ModifyLegacyAsset(@"dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/playervalues.ini"
                , new NativeReader(new FileStream(@"G:\Work\FIFA Modding\Career Mod\FIFA-21-Career-Mod\Source lmod\dlc\dlc_FootballCompEng\dlc\FootballCompEng\data\playervalues.ini", FileMode.Open)).ReadToEnd()
                , false
                );

            projectManagement.Project.WriteToMod("test.fbmod", new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.Run(this, GameInstanceSingleton.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    "test.fbmod"
                }.ToArray()).Wait();

        }
    }
}
