using FrostyEditor.Controls;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using paulv2k4ModdingExecuter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;
using v2k4FIFASDKGenerator;

namespace Modding_UI_2021.Forms
{
    public partial class Madden21Editor : Form, ILogger
    {
        ProjectManagement ProjectManagement;

        public Madden21Editor()
        {
            InitializeComponent();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "EXE files (*.exe)|*.exe";
           if(DialogResult.OK == openFileDialog.ShowDialog())
            {
                var filePath = openFileDialog.FileName;
                var FIFADirectory = openFileDialog.FileName.Substring(0, openFileDialog.FileName.LastIndexOf("\\") + 1);
                GameInstanceSingleton.GAMERootPath = FIFADirectory;
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                //if (!string.IsNullOrEmpty(fileName) && CompatibleFIFAVersions.Contains(fileName))
                //{
                    GameInstanceSingleton.GAMEVERSION = fileName.Replace(".exe", "");
                //}
               
                    Task.Delay(500);
                    
                    this.Enabled = false;
                    new TaskFactory().StartNew(() =>
                    {
                        var result = Start().Result;
                        if (this.InvokeRequired)
                        {
                            this.Invoke(new MethodInvoker(delegate { this.Enabled = true; }));
                            toolStripStatusLabel1.Text = string.Empty;
                        }
                        //    }
                        //    this.Enabled = true;
                        //else
                        //{
                        //    this.Close();
                        //}
                    });




            }

        }

        private async Task<bool> Start()
        {
            BuildCache buildCache = new BuildCache();
            await buildCache.LoadDataAsync(GameInstanceSingleton.GAMEVERSION, GameInstanceSingleton.GAMERootPath, this, loadSDK: true);

            ProjectManagement = new ProjectManagement(GameInstanceSingleton.GAMERootPath + "\\" + GameInstanceSingleton.GAMEVERSION + ".exe");
            ProjectManagement.FrostyProject = new FrostySdk.FrostyProject(AssetManager.Instance, AssetManager.Instance.fs);

            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate {

                    string lastPath = string.Empty;
                    TreeNode lastNode = null;
                    List<TreeNode> treeNodes = new List<TreeNode>();
                    foreach (var item in ProjectManagement.FrostyProject.AssetManager.EnumerateEbx().OrderBy(x=>x.Path))
                    {
                        var ebxA = item;
                        var type = ebxA.Type;

                        if(item.Path != lastPath)
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

                    if(treeNodes.Count > 0)
                    {
                        treeView1.Nodes.AddRange(treeNodes.ToArray());
                        treeView1.NodeMouseDoubleClick += TreeView1_NodeMouseDoubleClick;
                    }
                
                }));
                
            }


            return true;

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
                            }
                        }

                    }
                }
            }
        }

        public void SaveProject()
        {
            ProjectManagement.FrostyProject.Save();
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

        }

        private void btnExport_Click(object sender, EventArgs e)
        {

        }

        private void modToolStripMenuItem_Click(object sender, EventArgs e)
        {

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
    }
}
