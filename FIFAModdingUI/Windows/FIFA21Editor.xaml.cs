using FIFAModdingUI.Models;
using Frostbite.Textures;
using FrostbiteModdingUI.Windows;
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
using System.IO.Compression;
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
using v2k4FIFAModding;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;
using v2k4FIFASDKGenerator;

namespace FIFAModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for FIFA21Editor.xaml
    /// </summary>
    public partial class FIFA21Editor : Window, IEditorWindow
    {
        public FIFA21Editor()
        {
            InitializeComponent();
            this.DataContext = this;
            Closing += FIFA21Editor_Closing;
        }

        private void FIFA21Editor_Closing(object sender, CancelEventArgs e)
        {
            new MainWindow().Show();
        }

        private string WindowFIFAEditorTitle = "FIFA 21 Editor - Early Alpha";

        private string _windowTitle;
        public string WindowTitle 
        { 
            get 
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(WindowFIFAEditorTitle);
                _windowTitle = stringBuilder.ToString();
                return _windowTitle;
            }
            set
            {
                _windowTitle = WindowFIFAEditorTitle + " - [" + value + "]";
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
                GameInstanceSingleton.InitializeSingleton(filePath);
                txtFIFADirectory.Text = GameInstanceSingleton.GAMERootPath;



                Task.Run(() =>
                {

                    ProjectManagement = new ProjectManagement(filePath, this);
                    ProjectManagement.StartNewProject();

                    Log("Initialize Data Browser");
                    dataBrowser.AllAssetEntries = ProjectManagement.FrostyProject.AssetManager
                                       .EnumerateEbx()
                                       .Where(x => !x.Path.ToLower().Contains("character/kit")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();


                    // Kit Browser
                    Log("Initialize Kit Browser");
                    var kitList = ProjectManagement.FrostyProject.AssetManager
                                       .EnumerateEbx().Where(x => x.Path.ToLower().Contains("character/kit")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();
                    kitList = kitList.OrderBy(x => x.Name).ToList();
                    var citykits = kitList.Where(x => x.Name.ToLower().Contains("manchester_city"));
                    kitBrowser.AllAssetEntries = kitList;

                    

                    
                    Log("Initialize Gameplay Browser");
                    gameplayBrowser.AllAssetEntries = ProjectManagement.FrostyProject.AssetManager
                                      .EnumerateEbx()
                                      .Where(x => x.Filename.StartsWith("gp_")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();


                    var legacyFiles = ProjectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy").OrderBy(x => x.Path).ToList();

                    Log("Initialize Legacy Browser");
                    legacyBrowser.AllAssetEntries = legacyFiles.Select(x => (IAssetEntry)x).ToList();

                    Log("Initialize Texture Browser");
                    List<IAssetEntry> textureAssets = ProjectManagement.FrostyProject.AssetManager
                                       .EnumerateEbx("TextureAsset").OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();
                    textureAssets.AddRange(legacyBrowser.AllAssetEntries.Where(x => x.Name.ToUpper().Contains(".DDS")));

                    textureBrowser.AllAssetEntries = textureAssets;

                    playerEditor.InitPlayerSearch();

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
                //    GameplayMain.Initialize();
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
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Mod files|*.fbmod";
                var resultValue = saveFileDialog.ShowDialog();
                if (resultValue.HasValue && resultValue.Value)
                {
                    ProjectManagement.FrostyProject.WriteToMod(saveFileDialog.FileName
                        , new FrostySdk.ModSettings() { Author = "paulv2k4 Mod Tool", Description = "", Category = "", Title = "paulv2k4 Mod Tool Mod", Version = "1.00" });
                    using (var fs = new FileStream(saveFileDialog.FileName, FileMode.Open))
                    {
                        //if (fs.Length > 1024 * 2)
                        //{
                            Log("Saved mod successfully to " + saveFileDialog.FileName);
                        //}
                        //else
                        //{
                        //    Log("An error has occurred Saving mod to " + saveFileDialog.FileName + " file seems too small");

                        //}
                    }
                }

                if (ProjectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy", true).Count() > 0)
                {
                    saveFileDialog.Filter = "Legacy Mod files|*.lmod";
                    resultValue = saveFileDialog.ShowDialog();
                    saveFileDialog.FileName = saveFileDialog.FileName.Replace(".fbmod", "");
                    if (resultValue.HasValue && resultValue.Value)
                    {
                        if (Directory.Exists("Temp"))
                        {
                            Directory.Delete("Temp", true);
                        }
                        Directory.CreateDirectory("Temp");
                        foreach (var entry in ProjectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy", true))
                        {
                            var compilePath = "Temp/" + entry.Path;
                            if (!Directory.Exists(compilePath))
                            {
                                Directory.CreateDirectory(compilePath);
                            }

                            using (NativeWriter nativeWriter = new NativeWriter(new FileStream(compilePath + "/" + entry.Filename + "." + entry.Type, FileMode.Create)))
                            {
                                nativeWriter.Write(new NativeReader(ProjectManagement.Instance.FrostyProject.AssetManager.GetCustomAsset("legacy", entry)).ReadToEnd());
                            }
                        }

                        Task.Run(() =>
                        {
                            CompileLMOD(saveFileDialog.FileName);
                        }).ContinueWith((i) => {
                        


                        });
                    }
                }
            }
            catch(Exception SaveException)
            {
                LogError(SaveException.ToString());
            }
        }

        private void CompileLMOD(string filename)
        {
            List<string> listOfCompilableFiles = new List<string>();
            var index = 0;
            var allFiles = Directory.GetFiles("Temp", "*.*", SearchOption.AllDirectories).Where(x => !x.Contains(".mod"));
            foreach (var file in allFiles)
            {
                StringBuilder sbFinalResult = new StringBuilder();

                var encrypt = !file.Contains(".dds")
                    && !file.Contains(".db");

                if (encrypt)
                {
                    var splitExtension = file.Split('.');
                    if (splitExtension[splitExtension.Length - 1] != "mod")
                        splitExtension[splitExtension.Length - 1] = "mod";

                    foreach (var str in splitExtension)
                    {
                        if (str == "mod")
                        {
                            sbFinalResult.Append(".mod");
                        }
                        else
                        {
                            sbFinalResult.Append(str);
                        }
                    }
                }
                else
                {
                    sbFinalResult.Append(file);
                }

                if (encrypt)
                {
                    Log("Encrypting " + file);
                    v2k4Encryption.encryptFile(file, sbFinalResult.ToString());
                    File.Delete(file);
                }

                listOfCompilableFiles.Add(sbFinalResult.ToString());
                index++;
            }

            Log("Encryption compilation complete");

            if (File.Exists(filename))
                File.Delete(filename);

            using (ZipArchive zipArchive = new ZipArchive(new FileStream(filename, FileMode.OpenOrCreate), ZipArchiveMode.Create))
            {
                foreach (var compatFile in listOfCompilableFiles)
                {
                    zipArchive.CreateEntryFromFile(compatFile, compatFile.Replace("Temp\\", ""));
                }
            }

            Log($"Legacy Mod file saved to {filename}");

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

                    WindowTitle = openFileDialog.FileName;

                }
            }
        }

        private async void btnLaunchFIFAInEditor_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => { btnLaunchFIFAInEditor.IsEnabled = false; });

            ProjectManagement.FrostyProject.WriteToMod("test.fbmod"
                , new ModSettings() { Author = "test", Category = "test", Description = "test", Title = "test", Version = "1.00" });

            var modifiedLegacy = ProjectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy", true).ToList();
            
            modifiedLegacy.ForEach(
                entry => 
                {

                    var extractPath = AssetManager.Instance.fs.BasePath + "/LegacyMods/Legacy/" + entry.Path;
                    if(!string.IsNullOrEmpty(extractPath))
                    {
                        if(!Directory.Exists(extractPath))
                            Directory.CreateDirectory(extractPath);

                        if (File.Exists(extractPath + "/" + entry.Filename + ".mod"))
                        {

                        }
                    }

                
                });
            await Task.Run(() =>
            {

                paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
                frostyModExecutor.UseSymbolicLinks = true;
                frostyModExecutor.Run(AssetManager.Instance.fs, this, "", "", new System.Collections.Generic.List<string>() { @"test.fbmod" }.ToArray()).Wait();
            });

            InjectLegacyDLL();

            await Dispatcher.InvokeAsync(() => { btnLaunchFIFAInEditor.IsEnabled = true; });

        }

        private void InjectLegacyDLL()
        {
            string legacyModSupportFile = null;
            if (GameInstanceSingleton.GAMEVERSION == "FIFA20")
            {
                legacyModSupportFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA20Legacy.dll";
            }
            else if (GameInstanceSingleton.GAMEVERSION == "FIFA21")
            {
                legacyModSupportFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA.dll";
            }

            if (!string.IsNullOrEmpty(legacyModSupportFile))
            {

                if (File.Exists(legacyModSupportFile))
                {
                    File.Copy(legacyModSupportFile, @GameInstanceSingleton.GAMERootPath + "v2k4LegacyModSupport.dll", true);
                }

                var legmodsupportdllpath = @GameInstanceSingleton.GAMERootPath + @"v2k4LegacyModSupport.dll";
                GameInstanceSingleton.InjectDLLAsync(legmodsupportdllpath);
            }
        }

        private void btnProjectNew_Click(object sender, RoutedEventArgs e)
        {
            AssetManager.Instance.Reset();
            Log("Asset Manager Reset");
            ProjectManagement.FrostyProject = new FrostbiteProject(AssetManager.Instance, AssetManager.Instance.fs);
            Log("New Project Created");
        }

        private async void btnCompileLegacyModFromFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var legacyDirectory = dialog.SelectedPath;
                    if (!string.IsNullOrEmpty(legacyDirectory))
                    {
                        await Task.Run(() =>
                        {
                            DirectoryInfo directoryInfoTemp = new DirectoryInfo("TEMP");
                            if (directoryInfoTemp.Exists)
                            {
                                RecursiveDelete(directoryInfoTemp);
                            }

                            DirectoryCopy(legacyDirectory, "TEMP", true);
                            List<string> listOfCompilableFiles = new List<string>();

                            var allFiles = Directory.GetFiles("TEMP", "*.*", SearchOption.AllDirectories).Where(x => !x.Contains(".mod"));
                            Task[] tasks = new Task[allFiles.Count()];
                            var index = 0;
                            foreach (var file in allFiles)
                            {

                                //tasks[index] = Task.Run(() =>
                                //{
                                StringBuilder sbFinalResult = new StringBuilder();

                                var encrypt = !file.Contains(".dds")
                                    && !file.Contains(".db");

                                if (encrypt)
                                {
                                    var splitExtension = file.Split('.');
                                    if (splitExtension[splitExtension.Length - 1] != "mod")
                                        splitExtension[splitExtension.Length - 1] = "mod";

                                    foreach (var str in splitExtension)
                                    {
                                        if (str == "mod")
                                        {
                                            sbFinalResult.Append(".mod");
                                        }
                                        else
                                        {
                                            sbFinalResult.Append(str);
                                        }
                                    }
                                }
                                else
                                {
                                    sbFinalResult.Append(file);
                                }

                                if (encrypt)
                                {
                                    var fI = new FileInfo(file);
                                    if (fI != null && fI.Extension.Contains("mod"))
                                    {
                                        fI.Delete();
                                    }
                                    Log("Encrypting " + file);
                                    v2k4Encryption.encryptFile(file, sbFinalResult.ToString());
                                }

                                listOfCompilableFiles.Add(sbFinalResult.ToString());
                            }

                            Log("Legacy Compiler :: Compilation Complete");


                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = "Legacy Mod Files|.lmod";
                            var saveFileDialogResult = saveFileDialog.ShowDialog();
                            if (saveFileDialogResult.HasValue && saveFileDialogResult.Value)
                            {
                                if (File.Exists(saveFileDialog.FileName))
                                    File.Delete(saveFileDialog.FileName);

                                using (ZipArchive zipArchive = new ZipArchive(new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate), ZipArchiveMode.Create))
                                {
                                    foreach (var compatFile in listOfCompilableFiles)
                                    {
                                        zipArchive.CreateEntryFromFile(compatFile, compatFile.Replace("TEMP\\", ""));
                                    }
                                }
                                Log("Legacy Compiler :: Legacy Mod file saved");

                                foreach (var f in listOfCompilableFiles.Where(x => x.EndsWith(".mod")))
                                {
                                    var fI = new FileInfo(f);
                                    if (fI != null)
                                    {
                                        fI.Delete();
                                    }
                                }
                                Log("Legacy Compiler :: Cleaned up encrypted files");

                            }

                        });
                    }
                }
            }
        }

        public static void RecursiveDelete(DirectoryInfo baseDir)
        {
            try
            {
                if (!baseDir.Exists)
                    return;

                foreach (var dir in baseDir.EnumerateDirectories())
                {
                    RecursiveDelete(dir);
                }
                baseDir.Delete(true);
            }
            catch (Exception e)
            {

            }
        }


        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        private void MainViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var t = sender as TabControl;
            //if (t != null)
            //{
            //    var selectedTab = ((TabItem)t.SelectedItem);
            //    var selectedTabHeader = selectedTab.Header;

            //    if (selectedTabHeader.ToString().Contains("Player"))
            //    {
            //        var legacyFiles = ProjectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy").OrderBy(x => x.Path).ToList();
            //        var mainDb = legacyFiles.FirstOrDefault(x => x.Filename.Contains("fifa_ng_db") && x.Type == "DB");
            //        var mainDbMeta = legacyFiles.FirstOrDefault(x => x.Filename.Contains("fifa_ng_db-meta") && x.Type == "XML");
            //        playerEditor.LoadPlayerList(mainDb as LegacyFileEntry, mainDbMeta as LegacyFileEntry);
            //        playerEditor.UpdateLayout();
            //    }
            //}
        }
    }
}
