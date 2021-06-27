using FrostySdk;
using FrostySdk.Managers;
using FrostySdk.Resources;
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
                if(v2k4Util.PropertyExists(originalEbxData.RootObject, "Resource"))
                {
                    ResAssetEntry resAssetEntry = AssetManager.Instance.GetResEntry(((dynamic)originalEbxData.RootObject).Resource);
                    var rae = resAssetEntry.Clone() as ResAssetEntry;
                    rae.Name = NewEntryPath;

                    if (ae.Type == "TextureAsset")
                    {
                        using (Texture textureAsset = new Texture(rae))
                        {
                            var cae = textureAsset.ChunkEntry.Clone() as ChunkAssetEntry;
                            cae.Id = AssetManager.Instance.GenerateChunkId(cae);
                            textureAsset.ChunkId = cae.Id;
                            var newTextureData = textureAsset.ToBytes();
                            rae.ModifiedEntry = new ModifiedAssetEntry() { Data = Utils.CompressFile(newTextureData, textureAsset) };
                            cae.ModifiedEntry = new ModifiedAssetEntry() { Data = Utils.CompressFile(AssetManager.Instance.GetChunkData(cae)) };
                            AssetManager.Instance.AddChunk(cae);
                        }
                    }

                    AssetManager.Instance.AddRes(rae);
                }


            }

            DialogResult = true;
            this.Close();
        }
    }
}
