using Frostbite.Textures;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
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
using FrostySdk.FrostySdk.Managers;
using Microsoft.Win32;
using FIFAModdingUI.Windows;
using Newtonsoft.Json;
using FrostbiteModdingUI.Windows;
using Frostbite;
using DDSReader;
using HelixToolkit.SharpDX.Core.Assimp;
using FrostbiteModdingUI.Models;
using System.Threading;
using FrostySdk.Frostbite.IO.Output;
using System.Diagnostics;
using Frostbite.FileManagers;

namespace FIFAModdingUI.Pages.Common
{
	/// <summary>
	/// Interaction logic for Browser.xaml
	/// </summary>
	public partial class Browser : UserControl
	{

		private IEditorWindow MainEditorWindow
		{
			get
			{
				return App.MainEditorWindow as IEditorWindow;
			}
		}
		public Browser()
		{
			InitializeComponent();
			DataContext = this;
		}

		public int HalfMainWindowWidth { get { return MainEditorWindow != null ? (int)Math.Round(((Window)MainEditorWindow).ActualWidth / 2) : 400; } }


		private List<IAssetEntry> allAssets;

		public List<IAssetEntry> AllAssetEntries
		{
			get { return allAssets; }
			set { allAssets = value; Update(); }
		}

		public string FilterText { get; set; }

		public async Task<List<IAssetEntry>> GetFilteredAssetEntries()
		{

			return await Dispatcher.InvokeAsync(() =>
			{
				var assets = allAssets;
				if (assets != null)
				{
					bool OnlyModified
					= chkShowOnlyModified.IsChecked.HasValue
					&& chkShowOnlyModified.IsChecked.Value;

					if (!string.IsNullOrEmpty(txtFilter.Text))
					{
						assets = assets.Where(x =>
							x.Name.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase)
							).ToList();
					}

					assets = assets.Where(x =>

						(
						chkShowOnlyModified.IsChecked == true
						&& x.IsModified
						)
						|| chkShowOnlyModified.IsChecked == false

						).ToList();

				}
				return assets;

			});

		}

		public List<string> CurrentAssets { get; set; }

		public int CurrentTier { get; set; }

		public string CurrentPath { get; set; }

		public void SelectOption(string name)
		{

		}

		public void BrowseTo(string path)
		{
			if (!string.IsNullOrEmpty(CurrentPath))
				CurrentPath = CurrentPath + "/" + path;
			else
				CurrentPath = path;

			Update();
		}

		private Dictionary<string, AssetPath> assetPathMapping = new Dictionary<string, AssetPath>(StringComparer.OrdinalIgnoreCase);
		private AssetPath selectedPath = null;

		public async void Update()
		{
			AssetPath assetPath = new AssetPath("", "", null);

			var assets = await GetFilteredAssetEntries();

			foreach (AssetEntry item in assets)
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

			await Dispatcher.InvokeAsync((Action)(() =>
			{
				assetTreeView.ItemsSource = assetPath.Children;
				assetTreeView.Items.SortDescriptions.Add(new SortDescription("PathName", ListSortDirection.Ascending));
			}));

		}

		private void btnImport_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//var imageFilter = "Image files (*.DDS, *.PNG)|*.DDS;*.PNG";
				var imageFilter = "Image files (*.DDS, *.PNG)|*.DDS;*.PNG";
				if (SelectedLegacyEntry != null)
				{
					OpenFileDialog openFileDialog = new OpenFileDialog();
					openFileDialog.Filter = $"Files (*.{SelectedLegacyEntry.Type})|*.{SelectedLegacyEntry.Type}";
					openFileDialog.FileName = SelectedLegacyEntry.Filename;

					bool isImage = false;
					if (SelectedLegacyEntry.Type == "DDS")
					{
						//openFileDialog.Filter = imageFilter;
						isImage = true;
					}

					var result = openFileDialog.ShowDialog();
					if (result.HasValue && result.Value == true)
					{
						byte[] bytes = File.ReadAllBytes(openFileDialog.FileName);

						if (isImage)
						{
							DDSImage originalImage = new DDSImage(AssetManager.Instance.GetCustomAsset("legacy", SelectedLegacyEntry));
							DDSImage newImage = new DDSImage(bytes);
							if (originalImage._image.Width != newImage._image.Width)
							{
								throw new Exception("Invalid Width of New Image for Legacy Asset");
							}

							var textureBytes = new NativeReader(newImage.SaveToStream()).ReadToEnd();
							ImageViewer.Source = LoadImage(textureBytes);
							ImageViewerScreen.Visibility = Visibility.Visible;
						}
						else
						{
							TextViewer.Text = ASCIIEncoding.ASCII.GetString(bytes);
						}

						AssetManager.Instance.ModifyCustomAsset("legacy"
							, SelectedLegacyEntry.Name
							, bytes);


					}
				}
				else if (SelectedEntry != null)
				{
					if (SelectedEntry.Type == "TextureAsset" || SelectedEntry.Type == "Texture")
					{
						OpenFileDialog openFileDialog = new OpenFileDialog();
						openFileDialog.Filter = imageFilter;
						openFileDialog.FileName = SelectedEntry.Filename;
						if (openFileDialog.ShowDialog().Value)
						{
							var resEntry = ProjectManagement.Instance.Project.AssetManager.GetResEntry(SelectedEntry.Name);
							if (resEntry != null)
							{

								using (var resStream = ProjectManagement.Instance.Project.AssetManager.GetRes(resEntry))
								{

									Texture texture = new Texture(resStream, ProjectManagement.Instance.Project.AssetManager);
									TextureImporter textureImporter = new TextureImporter();
									EbxAssetEntry ebxAssetEntry = SelectedEntry as EbxAssetEntry;

									if (ebxAssetEntry != null)
										textureImporter.Import(openFileDialog.FileName, ebxAssetEntry, ref texture);

									var res = AssetManager.Instance.GetResEntry(SelectedEntry.Name);
									if (res != null)
									{
										BuildTextureViewerFromAssetEntry(res);
									}

									MainEditorWindow.Log($"Imported {openFileDialog.FileName} to {SelectedEntry.Filename}");
								}
							}
						}
					}

					if (SelectedEntry.Type == "HotspotDataAsset")
					{
						OpenFileDialog openFileDialog = new OpenFileDialog();
						var filt = "*.json";
						openFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
						openFileDialog.FileName = SelectedEntry.Filename;
						var dialogResult = openFileDialog.ShowDialog();
						if (dialogResult.HasValue && dialogResult.Value)
						{
							var ebx = AssetManager.Instance.GetEbx((EbxAssetEntry)SelectedEntry);
							if (ebx != null)
							{
								var robj = (dynamic)ebx.RootObject;
								var fileHS = (Newtonsoft.Json.Linq.JArray)JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(openFileDialog.FileName)).Hotspots;
								var fhs2 = fileHS.ToObject<List<dynamic>>();
								for (var i = 0; i < robj.Hotspots.Count; i++)
								{
									robj.Hotspots[i].Bounds.x = (float)fhs2[i].Bounds.x;
									robj.Hotspots[i].Bounds.y = (float)fhs2[i].Bounds.y;
									robj.Hotspots[i].Bounds.z = (float)fhs2[i].Bounds.z;
									robj.Hotspots[i].Bounds.w = (float)fhs2[i].Bounds.w;
									robj.Hotspots[i].Rotation = (float)fhs2[i].Rotation;
								}
								AssetManager.Instance.ModifyEbx(SelectedEntry.Name, ebx);

								// Update the Viewers
								UpdateAssetListView();
								EBXViewer.Children.Clear();
								EBXViewer.Children.Add(new Editor(SelectedEntry, ebx, ProjectManagement.Instance.Project, MainEditorWindow));
							}
						}
					}

					if (SelectedEntry.Type == "SkinnedMeshAsset")
					{
						OpenFileDialog openFileDialog = new OpenFileDialog();
						openFileDialog.Filter = "Fbx files (*.fbx)|*.fbx";
						openFileDialog.FileName = SelectedEntry.Filename;
						if (openFileDialog.ShowDialog().Value)
						{
							var skinnedMeshEbx = AssetManager.Instance.GetEbx((EbxAssetEntry)SelectedEntry);
							if (skinnedMeshEbx != null)
							{
								var resentry = AssetManager.Instance.GetResEntry(SelectedEntry.Name);
								var res = AssetManager.Instance.GetRes(resentry);
								MeshSet meshSet = new MeshSet(res);

								try
								{
									FrostySdk.Frostbite.IO.Input.FBXImporter importer = new FrostySdk.Frostbite.IO.Input.FBXImporter();
									importer.ImportFBX(openFileDialog.FileName, meshSet, skinnedMeshEbx, (EbxAssetEntry)SelectedEntry
										, new FrostySdk.Frostbite.IO.Input.MeshImportSettings()
										{
											SkeletonAsset = "content/character/rig/skeleton/player/skeleton_player"
										});
									MainEditorWindow.Log($"Imported {openFileDialog.FileName} to {SelectedEntry.Name}");

									UpdateAssetListView();
								}
								catch (Exception ImportException)
								{
									MainEditorWindow.LogError(ImportException.Message);
								}

							}
						}
					}
				}

			}
			catch(Exception ex)
            {
				MainEditorWindow.LogError(ex.Message);
            }
			UpdateAssetListView();
		}

		private void btnExport_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedLegacyEntry != null)
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				var filt = "*." + SelectedLegacyEntry.Type;
				if(SelectedLegacyEntry.Type == "DDS")
					saveFileDialog.Filter = "PNG files (*.png)|*.png";
				else
					saveFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
				
				saveFileDialog.FileName = SelectedLegacyEntry.Filename;

				if (saveFileDialog.ShowDialog().Value)
				{
					var legacyData = ProjectManagement.Instance.Project.AssetManager.GetCustomAsset("legacy", SelectedLegacyEntry);
					if (SelectedLegacyEntry.Type == "DDS")
					{
						DDSImage image = new DDSImage(legacyData);
						image.Save(saveFileDialog.FileName);
					}
					else
                    {
						File.WriteAllBytes(saveFileDialog.FileName, new NativeReader(legacyData).ReadToEnd());
                    }
					MainEditorWindow.Log($"Exported {SelectedLegacyEntry.Filename} to {saveFileDialog.FileName}");
				}
			}
			else if (SelectedEntry != null)
			{
				if (SelectedEntry.Type == "TextureAsset")
				{
					SaveFileDialog saveFileDialog = new SaveFileDialog();
					//var imageFilter = "Image files (*.DDS, *.PNG)|*.DDS;*.PNG";
					var imageFilter = "Image files (*.PNG)|*.PNG";
					saveFileDialog.Filter = imageFilter;
					saveFileDialog.FileName = SelectedEntry.Filename;
					saveFileDialog.AddExtension = true;
					if (saveFileDialog.ShowDialog().Value)
					{
						var resEntry = ProjectManagement.Instance.Project.AssetManager.GetResEntry(SelectedEntry.Name);
						if (resEntry != null)
						{

							using (var resStream = ProjectManagement.Instance.Project.AssetManager.GetRes(resEntry))
							{
								Texture texture = new Texture(resStream, ProjectManagement.Instance.Project.AssetManager);
								var extractedExt = saveFileDialog.FileName.Substring(saveFileDialog.FileName.Length - 3, 3);
								TextureExporter textureExporter = new TextureExporter();
								textureExporter.Export(texture, saveFileDialog.FileName, "*." + extractedExt);
								MainEditorWindow.Log($"Exported {SelectedEntry.Filename} to {saveFileDialog.FileName}");
							}


						}
					}
				}

				if (SelectedEntry.Type == "HotspotDataAsset")
				{
					var ebx = AssetManager.Instance.GetEbx((EbxAssetEntry)SelectedEntry);
					if (ebx != null)
					{
						SaveFileDialog saveFileDialog = new SaveFileDialog();
						var filt = "*.json";
						saveFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
						saveFileDialog.FileName = SelectedEntry.Filename;
						var dialogAnswer = saveFileDialog.ShowDialog();
						if (dialogAnswer.HasValue && dialogAnswer.Value)
						{
							var json = JsonConvert.SerializeObject(ebx.RootObject, Formatting.Indented);
							File.WriteAllText(saveFileDialog.FileName, json);
							MainEditorWindow.Log($"Exported {SelectedEntry.Filename} to {saveFileDialog.FileName}");
						}
					}
					else
					{
						MainEditorWindow.Log("Failed to export file");
					}
				}

				if (SelectedEntry.Type == "SkinnedMeshAsset")
				{
					var skinnedMeshEntry = (EbxAssetEntry)SelectedEntry;
					if (skinnedMeshEntry != null)
					{

						var skinnedMeshEbx = AssetManager.Instance.GetEbx(skinnedMeshEntry);
						if (skinnedMeshEbx != null)
						{
							var resentry = AssetManager.Instance.GetResEntry(skinnedMeshEntry.Name);
							var res = AssetManager.Instance.GetRes(resentry);
							MeshSet meshSet = new MeshSet(res);

							SaveFileDialog saveFileDialog = new SaveFileDialog();
							var filt = "*.fbx";
							saveFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
							saveFileDialog.FileName = SelectedEntry.Filename;
							var dialogAnswer = saveFileDialog.ShowDialog();
							if (dialogAnswer.HasValue && dialogAnswer.Value)
							{
								var exporter = new MeshToFbxExporter();
								exporter.Export(AssetManager.Instance
									, skinnedMeshEbx.RootObject
									, saveFileDialog.FileName, "2016", "Millimeters", true, "content/character/rig/skeleton/player/skeleton_player", "*.fbx", meshSet);
								
								
								MainEditorWindow.Log($"Exported {SelectedEntry.Name} to {saveFileDialog.FileName}");


							}
						}
					}
				}
			}
		}

		AssetPath SelectedAssetPath = null;

		private void Label_MouseUp(object sender, MouseButtonEventArgs e)
		{
			Label label = sender as Label;
			if (label != null && label.Tag != null)
			{
				SelectedAssetPath = label.Tag as AssetPath;
				UpdateAssetListView();
			}
		}

		private void Label_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				Label label = sender as Label;
				if (label != null && label.Tag != null)
				{
					SelectedAssetPath = label.Tag as AssetPath;
					UpdateAssetListView();
				}
			}
		}

		public void UpdateAssetListView()
		{
			if (SelectedAssetPath != null)
			{
				if (SelectedAssetPath.FullPath.Length > 3)
				{
					var filterPath = (SelectedAssetPath.FullPath.Substring(1, SelectedAssetPath.FullPath.Length - 1));
					var filteredAssets = AllAssetEntries.Where(x => x.Path.ToLower() == filterPath.ToLower());
					filteredAssets = filteredAssets.Where(x => x.Name.Contains(txtFilter.Text, StringComparison.OrdinalIgnoreCase));

					filteredAssets = filteredAssets.Where(x =>

						(
						chkShowOnlyModified.IsChecked == true
						&& x.IsModified
						)
						|| chkShowOnlyModified.IsChecked == false

						).ToList();

					var selectedit = assetListView.SelectedItem;
					assetListView.ItemsSource = filteredAssets.OrderBy(x => x.Name);
					assetListView.SelectedItem = selectedit;

				}
			}
		}

		public AssetEntry SelectedEntry;
		public LegacyFileEntry SelectedLegacyEntry;

		private void AssetEntry_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			AssetEntry entry = ((TextBlock)sender).Tag as EbxAssetEntry;
			if (entry == null)
				entry = ((TextBlock)sender).Tag as LegacyFileEntry;

			OpenAsset(entry);
		}

		private void AssetEntry_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				AssetEntry entry = ((TextBlock)sender).Tag as EbxAssetEntry;
				if (entry == null)
					entry = ((TextBlock)sender).Tag as LegacyFileEntry;

				OpenAsset(entry);
			}
		}


		private void OpenAsset(AssetEntry entry)
		{
			try
			{
				SelectedEntry = null;
				SelectedLegacyEntry = null;
				btnImport.IsEnabled = false;
				btnExport.IsEnabled = false;
				btnRevert.IsEnabled = false;

				ImageViewerScreen.Visibility = Visibility.Collapsed;
				TextViewer.Visibility = Visibility.Collapsed;
				EBXViewer.Visibility = Visibility.Collapsed;
				UnknownLegacyFileViewer.Visibility = Visibility.Collapsed;
				ModelViewer.Visibility = Visibility.Collapsed;
				//HEXViewer.Visibility = Visibility.Collapsed;

				//TextBlock control = sender as TextBlock;
				//if (control != null)
				{
					EbxAssetEntry ebxEntry = entry as EbxAssetEntry;
					if (ebxEntry != null)
					{
						SelectedEntry = ebxEntry;
						if (ebxEntry.Type == "TextureAsset")
						{
							try
							{
								MainEditorWindow.Log("Loading Texture " + ebxEntry.Filename);

								var eb = AssetManager.Instance.GetEbx(ebxEntry);
								if (eb != null)
								{
									var res = AssetManager.Instance.GetResEntry(ebxEntry.Name);
									if (res != null)
									{
										BuildTextureViewerFromAssetEntry(res);
									}
									else
                                    {
										throw new Exception("Unable to find RES Entry for " + ebxEntry.Name);
                                    }
								}
							}
							catch (Exception e)
							{
								MainEditorWindow.Log($"Failed to load texture with the message :: {e.Message}");
							}



						}
						else if (ebxEntry.Type == "SkinnedMeshAsset")
						{
							if (ebxEntry == null || ebxEntry.Type == "EncryptedAsset")
							{
								return;
							}
							var ebx = ProjectManagement.Instance.Project.AssetManager.GetEbx(ebxEntry);
							if (ebx != null)
							{
								MainEditorWindow.Log("Loading 3D Model " + ebxEntry.Filename);
								var skinnedMeshEbx = AssetManager.Instance.GetEbx(ebxEntry);
								if (skinnedMeshEbx != null)
								{
									var resentry = AssetManager.Instance.GetResEntry(ebxEntry.Name);
									var res = AssetManager.Instance.GetRes(resentry);
									MeshSet meshSet = new MeshSet(res);

									var exporter = new MeshToFbxExporter();
									exporter.OnlyFirstLOD = true;
									exporter.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test_noSkel.obj", "2016", "Meters", true, null, "*.obj", meshSet);
									Thread.Sleep(1000);

									//var import = new Importer();
									//var scene = import.Load("test_noSkel.obj", new ImporterConfiguration() { GlobalScale = 100, FlipWindingOrder = true, CullMode = SharpDX.Direct3D11.CullMode.None, });
									var m = new MainViewModel();
									this.ModelViewer.DataContext = m;
									this.ModelViewer.Visibility = Visibility.Visible;
									
									this.btnExport.IsEnabled = true;
									this.btnImport.IsEnabled = true;
								}

									
							}
						}
						else
						{
							if (ebxEntry == null || ebxEntry.Type == "EncryptedAsset")
							{
								return;
							}
							EBXViewer.Children.Clear();
							var ebx = ProjectManagement.Instance.Project.AssetManager.GetEbx(ebxEntry);
							if (ebx != null)
							{
								MainEditorWindow.Log("Loading EBX " + ebxEntry.Filename);

								EBXViewer.Children.Add(new Editor(ebxEntry, ebx, ProjectManagement.Instance.Project, MainEditorWindow));
								EBXViewer.Visibility = Visibility.Visible;
								btnRevert.IsEnabled = true;
								if (ebxEntry.Type == "HotspotDataAsset")
								{
									btnImport.IsEnabled = true;
									btnExport.IsEnabled = true;
								}
							}
						}
					}
					else
					{

						{

							LegacyFileEntry legacyFileEntry = entry as LegacyFileEntry;
							if (legacyFileEntry != null)
							{
								SelectedLegacyEntry = legacyFileEntry;
								btnImport.IsEnabled = true;

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
									MainEditorWindow.Log("Loading Legacy File " + SelectedLegacyEntry.Filename);

									btnImport.IsEnabled = true;
									btnExport.IsEnabled = true;
									btnRevert.IsEnabled = true;

									TextViewer.Visibility = Visibility.Visible;
									using (var nr = new NativeReader(ProjectManagement.Instance.Project.AssetManager.GetCustomAsset("legacy", legacyFileEntry)))
									{
										TextViewer.Text = ASCIIEncoding.ASCII.GetString(nr.ReadToEnd());
									}
								}
								else if (imageViewers.Contains(legacyFileEntry.Type))
								{
									MainEditorWindow.Log("Loading Legacy File " + SelectedLegacyEntry.Filename);
									btnImport.IsEnabled = true;
									btnExport.IsEnabled = true;
									ImageViewerScreen.Visibility = Visibility.Visible;

									BuildTextureViewerFromStream(ProjectManagement.Instance.Project.AssetManager.GetCustomAsset("legacy", legacyFileEntry), legacyFileEntry);


								}
								else
								{
									MainEditorWindow.Log("Loading Unknown Legacy File " + SelectedLegacyEntry.Filename);
									btnExport.IsEnabled = true;
									btnRevert.IsEnabled = true;

									UnknownLegacyFileViewer.Visibility = Visibility.Visible;
									//HEXViewer.Visibility = Visibility.Visible;
									//byte[] bytes;
									//using (var nr = new NativeReader(ProjectManagement.Instance.FrostyProject.AssetManager.GetCustomAsset("legacy", legacyFileEntry)))
									//	bytes = nr.ReadToEnd();

									//if (File.Exists("HexViewer.dat"))
									//	File.Delete("HexViewer.dat");

									//File.WriteAllBytes("HexViewer.dat", bytes);
									//HEXViewer.FileName = "HexViewer.dat";
								}

							}
						}

					}
				}
			}
			catch (Exception e)
			{
				MainEditorWindow.Log($"Failed to load file with message {e.Message}");
				Debug.WriteLine(e.ToString());

			}
		}

		private void BuildTextureViewerFromAssetEntry(ResAssetEntry res)
        {
			using (var resStream = ProjectManagement.Instance.Project.AssetManager.GetRes(res))
			{
				// {3e0a186b-c286-1dff-455b-7eb097c3e8f9} Splashscreen Guid
				using (Texture textureAsset = new Texture(resStream, ProjectManagement.Instance.Project.AssetManager))
                {
                    try
					{
						ImageViewer.Source = null;

						var bPath = Directory.GetCurrentDirectory() + @"\temp.png";

						TextureExporter textureExporter = new TextureExporter();
						MemoryStream memoryStream = new MemoryStream();

						Stream expToStream = null;
						try
						{
							expToStream = textureExporter.ExportToStream(textureAsset, TextureUtils.ImageFormat.PNG);
						}
						catch (Exception exception_ToStream)
                        {
							MainEditorWindow.LogError($"Error loading texture with message :: {exception_ToStream.Message}");
							MainEditorWindow.LogError(exception_ToStream.ToString());
							ImageViewer.Source = null; ImageViewerScreen.Visibility = Visibility.Collapsed;

							textureExporter.Export(textureAsset, res.Filename + ".DDS", "*.DDS");
							MainEditorWindow.LogError($"As the viewer failed. The image has been exported to {res.Filename}.dds instead.");
							return;
						}

						using var nr = new NativeReader(expToStream);
						nr.Position = 0;
						var textureBytes = nr.ReadToEnd();

						ImageViewer.Source = LoadImage(textureBytes);
						ImageViewerScreen.Visibility = Visibility.Visible;

						lblImageName.Content = res.Filename;
						lblImageDDSType.Content = textureAsset.PixelFormat;
						lblImageRESType.Content = textureAsset.Type;
						lblImageSize.Content = textureAsset.Width + "x" + textureAsset.Height;
						lblImageCASFile.Content = res.ExtraData.CasPath;
						lblImageCASOffset.Content = res.ExtraData.DataOffset;
						lblImageBundleFile.Content = !string.IsNullOrEmpty(res.SBFileLocation) ? res.SBFileLocation : res.TOCFileLocation;

						btnExport.IsEnabled = true;
						btnImport.IsEnabled = true; 
						btnRevert.IsEnabled = true;

					}
					catch (Exception e) {
						MainEditorWindow.LogError($"Error loading texture with message :: {e.Message}");
						MainEditorWindow.LogError(e.ToString());
						ImageViewer.Source = null; ImageViewerScreen.Visibility = Visibility.Collapsed; }
				}
			}
		}

		private void BuildTextureViewerFromStream(Stream stream, AssetEntry assetEntry)
        {

				try
				{
					ImageViewer.Source = null;

					var bPath = Directory.GetCurrentDirectory() + @"\temp.png";

				DDSImage image = new DDSImage(stream);
				var textureBytes = new NativeReader(image.SaveToStream()).ReadToEnd();
					//var textureBytes = new NativeReader(textureExporter.ExportToStream(texture)).ReadToEnd();

					ImageViewer.Source = LoadImage(textureBytes);
					ImageViewerScreen.Visibility = Visibility.Visible;

				lblImageName.Content = assetEntry.Filename;
				lblImageDDSType.Content = image._image.Format;
				lblImageRESType.Content = "DDS";
				lblImageSize.Content = image.Data.Length;
				//lblImageRESType.Content = textureAsset.Type;
				//lblImageSize.Content = textureAsset.Width + "x" + textureAsset.Height;
				//lblImageCASFile.Content = assetEntry.ExtraData.CasPath;
				//lblImageCASOffset.Content = assetEntry.ExtraData.DataOffset;
				//lblImageBundleFile.Content = !string.IsNullOrEmpty(assetEntry.SBFileLocation) ? assetEntry.SBFileLocation : assetEntry.TOCFileLocation;

				btnExport.IsEnabled = true;
					btnImport.IsEnabled = true;
					btnRevert.IsEnabled = true;

				}
				catch(Exception e) 
			{
				MainEditorWindow.LogError(e.Message);
				ImageViewer.Source = null; ImageViewerScreen.Visibility = Visibility.Collapsed; }

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

        private void btnRevert_Click(object sender, RoutedEventArgs e)
        {
			if (SelectedEntry != null)
			{
				if (SelectedEntry.Type == "TextureAsset" || SelectedEntry.Type == "Texture" || SelectedEntry.Type == "SkinnedMeshAsset")
				{
					AssetManager.Instance.RevertAsset(SelectedEntry);
				}
				else if (EBXViewer.Children.Count > 0)
				{
					var ebxviewer = EBXViewer.Children[0] as Editor;
					if (ebxviewer != null)
					{
						ebxviewer.RevertAsset();
					}
				}
			}
			else if (SelectedLegacyEntry != null)
            {
				AssetManager.Instance.RevertAsset(SelectedLegacyEntry);
            }
			

        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
			//Update();
        }

        private void txtFilter_KeyUp(object sender, KeyEventArgs e)
        {
			if (e.Key == Key.Enter)
			{
				Update();
			}
        }

        private void chkShowOnlyModified_Checked(object sender, RoutedEventArgs e)
        {
			Update();
        }

        private void chkShowOnlyModified_Unchecked(object sender, RoutedEventArgs e)
        {
			Update();
		}

        private void TextViewer_LostFocus(object sender, RoutedEventArgs e)
        {
			var bytes = ASCIIEncoding.UTF8.GetBytes(TextViewer.Text);

			if (SelectedLegacyEntry != null)
			{
				AssetManager.Instance.ModifyCustomAsset("legacy"
							, SelectedLegacyEntry.Name
							, bytes);
				UpdateAssetListView();
			}
		}

        private void TextBlock_PreviewKeyUp(object sender, KeyEventArgs e)
        {
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				TextBlock label = sender as TextBlock;
				if (label != null && label.Tag != null)
				{
					SelectedAssetPath = label.Tag as AssetPath;
					UpdateAssetListView();
				}
			}
		}

        private void TextBlock_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
			TextBlock label = sender as TextBlock;
			if (label != null && label.Tag != null)
			{
				SelectedAssetPath = label.Tag as AssetPath;
				UpdateAssetListView();
			}
		}

        private void assetTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
			var assetTreeViewSelectedItem = assetTreeView.SelectedItem as AssetPath;
			SelectedAssetPath = assetTreeViewSelectedItem;
			UpdateAssetListView();
		}

        private void assetListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			AssetEntry entry = ((ListView)sender).SelectedItem as EbxAssetEntry;
			if (entry == null)
				entry = ((ListView)sender).SelectedItem as LegacyFileEntry;

			OpenAsset(entry);
		}
    }

    internal class AssetPath
	{
		//private static ImageSource ClosedImage;// = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyEditor;component/Images/CloseFolder.png") as ImageSource;

		//private static ImageSource OpenImage;// = new ImageSourceConverter().ConvertFromString("pack://application:,,,/FrostyEditor;component/Images/OpenFolder.png") as ImageSource;

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
