using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FIFAModdingUI;
using Frostbite.Textures;
using FrostyEditor.Controls;
using FrostySdk;
using FrostySdk.FrostySdk.Ebx;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using paulv2k4ModdingExecuter;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;

namespace FIFALibraryNETFrameworkTests
{
    [TestClass]
    public class FrostbiteProjectTests : ILogger
    {

        private void InitializeOfSelectedGame(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                var FIFADirectory = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                FIFAInstanceSingleton.FIFARootPath = FIFADirectory;
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                FIFAInstanceSingleton.FIFAVERSION = fileName.Replace(".exe", "");
                if (!ProfilesLibrary.Initialize(FIFAInstanceSingleton.FIFAVERSION))
                {
                    throw new Exception("Unable to Initialize Profile");
                }
            }
        }

        [TestMethod]
        public void LoadProjectAndRun()
        {
            InitializeOfSelectedGame(@"E:\Origin Games\FIFA 20\FIFA20.exe");
            ProjectManagement projectManagement = new ProjectManagement();
            projectManagement.StartNewProject();
            projectManagement.FrostyProject.Load(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\v2k4CareerMod.fbproject");
            projectManagement.FrostyProject.WriteToMod("TestFullMod.fbmod", new ModSettings() { Title = "v2k4 Test Full Mod", Author = "paulv2k4", Version = "1.00" });

            var r = LaunchFIFA.LaunchAsync(
               FIFAInstanceSingleton.FIFARootPath
               , ""
               , new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }
               , this
               , FIFAInstanceSingleton.FIFAVERSION
               , true).Result;
        }

        public List<Tuple<string, string, object>> GetRootObjectProperties(object RootObject)
        {
            List<Tuple<string, string, object>> items = new List<Tuple<string, string, object>>();
            foreach (var p in RootObject.GetType().GetProperties())
            {
                items.Add(new Tuple<string, string, object>(p.Name, p.PropertyType.ToString(), p.GetValue(RootObject, null)));
            }
            return items;
        }

        [TestMethod]
        public void LoadProjectAndExtractGameplay_Madden21()
        {
            if (!Directory.Exists("Debugging\\EBX\\Gameplay"))
                Directory.CreateDirectory("Debugging\\EBX\\Gameplay");

            InitializeOfSelectedGame(@"E:\Origin Games\Madden NFL 21\Madden21.exe");
            ProjectManagement projectManagement = new ProjectManagement();
            projectManagement.StartNewProject();
            var allEBX = projectManagement.FrostyProject.AssetManager.EnumerateEbx().ToList();
            var character_interaction = allEBX.Where(x => x.Path.ToLower().Contains("attribsys")).ToList();
            foreach (var eb in character_interaction)
            {
                var ebx = projectManagement.FrostyProject.AssetManager.GetEbx(eb as EbxAssetEntry);
                if (ebx != null)
                {
                    //foreach (var o in ebx.Objects)
                    //{

                    //}
                    var robjProps = GetRootObjectProperties(ebx.RootObject);
                    File.WriteAllText($"Debugging/EBX/Gameplay/{eb.DisplayName}.dat", JsonConvert.SerializeObject(robjProps));
                    Assert.IsNotNull(robjProps);
                }
            }
        }
    

        [TestMethod]
        public void LoadProjectAndRun_Madden21()
        {
            InitializeOfSelectedGame(@"E:\Origin Games\Madden NFL 21\Madden21.exe");
            ProjectManagement projectManagement = new ProjectManagement();
            projectManagement.StartNewProject();

            var alllegacy = projectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy").ToList();


            var allEBX = projectManagement.FrostyProject.AssetManager.EnumerateEbx(includeLinked: true).ToList();
            var character_interaction = allEBX.Where(x => x.DisplayName.ToLower().Contains("character_interaction")).ToList();
            foreach (var eb in character_interaction)
            {
                var ebx = projectManagement.FrostyProject.AssetManager.GetEbx(eb);
                if (ebx != null)
                {
                    foreach(var o in ebx.Objects)
                    {
                        
                    }
                    var robjProps = GetRootObjectProperties(ebx.RootObject);
                    var r = robjProps.Where(x => x.Item1.Contains("run_indication_"));
                    foreach(var it in r)
                    {
                        var nIt3 = new {
                            x = 0.0,
                            y = 0.0,
                            z = 1.0,
                            w = 1.0
                        };
                    }

                    var root = ebx.RootObject as dynamic;
                    root.run_indication_indicator_color.x = 0.0f;
                    root.run_indication_indicator_color.y = 0.0f;
                    root.run_indication_indicator_color.z = 1.0f;

                    root.run_indication_indicator_color_non_loco.x = 0.0f;
                    root.run_indication_indicator_color_non_loco.y = 0.0f;
                    root.run_indication_indicator_color_non_loco.z = 1.0f;

                    root.run_indication_indicator_color_intersection.x = 0.0f;
                    root.run_indication_indicator_color_intersection.y = 0.0f;
                    root.run_indication_indicator_color_intersection.z = 1.0f;

                    root.run_indication_debug_show_line = true;
                    //getEbx_movement.Objects.Count(x=>x.)
                    //getEbx_movement.AddRootObject(root);
                    //getEbx_movement.RemoveObject(root);
                    ebx.AddObject(root);
                    //getEbx_movement.AddRootObject(root);
                    projectManagement.FrostyProject.AssetManager.ModifyEbx(eb.Name, ebx);

                    File.WriteAllText($"Debugging/EBX/{eb.Filename}.dat", JsonConvert.SerializeObject(robjProps));
                    Assert.IsNotNull(robjProps);
                }
            }

            var allRes = projectManagement.FrostyProject.AssetManager.EnumerateRes().ToList();
            var ebxofres = allEBX.Where(x => x.DisplayName.ToLower().Contains("splashscreen")).ToList();

            foreach (var res in projectManagement.FrostyProject.AssetManager.EnumerateRes().Where(x=>x.Name.ToLower().Contains("splashscreen")))
            {
                File.WriteAllText($"Debugging/RES/{res.DisplayName}", JsonConvert.SerializeObject(res));
               var resStream = projectManagement.FrostyProject.AssetManager.GetRes(res);
                Texture textureAsset = new Texture(resStream, projectManagement.FrostyProject.AssetManager);
                new TextureExporter().Export(textureAsset, $"G:\\{res.Filename}.PNG", "*.png");
                new TextureExporter().Export(textureAsset, $"G:\\{res.Filename}.DDS", "*.dds");
                if (res.Filename == "splashscreen")
                {
                    var linked = res.LinkedAssets;
                   // new TextureImporter().ImportTextureFromFile("G:\\splashscreen_v2k4.DDS", textureAsset, res, projectManagement.FrostyProject.AssetManager, out string errorMessage);
                    //if (errorMessage != string.Empty)
                    //{

                    //}
                }

                //if (res.Filename == "mainmenusplashscreen")
                //{
                //    var linked = res.LinkedAssets;
                //    new TextureImporter().ImportTextureFromFile("G:\\mainmenusplashscreen_v2k4.DDS", textureAsset, res, projectManagement.FrostyProject.AssetManager, out string errorMessage);
                //    if (errorMessage != string.Empty)
                //    {

                //    }
                //}
            }
            projectManagement.FrostyProject.WriteToMod("TestFullMod.fbmod", new ModSettings() { Title = "v2k4 Test Full Mod", Author = "paulv2k4", Version = "1.00" });

            //var r = LaunchFIFA.LaunchAsync(
            //   FIFAInstanceSingleton.FIFARootPath
            //   , ""
            //   , new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }
            //   , this
            //   , FIFAInstanceSingleton.FIFAVERSION
            //   , true).Result;


            if (!ProfilesLibrary.Initialize(FIFAInstanceSingleton.FIFAVERSION))
            {
                throw new Exception("Unable to Initialize Profile");
            }
            FileSystem fileSystem = new FileSystem(FIFAInstanceSingleton.FIFARootPath);
            foreach (FileSystemSource source in ProfilesLibrary.Sources)
            {
                fileSystem.AddSource(source.Path, source.SubDirs);
            }
            fileSystem.Initialize();
            var fme = new FrostyModExecutor();
            //var result = fme.BuildModData(fileSystem, this, "", "", new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }.ToArray()).Result;
            var result = fme.Run(fileSystem, this, "", "", new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }.ToArray()).Result;



        }

        [TestMethod]
        public void CreateProjectAndInsertLegacyFilesTest()
        {
            InitializeOfSelectedGame(@"E:\Origin Games\FIFA 20\FIFA20.exe");

            ProjectManagement projectManagement = new ProjectManagement();
            projectManagement.StartNewProject();
            var legacyAssets = projectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy").ToList();
            var youth_scout = legacyAssets.FirstOrDefault(x => x.Name.Contains("youth_scout.ini"));
            var scout = legacyAssets.FirstOrDefault(x => x.Name.EndsWith("scout.ini"));
            var player_values = legacyAssets.Where(x => x.Name.Contains("playervalues.ini")).ToList();
            // {ce5081ca-ace4-39e9-8d60-5487f9502877}
            //legacyAssets.Where(x=>x.)

            var splash = legacyAssets.FirstOrDefault(x => x.Name.Contains("PressStart.dds"));


            byte[] data = null;
            //using (NativeReader nativeReader = new NativeReader(new FileStream("TestData\\CareerFiles\\youth_scout.ini", FileMode.Open, FileAccess.Read)))
            //{
            //    data = nativeReader.ReadToEnd();
            //}
            //projectManagement.FrostyProject.AssetManager.ModifyCustomAsset("legacy", youth_scout.Name, data);


            //using (NativeReader nativeReader = new NativeReader(new FileStream("TestData\\CareerFiles\\scout.ini", FileMode.Open, FileAccess.Read)))
            //{
            //    data = nativeReader.ReadToEnd();
            //}
            //projectManagement.FrostyProject.AssetManager.ModifyCustomAsset("legacy", scout.Name, data);

            using (NativeReader nativeReader = new NativeReader(new FileStream("TestData\\CareerFiles\\playervalues.ini", FileMode.Open, FileAccess.Read)))
            {
                data = nativeReader.ReadToEnd();
            }
            foreach (var i in player_values)
            {
                projectManagement.FrostyProject.AssetManager.ModifyCustomAsset("legacy", @"dlc/dlc_FootballCompEng/dlc/FootballCompEng/data/playervalues.ini", data);
                projectManagement.FrostyProject.AssetManager.SendManagerCommand("legacy", "FlushCache");
            }

            using (NativeReader nativeReader = new NativeReader(new FileStream(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\data\ui\imgAssets\bootflow\PressStart.DDS", FileMode.Open, FileAccess.Read)))
            {
                data = nativeReader.ReadToEnd();
            }

            data = File.ReadAllBytes(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\Source\data\ui\imgAssets\bootflow\PressStart.DDS");
            projectManagement.FrostyProject.AssetManager.ModifyCustomAsset("legacy", splash.Name, data);
            projectManagement.FrostyProject.AssetManager.SendManagerCommand("legacy", "FlushCache");

            if (File.Exists("test.project"))
            {
                File.Delete("test.project");
            }
            if (File.Exists("TestMod.fbmod"))
            {
                File.Delete("TestMod.fbmod");
            }
            projectManagement.FrostyProject.Save("test.project", true);
            projectManagement.FrostyProject.WriteToMod("TestMod.fbmod", new ModSettings() { Title = "v2k4 Test Mod", Author = "paulv2k4", Version = "1.00" });

            var r = LaunchFIFA.LaunchAsync(
                FIFAInstanceSingleton.FIFARootPath
                , ""
                , new System.Collections.Generic.List<string>() { @"G:\Work\FIFA Modding\FIFAModdingUI\FIFALibraryNETFrameworkTests\bin\Debug\TestMod.fbmod" }
                , this
                , FIFAInstanceSingleton.FIFAVERSION
                , true).Result;

        }

        [TestMethod]
        public void CreateProjectAndEditGameplayTest()
        {
            InitializeOfSelectedGame(@"E:\Origin Games\Madden NFL 21\Madden21.exe");

            ProjectManagement projectManagement = new ProjectManagement();
            projectManagement.StartNewProject();
        }

        private string lastMessage = null;

        public void Log(string text, params object[] vars)
        {
            if(lastMessage != text)
                Debug.WriteLine(text);

            lastMessage = text;
        }

        public void LogWarning(string text, params object[] vars)
        {
            Debug.WriteLine(text);
        }

        public void LogError(string text, params object[] vars)
        {
            Debug.WriteLine(text);
        }
    }
}
