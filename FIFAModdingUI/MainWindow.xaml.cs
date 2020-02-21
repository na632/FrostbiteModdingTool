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
using System.Runtime.InteropServices;
using System.Diagnostics;
using Simple_Injector.Etc;
using System.Data;
using System.Threading;
using Microsoft.VisualBasic.FileIO;
using v2k4FIFAModdingCL.CGFE;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Toolkit.Wpf.UI.Controls;
using FrostySdk.IO;
using v2k4FIFAModding.Career;
using System.Collections.ObjectModel;

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

        //public Task HookDLLThread;
        public OverlayWindow OverlayWindow;

        public MainWindow()
        {
            InitializeComponent();
            InitializeIniSettings();
            GetListOfModsAndOrderThem();
            //HookDLLThread = new TaskFactory().StartNew(HookFIFADLL);

            try
            {
                if (!string.IsNullOrEmpty(AppSettings.Settings.FIFAInstallEXEPath))
                {
                    txtFIFADirectory.Text = AppSettings.Settings.FIFAInstallEXEPath;
                    InitializeOfSelectedFIFA(AppSettings.Settings.FIFAInstallEXEPath);
                }
            }
            catch(Exception e)
            {
                txtFIFADirectory.Text = "";
                Trace.WriteLine(e.ToString());
            }


            this.Closed += MainWindow_Closed;

            EventHookedIntoFIFA += HandleCustomEvent;

            var tskCheckForF2 = new TaskFactory().StartNew(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);
                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        if (Keyboard.IsKeyDown(Key.F2))
                        {
                            if (OverlayWindow == null)
                            {
                                OverlayWindow = new OverlayWindow(this);
                                OverlayWindow.Closed += (o, e) => OverlayWindow = null;
                                OverlayWindow.Show();
                                // Cannot close for 2 seconds
                                await Task.Delay(2000);
                            }
                            else
                            {
                                OverlayWindow.Hide();
                                OverlayWindow.Close();
                                await Task.Delay(1000);
                            }


                        }
                    });
                    
                }
            });
        }


        private void MainWindow_Closed(object sender, EventArgs e)
        {
            //HookDLLThread.Dispose();

            //if(HookedFIFA)
            //    CloseHook_OUT();
        }

        // Define what actions to take when the event is raised.
        void HandleCustomEvent(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => {
                TabCareer.IsEnabled = true;
                //panelHookedIntoFIFA.Visibility = Visibility.Visible;
            });
        }

        //[DllImport("FIFACareerDLL.dll")]
        //public static extern int GetTransferBudget_OUT();

        //[DllImport("FIFACareerDLL.dll")]
        //public static extern bool RequestAdditionalFunds_OUT();

        //[DllImport("FIFACareerDLL.dll")]
        //public static extern void CloseHook_OUT();

        //[DllImport("FIFACareerDLL.dll")]
        //public static extern bool CareerModeLoaded_OUT();

        //[DllImport("user32.dll")]
        //private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        //[DllImport("user32.dll")]
        //static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //private static readonly Simple_Injection.Injector Injector = new Simple_Injection.Injector();

        //private static readonly Status Status = new Status();

        //private readonly Config _config = new Config();

        //private readonly DataTable _processTable = new DataTable();

        //public static Status Inject(Config config)
        //{
        //    // Inject using specified method

        //    switch (config.InjectionMethod)
        //    {
        //        case "CreateRemoteThread":

        //            if (Injector.CreateRemoteThread(config.DllPath, config.ProcessName))
        //            {
        //                Status.InjectionOutcome = true;
        //            }

        //            break;

        //        case "RtlCreateUserThread":

        //            if (Injector.RtlCreateUserThread(config.DllPath, config.ProcessName))
        //            {
        //                Status.InjectionOutcome = true;
        //            }

        //            break;

        //        case "SetThreadContext":

        //            if (Injector.SetThreadContext(config.DllPath, config.ProcessName))
        //            {
        //                Status.InjectionOutcome = true;
        //            }

        //            break;
        //    }

        //    // Erase headers if EraseHeaders is checked

        //    if (config.EraseHeaders)
        //    {
        //        if (Injector.EraseHeaders(config.DllPath, config.ProcessName))
        //        {
        //            Status.EraseHeadersOutcome = true;
        //        }
        //    }

        //    return Status;
        //}

        public event EventHandler EventHookedIntoFIFA;
        public event EventHandler EventHookedIntoFIFACareerMode;

        private bool _hookedFIFA;
        private bool HookedFIFA
        {
            get { return _hookedFIFA;  }
            set { _hookedFIFA = value; EventHookedIntoFIFA(this, null); }
        }

        private bool _hookedFIFACareerMode;
        private bool HookedFIFACareerMode
        {
            get { return _hookedFIFACareerMode; }
            set { _hookedFIFACareerMode = value; EventHookedIntoFIFACareerMode(this, null); }
        }

        //public Process FIFAProcess;

        //private async void HookFIFADLL()
        //{
        //    while (!HookedFIFA)
        //    {
        //        await Task.Delay(600);
        //        if (FIFAInstanceSingleton.FIFAVERSION == "FIFA20")
        //        {
        //            // the target process - I'm using a dummy process for this
        //            // if you don't have one, open Task Manager and choose wisely
        //            var processes = Process.GetProcesses();
        //            if (processes.Any(p => p.ProcessName.Contains("FIFA20")))
        //            {
        //                FIFAProcess = processes.FirstOrDefault(x => x.ProcessName == "FIFA20");

        //                _config.DllPath = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("FIFAModdingUI.dll", "") + "\\FIFACareerDLL.dll";

        //                //var budget = GetTransferBudget_OUT();
        //                //if (budget != 0)
        //                //{

        //                //}
                        
        //                if (File.Exists(_config.DllPath))
        //                {
        //                    await Task.Delay(10000);

        //                    _config.ProcessName = FIFAProcess.ProcessName;
        //                    _config.InjectionMethod = "CreateRemoteThread";
        //                    Status status = Inject(_config);
        //                    if(status.InjectionOutcome)
        //                    {
        //                        HookedFIFA = true;
        //                        //while(!HookedFIFACareerMode)
        //                        //{
        //                        //    if (CareerModeLoaded_OUT())
        //                        //        HookedFIFACareerMode = true;
        //                        //}
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

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
                            chkSliderFOOT.IsChecked = true;
                            break;
                        case "RIGHTUPPERLEG":
                            sliderUPPERLEG.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            chkSliderUPPERLEG.IsChecked = true;
                            break;
                        case "RIGHTANKLE":
                            sliderANKLE.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            chkSliderANKLE.IsChecked = true;
                            break;
                        case "TORSO":
                            sliderTORSO.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            chkSliderTORSO.IsChecked = true;
                            break;
                        case "HIPS":
                            sliderHIPS.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            chksliderHIPS.IsChecked = true;
                            break;
                        case "BACKTORSO":
                            sliderBACKTORSO.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            chkSliderBACKTORSO.IsChecked = true;
                            break;
                        case "RIGHTARM":
                            sliderARM.Value = Convert.ToDouble(localeini.GetValue(k, ""));
                            chkSliderARM.IsChecked = true;
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

                        case "POSSESSION_TOUCH":
                            chkPossessionTouch.IsChecked = true;
                            sliderPossessionTouch.Value = Convert.ToDouble(localeini.GetValue(k, "").Replace("f", ""));
                            break;

                        case "CONTEXTUAL_TURN":
                            chkContextualTurn.IsChecked = true;
                            sliderContextualTurn.Value = Convert.ToDouble(localeini.GetValue(k, "").Replace("f", ""));
                            break;
                    }
                }
                var cpuaikeys = localeini.GetKeys("CPUAI");
                if(cpuaikeys.Length > 0)
                {
                    foreach (var k in cpuaikeys)
                    {
                        switch (k.Trim())
                        {
                            case "HOME_OFFENSE_DIFFICULTY":
                                chkDifficultyOffense.IsChecked = true;
                                sliderDifficultyOffense.Value = Convert.ToDouble(localeini.GetValue(k, "CPUAI").Replace("f", ""));
                                break;
                            case "HOME_DEFENSE_DIFFICULTY":
                                chkDifficultyDefense.IsChecked = true;
                                sliderDifficultyDefense.Value = Convert.ToDouble(localeini.GetValue(k, "CPUAI").Replace("f", ""));
                                break;
                            case "HOME_DIFFICULTY":
                                chkDifficultyHome.IsChecked = true;
                                sliderDifficultyHome.Value = Convert.ToDouble(localeini.GetValue(k, "CPUAI").Replace("f", ""));
                                break;
                            case "AWAY_DIFFICULTY":
                                chkDifficultyAway.IsChecked = true;
                                sliderDifficultyAway.Value = Convert.ToDouble(localeini.GetValue(k, "CPUAI").Replace("f", ""));
                                break;

                        }
                    }
                }

                //sb.AppendLine("[]");
                //sb.AppendLine("ADAPTIVE_DIFFICULTY=0");
                //sb.AppendLine("OVERRIDE_HOME_DEFENSE_DIFFICULTY=1");
                //sb.AppendLine("OVERRIDE_HOME_OFFENSE_DIFFICULTY=1");
                //sb.AppendLine("OVERRIDE_AWAY_OFFENSE_DIFFICULTY=1");
                //sb.AppendLine("OVERRIDE_AWAY_DEFENSE_DIFFICULTY=1");
                //sb.AppendLine("");
                //sb.AppendLine("[CPUAI]");
                //sb.AppendLine("HOME_OFFENSE_DIFFICULTY=0.88");
                //sb.AppendLine("HOME_DEFENSE_DIFFICULTY=0.21");
                //sb.AppendLine("AWAY_OFFENSE_DIFFICULTY=0.88");
                //sb.AppendLine("AWAY_DEFENSE_DIFFICULTY=0.21");
                //sb.AppendLine("HOME_DIFFICULTY=0.23");
                //sb.AppendLine("AWAY_DIFFICULTY=0.23");

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
                        if (FIFAInstanceSingleton.FIFAVERSION.Contains("20"))
                        {
                            c.Minimum = 0;
                            c.Maximum = 1;
                            c.Width = 350;
                            c.TickFrequency = 0.05;
                        }
                        else
                        {
                            c.Minimum = -100;
                            c.Maximum = 350;
                            c.Width = 350;
                            c.TickFrequency = 5;
                        }

                        var v = aiobjsystem.GetValue(k, "").Trim();
                        c.Value = Convert.ToDouble(v);
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
            g.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(500) });
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

                    if (FIFAInstanceSingleton.FIFAVERSION.Contains("20"))
                    {
                        c.Minimum = 0;
                        c.Maximum = 1;
                        c.TickFrequency = 0.05;
                        c.Value = 1;
                    }
                    else
                    {
                        c.Minimum = 0;
                        c.Maximum = 900;
                        c.TickFrequency = 2;
                        c.Value = 50;
                    }


                   
                    c.IsSnapToTickEnabled = true;

                    //if (!c.Name.StartsWith("ContextEffect"))
                    //{
                    //    c.Maximum = 900;
                    //}
                    //else
                    //{
                    //    c.Minimum = 0;
                    //    c.Maximum = 1;
                    //    c.TickFrequency = 0.1;
                    //    c.Ticks = new DoubleCollection(10);
                    //    c.Value = 0.5;
                    //}

                    c.TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight;
                    c.Width = 200;
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
                    var kTrimmed = k.Trim();
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
                                    if (float.TryParse(localeini.GetValue(k).Replace(".f", "").Replace("f",""), out float fV))
                                    {
                                        if (double.TryParse(fV.ToString(), out double value))
                                        {
                                            //childSlider.Value = kTrimmed.StartsWith("ContextEffect") ? value : value * 100;
                                            childSlider.Value = value;
                                        }
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
            //var aiobjsystem = new IniReader("AIObjectiveSystem.ini", true);
            var aiobjsystem = new IniReader("AIObjectiveSystem_" + FIFAInstanceSingleton.FIFAVERSION_NODEMO + ".ini", true);
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
            InitializeOfSelectedFIFA(filePath);
        }

        private void InitializeOfSelectedFIFA(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                AppSettings.Settings.FIFAInstallEXEPath = filePath;
                AppSettings.Settings.Save();

                FIFADirectory = filePath.Substring(0, filePath.LastIndexOf("\\") + 1);
                FIFAInstanceSingleton.FIFARootPath = FIFADirectory;
                var fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1, filePath.Length - filePath.LastIndexOf("\\") - 1);
                if (!string.IsNullOrEmpty(fileName) && CompatibleFIFAVersions.Contains(fileName))
                {
                    FIFAInstanceSingleton.FIFAVERSION = fileName.Replace(".exe", "");

                    txtFIFADirectory.Text = FIFADirectory;
                    MainViewer.IsEnabled = true;

                    LoadingProgressBar.Value = 0;
                    InitializeIniSettings();
                    LoadingProgressBar.Value = 100;
                    Thread.Sleep(10);
                    LoadingProgressBar.Value = 0;
                    InitializeCareerSaves();
                    LoadingProgressBar.Value = 100;
                }
                else
                {
                    throw new Exception("This Version of FIFA is incompatible with this tool");
                }
            }
        }

        private void InitializeCareerSaves()
        {
            var myDocs = SpecialDirectories.MyDocuments + "\\"
                                            + FIFAInstanceSingleton.FIFAVERSION.Substring(0, 4) + " " + FIFAInstanceSingleton.FIFAVERSION.Substring(4, 2)
                                            + "\\settings\\";
            Dictionary<string, string> results = CareerUtil.GetCareerSaves(myDocs);

            CareerSaves.ItemsSource = results;
        }

        

        CareerFile CareerFile { get; set; }
        protected void CareerSaves_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            if(item != null)
            {
                CareerOpened.Header = "";
                CareerOpened.IsEnabled = false;

                var iFile = item.Content.ToString().Replace("{", "").Replace("[", "").Split(",")[0];
                new TaskFactory().StartNew(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        LoadingProgressBar.Value = 0;
                    });
                    CareerFile = new CareerFile(iFile, @"C:\Program Files (x86)\RDBM 20\RDBM20\Templates\FIFA 20\fifa_ng_db-meta.XML");
                    Dispatcher.Invoke(() =>
                    {
                        LoadingProgressBar.Value = 10;
                    });

                    List<DataSet> dataSets = new List<DataSet>();
                    if (CareerFile != null)
                    {
                        int indexOfFiles = 0;
                        foreach (var dbf in CareerFile.Databases.Where(x => x != null))
                        {
                            var convertedDS = dbf.ConvertToDataSet();
                            string json = JsonConvert.SerializeObject(convertedDS, Formatting.Indented);
                            File.WriteAllText(dbf.FileName + "_" + indexOfFiles + ".json", json);
                            indexOfFiles++;
                            dataSets.Add(convertedDS);
                            Dispatcher.Invoke(() =>
                            {
                                LoadingProgressBar.Value += 20;
                            });

                        }
                    }
                    if (dataSets.Count > 0)
                    {
                       

                        Dispatcher.Invoke(() =>
                        {
                            CareerOpened.Header = CareerFile.InGameName;
                            CareerOpened.IsEnabled = true;

                            CareerDatabaseTables.ItemsSource = new List<DataTable>(dataSets[1].Tables.Cast<DataTable>()).Select(x => x.TableName).OrderBy(x => x);

                            LoadingProgressBar.Value = 100;
                        });

                    }
                });
            }
        }

        List<DataSet> CareerDatabaseDataSets { get; set; }
        int CareerDatabaseTable_DataSetIndex = 1;
        List<DataRow> CareerDatabaseTable { get; set; }
        int CareerDatabaseTable_CurrentPage = 1;
        int CareerDatabaseTable_RecordPerPage = 80;

        private void dgCareerDatabaseTable_KeyUp(object sender, KeyEventArgs e)
        {

        }

        protected void CareerDatabaseTables_Click(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            if (item != null)
            {
                var iFile = item.Content.ToString();
                new TaskFactory().StartNew(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        LoadingProgressBar.Value = 0;
                    });

                    CareerDatabaseDataSets = new List<DataSet>();
                    if (CareerFile != null)
                    {
                        foreach (var dbf in CareerFile.Databases.Where(x => x != null))
                        {
                            CareerDatabaseDataSets.Add(dbf.ConvertToDataSet());
                            Dispatcher.Invoke(() =>
                            {
                                LoadingProgressBar.Value += 20;
                            });

                        }
                    }
                    if (CareerDatabaseDataSets.Count > 0)
                    {

                        Dispatcher.Invoke(() =>
                        {
                            CareerDatabaseTable = CareerDatabaseDataSets[CareerDatabaseTable_DataSetIndex].Tables[iFile].AsEnumerable().ToList();
                            CareerDatabaseTable_CurrentPage = 1;
                            var numOfPages = CareerDatabaseTable.Count() / CareerDatabaseTable_RecordPerPage;
                            for (var indx = 0; indx < numOfPages; indx++)
                            {
                                var b = new Button()
                                {
                                    Content = indx + 1
                                    , Width = 25
                                    , Margin = new Thickness(0,0,5,0)
                                };
                                b.Click += (object bS, RoutedEventArgs bE) => {
                                    var internal_btn = (Button)bS;
                                    CareerDatabaseTable_CurrentPage = Convert.ToInt32(internal_btn.Content);
                                    dgCareerDatabaseTable.DataContext = CareerDatabaseTable.Skip((CareerDatabaseTable_CurrentPage - 1) * CareerDatabaseTable_RecordPerPage).Take(CareerDatabaseTable_RecordPerPage).ToList();
                                };
                                dgCareerDatabaseTablePages.Children.Add(b);


                            }

                            LoadingProgressBar.Value += 75;

                            dgCareerDatabaseTable.Columns.Clear();
                            foreach (DataColumn col in CareerDatabaseDataSets[CareerDatabaseTable_DataSetIndex].Tables[iFile].Columns)
                            {
                                dgCareerDatabaseTable.Columns.Add(new DataGridTextColumn
                                {
                                    Header = col.ColumnName,
                                    Binding = new Binding(string.Format("[{0}]", col.ColumnName))
                                });
                            }
                            dgCareerDatabaseTable.DataContext = CareerDatabaseTable.Skip((CareerDatabaseTable_CurrentPage - 1) * CareerDatabaseTable_RecordPerPage).Take(CareerDatabaseTable_RecordPerPage).ToList(); 

                            LoadingProgressBar.Value = 100;
                        });

                    }
                });
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
            var file = new IniReader("_" + FIFAInstanceSingleton.FIFAVERSION_NODEMO + "_base.ini", true);
            foreach (var s in file.GetSections())
            {
                sb.AppendLine("[" + s + "]");
                foreach (var k in file.GetKeys(s))
                {
                    if (!k.StartsWith("//"))
                    {
                        var v = file.GetValue(k, s);
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
                            {
                                sb.AppendLine(childSlider.Name + "=" + Math.Round(childSlider.Value, 2));
                            }
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
            if (chkSliderUPPERLEG.IsChecked.HasValue && chkSliderUPPERLEG.IsChecked.Value)
            {
                sb.AppendLine("RIGHTUPPERLEG=" + Math.Round(sliderUPPERLEG.Value, 2));
                sb.AppendLine("LEFTUPPERLEG=" + Math.Round(sliderUPPERLEG.Value, 2));
            }
            if (chkSliderFOOT.IsChecked.HasValue && chkSliderFOOT.IsChecked.Value)
            {
                sb.AppendLine("RIGHTFOOT=" + Math.Round(sliderFOOT.Value, 2));
                sb.AppendLine("LEFTFOOT=" + Math.Round(sliderFOOT.Value, 2));
            }
            if (chkSliderANKLE.IsChecked.HasValue && chkSliderANKLE.IsChecked.Value)
            {
                sb.AppendLine("RIGHTANKLE=" + Math.Round(sliderANKLE.Value, 2));
                sb.AppendLine("LEFTANKLE=" + Math.Round(sliderANKLE.Value, 2));
            }
            if (chkSliderTORSO.IsChecked.HasValue && chkSliderTORSO.IsChecked.Value)
            {
                sb.AppendLine("TORSO=" + Math.Round(sliderTORSO.Value, 2));
            }
            if (chksliderHIPS.IsChecked.HasValue && chksliderHIPS.IsChecked.Value)
            {
                sb.AppendLine("HIPS=" + Math.Round(sliderHIPS.Value, 2));
            }
            if (chkSliderBACKTORSO.IsChecked.HasValue && chkSliderBACKTORSO.IsChecked.Value)
            {
                sb.AppendLine("BACKTORSO=" + Math.Round(sliderBACKTORSO.Value, 2));
            }
            if (chkSliderARM.IsChecked.HasValue && chkSliderARM.IsChecked.Value)
            {
                sb.AppendLine("RIGHTARM=" + Math.Round(sliderARM.Value, 2));
                sb.AppendLine("LEFTARM=" + Math.Round(sliderARM.Value, 2));
                sb.AppendLine("RIGHTHAND=" + Math.Round(sliderARM.Value, 2));
                sb.AppendLine("LEFTHAND=" + Math.Round(sliderARM.Value, 2));
            }
            sb.AppendLine("");

            sb.AppendLine("");
            sb.AppendLine("// ATTRIBUTES");
            sb.AppendLine("[]");
            sb.AppendLine("AI_USE_ATTRIBULATOR_TO_UPDATE_GENERIC_CONVERT_TBL=1");
            foreach (StackPanel container in panel_GP_AttributeWeightSystem.Children.OfType<StackPanel>())
            {
                foreach (Slider slider in container.Children.OfType<Slider>())
                {
                    sb.AppendLine(slider.Name + "=" + Math.Round(slider.Value, 2));
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
            sb.AppendLine("RULESCOLLISION_ENABLE_INTENDED_BALLTOUCH=0");

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

            // CPU AI
            sb.AppendLine("");
            sb.AppendLine("[]");
            sb.AppendLine("ADAPTIVE_DIFFICULTY=0");
            if (chkDifficultyHome.IsChecked.Value || chkDifficultyDefense.IsChecked.Value || chkDifficultyOffense.IsChecked.Value)
            {
                sb.AppendLine("OVERRIDE_HOME_DEFENSE_DIFFICULTY=1");
                sb.AppendLine("OVERRIDE_HOME_OFFENSE_DIFFICULTY=1");
            }
            if (chkDifficultyAway.IsChecked.Value || chkDifficultyDefense.IsChecked.Value || chkDifficultyOffense.IsChecked.Value)
            {
                sb.AppendLine("OVERRIDE_AWAY_OFFENSE_DIFFICULTY=1");
                sb.AppendLine("OVERRIDE_AWAY_DEFENSE_DIFFICULTY=1");
            }
            sb.AppendLine("");

            if (chkDifficultyHome.IsChecked.Value || chkDifficultyAway.IsChecked.Value || chkDifficultyDefense.IsChecked.Value || chkDifficultyOffense.IsChecked.Value)
            {
                sb.AppendLine("[CPUAI]");
            }

            if(chkDifficultyHome.IsChecked.Value)
                sb.AppendLine("HOME_DIFFICULTY=" + Math.Round(sliderDifficultyHome.Value,2));

            if (chkDifficultyAway.IsChecked.Value)
                sb.AppendLine("AWAY_DIFFICULTY=" + Math.Round(sliderDifficultyAway.Value, 2));

            if (chkDifficultyOffense.IsChecked.Value)
            {
                sb.AppendLine("HOME_OFFENSE_DIFFICULTY=" + Math.Round(sliderDifficultyOffense.Value, 2));
                sb.AppendLine("AWAY_OFFENSE_DIFFICULTY=" + Math.Round(sliderDifficultyOffense.Value, 2));
            }

            if (chkDifficultyDefense.IsChecked.Value)
            {
                sb.AppendLine("HOME_DEFENSE_DIFFICULTY=" + Math.Round(sliderDifficultyDefense.Value, 2));
                sb.AppendLine("AWAY_DEFENSE_DIFFICULTY=" + Math.Round(sliderDifficultyDefense.Value, 2));
            }


            if (chkSkipBootFlow.IsChecked.HasValue && chkSkipBootFlow.IsChecked.Value && !FIFAInstanceSingleton.FIFAVERSION.Contains("demo"))
            {
                sb.AppendLine("");
                sb.AppendLine("[]");
                sb.AppendLine("SKIP_BOOTFLOW=1");
            }

            if (FIFAInstanceSingleton.FIFAVERSION.Contains("20"))
            {
                sb.AppendLine("");
                sb.AppendLine("[]");
                sb.AppendLine("FRONT=19.0");
                sb.AppendLine("FORCE_BACK=0.3");
                sb.AppendLine("FORCE_OUTSIDE=0.9");
                sb.AppendLine("FORCE_INSIDE=1.0");
                sb.AppendLine("FORCE_FRONT=0.9");


                sb.AppendLine("");
                sb.AppendLine("[]");
                sb.AppendLine("DRIBBLE=2.0");

                if(chkPossessionTouch.IsChecked.Value)
                    sb.AppendLine("POSSESSION_TOUCH=" + Math.Round(sliderPossessionTouch.Value, 2));

                if (chkContextualTurn.IsChecked.Value)
                    sb.AppendLine("CONTEXTUAL_TURN=" + Math.Round(sliderContextualTurn.Value, 2));
                //else
                //    sb.AppendLine("CONTEXTUAL_TURN=0.7");

                sb.AppendLine("HARDSTOP=0.1");
                sb.AppendLine("EVASIVE=0.05");
                sb.AppendLine("AVOID=0.1");
                sb.AppendLine("PASS=0.5");
                sb.AppendLine("SHOT=0.501");
                sb.AppendLine("CROSS=0.1");
                sb.AppendLine("THROUGH=0.1");
                sb.AppendLine("LOB=0.2");
                sb.AppendLine("UNBALANCE_TURN=4.0");
                sb.AppendLine("STRAFE=2.0");
                sb.AppendLine("SHIELDING=2.0");
                sb.AppendLine("UNCONTROLLED=1.0");
                sb.AppendLine("LOB_GROUND=3.0");
            
            }
            else
            {
                sb.AppendLine("RIGHTKNEE=13.0");
                sb.AppendLine("LEFTKNEE=13.0");
                sb.AppendLine("FRONT=20.0");
                sb.AppendLine("FORCE_BACK=0.2");
                sb.AppendLine("FORCE_OUTSIDE=1.0");
            }

            //sb.AppendLine("");
            //sb.AppendLine("// Cross Test");
            //sb.AppendLine("[]");
            //sb.AppendLine("Jockey_MarkingDistance=0");
            //sb.AppendLine("Jockey_PadEffectJockeyModifyAngle=60");
            //sb.AppendLine("Jockey_MarkingDistanceShielded=1");
            //sb.AppendLine("Jockey_PadEffectJockeyShieldedModifyAngle=1");
            //sb.AppendLine("Jockey_PadEffectJockeyModifyDistance=1");
            //sb.AppendLine("Jockey_ReactionTime=1");
            //sb.AppendLine("Jockey_PadEffectJockeyShieldedModifyDistance=1");
            //sb.AppendLine("Jockey_ReactionTimeModifier=90");
            //sb.AppendLine("PushPull_BalanceIntensityScale=1");
            //sb.AppendLine("PushPull_FallContextIncreaseBalance=1");
            //sb.AppendLine("PushPull_DiverTraitMod=1000");
            //sb.AppendLine("PushPull_FallContextIncreaseStrength=1");
            //sb.AppendLine("PushPull_FallContextIncreaseDiver=1000");
            //sb.AppendLine("[]");
            //sb.AppendLine("SPINE = 1.0");
            //sb.AppendLine("SPINE1 = 1.0");
            //sb.AppendLine("SPINE2 = 1.0");
            //sb.AppendLine("NECK = 1.0");
            //sb.AppendLine("RIGHTUPLEG = 0.6");
            //sb.AppendLine("LEFTUPLEG = 0.6");
            //sb.AppendLine("RM_Shorts_Back = 1.0");
            //sb.AppendLine("RM_Shorts_Crack = 1.0");
            //sb.AppendLine("RM_Shorts_Crotch = 1.0");
            //sb.AppendLine("RightLeg = 0.6");
            //sb.AppendLine("LeftLeg = 0.6");

            sb.AppendLine("");
            if (KILL_EVERYONE.IsChecked.HasValue)
            {
                sb.AppendLine("// AI Fouling");
                sb.AppendLine("[]");
                sb.AppendLine("KILL_EVERYONE=" + (KILL_EVERYONE.IsChecked.Value ? "1" : "0"));
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

        private void btnRequestAdditionalFunds_Click(object sender, RoutedEventArgs e)
        {
            //txtTransferBudget.Text = GetTransferBudget_OUT().ToString();

            //if (RequestAdditionalFunds_OUT())
            //{
            //    btnRequestAdditionalFunds.Background = Brushes.Green;
            //}
            //else
            //{
            //    btnRequestAdditionalFunds.Background = Brushes.Red;
            //}
        }

        private void btnAddOverlayToGame_Click(object sender, RoutedEventArgs e)
        {
            OverlayWindow overlayWindow = new OverlayWindow(this);
            overlayWindow.Show();
        }

        private void btnLaunchFIFA_Click(object sender, RoutedEventArgs e)
        {
            LaunchFIFA.Launch();
        }

        private ObservableCollection<string> ListOfMods = new ObservableCollection<string>();

        private void up_click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = this.listMods.SelectedIndex;

            if (selectedIndex > 0)
            {
                var itemToMoveUp = this.ListOfMods[selectedIndex];
                this.ListOfMods.RemoveAt(selectedIndex);
                this.ListOfMods.Insert(selectedIndex - 1, itemToMoveUp);
                this.listMods.SelectedIndex = selectedIndex - 1;
            }
        }

        private void down_click(object sender, RoutedEventArgs e)
        {
            var selectedIndex = this.listMods.SelectedIndex;

            if (selectedIndex + 1 < this.ListOfMods.Count)
            {
                var itemToMoveDown = this.ListOfMods[selectedIndex];
                this.ListOfMods.RemoveAt(selectedIndex);
                this.ListOfMods.Insert(selectedIndex + 1, itemToMoveDown);
                this.listMods.SelectedIndex = selectedIndex + 1;
            }
        }

        private void GetListOfModsAndOrderThem()
        {
            ListOfMods = new ObservableCollection<string>(Directory.EnumerateFiles(
                Directory.GetParent(Assembly.GetExecutingAssembly().Location)
                + "\\Mods\\").Where(x => x.ToLower().Contains(".fbmod")).Select(
                f => new FileInfo(f).Name).ToList());
            listMods.ItemsSource = ListOfMods;
        }
    }

    public class AppSettings
    {
        public static AppSettings Settings = new AppSettings("FIFAUIAppSettings.json");

        public AppSettings()
        {

        }
        public AppSettings(string filename)
        {
            this.FileName = filename;
            if (!File.Exists(FullFilePath))
            {
                File.WriteAllText(FullFilePath, JsonConvert.SerializeObject(this));
            }
            Read();

        }
        public string FileName { get; set; }
        public string FullFilePath
        {
            get
            {
                return Directory.GetParent(Assembly.GetExecutingAssembly().Location) + "\\" + FileName;
            }
        }

        #region FIFA
        public string FIFAInstallEXEPath { get; set; }
        public string FIFAFrostyModsDirectory { get; set; }
        #endregion
        public void Save()
        {
            using (StreamWriter sw = new StreamWriter(FullFilePath))
            {
                var serialised = JsonConvert.SerializeObject(this);
                sw.Write(serialised);
            }
        }
        public AppSettings Read()
        {
            var r = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(FullFilePath));
            FIFAInstallEXEPath = r.FIFAInstallEXEPath;
            return r;
        }
    }
}
