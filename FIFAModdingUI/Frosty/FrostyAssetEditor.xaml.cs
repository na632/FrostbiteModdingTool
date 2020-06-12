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
using FrostyEditor.Windows;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FIFAModdingUI.Frosty
{
    /// <summary>
    /// Interaction logic for FrostyAssetEditor.xaml
    /// </summary>
    public partial class FrostyAssetEditor : UserControl
    {
        public FrostyAssetEditor()
        {
            InitializeComponent();
        }
 
		public static readonly DependencyProperty AssetEntryProperty;

		public static readonly DependencyProperty AssetModifiedProperty;

		protected ILogger logger;

		protected List<object> objects;

		protected List<object> rootObjects;

		protected Dictionary<Guid, EbxAsset> dependentObjects = new Dictionary<Guid, EbxAsset>();

		protected EbxAsset asset;

		public object RootObject => asset.RootObject;

		public IEnumerable<object> RootObjects => asset.RootObjects;

		public IEnumerable<object> Objects => asset.Objects;

		public AssetEntry AssetEntry
		{
			get
			{
				return (AssetEntry)GetValue(AssetEntryProperty);
			}
			private set
			{
				SetValue(AssetEntryProperty, value);
			}
		}

		public EbxAsset Asset => asset;

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

		//private static void OnAssetModifiedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
		//{
		//	if ((bool)e.NewValue)
		//	{
		//		FrostyAssetEditor frostyAssetEditor = o as FrostyAssetEditor;
		//		frostyAssetEditor.asset.Update();
		//		AssetManager.ModifyEbx(frostyAssetEditor.AssetEntry.Name, frostyAssetEditor.asset);
		//		frostyAssetEditor.InvokeOnAssetModified();
		//		frostyAssetEditor.AssetModified = false;
		//	}
		//}

		static FrostyAssetEditor()
		{
			AssetEntryProperty = DependencyProperty.Register("AssetEntry", typeof(AssetEntry), typeof(FrostyAssetEditor), new FrameworkPropertyMetadata(null));
			//AssetModifiedProperty = DependencyProperty.Register("AssetModified", typeof(bool), typeof(FrostyAssetEditor), new UIPropertyMetadata(false, OnAssetModifiedChanged));
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FrostyAssetEditor), new FrameworkPropertyMetadata(typeof(FrostyAssetEditor)));
		}

		private AssetManager AssetManager;

		public FrostyAssetEditor(AssetManager assetManager, ILogger inLogger)
		{
			logger = inLogger;
			AssetManager = assetManager;
		}

		public void AddDependentObject(Guid guid)
		{
			if (asset.AddDependency(guid) && !dependentObjects.ContainsKey(guid))
			{
				dependentObjects.Add(guid, AssetManager.GetEbx(AssetManager.GetEbxEntry(guid)));
			}
		}

		public EbxAsset RefreshDependentObject(Guid guid)
		{
			if (!dependentObjects.ContainsKey(guid))
			{
				return null;
			}
			dependentObjects[guid] = AssetManager.GetEbx(AssetManager.GetEbxEntry(guid));
			return dependentObjects[guid];
		}

		public EbxAsset GetDependentObject(Guid guid)
		{
			if (!dependentObjects.ContainsKey(guid))
			{
				return null;
			}
			return dependentObjects[guid];
		}

		public void AddObject(object obj)
		{
			asset.AddObject(obj);
		}

		public void RemoveObject(object obj)
		{
			asset.RemoveObject(obj);
		}

		public async Task<int> SetAsset(AssetEntry entry)
		{
			if (entry is EbxAssetEntry)
			{
				await Task.Run(delegate
				{
					asset = AssetManager.GetEbx(entry as EbxAssetEntry);
					int num = asset.Dependencies.Count();
					int num2 = 0;
					foreach (Guid dependency in asset.Dependencies)
					{
						EbxAssetEntry ebxEntry = AssetManager.GetEbxEntry(dependency);
						//FrostyTask.Update("Loading dependencies", (double)num2++ / (double)num * 100.0);
						//if (ebxEntry != null)
						//{
						//	dependentObjects.Add(dependency, AssetManager.GetEbx(ebxEntry));
						//}
					}
				});
			}
			AssetEntry = entry;
			return 0;
		}

		protected virtual void InvokeOnAssetModified()
		{
			this.onAssetModified?.Invoke(this, new RoutedEventArgs());
		}

		public virtual void Closed()
		{
		}

		//public virtual List<ToolbarItem> RegisterToolbarItems()
		//{
		//	return new List<ToolbarItem>
		//	{
		//		new ToolbarItem("View Instances", "View class instances", null, new RelayCommand(ViewInstances_Click, ViewInstances_CanClick))
		//	};
		//}

		//private void ViewInstances_Click(object state)
		//{
		//	FrostyPropertyGrid frostyPropertyGrid = GetTemplateChild("PART_AssetPropertyGrid") as FrostyPropertyGrid;
		//	if (frostyPropertyGrid != null)
		//	{
		//		AssetInstancesWindow assetInstancesWindow = new AssetInstancesWindow(asset.RootObjects, frostyPropertyGrid.SelectedClass, Asset, (EbxAssetEntry)AssetEntry);
		//		bool flag = assetInstancesWindow.ShowDialog() == true;
		//		foreach (dynamic newlyCreatedObject in assetInstancesWindow.NewlyCreatedObjects)
		//		{
		//			newlyCreatedObject.Name = AssetEntry.Name;
		//			asset.AddRootObject(newlyCreatedObject);
		//		}
		//		foreach (object deletedObject in assetInstancesWindow.DeletedObjects)
		//		{
		//			if (deletedObject == frostyPropertyGrid.Object)
		//			{
		//				frostyPropertyGrid.Object = RootObject;
		//			}
		//			asset.RemoveObject(deletedObject);
		//		}
		//		if (assetInstancesWindow.Modified)
		//		{
		//			AssetModified = true;
		//		}
		//		if (flag)
		//		{
		//			frostyPropertyGrid.SetClass(assetInstancesWindow.SelectedItem);
		//		}
		//	}
		//}

		//private bool ViewInstances_CanClick(object state)
		//{
		//	return true;
		//}
	}
}

