using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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

        private static string Tenant = "common";

        private static IPublicClientApplication _clientApp;

        public static IPublicClientApplication PublicClientApp { get { return _clientApp; } }

        public static EditorWindow MainEditorWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);


            _clientApp = PublicClientApplicationBuilder.Create(ClientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, "b72a5240-b495-45c8-8c18-fdad4089216e")
            .WithRedirectUri("com.FIFAModding.Editor://oauth/redirect")
            .Build();

            base.OnStartup(e);
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
