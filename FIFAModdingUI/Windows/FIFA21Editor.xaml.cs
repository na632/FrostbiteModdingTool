using Frostbite.Textures;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using FrostySdk.Resources;
using System;
using System.Collections.Generic;
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
using v2k4FIFASDKGenerator;

namespace FIFAModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for FIFA21Editor.xaml
    /// </summary>
    public partial class FIFA21Editor : Window, ILogger
    {
        public FIFA21Editor()
        {
            InitializeComponent();
        }

        ProjectManagement ProjectManagement { get; set; }

        private async void btnBrowseFIFADirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dialog.Title = "Find your FIFA exe";
            dialog.Multiselect = false;
            dialog.Filter = "exe files (*.exe)|*.exe";
            dialog.FilterIndex = 0;
            dialog.ShowDialog(this);
            var filePath = dialog.FileName;
            GameInstanceSingleton.InitialiseSingleton(filePath);
            txtFIFADirectory.Text = GameInstanceSingleton.GAMERootPath;
            await Start();
        }

        private void btnLaunchFIFAMods(object sender, RoutedEventArgs e)
        {
            ProjectManagement.FrostyProject.Save("test_gp_speed_change.fbproject");
            ProjectManagement.FrostyProject.WriteToMod("test_gp_speed_change.fbmod"
                , new ModSettings() { Author = "paulv2k4", Category = "Gameplay", Description = "Gameplay Test", Title = "Gameplay Test", Version = "1.00" });

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.Run(AssetManager.Instance.fs, this, "", "", new System.Collections.Generic.List<string>() { @"test_gp_speed_change.fbmod" }.ToArray()).Wait();

        }

        public async Task<bool> Start()
        {
            return await Task.Run<bool>(() =>
            {
                BuildCache buildCache = new BuildCache();
                buildCache.LoadDataAsync(GameInstanceSingleton.GAMEVERSION, GameInstanceSingleton.GAMERootPath, this, loadSDK: true).Wait();

                ProjectManagement = new ProjectManagement(GameInstanceSingleton.GAMERootPath + "\\" + GameInstanceSingleton.GAMEVERSION + ".exe");
                ProjectManagement.FrostyProject = new FrostySdk.FrostyProject(AssetManager.Instance, AssetManager.Instance.fs);

                Dispatcher.Invoke(() =>
                {
                    GPFrostyGameplayMainFrame.Source = new Uri("../Pages/Gameplay/FrostyGameplayMain.xaml", UriKind.Relative);
                    MainViewer.IsEnabled = true;
                });

                BuildTextureBrowser(null);

                return true;
            });
        }

        string lastTemporaryFileLocation;
        Random Randomizer = new Random();
        private void BuildTextureBrowser(string filter)
        {
            Dispatcher.Invoke(() =>
            {
                var index = 0;

                string lastPath = null;
                TreeViewItem treeItem = null;
                var items = ProjectManagement.FrostyProject.AssetManager
                    .EnumerateEbx("TextureAsset").OrderBy(x => x.Path).ToList();
                foreach (var i in items)
                {
                    var splitPath = i.Path.Split('/');
                    //foreach(var innerPath in splitPath)
                    //{

                        bool usePreviousTree = string.IsNullOrEmpty(lastPath) || lastPath.ToLower() == i.Path.ToLower();

                        // use previous tree
                        if (!usePreviousTree || treeItem == null)
                        {
                            treeItem = new TreeViewItem();
                            tvTextureBrowser.Items.Add(treeItem);
                        }
                        treeItem.Header = i.Path;
                        lastPath = i.Path;
                    var innerTreeItem = new Button() { Content = i.DisplayName, Tag = i };

                    innerTreeItem.PreviewMouseRightButtonUp += InnerTreeItem_PreviewMouseRightButtonUp;

                    innerTreeItem.MouseDoubleClick += (object sender, MouseButtonEventArgs e) =>
                    {
                        try
                        {
                            var eb = AssetManager.Instance.GetEbx(i);
                            if (eb != null)
                            {
                                var res = AssetManager.Instance.GetResEntry(i.Name);
                                if (res != null)
                                {
                                    using (var resStream = ProjectManagement.FrostyProject.AssetManager.GetRes(res))
                                    {
                                        using (Texture textureAsset = new Texture(resStream, ProjectManagement.FrostyProject.AssetManager))
                                        {
                                            ImageViewer.Source = null;
                                            var dumpFile = $"temp_{Randomizer.Next().ToString()}.DDS";
                                            new TextureExporter().Export(textureAsset, dumpFile, "*.dds");

                                            var tempLoc = Directory.GetParent(Assembly.GetEntryAssembly().Location) + "\\" + dumpFile;
                                            Uri fileUri = new Uri(tempLoc);
                                            var bImage = new BitmapImage(fileUri);
                                            ImageViewer.Source = bImage;
                                            bImage = null;
                                            fileUri = null;

                                            //if (!string.IsNullOrEmpty(lastTemporaryFileLocation))
                                            //    File.Delete(lastTemporaryFileLocation);

                                            lastTemporaryFileLocation = dumpFile;
                                        }
                                    }
                                }
                            }
                        }
                        catch(Exception)
                        {
                            Log("Failed to load texture");
                        }


                       
                        
                    };

                    treeItem.Items.Add(innerTreeItem);

                    
                    //}
                    index++;
                    Log($"Loading Texture Browser ({index}/{items.Count()})");

                }
            });
        }

        private void InnerTreeItem_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        public void Log(string text, params object[] vars)
        {
            Dispatcher.Invoke(() =>
            {
                lblProgressText.Text = string.Format(text, vars);
            });
        }

        public void LogWarning(string text, params object[] vars)
        {
        }

        public void LogError(string text, params object[] vars)
        {
        }
    }
}
