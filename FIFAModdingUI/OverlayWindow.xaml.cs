using FifaLibrary;
using Memory;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using v2k4FIFAModding.Career;
using v2k4FIFAModding.Career.CME.FIFA;
using v2k4FIFAModdingCL;
using v2k4FIFAModdingCL.MemHack.Career;
using v2k4FIFAModdingCL.MemHack.Core;

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class OverlayWindow : Window
    {
        public const string WINDOW_NAME = "FIFA 20";
        IntPtr FIFAWindowHandle = FindWindow(null, WINDOW_NAME);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public struct RECT
        {
            public int left, top, right, bottom;
        }
        RECT rect;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        HwndSource hwndSource;
        CoreHack coreHack = new CoreHack();

        MainWindow ParentWindow;

        public OverlayWindow(MainWindow parentWindow)
        {
            InitializeComponent();

            ParentWindow = parentWindow;
            ParentWindow.HasCareerExpansionModBeenUsed = true;

            hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            //IntPtr hwnd = hwndSource.Handle;
            // OR 
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            this.Topmost = true;

            int initialStyle = GetWindowLong(hwnd, -20);
            SetWindowLong(hwnd, -20, initialStyle | 0x80000 | 0x20);

            GetWindowRect(FIFAWindowHandle, out rect);
            this.Left = rect.left;
            this.Top = rect.top;
            this.Width = rect.right - rect.left;
            this.Height = rect.bottom - rect.top;

            if(CoreHack.GetProcess(out Mem MemLib) != 0)
            {
                hookedtext.Visibility = Visibility.Visible;
                TabCareer.IsEnabled = true; MainViewer.IsEnabled = true;
            }

            //txtLoadedCareerSave.Text = coreHack.SaveName;
            GetCurrentGameDate();
            coreHack.GameDateHasChanged += GameDateChanged;

            Finance_StartingBudget.Text = Finances.GetTransferBudget().ToString();
            Finance_TransferBudget.Text = Finances.GetTransferBudget().ToString();


            var myDocs = SpecialDirectories.MyDocuments + "\\"
                                            + FIFAInstanceSingleton.FIFAVERSION.Substring(0, 4) + " " + FIFAInstanceSingleton.FIFAVERSION.Substring(4, 2)
                                            + "\\settings\\";
            Dictionary<string, string> results = CareerUtil.GetCareerSaves(myDocs);

            CareerSaves.ItemsSource = results;
        }

        private void GetCurrentGameDate()
        {
            Dispatcher.Invoke(() => {
                CoreHack coreHack = new CoreHack();
                txtGameDate.Text = coreHack.GameDate.ToShortDateString();
            });
        }

        private void GameDateChanged(object s, EventArgs e)
        {
            GetCurrentGameDate();
        }

    private void btnRequestAdditionalFunds_Click(object sender, RoutedEventArgs e)
        {
            var finances = new Finances();
            if(finances.RequestAdditionalFunds(out string message))
            {
                //txtRequestFundsAnswer.Text = "Yes here we go!";
                btnRequestAdditionalFunds.Background = Brushes.Green;
            }
            else
            { 
                //txtRequestFundsAnswer.Text = "Nope!";
                btnRequestAdditionalFunds.Background = Brushes.Red;
            }

            new TaskFactory().StartNew(async () => 
                { 
                    await System.Threading.Tasks.Task.Delay(1000); 
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        btnRequestAdditionalFunds.Background = Brushes.White;
                });
            });

        }

        private void CareerSaves_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            if (item != null)
            {
                var iFile = item.Content.ToString().Replace("{", "").Replace("[", "").Split(',')[0];
                LoadCareerFile(iFile);
            }
        }

        private void LoadCareerFile(string filePath)
        {
            new TaskFactory().StartNew(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    LoadingProgressBar.Value = 0;
                });
                var careerFile = new CareerFile(filePath, @"C:\Program Files (x86)\RDBM 20\RDBM20\Templates\FIFA 20\fifa_ng_db-meta.XML");
                Dispatcher.Invoke(() =>
                {
                    LoadingProgressBar.Value = 10;
                });

                List<DataSet> dataSets = new List<DataSet>();
                if (careerFile != null)
                {
                    int indexOfFiles = 0;
                    foreach (var dbf in careerFile.Databases.Where(x => x != null))
                    {
                        var convertedDS = dbf.ConvertToDataSet();
                        string json = JsonConvert.SerializeObject(convertedDS, Formatting.Indented);
                        File.WriteAllText(careerFile.InGameName + "_" + indexOfFiles + ".json", json);
                        indexOfFiles++;
                        dataSets.Add(convertedDS);
                        Dispatcher.Invoke(() =>
                        {
                            LoadingProgressBar.Value += 20;
                        });
                    }
                }
                if (dataSets.Count > 0)
                {
                    var d = JsonConvert.DeserializeObject<CareerDB2>(File.ReadAllText(careerFile.InGameName + "_1.json"));

                    Dispatcher.Invoke(() =>
                    {
                        TabFinance.IsEnabled = true;
                        TabYouth.IsEnabled = true;
                        txtLoadedCareerSave.Text = careerFile.InGameName;

                        LoadingProgressBar.Value = 100;
                    });

                }
            });
        }

        private void btnCloseCareerExpansionMod_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //private double OpacityAmount = 0.75d;


        //private void Window_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if(e.Key == Key.F2)
        //    {
        //        if (WindowOpacityColor.Opacity == OpacityAmount)
        //        {
        //            WindowOpacityColor.Opacity = 0;
        //            GridOfTiles.Opacity = 0;
        //        }
        //        else
        //        {
        //            WindowOpacityColor.Opacity = OpacityAmount;
        //            GridOfTiles.Opacity = 1;
        //        }
        //    }
        //}
    }
}
