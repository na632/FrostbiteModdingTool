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
    public partial class BrowserOfLTU : UserControl
    {
        public BrowserOfLTU()
        {
            InitializeComponent();
			Load();
        }

        public void Load()
		{

			RefreshList();
		}

		private void RefreshList()
        {
            ltuBrowser.AllAssetEntries = FileSystem.Instance.LiveTuningUpdate.LiveTuningUpdateEntries.Select(x=>x.Value);
        }

	}
}
