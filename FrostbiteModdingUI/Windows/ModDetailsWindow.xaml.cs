using System.Windows;
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
            this.DataContext = ProjectManagement.Instance.Project.ModSettings;
        }

        private void SaveAndClose_Click(object sender, RoutedEventArgs e)
        {
            ProjectManagement.Instance.Project.Save();
            this.Close();
        }
    }
}
