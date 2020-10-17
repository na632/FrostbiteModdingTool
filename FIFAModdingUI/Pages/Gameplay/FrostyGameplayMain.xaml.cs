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
using v2k4FIFAModding.Frostbite.EbxTypes;
using System.IO;
using System.ComponentModel;

namespace FIFAModdingUI.Pages.Gameplay
{
    /// <summary>
    /// Interaction logic for FrostyGameplayMain.xaml
    /// </summary>
    public partial class FrostyGameplayMain : Page
    {
        public static ProjectManagement GameplayProjectManagement { get; set; }

        class DC
        {
            AttribSchema_gp_actor_movement Movement = AttribSchema_gp_actor_movement.GetAttrib();
        };

        DC dc = new DC();

        public FrostyGameplayMain()
        {
            InitializeComponent();
            //tabsGameplayMain.IsEnabled = false;
            this.DataContext = dc;

            BackgroundWorker backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (o, bw_e) =>
            {

                GameplayProjectManagement = new ProjectManagement();
                //GameplayProjectManagement.Logger = App.fif;

                GameplayProjectManagement.StartNewProjectAsync().Wait();

                var ebxItems = GameplayProjectManagement.FrostyProject.AssetManager.EnumerateEbx().Where(x => x.Filename.StartsWith("gp_") && !x.Path.Contains("smallsided")).OrderBy(x => x.Path).ThenBy(x => x.Filename).ToList();
                if (ebxItems != null)
                {
                    
                        string lastPath = null;
                        TreeViewItem treeItem = null;
                        // only looking for gameplay that is not "smallsided/volta"
                        foreach (var i in ebxItems)
                        {
                            bool usePreviousTree = string.IsNullOrEmpty(lastPath) || lastPath == i.Path;

                            // use previous tree
                            if (!usePreviousTree || treeItem == null)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    treeItem = new TreeViewItem();

                                    FrostyGameplayAdvancedView.FrostyTreeView.Items.Add(treeItem);
                                });
                            }
                            Dispatcher.Invoke(() =>
                            {
                                treeItem.Header = i.Path;
                                lastPath = i.Path;
                            
                                var innerTreeItem = new TreeViewItem() { Header = i.Filename };

                                innerTreeItem.MouseDoubleClick += (object sender, MouseButtonEventArgs e) =>
                                {
                                    OpenAsset(i);
                                };

                                treeItem.Items.Add(innerTreeItem);
                            });

                    }
                }
                    

            };

            backgroundWorker.RunWorkerAsync();
        }

        public void OpenAsset(AssetEntry asset, bool createDefaultEditor = true)
        {
            if (asset == null || asset.Type == "EncryptedAsset")
            {
                return;
            }
            FrostyGameplayAdvancedView.spPropertyPanel.Children.Clear();
            var ebx = GameplayProjectManagement.FrostyProject.AssetManager.GetEbx(asset as EbxAssetEntry);
            if(ebx != null)
            {
                FrostyGameplayAdvancedView.spPropertyPanel.Children.Add(new Editor(asset, ebx, GameplayProjectManagement.FrostyProject));
            }
        }

        private void btnTestChangeSpeed_Click(object sender, RoutedEventArgs e)
        {

            var ebxItem_movement = GameplayProjectManagement.FrostyProject.AssetManager.EnumerateEbx().FirstOrDefault(x => x.Name == "Fifa/Attribulator/Gameplay/groups/gp_actor/gp_actor_movement_runtime");
            var getEbx_movement = GameplayProjectManagement.FrostyProject.AssetManager.GetEbx(ebxItem_movement);
            GameplayProjectManagement.FrostyProject.AssetManager.RevertAsset(ebxItem_movement);

            var ebxItem_markingdist = GameplayProjectManagement.FrostyProject.AssetManager.EnumerateEbx().FirstOrDefault(x => x.Name == "Fifa/Attribulator/Gameplay/groups/gp_positioning/gp_positioning_markingdist_runtime");
            var getEbx_markingdist = GameplayProjectManagement.FrostyProject.AssetManager.GetEbx(ebxItem_movement);

            var root = getEbx_movement.RootObject as dynamic;
            root.ATTR_DribbleJogSpeed = 0.01f;
            root.ATTR_JogSpeed = 0.01f;
            root.ATTR_SprintSpeedTbl = new List<float>() { 0.01f, 0.02f };
            //getEbx_movement.Objects.Count(x=>x.)
            //getEbx_movement.AddRootObject(root);
            //getEbx_movement.RemoveObject(root);
            getEbx_movement.AddObject(root);
            //getEbx_movement.AddRootObject(root);
            GameplayProjectManagement.FrostyProject.AssetManager.ModifyEbx(ebxItem_movement.Name, getEbx_movement);
            //btnTestChangeSpeed.IsEnabled = false;
        }
    }
}
