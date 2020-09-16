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
using v2k4FIFAModdingCL;
using v2k4FIFASDKGenerator;

namespace v2k4FIFAModding.Frosty
{
    public class ProjectManagement : ILogger
    {
        public ProjectManagement()
        {
            var buildCache = new BuildCache();
            buildCache.LoadData(GameInstanceSingleton.GAMEVERSION, GameInstanceSingleton.GAMERootPath, loadSDK: true);
            //if(!File.Exists(FIFAInstanceSingleton.V))
            //var buildSDK = new BuildSDK();
            //var b = buildSDK.Build().Result;
        }

        public FrostyProject FrostyProject = null;
        AssetManager AssetManager = null;

        public ILogger Logger = null;

        public void Log(string text, params object[] vars)
        {

        }

        public void LogError(string text, params object[] vars)
        {

        }

        public void LogWarning(string text, params object[] vars)
        {

        }

        /// <summary>
        /// Starts the process of Loading Cache etc and Creates a New Project
        /// </summary>
        /// <returns></returns>
        public async Task<FrostyProject> StartNewProjectAsync()
        {
            return await new TaskFactory().StartNew(() =>
            {

                return StartNewProject();
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

        public FrostyProject StartNewProject()
        {
            if (ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
            {
                if (KeyManager.Instance.ReadInKeys())
                {
                    if (TypeLibrary.Initialize())
                    {
                        var FileSystem = new FrostySdk.FileSystem(GameInstanceSingleton.GAMERootPath);
                        foreach (FileSystemSource source in ProfilesLibrary.Sources)
                        {
                            FileSystem.AddSource(source.Path, source.SubDirs);
                        }
                        FileSystem.Initialize(KeyManager.Instance.GetKey("Key1"));
                        var ResourceManager = new ResourceManager(FileSystem);
                        ResourceManager.SetLogger(Logger ?? this);
                        ResourceManager.Initialize();
                        AssetManager = new AssetManager(FileSystem, ResourceManager);
                        //LegacyFileManager.AssetManager = AssetManager;
                        //AssetManager.RegisterCustomAssetManager("legacy", typeof(LegacyFileManager));
                        AssetManager.RegisterLegacyAssetManager();
                        AssetManager.SetLogger(Logger ?? this);
                        AssetManager.Initialize(additionalStartup: true);

                        FrostyProject = new FrostyProject(AssetManager, FileSystem);
                        return FrostyProject;
                    }
                    else
                    {
                        Debug.WriteLine("Could Init Type Library");
                    }
                }
                else
                {
                    Debug.WriteLine("Could not read in keys");
                }
            }
            else
            {
                Debug.WriteLine("Couldn't find FIFA Version");
            }

            return null;
        }

    }
}
