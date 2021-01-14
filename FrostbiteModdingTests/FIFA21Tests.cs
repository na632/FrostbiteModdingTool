using FrostbiteSdk.Import;
using FrostySdk.Frostbite;
using FrostySdk.Frostbite.IO.Output;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
    }
}
