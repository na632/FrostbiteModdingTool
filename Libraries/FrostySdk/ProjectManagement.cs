using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using System;
using System.Diagnostics;
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


        private static string PreviousGameVersion { get; set; }

        //public ProjectManagement()
        //{
        //    if (AssetManager.Instance == null)
        //        throw new NullReferenceException("AssetManager Instance must be set before ProjectManagement can be used!");

        //    if (Instance == null)
        //    {
        //        Instance = this;
        //    }
        //    else
        //    {
        //        throw new OverflowException("Cannot create 2 instances of ProjectManagement");
        //    }
        //}

        public ProjectManagement(string gamePath)
        {
            if (AssetManager.Instance == null)
                throw new NullReferenceException("AssetManager Instance must be instantiated before ProjectManagement can be used!");

            if (Instance == null)
            {
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
            if (AssetManager.Instance == null)
                throw new NullReferenceException("AssetManager Instance must be instantiated before ProjectManagement can be used!");

            if (Instance == null)
            {
                Logger = logger;
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
            if (text != lastMessage)
            {
                Debug.WriteLine(text);

                ClearCurrentConsoleLine();
                Console.WriteLine(text);
                lastMessage = text;

                if (Logger != null)
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
            if (AssetManager.Instance == null)
            {
                CacheManager buildCache = new CacheManager();
                buildCache.LoadData(GameInstanceSingleton.Instance.GAMEVERSION, GameInstanceSingleton.Instance.GAMERootPath, Logger, false, true);
            }

            Project = new FrostbiteProject(AssetManager.Instance, AssetManager.Instance.fs);
            return Project;
        }

    }
}
