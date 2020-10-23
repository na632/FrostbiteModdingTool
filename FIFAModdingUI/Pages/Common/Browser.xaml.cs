using FrostySdk.Managers;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            set { allAssets = value; CurrentPath = ""; CurrentTier = 0; Update(); }
        }

        public List<string> CurrentAssets { get; set; }

        public int CurrentTier { get; set; }

        public string CurrentPath { get; set; }

        public void SelectOption(string name)
        {

        }

        public void BrowseTo(string path)
        {
            CurrentPath = CurrentPath + path;

            Update();
        }

        public void Update()
        {
            CurrentAssets = AllAssetEntries
                .Where(x =>
                {
                    var split = x.Path.Split('/');
                    if (split.Length > 0)
                    {
                        var path = split[CurrentTier];
                        if(x.Path.Contains(path))
                        {

                        }

                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }).Select(x => {

                    var split = x.Path.Split('/');
                    if (split.Length > 0)
                    {
                        var path = split[CurrentTier];
                        if (x.Path.Contains(path))
                        {
                            return path;
                        }

                        return path;
                    }
                    else
                    {
                        return x.Name;
                    }


                }).ToList();

            /*
             * <Button Height="30" Background="Transparent" BorderBrush="{x:Null}" VerticalAlignment="Center">
                        <StackPanel Height="30" VerticalAlignment="Center"  HorizontalAlignment="Right" Orientation="Horizontal">
                            <MaterialDesign:PackIcon Kind="Person" VerticalAlignment="Center" />
                            <Label Content="Default" VerticalAlignment="Center" />
                        </StackPanel>
                    </Button>
             * 
             */
            foreach (var item in CurrentAssets)
            {
                Button button = new Button();
                button.Height = 30;
                button.VerticalAlignment = VerticalAlignment.Center;

                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.Height = 30;

                PackIcon packIcon = new PackIcon();
                packIcon.Kind = PackIconKind.Folder;
                stackPanel.Children.Add(packIcon);

                Label label = new Label();
                label.Content = item;

                button.Content = stackPanel;

                spParent.Children.Add(button);


            }




        }
    }
}
