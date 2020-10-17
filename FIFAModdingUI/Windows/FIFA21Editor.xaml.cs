using Frostbite.Textures;
using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private void btnBrowseFIFADirectory_Click(object sender, RoutedEventArgs e)
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
            _ = Start();
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
                BuildLegacyBrowser(null);

                return true;
            });
        }

        string lastTemporaryFileLocation;
        Random Randomizer = new Random();
        EbxAssetEntry CurrentTextureAssetEntry = null;

        private bool BuildTextureBrowser(string filter)
        {
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (s, we) =>
            {
                
                    var index = 0;

                Dispatcher.Invoke(() =>
                {

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
                        var innerTreeItem = new Label() { Content = i.DisplayName };

                        innerTreeItem.PreviewMouseRightButtonUp += InnerTreeItem_PreviewMouseRightButtonUp;

                        innerTreeItem.MouseDoubleClick += (object sender, MouseButtonEventArgs e) =>
                        {
                            try
                            {
                                CurrentTextureAssetEntry = i;
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
                                                try
                                                {
                                                    ImageViewer.Source = null;
                                                    var bImage = LoadImage(new TextureExporter().WriteToDDS(textureAsset));
                                                    ImageViewer.Source = bImage;
                                                    bImage = null;
                                                }
                                                catch { ImageViewer.Source = null; }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
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
            };
            worker.RunWorkerAsync();
            return true;
        }

        private bool BuildLegacyBrowser(string filter)
        {
            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (s, we) =>
            {
                Dispatcher.Invoke(() =>
                {
                    var index = 0;

                    string lastPath = null;
                    TreeViewItem treeItem = null;
                    var items = ProjectManagement.FrostyProject.AssetManager
                        .EnumerateCustomAssets("legacy").OrderBy(x => x.Path).ToList();
                    Debug.WriteLine($"Count of Legacy: {items.Count}");
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
                            tvLegacy.Items.Add(treeItem);
                        }
                        treeItem.Header = i.Path;
                        lastPath = i.Path;
                        var innerTreeItem = new Label() { Content = i.DisplayName, Tag = i };

                        //innerTreeItem.PreviewMouseRightButtonUp += InnerTreeItem_PreviewMouseRightButtonUp;
                        innerTreeItem.MouseLeftButtonUp += InnerTreeItem_MouseLeftButtonUp;
                        
                        treeItem.Items.Add(innerTreeItem);

                        index++;
                        Log($"Loading Legacy Browser ({index}/{items.Count()})");

                    }
                });
            };
            worker.RunWorkerAsync();
            return true;
        }

        private static System.Windows.Media.Imaging.BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new System.Windows.Media.Imaging.BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void InnerTreeItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Label obj = sender as Label;
            if(obj != null)
            {
                AssetEntry assetEntry = obj.Tag as AssetEntry;
                if(assetEntry != null)
                {
                    NameOfLegacyAsset.Content = assetEntry.Name;
                    txtLegacyViewer.Visibility = Visibility.Hidden;
                    ImageViewerLegacy.Visibility = Visibility.Hidden;

                    CurrentLegacySelection = assetEntry;
                    if(CurrentLegacySelection.Type == "JSON" 
                        || CurrentLegacySelection.Type == "INI"
                        || CurrentLegacySelection.Type == "CSV"
                        || CurrentLegacySelection.Type == "LUA"
                        )
                    {
                        txtLegacyViewer.Visibility = Visibility.Visible;
                        using (var nr = new NativeReader(ProjectManagement.FrostyProject.AssetManager.GetCustomAsset("legacy", CurrentLegacySelection)))
                        {
                            txtLegacyViewer.Text = ASCIIEncoding.ASCII.GetString(nr.ReadToEnd());
                        }
                    }
                    else if(CurrentLegacySelection.Type == "DDS")
                    {

                        try
                        {
                            using (var nr = new NativeReader(ProjectManagement.FrostyProject.AssetManager.GetCustomAsset("legacy", CurrentLegacySelection)))
                            {
                                ImageViewerLegacy.Source = null;
                                var bImage = LoadImage(nr.ReadToEnd());
                                ImageViewerLegacy.Source = bImage;
                                ImageViewerLegacy.Visibility = Visibility.Visible;
                                bImage = null;
                            }
                        }
                        catch { ImageViewer.Source = null; }

                    }

                }
            }
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

        private void btnExportTexture_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "DDS File|*.dds";
            if(saveFileDialog.ShowDialog() == true)
            {
                if (CurrentTextureAssetEntry != null)
                {
                    var res = AssetManager.Instance.GetResEntry(CurrentTextureAssetEntry.Name);
                    if (res != null)
                    {
                        using (var resStream = ProjectManagement.FrostyProject.AssetManager.GetRes(res))
                        {
                            using (Texture textureAsset = new Texture(resStream, ProjectManagement.FrostyProject.AssetManager))
                            {
                                new TextureExporter().Export(textureAsset, saveFileDialog.FileName, "*.dds");
                            }
                        }
                    }
                }
            }
        }

        private AssetEntry CurrentLegacySelection;

        private void btnExportLegacy_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            var filt = "*." + CurrentLegacySelection.Type;
            saveFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
            saveFileDialog.FileName = CurrentLegacySelection.Name;
            if (saveFileDialog.ShowDialog().Value)
            {
                using (NativeWriter nativeWriter = new NativeWriter(new FileStream(saveFileDialog.FileName, FileMode.Create)))
                {
                    nativeWriter.Write(new NativeReader(ProjectManagement.FrostyProject.AssetManager.GetCustomAsset("legacy", CurrentLegacySelection)).ReadToEnd());
                }
            }
        }

        private void btnImportLegacy_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
