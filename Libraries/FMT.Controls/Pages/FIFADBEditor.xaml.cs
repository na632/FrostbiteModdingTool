using FifaLibrary;
using FrostySdk;
using FrostySdk.IO;
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
        private string pathToDBFile { get; set; }
        public LegacyFileEntry DBAssetEntry { get; set; }
        public MemoryStream DBAssetStream { get; set; }
        public MemoryStream DBMetaAssetStream { get; set; }

        public IList<AssetEntry> LOCAssetEntries { get; set; }
        public DbFile DB { get; set; }

        public enum DBEditorMode
        {
            DB,
            Career,
            Squad
        };

        public DBEditorMode EditorMode { get; set; } = DBEditorMode.DB;

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

            //if (DB != null)
            //    DB.Dispose();

            DBAssetEntry = null;
            DBAssetStream = null;
            DBMetaAssetStream = null;
            DB = null;
        }

        public void AttachToAssetManager()
        {
            if (EditorMode != DBEditorMode.DB)
                return;

            AssetManager.AssetManagerInitialised += AssetManagerInitialised;
            //AssetManager.AssetManagerModified += AssetManagerInitialised;
        }

        public void DetachFromAssetManager()
        {
            if (EditorMode != DBEditorMode.DB)
                return;

            AssetManager.AssetManagerInitialised -= AssetManagerInitialised;
            //AssetManager.AssetManagerModified -= AssetManagerInitialised;
        }

        private async void AssetManagerInitialised()
        {
            if (EditorMode != DBEditorMode.DB)
                return;

            try
            {
                await Load();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public async Task Load(string pathToFile = "")
        {
            if(!string.IsNullOrEmpty(pathToFile))
            {
                if(pathToFile.Contains("Career", StringComparison.OrdinalIgnoreCase))
                {
                    //EditorMode = DBEditorMode.Career;
                    // Currently not supported
                    return;
                }
                else if (pathToFile.Contains("Squad", StringComparison.OrdinalIgnoreCase))
                {
                    EditorMode = DBEditorMode.Squad;
                }
            }

            switch(EditorMode)
            {
                case DBEditorMode.DB:
                    await LoadDbFromLegacy();
                    break;
                default:
                    await LoadFromFile(pathToFile);
                    break;
            }
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
        private async Task LoadFromFile(string pathToFile)
        {
            if(!string.IsNullOrEmpty(pathToFile))
                this.pathToDBFile = pathToFile;

            DBAssetStream = new MemoryStream(File.ReadAllBytes(this.pathToDBFile));
            if (DBAssetStream == null)
                return;

            var dbMeta = await AssetManager.Instance.GetCustomAssetEntryAsync("legacy", "data/db/fifa_ng_db-meta.xml") as LegacyFileEntry;
            DBMetaAssetStream = await AssetManager.Instance.GetCustomAssetAsync("legacy", dbMeta);

            if (DBMetaAssetStream == null)
                return;

            var metaFilePath = System.IO.Path.Combine(AppContext.BaseDirectory, ProfilesLibrary.ProfileName + "-db-meta.xml");
            if (!File.Exists(metaFilePath))
            {
                NativeWriter nativeWriter =new NativeWriter(new FileStream(metaFilePath, FileMode.Create));
                nativeWriter.Write(DBMetaAssetStream.ToArray());
                nativeWriter.Close();
            }

            DB = new CareerFile(pathToFile, metaFilePath);

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
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CSV Files|*.csv";
                if (openFileDialog.ShowDialog().Value)
                {
                    FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                    if (fileInfo.Exists)
                    {
                        System.Data.DataTable dt = new System.Data.DataTable();
                        if (tblList.SelectedItem is FifaTable table)
                        {
                            var newDT = new System.Data.DataTable().FromCSV(fileInfo.FullName);
                            table.ConvertFromDataTable(newDT);
                            tblData.ItemsSource = null;
                            tblData.ItemsSource = table.ConvertToDataTable().DefaultView;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
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
