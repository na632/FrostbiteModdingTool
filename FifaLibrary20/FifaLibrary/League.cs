using System.Drawing;

namespace FifaLibrary
{
	public class League : IdObject
	{
		private string m_leaguename;

		private int m_level;

		private int m_countryid;

		private ELeaguePrestige m_Prestige;

		private Country m_Country;

		private int[] m_boardoutcomes = new int[5];

		private bool m_iswithintransferwindow;

		private int m_leaguetimeslice;

		private string m_ShortName;

		private string m_LongName;

		private TeamList m_PlayingTeams = new TeamList();

		private Trophy m_Trophy;

		public string leaguename
		{
			get
			{
				return m_leaguename;
			}
			set
			{
				m_leaguename = value;
			}
		}

		public int level
		{
			get
			{
				return m_level;
			}
			set
			{
				m_level = value;
			}
		}

		public int countryid
		{
			get
			{
				return m_countryid;
			}
			set
			{
				m_countryid = value;
			}
		}

		public ELeaguePrestige Prestige
		{
			get
			{
				return m_Prestige;
			}
			set
			{
				m_Prestige = value;
			}
		}

		public Country Country
		{
			get
			{
				return m_Country;
			}
			set
			{
				m_Country = value;
				if (m_Country != null)
				{
					m_countryid = m_Country.Id;
				}
				else
				{
					m_countryid = 0;
				}
			}
		}

		public int[] boardoutcomes
		{
			get
			{
				return m_boardoutcomes;
			}
			set
			{
				m_boardoutcomes = value;
			}
		}

		public bool iswithintransferwindow
		{
			get
			{
				return m_iswithintransferwindow;
			}
			set
			{
				m_iswithintransferwindow = value;
			}
		}

		public int leaguetimeslice
		{
			get
			{
				return leaguetimeslice;
			}
			set
			{
				leaguetimeslice = value;
			}
		}

		public string ShortName
		{
			get
			{
				return m_ShortName;
			}
			set
			{
				m_ShortName = value;
			}
		}

		public string LongName
		{
			get
			{
				return m_LongName;
			}
			set
			{
				m_LongName = value;
			}
		}

		public TeamList PlayingTeams
		{
			get
			{
				return m_PlayingTeams;
			}
			set
			{
				m_PlayingTeams = value;
			}
		}

		public Trophy Trophy
		{
			get
			{
				return m_Trophy;
			}
			set
			{
				m_Trophy = value;
			}
		}

		public override string ToString()
		{
			if (m_ShortName != null && m_ShortName != string.Empty)
			{
				return m_ShortName;
			}
			if (m_leaguename != null)
			{
				return m_leaguename;
			}
			return string.Empty;
		}

		public string DatabaseString()
		{
			return m_leaguename;
		}

		public League(int leagueid)
			: base(leagueid)
		{
			base.Id = leagueid;
			m_leaguename = "New League";
			m_level = 1;
			m_Prestige = ELeaguePrestige.Undefined;
			m_countryid = 0;
			LinkCountry(FifaEnvironment.Countries);
			m_iswithintransferwindow = false;
			m_leaguetimeslice = 0;
			m_ShortName = "Short League Name";
			m_LongName = "Long League Name";
			m_boardoutcomes[0] = 0;
			m_boardoutcomes[1] = 0;
			m_boardoutcomes[2] = 0;
			m_boardoutcomes[3] = 0;
			m_boardoutcomes[4] = 0;
		}

		public League(Record r)
			: base(r.IntField[FI.leagues_leagueid])
		{
			Load(r);
			FillFromLanguage();
		}

		public void Load(Record r)
		{
			m_leaguename = r.StringField[FI.leagues_leaguename];
			m_level = r.GetAndCheckIntField(FI.leagues_level);
			m_countryid = r.GetAndCheckIntField(FI.leagues_countryid);
			m_iswithintransferwindow = (r.GetAndCheckIntField(FI.leagues_iswithintransferwindow) != 0);
			m_leaguetimeslice = r.GetAndCheckIntField(FI.leagues_leaguetimeslice);
			m_Prestige = ELeaguePrestige.Undefined;
		}

		public static League GetDefaultLeague()
		{
			return FifaEnvironment.Leagues.SearchLeague(76);
		}

		public static int GetDefaultLeagueId()
		{
			return 76;
		}

		public void LinkTeam(int teamid)
		{
			if (FifaEnvironment.Teams != null)
			{
				Team team = (Team)FifaEnvironment.Teams.SearchId(teamid);
				if (team == null)
				{
					FifaEnvironment.UserMessages.ShowMessage(5022, teamid);
				}
				else
				{
					m_PlayingTeams.Add(team);
				}
			}
		}

		public void LinkTeam(Team team)
		{
			if (team == null)
			{
				FifaEnvironment.UserMessages.ShowMessage(5022, team.Id);
			}
			else
			{
				m_PlayingTeams.Add(team);
			}
		}

		public void LinkCountry(CountryList countryList)
		{
			if (countryList != null)
			{
				m_Country = (Country)countryList.SearchId(m_countryid);
			}
		}

		public void FillFromLanguage()
		{
			if (FifaEnvironment.Language != null)
			{
				m_ShortName = FifaEnvironment.Language.GetLeagueString(base.Id, Language.ELeagueStringType.Abbr15);
				m_LongName = FifaEnvironment.Language.GetLeagueString(base.Id, Language.ELeagueStringType.Full);
				if (m_LongName == null || m_LongName == string.Empty)
				{
					m_LongName = m_ShortName;
				}
			}
			else
			{
				m_ShortName = string.Empty;
				m_LongName = string.Empty;
				if (m_LongName == null || m_LongName == string.Empty)
				{
					m_LongName = m_ShortName;
				}
			}
		}

		public void FillFromBoardOutcomes(Record r)
		{
			m_boardoutcomes[0] = r.GetAndCheckIntField(FI.career_boardoutcomes_outcome1);
			m_boardoutcomes[1] = r.GetAndCheckIntField(FI.career_boardoutcomes_outcome2);
			m_boardoutcomes[2] = r.GetAndCheckIntField(FI.career_boardoutcomes_outcome3);
			m_boardoutcomes[3] = r.GetAndCheckIntField(FI.career_boardoutcomes_outcome4);
			m_boardoutcomes[4] = r.GetAndCheckIntField(FI.career_boardoutcomes_outcome5);
		}

		public void SaveBoardOutcomes(Record r)
		{
			r.IntField[FI.career_boardoutcomes_leagueid] = base.Id;
			r.IntField[FI.career_boardoutcomes_outcome1] = m_boardoutcomes[0];
			r.IntField[FI.career_boardoutcomes_outcome2] = m_boardoutcomes[1];
			r.IntField[FI.career_boardoutcomes_outcome3] = m_boardoutcomes[2];
			r.IntField[FI.career_boardoutcomes_outcome4] = m_boardoutcomes[3];
			r.IntField[FI.career_boardoutcomes_outcome5] = m_boardoutcomes[4];
		}

		public void AddTeam(Team team)
		{
			if (team != null)
			{
				if (team.League != null && team.League != this)
				{
					team.League.RemoveTeam(team);
				}
				m_PlayingTeams.InsertId(team);
				team.League = this;
				team.PrevLeague = this;
				team.currenttableposition = m_PlayingTeams.Count;
				team.previousyeartableposition = m_PlayingTeams.Count;
			}
		}

		public void RemoveTeam(Team team)
		{
			if (team != null)
			{
				if (team.League == this)
				{
					team.League = null;
				}
				m_PlayingTeams.RemoveId(team);
			}
		}

		public void RemoveAllTeams()
		{
			while (m_PlayingTeams.Count > 0)
			{
				Team team = (Team)m_PlayingTeams[0];
				RemoveTeam(team);
			}
		}

		public static string ReplayLogoTextureFileName(int id)
		{
			return "data/sceneassets/leaguelogo/leaguelogo_" + id.ToString() + "_textures.rx3";
		}

		public string ReplayLogoTextureFileName()
		{
			return ReplayLogoTextureFileName(base.Id);
		}

		public string ReplayLogoTexturesTemplateFileName()
		{
			return "data/sceneassets/leaguelogo/leaguelogo_#_textures.rx3";
		}

		public static Bitmap[] GetReplayLogoTextures(int leagueId)
		{
			return FifaEnvironment.GetBmpsFromRx3(ReplayLogoTextureFileName(leagueId));
		}

		public Bitmap[] GetReplayLogoTextures()
		{
			return FifaEnvironment.GetBmpsFromRx3(ReplayLogoTextureFileName());
		}

		public bool SetReplayLogoTextures(Bitmap[] bitmaps)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(ReplayLogoTexturesTemplateFileName(), base.Id, bitmaps, ECompressionMode.Chunkzip2);
		}

		public bool DeleteReplayLogoTextures()
		{
			return FifaEnvironment.DeleteFromZdata(ReplayLogoTextureFileName());
		}

		public static string ReplayLogoModelFileName(int id)
		{
			return "data/sceneassets/leaguelogo/leaguelogo_" + id.ToString() + ".rx3";
		}

		public string ReplayLogoModelTemplateFileName()
		{
			return "data/sceneassets/leaguelogo/leaguelogo_#.rx3";
		}

		public string ReplayLogoModelFileName()
		{
			return ReplayLogoModelFileName(base.Id);
		}

		public Rx3File GetReplayLogoModel()
		{
			return FifaEnvironment.GetRx3FromZdata(ReplayLogoModelFileName());
		}

		public static Rx3File GetReplayLogoModel(int id)
		{
			return FifaEnvironment.GetRx3FromZdata(ReplayLogoModelFileName(id));
		}

		public bool SetReplayLogoModel(string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, ReplayLogoModelFileName(), delete: false, ECompressionMode.Chunkzip2);
		}

		public bool DeleteReplayLogoModel()
		{
			return FifaEnvironment.DeleteFromZdata(ReplayLogoModelFileName());
		}

		public bool IsReplayLogoPatched()
		{
			bool flag = FifaEnvironment.IsPatched(ReplayLogoTextureFileName());
			return FifaEnvironment.IsPatched(ReplayLogoTextureFileName()) | flag;
		}

		public bool CreateReplayLogoPatch()
		{
			bool flag = FifaEnvironment.ImportFileIntoZdataAs(FifaEnvironment.LaunchDir + "\\Templates\\" + ReplayLogoTexturesTemplateFileName(), ReplayLogoTextureFileName(), delete: false, ECompressionMode.None);
			return FifaEnvironment.ImportFileIntoZdataAs(FifaEnvironment.LaunchDir + "\\Templates\\" + ReplayLogoModelTemplateFileName(), ReplayLogoModelFileName(), delete: false, ECompressionMode.None) && flag;
		}

		public bool RemoveReplayLogoPatch()
		{
			bool flag = FifaEnvironment.DeleteFromZdata(ReplayLogoTextureFileName());
			return FifaEnvironment.DeleteFromZdata(ReplayLogoModelFileName()) && flag;
		}

		public static string AnimLogoDdsFileName(int id, int year)
		{
			if (year == 14)
			{
				return "data/ui/imgassets/league/l" + id.ToString() + ".dds";
			}
			return "data/ui/imgassets/league/light/l" + id.ToString() + ".dds";
		}

		public static string AnimLogoDdsFileName(int id)
		{
			return AnimLogoDdsFileName(id, FifaEnvironment.Year);
		}

		public string AnimLogoTemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/imgassets/league/l#.dds";
			}
			return "data/ui/imgassets/league/light/l#.dds";
		}

		public string AnimLogoDdsFileName()
		{
			return AnimLogoDdsFileName(base.Id);
		}

		public Bitmap GetAnimLogo()
		{
			return FifaEnvironment.GetDdsArtasset(AnimLogoDdsFileName());
		}

		public bool SetAnimLogo(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return FifaEnvironment.SetDdsArtasset(AnimLogoTemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteAnimLogo()
		{
			return FifaEnvironment.DeleteFromZdata(AnimLogoDdsFileName());
		}

		public static string AnimLogoDarkDdsFileName(int id)
		{
			return "data/ui/imgassets/league/dark/l" + id.ToString() + ".dds";
		}

		public string AnimLogoDarkTemplateFileName()
		{
			return "data/ui/imgassets/league/dark/l#.dds";
		}

		public string AnimLogoDarkDdsFileName()
		{
			return AnimLogoDarkDdsFileName(base.Id);
		}

		public Bitmap GetAnimLogoDark()
		{
			return FifaEnvironment.GetDdsArtasset(AnimLogoDarkDdsFileName());
		}

		public bool SetAnimLogoDark(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return FifaEnvironment.SetDdsArtasset(AnimLogoDarkTemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteAnimLogoDark()
		{
			return FifaEnvironment.DeleteFromZdata(AnimLogoDarkDdsFileName());
		}

		public static string Logo512x128DdsFileName(int id)
		{
			if (FifaEnvironment.Year == 14)
			{
				return null;
			}
			return "data/ui/imgassets/league512x128/light/l" + id.ToString() + ".dds";
		}

		public string Logo512x128TemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return null;
			}
			return "data/ui/imgassets/league512x128/light/l#.dds";
		}

		public string Logo512x128DdsFileName()
		{
			return Logo512x128DdsFileName(base.Id);
		}

		public Bitmap GetLogo512x128()
		{
			return FifaEnvironment.GetDdsArtasset(Logo512x128DdsFileName());
		}

		public bool SetLogo512x128(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return FifaEnvironment.SetDdsArtasset(Logo512x128TemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteLogo512x128()
		{
			return FifaEnvironment.DeleteFromZdata(Logo512x128DdsFileName());
		}

		public static string Logo512x128DarkDdsFileName(int id)
		{
			if (FifaEnvironment.Year == 14)
			{
				return null;
			}
			return "data/ui/imgassets/league512x128/dark/l" + id.ToString() + ".dds";
		}

		public string Logo512x128DarkTemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return null;
			}
			return "data/ui/imgassets/league512x128/dark/l#.dds";
		}

		public string Logo512x128DarkDdsFileName()
		{
			return Logo512x128DarkDdsFileName(base.Id);
		}

		public Bitmap GetLogo512x128Dark()
		{
			return FifaEnvironment.GetDdsArtasset(Logo512x128DarkDdsFileName());
		}

		public bool SetLogo512x128Dark(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return FifaEnvironment.SetDdsArtasset(Logo512x128DarkTemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteLogo512x128Dark()
		{
			return FifaEnvironment.DeleteFromZdata(Logo512x128DarkDdsFileName());
		}

		public static string TinyLogoDdsFileName(int id, int year)
		{
			if (year == 14)
			{
				return "data/ui/imgassets/leaguelogos_tiny/l" + id.ToString() + ".dds";
			}
			return "data/ui/imgassets/leaguelogos_tiny/light/l" + id.ToString() + ".dds";
		}

		public static string TinyLogoDdsFileName(int id)
		{
			return TinyLogoDdsFileName(id, FifaEnvironment.Year);
		}

		public string TinyLogoTemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/imgassets/leaguelogos_tiny/l#.dds";
			}
			return "data/ui/imgassets/leaguelogos_tiny/light/l#.dds";
		}

		public string TinyLogoDdsFileName()
		{
			return TinyLogoDdsFileName(base.Id);
		}

		public Bitmap GetTinyLogo()
		{
			return FifaEnvironment.GetDdsArtasset(TinyLogoDdsFileName());
		}

		public bool SetTinyLogo(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return FifaEnvironment.SetDdsArtasset(TinyLogoTemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteTinyLogo()
		{
			return FifaEnvironment.DeleteFromZdata(TinyLogoDdsFileName());
		}

		public static string TinyLogoDarkDdsFileName(int id)
		{
			return "data/ui/imgassets/leaguelogos_tiny/dark/l" + id.ToString() + ".dds";
		}

		public string TinyLogoDarkTemplateFileName()
		{
			return "data/ui/imgassets/leaguelogos_tiny/dark/l#.dds";
		}

		public string TinyLogoDarkDdsFileName()
		{
			return TinyLogoDarkDdsFileName(base.Id);
		}

		public Bitmap GetTinyLogoDark()
		{
			return FifaEnvironment.GetDdsArtasset(TinyLogoDarkDdsFileName());
		}

		public bool SetTinyLogoDark(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return FifaEnvironment.SetDdsArtasset(TinyLogoDarkTemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteTinyLogoDark()
		{
			return FifaEnvironment.DeleteFromZdata(TinyLogoDarkDdsFileName());
		}

		public static string SmallLogoDdsFileName(int id)
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/imgassets/leaguelogos_sm/l" + id.ToString() + ".dds";
			}
			return null;
		}

		public string SmallLogoTemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/imgassets/leaguelogos_sm/l#.dds";
			}
			return null;
		}

		public string SmallLogoDdsFileName()
		{
			return SmallLogoDdsFileName(base.Id);
		}

		public Bitmap GetSmallLogo()
		{
			return FifaEnvironment.GetDdsArtasset(SmallLogoDdsFileName());
		}

		public bool SetSmallLogo(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return FifaEnvironment.SetDdsArtasset(SmallLogoTemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteSmallLogo()
		{
			return FifaEnvironment.DeleteFromZdata(SmallLogoDdsFileName());
		}

		public static string SmallLogoDarkDdsFileName(int id)
		{
			return "data/ui/imgassets/leaguelogos_sm/dark/l" + id.ToString() + ".dds";
		}

		public string SmallLogoDarkTemplateFileName()
		{
			return "data/ui/imgassets/leaguelogos_sm/dark/l#.dds";
		}

		public string SmallLogoDarkDdsFileName()
		{
			return SmallLogoDarkDdsFileName(base.Id);
		}

		public Bitmap GetSmallLogoDark()
		{
			return FifaEnvironment.GetDdsArtasset(SmallLogoDarkDdsFileName());
		}

		public bool SetSmallLogoDark(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			return FifaEnvironment.SetDdsArtasset(SmallLogoDarkTemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteSmallLogoDark()
		{
			return FifaEnvironment.DeleteFromZdata(SmallLogoDarkDdsFileName());
		}

		public void SaveLeague(Record r)
		{
			r.IntField[FI.leagues_leagueid] = base.Id;
			r.StringField[FI.leagues_leaguename] = m_leaguename;
			r.IntField[FI.leagues_level] = m_level;
			r.IntField[FI.leagues_countryid] = m_countryid;
			r.IntField[FI.leagues_leaguetimeslice] = m_leaguetimeslice;
			r.IntField[FI.leagues_iswithintransferwindow] = (m_iswithintransferwindow ? 1 : 0);
		}

		public void SaveTeamLink(Record r, Team team, int artificialkey)
		{
			r.IntField[FI.leagueteamlinks_artificialkey] = artificialkey;
			r.IntField[FI.leagueteamlinks_leagueid] = base.Id;
			r.IntField[FI.leagueteamlinks_teamid] = team.Id;
			if (team.PrevLeague != null)
			{
				r.IntField[FI.leagueteamlinks_prevleagueid] = team.PrevLeague.Id;
				r.IntField[FI.leagueteamlinks_champion] = (team.IsChampion ? 1 : 0);
			}
			else
			{
				r.IntField[FI.leagueteamlinks_prevleagueid] = base.Id;
				r.IntField[FI.leagueteamlinks_champion] = 0;
			}
			r.IntField[FI.leagueteamlinks_previousyeartableposition] = team.previousyeartableposition;
			r.IntField[FI.leagueteamlinks_currenttableposition] = team.currenttableposition;
			r.IntField[FI.leagueteamlinks_teamshortform] = team.teamshortform;
			r.IntField[FI.leagueteamlinks_teamlongform] = team.teamlongform;
			r.IntField[FI.leagueteamlinks_teamform] = team.teamform;
			r.IntField[FI.leagueteamlinks_hasachievedobjective] = (team.hasachievedobjective ? 1 : 0);
			r.IntField[FI.leagueteamlinks_yettowin] = (team.yettowin ? 1 : 0);
			r.IntField[FI.leagueteamlinks_unbeatenallcomps] = (team.unbeatenallcomps ? 1 : 0);
			r.IntField[FI.leagueteamlinks_unbeatenaway] = (team.unbeatenaway ? 1 : 0);
			r.IntField[FI.leagueteamlinks_unbeatenhome] = (team.unbeatenhome ? 1 : 0);
			r.IntField[FI.leagueteamlinks_unbeatenleague] = (team.unbeatenleague ? 1 : 0);
			r.IntField[FI.leagueteamlinks_highestpossible] = team.highestpossible;
			r.IntField[FI.leagueteamlinks_highestprobable] = team.highestprobable;
			r.IntField[FI.leagueteamlinks_nummatchesplayed] = team.nummatchesplayed;
			r.IntField[FI.leagueteamlinks_gapresult] = team.gapresult;
			r.IntField[FI.leagueteamlinks_grouping] = team.grouping;
			r.IntField[FI.leagueteamlinks_objective] = team.objective;
			r.IntField[FI.leagueteamlinks_actualvsexpectations] = team.actualvsexpectations;
			r.IntField[FI.leagueteamlinks_lastgameresult] = team.lastgameresult;
			r.IntField[FI.leagueteamlinks_homega] = team.homega;
			r.IntField[FI.leagueteamlinks_homegf] = team.homegf;
			r.IntField[FI.leagueteamlinks_points] = team.points;
			r.IntField[FI.leagueteamlinks_awayga] = team.awayga;
			r.IntField[FI.leagueteamlinks_homewins] = team.secondarytable;
			r.IntField[FI.leagueteamlinks_homewins] = team.homewins;
			r.IntField[FI.leagueteamlinks_awaywins] = team.awaywins;
			r.IntField[FI.leagueteamlinks_homelosses] = team.homelosses;
			r.IntField[FI.leagueteamlinks_awaylosses] = team.awaylosses;
			r.IntField[FI.leagueteamlinks_awaydraws] = team.awaydraws;
			r.IntField[FI.leagueteamlinks_homedraws] = team.homedraws;
		}

		public void SynchronizeLeague()
		{
		}

		public bool ContainsNationalTeams()
		{
			foreach (Team playingTeam in m_PlayingTeams)
			{
				if (playingTeam.NationalTeam)
				{
					return true;
				}
			}
			return false;
		}
	}
}
