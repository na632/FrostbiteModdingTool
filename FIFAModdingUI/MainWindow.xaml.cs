using System;
using System.Collections.Generic;
using System.IO;
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
using FIFAModdingUI.ini;

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> CompatibleFIFAVersions = new List<string>()
        {
            "FIFA19.exe"
        };

        public static string FIFADirectory = string.Empty;
        public static string FIFALocaleIni
        {
            get
            {
                if (!string.IsNullOrEmpty(FIFADirectory))
                    return FIFADirectory + "\\Data\\locale.ini";
                else
                    return null;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeIniSettings();

        }

        private void InitializeIniSettings()
        {
            InitializeLanguageSystem();
            InitializeAIObjectiveSystem();
            InitializeContextEffectSystem();
            InitializeAttributeWeightSystem();
            InitializeOtherSettings();
        }

        private void InitializeOtherSettings()
        {
            if(!string.IsNullOrEmpty(FIFALocaleIni))
            {
                var localeini = new IniReader(FIFALocaleIni);
                var allKeys = localeini.GetKeys("");
                foreach (var k in allKeys)
                {
                    switch(k.Trim())
                    {
                        case "OVERRIDE_ERROR_ATTRIBUTE":
                            OVERRIDE_ERROR_ATTRIBUTE.IsChecked = localeini.GetValue(k, "") == "1" ? true : false;
                            break;
                        case "OVERRIDE_GRAPH_SHAPE":
                            OVERRIDE_GRAPH_SHAPE.IsChecked = localeini.GetValue(k, "") == "1" ? true : false;
                            break;
                        case "OVERRIDE_GRAPH_SHAPE_HIGH":
                            OVERRIDE_GRAPH_SHAPE_HIGH.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            break;
                        case "OVERRIDE_GRAPH_SHAPE_LOW":
                            OVERRIDE_GRAPH_SHAPE_LOW.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            break;
                        case "RIGHTFOOT":
                            sliderFOOT.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            break;
                        case "RIGHTUPPERLEG":
                            sliderUPPERLEG.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            break;
                        case "RIGHTANKLE":
                            sliderANKLE.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            break;
                        case "TORSO":
                            sliderTORSO.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            break;
                        case "HIPS":
                            sliderHIPS.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            break;
                        case "BALL_Y_VELOCITY_HEADER_REDUCTION":
                            BALL_Y_VELOCITY_HEADER_REDUCTION.Value = Convert.ToDouble(localeini.GetValue(k, "").Replace("f", ""));
                            break;
                        case "BALL_LATERAL_VELOCITY_HEADER_REDUCTION":
                            BALL_LATERAL_VELOCITY_HEADER_REDUCTION.Value = Convert.ToDouble(localeini.GetValue(k, "").Replace("f", ""));
                            break;
                        case "KILL_EVERYONE":
                            KILL_EVERYONE.IsChecked = localeini.GetValue(k, "").Trim() == "1" ? true : false;
                            break;
                        case "UCC_MULTICHARACTER":
                            chkEnableUnlockBootsAndCelebrations.IsChecked = localeini.GetValue(k, "").Trim() == "1" ? true : false;
                            break;
                        case "RandomSeed":
                            chkDisableRandomSeed.IsChecked = localeini.GetValue(k, "").Trim() == "0" ? true : false;
                            break;
                    }
                }
                var cpuaikeys = localeini.GetKeys("CPUAI");
                if(cpuaikeys.Length > 0)
                {
                    chkEnableHardDifficulty.IsChecked = true;
                }

            }
        }

        private void InitializeAttributeWeightSystem()
        {
            panel_GP_AttributeWeightSystem.Children.Clear();
            var aiobjsystem = new IniReader("ini/ATTRIBUTE_WEIGHTS.ini");
            var nameOfAttribute = string.Empty;
            foreach (var k in aiobjsystem.GetKeys(""))
            {
                if (k.StartsWith("//"))
                {
                    nameOfAttribute = k.Substring(2, k.Length - 2);
                }
                else
                {
                    var sp = new StackPanel();
                    sp.Orientation = Orientation.Horizontal;

                    if (k.Contains("AI_USE_ATTRIBULATOR_TO_UPDATE_GENERIC_CONVERT_TBL"))
                    {

                    }
                    else
                    {
                        var label = new Label();
                        label.Content = nameOfAttribute;
                        label.Width = 275;
                        sp.Children.Add(label);

                        var c = new Slider();
                        c.Name = k.Trim();
                        c.Minimum = 1;
                        c.Maximum = 300;
                        c.Width = 200;
                        var v = aiobjsystem.GetValue(k, "").Trim();
                        c.Value = Convert.ToDouble(v);
                        c.TickFrequency = 1;
                        sp.Children.Add(c);
                    }
                    this.panel_GP_AttributeWeightSystem.Children.Add(sp);
                }
            }

            // read from locale.ini 
            if (!chkUseBaseFIFAINI.IsChecked.Value && !string.IsNullOrEmpty(FIFALocaleIni))
            {
                var localeini = new IniReader(FIFALocaleIni);
                var allKeys = localeini.GetKeys("");
                foreach (var k in allKeys)
                {
                    foreach (StackPanel container in panel_GP_AttributeWeightSystem.Children.OfType<StackPanel>())
                    {
                        foreach (Slider slider in container.Children.OfType<Slider>())
                        {
                            if (slider.Name.Trim() == k.Trim())
                            {
                                var v = localeini.GetValue(k, "");
                                slider.Value = Convert.ToDouble(v);
                            }
                        }
                    }
                }
            }
        }

        private void InitializeLanguageSystem()
        {
            var file = new IniReader("ini/_FIFA19_base.ini");
            Dictionary<string, string> languages = new Dictionary<string, string>();
            foreach (var k in file.GetKeys("LOCALE"))
            {
                if(k.Contains("AVAILABLE_LANG_"))
                    languages.Add(file.GetValue(k, "LOCALE"), file.GetValue(k, "LOCALE"));
            }
            cbLanguages.ItemsSource = languages;
            cbLanguages.SelectedValuePath = "Key";
            cbLanguages.DisplayMemberPath = "Value";

            // read from locale.ini 
            if (chkUseBaseFIFAINI.IsChecked.Value || string.IsNullOrEmpty(FIFALocaleIni))
            {
                cbLanguages.SelectedValue = file.GetValue(file.GetKeys("LOCALE").FirstOrDefault(x => x.Contains("DEFAULT_LANGUAGE")), "LOCALE");
            }

            // read from locale.ini 
            if (!chkUseBaseFIFAINI.IsChecked.Value && !string.IsNullOrEmpty(FIFALocaleIni))
            {
                var localeini = new IniReader(FIFALocaleIni);
                var localeLangKeys = localeini.GetKeys("LOCALE");
                cbLanguages.SelectedValue = localeini.GetValue(localeLangKeys.FirstOrDefault(x => x.Contains("DEFAULT_LANGUAGE")), "LOCALE");
            }
        }

        private void InitializeContextEffectSystem()
        {
            var aiobjsystem = new IniReader("ini/ContextEffect.ini");
            var commentBuildUp = new StringBuilder();
            foreach (var k in aiobjsystem.GetKeys(""))
            {
                if (k.StartsWith("//"))
                {
                    commentBuildUp.AppendLine(k.Replace("// ", ""));
                }
                else
                {
                    var sp = new StackPanel();
                    sp.Orientation = Orientation.Horizontal;

                    var label = new Label();
                    label.Content = k.Trim();
                    if (Resources.Contains(k.Trim() + "Desc"))
                    {
                        label.Content = Resources[k.Trim() + "Desc"];
                    }
                    sp.Children.Add(label);

                    var c = new Slider();
                    c.Name = k.Trim();
                    c.Minimum = 0.01;
                    c.Maximum = 1.00;
                    c.Width = 200;
                    c.Value = 0.5;
                    sp.Children.Add(c);

                    var tb = new TextBlock();
                    tb.Text = commentBuildUp.ToString();
                    sp.Children.Add(tb);

                    this.panel_GP_ContextEffect.Children.Add(sp);
                    commentBuildUp = new StringBuilder();
                }
            }
        }

        private void InitializeAIObjectiveSystem()
        {
            panel_GP_ObjectiveSystem.Children.Clear();

            var aiobjsystem = new IniReader("ini/AIObjectiveSystem.ini");
            foreach (var k in aiobjsystem.GetKeys("").OrderBy(x=>x))
            {
                if (!k.StartsWith("//"))
                {
                    var c = new CheckBox();
                    c.Name = k.Trim();
                    c.Content = k.Replace("_", " ").Replace("ENABLE OBJECTIVE", "").Trim();
                    c.IsChecked = true;
                    if(Resources.Contains(c.Content+"Desc"))
                    {

                    }
                    panel_GP_ObjectiveSystem.Children.Add(c);
                    panel_GP_ObjectiveSystem.UpdateLayout();
                }
            }
            panel_GP_ObjectiveSystem.UpdateLayout();

            // read from locale.ini 
            if (!chkUseBaseFIFAINI.IsChecked.Value && !string.IsNullOrEmpty(FIFALocaleIni))
            {
                var localeini = new IniReader(FIFALocaleIni);
                foreach (var k in aiobjsystem.GetKeys(""))
                {
                    foreach (Control ch in panel_GP_ObjectiveSystem.Children)
                    {
                        if (ch.GetType() == typeof(CheckBox) && ch.Name.Trim() == k.Trim())
                        {
                            var value = aiobjsystem.GetValue(k);
                            ((CheckBox)ch).IsChecked = aiobjsystem.GetValue(k).Contains("1") ? true : false;
                        }
                    }
                }
            }
            panel_GP_ObjectiveSystem.UpdateLayout();
        }

        private void ChkEnableUnlockBootsAndCelebrations_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnBrowseFIFADirectory_Click(object sender, RoutedEventArgs e)
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.ShowDialog(this);
            FIFADirectory = dialog.SelectedPath;
            if (!string.IsNullOrEmpty(FIFADirectory))
            {
                // Check for compatible FIFA Version
                var filesInDirectory = Directory.EnumerateFiles(FIFADirectory).ToList();
                // Rip out the file names
                for (var i = 0; i < filesInDirectory.Count; i++)
                {
                    filesInDirectory[i] = filesInDirectory[i].Split("\\")[2];
                }
                // Do Check against List
                if (filesInDirectory.Any(x => CompatibleFIFAVersions.Any(y => y == x)))
                {
                    txtFIFADirectory.Text = FIFADirectory;
                    MainViewer.IsEnabled = true;
                    InitializeIniSettings();
                }
            }
        }

        private void Btn_GP_SaveINI_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("newFile.ini"))
                File.Delete("newFile.ini");

            using (StreamWriter stream = new StreamWriter("newFile.ini"))
            {
                stream.Write(GetResultingFileString());
            }

            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to overwrite your existing locale.ini?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                if (File.Exists(FIFALocaleIni))
                {
                    File.Copy(FIFALocaleIni, FIFADirectory + "\\Data\\locale.ini." + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".backup");
                    File.Delete(FIFALocaleIni);
                }

                using (StreamWriter stream = new StreamWriter(FIFALocaleIni))
                {
                    stream.Write(GetResultingFileString());
                }
            }
        }

        protected void tabRawFileView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            tbRawFileView.Text = GetResultingFileString();
        }

        private void MainViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbRawFileView.Text = GetResultingFileString();
        }

        public string GetResultingFileString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("// -------------------------------------------------");
            sb.AppendLine("// CREATED BY paulv2k4 FIFA Modding UI            //");
            sb.AppendLine("// -------------------------------------------------");
            sb.AppendLine("");


            sb.AppendLine("// Languages");
            var file = new IniReader("ini/_FIFA19_base.ini");
            foreach (var s in file.GetSections())
            {
                sb.AppendLine("[" + s + "]");
                foreach (var k in file.GetKeys(s))
                {
                    if (!k.StartsWith("//"))
                    {
                        var v = file.GetValue(k, s);
                        if (s.Contains("LOCALE") && k.Contains("DEFAULT_LANGUAGE"))
                        {
                            // get changed language settings
                            if (chkUseBaseFIFAINI.IsChecked.HasValue && !chkUseBaseFIFAINI.IsChecked.Value)
                            {
                                v = cbLanguages.SelectedValue.ToString();
                            }
                        }

                        sb.AppendLine(k + "=" + v);
                    }
                }
            }

            


            sb.AppendLine("");
            sb.AppendLine("// Objective System");
            sb.AppendLine("[]");
            // Obj system
            foreach (Control ch in panel_GP_ObjectiveSystem.Children)
            {
                if (ch.GetType() == typeof(CheckBox))
                {
                    var chkBox = ch as CheckBox;
                    sb.AppendLine(ch.Name + "=" + (chkBox.IsChecked.Value ? "1" : "0"));
                }
            }

            sb.AppendLine("");
            if (chkEnableUnlockBootsAndCelebrations.IsChecked.HasValue && chkEnableUnlockBootsAndCelebrations.IsChecked.Value)
            {
                sb.AppendLine("// Unlock Boots");
                file = new IniReader("ini/UnlockBootsAndCelebrations.ini");
                foreach (var s in file.GetSections())
                {
                    sb.AppendLine("[" + s + "]");
                    foreach (var k in file.GetKeys(s))
                    {
                        if (!k.StartsWith("//"))
                        {
                            sb.AppendLine(k + "=" + file.GetValue(k, s));
                        }
                    }
                }
            }

            sb.AppendLine("");
            sb.AppendLine("// Animation & Gameplay");
            sb.AppendLine("[]");
            sb.AppendLine("RIGHTUPPERLEG=" + Math.Round(sliderUPPERLEG.Value));
            sb.AppendLine("LEFTUPPERLEG=" + Math.Round(sliderUPPERLEG.Value));
            sb.AppendLine("RIGHTFOOT=" + Math.Round(sliderFOOT.Value));
            sb.AppendLine("LEFTFOOT=" + Math.Round(sliderFOOT.Value));
            sb.AppendLine("RIGHTANKLE=" + Math.Round(sliderANKLE.Value));
            sb.AppendLine("LEFTANKLE=" + Math.Round(sliderANKLE.Value));
            sb.AppendLine("TORSO=" + Math.Round(sliderTORSO.Value));
            sb.AppendLine("HIPS=" + Math.Round(sliderHIPS.Value));
            sb.AppendLine("");

            sb.AppendLine("");
            sb.AppendLine("// ATTRIBUTES");
            sb.AppendLine("[]");
            foreach (StackPanel container in panel_GP_AttributeWeightSystem.Children.OfType<StackPanel>())
            {
                foreach (Slider slider in container.Children.OfType<Slider>())
                {
                    sb.AppendLine(slider.Name + "=" + slider.Value);
                }
            }
            sb.AppendLine("");

            sb.AppendLine("// Other Settings");
            sb.AppendLine("[]");
            sb.AppendLine("OVERRIDE_ERROR_ATTRIBUTE=" + (OVERRIDE_ERROR_ATTRIBUTE.IsChecked.Value ? "1" : "0"));
            sb.AppendLine("OVERRIDE_GRAPH_SHAPE=" + (OVERRIDE_GRAPH_SHAPE.IsChecked.Value ? "1" : "0"));
            sb.AppendLine("OVERRIDE_GRAPH_SHAPE_HIGH=" + Math.Round(OVERRIDE_GRAPH_SHAPE_HIGH.Value, 2));
            sb.AppendLine("OVERRIDE_GRAPH_SHAPE_LOW=" + Math.Round(OVERRIDE_GRAPH_SHAPE_LOW.Value, 2));

            sb.AppendLine("DISABLE_BALLTOUCH_LIMITATION=1");
            sb.AppendLine("ENABLE_CPUAI_TRAP_ERROR=1");
            sb.AppendLine("USE_TRAP_ERROR_SYSTEM=1");

            sb.AppendLine("RULESCOLLISION_DISABLE_NON_USER_CONTROLLED_PLAYER_LOGIC=1");
            sb.AppendLine("RULESCOLLISION_ENABLE_INTENDED_BALLTOUCH=1");

            sb.AppendLine("AccelerationGain=0.04");
            sb.AppendLine("DecelerationGain=2.00");
            //sb.AppendLine("ENABLE_DRIBBLE_ACCEL_MOD = 1");
            //sb.AppendLine("ACCEL = 100.0");
            //sb.AppendLine("DECEL = 175.0");
            
            if (KILL_EVERYONE.IsChecked.HasValue)
            {
                sb.AppendLine("KILL_EVERYONE=" + (KILL_EVERYONE.IsChecked.Value ? "1" : "0"));
                if(KILL_EVERYONE.IsChecked.Value)
                {
                    sb.AppendLine("foulstrictness = 1");
                    sb.AppendLine("Cardstrictness = 0");
                    sb.AppendLine("REFEREE_CARD_STRICTNESS_OVERRIDE = 1");
                    sb.AppendLine("REFEREE_FOUL_STRICTNESS_OVERRIDE = 1");
                    sb.AppendLine("REF_STRICTNESS = 1");
                    sb.AppendLine("RefStrictness_0 = 1");
                    sb.AppendLine("RefStrictness_1 = 1");
                    sb.AppendLine("RefStrictness_2 = 1");
                    sb.AppendLine("RefStrictness_3 = 1");
                    sb.AppendLine("SLIDE_TACKLE = 0");
                    sb.AppendLine("SLIDETACKLE = 0");
                    sb.AppendLine("TACKLE = 0");
                }
            }

            sb.AppendLine("ContextEffectTrapBallInAngle=50");
            sb.AppendLine("ContextEffectTrapBallXZVelocity=75");
            sb.AppendLine("DEBUG_DISABLE_LOSE_ATTACKER_EFFECT = 1");

            sb.AppendLine("FALL = 50");
            sb.AppendLine("STUMBLE = 10");

            sb.AppendLine("BALL_Y_VELOCITY_HEADER_REDUCTION=" + Math.Round(BALL_Y_VELOCITY_HEADER_REDUCTION.Value, 2));
            sb.AppendLine("BALL_LATERAL_VELOCITY_HEADER_REDUCTION=" + Math.Round(BALL_LATERAL_VELOCITY_HEADER_REDUCTION.Value, 2));

            if (chkDisableRandomSeed.IsChecked.HasValue && chkDisableRandomSeed.IsChecked.Value)
            {
                sb.AppendLine("");
                sb.AppendLine("[]");
                sb.AppendLine("AI_LASTRANDOMSEED = 0");
                sb.AppendLine("RandomSeed = 0");
                sb.AppendLine("RandomTimeSeed = 0");
                sb.AppendLine("RandomTickSeed = 0");
                sb.AppendLine("RandomIntensityMin = 0");
                sb.AppendLine("RandomIntensityMax = 0");
                sb.AppendLine("ClipPlayerOverallRating = 80");

                sb.AppendLine("[DEFAULTS]");
                sb.AppendLine("RANDOMSEED=0");
                sb.AppendLine("[GAMEMODE]");
                sb.AppendLine("RANDOM_SEED=0");

            }

            // Do CPUAI
            if (chkEnableHardDifficulty.IsChecked.HasValue && chkEnableHardDifficulty.IsChecked.Value)
            {
                sb.AppendLine("");
                sb.AppendLine("[CPUAI]");
                sb.AppendLine("HOME_OFFENSE_DIFFICULTY=0.75");
                sb.AppendLine("HOME_DEFENSE_DIFFICULTY=0.02");
                sb.AppendLine("AWAY_OFFENSE_DIFFICULTY=0.75");
                sb.AppendLine("AWAY_DEFENSE_DIFFICULTY=0.02");
                sb.AppendLine("HOME_DIFFICULTY=0.75");
                sb.AppendLine("AWAY_DIFFICULTY=0.75");
                sb.AppendLine("CPUAI_PROCESS_ALL_DECISIONS=0");

                sb.AppendLine("");
                sb.AppendLine("[]");
                sb.AppendLine("FORCE_ANY=0.1");
                sb.AppendLine("FORCE_BACK=0.1");
                sb.AppendLine("FORCE_FRONT=0.1");
                sb.AppendLine("FORCE_INSIDE=0.1");
                sb.AppendLine("FORCE_OUTSIDE=0.1");
            }

            sb.AppendLine("");

            return sb.ToString();
        }

        private void ChkEnableHardDifficulty_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ChkDisableRandomSeed_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
