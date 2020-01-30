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

            //HookDLLThread = new TaskFactory().StartNew(HookFIFADLL);

            this.Closed += MainWindow_Closed;

            EventHookedIntoFIFA += HandleCustomEvent;


            var tskCheckForF2 = new TaskFactory().StartNew(async () =>
            {
                while (true)
                {
                    await Task.Delay(200);
                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        if (Keyboard.IsKeyDown(Key.F2))
                        {
                            if (OverlayWindow == null)
                            {
                                OverlayWindow = new OverlayWindow();
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
                panelHookedIntoFIFA.Visibility = Visibility.Visible;
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
                        c.Minimum = -100;
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

                    c.Minimum = 0;
                    c.Maximum = 900;
                    c.TickFrequency = 2;
                    c.Value = 50;
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
            FIFADirectory = filePath.Substring(0, filePath.LastIndexOf("\\")+1);
            var fileName = filePath.Substring(filePath.LastIndexOf("\\")+1, filePath.Length - filePath.LastIndexOf("\\")-1);
            if (!string.IsNullOrEmpty(fileName) && CompatibleFIFAVersions.Contains(fileName))
            {
                FIFAInstanceSingleton.FIFAVERSION = fileName.Replace(".exe", "");
                FIFAVersionHelper.GetCustomAttributes(new CMSettings());

                txtFIFADirectory.Text = FIFADirectory;
                MainViewer.IsEnabled = true;

                //HookFIFADLL();


                //using var injector = new Injector(fileName, "dllPath", InjectionMethod.CreateThread, InjectionFlags.None);

                //// Inject the DLL into the process

                //var dllBaseAddress = injector.InjectDll();

                //// Eject the DLL from the process

                //injector.EjectDll();


                InitializeIniSettings();

                var myDocs = SpecialDirectories.MyDocuments + "\\"
                    + FIFAInstanceSingleton.FIFAVERSION.Substring(0, 4) + " " + FIFAInstanceSingleton.FIFAVERSION.Substring(4, 2)
                    + "\\settings\\";

                CareerSaves.ItemsSource = Directory.GetFiles(myDocs);


                //
                //CareerSaves.Items.Add()
               

            }
            else
            {
                throw new Exception("This Version of FIFA is incompatible with this tool");
            }
        }

        protected void CareerSaves_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = sender as ListViewItem;
            if(item != null)
            {
                var iFile = item.Content.ToString();
                new TaskFactory().StartNew(() =>
                {
                    var careerFile = new CareerFile(iFile, @"C:\Program Files (x86)\RDBM 20\RDBM20\Templates\FIFA 20\fifa_ng_db-meta.XML");

                    List<DataSet> dataSets = new List<DataSet>();
                    if (careerFile != null)
                    {
                        foreach (var dbf in careerFile.Databases.Where(x => x != null))
                        {
                            dataSets.Add(dbf.ConvertToDataSet());
                        }
                    }
                    if (dataSets.Count > 0)
                    {

                        Dispatcher.Invoke(() =>
                        {
                            foreach (DataColumn col in dataSets[1].Tables["players"].Columns)
                            {
                                dgCareerSave.Columns.Add(new DataGridTextColumn
                                {
                                    Header = col.ColumnName,
                                    Binding = new Binding(string.Format("[{0}]", col.ColumnName))
                                });
                            }
                            dgCareerSave.DataContext = dataSets[1].Tables["players"].Select("playerid = '41'");
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
                            {
                                //if (!childSlider.Name.StartsWith("ContextEffect"))
                                //{
                                //var vString = (childSlider.Value / 100).ToString();
                                ////if (!vString.Contains("."))
                                ////    vString += ".f";
                                ////else
                                ////    vString += "f";

                                //sb.AppendLine(childSlider.Name + "=" + Math.Round(childSlider.Value, 0));
                                //}
                                //else
                                //{
                                sb.AppendLine(childSlider.Name + "=" + Math.Round(childSlider.Value, 2));
                                //}
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
            //
            sb.AppendLine("RIGHTKNEE=13.0");
            sb.AppendLine("LEFTKNEE=13.0");
            sb.AppendLine("FRONT=20.0");
            sb.AppendLine("FORCE_BACK=0.2");
            sb.AppendLine("FORCE_OUTSIDE=1.0");

            //sb.AppendLine("AccelerationGain=0.04");
            //sb.AppendLine("DecelerationGain=2.00");
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
                sb.AppendLine("HOME_OFFENSE_DIFFICULTY=1");
                sb.AppendLine("HOME_DEFENSE_DIFFICULTY=1");
                sb.AppendLine("AWAY_OFFENSE_DIFFICULTY=1");
                sb.AppendLine("AWAY_DEFENSE_DIFFICULTY=1");
                sb.AppendLine("HOME_DIFFICULTY=1");
                sb.AppendLine("AWAY_DIFFICULTY=1");
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
            OverlayWindow overlayWindow = new OverlayWindow();
            overlayWindow.Show();
        }

        
    }
}
