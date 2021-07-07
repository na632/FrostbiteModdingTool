using FIFAModdingUI;
using FIFAModdingUI.Models;
using FIFAModdingUI.Pages.Common;
using FIFAModdingUI.Windows;
using FolderBrowserEx;
using Frostbite.FileManagers;
using Frostbite.Textures;
using FrostbiteModdingUI.Models;
using FrostbiteModdingUI.Windows;
using FrostbiteSdk.Frostbite.FileManagers;
using FrostySdk;
using FrostySdk.Ebx;
using FrostySdk.Frosty.FET;
using FrostySdk.FrostySdk.Managers;
using FrostySdk.Interfaces;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using v2k4FIFAModding;
using v2k4FIFAModding.Frosty;
using v2k4FIFAModdingCL;

namespace FrostbiteModdingUI.Windows
{
    public class FIFA19Editor : FIFA21Editor, IEditorWindow
    {
        public FIFA19Editor(Window owner) : base(owner)
        {

        }

        public override string LastGameLocation => App.ApplicationDirectory + "FIFA19LastLocation.json";
    }
}
