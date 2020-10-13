using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
using System.Windows.Shapes;
using v2k4FIFAModding;

namespace FIFAModdingUI.Windows
{
    /// <summary>
    /// Interaction logic for LegacyModEditor.xaml
    /// </summary>
    public partial class LegacyModEditor : Window
    {
        public LegacyModEditor()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            new MainWindow().Show();
        }

        private void btnBrowseLegacyDirectory_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if(result == System.Windows.Forms.DialogResult.OK)
                {
                    txtLegacyDirectory.Text = dialog.SelectedPath;
                    var allFiles = Directory.GetFiles(dialog.SelectedPath, "*.*", SearchOption.AllDirectories);
                    lstFilesToInclude.ItemsSource = allFiles;
                }
            }
        }

        public static void RecursiveDelete(DirectoryInfo baseDir)
        {
            try
            {
                if (!baseDir.Exists)
                    return;

                foreach (var dir in baseDir.EnumerateDirectories())
                {
                    RecursiveDelete(dir);
                }
                baseDir.Delete(true);
            }
            catch(Exception e)
            {

            }
        }

        private void SaveToCompressedFile_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtLegacyDirectory.Text))
            {
                DirectoryInfo directoryInfoTemp = new DirectoryInfo("TEMP");
                if (directoryInfoTemp.Exists)
                {
                    RecursiveDelete(directoryInfoTemp);
                }


                DirectoryCopy(txtLegacyDirectory.Text, "TEMP", true);
                List<string> listOfCompilableFiles = new List<string>();

                var allFiles = Directory.GetFiles("TEMP", "*.*", SearchOption.AllDirectories);
                foreach (var file in allFiles.Where(x => !x.Contains(".mod")))
                {
                    StringBuilder sbFinalResult = new StringBuilder();


                    if (chkEncryptFiles.IsChecked.HasValue && chkEncryptFiles.IsChecked.Value)
                    {
                        var splitExtension = file.Split('.');
                        if (splitExtension[splitExtension.Length - 1] != "mod")
                            splitExtension[splitExtension.Length - 1] = "mod";

                        foreach (var str in splitExtension)
                        {
                            if (str == "mod")
                            {
                                sbFinalResult.Append(".mod");
                            }
                            else
                            {
                                sbFinalResult.Append(str);
                            }
                        }
                    }
                    else
                    {
                        sbFinalResult.Append(file);
                    }

                    if (chkEncryptFiles.IsChecked.HasValue && chkEncryptFiles.IsChecked.Value)
                    {
                        txtSaveFileStatus.Text = "Encrypting " + file;
                        v2k4Encryption.encryptFile(file, sbFinalResult.ToString());
                        File.Delete(file);
                    }

                    //sbFinalResult = sbFinalResult.Replace("TEMP\\", "");
                    listOfCompilableFiles.Add(sbFinalResult.ToString());

                }
                txtSaveFileStatus.Text = "Compilation Complete";


                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Legacy Mod Files|.lmod";
                var saveFileDialogResult = saveFileDialog.ShowDialog();
                if (saveFileDialogResult.HasValue && saveFileDialogResult.Value)
                {
                    using (ZipArchive zipArchive = new ZipArchive(new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate), ZipArchiveMode.Create))
                    {
                        foreach (var compatFile in listOfCompilableFiles)
                        {
                            zipArchive.CreateEntryFromFile(compatFile, compatFile.Replace("TEMP\\", ""));
                        }
                    }
                    txtSaveFileStatus.Text = "Legacy Mod file saved";

                }
            }
        }


        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = System.IO.Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = System.IO.Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
