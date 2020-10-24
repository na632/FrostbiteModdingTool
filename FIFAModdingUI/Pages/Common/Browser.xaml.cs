using FrostySdk.Managers;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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


        private List<EbxAssetEntry> allAssets;

        public List<EbxAssetEntry> AllAssetEntries
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
				assetPathMapping.Add("/", new AssetPath("![root]", "", null, bInRoot: true));
			}
			assetPath.Children.Insert(0, assetPathMapping["/"]);

			Dispatcher.BeginInvoke((Action)(() =>
			{
				assetTreeView.ItemsSource = assetPath.Children;
				assetTreeView.Items.SortDescriptions.Add(new SortDescription("PathName", ListSortDirection.Ascending));
			}));





		}

		private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if(button != null)
            {
                var file = AllAssetEntries.FirstOrDefault(x => x.Filename == button.Tag.ToString());
                if (file != null)
                {

                }
                else
                {
                    if (!string.IsNullOrEmpty(CurrentPath))
                    {
                        BrowseTo(CurrentPath + "/" + button.Tag.ToString());
                    }
                    else
                    {
                        BrowseTo(button.Tag.ToString());
                    }
                }
            }
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
