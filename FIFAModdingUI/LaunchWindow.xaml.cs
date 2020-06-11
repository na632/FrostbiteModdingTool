using FIFAModdingUI.Mods;
using FrostySdk.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
using v2k4FIFAModdingCL;

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for LaunchWindow.xaml
    /// </summary>
    public partial class LaunchWindow : Window, ILogger
    {
        public LaunchWindow()
        {
            InitializeComponent();
            GetListOfModsAndOrderThem();
            try
            {
                if (!string.IsNullOrEmpty(AppSettings.Settings.FIFAInstallEXEPath))
                {
                    txtFIFADirectory.Text = AppSettings.Settings.FIFAInstallEXEPath;
                    InitializeOfSelectedFIFA(AppSettings.Settings.FIFAInstallEXEPath);
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
        public string FIFADirectory { get; private set; }

        List<string> CompatibleFIFAVersions = new List<string>()
        {
            "FIFA19.exe",
            "FIFA20_demo.exe",
            "FIFA20.exe",
            "FIFA21_demo.exe"
        };

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
            //ListOfMods = new ObservableCollection<string>(Directory.EnumerateFiles(
            //    Directory.GetParent(Assembly.GetExecutingAssembly().Location)
            //    + "\\Mods\\").Where(x => x.ToLower().Contains(".fbmod")).Select(
            //    f => new FileInfo(f).Name).ToList());
            ListOfMods = new ObservableCollection<string>(new ModList().ModListItems);
            listMods.ItemsSource = ListOfMods;
        }

        Task<int> LaunchingTask = null;

        private async void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            await new TaskFactory().StartNew(async() =>
            {
                Dispatcher.Invoke(() =>
                {
                    btnLaunch.IsEnabled = false;
                });
                LaunchingTask = LaunchFIFA.LaunchAsync(FIFAInstanceSingleton.FIFARootPath, "", new Mods.ModList().ModListItems, this);
                await LaunchingTask;
                await Task.Delay(10 * 1000);
                Dispatcher.Invoke(() =>
                {
                    btnLaunch.IsEnabled = true;
                    LaunchingTask = null;
                });
            });
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
            dialog.Filter = "fbmod files (*.fbmod)|*.fbmod";
            dialog.FilterIndex = 0;
            dialog.ShowDialog(this);
            var filePath = dialog.FileName;
            //var filePathSplit = filePath.Split('\\');
            //var filename = filePathSplit[filePathSplit.Length-1];
            //File.Copy(filePath, "Mods\\" + filename);
            var mL = new Mods.ModList();
            mL.ModListItems.Add(filePath);
            mL.Save();
            GetListOfModsAndOrderThem();

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
            InitializeOfSelectedFIFA(filePath);
        }

        private void InitializeOfSelectedFIFA(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                AppSettings.Settings.FIFAInstallEXEPath = filePath;
                AppSettings.Settings.Save();

                FIFADirectory = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                FIFAInstanceSingleton.FIFARootPath = FIFADirectory;
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                if (!string.IsNullOrEmpty(fileName) && CompatibleFIFAVersions.Contains(fileName))
                {
                    FIFAInstanceSingleton.FIFAVERSION = fileName.Replace(".exe", "");

                    txtFIFADirectory.Text = FIFADirectory;
                    btnLaunch.IsEnabled = true;
                }
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
           
        }

        public void LogError(string text, params object[] vars)
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
