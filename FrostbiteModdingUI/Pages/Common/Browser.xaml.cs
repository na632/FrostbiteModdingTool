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
//using DDSReader;
using HelixToolkit.SharpDX.Core.Assimp;
using FrostbiteModdingUI.Models;
using System.Threading;
using FrostySdk.Frostbite.IO.Output;
using System.Diagnostics;
using Frostbite.FileManagers;
using Assimp.Unmanaged;
using System.Runtime.InteropServices;
using System.Reflection;
using FrostySdk.Ebx;
using Frosty.Hash;
using v2k4FIFAModding;
//using FMT.Util;
using CSharpImageLibrary;
using FMT;
using System.Security.Cryptography;
using SharpDX.DXGI;
using static Frostbite.Textures.TextureUtils;
using static FMT.Pages.Common.BrowserOfBIG;
using AvalonDock.Layout;
using FolderBrowserEx;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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


		private IEnumerable<IAssetEntry> allAssets;

		public IEnumerable<IAssetEntry> AllAssetEntries
		{
			get { return allAssets; }
			set { allAssets = value; Update(); }
		}

		public string FilterText { get; set; }

		#region Entry Properties

		private AssetEntry assetEntry1;

		public AssetEntry SelectedEntry
		{
			get
			{
				if (assetEntry1 == null && SelectedLegacyEntry != null)
					return SelectedLegacyEntry;

				return assetEntry1;
			}
			set { assetEntry1 = value; }
		}

		public LegacyFileEntry SelectedLegacyEntry { get; set; }

        private EbxAsset ebxAsset;

		public EbxAsset SelectedEbxAsset
        {
            get { return ebxAsset; }
            set { ebxAsset = value; }
        }


        #endregion

        public async Task<IEnumerable<IAssetEntry>> GetFilteredAssetEntries()
		{
			var onlymodified = false;

			Dispatcher.Invoke(() =>
			{
				onlymodified = chkShowOnlyModified.IsChecked.Value;
			});
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
							);
					}

					assets = assets.Where(x =>

						(
						onlymodified == true
						&& x.IsModified
						)
						|| onlymodified == false

						);

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

		private Dictionary<string, AssetPath> assetPathMapping = new Dictionary<string, AssetPath>(StringComparer.OrdinalIgnoreCase);
		private AssetPath selectedPath = null;

		public async void Update()
		{
			AssetPath assetPath = new AssetPath("", "", null, true);

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
					foreach (string text in array)
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


		private async void btnImport_Click(object sender, RoutedEventArgs e)
		{
			var importStartTime = DateTime.Now;

			LoadingDialog loadingDialog = new LoadingDialog();
			loadingDialog.Show();
			try
			{
				
				var imageFilter = "Image files (*.dds, *.png)|*.dds;*.png";
				if (SelectedLegacyEntry != null)
				{
					OpenFileDialog openFileDialog = new OpenFileDialog();
					openFileDialog.Filter = $"Files (*.{SelectedLegacyEntry.Type})|*.{SelectedLegacyEntry.Type}";
					openFileDialog.FileName = SelectedLegacyEntry.Filename;

					bool isImage = false;
					if (SelectedLegacyEntry.Type == "DDS")
					{
						openFileDialog.Filter = imageFilter;
						isImage = true;
					}

					var result = openFileDialog.ShowDialog();
					if (result.HasValue && result.Value == true)
					{
						byte[] bytes = File.ReadAllBytes(openFileDialog.FileName);

						if (isImage)
						{
							if (AssetManager.Instance.DoLegacyImageImport(openFileDialog.FileName, SelectedLegacyEntry))
							{
								BuildTextureViewerFromStream((MemoryStream)AssetManager.Instance.GetCustomAsset("legacy", SelectedLegacyEntry));
							}
                            else
                            {
								if (loadingDialog != null && loadingDialog.Visibility == Visibility.Visible)
								{
									loadingDialog.Close();
								}
								return;
                            }
						}
						else
						{
							if(SelectedLegacyEntry.Type.ToUpper() != "DB" && SelectedLegacyEntry.Type.ToUpper() != "LOC")
								TextViewer.Text = ASCIIEncoding.UTF8.GetString(bytes);

							AssetManager.Instance.ModifyLegacyAsset(
								SelectedLegacyEntry.Name
								, bytes
								, false);
						}

						MainEditorWindow.Log($"Imported {openFileDialog.FileName} to {SelectedLegacyEntry.Filename}");
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
								Texture texture = new Texture(resEntry);
								TextureImporter textureImporter = new TextureImporter();
								EbxAssetEntry ebxAssetEntry = SelectedEntry as EbxAssetEntry;

								if (ebxAssetEntry != null)
								{
									textureImporter.Import(openFileDialog.FileName, ebxAssetEntry, ref texture);
								}

								BuildTextureViewerFromAssetEntry(resEntry);

								MainEditorWindow.Log($"Imported {openFileDialog.FileName} to {SelectedEntry.Filename}");

							}
						}
					}

					else if (SelectedEntry.Type == "HotspotDataAsset")
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
								//EBXViewer = new Editor(SelectedEntry, ebx, ProjectManagement.Instance.Project, MainEditorWindow);
								EBXViewer.LoadEbx(SelectedEntry, ebx, ProjectManagement.Instance.Project, MainEditorWindow);

							}
						}
					}


					else if (SelectedEntry.Type == "SkinnedMeshAsset")
					{
						OpenFileDialog openFileDialog = new OpenFileDialog();
						openFileDialog.Filter = "Fbx files (*.fbx)|*.fbx";
						openFileDialog.FileName = SelectedEntry.Filename;

						var fbximport_dialogresult = openFileDialog.ShowDialog();
						if (fbximport_dialogresult.HasValue && fbximport_dialogresult.Value)
						{
							var skinnedMeshEbx = await AssetManager.Instance.GetEbxAsync((EbxAssetEntry)SelectedEntry);
							if (skinnedMeshEbx != null)
							{
								var resentry = AssetManager.Instance.GetResEntry(SelectedEntry.Name);
								var res = await AssetManager.Instance.GetResAsync(resentry);
								MeshSet meshSet = new MeshSet(res);

								var skeletonEntryText = "content/character/rig/skeleton/player/skeleton_player";
								MeshSkeletonSelector meshSkeletonSelector = new MeshSkeletonSelector();
								var meshSelectorResult = meshSkeletonSelector.ShowDialog();
								if (meshSelectorResult.HasValue && meshSelectorResult.Value)
								{
									if (!meshSelectorResult.Value)
									{
										MessageBox.Show("Cannot export without a Skeleton");
										return;
									}

									skeletonEntryText = meshSkeletonSelector.AssetEntry.Name;

								}
								else
								{
									MessageBox.Show("Cannot export without a Skeleton");
									return;
								}

								try
								{
									await loadingDialog.UpdateAsync("Importing Mesh", "");
									FrostySdk.Frostbite.IO.Input.FBXImporter importer = new FrostySdk.Frostbite.IO.Input.FBXImporter();
									importer.ImportFBX(openFileDialog.FileName, meshSet, skinnedMeshEbx, (EbxAssetEntry)SelectedEntry
										, new FrostySdk.Frostbite.IO.Input.MeshImportSettings()
										{
											SkeletonAsset = skeletonEntryText
										});
									MainEditorWindow.Log($"Imported {openFileDialog.FileName} to {SelectedEntry.Name}");

									UpdateAssetListView();
									OpenAsset(SelectedEntry);
								}
								catch (Exception ImportException)
								{
									MainEditorWindow.LogError(ImportException.Message);

								}

							}
						}
					}

					else // Raw data import
					{
						OpenFileDialog openFileDialog = new OpenFileDialog();
						var filt = "*.bin";
						openFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
						openFileDialog.FileName = SelectedEntry.Filename;
						var dialogResult = openFileDialog.ShowDialog();
						if (dialogResult.HasValue && dialogResult.Value)
						{
							var binaryData = File.ReadAllBytes(openFileDialog.FileName);
							AssetManager.Instance.ModifyEbxBinary(SelectedEntry.Name, binaryData);
							OpenAsset(SelectedEntry);
						}
					}
				}

			}
			catch(Exception ex)
            {
				MainEditorWindow.LogError(ex.Message);
			}
			UpdateAssetListView();

			if(MainEditorWindow != null)
            {
				MainEditorWindow.UpdateAllBrowsers();
            }

			if(loadingDialog != null && loadingDialog.Visibility == Visibility.Visible)
            {
				loadingDialog.Close();
            }
		}

		private async void btnExport_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedLegacyEntry != null)
			{
				SaveFileDialog saveFileDialog = new SaveFileDialog();
				var filt = "*." + SelectedLegacyEntry.Type;
				if(SelectedLegacyEntry.Type == "DDS")
					saveFileDialog.Filter = "Image files (*.png,*.dds)|*.png;*.dds;";
				else
					saveFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
				
				saveFileDialog.FileName = SelectedLegacyEntry.Filename;

				if (saveFileDialog.ShowDialog().Value)
				{
					var legacyData = ((MemoryStream)ProjectManagement.Instance.Project.AssetManager.GetCustomAsset("legacy", SelectedLegacyEntry)).ToArray();
					if (SelectedLegacyEntry.Type == "DDS" && saveFileDialog.FileName.Contains("PNG", StringComparison.OrdinalIgnoreCase))
					{
						ImageEngineImage originalImage = new ImageEngineImage(legacyData);

						var imageBytes = originalImage.Save(
							new ImageFormats.ImageEngineFormatDetails(
								ImageEngineFormat.PNG)
							, MipHandling.KeepTopOnly
							, removeAlpha: false);

						await File.WriteAllBytesAsync(saveFileDialog.FileName, imageBytes);
					}
					else if (SelectedLegacyEntry.Type == "DDS" && saveFileDialog.FileName.Contains("PNG", StringComparison.OrdinalIgnoreCase))
					{
						//DDSImage2 image2 = new DDSImage2(legacyData.ToArray());
						//File.WriteAllBytes(saveFileDialog.FileName, image2.GetTextureData());

						//await legacyData.CopyToAsync(new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate));
						//await File.WriteAllBytesAsync(saveFileDialog.FileName, legacyData);

						//await File.WriteAllBytesAsync(saveFileDialog.FileName, pOutData.Data);

						ImageEngineImage originalImage = new ImageEngineImage(legacyData);

						await originalImage.Save(saveFileDialog.FileName
							, new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.DDS_DXT5)
							, GenerateMips: MipHandling.KeepExisting
							, removeAlpha: false);
					}
					else
                    {
						await File.WriteAllBytesAsync(saveFileDialog.FileName, legacyData);
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

				else if (SelectedEntry.Type == "HotspotDataAsset")
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

				else if (SelectedEntry.Type == "SkinnedMeshAsset")
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

							var skeletonEntryText = "content/character/rig/skeleton/player/skeleton_player";
							var fifaMasterSkeleton = AssetManager.Instance.EBX.ContainsKey(skeletonEntryText);
							if(!fifaMasterSkeleton)
                            {
								MeshSkeletonSelector meshSkeletonSelector = new MeshSkeletonSelector();
								var meshSelectorResult = meshSkeletonSelector.ShowDialog();
								if(meshSelectorResult.HasValue && meshSelectorResult.Value)
                                {
                                    if (!meshSelectorResult.Value)
                                    {
										MessageBox.Show("Cannot export without a Skeleton");
										return;
                                    }

									skeletonEntryText = meshSkeletonSelector.AssetEntry.Name;

                                }
                                else
                                {
									MessageBox.Show("Cannot export without a Skeleton");
									return;
								}
                            }

							SaveFileDialog saveFileDialog = new SaveFileDialog();
							var filt = "*.fbx";
							saveFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
							saveFileDialog.FileName = SelectedEntry.Filename;
							var dialogAnswer = saveFileDialog.ShowDialog();
							if (dialogAnswer.HasValue && dialogAnswer.Value)
							{
								var exporter = new MeshSetToFbxExport();
								exporter.Export(AssetManager.Instance
									, skinnedMeshEbx.RootObject
									, saveFileDialog.FileName, "FBX_2012", "Meters", true, skeletonEntryText, "*.fbx", meshSet);
								
								
								MainEditorWindow.Log($"Exported {SelectedEntry.Name} to {saveFileDialog.FileName}");


							}
						}
					}
				}

				else
                {
					var ebx = AssetManager.Instance.GetEbxStream((EbxAssetEntry)SelectedEntry);
					if (ebx != null)
					{
						SaveFileDialog saveFileDialog = new SaveFileDialog();
						var filt = "*.bin";
						saveFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
						saveFileDialog.FileName = SelectedEntry.Filename;
						var dialogAnswer = saveFileDialog.ShowDialog();
						if (dialogAnswer.HasValue && dialogAnswer.Value)
						{
							File.WriteAllBytes(saveFileDialog.FileName, ((MemoryStream)ebx).ToArray());
							MainEditorWindow.Log($"Exported {SelectedEntry.Filename} to {saveFileDialog.FileName}");
						}
					}
					else
					{
						MainEditorWindow.Log("Failed to export file");
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
			var filterText = string.Empty;
			bool onlyModified = false;
			Dispatcher.Invoke(() => {
				filterText = txtFilter.Text;
				onlyModified = chkShowOnlyModified.IsChecked.HasValue && chkShowOnlyModified.IsChecked.Value;
			});


			if (SelectedAssetPath != null)
			{
				if (SelectedAssetPath.FullPath.Length > 3)
				{
					var filterPath = (SelectedAssetPath.FullPath.Substring(1, SelectedAssetPath.FullPath.Length - 1));
					var filteredAssets = AllAssetEntries.Where(x => x.Path.ToLower() == filterPath.ToLower());
					filteredAssets = filteredAssets.Where(x => x.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase));

					filteredAssets = filteredAssets.Where(x =>

						(
						onlyModified == true
						&& x.IsModified
						)
						|| onlyModified == false

						).ToList();

					Dispatcher.Invoke(() =>
					{
						var selectedit = assetListView.SelectedItem;
						assetListView.ItemsSource = filteredAssets.OrderBy(x => x.Name);
						assetListView.SelectedItem = selectedit;
					});

				}
			}
		}

        

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

		MainViewModel ModelViewerModel;

		private void OpenAsset(AssetEntry entry)
		{
			try
			{

				if (entry != null) 
				{
					var bundleName = entry.Bundle;


				}
				SelectedEntry = null;
				SelectedLegacyEntry = null;
				btnImport.IsEnabled = false;
				btnExport.IsEnabled = false;
				btnRevert.IsEnabled = false;

				ImageViewerScreen.Visibility = Visibility.Collapsed;
				TextViewer.Visibility = Visibility.Collapsed;
				EBXViewer.Visibility = Visibility.Collapsed;
				BackupEBXViewer.Visibility = Visibility.Collapsed;
				UnknownLegacyFileViewer.Visibility = Visibility.Collapsed;
				ModelDockingManager.Visibility = Visibility.Collapsed;
				BIGViewer.Visibility = Visibility.Collapsed;
				//HEXViewer.Visibility = Visibility.Collapsed;

				//TextBlock control = sender as TextBlock;
				//if (control != null)
				{
					EbxAssetEntry ebxEntry = entry as EbxAssetEntry;
					if (ebxEntry != null)
					{
						SelectedEntry = ebxEntry;
						SelectedEbxAsset = AssetManager.Instance.GetEbx(ebxEntry);
						if (ebxEntry.Type == "TextureAsset")
						{
							try
							{
								MainEditorWindow.Log("Loading Texture " + ebxEntry.Filename);
								var res = AssetManager.Instance.GetResEntry(ebxEntry.Name);
								if (res != null)
								{
									MainEditorWindow.Log("Loading RES " + ebxEntry.Filename);

									BuildTextureViewerFromAssetEntry(res);
								}
								else
								{
									throw new Exception("Unable to find RES Entry for " + ebxEntry.Name);
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

							MainEditorWindow.Log("Loading 3D Model " + ebxEntry.Filename);

							var resentry = AssetManager.Instance.GetResEntry(ebxEntry.Name);
							var res = AssetManager.Instance.GetRes(resentry);
							MeshSet meshSet = new MeshSet(res);

							var exporter = new MeshSetToFbxExport();
							exporter.Export(AssetManager.Instance, SelectedEbxAsset.RootObject, "test_noSkel.obj", "2012", "Meters", true, null, "*.obj", meshSet);
							Thread.Sleep(250);
							
							if (ModelViewerModel != null)
								ModelViewerModel.Dispose();

							ModelViewerModel = new MainViewModel(skinnedMeshAsset: SelectedEbxAsset, meshSet: meshSet);
							this.ModelViewer.DataContext = ModelViewerModel;
							this.ModelDockingManager.Visibility = Visibility.Visible;
							this.ModelViewerEBXGrid.SelectedObject = SelectedEbxAsset.RootObject;

							this.btnExport.IsEnabled = ProfilesLibrary.CanExportMeshes;
							this.btnImport.IsEnabled = ProfilesLibrary.CanImportMeshes;
							this.btnRevert.IsEnabled = SelectedEntry.HasModifiedData;

						}
						else if (string.IsNullOrEmpty(ebxEntry.Type))
                        {
							btnExport.IsEnabled = true;
							btnImport.IsEnabled = true;
							btnRevert.IsEnabled = true;

							unknownFileDocumentsPane.Children.Clear();
							var newLayoutDoc = new LayoutDocument();
							newLayoutDoc.Title = SelectedEntry.DisplayName;
							WpfHexaEditor.HexEditor hexEditor = new WpfHexaEditor.HexEditor();
							hexEditor.Stream = AssetManager.Instance.GetEbxStream(ebxEntry);
							newLayoutDoc.Content = hexEditor;
							hexEditor.BytesModified += HexEditor_BytesModified;
							unknownFileDocumentsPane.Children.Insert(0, newLayoutDoc);
							unknownFileDocumentsPane.SelectedContentIndex = 0;

							UnknownLegacyFileViewer.Visibility = Visibility.Visible;
						}
						else
						{
							if (ebxEntry == null || ebxEntry.Type == "EncryptedAsset")
							{
								return;
							}
							var ebx = ProjectManagement.Instance.Project.AssetManager.GetEbx(ebxEntry);
							if (ebx != null)
							{
								MainEditorWindow.Log("Loading EBX " + ebxEntry.Filename);

								//EBXViewer = new Editor(ebxEntry, ebx, ProjectManagement.Instance.Project, MainEditorWindow);
								var successful = EBXViewer.LoadEbx(ebxEntry, ebx, ProjectManagement.Instance.Project, MainEditorWindow);
								EBXViewer.Visibility = successful.Result ? Visibility.Visible : Visibility.Collapsed;
								BackupEBXViewer.Visibility = !successful.Result ? Visibility.Visible : Visibility.Collapsed;
								BackupEBXViewer.SelectedObject = ebx.RootObject;
								BackupEBXViewer.SelectedPropertyItemChanged += BackupEBXViewer_SelectedPropertyItemChanged;
								//BackupEBXViewer.Asset = ebx;
								//BackupEBXViewer.SetClass(ebx.RootObject);
								//BackupEBXViewer.Recreate();


								btnRevert.IsEnabled = true;
								//if (ebxEntry.Type == "HotspotDataAsset")
								//{
								btnImport.IsEnabled = true;
								btnExport.IsEnabled = true;
								//}
							}
						}
					}
					else
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
							"TXT",
							"CSV",
							"TG", // some custom XML / JS / LUA file that is used in FIFA
							"JLT", // some custom XML / LUA file that is used in FIFA
							"PLS" // some custom XML / LUA file that is used in FIFA
						};

								List<string> imageViewers = new List<string>()
						{
							"PNG",
							"DDS"
						};

							List<string> bigViewers = new List<string>()
						{
							"BIG",
							"AST"
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
										//TextViewer.Text = ASCIIEncoding.ASCII.GetString(nr.ReadToEnd());
										TextViewer.Text = UTF8Encoding.UTF8.GetString(nr.ReadToEnd());
									}
								}
								else if (imageViewers.Contains(legacyFileEntry.Type))
								{
									MainEditorWindow.Log("Loading Legacy File " + SelectedLegacyEntry.Filename);
									btnImport.IsEnabled = true;
									btnExport.IsEnabled = true;
									ImageViewerScreen.Visibility = Visibility.Visible;

									BuildTextureViewerFromStream((MemoryStream)ProjectManagement.Instance.Project.AssetManager.GetCustomAsset("legacy", legacyFileEntry));


								}
							else if (bigViewers.Contains(legacyFileEntry.Type))
                            {
								BIGViewer.Visibility = Visibility.Visible;
								BIGViewer.AssetEntry = legacyFileEntry;
								BIGViewer.ParentBrowser = this;
								switch(legacyFileEntry.Type)
                                {
									//case "BIG":
									//	BIGViewer.LoadBig();
									//	break;
									default:
										BIGViewer.LoadBig();
										break;

								}

								btnImport.IsEnabled = true;
								btnExport.IsEnabled = true;
								btnRevert.IsEnabled = true;
                            }
							else
							{
								MainEditorWindow.Log("Loading Unknown Legacy File " + SelectedLegacyEntry.Filename);
								btnExport.IsEnabled = true;
                                btnImport.IsEnabled = true;
                                btnRevert.IsEnabled = true;

								unknownFileDocumentsPane.Children.Clear();
								var newLayoutDoc = new LayoutDocument();
								newLayoutDoc.Title = SelectedEntry.DisplayName;
								WpfHexaEditor.HexEditor hexEditor = new WpfHexaEditor.HexEditor();
                                using (var nr = new NativeReader(ProjectManagement.Instance.Project.AssetManager.GetCustomAsset("legacy", legacyFileEntry)))
                                {
                                    hexEditor.Stream = new MemoryStream(nr.ReadToEnd());
                                }
                                newLayoutDoc.Content = hexEditor;
								hexEditor.BytesModified += HexEditor_BytesModified;
								unknownFileDocumentsPane.Children.Insert(0, newLayoutDoc);
								unknownFileDocumentsPane.SelectedContentIndex = 0;
							

								UnknownLegacyFileViewer.Visibility = Visibility.Visible;
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

			DataContext = null;
			DataContext = this;
		}

        private void HexEditor_BytesModified(object sender, WpfHexaEditor.Core.EventArguments.ByteEventArgs e)
        {
			var hexEditor = sender as WpfHexaEditor.HexEditor;
			if (hexEditor != null)
			{
				if(this.SelectedLegacyEntry != null)
                {
					AssetManager.Instance.ModifyLegacyAsset(this.SelectedLegacyEntry.Name, hexEditor.GetAllBytes(true), false);
					UpdateAssetListView();
                }
                else
                {

                }
			}
		}

        private void BackupEBXViewer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
			//Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid propertyGrid = sender as Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid;
			//if (propertyGrid != null)
			//{
			//	var ebxAsset = AssetManager.Instance.GetEbx((EbxAssetEntry)SelectedEntry);
			//	ebxAsset.SetRootObject(propertyGrid.SelectedObject);
			//	AssetManager.Instance.ModifyEbx(SelectedEntry.Name, ebxAsset);
			//	UpdateAssetListView();
			//}
		}

        private void BackupEBXViewer_SelectedPropertyItemChanged(object sender, RoutedPropertyChangedEventArgs<Xceed.Wpf.Toolkit.PropertyGrid.PropertyItemBase> e)
        {
            Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid propertyGrid = sender as Xceed.Wpf.Toolkit.PropertyGrid.PropertyGrid;
            if (propertyGrid != null)
            {
                var ebxAsset = AssetManager.Instance.GetEbx((EbxAssetEntry)SelectedEntry);

				bool hasChanged = false;
				var obj1 = ebxAsset.RootObject;
				var obj2 = propertyGrid.SelectedObject;
				var props = obj1.GetProperties();
				foreach (var p in props)
				{
					foreach (var p2 in v2k4Util.GetProperties(obj2))
					{
						if (p.Name == p2.Name)
						{
							if (!p.GetValue(obj1).Equals(p2.GetValue(obj2)))
							{
								hasChanged = true;
								break;
							}
						}

						if (hasChanged)
							break;
					}
				}
				if (hasChanged)
				{
					ebxAsset.SetRootObject(propertyGrid.SelectedObject);
					AssetManager.Instance.ModifyEbx(SelectedEntry.Name, ebxAsset);
					UpdateAssetListView();
				}
            }

        }

		private void BuildTextureViewerFromAssetEntry(ResAssetEntry res)
        {

			using (Texture textureAsset = new Texture(res))
			{
				try
				{
					ImageViewer.Source = null;
					CurrentDDSImageFormat = textureAsset.PixelFormat;


					var bPath = Directory.GetCurrentDirectory() + @"\temp.png";

					TextureExporter textureExporter = new TextureExporter();
					MemoryStream memoryStream = new MemoryStream();

                    Stream expToStream = null;
                    try
					{
                        expToStream = textureExporter.ExportToStream(textureAsset, TextureUtils.ImageFormat.PNG);
						expToStream.Position = 0;
                        //var ddsData = textureExporter.WriteToDDS(textureAsset);
                        //BuildTextureViewerFromStream(new MemoryStream(ddsData));

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

					//using var nr = new NativeReader(expToStream);
					//nr.Position = 0;
					//var textureBytes = nr.ReadToEnd();

					//ImageViewer.Source = LoadImage(textureBytes);
					ImageViewer.Source = LoadImage(((MemoryStream)expToStream).ToArray());
					ImageViewerScreen.Visibility = Visibility.Visible;

                    btnExport.IsEnabled = true;
					btnImport.IsEnabled = true;
					btnRevert.IsEnabled = true;

				}
				catch (Exception e)
				{
					MainEditorWindow.LogError($"Error loading texture with message :: {e.Message}");
					MainEditorWindow.LogError(e.ToString());
					ImageViewer.Source = null; ImageViewerScreen.Visibility = Visibility.Collapsed;
				}
			}
		}

		public string CurrentDDSImageFormat { get; set; }

        //private void BuildTextureViewerFromStream(Stream stream, AssetEntry assetEntry = null)
        private void BuildTextureViewerFromStream(MemoryStream stream)
        {

			try
			{
				ImageViewer.Source = null;

				var bPath = Directory.GetCurrentDirectory() + @"\temp.png";

				ImageEngineImage imageEngineImage = new ImageEngineImage(((MemoryStream)stream).ToArray());
				var iData = imageEngineImage.Save(new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.BMP), MipHandling.KeepTopOnly, removeAlpha: false);

				//var CurrentDDSImage = new DDSImage(stream);
				//stream.Position = 0;
				//var dds2 = new DDSImage2(((MemoryStream)stream).ToArray());
				//FourCC fourCC = dds2.GetPixelFormatFourCC();

				//CurrentDDSImageFormat = fourCC.ToString() + " - " + CurrentDDSImage._image.ToString() + " - " + CurrentDDSImage._image.Format.ToString();
				//var textureBytes = new NativeReader(CurrentDDSImage.SaveToStream()).ReadToEnd();
				////var textureBytes = new NativeReader(textureExporter.ExportToStream(texture)).ReadToEnd();

				//CurrentDDSImageFormat = imageEngineImage.Format.ToString() + " - " + imageEngineImage.FormatDetails.DX10Format;

				//ImageViewer.Source = LoadImage(textureBytes);
				ImageViewer.Source = LoadImage(iData);
				ImageViewerScreen.Visibility = Visibility.Visible;

				btnExport.IsEnabled = true;
				btnImport.IsEnabled = true;
				btnRevert.IsEnabled = true;

			}
			catch (Exception e)
			{
				MainEditorWindow.LogError(e.Message);
				ImageViewer.Source = null; ImageViewerScreen.Visibility = Visibility.Collapsed;
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

        private void btnRevert_Click(object sender, RoutedEventArgs e)
        {
			if (SelectedEntry != null)
			{
				if (EBXViewer != null && EBXViewer.Visibility == Visibility.Visible)
				{
					EBXViewer.RevertAsset();
				}
				else 
				{
					AssetManager.Instance.RevertAsset(SelectedEntry);
				}
			}
			else if (SelectedLegacyEntry != null)
            {
				AssetManager.Instance.RevertAsset(SelectedLegacyEntry);
				if(SelectedLegacyEntry.Type == "DDS")
                {
					//BuildTextureViewerFromStream(AssetManager.Instance.GetCustomAsset("legacy", SelectedLegacyEntry), SelectedLegacyEntry);
					BuildTextureViewerFromStream((MemoryStream)AssetManager.Instance.GetCustomAsset("legacy", SelectedLegacyEntry));
                }
            }

			if(MainEditorWindow != null)
				MainEditorWindow.UpdateAllBrowsers();

			OpenAsset(SelectedEntry);

			UpdateAssetListView();

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
				AssetManager.Instance.ModifyLegacyAsset(SelectedLegacyEntry.Name
							, bytes
							, false);
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
			if (assetTreeViewSelectedItem != null)
			{
				SelectedAssetPath = assetTreeViewSelectedItem;
				UpdateAssetListView();
			}
		}

        private void assetListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			AssetEntry entry = ((ListView)sender).SelectedItem as EbxAssetEntry;
			if (entry == null)
				entry = ((ListView)sender).SelectedItem as LegacyFileEntry;

			if(entry != null)
				OpenAsset(entry);
		}

        private void btnDuplicate_Click(object sender, RoutedEventArgs e)
        {
			FMT.Controls.Windows.DuplicateItem dupWindow = new FMT.Controls.Windows.DuplicateItem();
			dupWindow.EntryToDuplicate = SelectedEntry != null ? SelectedEntry : SelectedLegacyEntry;
			dupWindow.IsLegacy = SelectedLegacyEntry != null;
			var result = dupWindow.ShowDialog();
			if(result.HasValue && result.Value)
            {
				if (MainEditorWindow != null)
					MainEditorWindow.UpdateAllBrowsersFull();
            }
			dupWindow = null;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

		private void btnImportFolder_Click(object sender, RoutedEventArgs e)
		{
			MenuItem parent = sender as MenuItem;
			if (parent != null)
			{
				var assetPath = parent.Tag as AssetPath;

				FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
				folderBrowserDialog.AllowMultiSelect = false;
				folderBrowserDialog.InitialFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
				if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					string folder = folderBrowserDialog.SelectedFolder;
					foreach (string fileName in Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly))
					{
						var importFileInfo = new FileInfo(fileName);
						var importFileInfoSplit = importFileInfo.Name.Split("_");
						if (importFileInfoSplit.Length > 1)
						{
							importFileInfoSplit[1] = importFileInfoSplit[1].Replace(".png", "");
							var resEntryPath = AssetManager.Instance.RES.Keys.FirstOrDefault(
								x => x.StartsWith(assetPath.FullPath.Substring(1))
								&& x.Contains(importFileInfoSplit[0])
								&& x.Contains(importFileInfoSplit[1])
								&& !x.Contains("brand")
								&& !x.Contains("crest")
								);
							var resEntry = AssetManager.Instance.GetResEntry(resEntryPath);
							if (resEntry != null)
							{
								Texture texture = new Texture(resEntry);
								TextureImporter textureImporter = new TextureImporter();
								EbxAssetEntry ebxAssetEntry = AssetManager.Instance.GetEbxEntry(resEntryPath);

								if (ebxAssetEntry != null)
								{
									textureImporter.Import(fileName, ebxAssetEntry, ref texture);
									MainEditorWindow.Log($"Imported {fileName} to {resEntryPath}");
								}


							}
						}
					}
				}
			}
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
