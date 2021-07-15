using FrostbiteModdingUI.Pages.Common.EBX;
using FrostbiteModdingUI.Windows;
using FrostySdk;
using FrostySdk.FrostySdk.Ebx;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using v2k4FIFAModding;

namespace FIFAModdingUI.Pages.Common
{
	/// <summary>
	/// Interaction logic for Editor.xaml
	/// </summary>
	public partial class Editor : UserControl, INotifyPropertyChanged
	{
		public class ModdableProperty : INotifyPropertyChanged
        {
			public string PropertyName { get; set; }
			public string PropertyType { get; set; }

            private object propValue;

            public object PropertyValue
            {
                get { return propValue; }
                set 
				{
					if (propValue != value)
					{
						propValue = value;
						if (PropertyChanged != null)
							PropertyChanged.Invoke(this, new PropertyChangedEventArgs(PropertyName));
					}
				}
            }


            public ModdableProperty(string n, string t, object v)
            {
				PropertyName = n;
				PropertyType = t;
				propValue = v;
			}
			public ModdableProperty(string n, string t, object v, PropertyChangedEventHandler modpropchanged)
			{
				PropertyChanged += modpropchanged;
				PropertyName = n;
				PropertyType = t;
				propValue = v;
			}

			public event PropertyChangedEventHandler PropertyChanged;

            public override string ToString()
            {
				if(!string.IsNullOrEmpty(PropertyName))
                {
					return string.Format("{0} - {1}", PropertyName, PropertyType);
                }

                return base.ToString();
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }



		public static readonly DependencyProperty AssetEntryProperty;

		public static readonly DependencyProperty AssetModifiedProperty;

		protected IEditorWindow EditorWindow;

		protected List<object> objects;

		protected List<object> rootObjects;

		protected Dictionary<Guid, EbxAsset> dependentObjects = new Dictionary<Guid, EbxAsset>();

		protected EbxAsset asset;

		public object RootObject { get { return asset.RootObject; } }

		private List<ModdableProperty> _rootObjProps;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Item 1: PropertyName
        /// Item 2: PropertyType
        /// Item 3: PropertyValue
        /// </summary>
        public List<ModdableProperty> RootObjectProperties
        {
            get
            {
				if (_rootObjProps == null)
				{
					_rootObjProps = new List<ModdableProperty>();

					foreach (var p in RootObject.GetType().GetProperties())
					{
						var modprop = new ModdableProperty(p.Name, p.PropertyType.ToString(), p.GetValue(RootObject, null), Modprop_PropertyChanged);
						_rootObjProps.Add(modprop);
					}
					return _rootObjProps.OrderBy(x => x.PropertyName == "BaseField").ThenBy(x => x.PropertyName).ToList();
				}
				return _rootObjProps;
			}
			set
            {
				_rootObjProps = value;
				foreach(var item in _rootObjProps.Where(x=> !x.PropertyName.StartsWith("__")))
                {
					v2k4Util.SetPropertyValue(RootObject, item.PropertyName, item.PropertyValue);
                }
				FrostyProject.AssetManager.ModifyEbx(AssetEntry.Name, asset);
			}
        }

		private List<ModdableProperty> _VanillaRootProps;

		public List<ModdableProperty> VanillaRootObjectProperties
		{
			get
			{
				if (_VanillaRootProps == null)
				{
					_VanillaRootProps = new List<ModdableProperty>();

					var vanillaEbx = AssetManager.Instance.GetEbx((EbxAssetEntry)AssetEntry, false);
					if (vanillaEbx != null)
					{
						foreach (var p in vanillaEbx.RootObject.GetType().GetProperties())
						{
							var modprop = new ModdableProperty(p.Name, p.PropertyType.ToString(), p.GetValue(vanillaEbx.RootObject, null), Modprop_PropertyChanged);
							_VanillaRootProps.Add(modprop);
						}
						return _VanillaRootProps.OrderBy(x => x.PropertyName == "BaseField").ThenBy(x => x.PropertyName).ToList();
					}
				}
				return _VanillaRootProps;
			}
		}

		private void Modprop_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
			var index = _rootObjProps.FindIndex(x => x.PropertyName == ((ModdableProperty)sender).PropertyName);
			var o = _rootObjProps[index];
			o.PropertyValue = ((ModdableProperty)sender).PropertyValue;
			var robjs = RootObjectProperties;
			robjs[index] = o;
			RootObjectProperties = robjs;

			if(EditorWindow != null)
				EditorWindow.UpdateAllBrowsers();
		}

		public void RevertAsset() {
			FrostyProject.AssetManager.RevertAsset(AssetEntry);
			this.Visibility = Visibility.Collapsed;
		}

		public IEnumerable<object> RootObjects => asset.RootObjects;

		public IEnumerable<object> Objects => asset.Objects;

		public AssetEntry AssetEntry { get; set; }

		//public EbxAsset Asset { get { return asset; } set { asset = value; if(PropertyChanged != null) PropertyChanged.Invoke(this, null); } }
		public EbxAsset Asset { get { return asset; } set { asset = value; } }
        public FrostbiteProject FrostyProject { get; protected set; }

		[Obsolete("Incorrect usage of Editor Windows")]

		public Editor()
		{
            InitializeComponent();
			this.DataContext = Asset;
		}

		//[Deprecated("This is only used for Testing Purposes", DeprecationType.Deprecate, 1)]
		public Editor(EbxAsset ebx
			)
		{
			InitializeComponent();
			PropertyChanged += Editor_PropertyChanged;
			// intialise objs
			Asset = ebx;
			this.DataContext = ebx;
			this.TreeView1.DataContext = RootObject;
			this.TreeViewOriginal.DataContext = RootObject;
		}

		public static Editor CurrentEditorInstance { get; set; }

		//public Editor(AssetEntry inAssetEntry
		//	, EbxAsset inAsset
		//	, FrostbiteProject frostyProject
		//	, IEditorWindow inEditorWindow)
		//{
		//	InitializeComponent();
		//	CurrentEditorInstance = this;
		//	PropertyChanged += Editor_PropertyChanged;
		//	LoadEbx(inAssetEntry, inAsset, frostyProject, inEditorWindow);
			
		//}

		public async Task<bool> LoadEbx(AssetEntry inAssetEntry
			, EbxAsset inAsset
			, FrostbiteProject frostyProject
			, IEditorWindow inEditorWindow)
        {
			CurrentEditorInstance = this;
			//vanillaAnchorable.Hide();
			vanillaAnchorable.ToggleAutoHide();

			//LoadingDialog loadingDialog = new LoadingDialog();
			//loadingDialog.Show();
			//await loadingDialog.UpdateAsync("Loading EBX", "Loading EBX");

			//await Task.Delay(1);

			_VanillaRootProps = null;
			_rootObjProps = null;
			PropertyChanged += Editor_PropertyChanged;
			// intialise objs
			AssetEntry = inAssetEntry;
			Asset = inAsset;
			FrostyProject = frostyProject;
			EditorWindow = inEditorWindow;

			bool success = true;
			//await loadingDialog.UpdateAsync("Loading EBX", "Building Tree Views");
			
				Dispatcher.Invoke(() =>
				{
					this.DataContext = this;

					success = CreateEditor(RootObjectProperties, TreeView1).Result;
					success = CreateEditor(VanillaRootObjectProperties, TreeViewOriginal).Result;
				});

			//loadingDialog.Close();
			//loadingDialog = null;
			return success;
		}

        private void Editor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public async Task<bool> CreateEditor(object d, TreeView treeView)
		{
			var propertyNSearch = ((Type)d.GetType()).ToString().ToLower().Replace(".", "_", StringComparison.OrdinalIgnoreCase);

			var allTypes = Assembly.GetExecutingAssembly().GetTypes();
			var ty = allTypes.FirstOrDefault(x => x.Name.Contains(propertyNSearch, StringComparison.OrdinalIgnoreCase));
            if (ty == null && d is ModdableProperty)
            {
				var mp = d as ModdableProperty;

				if (IsList(mp.PropertyValue))
				{
					var lst = mp.PropertyValue as IList;
					foreach (var item in lst)
					{
						await CreateEditor(item, treeView);
					}
				}
				else
                {
					await CreateEditor(d, treeView);
				}
			}
            if (ty != null)
			{
				Control control = Activator.CreateInstance(ty) as Control;
				if (control != null)
				{
					control.DataContext = d;
					treeView.Items.Add(control);
					return true;
				}
			}
		
			return false;
		}

		public bool IsList(object o)
		{
			return o is IList &&
			   o.GetType().IsGenericType &&
			   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
		}

		public bool IsDictionary(object o)
		{
			return o is IDictionary &&
			   o.GetType().IsGenericType &&
			   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<object, object>));
		}

		public async Task<bool> CreateEditor(List<ModdableProperty> moddableProperties, TreeView treeView)
        {
			bool success = true;

			treeView.Items.Clear();
			foreach (var p in moddableProperties)
			{
				TreeViewItem propTreeViewParent = new TreeViewItem();
				propTreeViewParent.Header = p.PropertyName;

				bool AddToPropTreeViewParent = true;

                // Attempt to use the prebuilt controls
                var propertyNSearch = p.PropertyType.Replace(".", "_").ToLower();
                var ty = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => x.Name.ToLower().Contains(propertyNSearch));
                if (ty != null)
                {
                    Control control = Activator.CreateInstance(ty) as Control;
                    if (control != null)
                    {
                        control.DataContext = p;
                        AddToPropTreeViewParent = false;
						treeView.Items.Add(control);
                        continue;
                    }
                }

                //if (!CreateEditor(p.PropertyValue))
				{
					// if unable to use prebuilt controls, use old system
					switch (p.PropertyType)
					{
						case "FrostySdk.Ebx.PointerRef":
							if (v2k4Util.HasProperty(p.PropertyValue, "Internal"))
							{
								var Internal = p.PropertyValue.GetPropertyValue("Internal");
								if (Internal != null && v2k4Util.HasProperty(Internal, "Points"))
								{
									// Must be Float Curve if it has Points
									var FloatCurve = Internal;
									// Guid
									var spGuid = new StackPanel() { Orientation = Orientation.Horizontal };
									var lblGuid = new Label() { Content = "__Guid" };
									spGuid.Children.Add(lblGuid);
									var txtGuid = new Label() { Name = p.PropertyName + "___Guid", Content = FloatCurve.__InstanceGuid.ToString() };
									spGuid.Children.Add(txtGuid);
									propTreeViewParent.Items.Add(spGuid);

									
										// Min X
										var spMinX = new StackPanel() { Orientation = Orientation.Horizontal };
										var lblMinX = new Label() { Content = "MinX" };
										spMinX.Children.Add(lblMinX);
										var txtMinX = new TextBox() { Name = "MinX", Text = FloatCurve.MinX.ToString() };
										spMinX.Children.Add(txtMinX);
										propTreeViewParent.Items.Add(spMinX);

										// Max X 
										var spMaxX = new StackPanel() { Orientation = Orientation.Horizontal };
										var lblMaxX = new Label() { Content = "MaxX" };
										spMaxX.Children.Add(lblMaxX);
										var txtMaxX = new TextBox() { Name = "MaxX", Text = FloatCurve.MaxX.ToString() };
										spMaxX.Children.Add(txtMaxX);
										propTreeViewParent.Items.Add(spMaxX);

										TreeViewItem PointsTreeViewParent = new TreeViewItem();
										PointsTreeViewParent.Name = "Points";
										PointsTreeViewParent.Header = "Points";
										propTreeViewParent.Items.Add(PointsTreeViewParent);

										// Number of Points
										var txtNumberOfPoints = new TextBox() { Name = p.PropertyName + "_NumberOfPoints", Text = FloatCurve.Points.Count.ToString() };
										txtNumberOfPoints.PreviewLostKeyboardFocus += (object sender, KeyboardFocusChangedEventArgs e) =>
										{
											AssetHasChanged(sender as TextBox, p.PropertyName);
										};
										PointsTreeViewParent.Items.Add(txtNumberOfPoints);

									var btnNumberOfPointsSub = new Button() { Name = "btn_" + p.PropertyName + "_NumberOfPoints_Sub", Content = "-" };
										PointsTreeViewParent.Items.Add(btnNumberOfPointsSub);
									var btnNumberOfPointsAdd = new Button() { Name = "btn_" + p.PropertyName + "_NumberOfPoints_Add", Content = "+" };
									PointsTreeViewParent.Items.Add(btnNumberOfPointsAdd);

									for (var i = 0; i < FloatCurve.Points.Count; i++)
										{
											var point = FloatCurve.Points[i];
											if (point != null)
											{
												TreeViewItem Child1Item = new TreeViewItem();
												Child1Item.Header = "[" + i.ToString() + "]";
												Child1Item.IsExpanded = true;

												var txtPointX = new TextBox() { Name = p.PropertyName + "_Points_" + i.ToString() + "_X"
													, Text = FloatCurve.Points[i].X.ToString() };
												txtPointX.PreviewLostKeyboardFocus += (object sender, KeyboardFocusChangedEventArgs e) =>
												{
													AssetHasChanged(sender as TextBox, p.PropertyName);
												};
												Child1Item.Items.Add(txtPointX);

												var txtPointY = new TextBox() { Name = p.PropertyName + "_Points_" + i.ToString() + "_Y", Text = FloatCurve.Points[i].Y.ToString() };
												//txtPointY.PreviewLostKeyboardFocus += (object sender, KeyboardFocusChangedEventArgs e) =>
												//{
												txtPointY.LostFocus += (object sender, RoutedEventArgs e) =>
												{
													AssetHasChanged(sender as TextBox, p.PropertyName);
												};
												Child1Item.Items.Add(txtPointY);

												PointsTreeViewParent.Items.Add(Child1Item);
											}
										}
								}
								else
                                {

                                }


							}
							else
                            {

                            }
							break;
						case "System.Collections.Generic.List`1[System.Single]":

							var listSingle = p.PropertyValue as List<System.Single>;

							for (var i = 0; i < listSingle.Count; i++)
							{
								var point = listSingle[i];

								var txtPointX = new TextBox() { Name = p.PropertyName + "_Points_" + i.ToString() + "_Value", Text = listSingle[i].ToString() };
								txtPointX.TextChanged += (object sender, TextChangedEventArgs e) =>
								{
									AssetHasChanged(sender as TextBox, p.PropertyName);
								};
								propTreeViewParent.Items.Add(txtPointX);
							}


							break;
						case "System.Collections.Generic.List`1[System.Boolean]":

							var listBool = p.PropertyValue as List<System.Boolean>;

							for (var i = 0; i < listBool.Count; i++)
							{
								var point = listBool[i];

								var chk = new CheckBox() { Name = p.PropertyName + "_Points_" + i.ToString() + "_Value", IsChecked = listBool[i] };
                                chk.Checked += (object sender, RoutedEventArgs e) =>
								{
								};
								propTreeViewParent.Items.Add(chk);
							}


							break;

						case "System.Collections.Generic.List`1[FrostySdk.Ebx.HotspotEntry]":
							var d = (dynamic)p.PropertyValue;
							CreateEditor(p, treeView);

                            TreeViewItem lstHSDynamicTreeViewParent = new TreeViewItem();
                            lstHSDynamicTreeViewParent.Header = "Values";
                            for (var i = 0; i < d.Count; i++)
                            {
                                Grid gridBounds = new Grid();
                                RowDefinition gridRow1 = new RowDefinition();
                                RowDefinition gridRow2 = new RowDefinition();
                                RowDefinition gridRow3 = new RowDefinition();
                                RowDefinition gridRow4 = new RowDefinition();
                                RowDefinition gridRow5 = new RowDefinition();
								gridBounds.RowDefinitions.Add(gridRow1);
                                gridBounds.RowDefinitions.Add(gridRow2);
                                gridBounds.RowDefinitions.Add(gridRow3);
                                gridBounds.RowDefinitions.Add(gridRow4);
                                gridBounds.RowDefinitions.Add(gridRow5);

								ColumnDefinition gridCol1 = new ColumnDefinition();
                                ColumnDefinition gridCol2 = new ColumnDefinition();
                                ColumnDefinition gridCol3 = new ColumnDefinition();
                                ColumnDefinition gridCol4 = new ColumnDefinition();
                                gridBounds.ColumnDefinitions.Add(gridCol1);
                                gridBounds.ColumnDefinitions.Add(gridCol2);
                                gridBounds.ColumnDefinitions.Add(gridCol3);
                                gridBounds.ColumnDefinitions.Add(gridCol4);

								TextBlock hsTextGroup = new TextBlock();
								hsTextGroup.Text = d[i].Name + " (" + d[i].Group.ToString() + ")";
								Grid.SetRow(hsTextGroup, 0);
								gridBounds.Children.Add(hsTextGroup);

								TextBlock hsboundx = new TextBlock();
                                hsboundx.Text = "X";
								
								Grid.SetRow(hsboundx, 1);
                                Grid.SetColumn(hsboundx, 0);

								TextBlock lblhsboundy = new TextBlock();
                                lblhsboundy.Text = "Y";
                                Grid.SetRow(lblhsboundy, 1);
                                Grid.SetColumn(lblhsboundy, 1);

								TextBlock lblhsboundz = new TextBlock();
                                lblhsboundz.Text = "Z";
                                Grid.SetRow(lblhsboundz, 1);
                                Grid.SetColumn(lblhsboundz, 2);

								TextBlock lblhsboundw = new TextBlock();
								lblhsboundw.Text = "W";
								Grid.SetRow(lblhsboundw, 1);
								Grid.SetColumn(lblhsboundw, 3);

								TextBox txthsboundx = new TextBox();
								txthsboundx.Name = p.PropertyName + "_Bounds_X";
                                txthsboundx.Text = d[i].Bounds.x.ToString();
								txthsboundx.TextChanged += (object sender, TextChangedEventArgs e) =>
								{
									AssetHasChanged(sender as TextBox, p.PropertyName);
								};
								Grid.SetRow(txthsboundx, 2);
                                Grid.SetColumn(txthsboundx, 0);

                                TextBox txthsboundy = new TextBox();
								txthsboundy.Name = p.PropertyName + "_Bounds_Y";
								txthsboundy.Text = d[i].Bounds.y.ToString();
								txthsboundy.TextChanged += (object sender, TextChangedEventArgs e) =>
								{
									AssetHasChanged(sender as TextBox, p.PropertyName);
								};
								Grid.SetRow(txthsboundy, 2);
                                Grid.SetColumn(txthsboundy, 1);

                                TextBox txthsboundz = new TextBox();
								txthsboundz.Name = p.PropertyName + "_Bounds_Z";
                                txthsboundz.Text = d[i].Bounds.z.ToString();
								//txthsboundz.SetBinding(AssetEntryProperty, p.PropertyName);
								txthsboundz.TextChanged += (object sender, TextChangedEventArgs e) =>
								{
									AssetHasChanged(sender as TextBox, p.PropertyName);
								};
								Grid.SetRow(txthsboundz, 2);
                                Grid.SetColumn(txthsboundz, 2);

								TextBox txthsboundw = new TextBox();
								txthsboundw.Text = d[i].Bounds.w.ToString();
								txthsboundw.TextChanged += (object sender, TextChangedEventArgs e) =>
								{
									AssetHasChanged(sender as TextBox, p.PropertyName);
								};
								Grid.SetRow(txthsboundw, 2);
								Grid.SetColumn(txthsboundw, 3);

								gridBounds.Children.Add(hsboundx);
                                gridBounds.Children.Add(lblhsboundy);
                                gridBounds.Children.Add(lblhsboundz);

                                gridBounds.Children.Add(txthsboundx);
                                gridBounds.Children.Add(txthsboundy);
                                gridBounds.Children.Add(txthsboundz);

								TextBlock hsRotationLabel = new TextBlock();
								hsRotationLabel.Text = "Rotation";
								Grid.SetRow(hsRotationLabel, 3);
								gridBounds.Children.Add(hsRotationLabel);

								TextBox hsRotationText = new TextBox();
								hsRotationText.Text = d[i].Rotation.ToString();
								hsRotationText.TextChanged += (object sender, TextChangedEventArgs e) =>
								{
									AssetHasChanged(sender as TextBox, p.PropertyName);
								};
								Grid.SetRow(hsRotationText, 3);
								Grid.SetColumn(hsRotationText, 1);
								gridBounds.Children.Add(hsRotationText);

								lstHSDynamicTreeViewParent.Items.Add(gridBounds);

                            }
                            propTreeViewParent.Items.Add(lstHSDynamicTreeViewParent);

                            break;

						default:
							//EditorWindow.LogError($"Unhandled EBX Item {p.PropertyName} of type {p.PropertyType}");
							success = false;

							break;


					}
				}
				if (AddToPropTreeViewParent)
					treeView.Items.Add(propTreeViewParent);

			}
			return success;
		}

        private void Chk_Checked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void AssetHasChanged(TextBox sender, string propName)
		{
			if (string.IsNullOrEmpty(sender.Text))
				return;

			try
			{
				string txtboxName = sender.Name;
				if (!string.IsNullOrEmpty(txtboxName))
				{
					var rootProp = RootObjectProperties.Find(x => x.PropertyName == propName);
					if (rootProp != null)
					{
						if (txtboxName.EndsWith("_NumberOfPoints"))
                        {
                            if (v2k4Util.HasProperty(rootProp.PropertyValue, "Internal"))
                            {
								var floatCurve = rootProp.PropertyValue.GetPropertyValue("Internal") as Object;
								if (v2k4Util.HasProperty(floatCurve, "Points"))
								{
									var fcp = floatCurve.GetPropertyValue("Points");
									var pntsType = fcp.GetType();

									//var fpc = TypeLibrary.CreateObject("FrostySdk.Ebx.FloatCurvePoint");

									//var dataType = new Type[] { typeof(fpc) };
									//var genericBase = typeof(List<>);
									//var combinedType = genericBase.MakeGenericType(dataType);
									//var listStringInstance = Activator.CreateInstance(combinedType);
									//var addMethod = listStringInstance.GetType().GetMethod("Add");
									//addMethod.Invoke(genericInstance, new object[] { "Hello World" });

									var Points = ((IEnumerable<object>)floatCurve.GetPropertyValue("Points")).ToList();
									if (Points != null) 
									{
										if (int.TryParse(sender.Text, out int numberOfNewPoints))
										{
											var lastPoint = Points.Last();

											if (numberOfNewPoints == 0)
											{
												Points.Clear();
												Points.Add(lastPoint);
											}
											else
                                            {
												if(numberOfNewPoints < Points.Count)
                                                {
													Points = Points.Take(numberOfNewPoints).ToList();
                                                }
												else
                                                {
													for(var iPoint = Points.Count; iPoint < numberOfNewPoints; iPoint++)
                                                    {
														Points.Add(lastPoint);
                                                    }
                                                }
                                            }
										}
										v2k4Util.SetPropertyValue(floatCurve, "Points", Points.AsEnumerable());

										SaveToRootObject();
									}
								}
                            }
                        }
						//else if (!txtboxName.StartsWith("_") && !txtboxName.StartsWith("ATTR_") && txtboxName.Contains("_"))
						else if (!txtboxName.StartsWith("_") && txtboxName.Contains("_"))
						{
							// format should be 
							// PROPNAME _ ITEM
							// PROPNAME _ POINTS _ INDEX _ (X OR Y)
							// PROPNAME _ POINTS _ INDEX _ (VALUE)
							var splitPropName = txtboxName.Split('_');
							if (splitPropName.Length > 3 && !string.IsNullOrEmpty(sender.Text))
							{
								if (splitPropName[splitPropName.Length - 1] == "X" || splitPropName[splitPropName.Length - 1] == "Y")
								{
									var index = int.Parse(splitPropName[splitPropName.Length - 2]);

									var FloatCurve = rootProp.PropertyValue.GetPropertyValue("Internal");
									var fcPoint = FloatCurve.Points[index];

									if (splitPropName[splitPropName.Length - 1] == "X")
									{
										fcPoint.X = float.Parse(sender.Text);
										v2k4Util.SetPropertyValue(FloatCurve.Points[index], "X", fcPoint.X);
									}
									else
									{
										fcPoint.Y = float.Parse(sender.Text);
										v2k4Util.SetPropertyValue(FloatCurve.Points[index], "Y", fcPoint.Y);
									}

									SaveToRootObject();


								}
								else if(int.TryParse(splitPropName[splitPropName.Length - 2], out int index))
								{
									var List = rootProp.PropertyValue as List<Single>;
									List[index] = System.Single.Parse(sender.Text);
									var newlist = RootObjectProperties.Where(x => x.PropertyName != rootProp.PropertyName).ToList();
									newlist.Add(new ModdableProperty(rootProp.PropertyName, rootProp.PropertyType, List));
									RootObjectProperties = newlist;
								}
								else
                                {
									var replacementitem3 = rootProp.PropertyValue;
									replacementitem3 = System.Single.Parse(sender.Text);
									var newlist = RootObjectProperties.Where(x => x.PropertyName != rootProp.PropertyName).ToList();
									newlist.Add(new ModdableProperty(rootProp.PropertyName, rootProp.PropertyType, replacementitem3));
									RootObjectProperties = newlist;
								}
							}
							else if (splitPropName.Length > 1)// && splitPropName[3] == "VALUE")
							{
								var replacementitem3 = rootProp.PropertyValue;
								if(rootProp.PropertyType == "System.Int32")
									replacementitem3 = System.Int32.Parse(sender.Text);
								else
									replacementitem3 = System.Single.Parse(sender.Text);

								var newlist = RootObjectProperties.Where(x => x.PropertyName != rootProp.PropertyName).ToList();
								newlist.Add(new ModdableProperty(rootProp.PropertyName, rootProp.PropertyType, replacementitem3));
								RootObjectProperties = newlist;
							}
						}
						else if (propName.StartsWith("ATTR_"))
						{
							var splitPropName = txtboxName.Split('_');
							if (splitPropName.Length == 2)
							{
								var replacementitem3 = rootProp.PropertyValue;
								replacementitem3 = System.Single.Parse(sender.Text);
								var newlist = RootObjectProperties.Where(x => x.PropertyName != rootProp.PropertyName).ToList();
								newlist.Add(new ModdableProperty(rootProp.PropertyName, rootProp.PropertyType, replacementitem3));
								RootObjectProperties = newlist;
							}
						}
						else
						{
							var replacementitem3 = rootProp.PropertyValue;
							replacementitem3 = System.Single.Parse(sender.Text);
							var newlist = RootObjectProperties.Where(x => x.PropertyName != rootProp.PropertyName).ToList();
							newlist.Add(new ModdableProperty(rootProp.PropertyName, rootProp.PropertyType, replacementitem3));
							RootObjectProperties = newlist;
						}

					}
				}

			}
            catch(Exception e)
            {
				Debug.WriteLine(e.ToString());
            }

			if (EditorWindow != null)
				EditorWindow.UpdateAllBrowsers();
		}

        public async void SaveToRootObject()
        {
			LoadingDialog loadingDialog = null;
			await Dispatcher.InvokeAsync(() =>
			{
				loadingDialog = new LoadingDialog("Saving hotspot", "Saving hotspot");
				loadingDialog.Show();
			});

			await Task.Run(() =>
			{
				FrostyProject.AssetManager.ModifyEbx(AssetEntry.Name, Asset);
				//FrostyProject.Save("GameplayProject.fbproject", true);
			});

			await loadingDialog.UpdateAsync("Saving hotspot", "Updating browsers");

            await Dispatcher.InvokeAsync(() =>
            {
                if (EditorWindow != null)
                    EditorWindow.UpdateAllBrowsers();
            });

            await Dispatcher.InvokeAsync(() =>
			{
				loadingDialog.Close();
				loadingDialog = null;
			});
		}


        private void chkImportFromFiles_Checked(object sender, RoutedEventArgs e)
        {
			var listoffiles = FrostbiteModWriter.EbxResource.ListOfEBXRawFilesToUse;

			if (((CheckBox)sender).IsChecked.Value)
			{
				if (!listoffiles.Contains(AssetEntry.Filename))
					listoffiles.Add(AssetEntry.Filename);
			}
			else
			{
				if (listoffiles.Contains(AssetEntry.Filename))
					listoffiles.RemoveAll(x => x.ToLower() == AssetEntry.Filename.ToLower());
			}

			FrostbiteModWriter.EbxResource.ListOfEBXRawFilesToUse = listoffiles;
		}
    }

   
}
