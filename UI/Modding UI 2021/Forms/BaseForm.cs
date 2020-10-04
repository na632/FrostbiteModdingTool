using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using v2k4FIFAModdingCL;

namespace Modding_UI_2021.Forms
{
    public class BaseForm : Form
    {
        public void OpenTheGame()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "EXE files (*.exe)|*.exe";
            if (DialogResult.OK == openFileDialog.ShowDialog())
            {
                var filePath = openFileDialog.FileName;
                var FIFADirectory = openFileDialog.FileName.Substring(0, openFileDialog.FileName.LastIndexOf("\\") + 1);
                GameInstanceSingleton.GAMERootPath = FIFADirectory;
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                //if (!string.IsNullOrEmpty(fileName) && CompatibleFIFAVersions.Contains(fileName))
                //{
                GameInstanceSingleton.GAMEVERSION = fileName.Replace(".exe", "");
                //}

                Task.Delay(500);

                this.Enabled = false;
                new TaskFactory().StartNew(() =>
                {
                    var result = Start().Result;
                    result = PostStart().Result;
                    
                    //    }
                    //    this.Enabled = true;
                    //else
                    //{
                    //    this.Close();
                    //}
                });




            }
        }

        public virtual async Task<bool> Start()
        {
            return await Task.Run<bool>(() => { return false; });
        }

        public virtual async Task<bool> PostStart()
        {
            return await Task.Run<bool>(() => { return false; });
        }
    }
}
