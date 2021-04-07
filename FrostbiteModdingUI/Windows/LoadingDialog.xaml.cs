using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FrostbiteModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for LoadingDialog.xaml
    /// </summary>
    public partial class LoadingDialog : Window
    {
        public LoadingDialog()
        {
            InitializeComponent();
            pbar.Value = 0;
        }

        public LoadingDialog(string loadingSubTitle, string loadingCurrentMessage)
        {
            InitializeComponent();
            lblLoadingSubtitle.Content = loadingSubTitle;
            lblProgress.Content = loadingCurrentMessage;
            pbar.Value = 0;
        }

        public void Update(int progress)
        {
            Dispatcher.Invoke(() => {

                pbar.Value = progress;

            });
        }

        public void Update(string loadingSubTitle, string loadingCurrentMessage, int progress)
        {
            Dispatcher.Invoke(() => {

                lblLoadingSubtitle.Content = loadingSubTitle;
                lblProgress.Content = loadingCurrentMessage;
                pbar.Value = progress;

            });
        }
    }
}
