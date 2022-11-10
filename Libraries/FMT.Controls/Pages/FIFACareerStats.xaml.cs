using FifaLibrary;
using FrostySdk.Managers;
using FrostySdk;
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

namespace FMT.Controls.Pages
{
    /// <summary>
    /// Interaction logic for FIFACareerStats.xaml
    /// </summary>
    public partial class FIFACareerStats : UserControl
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

        public FIFACareerStats()
        {
            InitializeComponent();
        }
    }
}
