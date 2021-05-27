using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frosty
{
    public class NullLogger : ILogger
    {
        public void Log(string text, params object[] vars)
        {
            Console.WriteLine(text);
            Trace.WriteLine(text);
        }

        public void LogError(string text, params object[] vars)
        {
            Console.WriteLine(text);
            Trace.WriteLine(text);
        }

        public void LogWarning(string text, params object[] vars)
        {
            Console.WriteLine(text);
            Trace.WriteLine(text);
        }
    }

    public class OpenFrostyFiles
    {

        public void Extract()
        {
            if (ProfilesLibrary.Initialize("FIFA20"))
            {
                if (ProfilesLibrary.RequiresKey)
                {
                    byte[] array;

                    array = NativeReader.ReadInStream(new FileStream(ProfilesLibrary.CacheName + ".key", FileMode.Open, FileAccess.Read));
                    byte[] array2 = new byte[16];
                    Array.Copy(array, array2, 16);
                    KeyManager.Instance.AddKey("Key1", array2);
                    if (array.Length > 16)
                    {
                        array2 = new byte[16];
                        Array.Copy(array, 16, array2, 0, 16);
                        KeyManager.Instance.AddKey("Key2", array2);
                        array2 = new byte[16384];
                        Array.Copy(array, 32, array2, 0, 16384);
                        KeyManager.Instance.AddKey("Key3", array2);
                    }

                    TypeLibrary.Initialize();

                    ILogger logger = new NullLogger();
                    AssetManagerImportResult result = new AssetManagerImportResult();
                    LoadData(logger, KeyManager.Instance.GetKey("Key1"), result);
                    var ebx = AssetManager.Instance.GetEbxEntry("fifa/attribulator/gameplay/groups/gp_actor/gp_actor_movement_runtime");
                    if (ebx != null)
                    {

                    }
                    //var legacyfiles = AssetManager.EnumerateCustomAssets("legacy").Where(x => x.Type == "INI" || x.Type == "CSV");//.Select(x => x.Filename);
                    //foreach (var f in legacyfiles)
                    //{
                    //    using (var str = AssetManager.GetCustomAsset("legacy"
                    //   , AssetManager.GetCustomAssetEntry("legacy", f.Name)))
                    //    {
                    //        var saveToPath = "EditorMod\\Legacy\\" + f.Name;
                    //        if (!Directory.Exists("EditorMod\\Legacy\\"))
                    //            Directory.CreateDirectory("EditorMod\\Legacy\\");

                    //        if (saveToPath.Contains("/"))
                    //            Directory.CreateDirectory(saveToPath.Substring(0, saveToPath.LastIndexOf('/')));

                    //        using (FileStream fs = new FileStream(saveToPath, FileMode.OpenOrCreate))
                    //        {
                    //            str.CopyTo(fs);
                    //            fs.Position = 0;
                    //        }
                    //    }
                    //}

                    GC.Collect();
                }
            }
        }

        public static void ExportMod(ModSettings modSettings, string filename, bool bSilent)
        {
            var project = new FrostbiteProject(AssetManager.Instance, new FileSystem(@"E:\Origin Games\FIFA 20\"));
            project.WriteToMod(filename, modSettings);
            if (!bSilent)
            {
            }
        }

        private static int LoadData(ILogger logger, byte[] key, AssetManagerImportResult result)
        {
            var FileSystem = new FileSystem(@"E:\Origin Games\FIFA 20\");
            foreach (FileSystemSource source in ProfilesLibrary.Sources)
            {
                FileSystem.AddSource(source.Path, source.SubDirs);
            }
            FileSystem.Initialize(key);
            var ResourceManager = new ResourceManager(FileSystem);
            ResourceManager.SetLogger(logger);
            ResourceManager.Initialize();
            AssetManager.Instance = new AssetManager(FileSystem, ResourceManager);
            //LegacyFileManager.AssetManager = AssetManager;
            //    AssetManager.RegisterCustomAssetManager("legacy", typeof(LegacyFileManager));
            AssetManager.Instance.RegisterLegacyAssetManager();
            AssetManager.Instance.SetLogger(logger);
            AssetManager.Instance.Initialize(additionalStartup: true, result);
            return 0;
        }

        private static int LoadLocalizedStringResourceTables(ILogger logger)
        {
            logger.Log("Loading localized strings");
            //LocalizedStringDatabase.Current.Initialize();
            return 0;
        }

        private static int LoadStringList(ILogger logger)
        {
            logger.Log("Loading custom strings");
            Utils.GetString(0);
            return 0;
        }

        
    }
}
