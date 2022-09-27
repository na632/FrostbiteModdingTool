using AvalonDock.Layout;
using CSharpImageLibrary;
using FIFAModdingUI.Pages.Common;
using Frostbite.FileManagers;
using Frostbite.Textures;
using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Frostbite.IO;
using FrostySdk.IO;
using FrostySdk.Managers;
using Microsoft.Win32;
using SharpDX.Toolkit.Graphics;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
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
using static Frostbite.Textures.TextureUtils;
using static FrostySdk.Resources.GeometryDeclarationDesc;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace FMT.Pages.Common
{
    /// <summary>
    /// Interaction logic for BrowserOfInitfs.xaml
    /// </summary>
    public partial class BrowserOfInitfs : UserControl
    {
        public BrowserOfInitfs()
        {
            InitializeComponent();
        }

		public class FileEntry
		{
			private readonly InitFSMod initFsMod;

			public string FileName { get; }

			public bool IsModified => this.initFsMod.Data.ContainsKey(this.FileName);

			public FileEntry(InitFSMod initFsMod, string fileName)
			{
				this.initFsMod = initFsMod;
				this.FileName = fileName;
			}

		}

		public AssetEntry AssetEntry;

		public FileEntry SelectedEntry => (FileEntry)lb.SelectedItem;

		public string SelectedEntryText { get; set; }
        public Browser ParentBrowser { get; internal set; }

        public void Load()
		{
			browserDocumentsPane.Children.Clear();

			
			List<FileEntry> list = new List<FileEntry>();
			
			lb.ItemsSource = list;
		}

		private async void Export_Click(object sender, RoutedEventArgs e)
		{
			await ExportAsync().ConfigureAwait(continueOnCapturedContext: true);
		}

		private async void Import_Click(object sender, RoutedEventArgs e)
		{
			await ImportAsync().ConfigureAwait(continueOnCapturedContext: true);
		}

		private void Revert_Click(object sender, RoutedEventArgs e)
		{
		}

		private async Task ExportAsync(FileEntry entry, string file)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}
		}

		private async Task ExportAsync(IEnumerable<FileEntry> entries, string path, bool hierarchicalExport)
		{
			if (entries == null)
			{
				throw new ArgumentNullException("entries");
			}
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
		}

		private async Task ImportAsync(FileEntry entry, string file)
		{
			if (entry == null)
			{
				throw new ArgumentNullException("entry");
			}
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}
		}

		private void Reconstruct(AssetEntry assetEntry)
		{
			if (assetEntry == null)
			{
				throw new ArgumentNullException("assetEntry");
			}
			List<FileEntry> entries = (List<FileEntry>)lb.ItemsSource;
			
		}

		private async Task ExportAsync()
		{
            object selectedItem = lb.SelectedItem;
            FileEntry entry = selectedItem as FileEntry;
            if (entry == null)
            {
                return;
            }
            //string filter = entry.Type + " (*." + entry.Type.ToLower() + ")|*." + entry.Type;
            //if (entry.Type == "DDS")
            //{
            //    filter = "PNG (*.png)|*.png|" + filter;
            //}
            //SaveFileDialog dialog = new SaveFileDialog
            //{
            //    Filter = filter,
            //    FileName = entry.Name.Replace('\\', '_').Replace('/', '_'),
            //    AddExtension = true,
            //    Title = "Export File"
            //};
            //if (dialog.ShowDialog() == true)
            //{
            //    string filename = dialog.FileName;
            //    string extension = System.IO.Path.GetExtension(filename);
            //    string filterExtension = filter.Split('|')[dialog.FilterIndex * 2 - 1];
            //    if (string.IsNullOrEmpty(extension) || (filterExtension.Equals("*.DDS", StringComparison.OrdinalIgnoreCase) && !extension.Equals("." + entry.Type, StringComparison.Ordinal)))
            //    {
            //        filename = filename + "." + entry.Type;
            //    }

            //    await ExportAsync(entry, filename);
            //}
        }

		private async Task ImportAsync()
		{
            FileEntry entry = lb.SelectedItem as FileEntry;
            if (entry == null)
            {
                return;
            }
   //         string filter = entry.Type + " (*." + entry.Type.ToLower() + ")|*." + entry.Type;
   //         if (entry.Type == "DDS")
   //         {
   //             filter = "All Images|*.png;*.dds";
   //         }
   //         Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
			//{
   //             Filter = filter,
   //             FileName = entry.Name.Replace('\\', '_').Replace('/', '_'),
   //             Title = "Import File"
   //         };
   //         if (dialog.ShowDialog() == true)
   //         {
   //             try
   //             {
   //                 await ImportAsync(entry, dialog.FileName);
   //             }
   //             catch (InvalidDataException)
   //             {
   //             }
   //         }
			
			//if(ParentBrowser != null)
   //         {
			//	ParentBrowser.UpdateAssetListView();
   //         }
        }

        private void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if(lb.SelectedItem != null)
            {
				//var bigFileEntry = lb.SelectedItem as BigFileEntry;

				// ---- ----------
				// stop duplicates
				browserDocumentsPane.AllowDuplicateContent = false;
				foreach(var child in browserDocumentsPane.Children)
                {
					//if(child.Title == SelectedEntry.DisplayName)
     //               {
					//	browserDocumentsPane.SelectedContentIndex = browserDocumentsPane.Children.IndexOf(child);
					//	return;
     //               }
                }
				// ---- ----------

				//var newLayoutDoc = new LayoutDocument();
				//newLayoutDoc.Title = SelectedEntry.DisplayName;
				//if (SelectedEntry.Type == "DDS")
				//{
				//	System.Windows.Controls.Image imageEditor = new System.Windows.Controls.Image();
				//	ImageEngineImage imageEngineImage = new ImageEngineImage(SelectedEntry.Data);
				//	var iData = imageEngineImage.Save(new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.BMP), MipHandling.KeepTopOnly, removeAlpha: false);
				//	imageEditor.Source = LoadImage(iData);
				//	newLayoutDoc.Content = imageEditor;
				//}
				////else if (SelectedEntry.Type == "APT")
				////{
				////	APTEditor aptEditor = new APTEditor(this);
				////	newLayoutDoc.Content = aptEditor;
				////}
				//else
    //            {
				//	WpfHexaEditor.HexEditor hexEditor = new WpfHexaEditor.HexEditor();
				//	hexEditor.Stream = new MemoryStream(SelectedEntry.Data);
				//	newLayoutDoc.Content = hexEditor;
    //                hexEditor.BytesModified += HexEditor_BytesModified;

				//	//APT aptTest = new APT();
				//	//aptTest.Read(new MemoryStream(SelectedEntry.Data));
				//}
				//bigBrowserDocumentsPane.Children.Insert(0, newLayoutDoc);
				//bigBrowserDocumentsPane.SelectedContentIndex = 0;


			}
		}

        private void HexEditor_BytesModified(object sender, WpfHexaEditor.Core.EventArguments.ByteEventArgs e)
        {
			var fileEntry = lb.SelectedItem as FileEntry;

			//var hexEditor = sender as WpfHexaEditor.HexEditor;
			//if(hexEditor != null)
   //         {
			//	fileEntry.Data = hexEditor.GetAllBytes(true);
			//	fileEntry.Size = (uint)bigFileEntry.Data.Length;
			//	bigFileEntry.IsModified = true;
			//	Reconstruct(AssetEntry);
			//	if (ParentBrowser != null)
			//	{
			//		ParentBrowser.UpdateAssetListView();
			//	}
			//}
        }

        private static System.Windows.Media.Imaging.BitmapImage LoadImage(byte[] imageData)
		{
			if (imageData == null || imageData.Length == 0) return null;
			var image = new System.Windows.Media.Imaging.BitmapImage();
			using (var mem = new MemoryStream(imageData))
			{
				mem.Position = 0;
				image.BeginInit();
				image.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.UriSource = null;
				image.StreamSource = mem;
				image.EndInit();
			}
			image.Freeze();
			return image;
		}
	}
}
