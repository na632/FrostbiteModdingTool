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
using System.Reflection;
using FIFAModdingUI.ini;
using MahApps.Metro.Controls;
using v2k4FIFAModdingCL;
using v2k4FIFAModdingCL.CustomAttributes;
using v2k4FIFAModdingCL.Career;
using System.Text.RegularExpressions;

namespace FIFAModdingUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        List<string> CompatibleFIFAVersions = new List<string>()
        {
            "FIFA19.exe",
            "FIFA20_demo.exe",
            "FIFA20.exe"
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

                if (FIFAInstanceSingleton.FIFAVERSION.Contains("demo"))
                    chkSkipBootFlow.IsEnabled = false;

                var localeini = new IniReader(FIFALocaleIni);
                var allKeys = localeini.GetKeys("");
                foreach (var k in allKeys)
                {
                    switch(k.Trim())
                    {
                        case "SKIP_BOOTFLOW":
                            chkSkipBootFlow.IsChecked = localeini.GetValue(k, "") == "1" ? true : false;
                            break;
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
            var aiobjsystem = new IniReader("ATTRIBUTE_WEIGHTS.ini", true);
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
                        //label.Content = !nameOfAttribute.Contains("GK")
                        //    ? nameOfAttribute.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[2]
                        //    :
                        //    nameOfAttribute.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[2] +
                        //    nameOfAttribute.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries)[3];

                        label.Content = nameOfAttribute.Replace("_", " ").Replace("PLAYER ATTRIBUTE ", "");

                        label.Width = 275;
                        sp.Children.Add(label);

                        var c = new Slider();
                        c.Name = k.Trim();
                        c.Minimum = 0;
                        c.Maximum = 350;
                        c.Width = 350;
                        var v = aiobjsystem.GetValue(k, "").Trim();
                        c.Value = Convert.ToDouble(v);
                        c.TickFrequency = 5;
                        c.IsSnapToTickEnabled = true;
                        c.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                        sp.Children.Add(c);

                        var labelSliderIndicator = new TextBox();
                        labelSliderIndicator.TextAlignment = TextAlignment.Right;
                        labelSliderIndicator.Width = 40;
                        sp.Children.Add(labelSliderIndicator);


                        labelSliderIndicator.SetBinding(TextBox.TextProperty,
                            new Binding("Value") { Source = c });


                        if (FIFAModdingUI.Resources.ResourceManager.GetString(k.Trim()+"_DESC") != null)
                        {
                            var labelDesc = new Label();
                            labelDesc.Content = FIFAModdingUI.Resources.ResourceManager.GetString(k.Trim() + "_DESC");
                            //labelDesc.Width = 275;
                            sp.Children.Add(labelDesc);
                        }

                       
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
            var file = new IniReader("_FIFA19_base.ini", true);
            Dictionary<string, string> languages = new Dictionary<string, string>();
            foreach (var k in file.GetKeys("LOCALE"))
            {
                if(k.Contains("AVAILABLE_LANG_"))
                    languages.Add(file.GetValue(k, "LOCALE"), file.GetValue(k, "LOCALE"));
            }
            //cbLanguages.ItemsSource = languages;
            //cbLanguages.SelectedValuePath = "Key";
            //cbLanguages.DisplayMemberPath = "Value";

            //// read from locale.ini 
            //if (chkUseBaseFIFAINI.IsChecked.Value || string.IsNullOrEmpty(FIFALocaleIni))
            //{
            //    cbLanguages.SelectedValue = file.GetValue(file.GetKeys("LOCALE").FirstOrDefault(x => x.Contains("DEFAULT_LANGUAGE")), "LOCALE");
            //}

            //// read from locale.ini 
            //if (!chkUseBaseFIFAINI.IsChecked.Value && !string.IsNullOrEmpty(FIFALocaleIni))
            //{
            //    var localeini = new IniReader(FIFALocaleIni);
            //    var localeLangKeys = localeini.GetKeys("LOCALE");
            //    cbLanguages.SelectedValue = localeini.GetValue(localeLangKeys.FirstOrDefault(x => x.Contains("DEFAULT_LANGUAGE")), "LOCALE");
            //}
        }

        private void InitializeContextEffectSystem()
        {
            this.panel_GP_ContextEffect.Children.Clear();

            // Initialise 

            //var aiobjsystem = new IniReader("ini/ContextEffect.ini");
            var aiobjsystem = new IniReader("ContextEffect_" + FIFAInstanceSingleton.FIFAVERSION.Replace("_demo","") + ".ini", true);
            var commentBuildUp = new StringBuilder();

            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(400) });
            g.ColumnDefinitions.Add(new ColumnDefinition() { });
            g.ColumnDefinitions.Add(new ColumnDefinition() { });
            g.ColumnDefinitions.Add(new ColumnDefinition() { });
            var i = 0;
            foreach (var k in aiobjsystem.GetKeys("").OrderBy(x => x))
            {
                if (k.StartsWith("//"))
                {
                }
                else
                {
                    var checkbox = new CheckBox();
                    checkbox.Name = "chk_" + k.Trim();
                    checkbox.Content = "Enable - " + k.Trim()
                        .Replace("PASSSHOT_CONTEXTEFFECT_", "Passing/Shooting ")
                        .Replace("TRAP_CONTEXTEFFECT_", "Ball Control ")
                        .Replace("DRIBBLE_ERROR_", "Dribble Error ")
                        .Replace("_", "")
                        ;
                    checkbox.Content = Regex.Replace(checkbox.Content.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
                    checkbox.VerticalAlignment = VerticalAlignment.Top;
                    checkbox.VerticalContentAlignment = VerticalAlignment.Top;
                    Grid.SetColumn(checkbox, 0);
                    Grid.SetRow(checkbox, i);
                    g.Children.Add(checkbox);

                    var c = new Slider();
                    c.Name = k.Trim();
                    c.Minimum = 0;
                    c.Maximum = 100;
                    c.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                    c.TickFrequency = 5;
                    c.IsSnapToTickEnabled = true;
                    c.Width = 200;
                    c.Value = 50;
                    c.VerticalAlignment = VerticalAlignment.Top;
                    c.VerticalContentAlignment = VerticalAlignment.Top;
                    Grid.SetColumn(c, 1);
                    Grid.SetRow(c, i);
                    g.Children.Add(c);


                    var labelSliderIndicator = new TextBox();
                    labelSliderIndicator.TextAlignment = TextAlignment.Right;
                    labelSliderIndicator.Width = 40;
                    labelSliderIndicator.VerticalAlignment = VerticalAlignment.Top;
                    labelSliderIndicator.VerticalContentAlignment = VerticalAlignment.Top;
                    Grid.SetColumn(labelSliderIndicator, 2);
                    Grid.SetRow(labelSliderIndicator, i);
                    g.Children.Add(labelSliderIndicator);


                    labelSliderIndicator.SetBinding(TextBox.TextProperty,
                        new Binding("Value") { Source = c });

                    var tb = new TextBlock();
                    tb.MinHeight = 100;
                    tb.TextWrapping = TextWrapping.WrapWithOverflow;

                    if(FIFAModdingUI.Resources.ResourceManager.GetString(k.Trim()) != null) {
                        var resourceDescription = FIFAModdingUI.Resources.ResourceManager.GetString(k.Trim());
                        tb.Text = resourceDescription;
                    }
                    Grid.SetColumn(tb, 3);
                    Grid.SetRow(tb, i);
                    g.Children.Add(tb);

                    g.RowDefinitions.Add(new RowDefinition() { });

                    i++;
                }
            }
            this.panel_GP_ContextEffect.Children.Add(g);

            // read from locale.ini 
            if (!chkUseBaseFIFAINI.IsChecked.Value && !string.IsNullOrEmpty(FIFALocaleIni))
            {
                var localeini = new IniReader(FIFALocaleIni);
                foreach (var k in localeini.GetKeys("").OrderBy(x => x))
                {
                    foreach (var c in panel_GP_ContextEffect.Children)
                    {
                        Grid childGrid = c as Grid;
                        if (childGrid != null)
                        {
                            foreach (CheckBox childCheckBox in childGrid.Children.OfType<CheckBox>())
                            {
                                if (childCheckBox.Name.Trim() == "chk_" + k.Trim())
                                {
                                    var value = localeini.GetValue(k);
                                    childCheckBox.IsChecked = true;
                                }
                            }

                            foreach (Slider childSlider in childGrid.Children.OfType<Slider>())
                            {
                                if (childSlider.Name.Trim() == k.Trim())
                                {
                                    if (double.TryParse(localeini.GetValue(k), out double value))
                                    {
                                        childSlider.Value = value;
                                    }
                                    else
                                    {
                                        throw new InvalidCastException("Unable to convert " + k.Trim() + " with a value of " + localeini.GetValue(k));
                                    }
                                }
                            }
                        }
                    }
                }
            }



        }

        private void InitializeAIObjectiveSystem()
        {
            panel_GP_ObjectiveSystem.Children.Clear();

            //var aiobjsystem = new IniReader("ini/AIObjectiveSystem.ini");
            var aiobjsystem = new IniReader("AIObjectiveSystem.ini", true);
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
                foreach (var k in localeini.GetKeys("").OrderBy(x => x))
                {
                    foreach (Control ch in panel_GP_ObjectiveSystem.Children)
                    {
                        if (ch.GetType() == typeof(CheckBox) && ch.Name.Trim() == k.Trim())
                        {
                            var value = localeini.GetValue(k);
                            ((CheckBox)ch).IsChecked = localeini.GetValue(k) == "1" ? true : false;
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
            var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dialog.Title = "Find your FIFA exe";
            dialog.Multiselect = false;
            dialog.Filter = "exe files (*.exe)|*.exe";
            dialog.FilterIndex = 0;
            dialog.ShowDialog(this);
            var filePath = dialog.FileName;
            FIFADirectory = filePath.Substring(0, filePath.LastIndexOf("\\")+1);
            var fileName = filePath.Substring(filePath.LastIndexOf("\\")+1, filePath.Length - filePath.LastIndexOf("\\")-1);
            if (!string.IsNullOrEmpty(fileName) && CompatibleFIFAVersions.Contains(fileName))
            {
                FIFAInstanceSingleton.FIFAVERSION = fileName.Replace(".exe", "");
                FIFAVersionHelper.GetCustomAttributes(new CMSettings());

                txtFIFADirectory.Text = FIFADirectory;
                MainViewer.IsEnabled = true;
                InitializeIniSettings();

               

            }
            else
            {
                throw new Exception("This Version of FIFA is incompatible with this tool");
            }
        }

        private void Btn_GP_SaveINI_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(FIFAModdingUI.Resources.ResourceManager.GetString("SaveOverOldLocaleINIText"), "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                if (File.Exists(FIFALocaleIni))
                {
                    File.Copy(FIFALocaleIni, FIFADirectory + "\\Data\\locale.ini." + DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss") + ".backup");
                    File.Delete(FIFALocaleIni);
                }

                using (StreamWriter stream = new StreamWriter(FIFALocaleIni))
                {
                    stream.Write(GetResultingLocaleINIString());
                }
            }
        }

        protected void tabRawFileView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            tbRawFileView.Text = GetResultingLocaleINIString();
        }

        private void MainViewer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbRawFileView.Text = GetResultingLocaleINIString();
        }

        public string GetResultingLocaleINIString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("// -------------------------------------------------");
            sb.AppendLine("// CREATED BY paulv2k4 FIFA Modding UI            //");
            sb.AppendLine("// -------------------------------------------------");
            sb.AppendLine("");


            sb.AppendLine("// Languages");
            var file = new IniReader("_FIFA19_base.ini", true);
            foreach (var s in file.GetSections())
            {
                sb.AppendLine("[" + s + "]");
                foreach (var k in file.GetKeys(s))
                {
                    if (!k.StartsWith("//"))
                    {
                        var v = file.GetValue(k, s);
                        //if (s.Contains("LOCALE") && k.Contains("DEFAULT_LANGUAGE"))
                        //{
                        //    // get changed language settings
                        //    if (chkUseBaseFIFAINI.IsChecked.HasValue && !chkUseBaseFIFAINI.IsChecked.Value)
                        //    {
                        //        v = cbLanguages.SelectedValue.ToString();
                        //    }
                        //}

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
            sb.AppendLine("// Context System");
            sb.AppendLine("[]");
            foreach (var c in panel_GP_ContextEffect.Children)
            {
                Grid childGrid = c as Grid;
                if (childGrid != null)
                {
                    foreach (Slider childSlider in childGrid.Children.OfType<Slider>())
                    {
                        foreach (CheckBox childCheckBox in childGrid.Children.OfType<CheckBox>().Where(x=>x.Name.Contains(childSlider.Name)))
                        {
                            if (childCheckBox.IsChecked.HasValue && childCheckBox.IsChecked.Value)
                                sb.AppendLine(childSlider.Name + "=" + Math.Round(childSlider.Value));
                        }
                    }
                }
            }

            sb.AppendLine("");
            if (chkEnableUnlockBootsAndCelebrations.IsChecked.HasValue && chkEnableUnlockBootsAndCelebrations.IsChecked.Value)
            {
                sb.AppendLine("// Unlock Boots");
                file = new IniReader("UnlockBootsAndCelebrations.ini", true);
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
            sb.AppendLine("AI_USE_ATTRIBULATOR_TO_UPDATE_GENERIC_CONVERT_TBL=1");
            foreach (StackPanel container in panel_GP_AttributeWeightSystem.Children.OfType<StackPanel>())
            {
                foreach (Slider slider in container.Children.OfType<Slider>())
                {
                    sb.AppendLine(slider.Name + "=" + Math.Round(slider.Value));
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
            
            

            //sb.AppendLine("ContextEffectTrapBallInAngle=50");
            //sb.AppendLine("ContextEffectTrapBallXZVelocity=75");
            //sb.AppendLine("DEBUG_DISABLE_LOSE_ATTACKER_EFFECT = 1");

            sb.AppendLine("FALL=50");
            sb.AppendLine("STUMBLE=10");

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


            if(chkSkipBootFlow.IsChecked.HasValue && chkSkipBootFlow.IsChecked.Value && !FIFAInstanceSingleton.FIFAVERSION.Contains("demo"))
            {
                sb.AppendLine("");
                sb.AppendLine("[]");
                sb.AppendLine("SKIP_BOOTFLOW=1");
            }


            //sb.AppendLine("");
            //sb.AppendLine("// Cross Test");
            //sb.AppendLine("[]");
            //sb.AppendLine("CROSS=0");
            //sb.AppendLine("SCOOP=0");
            //sb.AppendLine("CROSS_LOW=100");
            //sb.AppendLine("CROSS_GROUND=100");
            //sb.AppendLine("GROUNDCROSS=100");


            if (KILL_EVERYONE.IsChecked.HasValue && KILL_EVERYONE.IsChecked.Value)
            {
                sb.AppendLine("");
                sb.AppendLine("// AI Fouling");
                sb.AppendLine("[]");
                sb.AppendLine("KILL_EVERYONE=" + (KILL_EVERYONE.IsChecked.Value ? "1" : "0"));
                sb.AppendLine("foulstrictness=2");
                sb.AppendLine("Cardstrictness=0");
                sb.AppendLine("REFEREE_CARD_STRICTNESS_OVERRIDE=1");
                sb.AppendLine("REFEREE_FOUL_STRICTNESS_OVERRIDE=2");
                sb.AppendLine("REF_STRICTNESS = 2");
                for(var iRS = 0; iRS < 20; iRS++) {  
                    sb.AppendLine("RefStrictness_" + iRS.ToString() + "=2");
                }
                for (var iRS = 0; iRS < 20; iRS++)
                {
                    sb.AppendLine("CardStrictness_" + iRS.ToString() + "=0");
                }
                sb.AppendLine("SLIDE_TACKLE = 1");
                sb.AppendLine("SLIDETACKLE = 1");
                sb.AppendLine("STAND_TACKLE = 0");
                sb.AppendLine("STANDTACKLE = 0");
                sb.AppendLine("TACKLE = 0");
            }

            if (FIFAInstanceSingleton.FIFAVERSION.Contains("demo"))
            {
                sb.AppendLine("");
                sb.AppendLine("[]");
                sb.AppendLine("FIFA_DEMO = 1");
                sb.AppendLine("[STORY_MODE]");
                sb.AppendLine("DISABLE_ONBOARDING=1");
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
