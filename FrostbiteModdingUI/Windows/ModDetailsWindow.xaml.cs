using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using v2k4FIFAModding.Frosty;

namespace FrostbiteModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for ModDetailsWindow.xaml
    /// </summary>
    public partial class ModDetailsWindow : Window
    {
        public ModDetailsWindow()
        {
            InitializeComponent();
            this.DataContext = ProjectManagement.Instance.FrostyProject.ModSettings;
        }

        private void SaveAndClose_Click(object sender, RoutedEventArgs e)
        {
            ProjectManagement.Instance.FrostyProject.Save();
            this.Close();
        }
    }
}
