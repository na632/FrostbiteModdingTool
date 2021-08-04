using FrostbiteModdingUI.Models;
using FrostbiteSdk;
using FrostySdk;
using FrostySdk.Frosty;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FrostbiteModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for FrostbiteModBundler.xaml
    /// </summary>
    public partial class FrostbiteModBundler : Window
    {
        public ModBundle Bundle { get; set; }

        public FrostbiteModBundler()
        {
            InitializeComponent();
            Bundle = new ModBundle();
            Bundle.Mods = new List<IFrostbiteMod>();
            DataContext = Bundle;
        }

        private void btnLoadBundle_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSaveBundle_Click(object sender, RoutedEventArgs e)
        {
            Bundle.Save("test.bundle");
        }

        private void btnAddToBundle_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value) 
            {
                var fi = new FileInfo(openFileDialog.FileName);
                if (fi.Extension.Contains("fifamod", StringComparison.OrdinalIgnoreCase))
                {
                    Bundle.Mods.Add(new FIFAMod(new FileStream(fi.FullName, FileMode.Open)));
                }
                if (fi.Extension.Contains("fbmod", StringComparison.OrdinalIgnoreCase))
                {
                    Bundle.Mods.Add(new FrostbiteMod(new FileStream(fi.FullName, FileMode.Open)));
                }
            }

            DataContext = null;
            DataContext = Bundle;
        }

        private void btnRemoveFromBundle_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
