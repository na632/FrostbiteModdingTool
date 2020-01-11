using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        [DllImport("FIFACareerDLL.dll")]
        static extern bool RequestAdditionalFunds_OUT();

        HwndSource hwndSource;
        

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
            ////this.RenderSize = new Size(rect.right - rect.left, rect.bottom - rect.top);
            this.Left = 0;
            this.Top = 0;
            this.Width = rect.right - rect.left;
            this.Height = rect.bottom - rect.top;

            if(this.Width != 0 && this.Height != 0)
            {

            }
        }

        private void btnRequestAdditionalFunds_Click(object sender, RoutedEventArgs e)
        {
            if(RequestAdditionalFunds_OUT())
            {
                txtRequestFundsAnswer.Text = "Yes here we go!";
            }
            else
            {
                txtRequestFundsAnswer.Text = "Nope!";
            }
        }
    }
}
