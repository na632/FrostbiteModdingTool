using FrostySdk;
using FrostySdk.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private void btnDuplicate_Click(object sender, RoutedEventArgs e)
        {
            //EbxAssetEntry ae = JsonConvert.DeserializeObject<EbxAssetEntry>(JsonConvert.SerializeObject(EntryToDuplicate));
            if (IsLegacy)
            {
                LegacyFileEntry ae = JsonConvert.DeserializeObject<LegacyFileEntry>(JsonConvert.SerializeObject(EntryToDuplicate));
                ae.Name = NewEntryPath;
                ICustomAssetManager customAssetManager = AssetManager.Instance.GetLegacyAssetManager();
                customAssetManager.AddAsset(ae.Name, ae);
            }
            else
            {

                EbxAssetEntry ae = EntryToDuplicate.Clone() as EbxAssetEntry;
                var originalEbxData = AssetManager.Instance.GetEbx(ae);

                ae.Name = NewEntryPath;
                AssetManager.Instance.AddEbx(ae);

                // Check for "Resource" property
                //if(originalEbxData.RootObject)

                
            }

            DialogResult = true;
            this.Close();
        }
    }
}
