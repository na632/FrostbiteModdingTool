using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace FrostbiteModdingUI.Pages.Common.EBX
{
    public partial class EditorOfType : UserControl
    {
        public EditorOfType() : base()
        {
            DataContextChanged += EditorOfType_DataContextChanged;
        }

        private void EditorOfType_DataContextChanged(global::Windows.UI.Xaml.FrameworkElement sender, global::Windows.UI.Xaml.DataContextChangedEventArgs args)
        {

        }
    }
}
