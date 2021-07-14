using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
//using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.Identity.Client;

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static string ClientId = "2b99df49-736b-4b7c-8e9d-bdaa87859b0b";
        //private static string ClientId = "f947871b-8962-4fbe-9444-45ad2ade761f";

        //private static string Tenant = "common";

        private static IPublicClientApplication _clientApp;

        public static IPublicClientApplication PublicClientApp { get { return _clientApp; } }

        public static Window MainEditorWindow;

        public static DiscordRPC.DiscordRpcClient DiscordRpcClient;

        public static TelemetryClient AppInsightClient = new TelemetryClient();

        public static string ApplicationDirectory
        {
            get
            {
                return System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\";
            }
        }

        public static string ProductVersion
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                return System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
            }
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);


            // _clientApp = PublicClientApplicationBuilder.Create(ClientId)
            // .WithAuthority(AzureCloudInstance.AzurePublic, "b72a5240-b495-45c8-8c18-fdad4089216e")
            // .WithRedirectUri("com.FIFAModding.Editor://oauth/redirect")
            // .Build();


            // --------------------------------------------------------------
            // Run the Powershell DLL
            UnblockAllDLL();

            // --------------------------------------------------------------
            // Discord Startup
            StartDiscordRPC();

            // --------------------------------------------------------------
            // Application Insights
            StartApplicationInsights();


            base.OnStartup(e);
        }

        private async void StartDiscordRPC()
        {
            DiscordRpcClient = new DiscordRPC.DiscordRpcClient("836520037208686652");
            DiscordRpcClient.Initialize();
            var presence = new DiscordRPC.RichPresence();
            presence.State = "In Main Menu";
            DiscordRpcClient.SetPresence(presence);
            DiscordRpcClient.Invoke();

            await Task.Delay(1000);
        }

        private async void StartApplicationInsights()
        {
            try
            {
                // Create a TelemetryConfiguration instance.
                TelemetryConfiguration config = TelemetryConfiguration.CreateDefault();
                config.InstrumentationKey = "92c621ff-61c0-43a2-a2d6-e539b359f053";
                //QuickPulseTelemetryProcessor quickPulseProcessor = null;
                //config.DefaultTelemetrySink.TelemetryProcessorChainBuilder
                //    .Use((next) =>
                //    {
                //        quickPulseProcessor = new QuickPulseTelemetryProcessor(next);
                //        return quickPulseProcessor;
                //    })
                //    .Build();

                //var quickPulseModule = new QuickPulseTelemetryModule();

                // Secure the control channel.
                // This is optional, but recommended.
                //quickPulseModule.AuthenticationApiKey = "YOUR-API-KEY-HERE";
                //quickPulseModule.Initialize(config);
                //quickPulseModule.RegisterTelemetryProcessor(quickPulseProcessor);

                // Create a TelemetryClient instance. It is important
                // to use the same TelemetryConfiguration here as the one
                // used to setup Live Metrics.
                AppInsightClient = new TelemetryClient(config);
            }
            catch (Exception ex)
            {

            }
            await Task.Delay(1000);
        }

        private async void UnblockAllDLL()
        {
            var loc = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
            var psCommmand = $"dir \"{loc}\" -Recurse|Unblock-File";
            var psCommandBytes = System.Text.Encoding.Unicode.GetBytes(psCommmand);
            var psCommandBase64 = Convert.ToBase64String(psCommandBytes);

            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy unrestricted -WindowStyle hidden -EncodedCommand {psCommandBase64}",
                UseShellExecute = true
            };
            startInfo.Verb = "runAs";
            Process.Start(startInfo);

            await Task.Delay(9000);

        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            foreach (var file in new DirectoryInfo(ApplicationDirectory).GetFiles())
            {
                if(file.Name.Contains("temp_") && file.Name.Contains(".DDS"))
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {

                    }
                }

                if(file.Name.Contains("autosave", StringComparison.OrdinalIgnoreCase))
                {
                    if(file.CreationTime < DateTime.Now.AddDays(-2))
                    {
                        file.Delete();
                    }
                }
            }
        }

        private void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            MessageBoxResult result = MessageBox.Show(e.ToString());

            if (File.Exists("ErrorLogging.txt"))
                File.Delete("ErrorLogging.txt");

            using (StreamWriter stream = new StreamWriter("ErrorLogging.txt"))
            {
                stream.WriteLine(e.ToString());
            }

            Trace.WriteLine(e.ToString());
            Console.WriteLine(e.ToString());
            Debug.WriteLine(e.ToString());

        }
    }
}
