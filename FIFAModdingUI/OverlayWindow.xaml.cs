using Memory;
using System;
using System.Collections.Generic;
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


        public OverlayWindow()
        {
            InitializeComponent();

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

            txtLoadedCareerSave.Text = coreHack.SaveName;
            GetCurrentGameDate();
            coreHack.GameDateHasChanged += GameDateChanged;

            Finance_StartingBudget.Text = Finances.GetTransferBudget().ToString();
            Finance_TransferBudget.Text = Finances.GetTransferBudget().ToString();
            
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
                    await Task.Delay(1000); 
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        btnRequestAdditionalFunds.Background = Brushes.White;
                });
            });

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
