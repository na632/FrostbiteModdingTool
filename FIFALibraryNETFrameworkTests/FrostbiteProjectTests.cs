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
using paulv2k4FrostyModdingSupport.FrostbiteModExecuters.BundleActions;
using paulv2k4ModdingExecuter;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;
using static paulv2k4ModdingExecuter.FrostyModExecutor;

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
                GameInstanceSingleton.GAMERootPath = FIFADirectory;
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                GameInstanceSingleton.GAMEVERSION = fileName.Replace(".exe", "");
                if (!ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
                {
                    throw new Exception("Unable to Initialize Profile");
                }
            }
        }

        [TestMethod]
        public void LoadProjectAndRun_FIFA20()
        {
            InitializeOfSelectedGame(@"E:\Origin Games\FIFA 20\FIFA20.exe");
            ProjectManagement projectManagement = new ProjectManagement();
            projectManagement.StartNewProject();
            projectManagement.FrostyProject.Load(@"G:\Work\FIFA Modding\Career Mod\FIFA-20-Career-Mod\v2k4CareerMod.fbproject");
            projectManagement.FrostyProject.WriteToMod("TestFullMod.fbmod", new ModSettings() { Title = "v2k4 Test Full Mod", Author = "paulv2k4", Version = "1.00" });

            //var r = LaunchFIFA.LaunchAsync(
            //   FIFAInstanceSingleton.FIFARootPath
            //   , ""
            //   , new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }
            //   , this
            //   , FIFAInstanceSingleton.FIFAVERSION
            //   , true).Result;

            if (!ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
            {
                throw new Exception("Unable to Initialize Profile");
            }
            FileSystem fileSystem = new FileSystem(GameInstanceSingleton.GAMERootPath);
            foreach (FileSystemSource source in ProfilesLibrary.Sources)
            {
                fileSystem.AddSource(source.Path, source.SubDirs);
            }
            fileSystem.Initialize();
            var fme = new FrostyModExecutor();
            var result = fme.BuildModData(fileSystem, this, "", "", new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }.ToArray()).Result;
            //var result = fme.Run(fileSystem, this, "", "", new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }.ToArray()).Result;
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
        public void LoadProjectAndExtractARES_FIFA20()
        {
            if (!Directory.Exists("Debugging\\RES\\"))
                Directory.CreateDirectory("Debugging\\RES\\");

            InitializeOfSelectedGame(@"E:\Origin Games\FIFA 20\FIFA20.exe");
            ProjectManagement projectManagement = new ProjectManagement();
            projectManagement.StartNewProject();
            var allRES = projectManagement.FrostyProject.AssetManager.EnumerateRes().ToList();
            var search = allRES.Where(x => x.Filename.Contains("211110")).ToList();
            if(search != null && search.Count() > 0)
            {
                foreach (var r in search) 
                {
                    var path = projectManagement.FrostyProject.FileSystem.ResolvePath(r.ExtraData.CasPath);

                    using (NativeReader reader = new NativeReader(new FileStream(path, FileMode.OpenOrCreate)))
                    {
                        reader.BaseStream.Seek(r.ExtraData.DataOffset, SeekOrigin.Begin);

                        using (NativeReader viewStreamReader = new NativeReader(reader.CreateViewStream(r.ExtraData.DataOffset, r.Size)))
                        {
                            using (NativeWriter writer = new NativeWriter(new FileStream("Debugging\\RES\\" + r.Filename, FileMode.OpenOrCreate)))
                            {
                                writer.Write(viewStreamReader.ReadToEnd());
                            }
                        }
                    }

                    
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
                    foreach (var o in ebx.Objects)
                    {

                    }
                    var robjProps = GetRootObjectProperties(ebx.RootObject);
                    var r = robjProps.Where(x => x.Item1.Contains("run_indication_"));
                    foreach (var it in r)
                    {
                        var nIt3 = new
                        {
                            x = 0.0,
                            y = 0.0,
                            z = 1.0,
                            w = 1.0
                        };
                    }

                    //var root = ((dynamic)ebx.RootObject) as dynamic;
                    //root.run_indication_indicator_color.x = 0.0f;
                    //root.run_indication_indicator_color.y = 0.0f;
                    //root.run_indication_indicator_color.z = 1.0f;

                    //        //root.run_indication_indicator_color_non_loco.x = 0.0f;
                    //        //root.run_indication_indicator_color_non_loco.y = 0.0f;
                    //        //root.run_indication_indicator_color_non_loco.z = 1.0f;

                    //        //root.run_indication_indicator_color_intersection.x = 0.0f;
                    //        //root.run_indication_indicator_color_intersection.y = 0.0f;
                    //        //root.run_indication_indicator_color_intersection.z = 1.0f;

                    //        ((dynamic)ebx.RootObject).run_indication_debug_show_line = true;
                    //        //getEbx_movement.Objects.Count(x=>x.)
                    //        //getEbx_movement.AddRootObject(root);
                    //        //getEbx_movement.RemoveObject(root);
                    //        //ebx.RemoveObject(root);
                    //        //ebx.AddRootObject(root);
                    //        //getEbx_movement.AddRootObject(root);
                    //        //projectManagement.FrostyProject.AssetManager.AddEbx(eb.Name, ebx, 0);
                    //projectManagement.FrostyProject.AssetManager.ModifyEbx(eb.Name, ebx);

                    //        File.WriteAllText($"Debugging/EBX/{eb.Filename}.dat", JsonConvert.SerializeObject(robjProps));
                    //        Assert.IsNotNull(robjProps);
                    }
                  }

            var allRes = projectManagement.FrostyProject.AssetManager.EnumerateRes().ToList();
            var ebxofres = allEBX.Where(x => x.DisplayName.ToLower().Contains("splashscreen")).ToList();

            foreach (var res in projectManagement.FrostyProject.AssetManager.EnumerateRes().Where(x => x.Name.ToLower().Contains("splashscreen")).ToList())
            {
                File.WriteAllText($"Debugging/RES/{res.DisplayName}", JsonConvert.SerializeObject(res));
                var resStream = projectManagement.FrostyProject.AssetManager.GetRes(res);
                Texture textureAsset = new Texture(resStream, projectManagement.FrostyProject.AssetManager);
                //new TextureExporter().Export(textureAsset, $"G:\\{res.Filename}.PNG", "*.png");
                new TextureExporter().Export(textureAsset, $"G:\\{res.Filename}.DDS", "*.dds");
                if (res.Filename == "splashscreen")
                {
                    var linked = res.LinkedAssets;
                    new TextureImporter().ImportTextureFromFile("G:\\splashscreen_v2k4.DDS", textureAsset, res, projectManagement.FrostyProject.AssetManager, out string errorMessage);
                    //new TextureImporter().ImportTextureFromFile("G:\\splashscreen_v2k4.png", textureAsset, res, projectManagement.FrostyProject.AssetManager, out string errorMessage);
                    //if (errorMessage != string.Empty)
                    //{

                    //}
                }
            }
            projectManagement.FrostyProject.WriteToMod("TestFullMod.fbmod", new ModSettings() { Title = "v2k4 Test Full Mod", Author = "paulv2k4", Version = "1.00" });

            //var r = LaunchFIFA.LaunchAsync(
            //   FIFAInstanceSingleton.FIFARootPath
            //   , ""
            //   , new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }
            //   , this
            //   , FIFAInstanceSingleton.FIFAVERSION
            //   , true).Result;


            if (!ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
            {
                throw new Exception("Unable to Initialize Profile");
            }
            FileSystem fileSystem = new FileSystem(GameInstanceSingleton.GAMERootPath);
            foreach (FileSystemSource source in ProfilesLibrary.Sources)
            {
                fileSystem.AddSource(source.Path, source.SubDirs);
            }
            fileSystem.Initialize();
            var fme = new FrostyModExecutor();
            //var result = fme.BuildModData(fileSystem, this, "", "", new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }.ToArray()).Result;
            var result = fme.Run(fileSystem, this, "", "", new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }.ToArray()).Result;

            //var result = fme.Run(fileSystem, this, "", "", new System.Collections.Generic.List<string>() { }.ToArray()).Result;


        }

        [TestMethod]
        public void LoadProjectAndTest_Madden21()
        {
            InitializeOfSelectedGame(@"E:\Origin Games\Madden NFL 21\Madden21.exe");
            ProjectManagement projectManagement = new ProjectManagement();
            projectManagement.StartNewProject();

            var alllegacy = projectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy").ToList();
            var allEBX = projectManagement.FrostyProject.AssetManager.EnumerateEbx(includeLinked: true).ToList();
            var ebxofsplash = allEBX.Where(x => x.Path.ToLower().EndsWith("splashscreen")).ToList()[1];
            var allRes = projectManagement.FrostyProject.AssetManager.EnumerateRes().ToList();
            var ebxofres = allEBX.Where(x => x.DisplayName.ToLower().Contains("splashscreen")).ToList();
            foreach (var res in projectManagement.FrostyProject.AssetManager.EnumerateRes().Where(x => x.Name.ToLower().Contains("splashscreen")).ToList())
            {
                File.WriteAllText($"Debugging/RES/{res.DisplayName}", JsonConvert.SerializeObject(res));
                var resStream = projectManagement.FrostyProject.AssetManager.GetRes(res);
                Texture textureAsset = new Texture(resStream, projectManagement.FrostyProject.AssetManager);
                new TextureExporter().Export(textureAsset, $"G:\\{res.Filename}.DDS", "*.dds");
                if (res.Filename == "splashscreen")
                {
                    var linked = res.LinkedAssets;
                    //new TextureImporter().ImportTextureFromFile("G:\\splashscreen_v2k4.DDS", textureAsset, res, projectManagement.FrostyProject.AssetManager, out string errorMessage);
                    new TextureImporter().ImportTextureFromFileToTextureAsset_Original(
                         @"G:\splashscreen_v2k4.DDS"
                         , ebxofsplash
                         , projectManagement.FrostyProject.AssetManager
                         , ref textureAsset
                         , out string message);

                }
            }
            projectManagement.FrostyProject.WriteToMod("TestFullMod.fbmod", new ModSettings() { Title = "v2k4 Test Full Mod", Author = "paulv2k4", Version = "1.00" });

            if (!ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
            {
                throw new Exception("Unable to Initialize Profile");
            }
            FileSystem fileSystem = new FileSystem(GameInstanceSingleton.GAMERootPath);
            foreach (FileSystemSource source in ProfilesLibrary.Sources)
            {
                fileSystem.AddSource(source.Path, source.SubDirs);
            }
            fileSystem.Initialize();
            var fme = new FrostyModExecutor();
            var result = fme.BuildModData(fileSystem, this, "", "", new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }.ToArray()).Result;

            foreach (CatalogInfo catalogItem in fileSystem.EnumerateCatalogInfos())
            {
                Madden21BundleAction maddenBundleAction = new Madden21BundleAction(catalogItem, null, fme);
                foreach (string sb_toc_file in catalogItem.SuperBundles.Keys)
                {
                    string sb_toc_file_path_cleaned = sb_toc_file;
                    if (catalogItem.SuperBundles[sb_toc_file])
                    {
                        sb_toc_file_path_cleaned = sb_toc_file.Replace("win32", catalogItem.Name);
                    }
                    string location_toc_file = fme.fs.ResolvePath($"{sb_toc_file_path_cleaned}.toc").ToLower();
                    location_toc_file = location_toc_file.Replace("\\patch", "\\ModData\\Patch");

                    if (location_toc_file != "" && File.Exists(location_toc_file))
                    {
                        // -----------------------------------------------------------------------------------------
                        // Read Original Toc File
                        var toc_array = maddenBundleAction.ReadTocIntoByteArray(location_toc_file, out int toc_starting_position, out int other_starting_position, out byte[] tocSbHeader);
                        if (toc_array.Length != 0)
                        {
                            maddenBundleAction.GetModifiedBundles(toc_array
                                , out List<Madden21BundleAction.BundleFileEntry> list_bundle_entries
                                , out Dictionary<ModBundleInfo, List<Madden21BundleAction.BundleFileEntry>> list_modified_bundles
                                , out List<int> IsEBXList);

                            Assert.IsTrue(list_bundle_entries.Count > 0);



                        }
                    }
                }
            }
        }


        [TestMethod]
        public void LoadProjectAndRun_Madden21_OverwriteCAS()
        {
            InitializeOfSelectedGame(@"E:\Origin Games\Madden NFL 21\Madden21.exe");
            ProjectManagement projectManagement = new ProjectManagement();
            projectManagement.StartNewProject();

            var allRes = projectManagement.FrostyProject.AssetManager.EnumerateRes().ToList();

            var chunk_casPath = string.Empty;
            foreach (var res in projectManagement.FrostyProject.AssetManager.EnumerateRes().Where(x => x.Name.ToLower().Contains("content/ui/splashscreen/splashscreen")).ToList())
            {
                //File.WriteAllText($"Debugging/RES/{res.DisplayName}", JsonConvert.SerializeObject(res));

                var res_casPath = res.ExtraData.CasPath;
                var res_casOffset = res.ExtraData.DataOffset;
                var res_casSize = res.Size;

                var resStream = projectManagement.FrostyProject.AssetManager.GetRes(res);
                Texture textureAsset = new Texture(resStream, projectManagement.FrostyProject.AssetManager);
                //new TextureExporter().Export(textureAsset, $"G:\\{res.Filename}.PNG", "*.png");
                new TextureExporter().Export(textureAsset, $"G:\\{res.Filename}.DDS", "*.dds");
                //if (res.Filename == "splashscreen")
                //{
                    var linked = res.LinkedAssets;
                new TextureImporter().ImportTextureFromFileToTextureAsset("G:\\splashscreen_v2k4.DDS", ref textureAsset, out string errorMessage);

                chunk_casPath = textureAsset.ChunkEntry.ExtraData.CasPath;
                var chunk_casOffset = textureAsset.ChunkEntry.ExtraData.DataOffset;
                var chunk_casSize = textureAsset.ChunkEntry.Size;

                var aobBuffer = new byte[textureAsset.LogicalSize];
                var d = textureAsset.Data.Read(aobBuffer, 0, (int)textureAsset.LogicalSize);
                var c0 = Utils.CompressTexture(aobBuffer, textureAsset);
                var c1 = Utils.CompressTexture(aobBuffer, textureAsset, CompressionType.Default);
                var c2 = Utils.CompressTexture(aobBuffer, textureAsset, CompressionType.Oodle);
                var c3 = Utils.CompressTexture(aobBuffer, textureAsset, CompressionType.None);
                var c4 = Utils.CompressTexture(aobBuffer, textureAsset, CompressionType.ZStd);
                File.Copy(projectManagement.FrostyProject.FileSystem.ResolvePath(chunk_casPath) + ".bak", "test.cas", true);
                NativeWriter nw = new NativeWriter(new FileStream("test.cas", FileMode.Open));
                nw.Seek(Convert.ToInt32(chunk_casOffset), SeekOrigin.Begin);
                nw.Write(c0);
                nw.Flush();
                nw.Close();
            }
            projectManagement.FrostyProject = null;
            projectManagement = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();


            //projectManagement.FrostyProject.WriteToMod("TestFullMod.fbmod", new ModSettings() { Title = "v2k4 Test Full Mod", Author = "paulv2k4", Version = "1.00" });

            //var r = LaunchFIFA.LaunchAsync(
            //   FIFAInstanceSingleton.FIFARootPath
            //   , ""
            //   , new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }
            //   , this
            //   , FIFAInstanceSingleton.FIFAVERSION
            //   , true).Result;


            //if (!ProfilesLibrary.Initialize(GameInstanceSingleton.FIFAVERSION))
            //{
            //    throw new Exception("Unable to Initialize Profile");
            //}
            //FileSystem fileSystem = new FileSystem(GameInstanceSingleton.FIFARootPath);
            //foreach (FileSystemSource source in ProfilesLibrary.Sources)
            //{
            //    fileSystem.AddSource(source.Path, source.SubDirs);
            //}
            //fileSystem.Initialize();
            //var fme = new FrostyModExecutor();
            ////var result = fme.BuildModData(fileSystem, this, "", "", new System.Collections.Generic.List<string>() { @"TestFullMod.fbmod" }.ToArray()).Result;
            //var result = fme.Run(fileSystem, this, "", "", new System.Collections.Generic.List<string>() {  }.ToArray()).Result;

            //var result = fme.Run(fileSystem, this, "", "", new System.Collections.Generic.List<string>() { }.ToArray()).Result;


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
                GameInstanceSingleton.GAMERootPath
                , ""
                , new System.Collections.Generic.List<string>() { @"G:\Work\FIFA Modding\FIFAModdingUI\FIFALibraryNETFrameworkTests\bin\Debug\TestMod.fbmod" }
                , this
                , GameInstanceSingleton.GAMEVERSION
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
