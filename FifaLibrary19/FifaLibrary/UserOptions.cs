using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary
{
	public class UserOptions : Form
	{
		private string m_XmlFileName;

		public bool m_AutoExportFolder = true;

		public string m_ExportFolder;

		public bool m_SaveDatabase = true;

		public bool m_SaveGui = true;

		public bool m_SaveZdata = true;

		public bool m_AutoZdata = true;

		public bool m_SpecificZdata;

		public bool m_SaveZdataInFolder = true;

		public int m_ZdataNumber;

		public bool m_SaveGuiInArchive;

		public bool m_SaveGuiInFolder = true;

		private IContainer components;

		private TextBox textExportFolder;

		private Button buttonBrowseExportFolder;

		private ToolTip toolTip;

		private CheckBox checkSaveDb;

		private CheckBox checkSaveZdata;

		private CheckBox checkSaveGui;

		private RadioButton radioAutoZdata;

		private RadioButton radioSpecificZdata;

		private NumericUpDown numericZdata;

		private GroupBox groupZdataSelection;

		private GroupBox groupAllowSaving;

		private GroupBox groupGuiSaveOptions;

		private RadioButton radioGuiSaveFolder;

		private RadioButton radioGuiSaveArchive;

		private GroupBox groupExportFolde;

		private Button buttonCancel;

		private Button buttonOK;

		private CheckBox checkAutoExportFolder;

		private Options optionsSet;

		private RadioButton radioZdataSaveFolder;

		public UserOptions()
		{
			InitializeComponent();
			string currentDirectory = Environment.CurrentDirectory;
			m_XmlFileName = currentDirectory + "\\Options.xml";
			if (File.Exists(m_XmlFileName))
			{
				optionsSet.ReadXml(m_XmlFileName);
				LoadOptions();
			}
		}

		public DialogResult ShowOptions()
		{
			LoadOptions();
			return ShowDialog();
		}

		private void LoadOptions()
		{
			for (int i = 0; i < optionsSet.DataTableOpt.Count; i++)
			{
				string option = optionsSet.DataTableOpt[i].Option;
				string value = optionsSet.DataTableOpt[i].Value;
				int num;
				try
				{
					num = Convert.ToInt32(value);
				}
				catch
				{
					num = 0;
				}
				bool flag = num != 0;
				_ = optionsSet.DataTableOpt[i].Default;
				if (option == "ExportFolderAuto")
				{
					checkAutoExportFolder.Checked = flag;
					m_AutoExportFolder = flag;
					textExportFolder.Enabled = !flag;
					buttonBrowseExportFolder.Enabled = !flag;
				}
				else if (option == "ExportFolder")
				{
					textExportFolder.Text = value;
					m_ExportFolder = value;
				}
				else if (option == "DatabaseEditing")
				{
					checkSaveDb.Checked = flag;
					m_SaveDatabase = flag;
				}
				else if (option == "ZdataEditing")
				{
					checkSaveZdata.Checked = flag;
					m_SaveZdata = flag;
				}
				else if (option == "GuiEditing")
				{
					checkSaveGui.Checked = flag;
					m_SaveGui = flag;
				}
				else if (option == "AutoZdata")
				{
					radioAutoZdata.Checked = flag;
					m_AutoZdata = flag;
					if (m_AutoZdata)
					{
						m_SpecificZdata = false;
						m_SaveZdataInFolder = false;
					}
					numericZdata.Enabled = m_SpecificZdata;
				}
				else if (option == "SpecificZdata")
				{
					radioSpecificZdata.Checked = flag;
					m_SpecificZdata = flag;
					if (m_SpecificZdata)
					{
						m_AutoZdata = false;
						m_SaveZdataInFolder = false;
					}
					numericZdata.Enabled = m_SpecificZdata;
				}
				else if (option == "SaveZdataInFolder")
				{
					radioZdataSaveFolder.Checked = flag;
					m_SaveZdataInFolder = flag;
					if (m_SaveZdataInFolder)
					{
						m_AutoZdata = false;
						m_SpecificZdata = false;
					}
					numericZdata.Enabled = m_SpecificZdata;
				}
				else if (option == "ZdataNumber")
				{
					numericZdata.Value = num;
					m_ZdataNumber = num;
				}
				else if (option == "SaveGuiInArchive")
				{
					radioGuiSaveArchive.Checked = flag;
					m_SaveGuiInArchive = flag;
					m_SaveGuiInFolder = !flag;
				}
				else if (option == "SaveGuiInFolder")
				{
					radioGuiSaveFolder.Checked = flag;
					m_SaveGuiInArchive = !flag;
					m_SaveGuiInFolder = flag;
				}
			}
		}

		private void buttonBrowseExportFolder_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.Description = "Select the export folder";
			folderBrowserDialog.ShowNewFolderButton = true;
			if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
			{
				folderBrowserDialog.Dispose();
				return;
			}
			textExportFolder.Text = folderBrowserDialog.SelectedPath;
			m_ExportFolder = textExportFolder.Text;
			folderBrowserDialog.Dispose();
		}

		private void checkEditDb_CheckedChanged(object sender, EventArgs e)
		{
			m_SaveDatabase = checkSaveDb.Checked;
		}

		private void textExportFolder_TextChanged(object sender, EventArgs e)
		{
			m_ExportFolder = textExportFolder.Text;
		}

		private void checkEditZdata_CheckedChanged(object sender, EventArgs e)
		{
			m_SaveZdata = checkSaveZdata.Checked;
		}

		private void checkEditGui_CheckedChanged(object sender, EventArgs e)
		{
			m_SaveGui = checkSaveGui.Checked;
		}

		private void radioAutoZdata_CheckedChanged(object sender, EventArgs e)
		{
			m_AutoZdata = radioAutoZdata.Checked;
		}

		private void radioSpecificZdata_CheckedChanged(object sender, EventArgs e)
		{
			m_SpecificZdata = radioSpecificZdata.Checked;
			numericZdata.Enabled = radioSpecificZdata.Checked;
		}

		private void numericZdata_ValueChanged(object sender, EventArgs e)
		{
			m_ZdataNumber = (int)numericZdata.Value;
		}

		private void radioGuiSaveArchive_CheckedChanged(object sender, EventArgs e)
		{
			m_SaveGuiInArchive = radioGuiSaveArchive.Checked;
		}

		private void radioGuiSaveFolder_CheckedChanged(object sender, EventArgs e)
		{
			m_SaveGuiInFolder = radioGuiSaveFolder.Checked;
		}

		private void SaveOptions()
		{
			for (int i = 0; i < optionsSet.DataTableOpt.Count; i++)
			{
				string option = optionsSet.DataTableOpt[i].Option;
				if (option == "ExportFolderAuto")
				{
					optionsSet.DataTableOpt[i].Value = (m_AutoExportFolder ? "1" : "0");
				}
				else if (option == "ExportFolder")
				{
					optionsSet.DataTableOpt[i].Value = m_ExportFolder;
				}
				else if (option == "DatabaseEditing")
				{
					optionsSet.DataTableOpt[i].Value = (m_SaveDatabase ? "1" : "0");
				}
				else if (option == "ZdataEditing")
				{
					optionsSet.DataTableOpt[i].Value = (m_SaveZdata ? "1" : "0");
				}
				else if (option == "GuiEditing")
				{
					optionsSet.DataTableOpt[i].Value = (m_SaveGui ? "1" : "0");
				}
				else if (option == "AutoZdata")
				{
					optionsSet.DataTableOpt[i].Value = (m_AutoZdata ? "1" : "0");
				}
				else if (option == "SpecificZdata")
				{
					optionsSet.DataTableOpt[i].Value = (m_SpecificZdata ? "1" : "0");
				}
				else if (option == "ZdataNumber")
				{
					optionsSet.DataTableOpt[i].Value = m_ZdataNumber.ToString();
				}
				else if (option == "SaveZdataInFolder")
				{
					optionsSet.DataTableOpt[i].Value = (m_SaveZdataInFolder ? "1" : "0");
				}
				else if (option == "SaveGuiInArchive")
				{
					optionsSet.DataTableOpt[i].Value = (m_SaveGuiInArchive ? "1" : "0");
				}
				else if (option == "SaveGuiInFolder")
				{
					optionsSet.DataTableOpt[i].Value = (m_SaveGuiInFolder ? "1" : "0");
				}
			}
			optionsSet.WriteXml(m_XmlFileName);
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			SaveOptions();
		}

		private void checkAutoExportFolder_CheckedChanged(object sender, EventArgs e)
		{
			m_AutoExportFolder = checkAutoExportFolder.Checked;
			textExportFolder.Enabled = !checkAutoExportFolder.Checked;
			buttonBrowseExportFolder.Enabled = !checkAutoExportFolder.Checked;
		}

		private void UserOptions_FormClosing(object sender, FormClosingEventArgs e)
		{
			SaveOptions();
		}

		private void radioZdataSaveFolder_CheckedChanged(object sender, EventArgs e)
		{
			m_SaveZdataInFolder = radioZdataSaveFolder.Checked;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaLibrary.UserOptions));
			textExportFolder = new System.Windows.Forms.TextBox();
			buttonBrowseExportFolder = new System.Windows.Forms.Button();
			toolTip = new System.Windows.Forms.ToolTip(components);
			checkSaveDb = new System.Windows.Forms.CheckBox();
			checkSaveZdata = new System.Windows.Forms.CheckBox();
			checkSaveGui = new System.Windows.Forms.CheckBox();
			radioAutoZdata = new System.Windows.Forms.RadioButton();
			radioSpecificZdata = new System.Windows.Forms.RadioButton();
			numericZdata = new System.Windows.Forms.NumericUpDown();
			radioGuiSaveArchive = new System.Windows.Forms.RadioButton();
			radioGuiSaveFolder = new System.Windows.Forms.RadioButton();
			checkAutoExportFolder = new System.Windows.Forms.CheckBox();
			radioZdataSaveFolder = new System.Windows.Forms.RadioButton();
			groupZdataSelection = new System.Windows.Forms.GroupBox();
			groupAllowSaving = new System.Windows.Forms.GroupBox();
			groupGuiSaveOptions = new System.Windows.Forms.GroupBox();
			groupExportFolde = new System.Windows.Forms.GroupBox();
			buttonCancel = new System.Windows.Forms.Button();
			buttonOK = new System.Windows.Forms.Button();
			optionsSet = new FifaLibrary.Options();
			((System.ComponentModel.ISupportInitialize)numericZdata).BeginInit();
			groupZdataSelection.SuspendLayout();
			groupAllowSaving.SuspendLayout();
			groupGuiSaveOptions.SuspendLayout();
			groupExportFolde.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)optionsSet).BeginInit();
			SuspendLayout();
			resources.ApplyResources(textExportFolder, "textExportFolder");
			textExportFolder.Name = "textExportFolder";
			textExportFolder.TextChanged += new System.EventHandler(textExportFolder_TextChanged);
			resources.ApplyResources(buttonBrowseExportFolder, "buttonBrowseExportFolder");
			buttonBrowseExportFolder.Name = "buttonBrowseExportFolder";
			buttonBrowseExportFolder.UseVisualStyleBackColor = true;
			buttonBrowseExportFolder.Click += new System.EventHandler(buttonBrowseExportFolder_Click);
			resources.ApplyResources(checkSaveDb, "checkSaveDb");
			checkSaveDb.Checked = true;
			checkSaveDb.CheckState = System.Windows.Forms.CheckState.Checked;
			checkSaveDb.Name = "checkSaveDb";
			toolTip.SetToolTip(checkSaveDb, resources.GetString("checkSaveDb.ToolTip"));
			checkSaveDb.UseVisualStyleBackColor = true;
			checkSaveDb.CheckedChanged += new System.EventHandler(checkEditDb_CheckedChanged);
			resources.ApplyResources(checkSaveZdata, "checkSaveZdata");
			checkSaveZdata.Checked = true;
			checkSaveZdata.CheckState = System.Windows.Forms.CheckState.Checked;
			checkSaveZdata.Name = "checkSaveZdata";
			toolTip.SetToolTip(checkSaveZdata, resources.GetString("checkSaveZdata.ToolTip"));
			checkSaveZdata.UseVisualStyleBackColor = true;
			checkSaveZdata.CheckedChanged += new System.EventHandler(checkEditZdata_CheckedChanged);
			resources.ApplyResources(checkSaveGui, "checkSaveGui");
			checkSaveGui.Checked = true;
			checkSaveGui.CheckState = System.Windows.Forms.CheckState.Checked;
			checkSaveGui.Name = "checkSaveGui";
			toolTip.SetToolTip(checkSaveGui, resources.GetString("checkSaveGui.ToolTip"));
			checkSaveGui.UseVisualStyleBackColor = true;
			checkSaveGui.CheckedChanged += new System.EventHandler(checkEditGui_CheckedChanged);
			resources.ApplyResources(radioAutoZdata, "radioAutoZdata");
			radioAutoZdata.Checked = true;
			radioAutoZdata.Name = "radioAutoZdata";
			radioAutoZdata.TabStop = true;
			toolTip.SetToolTip(radioAutoZdata, resources.GetString("radioAutoZdata.ToolTip"));
			radioAutoZdata.UseVisualStyleBackColor = true;
			radioAutoZdata.CheckedChanged += new System.EventHandler(radioAutoZdata_CheckedChanged);
			resources.ApplyResources(radioSpecificZdata, "radioSpecificZdata");
			radioSpecificZdata.Name = "radioSpecificZdata";
			radioSpecificZdata.TabStop = true;
			toolTip.SetToolTip(radioSpecificZdata, resources.GetString("radioSpecificZdata.ToolTip"));
			radioSpecificZdata.UseVisualStyleBackColor = true;
			radioSpecificZdata.CheckedChanged += new System.EventHandler(radioSpecificZdata_CheckedChanged);
			resources.ApplyResources(numericZdata, "numericZdata");
			numericZdata.Maximum = new decimal(new int[4]
			{
				98,
				0,
				0,
				0
			});
			numericZdata.Minimum = new decimal(new int[4]
			{
				40,
				0,
				0,
				0
			});
			numericZdata.Name = "numericZdata";
			toolTip.SetToolTip(numericZdata, resources.GetString("numericZdata.ToolTip"));
			numericZdata.Value = new decimal(new int[4]
			{
				49,
				0,
				0,
				0
			});
			numericZdata.ValueChanged += new System.EventHandler(numericZdata_ValueChanged);
			resources.ApplyResources(radioGuiSaveArchive, "radioGuiSaveArchive");
			radioGuiSaveArchive.Checked = true;
			radioGuiSaveArchive.Name = "radioGuiSaveArchive";
			radioGuiSaveArchive.TabStop = true;
			toolTip.SetToolTip(radioGuiSaveArchive, resources.GetString("radioGuiSaveArchive.ToolTip"));
			radioGuiSaveArchive.UseVisualStyleBackColor = true;
			radioGuiSaveArchive.CheckedChanged += new System.EventHandler(radioGuiSaveArchive_CheckedChanged);
			resources.ApplyResources(radioGuiSaveFolder, "radioGuiSaveFolder");
			radioGuiSaveFolder.Name = "radioGuiSaveFolder";
			toolTip.SetToolTip(radioGuiSaveFolder, resources.GetString("radioGuiSaveFolder.ToolTip"));
			radioGuiSaveFolder.UseVisualStyleBackColor = true;
			radioGuiSaveFolder.CheckedChanged += new System.EventHandler(radioGuiSaveFolder_CheckedChanged);
			resources.ApplyResources(checkAutoExportFolder, "checkAutoExportFolder");
			checkAutoExportFolder.Checked = true;
			checkAutoExportFolder.CheckState = System.Windows.Forms.CheckState.Checked;
			checkAutoExportFolder.Name = "checkAutoExportFolder";
			toolTip.SetToolTip(checkAutoExportFolder, resources.GetString("checkAutoExportFolder.ToolTip"));
			checkAutoExportFolder.UseVisualStyleBackColor = true;
			checkAutoExportFolder.CheckedChanged += new System.EventHandler(checkAutoExportFolder_CheckedChanged);
			resources.ApplyResources(radioZdataSaveFolder, "radioZdataSaveFolder");
			radioZdataSaveFolder.Name = "radioZdataSaveFolder";
			radioZdataSaveFolder.TabStop = true;
			toolTip.SetToolTip(radioZdataSaveFolder, resources.GetString("radioZdataSaveFolder.ToolTip"));
			radioZdataSaveFolder.UseVisualStyleBackColor = true;
			radioZdataSaveFolder.CheckedChanged += new System.EventHandler(radioZdataSaveFolder_CheckedChanged);
			groupZdataSelection.BackColor = System.Drawing.Color.Transparent;
			groupZdataSelection.Controls.Add(radioZdataSaveFolder);
			groupZdataSelection.Controls.Add(numericZdata);
			groupZdataSelection.Controls.Add(radioAutoZdata);
			groupZdataSelection.Controls.Add(radioSpecificZdata);
			resources.ApplyResources(groupZdataSelection, "groupZdataSelection");
			groupZdataSelection.Name = "groupZdataSelection";
			groupZdataSelection.TabStop = false;
			groupAllowSaving.BackColor = System.Drawing.Color.Transparent;
			groupAllowSaving.Controls.Add(checkSaveDb);
			groupAllowSaving.Controls.Add(checkSaveZdata);
			groupAllowSaving.Controls.Add(checkSaveGui);
			resources.ApplyResources(groupAllowSaving, "groupAllowSaving");
			groupAllowSaving.Name = "groupAllowSaving";
			groupAllowSaving.TabStop = false;
			groupGuiSaveOptions.BackColor = System.Drawing.Color.Transparent;
			groupGuiSaveOptions.Controls.Add(radioGuiSaveFolder);
			groupGuiSaveOptions.Controls.Add(radioGuiSaveArchive);
			resources.ApplyResources(groupGuiSaveOptions, "groupGuiSaveOptions");
			groupGuiSaveOptions.Name = "groupGuiSaveOptions";
			groupGuiSaveOptions.TabStop = false;
			groupExportFolde.BackColor = System.Drawing.Color.Transparent;
			groupExportFolde.Controls.Add(checkAutoExportFolder);
			groupExportFolde.Controls.Add(textExportFolder);
			groupExportFolde.Controls.Add(buttonBrowseExportFolder);
			resources.ApplyResources(groupExportFolde, "groupExportFolde");
			groupExportFolde.Name = "groupExportFolde";
			groupExportFolde.TabStop = false;
			buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(buttonCancel, "buttonCancel");
			buttonCancel.Name = "buttonCancel";
			buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(buttonOK, "buttonOK");
			buttonOK.Name = "buttonOK";
			buttonOK.UseVisualStyleBackColor = true;
			buttonOK.Click += new System.EventHandler(buttonOK_Click);
			optionsSet.DataSetName = "Options";
			optionsSet.Locale = new System.Globalization.CultureInfo("");
			optionsSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
			resources.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(buttonCancel);
			base.Controls.Add(buttonOK);
			base.Controls.Add(groupExportFolde);
			base.Controls.Add(groupGuiSaveOptions);
			base.Controls.Add(groupAllowSaving);
			base.Controls.Add(groupZdataSelection);
			base.Name = "UserOptions";
			base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(UserOptions_FormClosing);
			((System.ComponentModel.ISupportInitialize)numericZdata).EndInit();
			groupZdataSelection.ResumeLayout(false);
			groupZdataSelection.PerformLayout();
			groupAllowSaving.ResumeLayout(false);
			groupAllowSaving.PerformLayout();
			groupGuiSaveOptions.ResumeLayout(false);
			groupGuiSaveOptions.PerformLayout();
			groupExportFolde.ResumeLayout(false);
			groupExportFolde.PerformLayout();
			((System.ComponentModel.ISupportInitialize)optionsSet).EndInit();
			ResumeLayout(false);
		}
	}
}
