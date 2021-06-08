﻿using FIFAModdingUI.Models;
using FIFAModdingUI.Pages.Common;
using FolderBrowserEx;
using Frostbite.FileManagers;
using Frostbite.Textures;
using FrostbiteModdingUI.Models;
using FrostbiteModdingUI.Windows;
using FrostbiteSdk.Frostbite.FileManagers;
using FrostySdk;
using FrostySdk.Ebx;
using FrostySdk.Frosty.FET;
using FrostySdk.FrostySdk.Managers;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
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
using Windows.Foundation.Metadata;

namespace FIFAModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for FIFA21Editor.xaml
    /// </summary>
    public partial class FIFA21Editor : Window, IEditorWindow
    {
        public Window OwnerWindow { get; set; }

        [Deprecated("Incorrect usage of Editor Windows", DeprecationType.Remove, 0)]
        public FIFA21Editor()
        {
            throw new Exception("Incorrect usage of Editor Windows");
        }

        public FIFA21Editor(Window owner)
        {
            InitializeComponent();
            this.DataContext = this;
            Loaded += FIFA21Editor_Loaded;
            Owner = owner;

            

        }

        public string LastFIFA21Location => App.ApplicationDirectory + "FIFA21LastLocation.json";

        public string FIFA21RecentFilesLocation => App.ApplicationDirectory + "FIFA21RecentFilesLocation.json";


        private List<FileInfo> recentProjectFiles;

        public List<FileInfo> RecentProjectFiles
        {
            get 
            { 
                if(recentProjectFiles == null)
                {
                    recentProjectFiles = new List<FileInfo>();
                    if(new FileInfo(FIFA21RecentFilesLocation).Exists)
                    {
                        var allText = File.ReadAllText(FIFA21RecentFilesLocation);
                        var items = JsonConvert.DeserializeObject<List<string>>(allText);
                        recentProjectFiles = items.Select(x => new FileInfo(x)).ToList();
                    }
                }

                return recentProjectFiles.OrderByDescending(x => x.LastWriteTime).Take(5).ToList();
            }
            set 
            {
                recentProjectFiles = value;

                var str = JsonConvert.SerializeObject(recentProjectFiles.Select(x=>x.FullName));
                File.WriteAllText(FIFA21RecentFilesLocation, str);

                DataContext = null;
                DataContext = this;
            }
        }

        private void FIFA21Editor_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(LastFIFA21Location))
            {
                var tmpLoc = File.ReadAllText(LastFIFA21Location);
                if (File.Exists(tmpLoc))
                {
                    AppSettings.Settings.GameInstallEXEPath = tmpLoc;
                }
                else
                {
                    File.Delete(LastFIFA21Location);
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

            File.WriteAllText(LastFIFA21Location, AppSettings.Settings.GameInstallEXEPath);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if(ProjectManagement != null)
            {
                if (ProjectManagement.Project != null && ProjectManagement.Project.IsDirty)
                {
                    if(MessageBox.Show("Your project has been changed. Would you like to save it now?", "Project has not been saved", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        e.Cancel = true;
                        SaveProjectWithDialog();
                    }
                }
            }


            base.OnClosing(e);
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

        private string WindowFIFAEditorTitle = $"FIFA 21 Editor - {FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion} - ";

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
            lstProjectFiles.Items.Clear();
            lstProjectFiles.ItemsSource = null;

            bool? result = false;
            BuildSDKAndCache buildSDKAndCacheWindow = new BuildSDKAndCache();
            if(buildSDKAndCacheWindow.DoesCacheNeedsRebuilding())
            {
                result = buildSDKAndCacheWindow.ShowDialog();
            }

            Task.Run(() =>
            {
               
                ProjectManagement = new ProjectManagement(filePath, this);
                ProjectManagement.StartNewProject();

                Log("Initialize Data Browser");
                dataBrowser.AllAssetEntries = ProjectManagement.Project.AssetManager
                                   .EnumerateEbx()
                                   .Where(x => !x.Path.ToLower().Contains("character/kit")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();


                // Kit Browser
                Log("Initialize Kit Browser");
                var kitList = ProjectManagement.Project.AssetManager
                                   .EnumerateEbx().Where(x => x.Path.ToLower().Contains("character/kit")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();
                kitList = kitList.OrderBy(x => x.Name).ToList();
                var citykits = kitList.Where(x => x.Name.ToLower().Contains("manchester_city"));
                kitBrowser.AllAssetEntries = kitList;


                Log("Initialize Gameplay Browser");
                gameplayBrowser.AllAssetEntries = ProjectManagement.Project.AssetManager
                                  .EnumerateEbx()
                                  .Where(x => x.Filename.StartsWith("gp_")).OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();


                var legacyFiles = ProjectManagement.Project.AssetManager.EnumerateCustomAssets("legacy").OrderBy(x => x.Path).ToList();

                Log("Initialize Legacy Browser");
                legacyBrowser.AllAssetEntries = legacyFiles.Select(x => (IAssetEntry)x).ToList();

                Log("Initialize Texture Browser");
                List<IAssetEntry> textureAssets = ProjectManagement.Project.AssetManager
                                   .EnumerateEbx("TextureAsset").OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();
                textureAssets.AddRange(legacyBrowser.AllAssetEntries.Where(x => x.Name.Contains(".DDS", StringComparison.OrdinalIgnoreCase)));

                textureBrowser.AllAssetEntries = textureAssets;

                //playerEditor.InitPlayerSearch();

                Dispatcher.Invoke(() => {

                    btnProjectNew.IsEnabled = true;
                    btnProjectOpen.IsEnabled = true;
                    btnProjectSave.IsEnabled = true;
                    btnProjectWriteToMod.IsEnabled = true;
                    //btnProjectWriteToFIFAMod.IsEnabled = true;
                    btnOpenModDetailsPanel.IsEnabled = true;
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

            LauncherOptions = LauncherOptions.LoadAsync().Result;
            swUseModData.IsOn = LauncherOptions.UseModData.HasValue ? LauncherOptions.UseModData.Value : true;
        }

        LauncherOptions LauncherOptions { get; set; }

        private void btnBrowseFIFADirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Find your FIFA exe";
            dialog.Multiselect = false;
            dialog.Filter = "exe files (*.exe)|*.exe";
            dialog.FilterIndex = 0;
            dialog.ShowDialog(this);
            var filePath = dialog.FileName;
            if (!string.IsNullOrEmpty(filePath))
            {
                


                    //BuildTextureBrowser(null);

                //Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    GameplayMain.Initialize();
                //}));

            }

            
            //_ = Start();
        }

        //private void btnLaunchFIFAMods(object sender, RoutedEventArgs e)
        //{
        //    ProjectManagement.FrostyProject.Save("test_gp_speed_change.fbproject");
        //    ProjectManagement.FrostyProject.WriteToMod("test_gp_speed_change.fbmod"
        //        , new ModSettings() { Author = "paulv2k4", Category = "Gameplay", Description = "Gameplay Test", Title = "Gameplay Test", Version = "1.00" });

        //    paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
        //    frostyModExecutor.Run(AssetManager.Instance.fs, this, "", "", new System.Collections.Generic.List<string>() { @"test_gp_speed_change.fbmod" }.ToArray()).Wait();

        //}

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

        private void btnProjectWriteToMod_Click(object sender, RoutedEventArgs e)
        {
            // ---------------------------------------------------------
            // Remove chunks and actual unmodified files before writing
            LegacyFileManager_M21.CleanUpChunks();

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
            catch(Exception SaveException)
            {
                LogError(SaveException.ToString());
            }
        }

        private void btnProjectWriteToFIFAMod_Click(object sender, RoutedEventArgs e)
        {
            // ---------------------------------------------------------
            // Remove chunks and actual unmodified files before writing
            LegacyFileManager_M21.CleanUpChunks();

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "FET files|*.fifamod";
                var resultValue = saveFileDialog.ShowDialog();
                if (resultValue.HasValue && resultValue.Value)
                {
                    ProjectManagement.Project.WriteToFIFAMod(saveFileDialog.FileName, ProjectManagement.Project.ModSettings);
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

        private void CompileLMOD(string filename, bool tryEncrypt = true)
        {
            try
            {
                List<string> listOfCompilableFiles = new List<string>();
                var index = 0;
                var allFiles = Directory.GetFiles("Temp", "*.*", SearchOption.AllDirectories).Where(x => !x.Contains(".mod"));
                foreach (var file in allFiles)
                {
                    StringBuilder sbFinalResult = new StringBuilder();

                    var encrypt = !file.Contains(".dds")
                        && !file.Contains(".db") && tryEncrypt;

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
                        v2k4EncryptionInterop.encryptFile(file, sbFinalResult.ToString());
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
            catch (DllNotFoundException)
            {
                if (tryEncrypt)
                {
                    LogError("Error in trying to Encrypt your files. Will revert to unencrypted version of lmod instead.");
                    CompileLMOD(filename, false);
                }
            }
            catch (Exception e)
            {
                LogError("Error in LMOD Compile. Report the message below to Paulv2k4.");
                LogError(e.Message);
            }

        }

        private async void btnProjectSave_Click(object sender, RoutedEventArgs e)
        {
            await SaveProjectWithDialog();
        }

        private async Task<bool> SaveProjectWithDialog()
        {
            LoadingDialog loadingDialog = new LoadingDialog("Saving Project", "Cleaning loose Legacy Files");
            loadingDialog.Show();
            await Task.Delay(100);
            // ---------------------------------------------------------
            // Remove chunks and actual unmodified files before writing
            LegacyFileManager_M21.CleanUpChunks();

            loadingDialog.Close();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Project files|*.fbproject";
            var result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if (!string.IsNullOrEmpty(saveFileDialog.FileName))
                {
                    loadingDialog = new LoadingDialog("Saving Project", "Saving project to file");
                    loadingDialog.Show();
                    await ProjectManagement.Project.SaveAsync(saveFileDialog.FileName, true);

                    lstProjectFiles.ItemsSource = null;
                    lstProjectFiles.ItemsSource = ProjectManagement.Project.ModifiedAssetEntries;

                    Log("Saved project successfully to " + saveFileDialog.FileName);

                    RecentProjectFiles.Add(new FileInfo(saveFileDialog.FileName));
                    RecentProjectFiles = recentProjectFiles;

                    WindowTitle = saveFileDialog.FileName;


                }
            }
            loadingDialog.Close();
            loadingDialog = null;
            return true;
        }

        private async void btnProjectOpen_Click(object sender, RoutedEventArgs e)
        {
            LoadingDialog loadingDialog = new LoadingDialog("Loading Project", "Cleaning loose Legacy Files");
            loadingDialog.Show();
            await Task.Delay(100);
            // ---------------------------------------------------------
            // Remove chunks and actual unmodified files before writing
            LegacyFileManager_M21.CleanUpChunks();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Project files|*.fbproject";
            var result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if (!string.IsNullOrEmpty(openFileDialog.FileName))
                {
                    await loadingDialog.UpdateAsync("Loading Project", "Loading Project File");

                    ProjectManagement.Project.ModifiedAssetEntries = null;
                    await ProjectManagement.Project.LoadAsync(openFileDialog.FileName);
                    lstProjectFiles.ItemsSource = null;
                    lstProjectFiles.ItemsSource = ProjectManagement.Project.ModifiedAssetEntries;

                    Log("Opened project successfully from " + openFileDialog.FileName);

                    WindowTitle = openFileDialog.FileName;
                    
                }
            }
            loadingDialog.Close();
            loadingDialog = null;
        }

        public Random RandomSaver = new Random();

        private async void btnLaunchFIFAInEditor_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => { btnLaunchFIFAInEditor.IsEnabled = false; });

            Log("Autosaving Project");
            await Task.Run(() =>
            {
                ProjectManagement.Project.Save("Autosave-" + RandomSaver.Next().ToString() + ".fbproject");
            });

            //Log("Deleting old test mods");
            foreach (var tFile in Directory.GetFiles(App.ApplicationDirectory, "*.fbmod")) { File.Delete(tFile); };

            var testmodname = "test-" + RandomSaver.Next().ToString() + ".fbmod";

            // ---------------------------------------------------------
            // Remove chunks and actual unmodified files before writing
            LegacyFileManager_M21.CleanUpChunks();

            var author = ProjectManagement.Project.ModSettings != null ? ProjectManagement.Project.ModSettings.Author : string.Empty;
            var category = ProjectManagement.Project.ModSettings != null ? ProjectManagement.Project.ModSettings.Author : string.Empty;
            var desc = ProjectManagement.Project.ModSettings != null ? ProjectManagement.Project.ModSettings.Author : string.Empty;
            var title = ProjectManagement.Project.ModSettings != null ? ProjectManagement.Project.ModSettings.Author : string.Empty;
            var version = ProjectManagement.Project.ModSettings != null ? ProjectManagement.Project.ModSettings.Author : string.Empty;
            ProjectManagement.Project.WriteToMod(testmodname
                , new ModSettings() { Author = author, Category = category, Description = desc, Title = title, Version = version });


            /*
            if (chkEnableLegacyInjection.IsChecked.HasValue && chkEnableLegacyInjection.IsChecked.Value)
            {
                var modifiedLegacy = ProjectManagement.Project.AssetManager.EnumerateCustomAssets("legacy", true).ToList();

                modifiedLegacy.ForEach(
                    entry =>
                    {

                        var extractPath = AssetManager.Instance.fs.BasePath + "/LegacyMods/Legacy/" + entry.Path;
                        if (!string.IsNullOrEmpty(extractPath))
                        {
                            if (!Directory.Exists(extractPath))
                                Directory.CreateDirectory(extractPath);


                            if (File.Exists(extractPath + "/" + entry.Filename + ".mod"))
                            {
                                File.Delete(extractPath + "/" + entry.Filename + ".mod");
                            }

                            var aE = AssetManager.Instance.GetCustomAssetEntry("legacy", "");
                            if (aE != null)
                            {
                                var streamAsset = AssetManager.Instance.GetCustomAsset("legacy", aE);
                                if (streamAsset != null)
                                {
                                    var bytes = new NativeReader(streamAsset).ReadToEnd();
                                    File.WriteAllBytes(extractPath + "/" + entry.Filename + "." + entry.Type, bytes);
                                }
                            }
                        }


                    });
            }
            */
            var useModData = swUseModData.IsOn;
            await Task.Run(() =>
            {
                paulv2k4ModdingExecuter.FrostyModExecutor frostyModExecutor = new paulv2k4ModdingExecuter.FrostyModExecutor();
                paulv2k4ModdingExecuter.FrostyModExecutor.UseModData = useModData;
                frostyModExecutor.UseSymbolicLinks = false;
                frostyModExecutor.ForceRebuildOfMods = true;
                frostyModExecutor.Run(this, GameInstanceSingleton.GAMERootPath, "", new System.Collections.Generic.List<string>() { testmodname }.ToArray()).Wait();
            });

            

            await InjectLegacyDLL();

            await Dispatcher.InvokeAsync(() => { btnLaunchFIFAInEditor.IsEnabled = true; });

        }

        private async Task<bool> InjectLegacyDLL()
        {
            try
            {
                if (swEnableLegacyInjection.IsOn)
                {
                    string legacyModSupportFile = null;
                    if (GameInstanceSingleton.GAMEVERSION == "FIFA20")
                    {
                        legacyModSupportFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA20Legacy.dll";
                    }
                    else if (ProfilesLibrary.IsFIFA21DataVersion())
                    {
                        legacyModSupportFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA.dll";
                    }

                    if (!string.IsNullOrEmpty(legacyModSupportFile))
                    {

                        if (File.Exists(legacyModSupportFile))
                        {
                            if (File.Exists(GameInstanceSingleton.GAMERootPath + "v2k4LegacyModSupport.dll"))
                                File.Delete(GameInstanceSingleton.GAMERootPath + "v2k4LegacyModSupport.dll");

                            File.Copy(legacyModSupportFile, GameInstanceSingleton.GAMERootPath + "v2k4LegacyModSupport.dll");

                            if (File.Exists(@GameInstanceSingleton.GAMERootPath + @"v2k4LegacyModSupport.dll"))
                            {
                                var legmodsupportdllpath = @GameInstanceSingleton.GAMERootPath + @"v2k4LegacyModSupport.dll";
                                return await GameInstanceSingleton.InjectDLLAsync(legmodsupportdllpath);
                            }
                        }


                    }
                }
            }
            catch(Exception e)
            {
                LogError(e.ToString());
            }

            return false;
        }

        private void btnProjectNew_Click(object sender, RoutedEventArgs e)
        {
            AssetManager.Instance.Reset();
            Log("Asset Manager Reset");
            ProjectManagement.Project = new FrostbiteProject(AssetManager.Instance, AssetManager.Instance.fs);
            ProjectManagement.Project.ModifiedAssetEntries = null;
            lstProjectFiles.ItemsSource = null;
            WindowTitle = "New Project";
            Log("New Project Created");
        }

        private async void btnCompileLegacyModFromFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.AllowMultiSelect = false;
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var legacyDirectory = dialog.SelectedFolder;
                    await CompileLegacyModFromFolder(legacyDirectory);
                }
            }
        }

        private readonly string LegacyProjectExportedFolder = App.ApplicationDirectory + "\\TEMPLEGACYPROJECT";
        private readonly string LegacyTempFolder = App.ApplicationDirectory + "\\TEMP";

        private async Task<bool> CompileLegacyModFromFolder(string legacyDirectory)
        {
            if (string.IsNullOrEmpty(legacyDirectory))
            {
                MessageBox.Show("Inputted Legacy Directory is Empty", "Save Failed");
                return false;
            }

                await Task.Run(() =>
                {
                    DirectoryInfo directoryInfoTemp = new DirectoryInfo(LegacyTempFolder);
                    if (directoryInfoTemp.Exists)
                    {
                        RecursiveDelete(directoryInfoTemp);
                    }

                    DirectoryCopy(legacyDirectory, LegacyTempFolder, true);
                    List<string> listOfCompilableFiles = new List<string>();

                    var allFiles = Directory.GetFiles(LegacyTempFolder, "*.*", SearchOption.AllDirectories).Where(x => !x.Contains(".mod"));
                    Task[] tasks = new Task[allFiles.Count()];
                    foreach (var file in allFiles)
                    {
                        var fIFile = new FileInfo(file);
                        //tasks[index] = Task.Run(() =>
                        //{
                        StringBuilder sbFinalResult = new StringBuilder();

                        var encrypt = !fIFile.Extension.Contains(".dds", StringComparison.OrdinalIgnoreCase)
                            && !fIFile.Extension.Contains(".db", StringComparison.OrdinalIgnoreCase)
                            && !fIFile.Extension.Contains(".loc", StringComparison.OrdinalIgnoreCase);

                        if (encrypt)
                        {
                            var lastIndexOfDot = fIFile.FullName.LastIndexOf('.');
                            var newFile = fIFile.FullName.Substring(0, lastIndexOfDot) + ".mod";
                            sbFinalResult.Append(newFile);
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
                            v2k4EncryptionInterop.encryptFile(file, sbFinalResult.ToString());
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
                                var indexOfTemp = compatFile.LastIndexOf("TEMP\\");
                                var newZipLocation = compatFile.Substring(indexOfTemp + 5, compatFile.Length - (indexOfTemp + 5));
                                zipArchive.CreateEntryFromFile(compatFile, newZipLocation);
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

                    }

                    if (directoryInfoTemp.Exists)
                    {
                        RecursiveDelete(directoryInfoTemp);
                        Log("Legacy Compiler :: Cleaned up encrypted files");
                    }


                });

            return true;
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

        private void btnOpenModDetailsPanel_Click(object sender, RoutedEventArgs e)
        {
            var mdw = new ModDetailsWindow();
            mdw.Show();
        }

        private void btnRevertAsset_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            AssetEntry assetEntry = button.Tag as AssetEntry;
            AssetManager.Instance.RevertAsset(assetEntry);
            lstProjectFiles.ItemsSource = null;
            lstProjectFiles.ItemsSource = ProjectManagement.Project.ModifiedAssetEntries;
            Log("Reverted Asset " + assetEntry.Name);
        }


        void CreateTestEditor()
        {
            dynamic dHotspotObject = new ExpandoObject { };
            List<dynamic> testDList = new List<dynamic>();
            for (System.Single i = 0; i < 20; i++)
            {
                testDList.Add(new
                {
                    Bounds = new { x = 0, y = 0, z = 0, w = i }
                    , Group = new CString("Test G " + i.ToString())
                    , Name = new CString("Test N " + i.ToString())
                    , Rotation = (float)1.45 + i
                });
            }
            dHotspotObject.Name = "Test Obj";
            dHotspotObject.Hotspots = testDList;
            EbxAsset testEbxAsset = new EbxAsset();
            testEbxAsset.SetRootObject(dHotspotObject);

            TestEbxViewer.Children.Add(
                new Editor(testEbxAsset));

        }

        private void btnProjectSaveToFIFAProject_Click(object sender, RoutedEventArgs e)
        {
            // ---------------------------------------------------------
            // Remove chunks and actual unmodified files before writing
            LegacyFileManager_M21.CleanUpChunks();


            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "FIFA Project files|*.fifaproject";
            var result = saveFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if (!string.IsNullOrEmpty(saveFileDialog.FileName))
                {
                    FIFAEditorProject.ConvertFromFbProject(ProjectManagement.Project, saveFileDialog.FileName);

                    //FIFAEditorProject project = new FIFAEditorProject("FIFA21", AssetManager.Instance, AssetManager.Instance.fs);
                    //project.Save(saveFileDialog.FileName);

                    lstProjectFiles.ItemsSource = null;
                    lstProjectFiles.ItemsSource = ProjectManagement.Project.ModifiedAssetEntries;

                    Log("Saved project successfully to " + saveFileDialog.FileName);
                }
            }
        }

        public void UpdateAllBrowsers()
        {
            dataBrowser.UpdateAssetListView();
            textureBrowser.UpdateAssetListView();
            gameplayBrowser.UpdateAssetListView();
            legacyBrowser.UpdateAssetListView();
        }

        private void btnOpenModBundler_Click(object sender, RoutedEventArgs e)
        {
            FrostbiteModBundler frostbiteModBundler = new FrostbiteModBundler();
            frostbiteModBundler.ShowDialog();
        }

        private void btnCleanUpLegacyFiles_Click(object sender, RoutedEventArgs e)
        {
            LegacyFileManager_M21.CleanUpChunks();
            Log("Legacy files have been cleaned");
        }

        private async void btnProjectWriteToLegacyMod_Click(object sender, RoutedEventArgs e)
        {
            if(ProjectManagement.Project.AssetManager.EnumerateCustomAssets("legacy", true).Count() == 0)
            {
                MessageBox.Show("You do not have any modified legacy items to save!", "Save Failed");
                return;
            }

            LoadingDialog loadingDialog = new LoadingDialog("Saving Legacy Mod", "Exporting files");
            loadingDialog.Show();

            var exportFolder = LegacyProjectExportedFolder;
            if (!Directory.Exists(exportFolder))
                Directory.CreateDirectory(exportFolder);

            RecursiveDelete(new DirectoryInfo(exportFolder));

            foreach (var f in ProjectManagement.Project.AssetManager.EnumerateCustomAssets("legacy", true))
            {
                LegacyFileEntry lfe = f as LegacyFileEntry;
                if(lfe != null)
                {
                    await loadingDialog.UpdateAsync("Saving Legacy Mod", "Exporting " + lfe.Filename);
                    var lfeStream = (MemoryStream)ProjectManagement.Project.AssetManager.GetCustomAsset("legacy", lfe);

                    if (!Directory.Exists(exportFolder + "\\" + lfe.Path))
                        Directory.CreateDirectory(exportFolder + "\\" + lfe.Path);

                    using (var nw = new NativeWriter(new FileStream(exportFolder + "\\" + lfe.Path + "\\" + lfe.Filename + "." + lfe.Type, FileMode.Create)))
                    {
                        nw.WriteBytes(lfeStream.ToArray());
                    }
                }
            }

            await CompileLegacyModFromFolder(exportFolder);

            loadingDialog.Close();
            loadingDialog = null;


        }

        private void btnOpenEmbeddedFilesWindow_Click(object sender, RoutedEventArgs e)
        {
            FrostbiteModEmbeddedFiles frostbiteModEmbeddedFiles = new FrostbiteModEmbeddedFiles();
            frostbiteModEmbeddedFiles.ShowDialog();
        }
    }
}
