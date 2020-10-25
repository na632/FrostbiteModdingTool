using Frostbite.Textures;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using KUtility;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Globalization;
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
using v2k4FIFAModding.Frosty;
using Xceed.Wpf.AvalonDock.Converters;
using FrostySdk.FrostySdk.Managers;
using Microsoft.Win32;

namespace FIFAModdingUI.Pages.Common
{
    /// <summary>
    /// Interaction logic for Browser.xaml
    /// </summary>
    public partial class Browser : UserControl
    {

        public Browser()
        {
            InitializeComponent();
        }


        private List<IAssetEntry> allAssets;

        public List<IAssetEntry> AllAssetEntries
        {
            get { return allAssets; }
            set { allAssets = value; Update(); }
        }

        public List<string> CurrentAssets { get; set; }

        public int CurrentTier { get; set; }

        public string CurrentPath { get; set; }

        public void SelectOption(string name)
        {

        }

        public void BrowseTo(string path)
        {
            if(!string.IsNullOrEmpty(CurrentPath))
                CurrentPath = CurrentPath + "/" + path;
            else
                CurrentPath = path;

            Update();
        }

		private Dictionary<string, AssetPath> assetPathMapping = new Dictionary<string, AssetPath>(StringComparer.OrdinalIgnoreCase);
		private AssetPath selectedPath;

		public void Update()
        {
			AssetPath assetPath = new AssetPath("", "", null);
			foreach (AssetEntry item in AllAssetEntries)
			{
				//if ((!ShowOnlyModified || item.IsModified) && FilterText(item.Name, item))
				{
					string[] array = item.Path.Split(new char[1]
					{
							'/'
					}, StringSplitOptions.RemoveEmptyEntries);
					AssetPath assetPath2 = assetPath;
					string[] array2 = array;
					foreach (string text in array2)
					{
						bool flag = false;
						foreach (AssetPath child in assetPath2.Children)
						{
							if (child.PathName.Equals(text, StringComparison.OrdinalIgnoreCase))
							{
								if (text.ToCharArray().Any((char a) => char.IsUpper(a)))
								{
									child.UpdatePathName(text);
								}
								assetPath2 = child;
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							string text2 = assetPath2.FullPath + "/" + text;
							AssetPath assetPath3 = null;
							if (!assetPathMapping.ContainsKey(text2))
							{
								assetPath3 = new AssetPath(text, text2, assetPath2);
								assetPathMapping.Add(text2, assetPath3);
							}
							else
							{
								assetPath3 = assetPathMapping[text2];
								assetPath3.Children.Clear();
								if (assetPath3 == selectedPath)
								{
									selectedPath.IsSelected = true;
								}
							}
							assetPath2.Children.Add(assetPath3);
							assetPath2 = assetPath3;
						}
					}
				}
			}
            if (!assetPathMapping.ContainsKey("/"))
            {
                //assetPathMapping.Add("/", new AssetPath("![root]", "", null, bInRoot: true));
                assetPathMapping.Add("/", new AssetPath("", "", null, bInRoot: true));
            }
            assetPath.Children.Insert(0, assetPathMapping["/"]);

			Dispatcher.BeginInvoke((Action)(() =>
			{
				assetTreeView.ItemsSource = assetPath.Children;
				assetTreeView.Items.SortDescriptions.Add(new SortDescription("PathName", ListSortDirection.Ascending));
			}));





		}

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
			if(SelectedLegacyEntry != null)
            {
				if (SelectedLegacyEntry != null)
				{
					SaveFileDialog saveFileDialog = new SaveFileDialog();
					var filt = "*." + SelectedLegacyEntry.Type;
					saveFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
					saveFileDialog.FileName = SelectedLegacyEntry.Filename;
					if (saveFileDialog.ShowDialog().Value)
					{
						using (NativeWriter nativeWriter = new NativeWriter(new FileStream(saveFileDialog.FileName, FileMode.Create)))
						{
							nativeWriter.Write(new NativeReader(ProjectManagement.Instance.FrostyProject.AssetManager.GetCustomAsset("legacy", SelectedLegacyEntry)).ReadToEnd());
						}
					}
				}
			}
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
			Label label = sender as Label;
			if (label != null && label.Tag != null) 
			{
				var assetPath = label.Tag as AssetPath;
				if (assetPath != null) 
				{
					if (assetPath.FullPath.Length > 3)
					{
						var filterPath = (assetPath.FullPath.Substring(1, assetPath.FullPath.Length - 1));
						var filteredAssets = AllAssetEntries.Where(x => x.Path.ToLower() == filterPath.ToLower());
						assetListView.ItemsSource = filteredAssets.Take(100);
						
					}
				}
			}
        }

		public EbxAssetEntry SelectedEbxEntry;
		public LegacyFileEntry SelectedLegacyEntry;

        private void AssetEntry_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
			try
			{
				SelectedEbxEntry = null;
				SelectedLegacyEntry = null;
				btnImport.IsEnabled = false;
				btnExport.IsEnabled = false;

				ImageViewer.Visibility = Visibility.Collapsed;
				TextViewer.Visibility = Visibility.Collapsed;
				EBXViewer.Visibility = Visibility.Collapsed;

				Control control = sender as Control;
				if (control != null)
				{
					EbxAssetEntry ebxEntry = control.Tag as EbxAssetEntry;
					if (ebxEntry != null)
					{
						SelectedEbxEntry = ebxEntry;
						if (ebxEntry.Type == "TextureAsset")
						{
							try
							{
								var eb = AssetManager.Instance.GetEbx(ebxEntry);
								if (eb != null)
								{
									var res = AssetManager.Instance.GetResEntry(ebxEntry.Name);
									if (res != null)
									{
										using (var resStream = ProjectManagement.Instance.FrostyProject.AssetManager.GetRes(res))
										{
											using (Texture textureAsset = new Texture(resStream, ProjectManagement.Instance.FrostyProject.AssetManager))
											{
												try
												{
													ImageViewer.Source = null;

													var bPath = Directory.GetCurrentDirectory() + @"\temp.png";

													//var textureBytes = new TextureExporter().WriteToDDS(textureAsset);
													//using (NativeWriter writer = new NativeWriter(new FileStream("temp.dds", FileMode.OpenOrCreate)))
													//                                    {
													//	writer.Write(textureBytes);
													//                                    }
													//DDSImage img = new DDSImage(File.ReadAllBytes(bPath));
													TextureExporter textureExporter = new TextureExporter();
													MemoryStream memoryStream = new MemoryStream();
													var textureBytes = new NativeReader(textureExporter.ExportToSteam(textureAsset)).ReadToEnd();

													//for (int i = 0; i < img.images.Length; i++)
													//{
													//	img.images[i].Save(Directory.GetCurrentDirectory() + @"\mipmap-" + i + ".bmp", ImageFormat.Bmp);
													//}
													//var bPathMip = Directory.GetCurrentDirectory() + @"\mipmap-0.bmp";

													var bImage = LoadImage(textureBytes);
													ImageViewer.Source = bImage;
													ImageViewer.Visibility = Visibility.Visible;
													//bImage = null;
												}
												catch { ImageViewer.Source = null; }
											}
										}
									}
								}
							}
							catch (Exception)
							{
								//Log("Failed to load texture");
							}



						}
						else
                        {
							if (ebxEntry == null || ebxEntry.Type == "EncryptedAsset")
							{
								return;
							}
							EBXViewer.Children.Clear();
							var ebx = ProjectManagement.Instance.FrostyProject.AssetManager.GetEbx(ebxEntry);
							if (ebx != null)
							{
								EBXViewer.Children.Add(new Editor(ebxEntry, ebx, ProjectManagement.Instance.FrostyProject));
								EBXViewer.Visibility = Visibility.Visible;
							}
						}
					}

					LegacyFileEntry legacyFileEntry = control.Tag as LegacyFileEntry;
					if (legacyFileEntry != null)
					{
						SelectedLegacyEntry = legacyFileEntry;

						List<string> textViewers = new List<string>()
						{
							"LUA",
							"XML",
							"INI",
							"NAV",
							"JSON",
							"CSV"
						};

						List<string> imageViewers = new List<string>()
						{
							"PNG",
							"DDS"
						};
						if (textViewers.Contains(legacyFileEntry.Type))
						{
							btnExport.IsEnabled = true;
							TextViewer.Visibility = Visibility.Visible;
							using (var nr = new NativeReader(ProjectManagement.Instance.FrostyProject.AssetManager.GetCustomAsset("legacy", legacyFileEntry)))
							{
								TextViewer.Text = ASCIIEncoding.ASCII.GetString(nr.ReadToEnd());
							}
						}
						else if (imageViewers.Contains(legacyFileEntry.Type))
						{
							btnExport.IsEnabled = true;
							ImageViewer.Visibility = Visibility.Visible;
							using (var nr = new NativeReader(ProjectManagement.Instance.FrostyProject.AssetManager.GetCustomAsset("legacy", legacyFileEntry)))
							{
								var bImage = LoadImage(nr.ReadToEnd());
								ImageViewer.Source = bImage;
							}
						}

					}


				}
			}
			catch
            {

            }
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

    internal class AssetPath
	{
		private static ImageSource ClosedImage;// = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyEditor;component/Images/CloseFolder.png") as ImageSource;

		private static ImageSource OpenImage;// = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyEditor;component/Images/OpenFolder.png") as ImageSource;

		private string fullPath;

		private string name;

		private bool expanded;

		private bool selected;

		private bool root;

		private AssetPath parent;

		private List<AssetPath> children = new List<AssetPath>();

		public string DisplayName => name.Trim('!');

		public string PathName => name;

		public string FullPath => fullPath;

		public AssetPath Parent => parent;

		public List<AssetPath> Children => children;

		public bool IsExpanded
		{
			get
			{
				if (expanded)
				{
					return Children.Count != 0;
				}
				return false;
			}
			set
			{
				expanded = value;
			}
		}

		public bool IsSelected
		{
			get
			{
				return selected;
			}
			set
			{
				selected = value;
			}
		}

		public bool IsRoot => root;

		public AssetPath(string inName, string path, AssetPath inParent, bool bInRoot = false)
		{
			name = inName;
			fullPath = path;
			root = bInRoot;
			parent = inParent;
		}

		public void UpdatePathName(string newName)
		{
			name = newName;
		}
	}

}
