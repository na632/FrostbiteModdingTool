using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v2k4FIFAModdingCL;

namespace v2k4FIFAModding.Frosty
{
    public class ProjectManagement : ILogger
    {
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
        public async Task<FrostyProject> StartNewProject()
        {
            return await new TaskFactory().StartNew(() =>
            {

                if (ProfilesLibrary.Initialize("FIFA20"))
                {
                    if (KeyManager.Instance.ReadInKeys())
                    {
                        if (TypeLibrary.Initialize())
                        {
                            AssetManagerImportResult result = new AssetManagerImportResult();
                            var FileSystem = new FrostySdk.FileSystem(FIFAInstanceSingleton.FIFARootPath);
                            foreach (FileSystemSource source in ProfilesLibrary.Sources)
                            {
                                FileSystem.AddSource(source.Path, source.SubDirs);
                            }
                            FileSystem.Initialize(KeyManager.Instance.GetKey("Key1"));
                            var ResourceManager = new ResourceManager(FileSystem);
                            ResourceManager.SetLogger(Logger ?? this);
                            ResourceManager.Initialize();
                            AssetManager = new AssetManager(FileSystem, ResourceManager);
                            LegacyFileManager.AssetManager = AssetManager;
                            AssetManager.RegisterCustomAssetManager("legacy", typeof(LegacyFileManager));
                            AssetManager.SetLogger(Logger ?? this);
                            AssetManager.Initialize(additionalStartup: true, result);

                            FrostyProject = new FrostyProject(AssetManager, FileSystem);
                            return FrostyProject;
                        }
                    }
                }

                return null;
            });
        }

    }
}
