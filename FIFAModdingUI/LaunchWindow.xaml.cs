using FIFAModdingUI.Mods;
using FIFAModdingUI.Windows.Profile;
using FrostySdk;
using FrostySdk.Interfaces;
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
using v2k4FIFAModdingCL;

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for LaunchWindow.xaml
    /// </summary>
    public partial class LaunchWindow : Window, ILogger
    {
        ModListProfile Profile = new ModListProfile(null);
        public LaunchWindow()
        {
            InitializeComponent();
            GetListOfModsAndOrderThem();
            try
            {
                if (!string.IsNullOrEmpty(AppSettings.Settings.FIFAInstallEXEPath))
                {
                    txtFIFADirectory.Text = AppSettings.Settings.FIFAInstallEXEPath;
                    InitializeOfSelectedGame(AppSettings.Settings.FIFAInstallEXEPath);
                }
            }
            catch (Exception e)
            {
                txtFIFADirectory.Text = "";
                Trace.WriteLine(e.ToString());
            }
        }

        private ObservableCollection<string> ListOfMods = new ObservableCollection<string>();

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

                var mL = new Mods.ModList();
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


                var mL = new Mods.ModList();
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
                cbProfiles.Items.Add(profButton);

            }
            var addnewprofilebutton = new Button() { Content = "Add new profile" };
            addnewprofilebutton.Click += Addnewprofilebutton_Click;
            cbProfiles.Items.Add(addnewprofilebutton);

            // load list of mods
            ListOfMods = new ObservableCollection<string>(new ModList(Profile).ModListItems);
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

        private async void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (GameInstanceSingleton.GAMEVERSION != null)
            {
                // Copy the Locale.ini if checked
                if (chkInstallLocale.IsChecked.Value)
                {
                    foreach (var z in ListOfMods.Where(x => x.Contains(".zip")))
                    {
                        using (FileStream fs = new FileStream(z, FileMode.Open))
                        {
                            ZipArchive zipA = new ZipArchive(fs);
                            foreach (var ent in zipA.Entries)
                            {
                                if (ent.Name.Contains("locale.ini"))
                                {
                                    ent.ExtractToFile(GameInstanceSingleton.FIFALocaleINIPath);
                                }
                            }
                        }
                    }
                }

                var k = chkUseFileSystem.IsChecked.Value;
                var useLegacyMods = chkUseLegacyModSupport.IsChecked.Value;
                var useLiveEditor = chkUseLiveEditor.IsChecked.Value;
                // Start the game with mods
                await new TaskFactory().StartNew(async () =>
                {

                Dispatcher.Invoke(() =>
                {
                    btnLaunch.IsEnabled = false;
                });
                await Task.Delay(1000);
                //Dispatcher.Invoke(() =>
                //{
                await LaunchFIFA.LaunchAsync(GameInstanceSingleton.GAMERootPath, "", new Mods.ModList().ModListItems, this, GameInstanceSingleton.GAMEVERSION, true);

                    if (useLegacyMods)
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

                        if (!string.IsNullOrEmpty(legacyModSupportFile)) {

                            if (File.Exists(legacyModSupportFile))
                                File.Copy(legacyModSupportFile, @GameInstanceSingleton.GAMERootPath + "v2k4LegacyModSupport.dll", true);

                            var legmodsupportdllpath = @GameInstanceSingleton.GAMERootPath + @"v2k4LegacyModSupport.dll";
                            var actualsupportdllpath = @"E:\Origin Games\FIFA 20\v2k4LegacyModSupport.dll";
                            Debug.WriteLine(legmodsupportdllpath);
                            Debug.WriteLine(actualsupportdllpath);
                            GameInstanceSingleton.InjectDLLAsync(legmodsupportdllpath);
                        }
                    }

                    if (useLiveEditor)
                    {
                        if(File.Exists(@GameInstanceSingleton.GAMERootPath + @"FIFALiveEditor.DLL"))
                            GameInstanceSingleton.InjectDLLAsync(@GameInstanceSingleton.GAMERootPath + @"FIFALiveEditor.DLL");
                    }
                       
                    //});
                    await Task.Delay(1000);
                    Dispatcher.Invoke(() =>
                    {
                        btnLaunch.IsEnabled = true;
                    });

                });
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = this.listMods.SelectedIndex;
            if (selectedIndex > -1)
            {
                var mL = new Mods.ModList();
                mL.ModListItems.Remove(this.ListOfMods[selectedIndex]);
                mL.Save();
                this.ListOfMods.RemoveAt(selectedIndex);

                GetListOfModsAndOrderThem();
            }

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dialog.Title = "Browse for your mod";
            dialog.Multiselect = false;
            dialog.Filter = "zip files (*.zip)|*.zip|fbmod files (*.fbmod)|*.fbmod";
            dialog.FilterIndex = 0;
            dialog.ShowDialog(this);
            var filePath = dialog.FileName;
            //var filePathSplit = filePath.Split('\\');
            //var filename = filePathSplit[filePathSplit.Length-1];
            //File.Copy(filePath, "Mods\\" + filename);
            if (!string.IsNullOrEmpty(filePath))
            {
                var mL = new Mods.ModList();
                mL.ModListItems.Add(filePath);
                mL.Save();
                GetListOfModsAndOrderThem();
            }

        }

        private void btnCloseModdingTool_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnBrowseFIFADirectory_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dialog.Title = "Find your FIFA exe";
            dialog.Multiselect = false;
            dialog.Filter = "exe files (*.exe)|*.exe";
            dialog.FilterIndex = 0;
            dialog.ShowDialog(this);
            var filePath = dialog.FileName;
            InitializeOfSelectedGame(filePath);

        }

        private void InitializeOfSelectedGame(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                AppSettings.Settings.FIFAInstallEXEPath = filePath;
                AppSettings.Settings.Save();

                if(GameInstanceSingleton.InitialiseSingleton(filePath))
                {
                    if (!ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
                    {
                        throw new Exception("Unable to Initialize Profile");
                    }
                    txtFIFADirectory.Text = GameInstanceSingleton.GAMERootPath;
                    btnLaunch.IsEnabled = true;
                }

                if(GameInstanceSingleton.IsCompatibleWithFbMod())
                {
                    listMods.IsEnabled = true;
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

                //FIFADirectory = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                //GameInstanceSingleton.GAMERootPath = FIFADirectory;
                //var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                //if (!string.IsNullOrEmpty(fileName) && GameInstanceSingleton.CompatibleGameVersions.Contains(fileName))
                //{
                //    GameInstanceSingleton.GAMEVERSION = fileName.Replace(".exe", "");

                //}
            }
        }

        public void Log(string text, params object[] vars)
        {
            Dispatcher.Invoke(() =>
            {
                lblProgressText.Text = text;
            });
        }

        public void LogWarning(string text, params object[] vars)
        {
            Dispatcher.Invoke(() =>
            {
                lblProgressText.Text = text;
            });
        }

        public void LogError(string text, params object[] vars)
        {
            Dispatcher.Invoke(() =>
            {
                lblProgressText.Text = text;
            });
        }

        private void listMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.listMods != null && this.listMods.SelectedIndex != -1 && this.listMods.SelectedItem != null) {
                var selectedIndex = this.listMods.SelectedIndex;
                var selectedMod = this.listMods.SelectedItem as string;
                if (selectedMod.Contains(".fbmod")) {
                        var fm = new FrostyMod(selectedMod);
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
