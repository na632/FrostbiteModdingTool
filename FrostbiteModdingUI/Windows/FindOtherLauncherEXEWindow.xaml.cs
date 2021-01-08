using Microsoft.Win32;
using paulv2k4ModdingExecuter;
using System;
using System.Collections.Generic;
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
using v2k4FIFAModdingCL;

namespace FrostbiteModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for FindOtherLauncherEXEWindow.xaml
    /// </summary>
    public partial class FindOtherLauncherEXEWindow : Window
    {
        public FindOtherLauncherEXEWindow()
        {
            InitializeComponent();
        }

        public bool InjectLegacyModSupport { get; set; }
        public bool InjectLiveEditorSupport { get; set; }

        public static string OtherToolPath { get; set; }
        
        private void btnFindOtherTool_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "EXE files (*.exe)|*.exe";
            var ok = openFileDialog.ShowDialog();
            if(ok.HasValue && ok.Value)
            {
                OtherToolPath = openFileDialog.FileName;
                txtToolLocation.Text = OtherToolPath;
                btnLaunchOtherTool.IsEnabled = true;
            }
        }
        private void btnLaunchOtherTool_Click(object sender, RoutedEventArgs e)
        {
            FrostyModExecutor frostyModExecutor = new FrostyModExecutor();
            frostyModExecutor.ExecuteProcess(OtherToolPath, "");

            Task.Run(() =>
            {
                if (InjectLegacyModSupport)
                {
                    string legacyModSupportFile = null;
                    if (GameInstanceSingleton.GAMEVERSION == "FIFA20")
                    {
                        legacyModSupportFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA20Legacy.dll";
                    }
                    else if (GameInstanceSingleton.GAMEVERSION == "FIFA21")
                    {
                        legacyModSupportFile = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\FIFA.dll";
                    }

                    if (!string.IsNullOrEmpty(legacyModSupportFile))
                    {

                        if (File.Exists(legacyModSupportFile))
                        {
                            File.Copy(legacyModSupportFile, @GameInstanceSingleton.GAMERootPath + "v2k4LegacyModSupport.dll", true);
                        }

                        var legmodsupportdllpath = @GameInstanceSingleton.GAMERootPath + @"v2k4LegacyModSupport.dll";
                        try
                        {
                            LogAsync("Injecting Live Legacy Mod Support");
                            GameInstanceSingleton.InjectDLLAsync(legmodsupportdllpath);
                            LogAsync("Injected Live Legacy Mod Support");

                        }
                        catch (Exception InjectDLLException)
                        {
                            LogAsync("Launcher could not inject Live Legacy File Support");
                            LogAsync(InjectDLLException.ToString());
                        }
                    }

                    if (InjectLiveEditorSupport)
                    {
                        LogAsync("Injecting Aranaktu's Live Editor");
                        if (File.Exists(@GameInstanceSingleton.GAMERootPath + @"FIFALiveEditor.DLL"))
                        {
                            GameInstanceSingleton.InjectDLLAsync(@GameInstanceSingleton.GAMERootPath + @"FIFALiveEditor.DLL");
                            LogAsync("Injected Aranaktu's Live Editor");
                        }
                    }
                }
            });



        }
        public async void LogAsync(string in_text)
        {
            var txt = string.Empty;
            Dispatcher.Invoke(() => {
                txt = txtLog.Text;
            });

            var text = await Task.Run(() =>
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append(txt);
                stringBuilder.AppendLine(in_text);

                return stringBuilder.ToString();
            });

            await Dispatcher.InvokeAsync(() =>
            {
                txtLog.Text = text;
                txtLog.ScrollToEnd();
            });

        }


    }
}
