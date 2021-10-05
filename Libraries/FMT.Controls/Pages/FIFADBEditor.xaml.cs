using FifaLibrary;
using FrostySdk;
using FrostySdk.Managers;
using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FMT.Controls.Pages
{
    /// <summary>
    /// Interaction logic for FIFADBEditor.xaml
    /// </summary>
    public partial class FIFADBEditor : UserControl
    {
        public LegacyFileEntry DBAssetEntry { get; set; }
        public MemoryStream DBAssetStream { get; set; }
        public MemoryStream DBMetaAssetStream { get; set; }

        public IList<AssetEntry> LOCAssetEntries { get; set; }
        public DbFile DB { get; set; }

        public FIFADBEditor()
        {
            InitializeComponent();

            AttachToAssetManager();
            DataContext = null;
            DataContext = this;
        }

        ~FIFADBEditor()
        {
            DetachFromAssetManager();

            if (DB != null)
                DB.Dispose();

            DBAssetEntry = null;
            DBAssetStream = null;
            DBMetaAssetStream = null;
            DB = null;
        }

        public void AttachToAssetManager()
        {
            AssetManager.AssetManagerInitialised += AssetManagerInitialised;
            //AssetManager.AssetManagerModified += AssetManagerInitialised;
        }

        public void DetachFromAssetManager()
        {
            AssetManager.AssetManagerInitialised -= AssetManagerInitialised;
            //AssetManager.AssetManagerModified -= AssetManagerInitialised;
        }

        private async void AssetManagerInitialised()
        {
            try
            {
                await Load();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task Load()
        {
            await LoadDbFromLegacy();
        }

        private async Task LoadDbFromLegacy()
        {
            DBAssetEntry = await AssetManager.Instance.GetCustomAssetEntryAsync("legacy", "data/db/fifa_ng_db.db") as LegacyFileEntry;
            DBAssetStream = await AssetManager.Instance.GetCustomAssetAsync("legacy", DBAssetEntry);
            if (DBAssetStream == null)
                return;

            var dbMeta = await AssetManager.Instance.GetCustomAssetEntryAsync("legacy", "data/db/fifa_ng_db-meta.xml") as LegacyFileEntry;
            DBMetaAssetStream = await AssetManager.Instance.GetCustomAssetAsync("legacy", dbMeta);

            if (DBMetaAssetStream == null)
                return;

            DB = new DbFile(DBAssetStream, DBMetaAssetStream);


            await Dispatcher.InvokeAsync(() => {

                tblList.ItemsSource = null;
                tblList.ItemsSource = DB.GetTables().ToList().OrderBy(x => x.ToString());

            });

           
        }

        private void tblList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(tblList.SelectedItem != null)
            {
                FifaTable table = tblList.SelectedItem as FifaTable;
                tblData.ItemsSource = null;
                tblData.ItemsSource = table.ConvertToDataTable().DefaultView;
            }
        }

        private async void btn_RefreshClick(object sender, RoutedEventArgs e)
        {
            await Load();
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (tblList.SelectedItem != null)
            {
                FifaTable table = tblList.SelectedItem as FifaTable;
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv";
                if (saveFileDialog.ShowDialog() == true)
                {
                    table.ConvertToDataTable().ToCSV(saveFileDialog.FileName);
                }
            }
        }
    }
}
