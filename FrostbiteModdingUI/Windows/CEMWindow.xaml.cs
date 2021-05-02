﻿using CareerExpansionMod.CEM;
using CareerExpansionMod.CEM.FIFA;
using FrostbiteModdingUI.CEM;
using FrostySdk.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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
using v2k4FIFAModdingCL.MemHack.Core;

namespace FrostbiteModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for CEMWindow.xaml
    /// </summary>
    public partial class CEMWindow : Window, ILogger, ICEMVisualizer, INotifyPropertyChanged
    {

        private bool isInCM = false;
        public bool IsInCM
        {
            get 
            {
                var rValue = CoreHack.IsInCM();
                if (isInCM != rValue)
                {
                    isInCM = rValue;
                    NotifyPropertyChanged("IsInCM");
                }

                return isInCM;
            }
        }

        private FIFAUsers fifaUser;
        public FIFAUsers FIFAUser
        {
            get { return fifaUser; }
            set { 
                if (fifaUser != value) {
                    fifaUser = value;
                    NotifyPropertyChanged("FIFAUser");
                } 
            }
        }


        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token { get { return tokenSource.Token; } }

        public ConcurrentBag<Task> WindowTasks = new ConcurrentBag<Task>();

        CEMCore2 CEMCore;


        #region Constructor
        public CEMWindow()
        {
            InitializeComponent();

            //
            ContentRendered += CEMWindow_ContentRendered;
            Closing += CEMWindow_Closing;

            DataContext = this;

            Task.Run(() =>
            {
                CEMCore = new CEMCore2(GameInstanceSingleton.GAMEVERSION);
                CEMCore.FileSystemWatcher.Created += FileSystemWatcher_Created;
                CEMCore.FileSystemWatcher.Changed += FileSystemWatcher_Changed;
                SaveGameName = CEMCore.CurrentCareerFile.InGameName;
                var ps = CEMCore.GetPlayerStats();
                Dispatcher.InvokeAsync(() =>
                {
                    Stats.Execute(items => { items.Clear(); items.AddRange(ps.Where(x => x != null)); });
                    lvPlayerStats.ItemsSource = null;
                    lvPlayerStats.ItemsSource = Stats;
                });

            });
        }

        private void FileSystemWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
        }

        private void FileSystemWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        {
        }


        #endregion

        static bool EndAllTasks = false;

        private async void CEMWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            EndAllTasks = true;
            tokenSource.Cancel();

            try
            {
                await Task.WhenAll(WindowTasks.ToArray());
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"\n{nameof(OperationCanceledException)} thrown\n");
            }
            finally
            {
                tokenSource.Dispose();
            }

            //if (CEMCore.CEMCoreInstance != null)
            //{
            //    CEMCore.CEMCoreInstance.Dispose();
            //    CEMCore.CEMCoreInstance = null;
            //    GC.Collect();
            //    GC.WaitForPendingFinalizers();
            //}
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private void StartFIFADateWatch()
        {
            //WindowTasks.Add(Task.Run(() =>
            //{
            //    // Was cancellation already requested?
            //    if (token.IsCancellationRequested)
            //    {
            //        Console.WriteLine($"Task StartFIFADateWatch was cancelled before it got started.");
            //        token.ThrowIfCancellationRequested();
            //    }

            //    while (!EndAllTasks && CEMCore.CEMCoreInstance != null && CEMCore.CEMCoreInstance.CoreHack != null)
            //    {
            //        if (token.IsCancellationRequested)
            //        {
            //            Console.WriteLine($"Task StartFIFADateWatch was cancelled.");
            //            token.ThrowIfCancellationRequested();
            //        }

            //        try
            //        {
            //            var gameDate = CEMCore.CEMCoreInstance.CoreHack.GetInGameDate();
            //            if (gameDate.HasValue)
            //            {
            //                Dispatcher.Invoke(() =>
            //                {
            //                    lblGameDate.Text = gameDate.Value.ToString("dd, MMM, yyyy");
            //                });

            //                if (gameDate.Value.ToString("dd, MMM, yyyy") != GameDate)
            //                {
            //                    GameDate = gameDate.Value.ToString("dd, MMM, yyyy");
            //                    UpdateTeamStats();

            //                }
            //            }

            //        }
            //        catch
            //        {
            //            Dispatcher.Invoke(() =>
            //            {
            //                this.Close();
            //            });
            //        }
            //        Task.Delay(1000);
            //    }
            //}));
        }


        private void UpdateTeamStats()
        {
            if (FIFAUser != null)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    Stats.Execute(items => { items.Clear(); items.AddRange(FIFAPlayerStat.GetTeamPlayerStats(FIFAUser.clubteamid).Where(x => x != null)); });
                    lvPlayerStats.ItemsSource = null;
                    lvPlayerStats.ItemsSource = Stats;
                });
            }
        }


        bool CEMActive = false;


        private async void CEMWindow_ContentRendered(object sender, EventArgs e)
        {
            //try
            //{
            //    CEMActive = await CEMCore.InitialStartupOfCEM(this, this, token);
            //    if (CEMActive)
            //    {
            //        FIFAUser = CareerDB1.FIFAUser;
            //        Log("Getting the Save Name...");
            //        SaveGameName = CEMCore.CEMCoreInstance.CurrentSaveFileName;
            //        Log("Retrieving the Player Stats...");
            //        UpdateTeamStats();

            //        //StartIsInCMWatch();
            //        //StartFIFADateWatch();

            //    }
            //}
            //catch(OperationCanceledException)
            //{

            //}
            //finally
            //{

            //}
        }

        private void StartIsInCMWatch()
        {
            WindowTasks.Add(Task.Run(() =>
            {
                // Was cancellation already requested?
                if (token.IsCancellationRequested)
                {
                    Debug.WriteLine($"Task StartIsInCMWatch was cancelled before it got started.");
                    token.ThrowIfCancellationRequested();
                }

                while (!EndAllTasks && CEMActive)
                {
                    // Was cancellation already requested?
                    if (token.IsCancellationRequested)
                    {
                        Debug.WriteLine($"Task StartIsInCMWatch was cancelled.");
                        token.ThrowIfCancellationRequested();
                    }

                    Task.Delay(1000);
                    if (!IsInCM)
                    {
                        CoreHack.AOBtoAddress.Clear();
                        CoreHack.AOBtoAddresses.Clear();
                    }
                }
            }));
        }

        public ExtendedObservableCollection<FIFAPlayerStat> Stats = new ExtendedObservableCollection<FIFAPlayerStat>();

        private void StartFIFAProcessWatch(bool wait = false)
        {
            WindowTasks.Add(Task.Run(async () => {

                if (token.IsCancellationRequested)
                {
                    Console.WriteLine($"Task StartFIFAProcessWatch was cancelled before it could start.");
                    token.ThrowIfCancellationRequested();
                }

                if (wait)
                    await Task.Delay(30 * 1000);

                bool fifaStillExists = false;
                do
                {
                    fifaStillExists = Process.GetProcesses().Any(x => x.ProcessName.Contains("FIFA", StringComparison.OrdinalIgnoreCase));

                    if (token.IsCancellationRequested)
                    {
                        Console.WriteLine($"Task StartFIFAProcessWatch was cancelled.");
                        token.ThrowIfCancellationRequested();
                    }
                }
                while (!EndAllTasks && fifaStillExists);

                if (!fifaStillExists)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (MessageBox.Show("The FIFA Process is not available or has been closed.", "CEM Error") == MessageBoxResult.OK)
                        {
                            this.Close();
                            return;
                        }
                    });
                }

            }));
        }

        private string saveGameName;

        public string SaveGameName
        {
            get { return saveGameName; }
            set { saveGameName = value; NotifyPropertyChanged("SaveGameName"); }
        }


        private string lastLogMessage;

        public string LastLogMessage
        {
            get { return lastLogMessage; }
            set { lastLogMessage = value; NotifyPropertyChanged("LastLogMessage"); }
        }

        private string gameDate;

        public string GameDate
        {
            get { return gameDate; }
            set { gameDate = value; NotifyPropertyChanged("GameDate"); }
        }

        public void Log(string text, params object[] vars)
        {
            LastLogMessage = text;
        }

        public void LogError(string text, params object[] vars)
        {
            LastLogMessage = text;
        }

        public void LogWarning(string text, params object[] vars)
        {
            LastLogMessage = text;
        }

        public void SetSaveName(string newSaveName)
        {
            SaveGameName = newSaveName;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateTeamStats();
        }
    }

    public static class LinqExtensions
    {
        public static ICollection<T> AddRange<T>(this ICollection<T> source, IEnumerable<T> addSource)
        {
            foreach (T item in addSource)
            {
                source.Add(item);
            }

            return source;
        }
    }

    public class ExtendedObservableCollection<T> : ObservableCollection<T>
    {
        public void Execute(Action<IList<T>> itemsAction)
        {
            itemsAction(Items);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
