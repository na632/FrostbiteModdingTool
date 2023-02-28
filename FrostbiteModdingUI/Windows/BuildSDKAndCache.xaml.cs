using FrostySdk;
using FrostySdk.Frostbite;
using FrostySdk.Interfaces;
using FrostySdk.Managers;
using MahApps.Metro.Controls;
using SdkGenerator;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using v2k4FIFAModdingCL;

namespace FIFAModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for BuildSDKAndCache.xaml
    /// </summary>
    public partial class BuildSDKAndCache : MetroWindow, ILogger, IDisposable
    {

        private CacheManager buildCache = new CacheManager();
        public bool DoNotAutoRebuild = false;

        public BuildSDKAndCache()
        {
            InitializeComponent();

            Loaded += BuildSDKAndCache_Loaded;
        }

        //protected override void OnContentRendered(EventArgs e)
        //{
        //    base.OnContentRendered(e);

        //    if(!DoNotAutoRebuild)
        //        Rebuild();
        //}

        private async void BuildSDKAndCache_Loaded(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                if (!DoNotAutoRebuild)
                    await Rebuild();
            }
        }


        public async Task Rebuild(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CacheManager.DoesCacheNeedsRebuilding())
            {
                lblCacheNeedsRebuilding.Visibility = Visibility.Visible;
                btnRunBuildCache.IsEnabled = false;
                btnRunBuild.IsEnabled = false;

                await Task.Delay(1000);

                await Dispatcher.InvokeAsync(() => { txtOuputMessage.Content = "Building Cache. Please wait 3-15 minutes to complete!"; });

                if (FileSystem.Instance == null)
                    new FileSystem(GameInstanceSingleton.Instance.GAMERootPath);
                // -----------------------------------------
                //
                await buildCache.LoadDataAsync(GameInstanceSingleton.Instance.GAMEVERSION, FileSystem.Instance.BasePath, this, true, false);

                await Dispatcher.InvokeAsync(() => { txtOuputMessage.Content = "Building SDK. Please wait 1-2 minutes to complete!"; });

                // -----------------------------------------
                // finish off with the SDK
                await Dispatcher.InvokeAsync(() => { txtOuputMessage.Content = "Building Cache EBX Indexing. Please wait 1-3 minutes to complete!"; });

                AssetManager.Instance.FullReset();
                AssetManager.Instance.Dispose();
                AssetManager.Instance = null;

                await buildCache.LoadDataAsync(GameInstanceSingleton.Instance.GAMEVERSION, GameInstanceSingleton.Instance.GAMERootPath, this, false, true);

                await Task.Delay(2000);

            }

            DialogResult = true;
            this.Close();
        }

        public void Dispose()
        {
        }



        public string LogText = string.Empty;

        public void Log(string text, params object[] vars)
        {
            //if (LastMessage == text)
            //    return;

            //LastMessage = text;

            //Dispatcher.Invoke(() => { txtOutputSubMessage.Content = text; });
            //txtOutputSubMessage.Content = text;
            LogAsync(text);
        }

        private string LastMessage = string.Empty;

        public async Task LogAsync(string text)
        {
            if (LastMessage == text)
                return;

            LastMessage = text;

            await Dispatcher.InvokeAsync(() => { txtOutputSubMessage.Content = text; });

        }

        public void LogWarning(string text, params object[] vars)
        {
            Debug.WriteLine("[WARNING] " + text);
            LogAsync("[WARNING] " + text);
        }

        public void LogError(string text, params object[] vars)
        {
            Debug.WriteLine("[ERROR] " + text);
            LogAsync("[ERROR] " + text);
        }


        private async void btnRunBuildCache_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await buildCache.LoadDataAsync(GameInstanceSingleton.Instance.GAMEVERSION, GameInstanceSingleton.Instance.GAMERootPath, this, true);
            }
            catch (Exception ex)
            {
                txtOuputMessage.Content = "There was an error during the cache building process";
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
