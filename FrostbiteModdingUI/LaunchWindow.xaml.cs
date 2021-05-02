using CareerExpansionMod.CEM;
using FIFAModdingUI.Mods;
using FIFAModdingUI.Windows;
using FIFAModdingUI.Windows.Profile;
using FrostbiteModdingUI.Models;
using FrostbiteModdingUI.Windows;
using FrostySdk;
using FrostySdk.Frosty;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using Microsoft.Win32;
using paulv2k4ModdingExecuter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
using System.Windows.Shapes;
using v2k4FIFAModding;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for LaunchWindow.xaml
    /// </summary>
    public partial class LaunchWindow : Window, ILogger
    {
        public string WindowTitle { get; set; }

        public ModListProfile Profile { get; set; }

        public LaunchWindow(Window owner)
        {
            Owner = owner;
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly();
            WindowTitle = "FMT Launcher - " + System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
            DataContext = this;

            App.AppInsightClient.TrackRequest("Launcher Window - " + WindowTitle, DateTimeOffset.Now,
                   TimeSpan.FromMilliseconds(0), "200", true);

            try
            {
                //if (!File.Exists(AppSettings.Settings.FIFAInstallEXEPath))
                //{
                //    AppSettings.Settings.FIFAInstallEXEPath = null;
                //}

                if (!string.IsNullOrEmpty(AppSettings.Settings.GameInstallEXEPath))
                {
                    //txtFIFADirectory.Text = AppSettings.Settings.GameInstallEXEPath;
                    InitialiseSelectedGame(AppSettings.Settings.GameInstallEXEPath);
                }
                else
                {
                    var bS = new FindGameEXEWindow().ShowDialog();
                    if (bS.HasValue && !string.IsNullOrEmpty(AppSettings.Settings.GameInstallEXEPath))
                    {
                        InitialiseSelectedGame(AppSettings.Settings.GameInstallEXEPath);
                    }
                    else
                    {
                        throw new Exception("Unable to start up Game");
                    }
                }
            }
            catch (Exception e)
            {
                //txtFIFADirectory.Text = "";
                Trace.WriteLine(e.ToString());
            }

            bool? result = false;
            BuildSDKAndCache buildSDKAndCacheWindow = new BuildSDKAndCache();
            if (buildSDKAndCacheWindow.DoesCacheNeedsRebuilding())
            {
                result = buildSDKAndCacheWindow.ShowDialog();
            }


        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            ProjectManagement.Instance = null;
            if (AssetManager.Instance != null)
            {
                AssetManager.Instance.Dispose();
                AssetManager.Instance = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            AppSettings.Settings.GameInstallEXEPath = null;
            Owner.Visibility = Visibility.Visible;
        }

        private ObservableCollection<ModList.ModItem> ListOfMods = new ObservableCollection<ModList.ModItem>();

        public bool EditorModIncluded { get; internal set; }

        private void up_click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = this.listMods.SelectedIndex;

            if (selectedIndex > -1 && selectedIndex > 0)
            {
                var itemToMoveUp = this.ListOfMods[selectedIndex];
                this.ListOfMods.RemoveAt(selectedIndex);
                this.ListOfMods.Insert(selectedIndex - 1, itemToMoveUp);
                this.listMods.SelectedIndex = selectedIndex - 1;

                var mL = new Mods.ModList(Profile);
                mL.ModListItems.Swap(selectedIndex -1, selectedIndex);
                mL.Save();
            }
        }

        private void down_click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = this.listMods.SelectedIndex;

            if (selectedIndex > -1 && selectedIndex + 1 < this.ListOfMods.Count)
            {
                var itemToMoveDown = this.ListOfMods[selectedIndex];
                this.ListOfMods.RemoveAt(selectedIndex);
                this.ListOfMods.Insert(selectedIndex + 1, itemToMoveDown);
                this.listMods.SelectedIndex = selectedIndex + 1;


                var mL = new Mods.ModList(Profile);
                mL.ModListItems.Swap(selectedIndex, selectedIndex + 1);
                mL.Save();
            }
        }

        private void GetListOfModsAndOrderThem()
        {
            // Load last profile


            // get profile list
            var items = ModListProfile.LoadAll().Select(x => x.ProfileName).ToList();
            foreach(var i in items)
            {
                var profButton = new Button() { Content = i };
                profButton.Click += (object sender, RoutedEventArgs e) => { };
                //cbProfiles.Items.Add(profButton);

            }
            var addnewprofilebutton = new Button() { Content = "Add new profile" };
            addnewprofilebutton.Click += Addnewprofilebutton_Click;
            //cbProfiles.Items.Add(addnewprofilebutton);

            // load list of mods
            var modItems = new ModList(Profile).ModListItems;
            ListOfMods = new ObservableCollection<ModList.ModItem>(modItems);
            listMods.ItemsSource = ListOfMods;
            
        }

        private void Addnewprofilebutton_Click(object sender, RoutedEventArgs e)
        {
            var newmodlistprofilewindow = new AddNewModListProfile();
            newmodlistprofilewindow.Show();
            newmodlistprofilewindow.Closed += Newmodlistprofilewindow_Closed;
        }

        private void Newmodlistprofilewindow_Closed(object sender, EventArgs e)
        {
            AddNewModListProfile newmodlistprofilewindow = sender as AddNewModListProfile;

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

        bool stopLoggingUntilComplete = false;

        public async void LogAsync(string in_text)
        {
            if (stopLoggingUntilComplete)
                return;

            if(in_text.Contains("Read out of Cache"))
            {
                stopLoggingUntilComplete = true;

            }
            if (in_text.Contains("Loading complete "))
            {
                stopLoggingUntilComplete = false;
            }

            var txt = string.Empty;
            Dispatcher.Invoke(() => {
                txt = txtLog.Text;
            });

            var text = await Task.Run(() =>
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append(txt);
                stringBuilder.AppendLine(in_text);
                if(stopLoggingUntilComplete)
                {
                    stringBuilder.AppendLine("Please wait for compiler to finish first load. This may take 10-20 minutes");
                }

                return stringBuilder.ToString();
            });

            await Dispatcher.InvokeAsync(() =>
            {
                txtLog.Text = text;
                txtLog.ScrollToEnd();
            });

        }

        public void LogSync(string in_text)
        {
            Dispatcher.Invoke(() => {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(txtLog.Text);
                stringBuilder.AppendLine(in_text);
                txtLog.Text = stringBuilder.ToString();
            });
        }

        /// <summary>
        /// Setup and Extract lmods to the LegacyMods folder
        /// </summary>
        private void DoLegacyModSetup()
        {
            if (chkCleanLegacyModDirectory.IsChecked.HasValue && chkCleanLegacyModDirectory.IsChecked.Value)
            {
                RecursiveDelete(new DirectoryInfo(GameInstanceSingleton.LegacyModsPath));
            }

            if (!Directory.Exists(GameInstanceSingleton.GAMERootPath + "\\LegacyMods\\"))
                Directory.CreateDirectory(GameInstanceSingleton.GAMERootPath + "\\LegacyMods\\");
            if (!Directory.Exists(GameInstanceSingleton.LegacyModsPath))
                Directory.CreateDirectory(GameInstanceSingleton.LegacyModsPath);

            if (chkUseLegacyModSupport.IsChecked.HasValue && chkUseLegacyModSupport.IsChecked.Value)
            {
                foreach (var lmodZipped in ListOfMods.Select(x=>x.Path).Where(x => x.Contains(".zip")))
                {
                    using (FileStream fsZip = new FileStream(lmodZipped, FileMode.Open))
                    {
                        ZipArchive zipA = new ZipArchive(fsZip);
                        foreach (var zipEntry in zipA.Entries.Where(x => x.FullName.Contains(".lmod")))
                        {
                            ZipArchive zipLMod = new ZipArchive(zipEntry.Open());
                            foreach (var zipEntLMOD in zipA.Entries)
                            {
                                if (File.Exists(GameInstanceSingleton.GAMERootPath + "\\LegacyMods\\Legacy\\" + zipEntLMOD.FullName))
                                {
                                    File.Delete(GameInstanceSingleton.GAMERootPath + "\\LegacyMods\\Legacy\\" + zipEntLMOD.FullName);
                                }
                            }
                            zipLMod.ExtractToDirectory(GameInstanceSingleton.GAMERootPath + "\\LegacyMods\\Legacy\\");
                        }
                    }
                }
                foreach (var lmod in ListOfMods.Select(x => x.Path).Where(x => x.Contains(".lmod")))
                {
                    using (FileStream fs = new FileStream(lmod, FileMode.Open))
                    {
                        ZipArchive zipA = new ZipArchive(fs);
                        foreach (var ent in zipA.Entries)
                        {
                            if (File.Exists(GameInstanceSingleton.GAMERootPath + "\\LegacyMods\\Legacy\\" + ent.FullName))
                            {
                                File.Delete(GameInstanceSingleton.GAMERootPath + "\\LegacyMods\\Legacy\\" + ent.FullName);
                            }
                        }
                        zipA.ExtractToDirectory(GameInstanceSingleton.GAMERootPath + "\\LegacyMods\\Legacy\\");
                    }
                }
            }

        }

        private async void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            var launchStartTime = DateTimeOffset.Now;
            if (!string.IsNullOrEmpty(GameInstanceSingleton.GAMEVERSION) && !string.IsNullOrEmpty(GameInstanceSingleton.GAMERootPath))
            {
                // -------------------------------------
                // Ensure this is the logger
                //
                GameInstanceSingleton.Logger = this;

                TabCont.SelectedIndex = 0;
                DoLegacyModSetup();

                // Copy the Locale.ini if checked
                if (chkInstallLocale.IsChecked.Value)
                {
                    foreach (var z in ListOfMods.Select(x => x.Path).Where(x => x.Contains(".zip")))
                    {
                        using (FileStream fs = new FileStream(z, FileMode.Open))
                        {
                            ZipArchive zipA = new ZipArchive(fs);
                            foreach (var ent in zipA.Entries)
                            {
                                if (ent.Name.Contains("locale.ini"))
                                {
                                    ent.ExtractToFile(GameInstanceSingleton.FIFALocaleINIPath, true);
                                }
                            }
                        }
                    }
                }

                //var k = chkUseFileSystem.IsChecked.Value;
                var useLegacyMods = chkUseLegacyModSupport.IsChecked.Value;
                var useLiveEditor = chkUseLiveEditor.IsChecked.Value;
                var useSymbolicLink = chkUseSymbolicLink.IsChecked.Value;
                // Start the game with mods
                await new TaskFactory().StartNew(async () =>
                {

                Dispatcher.Invoke(() =>
                {
                    btnLaunch.IsEnabled = false;
                    btnLaunchOtherTool.IsEnabled = false;
                });
                await Task.Delay(1000);

                    //try
                    //{
                    //    if (AssetManager.Instance == null)
                    //    {
                    //        if (string.IsNullOrEmpty(GameInstanceSingleton.GAMERootPath))
                    //            throw new Exception("Game path has not been selected or initialized");

                    //        if (string.IsNullOrEmpty(GameInstanceSingleton.GAMEVERSION))
                    //            throw new Exception("Game EXE has not been selected or initialized");

                    //        Log("Asset Manager is not initialised - Starting");
                    //        ProjectManagement projectManagement = new ProjectManagement(
                    //            GameInstanceSingleton.GAMERootPath + "\\" + GameInstanceSingleton.GAMEVERSION + ".exe"
                    //            , this);

                    //        if(AssetManager.Instance == null)
                    //        {
                    //            throw new Exception("Asset Manager has not been loaded against " + GameInstanceSingleton.GAMERootPath + "\\" + GameInstanceSingleton.GAMEVERSION + ".exe");
                    //        }
                    //        Log("Asset Manager loading complete");
                    //    }
                    //}
                    //catch(Exception AssetManagerException)
                    //{
                    //    LogError(AssetManagerException.ToString());
                    //    return;
                    //}
                    //Dispatcher.Invoke(() =>
                    //{

                    Log("Mod Compiler Started for " + GameInstanceSingleton.GAMEVERSION);

                    var launchSuccess = false;
                    try
                    {
                        var launchTask = LaunchFIFA.LaunchAsync(
                            GameInstanceSingleton.GAMERootPath
                            , ""
                            , new Mods.ModList(Profile).ModListItems.Select(x=>x.Path).ToList()
                            , this
                            , GameInstanceSingleton.GAMEVERSION
                            , true
                            , useSymbolicLink);
                        launchSuccess = await launchTask;

                        App.AppInsightClient.TrackRequest("Launcher Window - " + WindowTitle, launchStartTime,
                            TimeSpan.FromMilliseconds((DateTime.Now - launchStartTime).Milliseconds), "200", true);
                    }
                    catch(Exception launchException)
                    {
                        Log("[ERROR] Error caught in Launch Task. You must fix the error before using this Launcher.");
                        LogError(launchException.ToString());

                        App.AppInsightClient.TrackRequest("Launcher Window - " + WindowTitle, launchStartTime,
                           TimeSpan.FromMilliseconds((DateTime.Now - launchStartTime).Milliseconds), "200", false);

                        App.AppInsightClient.TrackException(launchException);

                    }
                    if (launchSuccess)
                    {

                        if (useLegacyMods)
                        {
                            var runningLocation = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                            LogSync("Legacy Injection - Tool running location is " + runningLocation);

                            string legacyModSupportFile = null;
                            if (GameInstanceSingleton.GAMEVERSION == "FIFA20")
                            {
                                LogSync("Legacy Injection - FIFA 20 found. Using FIFA20Legacy.DLL.");

                                //legacyModSupportFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA20Legacy.dll";
                                legacyModSupportFile = runningLocation + @"\FIFA20Legacy.dll";
                            }
                            else if (ProfilesLibrary.IsFIFA21DataVersion())// GameInstanceSingleton.GAMEVERSION == "FIFA21")
                            {
                                LogSync("Legacy Injection - FIFA 21 found. Using FIFA.DLL.");
                                //legacyModSupportFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA.dll";
                                legacyModSupportFile = runningLocation + @"\FIFA.dll";
                            }

                            if (!File.Exists(legacyModSupportFile))
                            {
                                LogError($"Legacy Injection - Unable to find Legacy Injection DLL {legacyModSupportFile}");
                            }
                            else
                            {
                                var legmodsupportdllpath = @GameInstanceSingleton.GAMERootPath + @"v2k4LegacyModSupport.dll";

                                LogSync("Copying " + legacyModSupportFile + " to " + legmodsupportdllpath);
                                await Task.Delay(500);
                                File.Copy(legacyModSupportFile, legmodsupportdllpath, true);
                                await Task.Delay(500);

                                try
                                {
                                    LogSync("Injecting Live Legacy Mod Support");
                                    await Task.Delay(500);
                                    bool InjectionSuccess = await GameInstanceSingleton.InjectDLLAsync(legmodsupportdllpath);
                                    if (InjectionSuccess)
                                        LogSync("Injected Live Legacy Mod Support");
                                    else
                                        LogError("Launcher could not inject Live Legacy File Support");

                                }
                                catch (Exception InjectDLLException)
                                {
                                    LogError("Launcher could not inject Live Legacy File Support");
                                    LogError(InjectDLLException.ToString());

                                    App.AppInsightClient.TrackException(InjectDLLException);

                                }
                            }
                        }

                        if (useLiveEditor)
                        {
                            if (File.Exists(@GameInstanceSingleton.GAMERootPath + @"FIFALiveEditor.DLL"))
                                await GameInstanceSingleton.InjectDLLAsync(@GameInstanceSingleton.GAMERootPath + @"FIFALiveEditor.DLL");
                        }

                        Dispatcher.Invoke(() => {
                            var presence = new DiscordRPC.RichPresence();
                            presence.Details = "Playing " + GameInstanceSingleton.GAMEVERSION + " with " + ListOfMods.Count + " mods";
                            presence.State = "Playing Solo";
                            App.DiscordRpcClient.SetPresence(presence);
                            App.DiscordRpcClient.Invoke();
                        });

                        if (AssetManager.Instance != null)
                        {
                            AssetManager.Instance.Reset();
                            // Do Cleanup of Resources - Saving Memory
                            AssetManager.Instance.Dispose();
                            AssetManager.Instance = null;
                        }
                        ProjectManagement.Instance = null;

                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                    }
                    //});
                    await Task.Delay(1000);


                    Dispatcher.Invoke(() =>
                    {
                        if (chkUseCEM.IsChecked.HasValue && chkUseCEM.IsChecked.Value && ProfilesLibrary.IsFIFA21DataVersion())
                        {
                            CEMWindow = new CEMWindow();
                            CEMWindow.Show();
                        }
                    });


                    Dispatcher.Invoke(() =>
                    {
                        btnLaunch.IsEnabled = true;
                        btnLaunchOtherTool.IsEnabled = true;
                    });



                });
            }
        }

        private CEMWindow CEMWindow;

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = this.listMods.SelectedIndex;
            if (selectedIndex > -1)
            {
                var mL = new Mods.ModList(Profile);
                mL.ModListItems.Remove(this.ListOfMods[selectedIndex]);
                mL.Save();
                this.ListOfMods.RemoveAt(selectedIndex);

                GetListOfModsAndOrderThem();
            }

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Browse for your mod";
            dialog.Multiselect = false;
            // remove zip as not compatible with *.fifamod / FIFA 21
            //dialog.Filter = "Mod files (*.zip, *.lmod, *.fbmod, *.fifamod)|*.zip;*.lmod;*.fbmod;*.fifamod";
            var supportedFiles = ProfilesLibrary.SupportedLauncherFileTypes;
            var filter = "Mod files (*.fbmod)|*.fbmod";
            if (supportedFiles != null && supportedFiles.Count > 0)
            {
                filter = "Mod files (";
                for (var i = 0; i < supportedFiles.Count; i++)
                {
                    filter += "*." + supportedFiles[i];
                    if (i < supportedFiles.Count - 1)
                        filter += ",";
                }
                filter += ")|";
                for (var i = 0; i < supportedFiles.Count; i++)
                {
                    filter += "*." + supportedFiles[i];
                    if (i < supportedFiles.Count - 1)
                        filter += ";";
                }
            }
            dialog.Filter = filter;//"Mod files (*.lmod, *.fbmod, *.fifamod)|*.lmod;*.fbmod;*.fifamod";
            dialog.FilterIndex = 0;
            dialog.ShowDialog(this);
            var filePath = dialog.FileName;
            if (!string.IsNullOrEmpty(filePath))
            {
                var mL = new Mods.ModList(Profile);
                mL.ModListItems.Add(new ModList.ModItem(filePath));
                mL.Save();
                GetListOfModsAndOrderThem();
            }

        }

        private void btnCloseModdingTool_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //private void btnBrowseFIFADirectory_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new OpenFileDialog();
        //    dialog.Title = "Find your FIFA exe";
        //    dialog.Multiselect = false;
        //    dialog.Filter = "exe files (*.exe)|*.exe";
        //    dialog.FilterIndex = 0;
        //    dialog.ShowDialog(this);
        //    var filePath = dialog.FileName;
        //    InitializeOfSelectedGame(filePath);

        //}

        private void InitialiseSelectedGame(string filePath)
        {
            if(!File.Exists(filePath))
            {
                LogError($"Game EXE Path {filePath} doesn't exist!");
                return;
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                this.DataContext = this;
                AppSettings.Settings.GameInstallEXEPath = filePath;
                //AppSettings.Settings.Save();

                if(GameInstanceSingleton.InitializeSingleton(filePath))
                {
                    if (!ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
                    {
                        throw new Exception("Unable to Initialize Profile");
                    }
                    btnLaunch.IsEnabled = true;
                    GameInstanceSingleton.Logger = this;

                }
                else
                {
                    throw new Exception("Unsupported Game EXE Selected");
                }

                var presence = new DiscordRPC.RichPresence();
                presence.State = "In Launcher - " + GameInstanceSingleton.GAMEVERSION;
                App.DiscordRpcClient.SetPresence(presence);
                App.DiscordRpcClient.Invoke();

                if (ProfilesLibrary.IsFIFA21DataVersion())
                {
                    //txtWarningAboutPersonalSettings.Visibility = Visibility.Visible;
                    chkUseSymbolicLink.Visibility = Visibility.Collapsed;
                    chkUseSymbolicLink.IsChecked = false;
                    btnLaunchOtherTool.Visibility = Visibility.Visible;

                    btnOpenCEMWindow.Visibility = Visibility.Visible;

                    chkUseCEM.IsEnabled = true;
                }

                if (ProfilesLibrary.IsMadden21DataVersion())
                {
                    chkUseSymbolicLink.Visibility = Visibility.Collapsed;
                    chkUseSymbolicLink.IsChecked = false;
                }

                chkCleanLegacyModDirectory.IsChecked = false;
                chkCleanLegacyModDirectory.IsEnabled = GameInstanceSingleton.IsCompatibleWithLegacyMod();

                chkUseLegacyModSupport.IsEnabled = GameInstanceSingleton.IsCompatibleWithLegacyMod();
                if (!GameInstanceSingleton.IsCompatibleWithLegacyMod())
                    chkUseLegacyModSupport.IsChecked = false;

                chkInstallLocale.IsEnabled = ProfilesLibrary.IsFIFA21DataVersion();
                chkUseLiveEditor.IsEnabled = ProfilesLibrary.IsFIFA21DataVersion();

                if (GameInstanceSingleton.IsCompatibleWithFbMod() || GameInstanceSingleton.IsCompatibleWithLegacyMod())
                {
                    listMods.IsEnabled = true;

                    Profile = new ModListProfile(GameInstanceSingleton.GAMEVERSION);
                    GetListOfModsAndOrderThem();
                }
                else
                {
                    btnAdd.IsEnabled = false;
                    btnRemove.IsEnabled = false;
                    btnUp.IsEnabled = false;
                    btnDown.IsEnabled = false;
                    listMods.IsEnabled = false;
                    //listMods.Items.Clear();
                    new Mods.ModList();
                }
            }

            DataContext = null;
            DataContext = this;
        }

        public void Log(string text, params object[] vars)
        {
            LogAsync(text);
        }

        public void LogWarning(string text, params object[] vars)
        {
            LogAsync("[WARNING] " + text);
        }

        public void LogError(string text, params object[] vars)
        {
            //LogAsync("[ERROR] " + text);
            LogSync("[ERROR] " + text);

            if (File.Exists("ErrorLog.txt"))
                File.Delete("ErrorLog.txt");

            File.WriteAllText("ErrorLog.txt", DateTime.Now.ToString() + " \n" + text);
        }

        public ModList.ModItem SelectedModListItem { get; set; }

        private void listMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.listMods != null && this.listMods.SelectedIndex != -1 && this.listMods.SelectedItem != null) {
                var selectedIndex = this.listMods.SelectedIndex;
                SelectedModListItem = this.listMods.SelectedItem as ModList.ModItem;
                if (SelectedModListItem == null)
                    return;

                this.DataContext = null;
                this.DataContext = this;


                var selectedMod = SelectedModListItem.Path;
                if (selectedMod.Contains(".fbmod")) 
                {
                    var fm = new FrostbiteMod(selectedMod);
                    if (fm.ModDetails != null)
                    {
                        txtModAuthor.Text = fm.ModDetails.Author;
                        txtModDescription.Text = fm.ModDetails.Description;
                        txtModTitle.Text = fm.ModDetails.Title;
                        txtModVersion.Text = fm.ModDetails.Version;
                    }
                }
                else if (selectedMod.Contains(".fifamod"))
                {
                    var fm = new FIFAMod(string.Empty, selectedMod);
                    if (fm.ModDetails != null)
                    {
                        txtModAuthor.Text = fm.ModDetails.Author;
                        txtModDescription.Text = fm.ModDetails.Description;
                        txtModTitle.Text = fm.ModDetails.Title;
                        txtModVersion.Text = fm.ModDetails.Version;
                    }

                }
                else if (selectedMod.Contains(".zip"))
                    {
                        txtModDescription.Text = "Includes the following mods: \n";
                        using (FileStream fsModZipped = new FileStream(selectedMod, FileMode.Open))
                        {
                            ZipArchive zipArchive = new ZipArchive(fsModZipped);
                            foreach (var zaentr in zipArchive.Entries.Where(x => x.FullName.Contains(".fbmod")))
                            {
                                txtModDescription.Text += zaentr.Name + "\n";
                            }
                        }

                        txtModAuthor.Text = "Multiple";
                        txtModTitle.Text = selectedMod;
                        FileInfo fiZip = new FileInfo(selectedMod);
                        txtModVersion.Text = fiZip.CreationTime.ToString();
                    }
            }
        }

        private void cbProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnLegacyModSupportSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLaunchOtherTool_Click(object sender, RoutedEventArgs e)
        {
            DoLegacyModSetup();

            FindOtherLauncherEXEWindow findOtherLauncherEXEWindow = new FindOtherLauncherEXEWindow();
            findOtherLauncherEXEWindow.InjectLegacyModSupport = chkUseLegacyModSupport.IsChecked.Value;
            findOtherLauncherEXEWindow.InjectLiveEditorSupport = chkUseLiveEditor.IsChecked.Value;

            btnLaunchOtherTool.IsEnabled = false;
            findOtherLauncherEXEWindow.ShowDialog();
            btnLaunchOtherTool.IsEnabled = true;

        }

        private void btnOpenCEMWindow_Click(object sender, RoutedEventArgs e)
        {
            if(CEMWindow != null)
            {
                CEMWindow.Close();
                CEMWindow = null;
            }
            CEMWindow = new CEMWindow();
            CEMWindow.ShowDialog();
        }
    }

    static class IListExtensions
    {
        public static void Swap<T>(
            this IList<T> list,
            int firstIndex,
            int secondIndex
        )
        {
            Contract.Requires(list != null);
            Contract.Requires(firstIndex >= 0 && firstIndex < list.Count);
            Contract.Requires(secondIndex >= 0 && secondIndex < list.Count);
            if (firstIndex == secondIndex)
            {
                return;
            }
            T temp = list[firstIndex];
            list[firstIndex] = list[secondIndex];
            list[secondIndex] = temp;
        }
    }
}
