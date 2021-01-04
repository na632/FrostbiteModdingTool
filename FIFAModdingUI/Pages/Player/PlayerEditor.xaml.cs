using FrostbiteModdingUI.Models;
using FrostbiteSdk;
using FrostySdk;
using FrostySdk.Frostbite.IO.Output;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
using HelixToolkit.Wpf;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Assimp;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using HelixToolkit.Wpf.SharpDX.Model;
using HelixToolkit.Logger;
using System.Threading;

namespace FrostbiteModdingUI.Pages.Player
{
    /// <summary>
    /// Interaction logic for PlayerEditor.xaml
    /// </summary>
    public partial class PlayerEditor : UserControl
    {
        public class PlayerVM
        {
            public int PlayerId { get; set; }

            public string Firstname { get; set; }
            public string Lastname { get; set; }
            public string Nickname { get; set; }
            public string PlayerName 
            { 
                get 
                {
                    if (!string.IsNullOrEmpty(Nickname))
                        return Nickname;
                    else
                        return Firstname + " " + Lastname;
                }
            } 
            public DataRow Data { get; set; }
        }

        public List<PlayerVM> PlayerList = new List<PlayerVM>(27000);
        public ObservableCollection<PlayerVM> PlayerListFiltered
        {
            get
            {
                return new ObservableCollection<PlayerVM>(PlayerList.Take(50));
            }
        }

        public PlayerVM Player { get; set; }

        public MainViewModel ViewportViewModel { get; } = new MainViewModel();

        public PlayerEditor()
        {
            InitializeComponent();

            Loaded += PlayerEditor_Loaded;
            Viewport.DataContext = ViewportViewModel;
            this.DataContext = this;
        }

        private void PlayerEditor_Loaded(object sender, RoutedEventArgs e)
        {
            PlayerList = new List<PlayerVM>()
            {
                new PlayerVM() { PlayerId = -1, Nickname = "Loading..." },
            };

            

            this.txtPlayerSearch.KeyUp += TxtPlayerSearch_KeyUp;

           // SetupHeadScene();

        }

        public void InitPlayerSearch()
        {
            Task.Run(() =>
            {
                //LoadSquadFile();

                //var legacyFiles = AssetManager.Instance.EnumerateCustomAssets("legacy").OrderBy(x => x.Path).ToList();
                //var mainDb = legacyFiles.FirstOrDefault(x => x.Filename.Contains("fifa_ng_db") && x.Type == "DB");
                //var mainDbMeta = legacyFiles.FirstOrDefault(x => x.Filename.Contains("fifa_ng_db-meta") && x.Type == "XML");
                //LoadPlayerList(mainDb as LegacyFileEntry, mainDbMeta as LegacyFileEntry);
            }).Wait();

            // Binding  
            Dispatcher.Invoke(() =>
            {
                lstPlayerSearch.ItemsSource = PlayerListFiltered;
            });
        }

        CareerFile squadFile;
        DataTable dtPlayers;
        CareerFile locFile;
        DataTable dtPlayerNames;


        public bool LoadSquadFile()
        {
            List<string> files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FIFA 21\\settings\\").ToList();
            if (files != null && files.Any())
            {
                var squadFilePath = files.Where(x => x.Contains("\\Squads")).OrderByDescending(x => new FileInfo(x).LastWriteTime).FirstOrDefault();
                using (var fsSquadFile = new FileStream(squadFilePath, FileMode.Open))
                {
                    using (var fsMeta = new FileStream("Resources/FIFA21DBMeta.xml", FileMode.Open))
                    {
                        squadFile = new CareerFile(fsSquadFile, fsMeta);
                        squadFile.Load();

                        dtPlayers = squadFile.Databases[0].GetTable("players").ConvertToDataTable();
                        return true;
                    }
                }
            }
            return false;
        }

        public void LoadPlayerList(LegacyFileEntry locDB, LegacyFileEntry locMeta)
        {
                if(squadFile == null)
                {
                    var resultOfSquadload = LoadSquadFile();
                    if (!resultOfSquadload)
                    {
                        throw new Exception("Unable to Load Squad File");
                    }
                }
                if (squadFile != null && dtPlayerNames == null)
                {
                    var dbAsset1 = AssetManager.Instance.GetCustomAsset("legacy", locDB);
                    var dbAsset2 = AssetManager.Instance.GetCustomAsset("legacy", locDB);
                    var dbMetaAsset = AssetManager.Instance.GetCustomAsset("legacy", locMeta);

                    DbFile dbFile = new DbFile();
                    dbFile.LoadXml(dbMetaAsset);
                    dbFile.LoadDb(dbAsset2);

                    dtPlayerNames = dbFile.GetTable("playernames").ConvertToDataTable();
                    PlayerList.Clear();
                    foreach (DataRow pRow in dtPlayers.Rows)
                    {
                        PlayerVM player = new PlayerVM();
                        player.PlayerId = int.Parse(pRow["playerid"].ToString());
                        var fnid = int.Parse(pRow["firstnameid"].ToString());
                        var lnid = int.Parse(pRow["lastnameid"].ToString());

                        int? cnid = null;
                        if(pRow["commonnameid"] != null)
                        {
                            cnid = int.Parse(pRow["commonnameid"].ToString());
                        }

                        player.Data = pRow;
                        player.Firstname = "Unknown Firstname";
                        player.Lastname = "Unknown Lastname";
                        if (fnid < dtPlayerNames.Rows.Count)
                        {
                            player.Firstname = dtPlayerNames.Rows[fnid]["name"].ToString();
                            if(lnid < dtPlayerNames.Rows.Count)
                                player.Lastname = dtPlayerNames.Rows[lnid]["name"].ToString();
                        }

                        if(cnid.HasValue)
                        {
                            if (cnid < dtPlayerNames.Rows.Count)
                            {
                                player.Nickname = dtPlayerNames.Rows[cnid.Value]["name"].ToString();
                            }
                        }
                        
                        PlayerList.Add(player);
                    }
                    
                }

                
        }

        private void TxtPlayerSearch_KeyUp(object sender, KeyEventArgs e)
        {
            SearchForPlayer(txtPlayerSearch.Text);
        }

        private void CbPlayerSelection_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            SearchForPlayer(e.Text);
            txtPlayerSearch.Text = e.Text;
        }

        private void SearchForPlayer(string searchText)
        {
            var plCopy = PlayerList.ToList().Where(x => x.PlayerName.ToLower().Contains(searchText.ToLower())).Take(200);
            lstPlayerSearch.ItemsSource = plCopy;
        }

        private void lstPlayerSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            var p = listView.SelectedItem as PlayerVM;

            if (p != null)
            {
                if (File.Exists("test.fbx"))
                    File.Delete("test.fbx");

                Player = p;

                var headEntry = AssetManager.Instance.EnumerateEbx("SkinnedMeshAsset")
                    .FirstOrDefault(x => x.Path.Contains("content/character/player")
                    && x.Filename.Contains("_" + Player.PlayerId)
                    && x.Filename.Contains("head"));
                if (headEntry != null)
                {
                    var skinnedMeshEbx = AssetManager.Instance.GetEbx(headEntry);
                    if (skinnedMeshEbx != null)
                    {
                        var resentry = AssetManager.Instance.GetResEntry(headEntry.Name);
                        var res = AssetManager.Instance.GetRes(resentry);
                        MeshSet meshSet = new MeshSet(res, AssetManager.Instance);

                        var exporter = new MeshToFbxExporter();
                        exporter.OnlyFirstLOD = true;
                        exporter.Export(AssetManager.Instance, skinnedMeshEbx.RootObject, "test_noSkel.obj", "2016", "Meters", true, null, "*.obj", meshSet);

                        SetupHeadScene();
                    }
                }
            }
        }

        public void ExportHead()
        {

        }

        public void SetupHeadScene()
        {
            if (!File.Exists("test_noSkel.obj"))
                return;

            
            

            var import = new Importer();
            var scene = import.Load("test_noSkel.obj", new ImporterConfiguration() { GlobalScale = 100, FlipWindingOrder = true,  CullMode = SharpDX.Direct3D11.CullMode.None, });
            //ViewportViewModel.BunnyModel = scene;

            //ViewportViewModel.GroupModel.RemoveNode(scene.Root);
            //ViewportViewModel.GroupModel.AddNode(scene.Root);

        }

    }
}
