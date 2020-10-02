using FrostySdk;
using Modding_UI_2021.Forms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FrostySdk.ProfilesLibrary;

namespace Modding_UI_2021
{
    public partial class StartForm : Form
    {
        public StartForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            var allFiles = Directory.GetFiles(Application.StartupPath);
            //foreach(var f in Directory.EnumerateFiles("/").Where(x=>x.Contains("Profile.json")))
            foreach (var f in allFiles.Where(x=>x.Contains("Profile.json")))
            {
                Profile profile = default(Profile);
                profile = JsonConvert.DeserializeObject<Profile>(File.ReadAllText(f));

                Button button = new Button();
                button.Text = profile.Name;
                button.Enabled = false;
                button.Width = 100;
                if (profile.Name == "MADDEN21" || profile.Name == "FIFA21")
                {
                    button.Click += Button_Click;
                    button.Enabled = true;
                }
                flowLayoutPanel1.Controls.Add(button);
            }
        }

        Form CurrentEditor;

        private void Button_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if(button != null)
            {
                switch(button.Text)
                {
                    case "MADDEN21":
                        CurrentEditor = new Madden21Editor();
                        //this.Close();
                        break;
                    case "FIFA21":

                        break;
                }

                CurrentEditor.Show();

            }
        }
    }
}
