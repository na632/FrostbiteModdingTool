using FrostbiteModdingUI.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;

namespace FMT.Windows
{
    public class Madden22Editor : Madden21Editor, IEditorWindow
    {
        private string WindowEditorTitle = $"Madden 22 Editor - {FileVersionInfo.GetVersionInfo(AppContext.BaseDirectory).ProductVersion} - ";
        public new string LastGameLocation => App.ApplicationDirectory + "MADDEN22LastLocation.json";

        private string _windowTitle;
        public new string WindowTitle
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(WindowEditorTitle);
                stringBuilder.Append(_windowTitle);
                return stringBuilder.ToString();
            }
            set
            {
                _windowTitle = "[" + value + "]";
                this.DataContext = null;
                this.DataContext = this;
                this.UpdateLayout();
            }
        }

        public Madden22Editor()
        {

        }

        public Madden22Editor(Window owner)
        {
            InitializeComponent();
            this.Closing += Madden21Editor_Closing;
            this.Loaded += Madden21Editor_Loaded;
            OwnerWindow = owner;
            Owner = owner;

        }

    }
}
