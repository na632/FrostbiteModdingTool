using FrostySdk;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
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
using v2k4FIFAModding;

namespace FIFAModdingUI.Pages.Common
{
	/// <summary>
	/// Interaction logic for Editor.xaml
	/// </summary>
	public partial class Editor : UserControl
	{

		public static readonly DependencyProperty AssetEntryProperty;

		public static readonly DependencyProperty AssetModifiedProperty;

		protected ILogger logger;

		protected List<object> objects;

		protected List<object> rootObjects;

		protected Dictionary<Guid, EbxAsset> dependentObjects = new Dictionary<Guid, EbxAsset>();

		protected EbxAsset asset;

		public object RootObject { get { return asset.RootObject; } }

		private List<Tuple<string, string, object>> _rootObjProps;
		/// <summary>
		/// Item 1: PropertyName
		/// Item 2: PropertyType
		/// Item 3: PropertyValue
		/// </summary>
		public List<Tuple<string,string,object>> RootObjectProperties
        {
            get
            {
				List<Tuple<string, string, object>> items = new List<Tuple<string, string, object>>();
				foreach (var p in RootObject.GetType().GetProperties())
                {
					items.Add(new Tuple<string, string, object>(p.Name, p.PropertyType.ToString(), p.GetValue(RootObject, null)));
                }
				return items;
			}
			set
            {
				//var originalAssetEntry = FrostyProject.AssetManager.EnumerateEbx().FirstOrDefault(x => x.Name == AssetEntry.Name);
				FrostyProject.AssetManager.RevertAsset(AssetEntry);

				var copyOfRoot = RootObject;
				_rootObjProps = value;
				foreach(var item in _rootObjProps.Where(x=> !x.Item1.StartsWith("__")))
                {
					v2k4Util.SetPropertyValue(copyOfRoot, item.Item1, item.Item3);
                }
				asset.AddObject(copyOfRoot);
				asset.AddObject(copyOfRoot, true);
				FrostyProject.AssetManager.ModifyEbx(AssetEntry.Name, asset);
			}
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

        public FrostyProject FrostyProject { get; }

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

        public object MyChangingProperty { get; internal set; }

        protected event RoutedEventHandler onAssetModified;

		public event RoutedEventHandler OnAssetModified
		{
			add
			{
				onAssetModified += value;
			}
			remove
			{
				onAssetModified -= value;
			}
		}

		private static void OnAssetModifiedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue)
			{
				Editor frostyAssetEditor = o as Editor;
				frostyAssetEditor.asset.Update();
			
				//App.AssetManager.ModifyEbx(frostyAssetEditor.AssetEntry.Name, frostyAssetEditor.asset);
				//frostyAssetEditor.InvokeOnAssetModified();
				//frostyAssetEditor.AssetModified = false;
			}
		}

		public Editor()
        {
            InitializeComponent();
			this.DataContext = Asset;
		}

		public Editor(AssetEntry inAssetEntry, EbxAsset inAsset, FrostyProject frostyProject)
		{
			InitializeComponent();
			// intialise objs
			AssetEntry = inAssetEntry;
			Asset = inAsset;
			FrostyProject = frostyProject;

            this.DataContext = this;
			this.TreeView1.Items.Clear();
			foreach (var p in RootObjectProperties.OrderBy(x=>x.Item1))
			{
				TreeViewItem propTreeViewParent = new TreeViewItem();
				propTreeViewParent.Header = p.Item1;
				TreeView1.Items.Add(propTreeViewParent);

				switch (p.Item2)
				{
					case "FrostySdk.Ebx.PointerRef":
						if (p.Item3.PropertyExists("Internal"))
						{
							var FloatCurve = p.Item3.GetPropertyValue("Internal");
							if (FloatCurve != null)
							{

								// Guid
								var spGuid = new StackPanel() { Orientation = Orientation.Horizontal };
								var lblGuid = new TextBlock() { Text = "__Guid" };
								spGuid.Children.Add(lblGuid);
								var txtGuid = new TextBlock() { Name = p.Item1 + "___Guid", Text = FloatCurve.__InstanceGuid.ToString() };
								spGuid.Children.Add(txtGuid);
								propTreeViewParent.Items.Add(spGuid);

								// Min X
								var spMinX = new StackPanel() { Orientation = Orientation.Horizontal };
								var lblMinX = new TextBlock() { Text = "MinX" };
								spMinX.Children.Add(lblMinX);
								var txtMinX = new TextBox() { Name = "MinX", Text = FloatCurve.MinX.ToString() };
								spMinX.Children.Add(txtMinX);
								propTreeViewParent.Items.Add(spMinX);

								// Max X 
								var spMaxX = new StackPanel() { Orientation = Orientation.Horizontal };
								var lblMaxX = new TextBlock() { Text = "MaxX" };
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
										var txtPointX = new TextBox() { Name = p.Item1 + "_Points_" + i.ToString() + "_X", Text = FloatCurve.Points[i].X.ToString() };
										txtPointX.TextChanged += (object sender, TextChangedEventArgs e) =>
										{
											AssetHasChanged(sender as TextBox, p.Item1);
										};
										SubChild1ItemX.Items.Add(txtPointX);
										Child1Item.Items.Add(SubChild1ItemX);

										TreeViewItem SubChild1ItemY = new TreeViewItem();
										SubChild1ItemY.Header = "Y";
										var txtPointY = new TextBox() { Name = p.Item1 + "_Points_" + i.ToString() + "_Y", Text = FloatCurve.Points[i].Y.ToString() };
										txtPointY.TextChanged += (object sender, TextChangedEventArgs e) =>
										{
											AssetHasChanged(sender as TextBox, p.Item1);
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
						TreeViewItem SysSingleTreeView = new TreeViewItem();
						SysSingleTreeView.Header = "Value";

						TextBox txt = new TextBox();
						txt.Name = p.Item1;
						txt.Text = p.Item3.ToString();
						txt.TextChanged += (object sender, TextChangedEventArgs e) => {
							AssetHasChanged(sender as TextBox, p.Item1);
						};

						SysSingleTreeView.Items.Add(txt);


						propTreeViewParent.Items.Add(SysSingleTreeView);

						break;

					case "System.Collections.Generic.List`1[System.Single]":
						TreeViewItem lstSingleTreeViewParent = new TreeViewItem();
						lstSingleTreeViewParent.Header = "Values";

						var listSingle = p.Item3 as List<System.Single>;
						for (var i = 0; i < listSingle.Count; i++)
						{
							var point = listSingle[i];
							TreeViewItem Child1Item = new TreeViewItem();
							Child1Item.Header = "[" + i.ToString() + "]";

							TreeViewItem SubChild1ItemX = new TreeViewItem();
							SubChild1ItemX.Header = "Value";
							var txtPointX = new TextBox() { Name = p.Item1 + "_Points_" + i.ToString() + "_Value", Text = listSingle[i].ToString() };
							txtPointX.TextChanged += (object sender, TextChangedEventArgs e) => {
								AssetHasChanged(sender as TextBox, p.Item1);
							};
							SubChild1ItemX.Items.Add(txtPointX);
							Child1Item.Items.Add(SubChild1ItemX);

							lstSingleTreeViewParent.Items.Add(Child1Item);
						}

						propTreeViewParent.Items.Add(lstSingleTreeViewParent);

						break;
					case "FrostySdk.Ebx.CString":
                        TreeViewItem SysCStringTreeView = new TreeViewItem();

						SysCStringTreeView.Header = "Value";

						TextBox txtCS = new TextBox();
						txtCS.Text = p.Item3.ToString();
						SysCStringTreeView.Items.Add(txtCS);


						propTreeViewParent.Items.Add(SysCStringTreeView);

						break;


				}
			}
		}

		public void AssetHasChanged(TextBox sender, string propName)
		{
			if (string.IsNullOrEmpty(sender.Text))
				return;

			string txtboxName = sender.Name;
			if (!string.IsNullOrEmpty(txtboxName))
			{
				var rootProp = RootObjectProperties.Find(x => x.Item1 == propName);
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
							if (splitPropName[3] == "X" || splitPropName[3] == "Y")
							{
								var index = int.Parse(splitPropName[2]);

								var FloatCurve = rootProp.Item3.GetPropertyValue("Internal");
								var fcPoint = FloatCurve.Points[index];

								if (splitPropName[3] == "X")
								{
									fcPoint.X = float.Parse(sender.Text);
									v2k4Util.SetPropertyValue(FloatCurve.Points[index], "X", fcPoint.X);
								}
								else
								{
									fcPoint.Y = float.Parse(sender.Text);
								}

							}
						}
						else if (splitPropName.Length > 2)// && splitPropName[3] == "VALUE")
						{

						}
					}
					else if (propName.StartsWith("ATTR_"))
					{
						var splitPropName = txtboxName.Split('_');
						if (splitPropName.Length == 2)
						{
							var replacementitem3 = rootProp.Item3;
							replacementitem3 = System.Single.Parse(sender.Text);
							var newlist = RootObjectProperties.Where(x => x.Item1 != rootProp.Item1).ToList();
							newlist.Add(new Tuple<string, string, object>(rootProp.Item1, rootProp.Item2, replacementitem3));
							RootObjectProperties = newlist;
						}
					}
					else
					{
						var replacementitem3 = rootProp.Item3;
						replacementitem3 = System.Single.Parse(sender.Text);
						var newlist = RootObjectProperties.Where(x => x.Item1 != rootProp.Item1).ToList();
						newlist.Add(new Tuple<string, string, object>(rootProp.Item1, rootProp.Item2, replacementitem3));
						RootObjectProperties = newlist;
					}

				}
            }

			SaveToRootObject();
		}

        private void SaveToRootObject()
        {
			FrostyProject.AssetManager.ModifyEbx(AssetEntry.Name, Asset);
			FrostyProject.Save("GameplayProject.fbproject", true);
		}

		public void AssetHasChanged(TextBox sender, TreeViewItem treetopmostparent, TreeViewItem treeofpropertychanged)
        {

		}

    }

	public class MyDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate TextTemplate { get; set; }
		public DataTemplate ImageTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return base.SelectTemplate(item, container);
        }
  //      protected override DataTemplate SelectTemplateCore(object item)
		//{
		//	//if (item is TextItem)
		//	//	return TextTemplate;
		//	//if (item is ImageItem)
		//	//	return ImageTemplate;

		//	return base.SelectTemplateCore(item);
		//}

		//protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
		//{
		//	return SelectTemplateCore(item);
		//}
	}

	public class FileItemTemplateSelector : DataTemplateSelector
	{


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
			var castedTuple = item as Tuple<string, string, object>;
			if (castedTuple != null) {
				if (castedTuple.Item3.GetType().ToString() == "FrostySdk.Ebx.PointerRef")
                {
					return PointerRefItemTemplate;
				}
				switch(castedTuple.Item3.GetType().ToString())
                {
					case "System.Single":
						return FloatItemTemplate;
                }
			}

            return base.SelectTemplate(item, container);
        }
        //protected override DataTemplate SelectTemplate(object item, DependencyObject container)
        //{
        //	var file = item as FileObject;
        //	switch (file.MimeType)
        //	{
        //		case "image/png":
        //		case "image/jpg":
        //			return ImageFileItemTemplate;
        //		default:
        //			return GeneralFileItemTemplate;
        //	}
        //}

        public DataTemplate GeneralFileItemTemplate { get; set; }
		public DataTemplate ImageFileItemTemplate { get; set; }
		public DataTemplate FloatItemTemplate { get; set; }
		public DataTemplate PointerRefItemTemplate { get; set; }
	}

	public class PointerRefTemplateSelector : DataTemplateSelector
    {

		public DataTemplate GeneralFileItemTemplate { get; set; }
		public DataTemplate ImageFileItemTemplate { get; set; }
		public DataTemplate FloatItemTemplate { get; set; }
		public DataTemplate PointerRefItemTemplate { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return base.SelectTemplate(item, container);
        }
    }
}
