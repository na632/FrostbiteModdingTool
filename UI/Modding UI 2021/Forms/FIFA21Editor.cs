using Frostbite.Textures;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using Newtonsoft.Json;
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
using v2k4FIFAModding;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;
using v2k4FIFASDKGenerator;

namespace Modding_UI_2021.Forms
{
    public partial class FIFA21Editor : Form, IEditorForm, ILogger
    {
        ProjectManagement ProjectManagement;
        public FIFA21Editor()
        {
            InitializeComponent();
            OpenTheGame();
        }

        public void OpenTheGame()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "EXE files (*.exe)|*.exe";
            if (DialogResult.OK == openFileDialog.ShowDialog())
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
                    result = PostStart().Result;

                    //    }
                    //    this.Enabled = true;
                    //else
                    //{
                    //    this.Close();
                    //}
                });
            }
        }
        public Task<bool> PostStart()
        {
            return Task.Run<bool>(() => {

                if (this.InvokeRequired)
                {
                    //this.toolStripStatusLabel1.Text = "Complete";
                    this.Invoke(new MethodInvoker(delegate
                    {
                        this.Enabled = true;
                        txtStandardSearchBox.KeyUp += TxtStandardSearchBox_KeyUp;
                    }));
                }

                return true;

            });
        }

        private void TxtStandardSearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                Task.Run(() => { BuildStandardTreeView(txtStandardSearchBox.Text); });
            }
        }

        public async Task<bool> PreStart()
        {
            return await Task.Run<bool>(() =>
            {
                return false;
            });
        }

        public async Task<bool> Start()
        {
            return await Task.Run<bool>(() =>
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

        private void BuildStandardTreeView(string filter = null)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    toolStripStatusLabel1.Text = "Loading Standard Tree View";
                    treeView1.Nodes.Clear();

                    string lastPath = string.Empty;
                    TreeNode lastNode = null;
                    List<TreeNode> treeNodes = new List<TreeNode>();
                    var allEBX = ProjectManagement.FrostyProject.AssetManager.EnumerateEbx();
                    if (!string.IsNullOrEmpty(filter)) 
                    {
                        allEBX = allEBX.Where(x => 
                            x.Path.ToLower().Contains(filter)
                            || x.Name.ToLower().Contains(filter)
                            ).ToList();
                    }
                    allEBX = allEBX.OrderBy(x => x.Path);
                    foreach (var item in allEBX)
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

                        var innerTreeNode = new TreeNode(item.DisplayName);
                        innerTreeNode.Tag = item;
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

        private void BuildLegacyTreeView()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate
                {
                    toolStripStatusLabel1.Text = "Loading Legacy Tree View";

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

                        var innerTreeNode = new TreeNode(item.DisplayName);
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
        }

        private void TreeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeView treeView = sender as TreeView;
            if (treeView != null)
            {
                var selectedNode = treeView.SelectedNode;
                if (selectedNode != null && selectedNode.Tag != null)
                {
                    var assetEntry = (EbxAssetEntry)selectedNode.Tag;
                    var eb = AssetManager.Instance.GetEbx(assetEntry);
                    if (eb != null)
                    {
                        ImageViewer.Visible = false;
                        txtTextViewer.Visible = false;

                        if (assetEntry.Type == "TextureAsset")
                        {
                            var resAssetEntry = ProjectManagement.FrostyProject.AssetManager.GetResEntry(assetEntry.Name);
                            var resStream = ProjectManagement.FrostyProject.AssetManager.GetRes(resAssetEntry);
                            FrostySdk.Resources.Texture textureAsset = new FrostySdk.Resources.Texture(resStream, ProjectManagement.FrostyProject.AssetManager);
                            new TextureExporter().Export(textureAsset, $"temp.png", "*.png");
                            ImageViewer.ImageLocation = Application.StartupPath + "//temp.png";
                            ImageViewer.SizeMode = PictureBoxSizeMode.StretchImage;
                            ImageViewer.Visible = true;
                        }
                        else if(assetEntry.Type.Contains("AttribSchema"))
                        {
                            rightLayoutPanel.Controls.Clear();
                            foreach(var prop in eb.RootObject.GetProperties())
                            {
                                var leftrightLayout = new FlowLayoutPanel();
                                leftrightLayout.FlowDirection = FlowDirection.LeftToRight;
                                var leftLabel = new Label();
                                leftLabel.Text = prop.Name;

                                var rightText = new TextBox();
                                rightText.Text = prop.GetValue(eb.RootObject).ToString();

                                leftrightLayout.Controls.Add(leftLabel);
                                leftrightLayout.Controls.Add(rightText);

                                rightLayoutPanel.Controls.Add(leftrightLayout);
                            }
                        }
                        else
                        {
                            txtTextViewer.Text = JsonConvert.SerializeObject(eb.RootObject);
                            txtTextViewer.Visible = true;
                        }
                    }
                }
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

        public void LogWarning(string text, params object[] vars)
        {
        }

        public void LogError(string text, params object[] vars)
        {
        }

        private void newProjectFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject(true);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject();

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

        private void txtStandardSearchBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
