using FIFAModdingUI.Windows;
using FrostbiteModdingUI.Windows;
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

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditor_Click(object sender, RoutedEventArgs e)
        {
            //new EditorLoginWindow().Show();
            App.MainEditorWindow = new FIFA21Editor();
            App.MainEditorWindow.Show();
            this.Close();
        }

        private void btnLegacyEditor_Click(object sender, RoutedEventArgs e)
        {
            new LegacyModEditor().Show();
            this.Close();
        }

        private void btnLauncher_Click(object sender, RoutedEventArgs e)
        {
            new LaunchWindow().Show();
            this.Close();
        }

        private void btnMadden21Editor_Click(object sender, RoutedEventArgs e)
        {
            App.MainEditorWindow = new Madden21Editor();
            App.MainEditorWindow.Show();
            this.Close();

        }
    }
}
