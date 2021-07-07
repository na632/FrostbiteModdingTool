using FIFAModdingUI;
using FIFAModdingUI.Windows;
using FrostbiteModdingUI.Models;
using FrostySdk.FrostySdk.Managers;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// Interaction logic for BF4Editor.xaml
    /// </summary>
    public partial class BF4Editor : Window, IEditorWindow
    {
        public Window OwnerWindow { get; set; }

        [Obsolete("Incorrect usage of Editor Windows")]

        public BF4Editor()
        {
            throw new Exception("Incorrect usage of Editor Windows");
        }

        public BF4Editor(Window owner)
        {
            InitializeComponent();
            this.DataContext = this;
            Loaded += BF4Editor_Loaded; ;
            Owner = owner;
        }

        public string LastGameLocation => App.ApplicationDirectory + "BF4LastLocation.json";

        public string RecentFilesLocation => App.ApplicationDirectory + "BF4RecentFilesLocation.json";

        private void BF4Editor_Loaded(object sender, RoutedEventArgs e)
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (App.AppInsightClient != null)
            {
                App.AppInsightClient.Flush();
            }

            ProjectManagement = null;
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

        private string WindowFIFAEditorTitle = $"BF4 Editor - {App.ProductVersion} - ";

        private string _windowTitle;
        public string WindowTitle
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(WindowFIFAEditorTitle);
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

        public static ProjectManagement ProjectManagement { get; set; }

        public void InitialiseOfSelectedGame(string filePath)
        {
            GameInstanceSingleton.InitializeSingleton(filePath);
            GameInstanceSingleton.Logger = this;

            bool? result = false;
            BuildSDKAndCache buildSDKAndCacheWindow = new BuildSDKAndCache();
            if (buildSDKAndCacheWindow.DoesCacheNeedsRebuilding())
            {
                result = buildSDKAndCacheWindow.ShowDialog();
            }

            Task.Run(() =>
            {

                ProjectManagement = new ProjectManagement(filePath, this);
                ProjectManagement.StartNewProject();

                Log("Initialize Data Browser");
                dataBrowser.AllAssetEntries = ProjectManagement.Project.AssetManager.EnumerateEbx().Select(x => (IAssetEntry)x).ToList();

                Dispatcher.Invoke(() => {

                    var wt = WindowTitle;
                    WindowTitle = "New Project";
                    ProjectManagement.Project.ModifiedAssetEntries = null;
                    this.DataContext = null;
                    this.DataContext = this;
                    this.UpdateLayout();

                });

            });

            var presence = new DiscordRPC.RichPresence();
            presence.State = "In Editor - " + GameInstanceSingleton.GAMEVERSION;
            App.DiscordRpcClient.SetPresence(presence);
            App.DiscordRpcClient.Invoke();
        }

        public void UpdateAllBrowsers()
        {
            throw new NotImplementedException();
        }

        public void Log(string text, params object[] vars)
        {
        }

        public void LogWarning(string text, params object[] vars)
        {
        }

        public void LogError(string text, params object[] vars)
        {
        }

        public void UpdateAllBrowsersFull()
        {
            throw new NotImplementedException();
        }
    }
}
