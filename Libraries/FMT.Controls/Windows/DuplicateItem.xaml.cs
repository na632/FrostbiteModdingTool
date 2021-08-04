using FrostySdk;
using FrostySdk.Ebx;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using v2k4FIFAModding;

namespace FMT.Controls.Windows
{
    /// <summary>
    /// Interaction logic for DuplicateItem.xaml
    /// </summary>
    public partial class DuplicateItem : Window
    {
        public AssetEntry EntryToDuplicate { get; set; }

        public bool IsLegacy { get; set; }

        public string NewEntryPath { get; set; }


        public DuplicateItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        public static Random ResRidRandomizer = new Random();

        private void btnDuplicate_Click(object sender, RoutedEventArgs e)
        {
            AssetManager.Instance.DuplicateEntry(EntryToDuplicate, NewEntryPath, IsLegacy);

            DialogResult = true;
            this.Close();
        }

       
    }
}
