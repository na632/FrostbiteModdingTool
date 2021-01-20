using FIFAModdingUI;
using FrostbiteSdk.Import;
using FrostySdk.Frostbite;
using FrostySdk.Frostbite.IO.Output;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;
using v2k4FIFASDKGenerator;

namespace FrostbiteModdingTests
{
    [TestClass]
    public class FIFA21Tests : ILogger
    {
        private string prevText = string.Empty;
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
        }

        [TestMethod]
        public void TestBuildCache()
        {
            var buildCache = new BuildCache();
            //buildCache.LoadData("FIFA21", @"E:\Origin Games\FIFA 21", this, false);
            buildCache.LoadData("FIFA21", @"E:\Origin Games\FIFA 21", this, true);
        }

        [TestMethod]
        public void TestBuildCacheIndexing()
        {
            var buildCache = new BuildCache();
            //buildCache.LoadData("FIFA21", @"E:\Origin Games\FIFA 21", this, false);
            buildCache.LoadData("FIFA21", @"E:\Origin Games\FIFA 21", this, false);
            //AssetManager.Instance.ebxGuidList.Clear();
            AssetManager.Instance.DoEbxIndexing();
        }

        [TestMethod]
        public void TestBuildSDK()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("FIFA21", @"E:\Origin Games\FIFA 21", this, false);

            var buildSDK = new BuildSDK();
            buildSDK.Build().Wait();
        }

        [TestMethod]
        public void TestExportFaceMesh()
        {
            ProjectManagement projectManagement = new ProjectManagement(@"E:\Origin Games\FIFA 21\FIFA21.exe");
            var project = projectManagement.StartNewProject();
            var skinnedMeshEntry = project.AssetManager.EnumerateEbx("SkinnedMeshAsset").Where(x => x.Name.ToLower().Contains("head_10264_0_0_mesh")).FirstOrDefault();
            if (skinnedMeshEntry != null)
            {
                var skinnedMeshEbx = project.AssetManager.GetEbx(skinnedMeshEntry);
                if (skinnedMeshEbx != null)
                {
                    var resentry = project.AssetManager.GetResEntry(skinnedMeshEntry.Name);
                    var res = project.AssetManager.GetRes(resentry);
                    MeshSet meshSet = new MeshSet(res, project.AssetManager);

                    var exporter = new MeshToFbxExporter();

                    exporter.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test.fbx", "FBX_2012", "Centimeters", false, "content/character/rig/skeleton/player/skeleton_player", "*.fbx", meshSet);
                    exporter.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test_noSkel.fbx", "FBX_2012", "Centimeters", false, null, "*.fbx", meshSet);

                    //exporter.OnlyFirstLOD = true;
                    //exporter.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test_noSkel.obj", "2016", "Meters", true, null, "*.obj", meshSet);
                }
            }
        }

        [TestMethod]
        public void TestImportFaceMesh()
        {
            ProjectManagement projectManagement = new ProjectManagement(@"E:\Origin Games\FIFA 21\FIFA21.exe");
            var project = projectManagement.StartNewProject();
            var skinnedMeshEntry = project.AssetManager.EnumerateEbx("SkinnedMeshAsset").Where(x => x.Name.ToLower().Contains("head_212118_0_0_mesh")).FirstOrDefault();
            if (skinnedMeshEntry != null)
            {
                var skinnedMeshEbx = project.AssetManager.GetEbx(skinnedMeshEntry);
                if (skinnedMeshEbx != null)
                {
                    var resentry = project.AssetManager.GetResEntry(skinnedMeshEntry.Name);
                    var res = project.AssetManager.GetRes(resentry);
                    MeshSet meshSet = new MeshSet(res, project.AssetManager);

                    FrostySdk.Frostbite.IO.Input.FBXImporter importer = new FrostySdk.Frostbite.IO.Input.FBXImporter();
                    var exporter = new MeshToFbxExporter();
                    exporter.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test.fbx", "FBX_2012", "Centimeters", false, "content/character/rig/skeleton/player/skeleton_player", "*.fbx", meshSet);
                    importer.ImportFBX("test.fbx", meshSet, skinnedMeshEbx, skinnedMeshEntry, new FrostySdk.Frostbite.IO.Input.MeshImportSettings()
                    {
                         SkeletonAsset = "content/character/rig/skeleton/player/skeleton_player"
                    });

                }
            }
        }

        [TestMethod]
        public void InjectSDKGeneratorIntoFIFA()
        {

            int? proc = GameInstanceSingleton.GetProcIDFromName("FIFA21");
            proc = GameInstanceSingleton.GetProcIDFromName("FIFA21");
            while (!proc.HasValue || proc == 0)
            {
                Debug.WriteLine($"Waiting for FIFA to appear");
                proc = GameInstanceSingleton.GetProcIDFromName("FIFA21");
                Thread.Sleep(1000);
            }
            if (proc.HasValue)
            {
                var dllpath = @"G:\Work\FIFA Modding\SDKGenerator\x64\Debug\Publish\Generator.dll";
                if (File.Exists(dllpath))
                {
                    Debug.WriteLine($"Injecting: {dllpath}");
                    var bl = new Bleak.Injector(Bleak.InjectionMethod.CreateThread, proc.Value, dllpath, false);
                    bl.InjectDll();
                    Debug.WriteLine($"Injected: {dllpath}");
                }
            }
        }

        [TestMethod]
        public void TestFETFIFAMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(@"E:\Origin Games\FIFA 21\FIFA21.exe");

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.Run(AssetManager.Instance.fs, this, "", "", 
                new System.Collections.Generic.List<string>() {
                    //@"C:\Users\paula\Downloads\Villalibre Molina.fifamod"
                    //@"E:\Origin Games\FIFA 21\fet gp change.fifamod"
                    @"C:\Users\paula\Downloads\FCB17 [FIFA 21] FACEPACK #3 FIX\FCB17 [FIFA 21] FACEPACK #3 FIX\FCB17 Facepack #3 (fix).fifamod"
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestGPMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(@"E:\Origin Games\FIFA 21\FIFA21.exe");
            projectManagement.FrostyProject = new FrostySdk.FrostbiteProject();
            projectManagement.FrostyProject.Load(@"G:\Work\FIFA Modding\Gameplay mod\FIFA 21\Paulv2k4 FIFA 21 Gameplay Version 2 Alpha 12.fbproject");
            projectManagement.FrostyProject.WriteToMod("test.fbmod", new FrostySdk.ModSettings());
            
            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.Run(AssetManager.Instance.fs, this, "", "",
                new System.Collections.Generic.List<string>() {
                    //@"G:\Work\FIFA Modding\Gameplay mod\FIFA 21\Paulv2k4 FIFA 21 Gameplay Version 2 Alpha 12.fbmod"
                    "test.fbmod"
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestLegacyMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(@"E:\Origin Games\FIFA 21\FIFA21.exe");
            projectManagement.FrostyProject = new FrostySdk.FrostbiteProject();
            projectManagement.FrostyProject.Load(@"E:\Origin Games\FIFA 21\test legacy.fbproject");

            var allPlayerLua = AssetManager.Instance.EnumerateCustomAssets("legacy").Where(x => x.Filename.Contains("player", System.StringComparison.OrdinalIgnoreCase));
            var ca = AssetManager.Instance.GetCustomAsset("legacy", AssetManager.Instance.GetCustomAssetEntry("legacy", "data/fifarna/lua/boot.lua"));
            byte[] data = null;
            using (NativeReader nr = new NativeReader(ca))
            {
                nr.Position = 0;
                data = nr.ReadToEnd();
            }

            var editedStream = new MemoryStream();
            using (NativeWriter nw = new NativeWriter(editedStream, leaveOpen: true))
            {
                nw.Write(data);
                nw.Write(Encoding.UTF8.GetBytes("HELLO_WORLD = true"));
                nw.Position = 0;
            }

            AssetManager.Instance.ModifyCustomAsset("legacy", "data/fifarna/lua/boot.lua", new NativeReader(editedStream).ReadToEnd());

            projectManagement.FrostyProject.WriteToMod("test.fbmod", new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.Run(AssetManager.Instance.fs, this, "", "",
                new System.Collections.Generic.List<string>() {
                    "test.fbmod"
                }.ToArray()).Wait();

        }
    }
}
