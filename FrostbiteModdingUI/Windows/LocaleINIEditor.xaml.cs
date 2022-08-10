using FrostySdk;
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
    public partial class LocaleINIEditor : Window
    {
        public LocaleINIEditor()
        {
            InitializeComponent();
        }

        private string data;
        public string Data
        {
            get
            {
                if (data == null)
                    data = UTF8Encoding.UTF8.GetString(FileSystem.Instance.LoadLocaleINI());

                return data;
            }
            set
            {
                data = value;
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            

        }
    }
}
