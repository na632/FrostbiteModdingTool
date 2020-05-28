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
using Xamarin.Forms;

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for EditorLoginWindow.xaml
    /// </summary>
    public partial class EditorLoginWindow : Window
    {
        public EditorLoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            new EditorWindow().Show();
            this.Close();
        }
    }
}
