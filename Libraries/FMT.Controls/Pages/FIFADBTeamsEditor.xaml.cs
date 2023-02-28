using CSharpImageLibrary;
using FifaLibrary;
using FrostySdk;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FMT.Controls.Pages
{
    /// <summary>
    /// Interaction logic for FIFADBTeamsEditor.xaml
    /// </summary>
    public partial class FIFADBTeamsEditor : UserControl
    {

        public struct TeamItem
        {
            public int TeamId { get; set; }
            public int LeagueId { get; set; }
            public string Name { get; set; }
            public string DisplayName
            {
                get
                {

                    return Name + " {" + TeamId + "} ";
                }
            }
        }

        public struct LeagueItem
        {
            public int LeagueId { get; set; }
            public string Name { get; set; }
            public string DisplayName
            {
                get
                {

                    return Name + " {" + LeagueId + "} ";
                }
            }
        }

        public struct LeagueTeamLinkItem
        {
            public int LeagueId { get; set; }
            public int TeamId { get; set; }
        }

        public LegacyFileEntry DBAssetEntry { get; set; }
        public MemoryStream DBAssetStream { get; set; }
        public MemoryStream DBMetaAssetStream { get; set; }

        public IList<AssetEntry> LOCAssetEntries { get; set; }

        public ICollection<TeamItem> TeamItems { get; set; }
        public ICollection<LeagueItem> LeagueItems { get; set; }

        public TeamItem SelectedTeamItem { get; set; }

        public DbFile DB { get; set; }

        ICollection<LeagueTeamLinkItem> LeagueTeamLinkItems = new List<LeagueTeamLinkItem>();

        public FIFADBTeamsEditor()
        {
            InitializeComponent();
            AttachToAssetManager();
            DataContext = null;
            DataContext = this;
        }

        ~FIFADBTeamsEditor()
        {
            DetachFromAssetManager();

            //if(DB != null)
            //    DB.Dispose();

            DBAssetEntry = null;
            DBAssetStream = null;
            DBMetaAssetStream = null;
            DB = null;
        }

        public void AttachToAssetManager()
        {
            //AssetManager.AssetManagerInitialised += AssetManagerInitialised;
            //AssetManager.AssetManagerModified += AssetManagerInitialised;
        }

        public void DetachFromAssetManager()
        {
            //AssetManager.AssetManagerInitialised -= AssetManagerInitialised;
            //AssetManager.AssetManagerModified -= AssetManagerInitialised;
        }

        private async Task Load()
        {
            await LoadDbFromLegacy();
        }

        private async void AssetManagerInitialised()
        {
            try
            {
                await Load();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task LoadDbFromLegacy()
        {
            DBAssetEntry = await AssetManager.Instance.GetCustomAssetEntryAsync("legacy", "data/db/fifa_ng_db.db") as LegacyFileEntry;
            DBAssetStream = await AssetManager.Instance.GetCustomAssetAsync("legacy", DBAssetEntry);
            if (DBAssetStream == null)
                return;

            var dbMeta = await AssetManager.Instance.GetCustomAssetEntryAsync("legacy", "data/db/fifa_ng_db-meta.xml") as LegacyFileEntry;
            DBMetaAssetStream = await AssetManager.Instance.GetCustomAssetAsync("legacy", dbMeta);

            if (DBMetaAssetStream == null)
                return;

            //DB = new DbFile(DBAssetStream, DBMetaAssetStream);

            await RefreshTeamList();
            await RefreshLeagueList();
        }

        private async Task RefreshTeamList()
        {
            await Task.Run(() =>
            {
                TeamItems = new List<TeamItem>();

                var teamsTable = DB.GetTable("teams").ConvertToDataTable();
                foreach (DataRow drv in teamsTable.Rows)
                {
                    TeamItems.Add(new TeamItem()
                    {

                        Name = drv["teamname"].ToString(),
                        TeamId = int.Parse(drv["teamid"].ToString()),

                    });
                }

                TeamItems = TeamItems.OrderBy(x => x.Name).ToList();


            });

            await Dispatcher.InvokeAsync(() => { lbTeams.ItemsSource = null; lbTeams.ItemsSource = TeamItems; });
        }

        private async Task RefreshLeagueList()
        {
            await Task.Run(() =>
            {
                LeagueItems = new List<LeagueItem>();

                var leaguesTable = DB.GetTable("leagues").ConvertToDataTable();
                foreach (DataRow drv in leaguesTable.Rows)
                {
                    LeagueItems.Add(new LeagueItem()
                    {

                        Name = drv["leaguename"].ToString(),
                        LeagueId = int.Parse(drv["leagueid"].ToString()),

                    });
                }

                LeagueItems = LeagueItems.OrderBy(x => x.Name).ToList();

            });

            await Dispatcher.InvokeAsync(() => { cbLeagues.ItemsSource = null; cbLeagues.ItemsSource = LeagueItems; });
        }

        private async void lbTeams_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbTeams.SelectedItem == null)
                return;

            SelectedTeamItem = (TeamItem)lbTeams.SelectedItem;

            // Crest
            var imgCrestEntry = await AssetManager.Instance.GetCustomAssetEntryAsync("legacy", "data/ui/imgAssets/crest/light/l" + SelectedTeamItem.TeamId + ".dds") as LegacyFileEntry;
            var stream = await AssetManager.Instance.GetCustomAssetAsync("legacy", imgCrestEntry);

            ImageEngineImage imageEngineImage = new ImageEngineImage(stream.ToArray());
            var iData = imageEngineImage.Save(new ImageFormats.ImageEngineFormatDetails(ImageEngineFormat.BMP), MipHandling.KeepTopOnly, removeAlpha: false);
            imgTeamCrest.Source = LoadImage(iData);

            if (!LeagueTeamLinkItems.Any())
            {
                foreach (DataRow dr in DB.GetTable("leagueteamlinks").ConvertToDataTable().Rows)
                {
                    LeagueTeamLinkItems.Add(new LeagueTeamLinkItem() { TeamId = int.Parse(dr["teamid"].ToString()), LeagueId = int.Parse(dr["leagueid"].ToString()) });
                }
            }

            // League
            cbLeagues.SelectedItem = LeagueItems.FirstOrDefault(x =>
                    x.LeagueId == LeagueTeamLinkItems.FirstOrDefault(z => z.TeamId == SelectedTeamItem.TeamId).LeagueId);





            DataContext = null;
            DataContext = this;
        }

        private static System.Windows.Media.Imaging.BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new System.Windows.Media.Imaging.BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void cbLeagues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLeagues.SelectedItem == null)
                return;

            bool madeChange = false;
            var selectedLeague = (LeagueItem)((ComboBox)sender).SelectedItem;

            var dt_ltl = DB.GetTable("leagueteamlinks").ConvertToDataTable();
            foreach (DataRow item in dt_ltl.Rows)
            {
                if (int.Parse(item["teamid"].ToString()) == SelectedTeamItem.TeamId)
                {
                    if (int.Parse(item["leagueid"].ToString()) != selectedLeague.LeagueId)
                    {
                        item["leagueid"] = selectedLeague.LeagueId;
                        madeChange = true;
                        break;
                    }

                }
            }
            //DB.GetTable("leagueteamlinks").ConvertFromDataTable(dt_ltl);
            LeagueTeamLinkItems.Clear();

            if (madeChange)
            {
                //DB.SaveDb("temp.db");
                //AssetManager.Instance.ModifyLegacyAsset(DBAssetEntry.Name, File.ReadAllBytes("temp.db"), false);
                //File.Delete("temp.db");
            }
        }
    }
}
