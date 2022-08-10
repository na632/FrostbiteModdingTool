using FIFAModdingUI.Pages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FrostbiteModdingUI.Pages.Common.EBX
{
    /// <summary>
    /// Interaction logic for FrostySdk_Ebx_HotspotEntry.xaml
    /// </summary>
    public partial class FrostySdk_Ebx_HotspotEntry : UserControl
    {
        public FrostySdk_Ebx_HotspotEntry()
        {
            InitializeComponent();

            

            this.Loaded += FrostySdk_Ebx_HotspotEntry_Loaded;
        }

        private void FrostySdk_Ebx_HotspotEntry_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (TextBox tb in FindVisualChildren<TextBox>(this))
            {
                tb.TextChanged += Tb_TextChanged;

                tb.LostKeyboardFocus += Tb_LostKeyboardFocus;
            }
        }

        private void Tb_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Editor.CurrentEditorInstance.SaveToRootObject();
        }

        private void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        public IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }


    }
}
