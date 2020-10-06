namespace Modding_UI_2021.Forms
{
    partial class Madden21Editor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Madden21Editor));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newProjectFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnLaunchGame = new System.Windows.Forms.ToolStripButton();
            this.PanelEditTextures = new System.Windows.Forms.FlowLayoutPanel();
            this.PanelTexturesImpExp = new System.Windows.Forms.FlowLayoutPanel();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.AssetNameHeader = new System.Windows.Forms.Label();
            this.AssetGuid = new System.Windows.Forms.Label();
            this.AssetType = new System.Windows.Forms.Label();
            this.ImageViewer = new System.Windows.Forms.PictureBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.TreeViewLegacy = new System.Windows.Forms.TreeView();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.PanelEditTextures.SuspendLayout();
            this.PanelTexturesImpExp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImageViewer)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 582);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1620, 26);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(151, 20);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.btnLaunchGame});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1620, 31);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(46, 28);
            this.toolStripDropDownButton1.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.openToolStripMenuItem.Text = "Open...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modToolStripMenuItem,
            this.newProjectFileToolStripMenuItem});
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.saveAsToolStripMenuItem.Text = "Save As...";
            // 
            // modToolStripMenuItem
            // 
            this.modToolStripMenuItem.Name = "modToolStripMenuItem";
            this.modToolStripMenuItem.Size = new System.Drawing.Size(199, 26);
            this.modToolStripMenuItem.Text = "Export Mod";
            this.modToolStripMenuItem.Click += new System.EventHandler(this.modToolStripMenuItem_Click);
            // 
            // newProjectFileToolStripMenuItem
            // 
            this.newProjectFileToolStripMenuItem.Name = "newProjectFileToolStripMenuItem";
            this.newProjectFileToolStripMenuItem.Size = new System.Drawing.Size(199, 26);
            this.newProjectFileToolStripMenuItem.Text = "New Project File";
            this.newProjectFileToolStripMenuItem.Click += new System.EventHandler(this.newProjectFileToolStripMenuItem_Click);
            // 
            // btnLaunchGame
            // 
            this.btnLaunchGame.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnLaunchGame.Image = ((System.Drawing.Image)(resources.GetObject("btnLaunchGame.Image")));
            this.btnLaunchGame.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLaunchGame.Name = "btnLaunchGame";
            this.btnLaunchGame.Size = new System.Drawing.Size(102, 28);
            this.btnLaunchGame.Text = "Launch Game";
            this.btnLaunchGame.Click += new System.EventHandler(this.btnLaunchGame_Click);
            // 
            // PanelEditTextures
            // 
            this.PanelEditTextures.Controls.Add(this.PanelTexturesImpExp);
            this.PanelEditTextures.Controls.Add(this.AssetNameHeader);
            this.PanelEditTextures.Controls.Add(this.AssetGuid);
            this.PanelEditTextures.Controls.Add(this.AssetType);
            this.PanelEditTextures.Controls.Add(this.ImageViewer);
            this.PanelEditTextures.Dock = System.Windows.Forms.DockStyle.Right;
            this.PanelEditTextures.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.PanelEditTextures.Location = new System.Drawing.Point(548, 31);
            this.PanelEditTextures.Name = "PanelEditTextures";
            this.PanelEditTextures.Size = new System.Drawing.Size(1072, 551);
            this.PanelEditTextures.TabIndex = 3;
            // 
            // PanelTexturesImpExp
            // 
            this.PanelTexturesImpExp.Controls.Add(this.btnImport);
            this.PanelTexturesImpExp.Controls.Add(this.btnExport);
            this.PanelTexturesImpExp.Location = new System.Drawing.Point(3, 3);
            this.PanelTexturesImpExp.Name = "PanelTexturesImpExp";
            this.PanelTexturesImpExp.Size = new System.Drawing.Size(1069, 29);
            this.PanelTexturesImpExp.TabIndex = 3;
            // 
            // btnImport
            // 
            this.btnImport.Enabled = false;
            this.btnImport.Location = new System.Drawing.Point(3, 3);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(75, 23);
            this.btnImport.TabIndex = 0;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnExport
            // 
            this.btnExport.Enabled = false;
            this.btnExport.Location = new System.Drawing.Point(84, 3);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 1;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // AssetNameHeader
            // 
            this.AssetNameHeader.AutoSize = true;
            this.AssetNameHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AssetNameHeader.Location = new System.Drawing.Point(3, 35);
            this.AssetNameHeader.Name = "AssetNameHeader";
            this.AssetNameHeader.Size = new System.Drawing.Size(119, 25);
            this.AssetNameHeader.TabIndex = 0;
            this.AssetNameHeader.Text = "Asset Name";
            // 
            // AssetGuid
            // 
            this.AssetGuid.AutoSize = true;
            this.AssetGuid.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AssetGuid.Location = new System.Drawing.Point(3, 60);
            this.AssetGuid.Name = "AssetGuid";
            this.AssetGuid.Size = new System.Drawing.Size(92, 20);
            this.AssetGuid.TabIndex = 1;
            this.AssetGuid.Text = "Asset Guid";
            // 
            // AssetType
            // 
            this.AssetType.AutoSize = true;
            this.AssetType.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AssetType.Location = new System.Drawing.Point(3, 80);
            this.AssetType.Name = "AssetType";
            this.AssetType.Size = new System.Drawing.Size(93, 20);
            this.AssetType.TabIndex = 2;
            this.AssetType.Text = "Asset Type";
            // 
            // ImageViewer
            // 
            this.ImageViewer.Location = new System.Drawing.Point(3, 103);
            this.ImageViewer.MaximumSize = new System.Drawing.Size(256, 256);
            this.ImageViewer.MinimumSize = new System.Drawing.Size(256, 256);
            this.ImageViewer.Name = "ImageViewer";
            this.ImageViewer.Size = new System.Drawing.Size(256, 256);
            this.ImageViewer.TabIndex = 4;
            this.ImageViewer.TabStop = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Left;
            this.tabControl1.Location = new System.Drawing.Point(0, 31);
            this.tabControl1.MinimumSize = new System.Drawing.Size(400, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(400, 551);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.treeView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(392, 522);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Standard";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // treeView1
            // 
            this.treeView1.CausesValidation = false;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(3, 3);
            this.treeView1.Name = "treeView1";
            this.treeView1.PathSeparator = "/";
            this.treeView1.Size = new System.Drawing.Size(386, 516);
            this.treeView1.TabIndex = 2;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.TreeViewLegacy);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(392, 522);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Legacy";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // TreeViewLegacy
            // 
            this.TreeViewLegacy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeViewLegacy.Location = new System.Drawing.Point(3, 3);
            this.TreeViewLegacy.Name = "TreeViewLegacy";
            this.TreeViewLegacy.Size = new System.Drawing.Size(386, 516);
            this.TreeViewLegacy.TabIndex = 0;
            // 
            // Madden21Editor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1620, 608);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.PanelEditTextures);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Madden21Editor";
            this.Text = "Madden 21 Editor";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.PanelEditTextures.ResumeLayout(false);
            this.PanelEditTextures.PerformLayout();
            this.PanelTexturesImpExp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ImageViewer)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.FlowLayoutPanel PanelEditTextures;
        private System.Windows.Forms.Label AssetNameHeader;
        private System.Windows.Forms.Label AssetGuid;
        private System.Windows.Forms.Label AssetType;
        private System.Windows.Forms.FlowLayoutPanel PanelTexturesImpExp;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newProjectFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton btnLaunchGame;
        private System.Windows.Forms.PictureBox ImageViewer;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TreeView TreeViewLegacy;
    }
}