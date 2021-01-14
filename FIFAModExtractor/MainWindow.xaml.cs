using FrostySdk;
using FrostySdk.Frosty;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FIFAModExtractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string GamePath { get; set; }

        public bool GamePathFound { get { return !string.IsNullOrEmpty(GamePath) && new FileInfo(GamePath).Exists; } }
        public string FIFAModFilePath { get; set; }

        public bool FIFAModFilePathFound { get { return !string.IsNullOrEmpty(FIFAModFilePath) && new FileInfo(FIFAModFilePath).Exists; } }

        public bool FIFAModFilePathReadable 
        { 
            get 
            {
                return FIFAModFilePathFound &&
                    CanReadFIFAModFile(FIFAModFilePath);
            } 
        }

        public bool CanReadFIFAModFile(string filePath)
        {
            if(!string.IsNullOrEmpty(filePath)) FIFAModFilePath = filePath;

            if (string.IsNullOrEmpty(FIFAModFilePath) || !new FileInfo(FIFAModFilePath).Exists)
                return false;

            if (FIFAModFilePath.Contains("paulv2k4", StringComparison.OrdinalIgnoreCase))
                return false;


            return true;
        }

        public FileInfo FIFAModFileInfo
        {
            get
            {
                if (!FIFAModFilePathFound)
                    return null;

                return new FileInfo(FIFAModFilePath);
            }
        }

        public ObservableCollection<BaseModResource> ItemsInFile
        {
            get
            {
                return GetItemsInFile(FIFAModFilePath);
            }
        }

        public ObservableCollection<BaseModResource> GetItemsInFile(string filePath)
        {
            List<BaseModResource> items = new List<BaseModResource>();
            if (!CanReadFIFAModFile(filePath) || !FIFAModFileInfo.Exists)
                return new ObservableCollection<BaseModResource>(items);

            switch (FIFAModFileInfo.Extension)
            {
                case ".fifamod":
                    ModFile = new FIFAMod(new FileStream(FIFAModFilePath, FileMode.Open));
                    break;
                default:
                    ModFile = new FrostbiteMod(new FileStream(FIFAModFilePath, FileMode.Open));
                    break;
            }

            items = ModFile.Resources.ToList();
            return new ObservableCollection<BaseModResource>(items);
        }

        public IFrostbiteMod ModFile { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void btnBrowseGame_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable files|*.exe";
            var result = openFileDialog.ShowDialog();
            if(result.HasValue && result.Value && !string.IsNullOrEmpty(openFileDialog.FileName))
            {
                GamePath = openFileDialog.FileName;
                FIFAModFilePath = null;

                var profName = new FileInfo(GamePath).Name.Replace(".exe", "", StringComparison.OrdinalIgnoreCase);
                ProfilesLibrary.Initialize(profName);


                this.DataContext = null;
                this.DataContext = this;


            }
        }

        private void btnBrowseFIFAMod_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Mod files|*.fbmod;*.fifamod";
            var result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value && !string.IsNullOrEmpty(openFileDialog.FileName))
            {
                FIFAModFilePath = openFileDialog.FileName;
                this.DataContext = null;
                this.DataContext = this;
            }
        }

        private void btnAttemptFullExtraction_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExtractAsset_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
