using FIFAModdingUI.Models;
using Frostbite.Textures;
using FrostySdk;
using FrostySdk.FrostySdk.Managers;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
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
            this.DataContext = this;
        }

        public string WindowTitle 
        { 
            get 
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("FIFA 21 Editor - Early Alpha");
                if(ProjectManagement != null 
                    && ProjectManagement.FrostyProject != null)
                {
                    stringBuilder.Append(" [ " + ProjectManagement.FrostyProject.DisplayName + " ] ");
                }
                    
                
                return stringBuilder.ToString();
            } 
        }

        public static ProjectManagement ProjectManagement { get; set; }

        private void btnBrowseFIFADirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dialog.Title = "Find your FIFA exe";
            dialog.Multiselect = false;
            dialog.Filter = "exe files (*.exe)|*.exe";
            dialog.FilterIndex = 0;
            dialog.ShowDialog(this);
            var filePath = dialog.FileName;
            if (!string.IsNullOrEmpty(filePath))
            {
                GameInstanceSingleton.InitialiseSingleton(filePath);
                txtFIFADirectory.Text = GameInstanceSingleton.GAMERootPath;



                Task.Run(() =>
                {

                    ProjectManagement = new ProjectManagement(filePath, this);
                    ProjectManagement.StartNewProject();

                   
                    // Kit Browser
                    Log("Initialise Kit Browser");
                    var kitList = ProjectManagement.FrostyProject.AssetManager
                                       .EnumerateEbx().Where(x => x.Path.ToLower().Contains("character/kit")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();
                    //kitList.AddRange(ProjectManagement.FrostyProject.AssetManager
                     //                       .EnumerateRes((int)ResourceType.Texture)
                     //                       .Where(x => x.Path.Contains("character/kit")));
                    kitList = kitList.OrderBy(x => x.Name).ToList();
                    var citykits = kitList.Where(x => x.Name.ToLower().Contains("manchester_city"));
                    kitBrowser.AllAssetEntries = kitList;

                    // Check and run Legacy Browser (UI is where the slowness is, FIXME)
                    var legacyFiles = ProjectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy").OrderBy(x => x.Path).ToList();

                    Log("Initialise Legacy Browser");
                    legacyBrowser.AllAssetEntries = legacyFiles.Select(x => (IAssetEntry)x).ToList();


                    Log("Initialise Texture Browser");
                    textureBrowser.AllAssetEntries = ProjectManagement.FrostyProject.AssetManager
                                       .EnumerateEbx("TextureAsset").OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();

                    Log("Initialise Data Browser");
                    dataBrowser.AllAssetEntries = ProjectManagement.FrostyProject.AssetManager
                                       .EnumerateEbx()
                                       .Where(x => !x.Path.ToLower().Contains("character/kit")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();

                    Log("Initialise Gameplay Browser");
                    gameplayBrowser.AllAssetEntries = ProjectManagement.FrostyProject.AssetManager
                                      .EnumerateEbx()
                                      .Where(x => x.Filename.StartsWith("gp_")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();


                    Dispatcher.InvokeAsync(() => {

                        btnProjectNew.IsEnabled = true;
                        btnProjectOpen.IsEnabled = true;
                        btnProjectSave.IsEnabled = true;
                        btnProjectWriteToMod.IsEnabled = true;
                        var wt = WindowTitle;
                    });
                
                });


                    //BuildTextureBrowser(null);

                //Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    GameplayMain.Initialise();
                //}));

            }

            
            //_ = Start();
        }

        private void btnLaunchFIFAMods(object sender, RoutedEventArgs e)
        {
            ProjectManagement.FrostyProject.Save("test_gp_speed_change.fbproject");
            ProjectManagement.FrostyProject.WriteToMod("test_gp_speed_change.fbmod"
                , new ModSettings() { Author = "paulv2k4", Category = "Gameplay", Description = "Gameplay Test", Title = "Gameplay Test", Version = "1.00" });

            paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
            frostyModExecutor.Run(AssetManager.Instance.fs, this, "", "", new System.Collections.Generic.List<string>() { @"test_gp_speed_change.fbmod" }.ToArray()).Wait();

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
                        || CurrentLegacySelection.Type == "NAV"
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
                        catch { //ImageViewer.Source = null; 
                        }

                    }

                }
            }
        }

        private void InnerTreeItem_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        public string LogText = string.Empty;

        public void Log(string text, params object[] vars)
        {
            Dispatcher.InvokeAsync(() =>
            {
                lblProgressText.Text = string.Format(text, vars);
            });

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
        }

        public void LogError(string text, params object[] vars)
        {
        }

        private AssetEntry CurrentLegacySelection;

        private void btnExportLegacy_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentLegacySelection != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                var filt = "*." + CurrentLegacySelection.Type;
                saveFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
                saveFileDialog.FileName = CurrentLegacySelection.Filename;
                if (saveFileDialog.ShowDialog().Value)
                {
                    using (NativeWriter nativeWriter = new NativeWriter(new FileStream(saveFileDialog.FileName, FileMode.Create)))
                    {
                        nativeWriter.Write(new NativeReader(ProjectManagement.FrostyProject.AssetManager.GetCustomAsset("legacy", CurrentLegacySelection)).ReadToEnd());
                    }
                }
            }
        }

        private void btnImportLegacy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnProjectWriteToMod_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Mod files|*.fbmod";
            var resultValue = saveFileDialog.ShowDialog(); 
            if (resultValue.HasValue && resultValue.Value)
            {
                ProjectManagement.FrostyProject.WriteToMod(saveFileDialog.FileName
                    , new FrostySdk.ModSettings() { Author = "paulv2k4 Mod Tool", Description = "", Category = "", Title = "paulv2k4 Mod Tool GP Mod", Version = "1.00" });
                using (var fs = new FileStream(saveFileDialog.FileName, FileMode.Open))
                {
                    if(fs.Length > 1024 * 5)
                    {
                        Log("Saved mod successfully to " + saveFileDialog.FileName );
                    }
                    else
                    {
                        Log("An error has occurred Saving mod to " + saveFileDialog.FileName + " file seems too small");

                    }
                }
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
                    ProjectManagement.FrostyProject.Save(saveFileDialog.FileName, true);

                    Log("Saved project successfully to " + saveFileDialog.FileName);

                }
            }
        }

        private void btnProjectOpen_Click(object sender, RoutedEventArgs e)
        {
            VistaOpenFileDialog openFileDialog = new VistaOpenFileDialog();
            openFileDialog.Filter = "Project files|*.fbproject";
            var result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName))
                {
                    ProjectManagement.FrostyProject.Load(openFileDialog.FileName);

                    Log("Opened project successfully from " + openFileDialog.FileName);

                }
            }
        }

        private async void btnLaunchFIFAInEditor_Click(object sender, RoutedEventArgs e)
        {
            ProjectManagement.FrostyProject.WriteToMod("test.fbmod"
                , new ModSettings() { Author = "test", Category = "test", Description = "test", Title = "test", Version = "1.00" });

            await Task.Run(() =>
            {

                paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
                frostyModExecutor.UseSymbolicLinks = true;
                frostyModExecutor.Run(AssetManager.Instance.fs, this, "", "", new System.Collections.Generic.List<string>() { @"test.fbmod" }.ToArray()).Wait();
            });
        }

        private void btnProjectNew_Click(object sender, RoutedEventArgs e)
        {
            AssetManager.Instance.Reset();
            Log("Asset Manager Reset");
            ProjectManagement.FrostyProject = new FrostyProject(AssetManager.Instance, AssetManager.Instance.fs);
            Log("New Project Created");
        }
    }
}
