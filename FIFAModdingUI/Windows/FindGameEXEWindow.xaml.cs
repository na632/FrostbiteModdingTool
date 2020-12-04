using FIFAModdingUI;
using FrostySdk;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
                AppSettings.Settings.FIFAInstallEXEPath = filePath;
                AppSettings.Settings.Save();

                if (GameInstanceSingleton.InitialiseSingleton(filePath))
                {
                    if (!ProfilesLibrary.Initialize(GameInstanceSingleton.GAMEVERSION))
                    {
                        throw new Exception("Unable to Initialize Profile");
                    }
                }
            }
        }
    }
}
