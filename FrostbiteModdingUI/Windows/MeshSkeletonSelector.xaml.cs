using FIFAModdingUI.Pages.Common;
using FrostySdk.FrostySdk.Managers;
using FrostySdk.Managers;
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

namespace FrostbiteModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for MeshSkeletonSelector.xaml
    /// </summary>
    public partial class MeshSkeletonSelector : Window
    {
        public EbxAssetEntry AssetEntry { get; set; }

        public Browser ParentBrowser { get; set; }

        public MeshSkeletonSelector()
        {
            InitializeComponent();

            assetListView.ItemsSource = null;
            assetListView.ItemsSource = AssetManager.Instance
                                   .EnumerateEbx()
                                   .Where(x => x.Type.Contains("SkeletonAsset", StringComparison.OrdinalIgnoreCase))
                                   .OrderBy(x => x.Path).Select(x => (IAssetEntry)x).ToList();

        }

        

        private void assetListView_Selected(object sender, RoutedEventArgs e)
        {
            ListView listView = sender as ListView;

        }

        private void assetListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            AssetEntry = listView.SelectedItem as EbxAssetEntry;
            DialogResult = true;
            this.Close();
        }
    }
}
