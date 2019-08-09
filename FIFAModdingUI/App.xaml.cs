using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            base.OnStartup(e);
        }

        private void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            if (File.Exists("ErrorLogging.txt"))
                File.Delete("ErrorLogging.txt");

            using (StreamWriter stream = new StreamWriter("ErrorLogging.txt"))
            {
                Exception e = (Exception)args.ExceptionObject;
                stream.WriteLine(e.ToString());
            }
        }
    }
}
