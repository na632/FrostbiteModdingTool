using CareerExpansionMod.CME.FIFA;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace v2k4FIFAModding.Career.CME.FIFA
{
    public partial class CareerDB2
    {
        public DataSet ParentDataSet { get; set; }

        public static CareerDB2 Current { get; set; }

        public CareerDB2()
        {
            Current = this;
        }

        public DataTable teams { get; set; }
        public List<FIFAPlayer> players { get; set; }
        public List<FIFAPlayerToTeam> teamplayerlinks { get; set; }
        public List<dynamic> teamstadiumlinks { get; set; }
        public List<dynamic> cz_teams { get; set; }
        public List<dynamic> playerloans { get; set; }
        public List<dynamic> default_mentalities { get; set; }
        public List<dynamic> formations { get; set; }
        public List<dynamic> default_teamsheets { get; set; }
        public List<dynamic> editedplayernames { get; set; }
        public EnumerableRowCollection<DataRow> leagueteamlinks { get; set; }
        public List<dynamic> teamkits { get; set; }
        public List<dynamic> competition { get; set; }
        public List<dynamic> leagues { get; set; }
        public List<dynamic> manager { get; set; }
        public List<dynamic> competitionballs { get; set; }
        public List<dynamic> rivals { get; set; }
        public List<dynamic> fixtures { get; set; }
        public List<dynamic> cz_teamkits { get; set; }
        public List<dynamic> player_grudgelove { get; set; }
        public List<dynamic> smrivals { get; set; }
        public List<dynamic> dcplayernames { get; set; }
        public List<dynamic> cz_assets { get; set; }
        public List<dynamic> createplayer { get; set; }
        public List<dynamic> rowteamnationlinks { get; set; }
        public List<dynamic> playersuspensions { get; set; }
        public List<dynamic> outfitarrangements { get; set; }
        public List<dynamic> playerformdiff { get; set; }
        public List<dynamic> competitionsponsorlinks { get; set; }
        public List<dynamic> stories { get; set; }
        public List<dynamic> modeadboardlinks { get; set; }
        public List<dynamic> teamformationteamstylelinks { get; set; }
        public List<dynamic> teamnationlinks { get; set; }
        public List<dynamic> teamsponsorlinks { get; set; }

        public List<dynamic> referee { get; set; }
        public List<dynamic> competitionkits { get; set; }
        public List<dynamic> stadiumassignments { get; set; }
        public List<dynamic> presentationmodesettings { get; set; }

        public List<dynamic> teamformdiff { get; set; }

        public List<dynamic> leaguerefereelinks { get; set; }

        public List<dynamic> previousteam { get; set; }
        public List<dynamic> euroseeds { get; set; }
        public List<dynamic> version { get; set; }
    }
}
