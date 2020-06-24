using FIFAModdingUI.ini;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace FIFAModdingUI.Pages.Gameplay
{
    /// <summary>
    /// Interaction logic for LocaleCustomSettings.xaml
    /// </summary>
    public partial class LocaleCustomSettings : UserControl
    {

        public LocaleCustomSettings()
        {
            InitializeComponent();
            this.Resources.Add("CustomSettings", CustomINISettings);
        }

        private Dictionary<string, string> cSettings;
        public Dictionary<string, string> CustomINISettings
        {
            get
            {
                if (File.Exists("CustomINISettings.json"))
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("CustomINISettings.json"));

                return new TaskFactory().StartNew(() =>
                {
                    cSettings = new Dictionary<string, string>();

                    // build from scratch raw locale.ini
                    var assembly = Assembly.GetCallingAssembly();

                    var manifestResourceNames = assembly.GetManifestResourceNames();
                    var iniResources = manifestResourceNames.Where(x => x.Contains(".ini")).ToList();

                    if (EditorWindow.FIFALocaleIni == null)
                        return null;

                    var locale = new IniReader(EditorWindow.FIFALocaleIni, false);
                    var sections = locale.GetSections();
                    foreach (var section in sections)
                    {
                        foreach (var k in locale.GetKeys(section))
                        {
                            if (k.StartsWith("//"))
                                continue;

                            var found = false;

                            // Now check all embedded to find the key
                            foreach (var r in iniResources)
                            {
                                if (found)
                                    break;

                                var resourceIni = new IniReader(r, true);
                                foreach (var riniSection in resourceIni.GetSections())
                                {
                                    if (found)
                                        break;

                                    foreach (var riniK in resourceIni.GetKeys(riniSection))
                                    {
                                        if (riniK == k)
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (!found && !cSettings.ContainsKey("[" + section + "]" + k))
                                cSettings.Add("[" + section + "]" + k, locale.GetValue(k, section));

                        }
                    }

                    CustomINISettings = cSettings;
                    return cSettings;
                }).Result;

            }
            set
            {
                cSettings = value;
                SaveCustomSettings(value);
            }
        }

        public void SaveCustomSettings(Dictionary<string,string> newSettings)
        {
            File.WriteAllText("CustomINISettings.json", JsonConvert.SerializeObject(newSettings));
        }

        public void InitializeSettings()
        {
            var customsettings = CustomINISettings;
        }

        private void btnAddSetting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditSetting_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if(btn != null && btn.DataContext != null)
            {
                var contextDict = (KeyValuePair<string, string>)btn.DataContext;
                txtSetting.Text = contextDict.Key;
                txtValue.Text = contextDict.Value;
            }
        }

        private void btnSaveSetting_Click(object sender, RoutedEventArgs e)
        {
            if(CustomINISettings.ContainsKey(txtSetting.Text))
            {
                CustomINISettings[txtSetting.Text] = txtValue.Text;
            }
            else
            {
                CustomINISettings.Add(txtSetting.Text, txtValue.Text);
            }
            lvSettings.ItemsSource = CustomINISettings;
        }
    }
}
