using FIFAModdingUI;
using FIFAModdingUI.Windows;
using FMT;
using Frostbite.FileManagers;
using FrostbiteModdingUI.Models;
using FrostySdk;
using FrostySdk.FrostySdk.Managers;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;

namespace FrostbiteModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for Madden21Editor.xaml
    /// </summary>
    public partial class Madden21Editor : Window, IEditorWindow
    {
        public Window OwnerWindow { get; set; }

        public ProjectManagement ProjectManagement { get; private set; }

        private string WindowEditorTitle = $"Madden Editor - {ProfilesLibrary.ProfileName} - {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion} - ";

        private string _windowTitle;
        public string WindowTitle
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(WindowEditorTitle);
                stringBuilder.Append(_windowTitle);
                return stringBuilder.ToString();
            }
            set
            {
                _windowTitle = "[" + value + "]";
                this.DataContext = null;
                this.DataContext = this;
                this.UpdateLayout();
            }
        }

        public Madden21Editor()
        {
            //throw new Exception("Incorrect usage of Editor Windows");
        }

        public Madden21Editor(Window owner)
        {
            InitializeComponent();
            this.Closing += Madden21Editor_Closing;
            this.Loaded += Madden21Editor_Loaded;
            OwnerWindow = owner;
            Owner = owner;

            App.AppInsightClient.TrackRequest("Madden21Editor Window", DateTimeOffset.Now,
                    TimeSpan.FromMilliseconds(230), "200", true);
        }

        public string LastGameLocation => App.ApplicationDirectory + "MADDEN21LastLocation.json";

        public string RecentFilesLocation => throw new NotImplementedException();

        public void Madden21Editor_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(LastGameLocation))
            {
                var tmpLoc = File.ReadAllText(LastGameLocation);
                if (File.Exists(tmpLoc))
                {
                    AppSettings.Settings.GameInstallEXEPath = tmpLoc;
                }
                else
                {
                    File.Delete(LastGameLocation);
                }
            }

            if (!string.IsNullOrEmpty(AppSettings.Settings.GameInstallEXEPath))
            {
                InitialiseOfSelectedGame(AppSettings.Settings.GameInstallEXEPath);
            }
            else
            {
                var findGameEXEWindow = new FindGameEXEWindow();
                var result = findGameEXEWindow.ShowDialog();
                if (result.HasValue && !string.IsNullOrEmpty(AppSettings.Settings.GameInstallEXEPath))
                {
                    InitialiseOfSelectedGame(AppSettings.Settings.GameInstallEXEPath);
                }
                else
                {
                    findGameEXEWindow.Close();
                    this.Close();
                }
            }

            File.WriteAllText(LastGameLocation, AppSettings.Settings.GameInstallEXEPath);
        }

        public void Madden21Editor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (App.AppInsightClient != null)
            {
                App.AppInsightClient.Flush();
            }

            ProjectManagement.Instance = null;
            if (AssetManager.Instance != null)
            {
                AssetManager.Instance.Dispose();
                AssetManager.Instance = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            Owner.Visibility = Visibility.Visible;
        }

        public void InitialiseOfSelectedGame(string filePath)
        {
            if (!filePath.ToLower().Contains("madden"))
            {
                LogError("Wrong EXE chosen for Editor");
                return;
            }

            GameInstanceSingleton.InitializeSingleton(filePath);
            GameInstanceSingleton.Logger = this;

            bool? result = false;
            BuildSDKAndCache buildSDKAndCacheWindow = new BuildSDKAndCache();
            if (buildSDKAndCacheWindow.DoesCacheNeedsRebuilding())
            {
                result = buildSDKAndCacheWindow.ShowDialog();
            }

            LoadingDialog loadingDialog = new LoadingDialog("Loading Editor", "Starting up...");
            loadingDialog.Show();

            Task.Run(() =>
            {
                ProjectManagement = new ProjectManagement(filePath, null);
                ProjectManagement.StartNewProject();

                loadingDialog.Update("Loading Editor", "Loading Legacy Files", 50);
                // Kit Browser
                var legacyFiles = ProjectManagement.Project.AssetManager.EnumerateCustomAssets("legacy").OrderBy(x => x.Path).ToList();

                Log("Initialise Legacy Browser");
                legacyBrowser.AllAssetEntries = legacyFiles.Select(x => (IAssetEntry)x).ToList();

                loadingDialog.Update("Loading Editor", "Loading Texture Files", 75);

                Log("Initialise Texture Browser");
                textureBrowser.AllAssetEntries = ProjectManagement.Project.AssetManager
                                   .EnumerateEbx("TextureAsset").OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();


                loadingDialog.Update("Loading Editor", "Loading Data Files", 95);

                Log("Initialise Data Browser");
                dataBrowser.AllAssetEntries = ProjectManagement.Project.AssetManager
                                   .EnumerateEbx().OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();

                Log("Initialize Face Browser");
                var faceList = ProjectManagement.Project.AssetManager
                                   .EnumerateEbx().Where(x => x.Path.ToLower().Contains("characters/player/players")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();
                faceList = faceList.OrderBy(x => x.Name).ToList();
                facesBrowser.AllAssetEntries = faceList;


                loadingDialog.Update("Loading Editor", "Loading Data Files", 100);

                Log("Initialise Gameplay Browser");
                gameplayBrowser.AllAssetEntries = ProjectManagement.Project.AssetManager
                                  .EnumerateEbx()
                                  .Where(x => x.Path.ToLower().Contains("attrib")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();


                Dispatcher.InvokeAsync(() =>
                {

                    btnProjectNew.IsEnabled = true;
                    btnProjectOpen.IsEnabled = true;
                    btnProjectSave.IsEnabled = true;
                    btnProjectWriteToMod.IsEnabled = true;

                    this.DataContext = null;
                    this.DataContext = this;

                    loadingDialog.Close();

                });


            });

            var presence = new DiscordRPC.RichPresence();
            presence.State = "In Editor - " + GameInstanceSingleton.GAMEVERSION;
            App.DiscordRpcClient.SetPresence(presence);
            App.DiscordRpcClient.Invoke();
        }

        private void btnProjectWriteToMod_Click(object sender, RoutedEventArgs e)
        {
            // ---------------------------------------------------------
            // Remove chunks and actual unmodified files before writing
            LegacyFileManager_FMTV2.CleanUpChunks();

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Mod files|*.fbmod";
                var resultValue = saveFileDialog.ShowDialog();
                if (resultValue.HasValue && resultValue.Value)
                {
                    ProjectManagement.Project.WriteToMod(saveFileDialog.FileName, ProjectManagement.Project.ModSettings);
                    using (var fs = new FileStream(saveFileDialog.FileName, FileMode.Open))
                    {
                        Log("Saved mod successfully to " + saveFileDialog.FileName);
                    }
                }

            }
            catch (Exception SaveException)
            {
                LogError(SaveException.ToString());
            }
        }

        private void btnProjectSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Project files|*.fbproject";
            var result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if (!string.IsNullOrEmpty(saveFileDialog.FileName))
                {
                    ProjectManagement.Project.Save(saveFileDialog.FileName, true);

                    Log("Saved project successfully to " + saveFileDialog.FileName);


                    WindowTitle = saveFileDialog.FileName;

                }
            }
        }

        private void btnProjectOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Project files|*.fbproject";
            var result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName))
                {
                    ProjectManagement.Project.Load(openFileDialog.FileName);

                    Log("Opened project successfully from " + openFileDialog.FileName);

                    WindowTitle = openFileDialog.FileName;

                }
            }
        }

        public string LogText { get; set; }

        public void Log(string text, params object[] vars)
        {
            LogAsync(text);
        }

        public async void LogAsync(string in_text)
        {
            var txt = string.Empty;
            Dispatcher.Invoke(() => {
                txt = txtLog.Text;
            });

            var text = await Task.Run(() =>
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append(txt);
                stringBuilder.AppendLine(in_text);

                return stringBuilder.ToString();
            });

            await Dispatcher.InvokeAsync(() =>
            {
                txtLog.Text = text;
                txtLog.ScrollToEnd();
            });

        }

        public void LogWarning(string text, params object[] vars)
        {
            Debug.WriteLine("[WARNING] " + text);
            LogAsync("[WARNING] " + text);
        }

        public void LogError(string text, params object[] vars)
        {
            Debug.WriteLine("[ERROR] " + text);
            LogAsync("[ERROR] " + text);
        }



        //private void btnBuildSDK_Click(object sender, RoutedEventArgs e)
        //{
        //    BuildSDK buildSDK = new BuildSDK();
        //    buildSDK.ShowDialog();
        //}

        private async void btnLaunchGameInEditor_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => { btnLaunchGameInEditor.IsEnabled = false; });


            var oldFiles = Directory.GetFiles(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, "*.fbmod");
            foreach (var oFile in oldFiles) File.Delete(oFile);
            var testfbmodname = "test-" + new Random().Next().ToString() + ".fbmod";


            ProjectManagement.Project.WriteToMod(testfbmodname
                , new ModSettings() { Author = "test", Category = "test", Description = "test", Title = "test", Version = "1.00" });

            await Task.Run(() =>
            {

                paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
                frostyModExecutor.UseSymbolicLinks = false;
                frostyModExecutor.ForceRebuildOfMods = true;
                frostyModExecutor.Run(this, GameInstanceSingleton.GAMERootPath, "", new System.Collections.Generic.List<string>() { testfbmodname }.ToArray()).Wait();
            });

            await Dispatcher.InvokeAsync(() => { btnLaunchGameInEditor.IsEnabled = true; });

        }

        private void btnProjectNew_Click(object sender, RoutedEventArgs e)
        {
            AssetManager.Instance.Reset();
            Log("Asset Manager Reset");
            ProjectManagement.Project = new FrostbiteProject(AssetManager.Instance, AssetManager.Instance.fs);
            Log("New Project Created");
        }

        public void UpdateAllBrowsers()
        {
            dataBrowser.UpdateAssetListView();
            gameplayBrowser.UpdateAssetListView();
            legacyBrowser.UpdateAssetListView();
        }

        private void btnOpenModDetailsPanel_Click(object sender, RoutedEventArgs e)
        {
            var mdw = new ModDetailsWindow();
            mdw.Show();
        }

        public void UpdateAllBrowsersFull()
        {
            //throw new NotImplementedException();
        }
    }
}
