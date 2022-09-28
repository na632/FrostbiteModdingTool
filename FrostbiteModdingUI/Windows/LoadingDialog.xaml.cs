using FrostySdk.Interfaces;
using System;
using System.Collections.Generic;
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

namespace FrostbiteModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for LoadingDialog.xaml
    /// </summary>
    public partial class LoadingDialog : Window, ILogger
    {
        public LoadingDialog() : base()
        {
            InitializeComponent();

        }

        public LoadingDialog(string loadingSubTitle, string loadingCurrentMessage) : base()
        {
            InitializeComponent();
            //Task.Run(() =>
            //{
            //    Dispatcher.Invoke(() =>
            //    {
            //        lblLoadingSubtitle.Content = loadingSubTitle;
            //        lblProgress.Content = loadingCurrentMessage;
            //    });
            //});
            Update(loadingSubTitle, loadingCurrentMessage);

        }

        public void SetProgressBarMaximum(int maximum)
        {
            pbar.Maximum = maximum;
        }

        public void Update(int progress)
        {
            Dispatcher.Invoke(() => {

                pring.Visibility = Visibility.Collapsed;
                pbar.Visibility = Visibility.Visible;
                pbar.Value = progress;
            });
        }

        public async Task UpdateAsync(int progress)
        {
            await Task.Run(() => { Update(progress); });
        }

        public void Update(string loadingSubTitle, string loadingCurrentMessage)
        {
            Dispatcher.Invoke(() => {

                lblLoadingSubtitle.Content = loadingSubTitle;
                lblProgress.Content = loadingCurrentMessage;

                this.Visibility = loadingSubTitle == string.Empty && loadingCurrentMessage == string.Empty ? Visibility.Collapsed : Visibility.Visible;
            });
        }

        public async Task<bool> UpdateAsync(string loadingSubTitle, string loadingCurrentMessage)
        {
            return await Task.Run(() => { Update(loadingSubTitle, loadingCurrentMessage); return true; });
        }

        public void Update(string loadingSubTitle, string loadingCurrentMessage, int progress)
        {
            Dispatcher.Invoke(() => {

                lblLoadingSubtitle.Content = loadingSubTitle;
                lblProgress.Content = loadingCurrentMessage;
                pbar.Value = progress;
            });
        }

        public async void UpdateAsync(string loadingSubTitle, string loadingCurrentMessage, int progress)
        {
            await Task.Run(() => { Update(loadingSubTitle, loadingCurrentMessage, progress); });
        }

        public void Log(string text, params object[] vars)
        {
            Dispatcher.Invoke(() =>
            {
                Update(lblLoadingSubtitle.Content.ToString(), text);
            });
        }

        public void LogWarning(string text, params object[] vars)
        {
            Update(lblLoadingSubtitle.Content.ToString(), text);
        }

        public void LogError(string text, params object[] vars)
        {
            Update(lblLoadingSubtitle.Content.ToString(), text);
        }
    }
}
