using Frostbite.Textures;
using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Frostbite.IO.Output;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SdkGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;

namespace FrostbiteModdingTests
{
    [TestClass]
    public class Fifa22Tests : ILogger
    {
        private string prevText = string.Empty;

        public const string GamePath = @"F:\Origin Games\FIFA 22";
        public const string GamePathEXE = @"F:\Origin Games\FIFA 22\FIFA22.exe";

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
        public void BuildCache()
        {
            var buildCache = new BuildCache();
            //buildCache.LoadData("Fifa22", GamePath, this, true, false);
            buildCache.LoadData("Fifa22", GamePath, this, true, true);

            var ebxItems = AssetManager.Instance.EnumerateEbx().ToList();
            var resItems = AssetManager.Instance.EnumerateRes().ToList();
            var chunkItems = AssetManager.Instance.EnumerateChunks().ToList();
            var legacyItems = AssetManager.Instance.EnumerateCustomAssets("legacy").ToList();
        }

        [TestMethod]
        public void BuildCacheIndexing()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("Fifa22", GamePath, this, false, true);
            AssetManager.Instance.DoEbxIndexing();
        }

        [TestMethod]
        public void BuildSDK()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("Fifa22", GamePath, this, false, false);

            var buildSDK = new BuildSDK();
            buildSDK.Build().Wait();

            var ebxItems = AssetManager.Instance.EnumerateEbx().ToList();
            var resItems = AssetManager.Instance.EnumerateRes().ToList();
            var chunkItems = AssetManager.Instance.EnumerateChunks().ToList();
            var legacyItems = AssetManager.Instance.EnumerateCustomAssets("legacy").ToList();
        }

        [TestMethod]
        public void ReadSharedTypeDescriptor()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("Fifa22", GamePath, this, false, false);
            EbxSharedTypeDescriptorV2 std = new EbxSharedTypeDescriptorV2("SharedTypeDescriptors.ebx", false);
        }

        [TestMethod]
        public void ReadSimpleGPFile()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.StartNewProject();

            var simpleEbxEntry = AssetManager.Instance.GetEbxEntry("fifa/attribulator/gameplay/groups/gp_actor/gp_actor_facialanim_runtime");
            Assert.IsNotNull(simpleEbxEntry);
            var simpleAsset = AssetManager.Instance.GetEbx(simpleEbxEntry);
        }

        [TestMethod]
        public void ReadComplexGPFile()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.StartNewProject();

            var ebxEntry = AssetManager.Instance.GetEbxEntry("fifa/attribulator/gameplay/groups/gp_actor/gp_actor_movement_runtime");
            Assert.IsNotNull(ebxEntry);
            var complexAsset = AssetManager.Instance.GetEbx(ebxEntry);
        }

        [TestMethod]
        public void ModComplexGPFile()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.StartNewProject();

            var ebxEntry = AssetManager.Instance.GetEbxEntry("fifa/attribulator/gameplay/groups/gp_actor/gp_actor_movement_runtime");
            Assert.IsNotNull(ebxEntry);
            var complexAsset = AssetManager.Instance.GetEbx(ebxEntry);
            var dyn = (dynamic)complexAsset.RootObject;
            dyn.ATTR_DribbleJogSpeed = 0.005f;
            dyn.ATTR_DribbleWalkSpeed = 0.005f;
            dyn.ATTR_JogSpeed = 0.005f;
            dyn.ATTR_WalkSpeed = 0.005f;
            //dyn.ATTR_DribbleJogSpeed = 0.9f;
            //dyn.ATTR_DribbleWalkSpeed = 0.9f;
            //dyn.ATTR_JogSpeed = 0.9f;
            //dyn.ATTR_WalkSpeed = 0.9f;
            AssetManager.Instance.ModifyEbx("fifa/attribulator/gameplay/groups/gp_actor/gp_actor_movement_runtime", complexAsset);

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            paulv2k4ModdingExecuter.FrostyModExecutor.UseModData = true;
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testR
                }.ToArray()).Wait();


        }

        [TestMethod]
        public void ModComplexGPFile2()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.StartNewProject();

            var ebxEntry = AssetManager.Instance.GetEbxEntry("fifa/attribulator/gameplay/groups/gp_kickerror/gp_kickerror_passshotcontexteffectshotdriven_runtime");
            Assert.IsNotNull(ebxEntry);
            var complexAsset = AssetManager.Instance.GetEbx(ebxEntry);
            var dyn = (dynamic)complexAsset.RootObject;
            dyn.PASSSHOT_CONTEXTEFFECT_Animation_Lace_Difficulty = 100.0f;
            dyn.PASSSHOT_CONTEXTEFFECT_MissRateVsAttribute.Internal.Points[0].Y = 100.0f;
            dyn.PASSSHOT_CONTEXTEFFECT_MissRateVsAttribute.Internal.Points[1].Y = 100.0f;
            AssetManager.Instance.ModifyEbx("fifa/attribulator/gameplay/groups/gp_kickerror/gp_kickerror_passshotcontexteffectshotdriven_runtime", complexAsset);

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            paulv2k4ModdingExecuter.FrostyModExecutor.UseModData = true;
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testR
                }.ToArray()).Wait();


        }

        [TestMethod]
        public void ModGPPhysics()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.StartNewProject();

            var ebxEntry = AssetManager.Instance.GetEbxEntry("Fifa/Attribulator/Gameplay/groups/gp_physics/gp_physics_airflow_runtime");
            Assert.IsNotNull(ebxEntry);
            var complexAsset = AssetManager.Instance.GetEbx(ebxEntry);
            var dyn = (dynamic)complexAsset.RootObject;
            dyn.Airflow_AirPressure = 100.0f;
            AssetManager.Instance.ModifyEbx("Fifa/Attribulator/Gameplay/groups/gp_physics/gp_physics_airflow_runtime", complexAsset);

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            paulv2k4ModdingExecuter.FrostyModExecutor.UseModData = true;
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testR
                }.ToArray()).Wait();


        }

        [TestMethod]
        public void ModEbxFromJson()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.StartNewProject();

            var ebxEntry = AssetManager.Instance.GetEbxEntry("fifa/attribulator/gameplay/groups/gp_actor/gp_actor_movement_runtime");
            Assert.IsNotNull(ebxEntry);
            var complexAsset = AssetManager.Instance.GetEbx(ebxEntry);
            AssetManager.Instance.ModifyEbxJson("fifa/attribulator/gameplay/groups/gp_actor/gp_actor_movement_runtime"
                , File.ReadAllText(@"G:\Work\FIFA Modding\Gameplay mod\FIFA 21\gp_actor_movement_runtime.json"));

            //var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            //projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            //paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            //paulv2k4ModdingExecuter.FrostyModExecutor.UseModData = false;
            //frostyModExecutor.ForceRebuildOfMods = true;
            //frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
            //    new System.Collections.Generic.List<string>() {
            //        testR
            //    }.ToArray()).Wait();


        }

        [TestMethod]
        public void TestArsenalKitMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            projectManagement.Project.Load(@"G:\Work\FIFA Modding\GraphicMod\FIFA 22\test kit project.fbproject");

            //var name = "content/character/kit/kit_0/arsenal_1/home_0_0/jersey_1_0_0_color";
            //var ebxEntry = AssetManager.Instance.GetEbxEntry(name);
            //var resEntry = AssetManager.Instance.GetResEntry(name);
            //if (resEntry != null)
            //{
            //    Texture texture = new Texture(resEntry);
            //    TextureImporter textureImporter = new TextureImporter();

            //    if (ebxEntry != null)
            //    {
            //        textureImporter.Import(@"G:\Work\FIFA Modding\GraphicMod\FIFA 21\Kits\Chelsea\Home\jersey_5_0_0_color.png", ebxEntry, ref texture);
            //    }
            //}

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            paulv2k4ModdingExecuter.FrostyModExecutor.UseModData = true;
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testR
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void LoadLegacy()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("Fifa22", GamePath, this, false, true);

            var ebxFCC = AssetManager.Instance.EBX.Keys.Where(x=>x.Contains("legacy", StringComparison.OrdinalIgnoreCase));
            var ebxFile = AssetManager.Instance.EBX.Keys.Where(x=>x.Contains("file", StringComparison.OrdinalIgnoreCase));
            var ebxCollector = AssetManager.Instance.EBX.Keys.Where(x=>x.Contains("collector", StringComparison.OrdinalIgnoreCase));
            var legacyItems = AssetManager.Instance.EnumerateCustomAssets("legacy").ToList();
        }

        [TestMethod]
        public void LaunchVanillaFromModData()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.StartNewProject();

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testR
                }.ToArray()).Wait();
        }

        [TestMethod]
        public void LaunchVanillaFromNormalFS()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.StartNewProject();

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            paulv2k4ModdingExecuter.FrostyModExecutor.UseModData = false;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testR
                }.ToArray()).Wait();
        }

        [TestMethod]
        public void LaunchCareerMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.StartNewProject();
            projectManagement.Project.Load(@"G:\Work\FIFA Modding\Career Mod\FIFA-22-Career-Mod\Paulv2k4 FIFA 22 Career Mod - Alpha 2.fbproject");

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            paulv2k4ModdingExecuter.FrostyModExecutor.UseModData = false;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testR
                }.ToArray()).Wait();
        }

        [TestMethod]
        public void ExportFaceMesh()
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
                    MeshSet meshSet = exporter1.LoadMeshSet(skinnedMeshEntry);

                    exporter1.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test.fbx", "FBX_2012", "Meters", true, "content/character/rig/skeleton/player/skeleton_player", "fbx", meshSet);
                }
            }
        }
    }
}

