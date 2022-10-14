using FIFAModdingUI;
using FMT;
using FrostbiteModdingUI.Models;
using FrostySdk;
using FrostySdk.Interfaces;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using v2k4FIFAModdingCL;

namespace FrostbiteModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for FindGameEXEWindow.xaml
    /// </summary>
    public partial class FindGameEXEWindow : Window, ILogger
    {
        private string modProfileDirectory { get; } = App.ApplicationDirectory + "\\Mods\\Profiles\\";

        public FindGameEXEWindow()
        {
            InitializeComponent();

            // Create the Profiles Directory if it doesn't already exist
            Directory.CreateDirectory(App.ApplicationDirectory + "\\Mods\\Profiles\\");

            List<string> lastLocationPaths = new List<string>();
            foreach(var dir in Directory.GetDirectories(modProfileDirectory))
            {
                lastLocationPaths.AddRange(
                Directory.GetFiles(dir)
                .Where(x => x.Contains("LastLocation.json", StringComparison.OrdinalIgnoreCase)).ToList());
            }

            var lstOfLocations = lastLocationPaths.Select(x 
                => 
                new FileInfo(File.ReadAllText(x))
                ).Where(x=>x.Exists).ToList();
            lv.ItemsSource = lstOfLocations;
        }

        private void btnFindGameEXE_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Find your Game exe";
            dialog.Multiselect = false;
            dialog.Filter = "exe files (*.exe)|*.exe";
            dialog.FilterIndex = 0;
            var dialogOK = dialog.ShowDialog(this);
            if (dialogOK.HasValue && dialogOK.Value)
            {
                var filePath = dialog.FileName;
                InitializeOfSelectedGame(filePath);
                this.Close();
            }

        }

        private void InitializeOfSelectedGame(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                AppSettings.Settings.GameInstallEXEPath = filePath;

                if (GameInstanceSingleton.InitializeSingleton(filePath, false, this))
                //if (GameInstanceSingleton.InitializeSingleton(filePath, true, this))
                {
                    //if (!ProfilesLibrary.Initialize(GameInstanceSingleton.Instance.GAMEVERSION))
                    //{
                    //    throw new Exception("Unable to Initialize Profile");
                    //}
                    DialogResult = true;
                    Directory.CreateDirectory(System.IO.Path.Combine(modProfileDirectory, ProfilesLibrary.ProfileName));
                    File.WriteAllText(System.IO.Path.Combine(modProfileDirectory, ProfilesLibrary.ProfileName, "LastLocation.json"), filePath);
                }
                else
                {
                    throw new FileNotFoundException($"Unable to initialise against {filePath}");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var fi = btn.Tag as FileInfo;

            InitializeOfSelectedGame(fi.FullName);
            this.Close();
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
    }
}
