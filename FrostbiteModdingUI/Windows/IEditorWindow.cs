using FrostySdk.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FrostbiteModdingUI.Windows
{
    public interface IEditorWindow : ILogger
    {
        public Window OwnerWindow { get; set; }

        public Task UpdateAllBrowsersFull();

        public Task UpdateAllBrowsers();

        public string LastGameLocation { get; }

        public string RecentFilesLocation { get; }

    }
}
