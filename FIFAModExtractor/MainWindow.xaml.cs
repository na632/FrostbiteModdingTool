using Frostbite.Textures;
using FrostySdk;
using FrostySdk.Frostbite.IO.Output;
using FrostySdk.Frosty;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static FrostbiteSdk.Frosty.Abstract.BaseModReader;

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


            if (ModFile == null)
            {
                switch (FIFAModFileInfo.Extension)
                {
                    case ".fifamod":
                        ModFile = new FIFAMod(new FileStream(FIFAModFilePath, FileMode.Open));
                        break;
                    default:
                        ModFile = new FrostbiteMod(new FileStream(FIFAModFilePath, FileMode.Open));
                        break;
                }
            }


            if (ModFile != null)
            {
                if(ModFile is FrostbiteMod)
                {
                    if(((FrostbiteMod)ModFile).IsEncrypted)
                        return new ObservableCollection<BaseModResource>(items);
                }

                items = ModFile.Resources.Where(x => 
                (x.Type == ModResourceType.Ebx
                || x.Type == ModResourceType.Chunk
                 || !x.Name.Contains("mesh"))
                ).ToList();
            }
            return new ObservableCollection<BaseModResource>(items);
        }

        public List<BaseModResource> GetAllItemsInFile()
        {
            if (ModFile != null)
            {
                return ModFile.Resources.ToList();
            }

            return null;
        }

        public IFrostbiteMod ModFile { get; set; }

        public IFrostbiteMod GetModFile()
        {
            if (ModFile == null)
            {
                MemoryStream memoryStream = new MemoryStream();
                using (var nr = new NativeReader(new FileStream(FIFAModFilePath, FileMode.Open)))
                    memoryStream = new MemoryStream(nr.ReadToEnd());

                switch (FIFAModFileInfo.Extension)
                {
                    case ".fifamod":
                        ModFile = new FIFAMod(memoryStream);
                        break;
                    default:
                        ModFile = new FrostbiteMod(memoryStream);
                        break;
                }
                //if (AssetManager.Instance == null)
                //    AssetManager.Instance = new AssetManager(FileSystem.Instance, ResourceManager.Instance);

                //foreach(var i in ItemsInFile)
                //{

                //    switch(i.Type)0
                //    {
                //        case ModResourceType.Ebx:

                //            EbxAssetEntry ebxAssetEntry = new EbxAssetEntry();
                //            i.FillAssetEntry(ebxAssetEntry);
                //            AssetManager.Instance.AddEbx(ebxAssetEntry);

                //            break;
                //        case ModResourceType.Res:

                //            ResAssetEntry resAssetEntry = new ResAssetEntry();
                //            i.FillAssetEntry(resAssetEntry);
                //            AssetManager.Instance.AddRes(resAssetEntry);

                //            break;
                //        case ModResourceType.Chunk:

                //            ChunkAssetEntry chunkAssetEntry = new ChunkAssetEntry();
                //            i.FillAssetEntry(chunkAssetEntry);
                //            AssetManager.Instance.AddChunk(chunkAssetEntry);

                //            break;
                //    }
                //}

            }
            return ModFile;
        }

        public List<string> ListOfTextureNames = new List<string>() { "color", "coeff", "normal", "taa" };
        public List<string> ListOfMeshNames = new List<string>() { "mesh", "" };


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

                FileSystem.Instance = new FileSystem(Directory.GetParent(GamePath).FullName);
                FileSystem.Instance.Initialize();

                ZStd.Bind();
                Oodle.Bind(Directory.GetParent(GamePath).FullName);
                TypeLibrary.Initialize(true);


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
                GetModFile();
            }
        }

        private void btnAttemptFullExtraction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstModItems.SelectedValue != null)
                {
                    // getting lots of "already in use" errors here from "getmodfile()"

                    var compResoruceData = GetModFile().GetResourceData((BaseModResource)lstModItems.SelectedValue);
                    if (compResoruceData.Length > 0)
                    {
                        var casReader = new CasReader(new MemoryStream(compResoruceData));
                        var uncompressedData = casReader.Read();

                        if (lstModItems.SelectedValue is EbxResource)
                        {
                            var ebxModResource = (EbxResource)lstModItems.SelectedItem;

                            var filename = ebxModResource.Name;
                            filename = filename.Split('/')[filename.Split('/').Length - 1];

                            EbxAsset ebx = new EbxReaderV3(new MemoryStream(uncompressedData), true).ReadAsset();

                            var lastNameItem = ebxModResource.Name.Split("_")[ebxModResource.Name.Split("_").Length - 1];
                            if (ListOfMeshNames.Contains(lastNameItem))
                            {
                                var resModResource = (ResResource)GetAllItemsInFile().FirstOrDefault(
                                    x => x.Name.Equals(ebxModResource.Name, StringComparison.OrdinalIgnoreCase)
                                    && x.Type == ModResourceType.Res);
                                var resResoruceData = GetModFile().GetResourceData(resModResource);
                                if (resResoruceData.Length > 0)
                                {
                                    casReader = new CasReader(new MemoryStream(resResoruceData));
                                    uncompressedData = casReader.Read();
                                    MeshSet meshSet = new MeshSet(new MemoryStream(uncompressedData));
                                    var exporter = new MeshSetToFbxExport();

                                    SaveFileDialog sfdExportMesh = new SaveFileDialog();
                                    sfdExportMesh.Filter = "Model (*.fbx)|*.fbx;";
                                    sfdExportMesh.FileName = filename;
                                    var sfd_result = sfdExportMesh.ShowDialog();
                                    if (sfd_result.HasValue && sfd_result.Value)
                                    {
                                        exporter.Export(null, ebx, sfdExportMesh.FileName, "FBX_2012", "Meters", true, null, "*.fbx", meshSet);
                                    }
                                }
                            }
                           else if (ListOfTextureNames.Contains(lastNameItem))
                            {
                                var resModResource = (ResResource)ItemsInFile.FirstOrDefault(x => x.Name == ebxModResource.Name && x.Type == ModResourceType.Res);
                                var resResoruceData = GetModFile().GetResourceData(resModResource);
                                if (resResoruceData.Length > 0)
                                {
                                    casReader = new CasReader(new MemoryStream(resResoruceData));
                                    uncompressedData = casReader.Read();
                                    Texture texture = new Texture(new MemoryStream(uncompressedData), null);
                                    if (texture != null && texture.Type == TextureType.TT_2d)
                                    {
                                        var mdR = ModFile.Resources.FirstOrDefault(x => x.Name == texture.ChunkId.ToString());
                                        if (mdR != null)
                                        {
                                            texture.SetData(DecompressData(ModFile.GetResourceData(mdR)));
                                            TextureExporter textureExporter = new TextureExporter();
                                            var exportableTextureData = textureExporter.WriteToDDS(texture);
                                            ExportDataToFile(exportableTextureData, filename, "*.dds");
                                        }
                                    }
                                }
                            }
                            else
                                ExportDataToFile(uncompressedData);
                        }
                        else if (lstModItems.SelectedValue is ResResource)
                        {
                            var resModResource = (ResResource)lstModItems.SelectedItem;

                            var lastNameItem = resModResource.Name.Split("_")[resModResource.Name.Split("_").Length - 1];

                            var filename = resModResource.Name;
                            filename = filename.Split('/')[filename.Split('/').Length - 1];


                            if (ListOfTextureNames.Contains(lastNameItem))
                            {
                                Texture texture = new Texture(new MemoryStream(uncompressedData), null);
                                if (texture != null && texture.Type == TextureType.TT_2d)
                                {
                                    var mdR = ModFile.Resources.FirstOrDefault(x => x.Name == texture.ChunkId.ToString());
                                    if (mdR != null)
                                    {
                                        texture.SetData(DecompressData(ModFile.GetResourceData(mdR)));
                                        TextureExporter textureExporter = new TextureExporter();
                                        var exportableTextureData = textureExporter.WriteToDDS(texture);
                                        ExportDataToFile(exportableTextureData, filename, "*.dds");
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception("Unknown file attempted to be extracted, try extracting decompresed chunk instead");
                            }

                        }
                        else
                        {
                            throw new Exception("You cannot fully extract a chunk, try a Res instead");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public byte[] DecompressData(byte[] data)
        {
            return DecompressData(new MemoryStream(data));
        }

        public byte[] DecompressData(MemoryStream datastream)
        {
            var casReader = new CasReader(datastream);
            var uncompressedData = casReader.Read();
            return uncompressedData;
        }

        private void btnExtractAsset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstModItems.SelectedValue != null) 
                {
                    var compResoruceData = GetModFile().GetResourceData((BaseModResource)lstModItems.SelectedValue);
                    if(compResoruceData.Length > 0)
                    {
                        var casReader = new CasReader(new MemoryStream(compResoruceData));
                        var uncompressedData = casReader.Read();
                        ExportDataToFile(uncompressedData);
                    }
                    else
                    {
                        throw new Exception("Had no data to process");
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ExportDataToFile(byte[] uncompressedData, string inName = "UnknownFile", string inExtension = "*.dat")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = inName;
            saveFileDialog.Filter = $"Files ({inExtension})|{inExtension}";
            var dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                using (var nw = new NativeWriter(new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate)))
                {
                    nw.WriteBytes(uncompressedData);
                }
            }
        }
    }
}
