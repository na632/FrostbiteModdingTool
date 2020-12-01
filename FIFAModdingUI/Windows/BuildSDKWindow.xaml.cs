using FrostySdk.Frostbite;
using FrostySdk.Interfaces;
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
using v2k4FIFAModdingCL;

namespace FrostbiteModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for BuildSDK.xaml
    /// </summary>
    public partial class BuildSDK : Window, ILogger
    {
        public BuildSDK()
        {
            InitializeComponent();
        }

        public void Log(string text, params object[] vars)
        {
            LogAsync(text);
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

        public void LogError(string text, params object[] vars)
        {
        }

        public void LogWarning(string text, params object[] vars)
        {
        }

        private async void btnBuildSDK_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => { btnBuildSDK.IsEnabled = false; });
            var buildCache = new BuildCache();
            buildCache.LoadData(GameInstanceSingleton.GAMEVERSION, GameInstanceSingleton.GAMERootPath, this, false);

            var buildSDK = new v2k4FIFASDKGenerator.BuildSDK();
            buildSDK.Logger = this;
            await buildSDK.Build();
            await Dispatcher.InvokeAsync(() => { btnBuildSDK.IsEnabled = true; });
        }
    }
}
