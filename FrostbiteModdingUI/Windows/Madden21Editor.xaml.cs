using FIFAModdingUI;
using FrostySdk;
using FrostySdk.FrostySdk.Managers;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using Microsoft.Win32;
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
    /// Interaction logic for Madden21Editor.xaml
    /// </summary>
    public partial class Madden21Editor : Window, IEditorWindow
    {
        public ProjectManagement ProjectManagement { get; private set; }

        public Madden21Editor()
        {
            InitializeComponent();
            this.Closing += Madden21Editor_Closing;
        }

        private void Madden21Editor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.MainEditorWindow = null;
            new MainWindow().Show();
        }

        private void btnBrowseGameDirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Find your Game exe";
            dialog.Multiselect = false;
            dialog.Filter = "exe files (*.exe)|*.exe";
            dialog.FilterIndex = 0;
            dialog.ShowDialog(this);
            var filePath = dialog.FileName;
            if (!string.IsNullOrEmpty(filePath))
            {
                if (!filePath.ToLower().Contains("madden"))
                {
                    LogError("Wrong EXE chosen for Editor");
                    return;
                }

                GameInstanceSingleton.InitializeSingleton(filePath);
                txtGameDirectory.Text = GameInstanceSingleton.GAMERootPath;

                Task.Run(() =>
                {

                    ProjectManagement = new ProjectManagement(filePath, this);
                    ProjectManagement.StartNewProject();


                    // Kit Browser
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
                                      .Where(x => x.Path.ToLower().Contains("attrib")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();


                    Dispatcher.InvokeAsync(() =>
                    {

                        btnProjectNew.IsEnabled = true;
                        btnProjectOpen.IsEnabled = true;
                        btnProjectSave.IsEnabled = true;
                        //btnProjectWriteToMod.IsEnabled = true;
                        //var wt = WindowTitle;
                    });

                });
            }
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
                    if (fs.Length > 10)
                    {
                        Log("Saved mod successfully to " + saveFileDialog.FileName);
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
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
         
        }

        public string LogText = string.Empty;

        public void Log(string text, params object[] vars)
        {
            //Dispatcher.InvokeAsync(() =>
            //{
            //lblProgressText.Text = string.Format(text, vars);
            //});

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
            LogAsync("[WARNING] " + text);
        }

        public void LogError(string text, params object[] vars)
        {
            LogAsync("[ERROR] " + text);
        }

        private void btnBuildSDK_Click(object sender, RoutedEventArgs e)
        {
            BuildSDK buildSDK = new BuildSDK();
            buildSDK.ShowDialog();
        }

        private async void btnLaunchGameInEditor_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => { btnLaunchGameInEditor.IsEnabled = false; });

            ProjectManagement.FrostyProject.WriteToMod("test.fbmod"
                , new ModSettings() { Author = "test", Category = "test", Description = "test", Title = "test", Version = "1.00" });

            await Task.Run(() =>
            {

                paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
                frostyModExecutor.UseSymbolicLinks = false;
                frostyModExecutor.Run(AssetManager.Instance.fs, this, "", "", new System.Collections.Generic.List<string>() { @"test.fbmod" }.ToArray()).Wait();
            });

            await Dispatcher.InvokeAsync(() => { btnLaunchGameInEditor.IsEnabled = true; });

        }

        private void btnProjectNew_Click(object sender, RoutedEventArgs e)
        {
            AssetManager.Instance.Reset();
            Log("Asset Manager Reset");
            ProjectManagement.FrostyProject = new FrostbiteProject(AssetManager.Instance, AssetManager.Instance.fs);
            Log("New Project Created");
        }
    }
}
