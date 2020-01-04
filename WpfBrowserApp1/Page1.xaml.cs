using FifaLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace WpfBrowserApp1
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();


            CareerFile careerFile = new CareerFile(@"C:\Users\paula\Documents\FIFA 20\settings\Career20191126231940A", "fifa_ng_db-meta.xml");
            if (careerFile != null)
            {
                var dsets = careerFile.ConvertToDataSet();
                if (dsets != null)
                {

                }
            }
        }
    }
}
