using FrostbiteModdingUI.Pages.Common.EBX;
using FrostySdk;
using FrostySdk.FrostySdk.Ebx;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using v2k4FIFAModding;
using Xceed.Wpf.AvalonDock.Controls;

namespace FIFAModdingUI.Pages.Common
{
	/// <summary>
	/// Interaction logic for Editor.xaml
	/// </summary>
	public partial class Editor : UserControl
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
        }



		public static readonly DependencyProperty AssetEntryProperty;

		public static readonly DependencyProperty AssetModifiedProperty;

		protected ILogger logger;

		protected List<object> objects;

		protected List<object> rootObjects;

		protected Dictionary<Guid, EbxAsset> dependentObjects = new Dictionary<Guid, EbxAsset>();

		protected EbxAsset asset;

		public object RootObject { get { return asset.RootObject; } }

		private List<ModdableProperty> _rootObjProps;
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
					return _rootObjProps.OrderBy(x=>x.PropertyName == "BaseField").ThenBy(x => x.PropertyName).ToList();
				}
				return _rootObjProps;
			}
			set
            {
				//var originalAssetEntry = FrostyProject.AssetManager.EnumerateEbx().FirstOrDefault(x => x.Name == AssetEntry.Name);
				//FrostyProject.AssetManager.RevertAsset(AssetEntry);

				_rootObjProps = value;
				foreach(var item in _rootObjProps.Where(x=> !x.PropertyName.StartsWith("__")))
                {
					v2k4Util.SetPropertyValue(RootObject, item.PropertyName, item.PropertyValue);

                }
				FrostyProject.AssetManager.ModifyEbx(AssetEntry.Name, asset);
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
		}

		public void RevertAsset() {
			FrostyProject.AssetManager.RevertAsset(AssetEntry);
			this.Visibility = Visibility.Collapsed;
		}



		//public object InteralObject {
		//          get
		//          {
		//		return RootObject.GetType().GetProperties().GetValue(RootObject, null);
		//	}
		//	set
		//          {

		//          }

		//	}

		public IEnumerable<object> RootObjects => asset.RootObjects;

		public IEnumerable<object> Objects => asset.Objects;

		//public AssetEntry AssetEntry
		//{
		//	get
		//	{
		//		return (AssetEntry)GetValue(AssetEntryProperty);
		//	}
		//	private set
		//	{
		//		SetValue(AssetEntryProperty, value);
		//	}
		//}
		public AssetEntry AssetEntry { get; set; }

		public EbxAsset Asset { get { return asset; } set { asset = value; } }

        public FrostbiteProject FrostyProject { get; }

        public bool AssetModified
		{
			get
			{
				return (bool)GetValue(AssetModifiedProperty);
			}
			set
			{
				SetValue(AssetModifiedProperty, value);
			}
		}

		public Editor()
        {
            InitializeComponent();
			this.DataContext = Asset;
		}

		public Editor(AssetEntry inAssetEntry, EbxAsset inAsset, FrostbiteProject frostyProject)
		{
			InitializeComponent();
			// intialise objs
			AssetEntry = inAssetEntry;
			Asset = inAsset;
			FrostyProject = frostyProject;

			if (FrostyModWriter.EbxResource.ListOfEBXRawFilesToUse.Contains(AssetEntry.Filename))
				chkImportFromFiles.IsChecked = true;

			this.DataContext = this;
			this.TreeView1.Items.Clear();
			foreach (var p in RootObjectProperties.OrderBy(x=>x.PropertyName))
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
						TreeView1.Items.Add(control);
						continue;
					}
				}


				
				{
					// if unable to use prebuilt controls, use old system
					switch (p.PropertyType)
					{
						case "FrostySdk.Ebx.PointerRef":
							if (p.PropertyValue.PropertyExists("Internal"))
							{
								var FloatCurve = p.PropertyValue.GetPropertyValue("Internal");
								//if (FloatCurve != null && v2k4Util.HasProperty(FloatCurve, "MinX"))
								if (FloatCurve != null)
								{

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
									for (var i = 0; i < FloatCurve.Points.Count; i++)
									{
										var point = FloatCurve.Points[i];
										if (point != null)
										{
											TreeViewItem Child1Item = new TreeViewItem();
											Child1Item.Header = "[" + i.ToString() + "]";

											TreeViewItem SubChild1ItemX = new TreeViewItem();
											SubChild1ItemX.Header = "X";
											var txtPointX = new TextBox() { Name = p.PropertyName + "_Points_" + i.ToString() + "_X", Text = FloatCurve.Points[i].X.ToString() };
											txtPointX.PreviewLostKeyboardFocus += (object sender, KeyboardFocusChangedEventArgs e) =>
											{

												AssetHasChanged(sender as TextBox, p.PropertyName);
											};
											SubChild1ItemX.Items.Add(txtPointX);
											Child1Item.Items.Add(SubChild1ItemX);

											TreeViewItem SubChild1ItemY = new TreeViewItem();
											SubChild1ItemY.Header = "Y";
											var txtPointY = new TextBox() { Name = p.PropertyName + "_Points_" + i.ToString() + "_Y", Text = FloatCurve.Points[i].Y.ToString() };
											txtPointY.PreviewLostKeyboardFocus += (object sender, KeyboardFocusChangedEventArgs e) =>
											{
												AssetHasChanged(sender as TextBox, p.PropertyName);
											};
											SubChild1ItemY.Items.Add(txtPointY);

											Child1Item.Items.Add(SubChild1ItemY);

											PointsTreeViewParent.Items.Add(Child1Item);
										}
									}
								}



							}
							break;
						case "System.Single":
						case "System.Int":
						case "System.Int32":
						case "System.Int64":
							TreeViewItem SysSingleTreeView = new TreeViewItem();
							SysSingleTreeView.Header = "Value";
							SysSingleTreeView.IsExpanded = true;
							TextBox txt = new TextBox();
							txt.Name = p.PropertyName;
							txt.Text = p.PropertyValue.ToString();
							//txt.SetBindi = "{Binding RootObject." + p.PropertyName + "}";// p.PropertyValue.ToString();
							//txt.TextChanged += (object sender, TextChangedEventArgs e) => {
							//	AssetHasChanged(sender as TextBox, p.PropertyName);
							//};

							txt.PreviewLostKeyboardFocus += (object sender, KeyboardFocusChangedEventArgs e) =>
							{
								AssetHasChanged(sender as TextBox, p.PropertyName);
							};

							SysSingleTreeView.Items.Add(txt);
							propTreeViewParent.IsExpanded = true;


							propTreeViewParent.Items.Add(SysSingleTreeView);

							break;

						case "System.Collections.Generic.List`1[System.Single]":
							TreeViewItem lstSingleTreeViewParent = new TreeViewItem();
							lstSingleTreeViewParent.Header = "Values";

							var listSingle = p.PropertyValue as List<System.Single>;
							for (var i = 0; i < listSingle.Count; i++)
							{
								var point = listSingle[i];
								TreeViewItem Child1Item = new TreeViewItem();
								Child1Item.Header = "[" + i.ToString() + "]";

								TreeViewItem SubChild1ItemX = new TreeViewItem();
								SubChild1ItemX.Header = "Value";
								var txtPointX = new TextBox() { Name = p.PropertyName + "_Points_" + i.ToString() + "_Value", Text = listSingle[i].ToString() };
								txtPointX.TextChanged += (object sender, TextChangedEventArgs e) =>
								{
									AssetHasChanged(sender as TextBox, p.PropertyName);
								};
								SubChild1ItemX.Items.Add(txtPointX);
								Child1Item.Items.Add(SubChild1ItemX);

								lstSingleTreeViewParent.Items.Add(Child1Item);
							}

							propTreeViewParent.Items.Add(lstSingleTreeViewParent);

							break;
						case "FrostySdk.Ebx.CString":
							TreeViewItem SysCStringTreeView = new TreeViewItem();
							SysCStringTreeView.IsExpanded = true;

							SysCStringTreeView.Header = "Value";

							TextBox txtCS = new TextBox();
							txtCS.Text = p.PropertyValue.ToString();
							SysCStringTreeView.Items.Add(txtCS);


							propTreeViewParent.Items.Add(SysCStringTreeView);
							propTreeViewParent.IsExpanded = true;


							break;
						case "FrostySdk.Ebx.AssetClassGuid":
							TextBox txtGuid2 = new TextBox();
							txtGuid2.Text = p.PropertyValue.ToString();
							txtGuid2.IsEnabled = false;
							propTreeViewParent.Items.Add(txtGuid2);
							propTreeViewParent.IsExpanded = true;

							break;

						case "System.Collections.Generic.List`1[FrostySdk.Ebx.HotspotEntry]":
							var d = (dynamic)p.PropertyValue;

							TreeViewItem lstHSDynamicTreeViewParent = new TreeViewItem();
							lstHSDynamicTreeViewParent.Header = "Values";
							for (var i = 0; i < d.Count; i++)
							{
								Grid gridBounds = new Grid();
								RowDefinition gridRow1 = new RowDefinition();
								RowDefinition gridRow2 = new RowDefinition();
								gridBounds.RowDefinitions.Add(gridRow1);
								gridBounds.RowDefinitions.Add(gridRow2);

								ColumnDefinition gridCol1 = new ColumnDefinition();
								ColumnDefinition gridCol2 = new ColumnDefinition();
								ColumnDefinition gridCol3 = new ColumnDefinition();
								gridBounds.ColumnDefinitions.Add(gridCol1);
								gridBounds.ColumnDefinitions.Add(gridCol2);
								gridBounds.ColumnDefinitions.Add(gridCol3);

								Label hsboundx = new Label();
								hsboundx.Content = "X";
								Grid.SetRow(hsboundx, 0);
								Grid.SetColumn(hsboundx, 0);

								Label lblhsboundy = new Label();
								lblhsboundy.Content = "Y";
								Grid.SetRow(lblhsboundy, 0);
								Grid.SetColumn(lblhsboundy, 1);

								Label lblhsboundz = new Label();
								lblhsboundz.Content = "Z";
								Grid.SetRow(lblhsboundz, 0);
								Grid.SetColumn(lblhsboundz, 2);

								TextBox txthsboundx = new TextBox();
								txthsboundx.Text = d[i].Bounds.x.ToString();
								Grid.SetRow(txthsboundx, 1);
								Grid.SetColumn(txthsboundx, 0);

								TextBox txthsboundy = new TextBox();
								txthsboundy.Text = d[i].Bounds.y.ToString();
								Grid.SetRow(txthsboundy, 1);
								Grid.SetColumn(txthsboundy, 1);

								TextBox txthsboundz = new TextBox();
								txthsboundz.Text = d[i].Bounds.z.ToString();
								Grid.SetRow(txthsboundz, 1);
								Grid.SetColumn(txthsboundz, 2);

								gridBounds.Children.Add(hsboundx);
								gridBounds.Children.Add(lblhsboundy);
								gridBounds.Children.Add(lblhsboundz);

								gridBounds.Children.Add(txthsboundx);
								gridBounds.Children.Add(txthsboundy);
								gridBounds.Children.Add(txthsboundz);

								lstHSDynamicTreeViewParent.Items.Add(gridBounds);

							}
							propTreeViewParent.Items.Add(lstHSDynamicTreeViewParent);

							break;


					}
				}
				if(AddToPropTreeViewParent)
					TreeView1.Items.Add(propTreeViewParent);

			}
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
						if (!txtboxName.StartsWith("_") && !txtboxName.StartsWith("ATTR_") && txtboxName.Contains("_"))
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
            catch
            {
            }
		}

        private void SaveToRootObject()
        {
			FrostyProject.AssetManager.ModifyEbx(AssetEntry.Name, Asset);
			//FrostyProject.Save("GameplayProject.fbproject", true);
		}

		public void AssetHasChanged(TextBox sender, TreeViewItem treetopmostparent, TreeViewItem treeofpropertychanged)
        {

		}

        private void chkImportFromFiles_Checked(object sender, RoutedEventArgs e)
        {
			var listoffiles = FrostyModWriter.EbxResource.ListOfEBXRawFilesToUse;

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

			FrostyModWriter.EbxResource.ListOfEBXRawFilesToUse = listoffiles;
		}
    }

   
}
