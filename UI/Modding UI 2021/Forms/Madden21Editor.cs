using Frostbite.Textures;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using paulv2k4ModdingExecuter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;
using v2k4FIFASDKGenerator;

namespace Modding_UI_2021.Forms
{
    public partial class Madden21Editor : BaseForm, ILogger
    {
        ProjectManagement ProjectManagement;

        public Madden21Editor()
        {
            InitializeComponent();
            OpenTheGame();

        }

        public override Task<bool> Start()
        {
            return Task.Run<bool>(() =>
            {
                BuildCache buildCache = new BuildCache();
                buildCache.LoadDataAsync(GameInstanceSingleton.GAMEVERSION, GameInstanceSingleton.GAMERootPath, this, loadSDK: true).Wait();

                ProjectManagement = new ProjectManagement(GameInstanceSingleton.GAMERootPath + "\\" + GameInstanceSingleton.GAMEVERSION + ".exe");
                ProjectManagement.FrostyProject = new FrostySdk.FrostyProject(AssetManager.Instance, AssetManager.Instance.fs);
                List<Task> tasks = new List<Task>();
                var taskFact = new TaskFactory();
                tasks.Add(taskFact.StartNew(() =>
                {
                    BuildStandardTreeView();
                }));
                tasks.Add(taskFact.StartNew(() =>
                {
                    BuildLegacyTreeView();
                }));

                Task.WaitAll(tasks.ToArray());
                return true;
            });
           

        }
        public override Task<bool> PostStart()
        {
            return Task.Run<bool>(() => {
            
                if(this.InvokeRequired)
                {
                    //this.toolStripStatusLabel1.Text = "Complete";
                    this.Enabled = true;
                }

                return true;
            
            });
        }

        private void BuildStandardTreeView()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {

                    string lastPath = string.Empty;
                    TreeNode lastNode = null;
                    List<TreeNode> treeNodes = new List<TreeNode>();
                    foreach (var item in ProjectManagement.FrostyProject.AssetManager.EnumerateEbx().OrderBy(x => x.Path))
                    {
                        var ebxA = item;
                        var type = ebxA.Type;

                        if (item.Path != lastPath)
                        {
                            if (lastNode != null)
                                treeNodes.Add(lastNode);

                            lastNode = new TreeNode(item.Path);
                            lastPath = item.Path;
                        }

                        if (lastNode == null)
                            lastNode = new TreeNode();

                        var innerTreeNode = new TreeNode(item.Name);
                        innerTreeNode.Tag = item.Guid;
                        lastNode.Nodes.Add(innerTreeNode);
                    }

                    if (treeNodes.Count > 0)
                    {
                        treeView1.Nodes.AddRange(treeNodes.ToArray());
                        treeView1.NodeMouseDoubleClick += TreeView1_NodeMouseDoubleClick;
                    }

                }));

            }
        }

        //public void PopulateLegacyTree(ref TreeNode root, List<AssetEntry> Entries)
        //{
        //    if (root == null)
        //    {
        //        root = new TreeNode();
        //        root.Text = "Departments";
        //        root.Tag = null;
        //        // get all departments in the list with parent is null
        //        var details = departments.Where(t => t.Parent == null);
        //        foreach (var detail in details)
        //        {
        //            var child = new TreeNode()
        //            {
        //                Text = detail.Name,
        //                Tag = detail.Id,
        //            };
        //            PopulateTree(ref child, departments);
        //            root.Nodes.Add(child);
        //        }
        //    }
        //    else
        //    {
        //        var id = (int)root.Tag;
        //        var details = departments.Where(t => t.Parent == id);
        //        foreach (var detail in details)
        //        {
        //            var child = new TreeNode()
        //            {
        //                Text = detail.Name,
        //                Tage = detail.Id,
        //            };
        //            PopulateTree(ref child, departments);
        //            root.Nodes.Add(child);
        //        }
        //    }
        //}

        private void BuildLegacyTreeView()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {

                    string lastPath = string.Empty;
                    TreeNode lastNode = null;
                    List<TreeNode> treeNodes = new List<TreeNode>();
                    foreach (var item in ProjectManagement.FrostyProject.AssetManager.EnumerateCustomAssets("legacy").OrderBy(x => x.Path))
                    {
                        var ebxA = item;
                        var type = ebxA.Type;

                        if (item.Path.ToLower() != lastPath.ToLower())
                        {
                            if (lastNode != null)
                                treeNodes.Add(lastNode);

                            lastNode = new TreeNode(item.Path.ToLower());
                            lastPath = item.Path.ToLower();
                        }

                        if (lastNode == null)
                            lastNode = new TreeNode();

                        var innerTreeNode = new TreeNode(item.Name);
                        innerTreeNode.Tag = item;
                        lastNode.Nodes.Add(innerTreeNode);
                    }

                    if (treeNodes.Count > 0)
                    {
                        TreeViewLegacy.Nodes.AddRange(treeNodes.ToArray());
                        TreeViewLegacy.NodeMouseClick += TreeViewLegacy_NodeMouseClick;
                    }

                }));

            }
        }


        private void TreeViewLegacy_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeView treeView = sender as TreeView;
            if (treeView != null)
            {
                var selectedNode = treeView.SelectedNode;
                if (selectedNode != null && selectedNode.Tag != null)
                {

                    //TreeNode node = sender as TreeNode;
                    //if (node != null)
                    //{
                    //    var assetEntry = node.Tag as AssetEntry;
                    // Check for extension
                    if (selectedNode.Text.Split('.').Length > 1)
                    {
                        if (e.Button == MouseButtons.Right)
                        {
                            var assetEntry = (AssetEntry)selectedNode.Tag;
                            ContextMenu LegacyViewContextMenu = new ContextMenu();
                            LegacyViewContextMenu.MenuItems.Add(
                                new MenuItem("Import", new EventHandler((object o, EventArgs args) =>
                                {
                                    OpenFileDialog openFileDialog = new OpenFileDialog();
                                    var filt = "*." + selectedNode.Text.Split('.')[1];
                                    openFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
                                    openFileDialog.FileName = selectedNode.Text;
                                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                                    {
                                        byte[] data;
                                        using (NativeReader nativeReader = new NativeReader(new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read)))
                                        {
                                            data = nativeReader.ReadToEnd();
                                        }

                                        ProjectManagement.FrostyProject.AssetManager.ModifyCustomAsset("legacy", assetEntry.Path, data);
                                        ProjectManagement.FrostyProject.AssetManager.SendManagerCommand("legacy", "FlushCache");
                                        SaveProject();

                                    }

                                })));
                            LegacyViewContextMenu.MenuItems.Add(new MenuItem("Export", new EventHandler((object o, EventArgs args) =>
                            {
                                SaveFileDialog saveFileDialog = new SaveFileDialog();
                                var filt = "*." + selectedNode.Text.Split('.')[1];
                                saveFileDialog.Filter = filt.Split('.')[1] + " files (" + filt + ")|" + filt;
                                saveFileDialog.FileName = selectedNode.Text;
                                if(saveFileDialog.ShowDialog() == DialogResult.OK)
                                {
                                    using (NativeWriter nativeWriter = new NativeWriter(new FileStream(saveFileDialog.FileName, FileMode.Create)))
                                    {
                                        nativeWriter.Write(new NativeReader(ProjectManagement.FrostyProject.AssetManager.GetCustomAsset("legacy", assetEntry)).ReadToEnd());
                                    }
                                }
                            })));
                            LegacyViewContextMenu.Show(TreeViewLegacy, e.Location);
                        }
                    }
                }
            }
            //}
        }

        EbxAssetEntry SelectedAssetEntry;

        private void TreeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeView treeView = sender as TreeView;
            if(treeView != null)
            {
                var selectedNode = treeView.SelectedNode;
                if (selectedNode.Tag != null)
                {
                    SelectedAssetEntry = ProjectManagement.FrostyProject.AssetManager.GetEbxEntry(Guid.Parse(selectedNode.Tag.ToString()));
                    if (SelectedAssetEntry != null)
                    {
                        var ebx = ProjectManagement.FrostyProject.AssetManager.GetEbx(SelectedAssetEntry);
                        if (ebx != null)
                        {
                            AssetNameHeader.Text = SelectedAssetEntry.Name;
                            AssetGuid.Text = SelectedAssetEntry.Guid.ToString();
                            AssetType.Text = SelectedAssetEntry.Type;

                            if(SelectedAssetEntry.Type == "TextureAsset")
                            {
                                btnImport.Enabled = true;
                                btnExport.Enabled = true;

                                var resAssetEntry = ProjectManagement.FrostyProject.AssetManager.GetResEntry(SelectedAssetEntry.Name);
                                var resStream = ProjectManagement.FrostyProject.AssetManager.GetRes(resAssetEntry);
                                FrostySdk.Resources.Texture textureAsset = new FrostySdk.Resources.Texture(resStream, ProjectManagement.FrostyProject.AssetManager);
                                new TextureExporter().Export(textureAsset, $"temp.png", "*.png");
                                ImageViewer.ImageLocation = Application.StartupPath + "//temp.png";
                                ImageViewer.SizeMode = PictureBoxSizeMode.StretchImage;
                            }
                        }

                    }
                }
            }
        }

        public void SaveProject(bool forceNewSave = false)
        {
            if (!string.IsNullOrEmpty(ProjectManagement.FrostyProject.Filename) && !forceNewSave)
                ProjectManagement.FrostyProject.Save();
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Project files|*.fbproject|P2 files|*.fbproject2";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ProjectManagement.FrostyProject.Save(saveFileDialog.FileName);
                }
            }
        }


        public void Log(string text, params object[] vars)
        {
                toolStripStatusLabel1.Text = text;
        }

        public void LogError(string text, params object[] vars)
        {
        }

        public void LogWarning(string text, params object[] vars)
        {
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (SelectedAssetEntry.Type == "TextureAsset")
            {
                btnImport.Enabled = true;
                btnExport.Enabled = true;

                var resAssetEntry = ProjectManagement.FrostyProject.AssetManager.GetResEntry(SelectedAssetEntry.Name);
                var resStream = ProjectManagement.FrostyProject.AssetManager.GetRes(resAssetEntry);
                FrostySdk.Resources.Texture textureAsset = new FrostySdk.Resources.Texture(resStream, ProjectManagement.FrostyProject.AssetManager);
                OpenFileDialog openFileDialog = new OpenFileDialog();
                var filt = "*.dds";
                openFileDialog.Filter = filt + " files (" + filt + ")|" + filt;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    new Frostbite.Textures.TextureImporter().Import(openFileDialog.FileName, SelectedAssetEntry, ref textureAsset);

                }
            }
          
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (SelectedAssetEntry.Type == "TextureAsset")
            {
                saveFileDialog.Filter = "*.dds|DDS File";
                if (DialogResult.OK == saveFileDialog.ShowDialog())
                {
                    var resAssetEntry = ProjectManagement.FrostyProject.AssetManager.GetResEntry(SelectedAssetEntry.Name);
                    var resStream = ProjectManagement.FrostyProject.AssetManager.GetRes(resAssetEntry);
                    FrostySdk.Resources.Texture textureAsset = new FrostySdk.Resources.Texture(resStream, ProjectManagement.FrostyProject.AssetManager);
                    new TextureExporter().Export(textureAsset, saveFileDialog.FileName, "*.dds");
                }
            }
            else
            {

            }
        }

        private void modToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Mod files|*.fbmod";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ProjectManagement.FrostyProject.WriteToMod(saveFileDialog.FileName, new FrostySdk.ModSettings() { Author = "T", Description = "T", Category = "T", Title = "T", Version = "T" });
            }
        }

        private void btnLaunchGame_Click(object sender, EventArgs e)
        {
            SaveProject();
            ProjectManagement.FrostyProject.WriteToMod("TestMod.fbmod"
                , new FrostySdk.ModSettings() { Author = "Madden 21 Editor", Category = "Test", Description = "Test", Title = "Madden 21 Editor Test Mod", Version = "1" });

            var fme = new FrostyModExecutor();
            var result = fme.Run(AssetManager.Instance.fs, this, "", "", new System.Collections.Generic.List<string>() { @"TestMod.fbmod" }.ToArray()).Result;

        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Project files|*.fbproject|P2 files|*.fbproject2";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                ProjectManagement.FrostyProject.Load(fileDialog.FileName);
            }

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        private void newProjectFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject(true);
        }
    }
}
