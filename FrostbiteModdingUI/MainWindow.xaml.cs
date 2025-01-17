﻿using FIFAModdingUI.Windows;
using FMT.Windows;
using FrostbiteModdingUI.Models;
using FrostbiteModdingUI.Windows;
using FrostySdk;
using FrostySdk.Managers;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static FrostySdk.ProfileManager;

//namespace FIFAModdingUI
namespace FMT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public List<Window> EditorWindows = new List<Window>();

        //public List<Profile> ProfilesWithEditorScreen = ProfilesLibrary.EditorProfiles.ToList();

        private List<Profile> profiles;

        public List<Profile> ProfilesWithEditor
        {
            get
            {

#pragma warning disable CA1416 // Validate platform compatibility
                if (profiles == null || !profiles.Any())
                    profiles = ProfileManager.EditorProfiles.ToList();
                return profiles;
#pragma warning restore CA1416 // Validate platform compatibility

            }
            set { profiles = value; }
        }

        public string WindowTitle { get; set; }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MainWindow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();

            WindowTitle = "Frostbite Modding Tool " + App.ProductVersion;

            if (AssetManager.Instance != null)
            {
                AssetManager.Instance.Dispose();
                AssetManager.Instance = null;
            }
            DataContext = this;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            foreach (var w in EditorWindows)
            {
                if (w != null)
                    w.Close();
            }

            EditorWindows.Clear();

            if (App.MainEditorWindow != null)
                App.MainEditorWindow.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (App.MainEditorWindow != null)
                App.MainEditorWindow.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (App.MainEditorWindow != null)
                App.MainEditorWindow.Close();

            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditor_Click(object sender, RoutedEventArgs e)
        {
            //new EditorLoginWindow().Show();
            App.MainEditorWindow = new FIFA21Editor(this);
            if (App.MainEditorWindow != null)
                App.MainEditorWindow.Show();

            this.Visibility = Visibility.Hidden;
        }

        private void btnLauncher_Click(object sender, RoutedEventArgs e)
        {
            var lw = new LaunchWindow(this);
            try
            {
                if (lw != null)
                {
                    lw.Show();
                    this.Visibility = Visibility.Hidden;
                }
            }
            catch
            {

            }
        }

        private void cbLanguageSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLanguageSelection.SelectedItem != null && cbLanguageSelection.SelectedIndex >= 1)
            {
                string selectedLanguage = null;
                var selectedItem = ((ComboBoxItem)cbLanguageSelection.SelectedItem).Content.ToString();
                switch (selectedItem)
                {
                    case "English":
                        selectedLanguage = "en";
                        break;
                    case "Deutsch":
                        selectedLanguage = "de";
                        break;
                    case "Português":
                        selectedLanguage = "pt";
                        break;
                }

                if (!string.IsNullOrEmpty(selectedLanguage))
                {
                    App.LoadLanguageFile(selectedLanguage);
                }
            }
        }

        private void Tile_Click(object sender, RoutedEventArgs e)
        {
            Profile profile = (Profile)((Tile)sender).Tag;
            ProfileManager.LoadedProfile = profile;

            var bS = new FindGameEXEWindow().ShowDialog();
            if (bS.HasValue && bS.Value == true && !string.IsNullOrEmpty(AppSettings.Settings.GameInstallEXEPath))
            {
                if (new FileInfo(AppSettings.Settings.GameInstallEXEPath).Name.Replace(".exe", "").Replace(" ", "") != profile.Name.Replace(" ", ""))
                {
                    MessageBox.Show("Your EXE does not match the Profile selected!");
                    return;
                }

                var gameveditordialog = new GameVsEditorVsTools(this, profile).ShowDialog();
                if (bS.HasValue && bS.Value == true)
                    this.Visibility = Visibility.Hidden;
                else
                    this.Visibility = Visibility.Visible;

            }
        }
    }
}
