using FrostySdk.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FrostbiteModdingUI.Pages.Common.EBX
{
    /// <summary>
    /// Interaction logic for ListControl.xaml
    /// </summary>
    public partial class ListControl : UserControl
    {
        List<object> ContextDataList;
        EbxAsset ParentAsset;


        public ListControl()
        {
            InitializeComponent();
        }

        public ListControl(List<object> inList, EbxAsset inAsset)
        {
            ContextDataList = inList;
            ParentAsset = inAsset;
            DataContext = ContextDataList;

            InitializeComponent();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
