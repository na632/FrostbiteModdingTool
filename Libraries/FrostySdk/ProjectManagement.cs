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

namespace v2k4FIFAModding.Frosty
{

    /// <summary>
    /// Project Management is a singleton class. Cannot create 2 instances of this class.
    /// </summary>
    public class ProjectManagement : ILogger
    {
        public static ProjectManagement Instance;

        public FrostbiteProject Project { get; set; } = new FrostbiteProject();

        public ILogger Logger = null;

        string lastMessage = null;



        /// <summary>
        /// Sets up all the Singleton Paths
        /// </summary>
        /// <param name="filePath"></param>
        private void InitializeOfSelectedGame(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("File path / Game EXE doesn't exist");

                GameInstanceSingleton.InitializeSingleton(filePath, true, this);

                var FIFADirectory = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                GameInstanceSingleton.Instance.GAMERootPath = FIFADirectory;
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                GameInstanceSingleton.Instance.GAMEVERSION = fileName.Replace(".exe", "");
                if (!ProfilesLibrary.Initialize(GameInstanceSingleton.Instance.GAMEVERSION))
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
            if (Instance == null)
            {
                Initialize();
                Instance = this;
            }
            else
            {
                throw new OverflowException("Cannot create 2 instances of ProjectManagement");
            }
        }

        private void Initialize()
        {
            if (string.IsNullOrEmpty(GameInstanceSingleton.Instance.GAMERootPath))
                throw new Exception("Game path has not been selected or initialized");

            if (string.IsNullOrEmpty(GameInstanceSingleton.Instance.GAMEVERSION))
                throw new Exception("Game EXE has not been selected or initialized");

            if (PreviousGameVersion != GameInstanceSingleton.Instance.GAMEVERSION || AssetManager.Instance == null)
            {
                var buildCache = new BuildCache();
                buildCache.LoadData(GameInstanceSingleton.Instance.GAMEVERSION, GameInstanceSingleton.Instance.GAMERootPath, logger: this, loadSDK: true);
                PreviousGameVersion = GameInstanceSingleton.Instance.GAMEVERSION;
            }
        }

        public ProjectManagement(string gamePath)
        {
            if (Instance == null)
            {
                InitializeOfSelectedGame(gamePath);
                Initialize();
                Instance = this;
                Project = new FrostbiteProject();
            }
            else
            {
                throw new OverflowException("Cannot create 2 instances of ProjectManagement");
            }
        }

        public ProjectManagement(string gamePath, ILogger logger)
        //: this(gamePath)
        {
            if (Instance == null)
            {
                Logger = logger;
                InitializeOfSelectedGame(gamePath);
                Initialize();
                Instance = this;
                Project = new FrostbiteProject();
            }
            else
            {
                throw new OverflowException("Cannot create 2 instances of ProjectManagement");
            }
        }

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
                Project = new FrostbiteProject(AssetManager.Instance, AssetManager.Instance.fs);
                return Project;
            });
        }

        public FrostbiteProject StartNewProject()
        {
            if(AssetManager.Instance == null)
            {
                BuildCache buildCache = new BuildCache();
                buildCache.LoadData(GameInstanceSingleton.Instance.GAMEVERSION, GameInstanceSingleton.Instance.GAMERootPath, Logger, false, true);
            }

            Project = new FrostbiteProject(AssetManager.Instance, AssetManager.Instance.fs);
            return Project;
        }

    }
}
