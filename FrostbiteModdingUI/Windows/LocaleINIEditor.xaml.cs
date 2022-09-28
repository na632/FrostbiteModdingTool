using FrostySdk;
using FrostySdk.Managers;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using v2k4FIFAModdingCL;

namespace FMT.Windows
{
    /// <summary>
    /// Interaction logic for LocaleINIEditor.xaml
    /// </summary>
    public partial class LocaleINIEditor : MetroWindow
    {
        public LocaleINIEditor()
        {
            InitializeComponent();
            var d = Data;

            this.DataContext = null;
            this.DataContext = this;
        }

        private string data;
        public string Data
        {
            get
            {
                if (data == null)
                    data = Encoding.UTF8.GetString(AssetManager.Instance.LocaleINIMod.OriginalData);

                if(AssetManager.Instance.LocaleINIMod.UserData != null && AssetManager.Instance.LocaleINIMod.UserData.Length > 0)
                    data = Encoding.UTF8.GetString(AssetManager.Instance.LocaleINIMod.UserData);

                return data;
            }
            set
            {
                data = value;
                AssetManager.Instance.LocaleINIMod.UserData = Encoding.UTF8.GetBytes(value);
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {

            base.OnContentRendered(e);

        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            AssetManager.Instance.LocaleINIMod = new FrostySdk.Frostbite.IO.LocaleINIMod();
            this.DataContext = null;
            this.DataContext = this;
        }
    }
}
