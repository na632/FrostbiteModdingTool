using FrostyEditor.IO;
using FrostyEditor.Windows;
using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using SdkGenerator;
using static Frosty.OpenFrostyFiles;

namespace FIFAModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for BuildSDKAndCache.xaml
    /// </summary>
    public partial class BuildSDKAndCache : Window, ILogger
    {
        public BuildSDKAndCache()
        {
            InitializeComponent();


        }

        public void Log(string text, params object[] vars)
        {
            Dispatcher.Invoke(() =>
            {
                txtOuputMessage.Text = text;
            });
        }

        public void LogError(string text, params object[] vars)
        {
            Dispatcher.Invoke(() =>
            {
                txtOuputMessage.Text = text;
            });
        }

        public void LogWarning(string text, params object[] vars)
        {
            Dispatcher.Invoke(() =>
            {
                txtOuputMessage.Text = text;
            });
        }

        private async void btnRunBuildCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var buildCache = new BuildCache();
                await buildCache.LoadDataAsync(GameInstanceSingleton.GAMEVERSION, GameInstanceSingleton.GAMERootPath, this, true);
            }
            catch(Exception ex)
            {
                txtOuputMessage.Text = "There was an error during the cache building process";
                File.WriteAllText("log_cache_error.txt", ex.ToString());
            }
        }

        private async void btnRunBuild_Click(object sender, RoutedEventArgs e)
        {
			Dispatcher.Invoke(() => btnRunBuild.IsEnabled = false);

			var buildSDK = new BuildSDK();
            await buildSDK.Build();
			
			Dispatcher.Invoke(() => btnRunBuild.IsEnabled = true);
		}

      
    }
}
