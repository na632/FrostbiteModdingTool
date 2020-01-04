using FifaLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
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
