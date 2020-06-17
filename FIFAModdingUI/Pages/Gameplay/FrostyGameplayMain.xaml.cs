using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using v2k4FIFAModding;
using FrostySdk.FrostySdk.Ebx;
using FIFAModdingUI.Pages.Common;

namespace FIFAModdingUI.Pages.Gameplay
{
    /// <summary>
    /// Interaction logic for FrostyGameplayMain.xaml
    /// </summary>
    public partial class FrostyGameplayMain : Page
    {
        
        ProjectManagement GameplayProjectManagement;
        public FrostyGameplayMain()
        {
            InitializeComponent();

            new TaskFactory().StartNew(async() => {

                GameplayProjectManagement = new ProjectManagement();
                GameplayProjectManagement.Logger = App.MainEditorWindow;
                await GameplayProjectManagement.StartNewProject();

                var ebxItems = GameplayProjectManagement.FrostyProject.AssetManager.EnumerateEbx();
                if (ebxItems != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        string lastPath = null;
                        TreeViewItem treeItem = null;
                        // only looking for gameplay that is not "smallsided/volta"
                        foreach (var i in ebxItems.Where(x => x.Filename.StartsWith("gp_") && !x.Path.Contains("smallsided")).OrderBy(x=>x.Path).ThenBy(x=>x.Filename))
                        {
                            Debug.WriteLine(i.Filename);
                            bool usePreviousTree = string.IsNullOrEmpty(lastPath) || lastPath == i.Path;

                            // use previous tree
                            if (!usePreviousTree || treeItem == null)
                            {
                                treeItem = new TreeViewItem();
                                FrostyGameplayAdvancedView.FrostyTreeView.Items.Add(treeItem);
                            }
                            treeItem.Header = i.Path;
                            lastPath = i.Path;
                            var innerTreeItem = new TreeViewItem() { Header = i.Filename };

                            innerTreeItem.MouseDoubleClick += (object sender, MouseButtonEventArgs e) =>
                            {
                                OpenAsset(i);
                            };

                            treeItem.Items.Add(innerTreeItem);

                        }
                    });
                    //foreach (var i in ebxItems.Where(x => x.Filename.StartsWith("gp_")))
                    //{
                    //    Debug.WriteLine(i.Filename);
                    //}
                }

            });
        }

        public async void OpenAsset(AssetEntry asset, bool createDefaultEditor = true)
        {
            //Debug.WriteLine(asset.AssetType);
            if (asset == null || asset.Type == "EncryptedAsset")
            {
                return;
            }
            FrostyGameplayAdvancedView.spPropertyPanel.Children.Clear();
            var ae = GameplayProjectManagement.FrostyProject.AssetManager.GetEbx(asset as EbxAssetEntry);
            if(ae != null)
            {
                FrostyGameplayAdvancedView.spPropertyPanel.Children.Add(new Editor(ae));

                //var aeObjects = ae.RootObject.
                //var propsOfEbx = ae.RootObject.GetType().GetProperties();
                //foreach(var p in propsOfEbx)
                //{
                //    StackPanel spProp = new StackPanel();
                //    spProp.Orientation = Orientation.Horizontal;

                //    object propValue = p.GetValue(ae.RootObject, null);
                //    if(propValue != null)
                //    {
                //        var listProp = propValue as IEnumerable<float>;
                //        if(listProp != null)
                //        {
                //            spProp.Orientation = Orientation.Vertical;
                //            Label label = new Label();
                //            label.Content = p.Name;
                //            spProp.Children.Add(label);

                //            var index = 0;
                //            foreach (var item in listProp)
                //            {
                //                StackPanel spSideBySideProp = new StackPanel();
                //                spSideBySideProp.Orientation = Orientation.Horizontal;

                //                Label tbPropName = new Label();
                //                tbPropName.Name = "tb" + p.Name + "_" + index;
                //                tbPropName.Content = p.Name + "[" + index + "]";
                //                spSideBySideProp.Children.Add(tbPropName);

                //                TextBox txtPropValue = new TextBox();
                //                txtPropValue.Name = p.Name + "_" + index;
                //                txtPropValue.Text = item.ToString();
                //                spSideBySideProp.Children.Add(txtPropValue);

                //                spProp.Children.Add(spSideBySideProp);

                //                index++;
                //            }
                //        }

                //        if (propValue is FrostySdk.Ebx.PointerRef)
                //        {
                //            //GameplayProjectManagement.FrostyProject.FileSystem.
                //            var pntRefProp = (FrostySdk.Ebx.PointerRef)propValue;
                //            if (pntRefProp.Internal != null)
                //            {
                //                spProp.Children.Add(new Editor(ae));

                //                //var fce = pntRefProp.Internal.GetObjectAsType<FloatCurve>();
                //                //if(fce != null)
                //                //{
                //                //    fce.Points[fce.Points.Count - 1].Y = 1;

                //                //    var ucFloatCurve = new FloatCurveControl();
                //                //    ucFloatCurve.DataContext = fce;
                //                //    spProp.Children.Add(ucFloatCurve);

                //                //}
                //                //    //var fce = pntRefProp.Internal.GetObjectAsType<FrostySdk.Ebx.FloatCurveEntity>();
                //                //    //if(fce != null)
                //                //    //{

                //                //    //}
                //                //    //if(pntRefProp.Internal.PropertyExists("Points"))
                //                //    //{
                //                //    //    var pointProp = pntRefProp.Internal.GetProperty("Points");
                //                //    //    if(pointProp != null)
                //                //    //    {
                //                //    //        //pointProp.
                //                //    //    }
                //                //    //}
                //                //}
                //                //if (pntRefProp.Internal is FrostySdk.Ebx.)
                //                //{

                //                //}
                //            }
                //        }
                //    }

                //    //var typeofprop = p.PropertyType.ToString().ToLower();
                //    //var val = p.GetValue(p);
                //    //if(typeofprop.Contains("list"))
                //    //{
                //    //}
                //    //else
                //    //{
                //    //    TextBlock tbPropName = new TextBlock();
                //    //    tbPropName.Text = p.Name;
                //    //    spProp.Children.Add(tbPropName);
                //    //}
                //    //switch (typeofprop)
                //    //{
                //    //    case "Array":
                //    //        //Array arrayProp = p as Array<object>;
                //    //        break;
                //    //    default:
                //    //        TextBlock tbPropName = new TextBlock();
                //    //        tbPropName.Text = p.Name;
                //    //        spProp.Children.Add(tbPropName);
                //    //        break;

                //    //}

                //    FrostyGameplayAdvancedView.spPropertyPanel.Children.Add(spProp);

            //}
            }
        }

        private void WebView1_Loaded(object sender, RoutedEventArgs e)
        {
            new TaskFactory().StartNew(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    //WebView1.Source = new Uri("https://localhost:3000");
                    //WebView1.Navigate(new Uri("https://localhost:3000"));
                    //WebView1.BringIntoView();
                });
            });
        }

        private void WebView1_FrameNavigationStarting(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationStartingEventArgs e)
        {

        }

        private void WebView1_FrameContentLoading(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlContentLoadingEventArgs e)
        {

        }

        private void WebView1_FrameNavigationCompleted(object sender, Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT.WebViewControlNavigationCompletedEventArgs e)
        {

        }
    }
}
