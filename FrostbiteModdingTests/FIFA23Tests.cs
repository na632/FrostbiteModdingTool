using FIFA23Plugin;
using FifaLibrary;
using Frostbite.Textures;
using FrostbiteModdingUI.CEM;
using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Frostbite.IO.Output;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using MinHook;
using ProcessMemoryUtilities.Managed;
using SdkGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;

namespace FrostbiteModdingTests
{
    [TestClass]
    public class Fifa23Tests : ILogger
    {
        private string prevText = string.Empty;

        public const string GamePath = @"F:\Origin Games\FIFA 23";
        public string GamePathEXE
        {
            get
            {

                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\EA Sports\\FIFA 23"))
                {
                    if (key != null)
                    {
                        string installDir = key.GetValue("Install Dir").ToString();
                        return installDir + "FIFA23.exe";
                    }
                }
                return string.Empty;
            }
        }// = @"F:\Origin Games\FIFA 23\FIFA23.exe";

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
        public void ReadDataInCareerFile()
        {
            GameInstanceSingleton.InitializeSingleton(GamePathEXE);
            Directory.GetFiles("C:\\Users\\paula\\Documents\\FIFA 23\\settings\\", "Career*")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
            var cem = new CEMCore2("FIFA23");

            var stats = cem.GetPlayerStatsAsync().Result;
            var statsDoncaster = cem.GetPlayerStatsAsync(142).Result;
        }

        [TestMethod]
        public void EditCareerFile()
        {

            var originalFile = "C:\\Users\\paula\\Documents\\FIFA 23\\settings\\Career20220930120547";
            var newFileFile = "C:\\Users\\paula\\Documents\\FIFA 23\\settings\\Career20220930102999";
            //CareerFile careerFile = new CareerFile(
            //    new FileStream(originalFile, FileMode.Open)
            //    , new FileStream("C:\\RDBM 22\\Templates\\FIFA 23\\fifa_ng_db-meta.XML", FileMode.Open)
            //    );
            CareerFile careerFile = new CareerFile(
                originalFile
                , "C:\\RDBM 22\\Templates\\FIFA 23\\fifa_ng_db-meta.XML"
                );
            File.Copy(originalFile, newFileFile, true);
            careerFile.InGameName = "testtestestestest";

            careerFile.SaveEa(newFileFile);
        }

        [TestMethod]
        public void BuildCache()
        {
            var buildCache = new BuildCache();
            //buildCache.LoadData("FIFA23", GamePath, this, true, false);
            buildCache.LoadData("FIFA23", GamePath, this, true, true);

            var ebxItems = AssetManager.Instance.EnumerateEbx().ToList();
            var resItems = AssetManager.Instance.EnumerateRes().ToList();
            var chunkItems = AssetManager.Instance.EnumerateChunks().ToList();
            var legacyItems = AssetManager.Instance.EnumerateCustomAssets("legacy").ToList();
        }

        [TestMethod]
        public void BuildCacheIndexing()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("FIFA23", GamePath, this, false, true);
            AssetManager.Instance.DoEbxIndexing();
        }

        [TestMethod]
        public void TestFindProcesses()
        {
            Process.GetProcesses().Where(x => x.ProcessName.Contains("fifa", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(x =>
              {
                  var prThread = x.Threads[1];
                  byte[] buffer = new byte[1024];
                  var v = NativeWrapper.ReadProcessMemoryArray<byte>(x.Handle, prThread.StartAddress, buffer);
                  //prThread.
              });
        }

        [TestMethod]
        public void BuildSDK()
        {
            //var buildCache = new BuildCache();
            //buildCache.LoadData("FIFA23", GamePath, this, false, false);

            GameInstanceSingleton.InitializeSingleton(GamePathEXE, false, this);
            var buildSDK = new BuildSDK();
            buildSDK.Build().Wait();

            var ebxItems = AssetManager.Instance.EnumerateEbx().ToList();
            var resItems = AssetManager.Instance.EnumerateRes().ToList();
            var chunkItems = AssetManager.Instance.EnumerateChunks().ToList();
            var legacyItems = AssetManager.Instance.EnumerateCustomAssets("legacy").ToList();
        }

        [TestMethod]
        public void BuildSDKFromEXE()
        {
            var buildSDK = new BuildSDKFromEXE();
            buildSDK.Build(GamePathEXE);
        }

        [TestMethod]
        public void ReadSharedTypeDescriptor()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("FIFA23", GamePath, this, false, false);
            EbxSharedTypeDescriptorV2 std = new EbxSharedTypeDescriptorV2("SharedTypeDescriptors.ebx", false);
            EbxSharedTypeDescriptorV2 patchstd = new EbxSharedTypeDescriptorV2("SharedTypeDescriptors_patch.ebx", false);
        }

        [TestMethod]
        public void ReadSimpleGPFile()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.StartNewProject();

            var simpleEbxEntry = AssetManager.Instance.GetEbxEntry("fifa/attribulator/gameplay/groups/gp_actor/gp_actor_facialanim_runtime");
            Assert.IsNotNull(simpleEbxEntry);
            var simpleAsset = AssetManager.Instance.GetEbx(simpleEbxEntry);
            if (simpleAsset != null)
            {

            }
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
            dyn.ATTR_DribbleJogSpeed = 0.01f;
            //dyn.ATTR_DribbleWalkSpeed = 0.005f;
            //dyn.ATTR_JogSpeed = 0.005f;
            //dyn.ATTR_WalkSpeed = 0.005f;
            //dyn.ATTR_DribbleJogSpeed = 0.9f;
            //dyn.ATTR_DribbleWalkSpeed = 0.9f;
            //dyn.ATTR_JogSpeed = 0.9f;
            //dyn.ATTR_WalkSpeed = 0.9f;
            AssetManager.Instance.ModifyEbx("fifa/attribulator/gameplay/groups/gp_actor/gp_actor_movement_runtime", complexAsset);

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            ModdingSupport.ModExecutor.UseModData = false;
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

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            ModdingSupport.ModExecutor.UseModData = false;
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
            dyn.Airflow_DragMultiplier = 100;
            AssetManager.Instance.ModifyEbx("Fifa/Attribulator/Gameplay/groups/gp_physics/gp_physics_airflow_runtime", complexAsset);

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            ModdingSupport.ModExecutor.UseModData = true;
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testR
                }.ToArray()).Wait();


        }

        [TestMethod]
        public void TestCareerMod()
        {
            GameInstanceSingleton.InitializeSingleton(GamePathEXE, true, this);
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            projectManagement.Project.Load(@"G:\Work\FIFA Modding\Career Mod\V Career Mod - Alpha 1.fbproject");

            if (File.Exists("test.fbmod"))
                File.Delete("test.fbmod");

            projectManagement.Project.WriteToMod("test.fbmod", new FrostySdk.ModSettings());

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    "test.fbmod"
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestGPModProject()
        {
            GameInstanceSingleton.InitializeSingleton(GamePathEXE, true, this);
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            var projectResult = projectManagement.Project.LoadAsync(@"G:\Work\FIFA Modding\Gameplay mod\FIFA 23\V Gameplay Mod - v0.5.fbproject").Result;

            if (File.Exists("test.fbmod"))
                File.Delete("test.fbmod");

            projectManagement.Project.WriteToMod("test.fbmod", new FrostySdk.ModSettings());

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    "test.fbmod"
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestGPMod()
        {
            var modPath = @"G:\Work\FIFA Modding\Gameplay mod\FIFA 23\V Gameplay Mod - v0.5.fbmod";
            GameInstanceSingleton.InitializeSingleton(GamePathEXE, true, this);

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    modPath
                }.ToArray()).Wait();

        }

        public void DeleteAllBackupsInFolder(string dir)
        {
            int countOfDelete = 0;
            foreach (var tFile in Directory.EnumerateFiles(dir, "*.bak", new EnumerationOptions() { RecurseSubdirectories = true }))
            {
                File.Delete(tFile);
                countOfDelete++;
            }

            Debug.WriteLine($"I have deleted {countOfDelete} *.bak files");
            //foreach(var childDir in Directory.EnumerateDirectories(dir))
            //{
            //    DeleteAllBackupsInFolder(childDir);
            //}
        }

        [TestMethod]
        public void DeleteAllBackups()
        {
            DeleteAllBackupsInFolder(GamePath);
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

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            ModdingSupport.ModExecutor.UseModData = true;
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testR
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestSplashscreenMod()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            projectManagement.Project.Load(@"G:\Work\FIFA Modding\test 23 splashscreen.fbproject");

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            ModdingSupport.ModExecutor.UseModData = false;
            frostyModExecutor.ForceRebuildOfMods = true;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>() {
                    testR
                }.ToArray()).Wait();

        }


        [TestMethod]
        public void LoadSplashscreenTexture()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("FIFA23", GamePath, this, false, true);
            var ebxEntrySplash = AssetManager.Instance.GetEbxEntry("fifa/fesplash/splashscreen/splashscreen");
            var ebxSplash = AssetManager.Instance.GetEbx(ebxEntrySplash);

        }

        [TestMethod]
        public void LoadInitfs()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("FIFA23", GamePath, this, false, true);

        }

        [TestMethod]
        public void LoadInitfsWriteInitfs()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("FIFA23", GamePath, this, false, true);
            var initfs = FileSystem.Instance.ReadInitfs(FileSystem.Instance.LoadKey());
            using (FileStream fsTestInitfs = new FileStream("initfsTest.dat", FileMode.OpenOrCreate))
            {
                FileSystem.Instance.WriteInitfs(fsTestInitfs);
            }
        }

        [TestMethod]
        public void LoadLocaleIni()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            var s = Encoding.UTF8.GetString(FileSystem.Instance.ReadLocaleIni());
            var s2 = Encoding.ASCII.GetString(FileSystem.Instance.ReadLocaleIni());
        }

        [TestMethod]
        public void ReadWriteLocaleIni()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.Project = new FrostySdk.FrostbiteProject();
            var localeBytes = FileSystem.Instance.ReadLocaleIni();
            var s = Encoding.UTF8.GetString(localeBytes);

            var skipBootflowText = new StringBuilder("");
            skipBootflowText.AppendLine("");
            skipBootflowText.AppendLine("[]");
            skipBootflowText.AppendLine("SKIP_BOOTFLOW=1");

            if (!s.Contains("SKIP_BOOTFLOW=1", StringComparison.OrdinalIgnoreCase))
                s += skipBootflowText;

            var enData = FileSystem.Instance.WriteLocaleIni(Encoding.UTF8.GetBytes(s));

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            ModdingSupport.ModExecutor.UseModData = false;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>()
                {
                }.ToArray()).Wait();
        }

        [TestMethod]
        public void LoadLegacy()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData("FIFA23", GamePath, this, false, true);

            var ebxFCC = AssetManager.Instance.EBX.Keys.Where(x => x.Contains("legacy", StringComparison.OrdinalIgnoreCase));
            var ebxFile = AssetManager.Instance.EBX.Keys.Where(x => x.Contains("file", StringComparison.OrdinalIgnoreCase));
            var ebxCollector = AssetManager.Instance.EBX.Keys.Where(x => x.Contains("collector", StringComparison.OrdinalIgnoreCase));
            var legacyItems = AssetManager.Instance.EnumerateCustomAssets("legacy").ToList();
        }

        [TestMethod]
        public void LaunchVanillaFromModData()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            projectManagement.StartNewProject();

            var testR = "test-" + new Random().Next().ToString() + ".fbmod";
            projectManagement.Project.WriteToMod(testR, new FrostySdk.ModSettings());

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
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

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            ModdingSupport.ModExecutor.UseModData = false;
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

            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            ModdingSupport.ModExecutor.UseModData = false;
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

        [TestMethod]
        public void TestDecompressRecompress()
        {
            Oodle.Bind(GamePath);
            //var filePath = GamePath + "/data/win32/superbundlelayout/fifa_installpackage_02/cas_01.cas";
            var filePath = "F:\\Origin Games\\FIFA 23\\Data\\\\win32\\superbundlelayout\\fifa_installpackage_02\\cas_01.cas";
            using (var readerCas = new NativeReader(filePath))
            {
                var originalSize = 692;
                var originalPosition = 4984462;
                readerCas.Position = originalPosition;
                var originalBytes = readerCas.ReadBytes((int)originalSize);
                var decomp = new CasReader(new MemoryStream(originalBytes)).Read();
                byte[] recomp = null;
                recomp = Utils.CompressFile(decomp, compressionOverride: CompressionType.Oodle, oodleCO: 8);
                for (uint i = 4; i < 14; i++)
                {
                    recomp = Utils.CompressFile(decomp, compressionOverride: CompressionType.Oodle, oodleCO: i);
                    if (recomp.Length <= originalSize)
                    {

                    }
                }
            }
        }


        [TestMethod]
        public void TestLoadValuesFromLiveTuningUpdate()
        {
            ProjectManagement projectManagement = new ProjectManagement(GamePathEXE);
            projectManagement.Project = new FrostySdk.FrostbiteProject();

            var assets = FileSystem.Instance.LiveTuningUpdate.ReadFIFALiveTuningUpdate();
            var asset = FileSystem.Instance.LiveTuningUpdate.GetLiveTuningUpdateAsset(assets.First().Key);

        }

        /// <summary>
        /// To start without AC you need to alter your Installer.xml
        /// </summary>
        [TestMethod]
        public void TestLoadWithoutAC()
        {
            //ProjectManagement projectManagement = new ProjectManagement(GamePathEXE, this);
            //projectManagement.Project = new FrostySdk.FrostbiteProject();

            GameInstanceSingleton.InitializeSingleton(GamePathEXE, false, this);
            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            ModdingSupport.ModExecutor.UseModData = false;
            frostyModExecutor.ForceRebuildOfMods = false;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>()
                {
                }.ToArray()).Wait();
            //var r1 = GameInstanceSingleton.InjectDLL(@"G:\Work\FIFA Modding\FIFAModdingUI\Libraries\v2k4FrostyModdingSupport\ThirdParty\FIFA23\FIFALiveEditor.DLL", true).Result;
            //var r2 = GameInstanceSingleton.InjectDLL(new FileInfo(@"ThirdParty\\FIFA23\\FIFA.dll").FullName, true).Result;
            //var r3 = GameInstanceSingleton.InjectDLL(@"G:\Work\FIFA Modding\FIFA_23_LE_v23.1.0.0\FIFALiveEditor.DLL", true).Result;

        }

        /// <summary>
        /// To start without AC you need to alter your Installer.xml
        /// </summary>
        [TestMethod]
        public void TestLoadWithoutACInModData()
        {
            GameInstanceSingleton.InitializeSingleton(GamePathEXE, false, this);
            ModdingSupport.ModExecutor frostyModExecutor = new ModdingSupport.ModExecutor();
            ModdingSupport.ModExecutor.UseModData = true;
            frostyModExecutor.ForceRebuildOfMods = false;
            frostyModExecutor.Run(this, GameInstanceSingleton.Instance.GAMERootPath, "",
                new System.Collections.Generic.List<string>()
                {
                    "C:\\Users\\paula\\Desktop\\V Gameplay Mod - v0.4.fbmod"
                }.ToArray()).Wait();

        }

        [TestMethod]
        public void TestLoadWithoutACAlt()
        {
            GameInstanceSingleton.InitializeSingleton(GamePathEXE, false, this);

            var r2 = GameInstanceSingleton.InjectDLL(new FileInfo(@"Plugins\\FIFA23Plugin.dll").FullName, true).Result;

        }

        /// <summary>
        /// To start without AC you need to alter your Installer.xml
        /// </summary>
        [TestMethod]
        public void TestTOCReadWrite()
        {
            GameInstanceSingleton.InitializeSingleton(GamePathEXE, false, this);
            var toc = new FIFA23Plugin.TOCFile();
            using (NativeReader nr = new NativeReader(new FileStream("F:\\Origin Games\\FIFA 23\\Patch\\Win32\\careersba.toc", FileMode.Open))) 
            {
                var dbo = toc.Read(nr);
            }

            toc.Write(new MemoryStream());
        }
    }
}

