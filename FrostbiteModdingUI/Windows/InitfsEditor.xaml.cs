using FMT.Pages.Common;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace FMT.Windows
{
    /// <summary>
    /// Interaction logic for InitfsEditor.xaml
    /// </summary>
    public partial class InitfsEditor : MetroWindow
    {
        public InitfsEditor()
        {
            InitializeComponent();
            this.Content = new BrowserOfInitfs();
        }
    }
}
