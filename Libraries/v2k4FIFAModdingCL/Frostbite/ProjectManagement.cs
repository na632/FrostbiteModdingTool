using FrostySdk;
using FrostySdk.Frostbite;
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
using v2k4FIFAModdingCL;
using v2k4FIFASDKGenerator;

namespace v2k4FIFAModding.Frosty
{
    public class ProjectManagement : ILogger
    {
        public static ProjectManagement Instance;
        private static void InitializeOfSelectedGame(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("File path / Game EXE doesn't exist");

                var FIFADirectory = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                GameInstanceSingleton.GAMERootPath = FIFADirectory;
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                GameInstanceSingleton.GAMEVERSION = fileName.Replace(".exe", "");
                if (!ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
                {
                    throw new Exception("Unable to Initialize Profile");
                }
            }
            else
            {
                throw new FileNotFoundException("Empty Game Path given!");
            }
        }

        private static string PreviousGameVersion { get; set; }

        public ProjectManagement()
        {
            Initialize();
            Instance = this;
        }

        private void Initialize()
        {
            if (string.IsNullOrEmpty(GameInstanceSingleton.GAMERootPath))
                throw new Exception("Game path has not been selected or initialized");

            if (string.IsNullOrEmpty(GameInstanceSingleton.GAMEVERSION))
                throw new Exception("Game EXE has not been selected or initialized");

            if (PreviousGameVersion != GameInstanceSingleton.GAMEVERSION || AssetManager.Instance == null)
            {
                var buildCache = new BuildCache();
                buildCache.LoadData(GameInstanceSingleton.GAMEVERSION, GameInstanceSingleton.GAMERootPath, logger: this, loadSDK: true);
                //if (!File.Exists(FIFAInstanceSingleton.V))
                //    var buildSDK = new BuildSDK();
                //var b = buildSDK.Build().Result;
                PreviousGameVersion = GameInstanceSingleton.GAMEVERSION;
            }
        }

        public ProjectManagement(string gamePath)
        {
            InitializeOfSelectedGame(gamePath);
            Initialize();
            Instance = this;
        }

        public ProjectManagement(string gamePath, ILogger logger)
            //: this(gamePath)
        {
            Logger = logger;
            InitializeOfSelectedGame(gamePath);
            Initialize();
            Instance = this;
        }

        public FrostbiteProject FrostyProject = null;
        public string FilePath = null;

        public ILogger Logger = null;

        string lastMessage = null;

        public static void ClearCurrentConsoleLine()
        {
            //int currentLineCursor = Console.CursorTop;
            //Console.SetCursorPosition(0, Console.CursorTop);
            //Console.Write(new string(' ', Console.WindowWidth));
            //Console.SetCursorPosition(0, currentLineCursor);
        }

        public void Log(string text, params object[] vars)
        {
            if(text != lastMessage)
            {
                Debug.WriteLine(text);

                ClearCurrentConsoleLine();
                Console.WriteLine(text);
                lastMessage = text;

                if(Logger != null)
                {
                    Logger.Log(text, vars);
                }
            }
        }


        public void LogError(string text, params object[] vars)
        {
            if (text != lastMessage)
            {
                Debug.WriteLine(text);

                ClearCurrentConsoleLine();
                Console.WriteLine(text);
                lastMessage = text;

                if (Logger != null)
                {
                    Logger.LogWarning(text, vars);
                }
            }
        }

        public void LogWarning(string text, params object[] vars)
        {
            if (text != lastMessage)
            {
                Debug.WriteLine(text);

                ClearCurrentConsoleLine();
                Console.WriteLine(text);
                lastMessage = text;

                if (Logger != null)
                {
                    Logger.LogError(text, vars);
                }
            }
        }

        /// <summary>
        /// Starts the process of Loading Cache etc and Creates a New Project
        /// </summary>
        /// <returns></returns>
        public async Task<FrostbiteProject> StartNewProjectAsync()
        {
            return await new TaskFactory().StartNew(() =>
            {
                FrostyProject = new FrostbiteProject(AssetManager.Instance, AssetManager.Instance.fs);
                return FrostyProject;
            });
        }

        /*
         * 
         * 
         * 
         * App.FileSystem = new FileSystem(Config.Get("Init", "GamePath", ""));
				foreach (FileSystemSource source in ProfilesLibrary.Sources)
				{
					App.FileSystem.AddSource(source.Path, source.SubDirs);
				}
				App.FileSystem.Initialize(key);
				App.ResourceManager = new ResourceManager(App.FileSystem);
				App.ResourceManager.SetLogger(logger);
				App.ResourceManager.Initialize();
				App.AssetManager = new AssetManager(App.FileSystem, App.ResourceManager);
				if (ProfilesLibrary.DataVersion == 20160927 || ProfilesLibrary.DataVersion == 20170929 || ProfilesLibrary.DataVersion == 20180807 || ProfilesLibrary.DataVersion == 20180914 || ProfilesLibrary.DataVersion == 20190729 || ProfilesLibrary.DataVersion == 20190911 || ProfilesLibrary.DataVersion == 20190905)
				{
					App.AssetManager.RegisterCustomAssetManager("legacy", typeof(LegacyFileManager));
				}
				App.AssetManager.SetLogger(logger);
				App.AssetManager.Initialize(additionalStartup: true, result);
         * 
         */


        public FrostbiteProject StartNewProject()
        {
            if(AssetManager.Instance == null)
            {
                BuildCache buildCache = new BuildCache();
                buildCache.LoadData(GameInstanceSingleton.GAMEVERSION, GameInstanceSingleton.GAMERootPath, Logger, false, true);
            }

            FrostyProject = new FrostbiteProject(AssetManager.Instance, AssetManager.Instance.fs);
            return FrostyProject;
            //if (ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
            //{
            //    PreviousGameVersion = GameInstanceSingleton.GAMEVERSION;

            //    if (KeyManager.Instance.ReadInKeys())
            //    {
            //        if (TypeLibrary.Initialize())
            //        {
            //            var FileSystem = new FrostySdk.FileSystem(GameInstanceSingleton.GAMERootPath);
            //            foreach (FileSystemSource source in ProfilesLibrary.Sources)
            //            {
            //                FileSystem.AddSource(source.Path, source.SubDirs);
            //            }
            //            FileSystem.Initialize(KeyManager.Instance.GetKey("Key1"));
            //            var ResourceManager = new ResourceManager(FileSystem);
            //            ResourceManager.SetLogger(Logger ?? this);
            //            ResourceManager.Initialize();
            //            AssetManager = new AssetManager(FileSystem, ResourceManager);
            //            //LegacyFileManager.AssetManager = AssetManager;
            //            //AssetManager.RegisterCustomAssetManager("legacy", typeof(LegacyFileManager));
            //            AssetManager.RegisterLegacyAssetManager();
            //            AssetManager.SetLogger(Logger ?? this);
            //            AssetManager.Initialize(additionalStartup: true);

            //            FrostyProject = new FrostyProject(AssetManager, FileSystem);
            //            return FrostyProject;
            //        }
            //        else
            //        {
            //            Debug.WriteLine("Could Init Type Library");
            //        }
            //    }
            //    else
            //    {
            //        Debug.WriteLine("Could not read in keys");
            //    }
            //}
            //else
            //{
            //    Debug.WriteLine("Couldn't find FIFA Version");
            //}

            return null;
        }

    }
}
