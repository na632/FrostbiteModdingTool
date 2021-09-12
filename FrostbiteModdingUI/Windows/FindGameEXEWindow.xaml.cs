using FIFAModdingUI;
using FMT;
using FrostbiteModdingUI.Models;
using FrostySdk;
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
    public partial class FindGameEXEWindow : Window
    {
        public FindGameEXEWindow()
        {
            InitializeComponent();

            var lastLocations = Directory.GetFiles(App.ApplicationDirectory).Where(x => x.Contains("LastLocation.json", StringComparison.OrdinalIgnoreCase)).ToList();

            var lstOfLocations = lastLocations.Select(x 
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

                if (GameInstanceSingleton.InitializeSingleton(filePath))
                {
                    if (!ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
                    {
                        throw new Exception("Unable to Initialize Profile");
                    }
                    DialogResult = true;
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
    }
}
