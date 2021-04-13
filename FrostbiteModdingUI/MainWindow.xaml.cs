using FIFAModdingUI.Windows;
using FrostbiteModdingUI.Windows;
using FrostySdk;
using FrostySdk.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using static FrostySdk.ProfilesLibrary;

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Window> EditorWindows = new List<Window>();

        public List<Profile> ProfilesWithEditorScreen = new List<Profile>();

        public string WindowTitle { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly();
            WindowTitle = "Frostbite Modding Tool " + System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

            // ------------------------------------------
            // This is unfinished. The plugins need to be loaded to find any of the editor windows to load them dynamically

            //foreach (var profile in ProfilesLibrary.AvailableProfiles.Where(x=>x.EditorScreen != null))
            //{
            //    foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            //    {
            //        var t = a.GetTypes().FirstOrDefault(x => x.Name.Contains(profile.EditorScreen, StringComparison.OrdinalIgnoreCase));
            //        if (t != null)
            //        {
            //            //var ew = (Window)Activator.CreateInstance(t);
            //            //EditorWindows.Add(ew);
            //            ProfilesWithEditorScreen.Add(profile);
            //        }
            //    }
            //}

            // This is unfinished. The plugins need to be loaded to find any of the editor windows to load them dynamically
            DataContext = this;

            // ------------------------------------------


        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            foreach (var w in EditorWindows)
            {
                if (w != null)
                    w.Close();
            }

            EditorWindows.Clear();

            foreach (var pr in ProfilesWithEditorScreen)
            {

            }

            ProfilesWithEditorScreen.Clear();

            if (App.MainEditorWindow != null)
                App.MainEditorWindow.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (App.MainEditorWindow != null)
                App.MainEditorWindow.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (App.MainEditorWindow != null)
                App.MainEditorWindow.Close();

            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditor_Click(object sender, RoutedEventArgs e)
        {
            //new EditorLoginWindow().Show();
            App.MainEditorWindow = new FIFA21Editor(this);
            if(App.MainEditorWindow != null)
                App.MainEditorWindow.Show();

            this.Visibility = Visibility.Hidden;
        }

        private void btnMadden21Editor_Click(object sender, RoutedEventArgs e)
        {
            App.MainEditorWindow = new Madden21Editor(this);
            App.MainEditorWindow.Show();
            this.Visibility = Visibility.Hidden;
        }

        private void btnLauncher_Click(object sender, RoutedEventArgs e)
        {
            new LaunchWindow(this).Show();
            this.Visibility = Visibility.Hidden;
        }

      
    }
}
