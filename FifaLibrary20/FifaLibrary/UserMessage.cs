using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary
{
	public class UserMessage : Form
	{
		private int m_CurrentIndex;

		private string m_XmlFileName;

		private IContainer components;

		private Button buttonOK;

		private TextBox textMessage;

		private CheckBox checkSuppressMessage;

		private TextBox textErrorNumber;

		private Messages setMessages;

		private Button buttonCancel;

		private Button buttonNo;

		private Button buttonYes;

		private PictureBox pictureBox;

		private ImageList imageList;

		public UserMessage()
		{
			InitializeComponent();
			string currentDirectory = Environment.CurrentDirectory;
			string str = CultureInfo.CurrentUICulture.Name.Substring(0, 2);
			string text = currentDirectory + "\\" + str;
			m_XmlFileName = null;
			if (Directory.Exists(text))
			{
				string text2 = text + "\\Messages.xml";
				if (File.Exists(text2))
				{
					m_XmlFileName = text2;
				}
			}
			if (m_XmlFileName == null)
			{
				m_XmlFileName = currentDirectory + "\\Messages.xml";
			}
			if (File.Exists(m_XmlFileName))
			{
				setMessages.ReadXml(m_XmlFileName);
			}
		}

		public DialogResult ShowMessage(int id)
		{
			string text = null;
			bool flag = true;
			for (int i = 0; i < setMessages.DataTableMex.Count; i++)
			{
				if (id == setMessages.DataTableMex[i].MexId)
				{
					text = setMessages.DataTableMex[i].MexText;
					flag = !setMessages.DataTableMex[i].MexSuppressed;
					m_CurrentIndex = i;
					break;
				}
			}
			textMessage.Text = text;
			if (flag)
			{
				SetUpLook(id);
				return ShowDialog();
			}
			return DialogResult.OK;
		}

		public DialogResult ShowMessage(int id, int reference)
		{
			string str = null;
			bool flag = true;
			for (int i = 0; i < setMessages.DataTableMex.Count; i++)
			{
				if (id == setMessages.DataTableMex[i].MexId)
				{
					str = setMessages.DataTableMex[i].MexText;
					flag = !setMessages.DataTableMex[i].MexSuppressed;
					m_CurrentIndex = i;
					break;
				}
			}
			textMessage.Text = str + " Reference: " + reference.ToString();
			if (flag)
			{
				SetUpLook(id);
				return ShowDialog();
			}
			return DialogResult.OK;
		}

		private void SetUpLook(int id)
		{
			if (id < 1000)
			{
				textErrorNumber.Text = "Please select your choice";
				pictureBox.Image = imageList.Images[3];
			}
			else if (id < 3000)
			{
				textErrorNumber.Text = "Warning: " + id.ToString();
				pictureBox.Image = imageList.Images[1];
			}
			else if (id < 5000)
			{
				textErrorNumber.Text = "Info: " + id.ToString();
				pictureBox.Image = imageList.Images[2];
			}
			else if (id < 15000)
			{
				textErrorNumber.Text = "Error: " + id.ToString();
				pictureBox.Image = imageList.Images[0];
			}
			else
			{
				textErrorNumber.Text = "Info";
				pictureBox.Image = imageList.Images[2];
			}
			checkSuppressMessage.Visible = (id < 10000);
			checkSuppressMessage.Checked = false;
			buttonOK.Visible = (id >= 1000);
			buttonNo.Visible = (id < 1000);
			buttonYes.Visible = (id < 1000);
			buttonCancel.Visible = (id < 1000);
		}

		public DialogResult ShowMessage(int id, string messageText)
		{
			bool flag = true;
			for (int i = 0; i < setMessages.DataTableMex.Count; i++)
			{
				if (id == setMessages.DataTableMex[i].MexId)
				{
					_ = setMessages.DataTableMex[i].MexText;
					flag = !setMessages.DataTableMex[i].MexSuppressed;
					m_CurrentIndex = i;
					break;
				}
			}
			textMessage.Text = messageText;
			if (flag)
			{
				SetUpLook(id);
				return ShowDialog();
			}
			return DialogResult.OK;
		}

		public DialogResult ShowMessage(int id, string messageText, bool merge)
		{
			if (!merge)
			{
				return ShowMessage(id, messageText);
			}
			string str = null;
			for (int i = 0; i < setMessages.DataTableMex.Count; i++)
			{
				if (id == setMessages.DataTableMex[i].MexId)
				{
					str = setMessages.DataTableMex[i].MexText;
					m_CurrentIndex = i;
					break;
				}
			}
			return ShowMessage(id, str + "\r\n" + messageText);
		}

		public void EnableMessages(bool enable)
		{
			for (int i = 0; i < setMessages.DataTableMex.Count; i++)
			{
				if (setMessages.DataTableMex[i].MexId < 10000)
				{
					setMessages.DataTableMex[i].MexSuppressed = !enable;
				}
			}
			setMessages.WriteXml(m_XmlFileName);
		}

		public void EnableWarnings(bool enable)
		{
			for (int i = 0; i < setMessages.DataTableMex.Count; i++)
			{
				if (setMessages.DataTableMex[i].MexId < 5000)
				{
					setMessages.DataTableMex[i].MexSuppressed = !enable;
				}
			}
			setMessages.WriteXml(m_XmlFileName);
		}

		public void EnableErrors(bool enable)
		{
			for (int i = 0; i < setMessages.DataTableMex.Count; i++)
			{
				if (setMessages.DataTableMex[i].MexId >= 5000 && setMessages.DataTableMex[i].MexId < 10000)
				{
					setMessages.DataTableMex[i].MexSuppressed = !enable;
				}
			}
			setMessages.WriteXml(m_XmlFileName);
		}

		private void checkSuppressMessage_CheckedChanged(object sender, EventArgs e)
		{
		}

		private void buttonOK_Click(object sender, EventArgs e)
		{
			if (checkSuppressMessage.Checked)
			{
				setMessages.DataTableMex[m_CurrentIndex].MexSuppressed = true;
			}
			setMessages.WriteXml(m_XmlFileName);
		}

		private void buttonYes_Click(object sender, EventArgs e)
		{
			if (checkSuppressMessage.Checked)
			{
				setMessages.DataTableMex[m_CurrentIndex].MexSuppressed = true;
			}
			setMessages.WriteXml(m_XmlFileName);
		}

		private void buttonNo_Click(object sender, EventArgs e)
		{
			if (checkSuppressMessage.Checked)
			{
				setMessages.DataTableMex[m_CurrentIndex].MexSuppressed = true;
			}
			setMessages.WriteXml(m_XmlFileName);
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FifaLibrary.UserMessage));
			buttonOK = new System.Windows.Forms.Button();
			textMessage = new System.Windows.Forms.TextBox();
			checkSuppressMessage = new System.Windows.Forms.CheckBox();
			textErrorNumber = new System.Windows.Forms.TextBox();
			buttonCancel = new System.Windows.Forms.Button();
			buttonNo = new System.Windows.Forms.Button();
			buttonYes = new System.Windows.Forms.Button();
			pictureBox = new System.Windows.Forms.PictureBox();
			imageList = new System.Windows.Forms.ImageList(components);
			setMessages = new FifaLibrary.Messages();
			((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
			((System.ComponentModel.ISupportInitialize)setMessages).BeginInit();
			SuspendLayout();
			buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			resources.ApplyResources(buttonOK, "buttonOK");
			buttonOK.Name = "buttonOK";
			buttonOK.UseVisualStyleBackColor = true;
			buttonOK.Click += new System.EventHandler(buttonOK_Click);
			textMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
			resources.ApplyResources(textMessage, "textMessage");
			textMessage.Name = "textMessage";
			textMessage.ReadOnly = true;
			resources.ApplyResources(checkSuppressMessage, "checkSuppressMessage");
			checkSuppressMessage.Name = "checkSuppressMessage";
			checkSuppressMessage.UseVisualStyleBackColor = true;
			checkSuppressMessage.CheckedChanged += new System.EventHandler(checkSuppressMessage_CheckedChanged);
			resources.ApplyResources(textErrorNumber, "textErrorNumber");
			textErrorNumber.Name = "textErrorNumber";
			textErrorNumber.ReadOnly = true;
			buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			resources.ApplyResources(buttonCancel, "buttonCancel");
			buttonCancel.Name = "buttonCancel";
			buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
			resources.ApplyResources(buttonNo, "buttonNo");
			buttonNo.Name = "buttonNo";
			buttonNo.Click += new System.EventHandler(buttonNo_Click);
			buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
			resources.ApplyResources(buttonYes, "buttonYes");
			buttonYes.Name = "buttonYes";
			buttonYes.Click += new System.EventHandler(buttonYes_Click);
			resources.ApplyResources(pictureBox, "pictureBox");
			pictureBox.Name = "pictureBox";
			pictureBox.TabStop = false;
			imageList.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageList.ImageStream");
			imageList.TransparentColor = System.Drawing.Color.Fuchsia;
			imageList.Images.SetKeyName(0, "Error_16.PNG");
			imageList.Images.SetKeyName(1, "Warning_16.PNG");
			imageList.Images.SetKeyName(2, "Info_16.PNG");
			imageList.Images.SetKeyName(3, "Help.PNG");
			setMessages.DataSetName = "Messages";
			setMessages.Locale = new System.Globalization.CultureInfo("");
			setMessages.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
			resources.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(pictureBox);
			base.Controls.Add(buttonCancel);
			base.Controls.Add(buttonNo);
			base.Controls.Add(buttonYes);
			base.Controls.Add(textErrorNumber);
			base.Controls.Add(checkSuppressMessage);
			base.Controls.Add(textMessage);
			base.Controls.Add(buttonOK);
			base.Name = "UserMessage";
			((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
			((System.ComponentModel.ISupportInitialize)setMessages).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
