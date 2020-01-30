using System.Drawing;
using System.Drawing.Imaging;

namespace FifaLibrary
{
	public class Team : IdObject
	{
		private static int[] c_TopLeaguesId = new int[6]
		{
			13,
			16,
			19,
			31,
			53,
			341
		};

		private bool m_ImpatientBoard;

		private bool m_LoyalBoard;

		private bool m_SquadRotation;

		private bool m_ConsistentLineup;

		private bool m_SwitchWingers;

		private bool m_CenterBacksSplit;

		private bool m_DefendLead;

		private bool m_KeepUpPressure;

		private bool m_MoreAttackingAtHome;

		private bool m_ShortOutBack;

		private int m_balltype = 1;

		private Ball m_Ball;

		private int m_adboardid = 1;

		private bool m_HasSpecificAdboard;

		private int m_stadiumid = -1;

		private Stadium m_Stadium;

		private int m_rivalteam;

		private Team m_RivalTeam;

		private int m_formationid;

		private Formation m_Formation;

		private int m_captainid;

		private int m_penaltytakerid;

		private int m_leftfreekicktakerid;

		private int m_rightfreekicktakerid;

		private int m_freekicktakerid;

		private int m_longkicktakerid;

		private int m_leftcornerkicktakerid;

		private int m_rightcornerkicktakerid;

		public Player PlayerCaptain;

		public Player PlayerPenalty;

		public Player PlayerFreeKick;

		public Player PlayerLeftFreeKick;

		public Player PlayerRightFreeKick;

		public Player PlayerLongKick;

		public Player PlayerLeftCorner;

		public Player PlayerRightCorner;

		private string m_TeamNameFull;

		private string m_TeamNameAbbr15;

		private string m_TeamNameAbbr10;

		private string m_TeamNameAbbr3;

		private string m_TeamNameAbbr7;

		private string m_teamname;

		private int m_jerseytype;

		private int m_fancrowdhairskintexturecode;

		private int m_stafftracksuitcolorcode;

		private int m_physioid_primary;

		private int m_physioid_secondary;

		private int m_teamcolor1r;

		private int m_teamcolor1g;

		private int m_teamcolor1b;

		private Color m_TeamColor1;

		private Color m_TeamColor2;

		private Color m_TeamColor3;

		private int m_teamcolor2r;

		private int m_teamcolor2g;

		private int m_teamcolor2b;

		private int m_teamcolor3r;

		private int m_teamcolor3g;

		private int m_teamcolor3b;

		private int m_form;

		private int m_managerid;

		private int m_latitude;

		private int m_longitude;

		private int m_bodytypeid;

		private int m_suitvariationid;

		private int m_suittypeid;

		private int m_personalityid;

		private int m_busdribbling;

		private int m_trait1;

		private int m_utcoffset;

		private int m_ethnicity;

		private int m_powid;

		public bool m_genericbanner;

		private int m_assetid;

		private int m_transferbudget;

		private int m_internationalprestige;

		private int m_domesticprestige;

		private int m_numtransfersin;

		private string m_stadiumcustomname;

		private string m_ManagerFirstName;

		private string m_ManagerSurname;

		private int m_busbuildupspeed;

		private int m_buspassing;

		private int m_buspositioning;

		private int m_ccpassing;

		private int m_cccrossing;

		private int m_ccshooting;

		private int m_ccpositioning;

		private int m_defmentality;

		private int m_defaggression;

		private int m_defteamwidth;

		private int m_defdefenderline;

		private int m_genericint2;

		private int m_genericint1;

		private int m_midfieldrating;

		private int m_defenserating;

		private int m_attackrating;

		private int m_overallrating;

		private int m_matchdayoverallrating;

		private int m_matchdaydefenserating;

		private int m_matchdaymidfieldrating;

		private int m_matchdayattackrating;

		private Roster m_Roster = new Roster(32);

		public int m_countryid_IfNationalTeam;

		public int m_countryid_IfRowTeam;

		public int m_countryid_IfLeagueTeam;

		private Country m_Country;

		private int m_leagueid;

		private League m_League;

		private int m_prevleagueid;

		private League m_PrevLeague;

		private bool m_champion;

		private int m_previousyeartableposition;

		private int m_currenttableposition;

		private int m_teamshortform;

		private int m_teamlongform;

		private int m_teamform;

		private bool m_hasachievedobjective;

		private bool m_yettowin;

		private bool m_unbeatenallcomps;

		private bool m_unbeatenaway;

		private bool m_unbeatenhome;

		private bool m_unbeatenleague;

		private int m_highestpossible;

		private int m_highestprobable;

		private int m_nummatchesplayed;

		private int m_gapresult;

		private int m_grouping;

		private int m_objective;

		private int m_actualvsexpectations;

		private int m_lastgameresult;

		private int m_homega;

		private int m_homegf;

		private int m_points;

		private int m_awayga;

		private int m_secondarytable;

		private int m_homewins;

		private int m_awaywins;

		private int m_homelosses;

		private int m_awaylosses;

		private int m_awaydraws;

		private int m_homedraws;

		public KitList m_KitList = new KitList();

		private int[] m_teamkitidList = new int[10]
		{
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1,
			-1
		};

		private int m_crestweight = 25;

		private int m_genericweight = 25;

		private int m_teamweight = 50;

		private int m_maxvariationsneg;

		private int m_maxvariationspos;

		private int m_maxvariationsstd;

		public bool ImpatientBoard
		{
			get
			{
				return m_ImpatientBoard;
			}
			set
			{
				m_ImpatientBoard = value;
			}
		}

		public bool LoyalBoard
		{
			get
			{
				return m_LoyalBoard;
			}
			set
			{
				m_LoyalBoard = value;
			}
		}

		public bool SquadRotation
		{
			get
			{
				return m_SquadRotation;
			}
			set
			{
				m_SquadRotation = value;
			}
		}

		public bool ConsistentLineup
		{
			get
			{
				return m_ConsistentLineup;
			}
			set
			{
				m_ConsistentLineup = value;
			}
		}

		public bool SwitchWingers
		{
			get
			{
				return m_SwitchWingers;
			}
			set
			{
				m_SwitchWingers = value;
			}
		}

		public bool CenterBacksSplit
		{
			get
			{
				return m_CenterBacksSplit;
			}
			set
			{
				m_CenterBacksSplit = value;
			}
		}

		public bool DefendLead
		{
			get
			{
				return m_DefendLead;
			}
			set
			{
				m_DefendLead = value;
			}
		}

		public bool KeepUpPressure
		{
			get
			{
				return m_KeepUpPressure;
			}
			set
			{
				m_KeepUpPressure = value;
			}
		}

		public bool MoreAttackingAtHome
		{
			get
			{
				return m_MoreAttackingAtHome;
			}
			set
			{
				m_MoreAttackingAtHome = value;
			}
		}

		public bool ShortOutBack
		{
			get
			{
				return m_ShortOutBack;
			}
			set
			{
				m_ShortOutBack = value;
			}
		}

		public int balltype
		{
			get
			{
				return m_balltype;
			}
			set
			{
				m_balltype = value;
			}
		}

		public int adboardid
		{
			get
			{
				return m_adboardid;
			}
			set
			{
				m_adboardid = value;
			}
		}

		public bool HasSpecifiAdboard
		{
			get
			{
				return m_HasSpecificAdboard;
			}
			set
			{
				m_HasSpecificAdboard = value;
			}
		}

		public int stadiumid
		{
			get
			{
				return m_stadiumid;
			}
			set
			{
				m_stadiumid = value;
			}
		}

		public Stadium Stadium
		{
			get
			{
				return m_Stadium;
			}
			set
			{
				m_Stadium = value;
				if (m_Stadium != null)
				{
					m_stadiumid = m_Stadium.Id;
				}
			}
		}

		public int rivalteam
		{
			get
			{
				return m_rivalteam;
			}
			set
			{
				m_rivalteam = value;
			}
		}

		public Team RivalTeam
		{
			get
			{
				return m_RivalTeam;
			}
			set
			{
				m_RivalTeam = value;
				if (m_RivalTeam != null)
				{
					m_rivalteam = m_RivalTeam.Id;
				}
			}
		}

		public int formationid
		{
			get
			{
				return m_formationid;
			}
			set
			{
				m_formationid = value;
			}
		}

		public Formation Formation
		{
			get
			{
				return m_Formation;
			}
			set
			{
				m_Formation = value;
				if (m_Formation != null)
				{
					m_formationid = m_Formation.Id;
				}
			}
		}

		public int captainid
		{
			get
			{
				return m_captainid;
			}
			set
			{
				m_captainid = value;
			}
		}

		public int penaltytakerid
		{
			get
			{
				return m_penaltytakerid;
			}
			set
			{
				m_penaltytakerid = value;
			}
		}

		public int leftfreekicktakerid
		{
			get
			{
				return m_leftfreekicktakerid;
			}
			set
			{
				m_leftfreekicktakerid = value;
			}
		}

		public int rightfreekicktakerid
		{
			get
			{
				return m_rightfreekicktakerid;
			}
			set
			{
				m_rightfreekicktakerid = value;
			}
		}

		public int freekicktakerid
		{
			get
			{
				return m_freekicktakerid;
			}
			set
			{
				m_freekicktakerid = value;
			}
		}

		public int longkicktakerid
		{
			get
			{
				return m_longkicktakerid;
			}
			set
			{
				m_longkicktakerid = value;
			}
		}

		public int leftcornerkicktakerid
		{
			get
			{
				return m_leftcornerkicktakerid;
			}
			set
			{
				m_leftcornerkicktakerid = value;
			}
		}

		public int rightcornerkicktakerid
		{
			get
			{
				return m_rightcornerkicktakerid;
			}
			set
			{
				m_rightcornerkicktakerid = value;
			}
		}

		public string TeamNameFull
		{
			get
			{
				return m_TeamNameFull;
			}
			set
			{
				m_TeamNameFull = value;
			}
		}

		public string TeamNameAbbr15
		{
			get
			{
				return m_TeamNameAbbr15;
			}
			set
			{
				m_TeamNameAbbr15 = value;
			}
		}

		public string TeamNameAbbr10
		{
			get
			{
				return m_TeamNameAbbr10;
			}
			set
			{
				m_TeamNameAbbr10 = value;
			}
		}

		public string TeamNameAbbr3
		{
			get
			{
				return m_TeamNameAbbr3;
			}
			set
			{
				m_TeamNameAbbr3 = value;
			}
		}

		public string TeamNameAbbr7
		{
			get
			{
				return m_TeamNameAbbr7;
			}
			set
			{
				m_TeamNameAbbr7 = value;
			}
		}

		public string DatabaseName
		{
			get
			{
				return m_teamname;
			}
			set
			{
				m_teamname = value;
			}
		}

		public int jerseytype
		{
			get
			{
				return m_jerseytype;
			}
			set
			{
				m_jerseytype = value;
			}
		}

		public int stafftracksuitcolorcode
		{
			get
			{
				return m_stafftracksuitcolorcode;
			}
			set
			{
				m_stafftracksuitcolorcode = value;
			}
		}

		public Color TeamColor1
		{
			get
			{
				return m_TeamColor1;
			}
			set
			{
				m_TeamColor1 = value;
			}
		}

		public Color TeamColor2
		{
			get
			{
				return m_TeamColor2;
			}
			set
			{
				m_TeamColor2 = value;
			}
		}

		public Color TeamColor3
		{
			get
			{
				return m_TeamColor3;
			}
			set
			{
				m_TeamColor3 = value;
			}
		}

		public int managerid
		{
			get
			{
				return m_managerid;
			}
			set
			{
				m_managerid = value;
			}
		}

		public int latitude
		{
			get
			{
				return m_latitude;
			}
			set
			{
				m_latitude = value;
			}
		}

		public int longitude
		{
			get
			{
				return m_longitude;
			}
			set
			{
				m_longitude = value;
			}
		}

		public int bodytypeid
		{
			get
			{
				return m_bodytypeid;
			}
			set
			{
				m_bodytypeid = value;
			}
		}

		public int suitvariationid
		{
			get
			{
				return m_suitvariationid;
			}
			set
			{
				m_suitvariationid = value;
			}
		}

		public int suittypeid
		{
			get
			{
				return m_suittypeid;
			}
			set
			{
				m_suittypeid = value;
			}
		}

		public int personalityid
		{
			get
			{
				return m_personalityid;
			}
			set
			{
				m_personalityid = value;
			}
		}

		public int busdribbling
		{
			get
			{
				return m_busdribbling;
			}
			set
			{
				m_busdribbling = value;
			}
		}

		public int trait1
		{
			get
			{
				return m_trait1;
			}
			set
			{
				m_trait1 = value;
			}
		}

		public int utcoffset
		{
			get
			{
				return m_utcoffset;
			}
			set
			{
				m_utcoffset = value;
			}
		}

		public int ethnicity
		{
			get
			{
				return m_ethnicity;
			}
			set
			{
				m_ethnicity = value;
			}
		}

		public int assetid
		{
			get
			{
				return m_assetid;
			}
			set
			{
				m_assetid = value;
			}
		}

		public int transferbudget
		{
			get
			{
				return m_transferbudget;
			}
			set
			{
				m_transferbudget = value;
			}
		}

		public int internationalprestige
		{
			get
			{
				return m_internationalprestige;
			}
			set
			{
				m_internationalprestige = value;
			}
		}

		public int domesticprestige
		{
			get
			{
				return m_domesticprestige;
			}
			set
			{
				m_domesticprestige = value;
			}
		}

		public string stadiumcustomname
		{
			get
			{
				return m_stadiumcustomname;
			}
			set
			{
				m_stadiumcustomname = value;
			}
		}

		public string ManagerFirstName
		{
			get
			{
				return m_ManagerFirstName;
			}
			set
			{
				m_ManagerFirstName = value;
			}
		}

		public string ManagerSurname
		{
			get
			{
				return m_ManagerSurname;
			}
			set
			{
				m_ManagerSurname = value;
			}
		}

		public int busbuildupspeed
		{
			get
			{
				return m_busbuildupspeed;
			}
			set
			{
				m_busbuildupspeed = value;
			}
		}

		public int buspassing
		{
			get
			{
				return m_buspassing;
			}
			set
			{
				m_buspassing = value;
			}
		}

		public int buspositioning
		{
			get
			{
				return m_buspositioning;
			}
			set
			{
				m_buspositioning = value;
			}
		}

		public int ccpassing
		{
			get
			{
				return m_ccpassing;
			}
			set
			{
				m_ccpassing = value;
			}
		}

		public int cccrossing
		{
			get
			{
				return m_cccrossing;
			}
			set
			{
				m_cccrossing = value;
			}
		}

		public int ccshooting
		{
			get
			{
				return m_ccshooting;
			}
			set
			{
				m_ccshooting = value;
			}
		}

		public int ccpositioning
		{
			get
			{
				return m_ccpositioning;
			}
			set
			{
				m_ccpositioning = value;
			}
		}

		public int defmentality
		{
			get
			{
				return m_defmentality;
			}
			set
			{
				m_defmentality = value;
			}
		}

		public int defaggression
		{
			get
			{
				return m_defaggression;
			}
			set
			{
				m_defaggression = value;
			}
		}

		public int defteamwidth
		{
			get
			{
				return m_defteamwidth;
			}
			set
			{
				m_defteamwidth = value;
			}
		}

		public int defdefenderline
		{
			get
			{
				return m_defdefenderline;
			}
			set
			{
				m_defdefenderline = value;
			}
		}

		public int midfieldrating
		{
			get
			{
				return m_midfieldrating;
			}
			set
			{
				m_midfieldrating = value;
			}
		}

		public int defenserating
		{
			get
			{
				return m_defenserating;
			}
			set
			{
				m_defenserating = value;
			}
		}

		public int attackrating
		{
			get
			{
				return m_attackrating;
			}
			set
			{
				m_attackrating = value;
			}
		}

		public int overallrating
		{
			get
			{
				return m_overallrating;
			}
			set
			{
				m_overallrating = value;
			}
		}

		public int matchdayoverallrating
		{
			get
			{
				return m_matchdayoverallrating;
			}
			set
			{
				m_matchdayoverallrating = value;
			}
		}

		public int matchdaydefenserating
		{
			get
			{
				return m_matchdaydefenserating;
			}
			set
			{
				m_matchdaydefenserating = value;
			}
		}

		public int matchdaymidfieldrating
		{
			get
			{
				return m_matchdaymidfieldrating;
			}
			set
			{
				m_matchdaymidfieldrating = value;
			}
		}

		public int matchdayattackrating
		{
			get
			{
				return m_matchdayattackrating;
			}
			set
			{
				m_matchdayattackrating = value;
			}
		}

		public Roster Roster => m_Roster;

		public Country Country
		{
			get
			{
				return m_Country;
			}
			set
			{
				if (value == m_Country)
				{
					return;
				}
				if (IsNationalTeam())
				{
					if (m_Country != null)
					{
						m_Country.NationalTeam = null;
					}
					value.SetNationalTeam(this, base.Id);
					SetCountryAsNationalTeam(value);
				}
				else if (value != null)
				{
					if (m_League != null)
					{
						if (value.Id == m_League.Country.Id)
						{
							SetCountryAsLeagueTeam(value);
						}
						else
						{
							SetCountryAsRowTeam(value);
						}
					}
					else
					{
						SetCountryAsRowTeam(value);
					}
				}
				else if (m_League != null)
				{
					SetCountryAsLeagueTeam(m_League.Country);
				}
				else
				{
					ClearCountry();
				}
			}
		}

		public bool NationalTeam
		{
			get
			{
				return IsNationalTeam();
			}
			set
			{
				if (value)
				{
					SetAsNationalTeam(m_Country);
				}
				else
				{
					UnsetAsNationalTeam();
				}
			}
		}

		public League League
		{
			get
			{
				return m_League;
			}
			set
			{
				if (value == null)
				{
					if (m_Country != null && IsLeagueTeam())
					{
						SetCountryAsRowTeam(m_Country);
					}
					m_League = League.GetDefaultLeague();
					m_leagueid = League.GetDefaultLeagueId();
					return;
				}
				m_League = value;
				m_leagueid = m_League.Id;
				if (m_League.Country == null || IsNationalTeam())
				{
					return;
				}
				if (IsRowTeam())
				{
					if (m_Country == m_League.Country)
					{
						SetCountryAsLeagueTeam(m_Country);
					}
				}
				else if (m_Country != null && m_Country != m_League.Country)
				{
					SetCountryAsRowTeam(m_Country);
				}
			}
		}

		public League PrevLeague
		{
			get
			{
				return m_PrevLeague;
			}
			set
			{
				if (value == null)
				{
					m_PrevLeague = null;
					m_prevleagueid = 0;
				}
				else
				{
					m_PrevLeague = value;
					m_prevleagueid = m_PrevLeague.Id;
				}
			}
		}

		public bool IsChampion
		{
			get
			{
				return m_champion;
			}
			set
			{
				m_champion = value;
			}
		}

		public int previousyeartableposition
		{
			get
			{
				return m_previousyeartableposition;
			}
			set
			{
				m_previousyeartableposition = value;
			}
		}

		public int currenttableposition
		{
			get
			{
				return m_currenttableposition;
			}
			set
			{
				m_currenttableposition = value;
			}
		}

		public int teamshortform
		{
			get
			{
				return m_teamshortform;
			}
			set
			{
				m_teamshortform = value;
			}
		}

		public int teamlongform
		{
			get
			{
				return m_teamlongform;
			}
			set
			{
				m_teamlongform = value;
			}
		}

		public int teamform
		{
			get
			{
				return m_teamform;
			}
			set
			{
				m_teamform = value;
			}
		}

		public bool hasachievedobjective
		{
			get
			{
				return m_hasachievedobjective;
			}
			set
			{
				m_hasachievedobjective = value;
			}
		}

		public bool yettowin
		{
			get
			{
				return m_yettowin;
			}
			set
			{
				m_yettowin = value;
			}
		}

		public bool unbeatenallcomps
		{
			get
			{
				return m_unbeatenallcomps;
			}
			set
			{
				m_unbeatenallcomps = value;
			}
		}

		public bool unbeatenaway
		{
			get
			{
				return m_unbeatenaway;
			}
			set
			{
				m_unbeatenaway = value;
			}
		}

		public bool unbeatenhome
		{
			get
			{
				return m_unbeatenhome;
			}
			set
			{
				m_unbeatenhome = value;
			}
		}

		public bool unbeatenleague
		{
			get
			{
				return m_unbeatenleague;
			}
			set
			{
				m_unbeatenleague = value;
			}
		}

		public int highestpossible
		{
			get
			{
				return m_highestpossible;
			}
			set
			{
				m_highestpossible = value;
			}
		}

		public int highestprobable
		{
			get
			{
				return m_highestprobable;
			}
			set
			{
				m_highestprobable = value;
			}
		}

		public int nummatchesplayed
		{
			get
			{
				return m_nummatchesplayed;
			}
			set
			{
				m_nummatchesplayed = value;
			}
		}

		public int gapresult
		{
			get
			{
				return m_gapresult;
			}
			set
			{
				m_gapresult = value;
			}
		}

		public int grouping
		{
			get
			{
				return m_grouping;
			}
			set
			{
				m_grouping = value;
			}
		}

		public int objective
		{
			get
			{
				return m_objective;
			}
			set
			{
				m_objective = value;
			}
		}

		public int actualvsexpectations
		{
			get
			{
				return m_actualvsexpectations;
			}
			set
			{
				m_actualvsexpectations = value;
			}
		}

		public int lastgameresult
		{
			get
			{
				return m_lastgameresult;
			}
			set
			{
				m_lastgameresult = value;
			}
		}

		public int homega
		{
			get
			{
				return m_homega;
			}
			set
			{
				m_homega = value;
			}
		}

		public int homegf
		{
			get
			{
				return m_homegf;
			}
			set
			{
				m_homegf = value;
			}
		}

		public int points
		{
			get
			{
				return m_points;
			}
			set
			{
				m_points = value;
			}
		}

		public int awayga
		{
			get
			{
				return m_awayga;
			}
			set
			{
				m_awayga = value;
			}
		}

		public int secondarytable
		{
			get
			{
				return m_secondarytable;
			}
			set
			{
				m_secondarytable = value;
			}
		}

		public int homewins
		{
			get
			{
				return m_homewins;
			}
			set
			{
				m_homewins = value;
			}
		}

		public int awaywins
		{
			get
			{
				return m_awaywins;
			}
			set
			{
				m_awaywins = value;
			}
		}

		public int homelosses
		{
			get
			{
				return m_homelosses;
			}
			set
			{
				m_homelosses = value;
			}
		}

		public int awaylosses
		{
			get
			{
				return m_awaylosses;
			}
			set
			{
				m_awaylosses = value;
			}
		}

		public int awaydraws
		{
			get
			{
				return m_awaydraws;
			}
			set
			{
				m_awaydraws = value;
			}
		}

		public int homedraws
		{
			get
			{
				return m_homedraws;
			}
			set
			{
				m_homedraws = value;
			}
		}

		public int maxvariationsneg
		{
			get
			{
				return m_maxvariationsneg;
			}
			set
			{
				m_maxvariationsneg = value;
			}
		}

		public int maxvariationspos
		{
			get
			{
				return m_maxvariationspos;
			}
			set
			{
				m_maxvariationspos = value;
			}
		}

		public int maxvariationsstd
		{
			get
			{
				return m_maxvariationsstd;
			}
			set
			{
				m_maxvariationsstd = value;
			}
		}

		public bool IsInTopLeague()
		{
			if (m_League == null)
			{
				return false;
			}
			for (int i = 0; i < c_TopLeaguesId.Length; i++)
			{
				if (m_League.Id == c_TopLeaguesId[i])
				{
					return true;
				}
			}
			return false;
		}

		public bool IsFemale()
		{
			bool result = false;
			if (m_Roster != null && m_Roster.Count > 0)
			{
				result = m_Roster.GetBestPlayer().Player.Female;
			}
			return result;
		}

		public static string RevModAdboardFileName(int id)
		{
			return Adboard.RevModTeamAdboardFileName(id);
		}

		public string RevModAdboardFileName()
		{
			return Adboard.RevModTeamAdboardFileName(base.Id);
		}

		public Bitmap GetRevModAdboard()
		{
			return Adboard.GetRevModTeamAdboard(base.Id);
		}

		public static string RevModNetFileName(int id)
		{
			return Net.RevModNetFileName(id);
		}

		public string RevModNetFileName()
		{
			return Net.RevModNetFileName(base.Id);
		}

		public Bitmap GetRevModNet()
		{
			return Net.GetRevModNet(base.Id);
		}

		public static string RevModManagerTextureFileName(int id)
		{
			return Manager.RevModManagerTextureFileName(id);
		}

		public string RevModManagerTextureFileName()
		{
			return Manager.RevModManagerTextureFileName(base.Id);
		}

		public static string RevModManagerModleFileName(int id)
		{
			return Manager.RevModManagerModelFileName(id);
		}

		public string RevModManagerModelFileName()
		{
			return Manager.RevModManagerModelFileName(base.Id);
		}

		public Rx3File GetRevModManagerModel()
		{
			return Manager.GetRevModManagerModel(base.Id);
		}

		public Bitmap GetRevModManagerTexture()
		{
			return Manager.GetRevModManagerTextures(base.Id);
		}

		public static string RevModBallTextureFileName(int id)
		{
			return Ball.RevModTeamBallTextureFileName(id);
		}

		public static string RevModBallModelFileName(int id)
		{
			return Ball.RevModTeamBallModelFileName(id);
		}

		public string RevModBallTextureFileName()
		{
			return Ball.RevModTeamBallTextureFileName(base.Id);
		}

		public string RevModBallModelFileName()
		{
			return Ball.RevModTeamBallModelFileName(base.Id);
		}

		public Bitmap[] GetRevModBallTextures()
		{
			return Ball.GetRevModTeamBallTextures(base.Id);
		}

		public bool SetRevModAdboard(Bitmap bitmap)
		{
			return Adboard.SetRevModTeamAdboard(base.Id, bitmap);
		}

		public bool SetRevModAdboard(string rx3FileName)
		{
			return Adboard.SetRevModTeamAdboard(base.Id, rx3FileName);
		}

		public bool SetRevModNet(Bitmap bitmap)
		{
			return Net.SetRevModNet(base.Id, bitmap);
		}

		public bool SetRevModNet(string rx3FileName)
		{
			return Net.SetRevModNet(base.Id, rx3FileName);
		}

		public bool SetRevModManagerTexture(Bitmap bitmap)
		{
			return Manager.SetRevModManagerTexture(base.Id, bitmap);
		}

		public bool SetRevModManagerModel(Bitmap bitmap)
		{
			return Manager.SetRevModManagerTexture(base.Id, bitmap);
		}

		public bool SetRevModManagerTexture(string rx3FileName)
		{
			return Manager.SetRevModManagerTexture(base.Id, rx3FileName);
		}

		public bool SetRevModManagerModel(string rx3FileName)
		{
			return Manager.SetRevModManagerModel(base.Id, rx3FileName);
		}

		public bool SetRevModBallTextures(Bitmap[] bitmaps)
		{
			return Ball.SetRevModTeamBallTextures(base.Id, bitmaps);
		}

		public bool SetRevModBallTextures(string rx3FileName)
		{
			return Ball.SetRevModTeamBallTextures(base.Id, rx3FileName);
		}

		public bool SetRevModBallModel(string rx3FileName)
		{
			return Ball.SetRevModTeamBallModel(base.Id, rx3FileName);
		}

		public bool ExportRevModBallTextures(string exportFolder)
		{
			return FifaEnvironment.ExportFileFromZdata(Ball.RevModTeamBallTextureFileName(base.Id), exportFolder);
		}

		public bool DeleteRevModBallTextures()
		{
			return Ball.DeleteRevModTeamBallTextures(base.Id);
		}

		public Bitmap GetAdboard()
		{
			Bitmap adboard = Adboard.GetAdboard(1000000 + base.Id);
			if (adboard == null)
			{
				adboard = Adboard.GetAdboard(adboardid);
				m_HasSpecificAdboard = false;
			}
			else
			{
				m_HasSpecificAdboard = true;
			}
			return adboard;
		}

		public bool SetAdboard(Bitmap bitmap)
		{
			if (m_HasSpecificAdboard)
			{
				return Adboard.SetAdboard(1000000 + base.Id, bitmap);
			}
			return Adboard.SetAdboard(adboardid, bitmap);
		}

		public bool CreateSpecificAdboard()
		{
			int adboardId = 1000000 + base.Id;
			Bitmap adboard = Adboard.GetAdboard(adboardid);
			for (int i = 0; i < 20; i++)
			{
				if (adboard != null)
				{
					break;
				}
				adboard = Adboard.GetAdboard(adboardid);
			}
			m_HasSpecificAdboard = true;
			return Adboard.SetAdboard(adboardId, adboard);
		}

		public bool DeleteSpecificAdboard()
		{
			int adboardId = 1000000 + base.Id;
			m_HasSpecificAdboard = false;
			return Adboard.DeleteAdboard(adboardId);
		}

		private void SetCountryAsNationalTeam(Country country)
		{
			m_Country = country;
			m_countryid_IfNationalTeam = country.Id;
			m_countryid_IfRowTeam = 0;
			m_countryid_IfLeagueTeam = 0;
		}

		private void SetCountryAsRowTeam(Country country)
		{
			if (country != null)
			{
				m_Country = country;
				m_countryid_IfNationalTeam = 0;
				m_countryid_IfRowTeam = country.Id;
				m_countryid_IfLeagueTeam = 0;
			}
		}

		private void SetCountryAsLeagueTeam(Country country)
		{
			m_Country = country;
			m_countryid_IfNationalTeam = 0;
			m_countryid_IfRowTeam = 0;
			m_countryid_IfLeagueTeam = country.Id;
		}

		private void ClearCountry()
		{
			m_Country = null;
			m_countryid_IfNationalTeam = 0;
			m_countryid_IfRowTeam = 0;
			m_countryid_IfLeagueTeam = 0;
		}

		private void SetAsNationalTeam(Country country)
		{
			SetCountryAsNationalTeam(country);
		}

		private void UnsetAsNationalTeam()
		{
			if (m_League != null && m_League.Country != null && m_Country == m_League.Country)
			{
				SetCountryAsLeagueTeam(m_Country);
			}
			SetCountryAsRowTeam(m_Country);
		}

		public bool IsNationalTeam()
		{
			return m_countryid_IfNationalTeam != 0;
		}

		public bool IsRowTeam()
		{
			if (!IsNationalTeam() && m_countryid_IfRowTeam > 0 && m_countryid_IfRowTeam != m_countryid_IfLeagueTeam)
			{
				return true;
			}
			return false;
		}

		public bool IsLeagueTeam()
		{
			if (!IsNationalTeam() && !IsRowTeam() && m_countryid_IfLeagueTeam > 0)
			{
				return true;
			}
			return false;
		}

		public bool IsClub()
		{
			if (!IsNationalTeam() && base.Id != 111072 && base.Id != 111205 && base.Id != 112190 && base.Id != 111596)
			{
				return base.Id != 111592;
			}
			return false;
		}

		public override string ToString()
		{
			if (DatabaseName.EndsWith(" X"))
			{
				return DatabaseName;
			}
			if (m_TeamNameFull != null && m_TeamNameFull != string.Empty)
			{
				return m_TeamNameFull;
			}
			if (m_teamname != null)
			{
				m_TeamNameFull = m_teamname;
				return m_teamname;
			}
			return string.Empty;
		}

		public Team(Record recTeams)
			: base(recTeams.IntField[FI.teams_teamid])
		{
			Load(recTeams);
		}

		public Team(int teamId)
		{
			base.Id = teamId;
			m_assetid = teamId;
			m_TeamNameFull = "Team " + teamId.ToString();
			InitNewTeam();
		}

		public void SetNameAutomatically(string longName, int length)
		{
			switch (length)
			{
			case 15:
				if (m_TeamNameAbbr15 != null && (m_TeamNameAbbr15 == null || !(m_TeamNameAbbr15 == string.Empty)) && !m_TeamNameAbbr15.StartsWith("Team"))
				{
					break;
				}
				if (longName.Length <= length)
				{
					m_TeamNameAbbr15 = longName;
					break;
				}
				m_TeamNameAbbr15 = longName.Substring(0, length - 1);
				if (!longName.Substring(length - 2, 2).Contains(" "))
				{
					m_TeamNameAbbr15 += ".";
				}
				break;
			case 10:
				if (m_TeamNameAbbr10 != null && (m_TeamNameAbbr10 == null || !(m_TeamNameAbbr10 == string.Empty)) && !m_TeamNameAbbr10.StartsWith("Team"))
				{
					break;
				}
				if (longName.Length <= length)
				{
					m_TeamNameAbbr10 = longName;
					break;
				}
				m_TeamNameAbbr10 = longName.Substring(0, length - 1);
				if (!longName.Substring(length - 2, 2).Contains(" "))
				{
					m_TeamNameAbbr10 += ".";
				}
				break;
			case 7:
				if (m_TeamNameAbbr7 != null && (m_TeamNameAbbr7 == null || !(m_TeamNameAbbr7 == string.Empty)) && !m_TeamNameAbbr7.StartsWith("Team"))
				{
					break;
				}
				if (longName.Length <= length)
				{
					m_TeamNameAbbr7 = longName;
					break;
				}
				m_TeamNameAbbr7 = longName.Substring(0, length - 1);
				if (!longName.Substring(length - 2, 2).Contains(" "))
				{
					m_TeamNameAbbr7 += ".";
				}
				break;
			case 3:
				if (m_TeamNameAbbr3 == null || (m_TeamNameAbbr3 != null && m_TeamNameAbbr3 == string.Empty))
				{
					if (longName.Length <= length)
					{
						m_TeamNameAbbr3 = longName;
					}
					else
					{
						m_TeamNameAbbr3 = longName.Substring(0, length);
					}
					m_TeamNameAbbr3 = m_TeamNameAbbr3.ToUpper();
				}
				break;
			}
		}

		public void InitNewTeam()
		{
			m_TeamNameAbbr10 = ((m_TeamNameFull.Length > 10) ? m_TeamNameFull.Substring(0, 10) : m_TeamNameFull);
			m_TeamNameAbbr15 = ((m_TeamNameFull.Length > 15) ? m_TeamNameFull.Substring(0, 15) : m_TeamNameFull);
			m_TeamNameAbbr7 = ((m_TeamNameFull.Length > 7) ? m_TeamNameFull.Substring(0, 7) : m_TeamNameFull);
			m_TeamNameAbbr3 = "XYZ";
			m_teamname = m_TeamNameFull;
			m_balltype = 1;
			m_adboardid = 1;
			m_Stadium = null;
			m_stadiumid = -1;
			m_genericbanner = false;
			m_jerseytype = 1;
			m_physioid_primary = 1;
			m_physioid_secondary = 2;
			m_teamcolor1r = 255;
			m_teamcolor1g = 255;
			m_teamcolor1b = 255;
			m_teamcolor2r = 0;
			m_teamcolor2g = 0;
			m_teamcolor2b = 0;
			m_teamcolor3r = 128;
			m_teamcolor3g = 128;
			m_teamcolor3b = 128;
			m_TeamColor1 = Color.FromArgb(255, m_teamcolor1r, m_teamcolor1g, m_teamcolor1b);
			m_TeamColor2 = Color.FromArgb(255, m_teamcolor2r, m_teamcolor2g, m_teamcolor2b);
			m_TeamColor3 = Color.FromArgb(255, m_teamcolor3r, m_teamcolor3g, m_teamcolor3b);
			m_form = 0;
			m_managerid = 8105;
			m_stadiumcustomname = null;
			m_ManagerSurname = null;
			m_ManagerFirstName = null;
			m_fancrowdhairskintexturecode = 0;
			m_stafftracksuitcolorcode = 0;
			for (int i = 0; i < 10; i++)
			{
				m_teamkitidList[i] = -1;
			}
			m_RivalTeam = null;
			m_rivalteam = 1;
			m_assetid = base.Id;
			m_transferbudget = 1000000;
			m_internationalprestige = 10;
			m_domesticprestige = 10;
			m_formationid = 0;
			m_busbuildupspeed = 50;
			m_buspassing = 50;
			m_buspositioning = 0;
			m_ccpassing = 50;
			m_cccrossing = 50;
			m_ccshooting = 50;
			m_ccpositioning = 0;
			m_defmentality = 50;
			m_defaggression = 50;
			m_defteamwidth = 50;
			m_defdefenderline = 0;
			m_captainid = 1;
			m_penaltytakerid = 1;
			m_freekicktakerid = 1;
			m_leftfreekicktakerid = 1;
			m_rightfreekicktakerid = 1;
			m_longkicktakerid = 1;
			m_leftcornerkicktakerid = 1;
			m_rightcornerkicktakerid = 1;
			PlayerCaptain = null;
			PlayerPenalty = null;
			PlayerFreeKick = null;
			PlayerLongKick = null;
			PlayerLeftCorner = null;
			PlayerRightCorner = null;
			m_numtransfersin = 0;
			m_genericint2 = -1;
			m_genericint1 = -1;
			m_latitude = 0;
			m_longitude = 0;
			m_utcoffset = 0;
			m_powid = -1;
			m_midfieldrating = 50;
			m_defenserating = 50;
			m_attackrating = 50;
			m_overallrating = 50;
			m_matchdayoverallrating = 50;
			m_matchdaydefenserating = 50;
			m_matchdaymidfieldrating = 50;
			m_matchdayattackrating = 50;
			m_suitvariationid = 0;
			m_suittypeid = 0;
			m_bodytypeid = 1;
			m_ethnicity = 2;
			m_personalityid = 0;
			m_countryid_IfNationalTeam = 0;
			m_countryid_IfRowTeam = 0;
			m_countryid_IfLeagueTeam = 0;
			m_Country = null;
			m_leagueid = 0;
			m_League = null;
			m_prevleagueid = 0;
			m_PrevLeague = null;
			m_champion = false;
			m_previousyeartableposition = 1;
			m_currenttableposition = 1;
			m_teamshortform = 50;
			m_teamlongform = 50;
			m_teamform = 0;
			m_hasachievedobjective = false;
			m_yettowin = false;
			m_unbeatenallcomps = false;
			m_unbeatenaway = false;
			m_unbeatenhome = false;
			m_unbeatenleague = false;
			m_highestpossible = 0;
			m_highestprobable = 0;
			m_nummatchesplayed = 0;
			m_gapresult = 0;
			m_grouping = 0;
			m_objective = 0;
			m_actualvsexpectations = 0;
			m_lastgameresult = 0;
			m_busdribbling = 50;
			m_trait1 = 0;
			m_ImpatientBoard = false;
			m_LoyalBoard = false;
			m_SquadRotation = false;
			m_ConsistentLineup = false;
			m_SwitchWingers = false;
			m_CenterBacksSplit = false;
			m_DefendLead = false;
			m_KeepUpPressure = false;
			m_MoreAttackingAtHome = false;
			m_ShortOutBack = false;
			m_homega = 0;
			m_homegf = 0;
			m_points = 0;
			m_awayga = 0;
			m_secondarytable = 0;
			m_homewins = 0;
			m_awaywins = 0;
			m_homelosses = 0;
			m_awaylosses = 0;
			m_awaydraws = 0;
			m_homedraws = 0;
			m_crestweight = 25;
			m_genericweight = 25;
			m_teamweight = 50;
			m_maxvariationsneg = 0;
			m_maxvariationspos = 0;
			m_maxvariationsstd = 0;
		}

		public void Load17(Record r, TableDescriptor td)
		{
			m_assetid = base.Id;
			m_teamname = r.StringField[td.GetFieldIndex("teamname")];
			m_transferbudget = r.GetAndCheckIntField(td.GetFieldIndex("transferbudget"));
			m_domesticprestige = r.GetAndCheckIntField(td.GetFieldIndex("domesticprestige"));
			m_internationalprestige = r.GetAndCheckIntField(td.GetFieldIndex("internationalprestige"));
			m_rivalteam = r.GetAndCheckIntField(td.GetFieldIndex("rivalteam"));
			m_captainid = r.GetAndCheckIntField(td.GetFieldIndex("captainid"));
			m_penaltytakerid = r.GetAndCheckIntField(td.GetFieldIndex("penaltytakerid"));
			m_freekicktakerid = r.GetAndCheckIntField(td.GetFieldIndex("freekicktakerid"));
			m_leftfreekicktakerid = r.GetAndCheckIntField(td.GetFieldIndex("leftfreekicktakerid"));
			m_rightfreekicktakerid = r.GetAndCheckIntField(td.GetFieldIndex("rightfreekicktakerid"));
			m_longkicktakerid = r.GetAndCheckIntField(td.GetFieldIndex("longkicktakerid"));
			m_leftcornerkicktakerid = r.GetAndCheckIntField(td.GetFieldIndex("leftcornerkicktakerid"));
			m_rightcornerkicktakerid = r.GetAndCheckIntField(td.GetFieldIndex("rightcornerkicktakerid"));
			m_adboardid = r.GetAndCheckIntField(td.GetFieldIndex("adboardid"));
			m_balltype = r.GetAndCheckIntField(td.GetFieldIndex("balltype"));
			m_genericbanner = (r.GetAndCheckIntField(td.GetFieldIndex("genericbanner")) != 0);
			m_jerseytype = r.GetAndCheckIntField(td.GetFieldIndex("jerseytype"));
			m_fancrowdhairskintexturecode = r.GetAndCheckIntField(td.GetFieldIndex("fancrowdhairskintexturecode"));
			m_stafftracksuitcolorcode = r.GetAndCheckIntField(td.GetFieldIndex("stafftracksuitcolorcode"));
			m_busbuildupspeed = r.GetAndCheckIntField(td.GetFieldIndex("busbuildupspeed"));
			m_buspassing = r.GetAndCheckIntField(td.GetFieldIndex("buspassing"));
			if (FifaEnvironment.Year == 14)
			{
				m_buspositioning = r.GetAndCheckIntField(td.GetFieldIndex("buspositioning")) - 1;
				m_ccpositioning = r.GetAndCheckIntField(td.GetFieldIndex("ccpositioning")) - 1;
				m_defdefenderline = r.GetAndCheckIntField(td.GetFieldIndex("defdefenderline")) - 1;
			}
			else
			{
				m_buspositioning = r.GetAndCheckIntField(td.GetFieldIndex("buspositioning"));
				m_ccpositioning = r.GetAndCheckIntField(td.GetFieldIndex("ccpositioning"));
				m_defdefenderline = r.GetAndCheckIntField(td.GetFieldIndex("defdefenderline"));
			}
			m_busdribbling = r.GetAndCheckIntField(td.GetFieldIndex("busdribbling"));
			m_cccrossing = r.GetAndCheckIntField(td.GetFieldIndex("cccrossing"));
			m_ccpassing = r.GetAndCheckIntField(td.GetFieldIndex("ccpassing"));
			m_ccshooting = r.GetAndCheckIntField(td.GetFieldIndex("ccshooting"));
			m_defmentality = r.GetAndCheckIntField(td.GetFieldIndex("defmentality"));
			m_defaggression = r.GetAndCheckIntField(td.GetFieldIndex("defaggression"));
			m_defteamwidth = r.GetAndCheckIntField(td.GetFieldIndex("defteamwidth"));
			m_physioid_primary = r.GetAndCheckIntField(td.GetFieldIndex("physioid_primary"));
			m_physioid_secondary = r.GetAndCheckIntField(td.GetFieldIndex("physioid_secondary"));
			m_teamcolor1r = r.GetAndCheckIntField(td.GetFieldIndex("teamcolor1r"));
			m_teamcolor1g = r.GetAndCheckIntField(td.GetFieldIndex("teamcolor1g"));
			m_teamcolor1b = r.GetAndCheckIntField(td.GetFieldIndex("teamcolor1b"));
			m_teamcolor2r = r.GetAndCheckIntField(td.GetFieldIndex("teamcolor2r"));
			m_teamcolor2g = r.GetAndCheckIntField(td.GetFieldIndex("teamcolor2g"));
			m_teamcolor2b = r.GetAndCheckIntField(td.GetFieldIndex("teamcolor2b"));
			m_teamcolor3r = r.GetAndCheckIntField(td.GetFieldIndex("teamcolor3r"));
			m_teamcolor3g = r.GetAndCheckIntField(td.GetFieldIndex("teamcolor3g"));
			m_teamcolor3b = r.GetAndCheckIntField(td.GetFieldIndex("teamcolor3b"));
			m_TeamColor1 = Color.FromArgb(255, m_teamcolor1r, m_teamcolor1g, m_teamcolor1b);
			m_TeamColor2 = Color.FromArgb(255, m_teamcolor2r, m_teamcolor2g, m_teamcolor2b);
			m_TeamColor3 = Color.FromArgb(255, m_teamcolor3r, m_teamcolor3g, m_teamcolor3b);
			m_form = r.GetAndCheckIntField(td.GetFieldIndex("form"));
			m_latitude = r.GetAndCheckIntField(td.GetFieldIndex("latitude"));
			m_longitude = r.GetAndCheckIntField(td.GetFieldIndex("longitude"));
			m_utcoffset = r.GetAndCheckIntField(td.GetFieldIndex("utcoffset"));
			m_powid = -1;
			m_midfieldrating = r.GetAndCheckIntField(td.GetFieldIndex("midfieldrating"));
			m_defenserating = r.GetAndCheckIntField(td.GetFieldIndex("defenserating"));
			m_attackrating = r.GetAndCheckIntField(td.GetFieldIndex("attackrating"));
			m_overallrating = r.GetAndCheckIntField(td.GetFieldIndex("overallrating"));
			m_matchdayoverallrating = r.GetAndCheckIntField(td.GetFieldIndex("matchdayoverallrating"));
			m_matchdaydefenserating = r.GetAndCheckIntField(td.GetFieldIndex("matchdaydefenserating"));
			m_matchdaymidfieldrating = r.GetAndCheckIntField(td.GetFieldIndex("matchdaymidfieldrating"));
			m_matchdayattackrating = r.GetAndCheckIntField(td.GetFieldIndex("matchdayattackrating"));
			m_suitvariationid = r.GetAndCheckIntField(td.GetFieldIndex("suitvariationid"));
			m_suittypeid = r.GetAndCheckIntField(td.GetFieldIndex("suittypeid"));
			m_bodytypeid = r.GetAndCheckIntField(td.GetFieldIndex("bodytypeid"));
			m_ethnicity = r.GetAndCheckIntField(td.GetFieldIndex("ethnicity"));
			m_personalityid = r.GetAndCheckIntField(td.GetFieldIndex("personalityid"));
			m_trait1 = r.GetAndCheckIntField(td.GetFieldIndex("trait1"));
			m_ImpatientBoard = (((m_trait1 & 1) != 0) ? true : false);
			m_LoyalBoard = (((m_trait1 & 2) != 0) ? true : false);
			m_SquadRotation = (((m_trait1 & 4) != 0) ? true : false);
			m_ConsistentLineup = (((m_trait1 & 8) != 0) ? true : false);
			m_SwitchWingers = (((m_trait1 & 0x10) != 0) ? true : false);
			m_CenterBacksSplit = (((m_trait1 & 0x20) != 0) ? true : false);
			m_DefendLead = (((m_trait1 & 0x40) != 0) ? true : false);
			m_KeepUpPressure = (((m_trait1 & 0x80) != 0) ? true : false);
			m_MoreAttackingAtHome = (((m_trait1 & 0x100) != 0) ? true : false);
			m_ShortOutBack = (((m_trait1 & 0x200) != 0) ? true : false);
			m_numtransfersin = r.GetAndCheckIntField(td.GetFieldIndex("numtransfersin"));
			m_genericint2 = r.GetAndCheckIntField(td.GetFieldIndex("genericint1"));
			m_genericint1 = r.GetAndCheckIntField(td.GetFieldIndex("genericint2"));
			if (FifaEnvironment.Language != null)
			{
				m_TeamNameFull = FifaEnvironment.Language.GetTeamString(base.Id, Language.ETeamStringType.Full);
				m_TeamNameAbbr15 = FifaEnvironment.Language.GetTeamString(base.Id, Language.ETeamStringType.Abbr15);
				m_TeamNameAbbr10 = FifaEnvironment.Language.GetTeamString(base.Id, Language.ETeamStringType.Abbr10);
				m_TeamNameAbbr3 = FifaEnvironment.Language.GetTeamString(base.Id, Language.ETeamStringType.Abbr3);
				m_TeamNameAbbr7 = FifaEnvironment.Language.GetTeamString(base.Id, Language.ETeamStringType.Abbr7);
				if (m_TeamNameFull == null || m_TeamNameFull == string.Empty)
				{
					m_TeamNameFull = m_TeamNameAbbr10;
				}
			}
			else
			{
				m_TeamNameFull = string.Empty;
				m_TeamNameAbbr15 = string.Empty;
				m_TeamNameAbbr3 = string.Empty;
				m_TeamNameAbbr7 = string.Empty;
				m_TeamNameAbbr10 = string.Empty;
			}
		}

		public void Load(Record r)
		{
			m_assetid = base.Id;
			m_teamname = r.StringField[FI.teams_teamname];
			m_transferbudget = r.GetAndCheckIntField(FI.teams_transferbudget);
			m_transferbudget = m_transferbudget / 1000 * 1000;
			m_domesticprestige = r.GetAndCheckIntField(FI.teams_domesticprestige);
			m_internationalprestige = r.GetAndCheckIntField(FI.teams_internationalprestige);
			m_rivalteam = r.GetAndCheckIntField(FI.teams_rivalteam);
			m_captainid = r.GetAndCheckIntField(FI.teams_captainid);
			m_penaltytakerid = r.GetAndCheckIntField(FI.teams_penaltytakerid);
			m_freekicktakerid = r.GetAndCheckIntField(FI.teams_freekicktakerid);
			if (FI.teams_leftfreekicktakerid >= 0)
			{
				m_leftfreekicktakerid = r.GetAndCheckIntField(FI.teams_leftfreekicktakerid);
			}
			if (FI.teams_rightfreekicktakerid >= 0)
			{
				m_rightfreekicktakerid = r.GetAndCheckIntField(FI.teams_rightfreekicktakerid);
			}
			m_longkicktakerid = r.GetAndCheckIntField(FI.teams_longkicktakerid);
			m_leftcornerkicktakerid = r.GetAndCheckIntField(FI.teams_leftcornerkicktakerid);
			m_rightcornerkicktakerid = r.GetAndCheckIntField(FI.teams_rightcornerkicktakerid);
			m_adboardid = r.GetAndCheckIntField(FI.teams_adboardid);
			m_balltype = r.GetAndCheckIntField(FI.teams_balltype);
			m_genericbanner = (r.GetAndCheckIntField(FI.teams_genericbanner) != 0);
			m_jerseytype = r.GetAndCheckIntField(FI.teams_jerseytype);
			m_fancrowdhairskintexturecode = r.GetAndCheckIntField(FI.teams_fancrowdhairskintexturecode);
			m_stafftracksuitcolorcode = r.GetAndCheckIntField(FI.teams_stafftracksuitcolorcode);
			m_busbuildupspeed = r.GetAndCheckIntField(FI.teams_busbuildupspeed);
			m_buspassing = r.GetAndCheckIntField(FI.teams_buspassing);
			if (FifaEnvironment.Year == 14)
			{
				m_buspositioning = r.GetAndCheckIntField(FI.teams_buspositioning) - 1;
				m_ccpositioning = r.GetAndCheckIntField(FI.teams_ccpositioning) - 1;
				m_defdefenderline = r.GetAndCheckIntField(FI.teams_defdefenderline) - 1;
			}
			else
			{
				m_buspositioning = r.GetAndCheckIntField(FI.teams_buspositioning);
				m_ccpositioning = r.GetAndCheckIntField(FI.teams_ccpositioning);
				m_defdefenderline = r.GetAndCheckIntField(FI.teams_defdefenderline);
			}
			m_busdribbling = r.GetAndCheckIntField(FI.teams_busdribbling);
			m_cccrossing = r.GetAndCheckIntField(FI.teams_cccrossing);
			m_ccpassing = r.GetAndCheckIntField(FI.teams_ccpassing);
			m_ccshooting = r.GetAndCheckIntField(FI.teams_ccshooting);
			m_defmentality = r.GetAndCheckIntField(FI.teams_defmentality);
			m_defaggression = r.GetAndCheckIntField(FI.teams_defaggression);
			m_defteamwidth = r.GetAndCheckIntField(FI.teams_defteamwidth);
			m_physioid_primary = r.GetAndCheckIntField(FI.teams_physioid_primary);
			m_physioid_secondary = r.GetAndCheckIntField(FI.teams_physioid_secondary);
			m_teamcolor1r = r.GetAndCheckIntField(FI.teams_teamcolor1r);
			m_teamcolor1g = r.GetAndCheckIntField(FI.teams_teamcolor1g);
			m_teamcolor1b = r.GetAndCheckIntField(FI.teams_teamcolor1b);
			m_teamcolor2r = r.GetAndCheckIntField(FI.teams_teamcolor2r);
			m_teamcolor2g = r.GetAndCheckIntField(FI.teams_teamcolor2g);
			m_teamcolor2b = r.GetAndCheckIntField(FI.teams_teamcolor2b);
			m_teamcolor3r = r.GetAndCheckIntField(FI.teams_teamcolor3r);
			m_teamcolor3g = r.GetAndCheckIntField(FI.teams_teamcolor3g);
			m_teamcolor3b = r.GetAndCheckIntField(FI.teams_teamcolor3b);
			m_TeamColor1 = Color.FromArgb(255, m_teamcolor1r, m_teamcolor1g, m_teamcolor1b);
			m_TeamColor2 = Color.FromArgb(255, m_teamcolor2r, m_teamcolor2g, m_teamcolor2b);
			m_TeamColor3 = Color.FromArgb(255, m_teamcolor3r, m_teamcolor3g, m_teamcolor3b);
			m_form = r.GetAndCheckIntField(FI.teams_form);
			m_latitude = r.GetAndCheckIntField(FI.teams_latitude);
			m_longitude = r.GetAndCheckIntField(FI.teams_longitude);
			m_utcoffset = r.GetAndCheckIntField(FI.teams_utcoffset);
			m_powid = r.GetAndCheckIntField(FI.teams_powid);
			if (m_powid == 0)
			{
				m_powid = -1;
			}
			m_midfieldrating = r.GetAndCheckIntField(FI.teams_midfieldrating);
			m_defenserating = r.GetAndCheckIntField(FI.teams_defenserating);
			m_attackrating = r.GetAndCheckIntField(FI.teams_attackrating);
			m_overallrating = r.GetAndCheckIntField(FI.teams_overallrating);
			m_matchdayoverallrating = r.GetAndCheckIntField(FI.teams_matchdayoverallrating);
			m_matchdaydefenserating = r.GetAndCheckIntField(FI.teams_matchdaydefenserating);
			m_matchdaymidfieldrating = r.GetAndCheckIntField(FI.teams_matchdaymidfieldrating);
			m_matchdayattackrating = r.GetAndCheckIntField(FI.teams_matchdayattackrating);
			m_suitvariationid = r.GetAndCheckIntField(FI.teams_suitvariationid);
			m_suittypeid = r.GetAndCheckIntField(FI.teams_suittypeid);
			m_bodytypeid = r.GetAndCheckIntField(FI.teams_bodytypeid);
			m_ethnicity = r.GetAndCheckIntField(FI.teams_ethnicity);
			m_personalityid = r.GetAndCheckIntField(FI.teams_personalityid);
			m_trait1 = r.GetAndCheckIntField(FI.teams_trait1);
			m_ImpatientBoard = (((m_trait1 & 1) != 0) ? true : false);
			m_LoyalBoard = (((m_trait1 & 2) != 0) ? true : false);
			m_SquadRotation = (((m_trait1 & 4) != 0) ? true : false);
			m_ConsistentLineup = (((m_trait1 & 8) != 0) ? true : false);
			m_SwitchWingers = (((m_trait1 & 0x10) != 0) ? true : false);
			m_CenterBacksSplit = (((m_trait1 & 0x20) != 0) ? true : false);
			m_DefendLead = (((m_trait1 & 0x40) != 0) ? true : false);
			m_KeepUpPressure = (((m_trait1 & 0x80) != 0) ? true : false);
			m_MoreAttackingAtHome = (((m_trait1 & 0x100) != 0) ? true : false);
			m_ShortOutBack = (((m_trait1 & 0x200) != 0) ? true : false);
			m_numtransfersin = r.GetAndCheckIntField(FI.teams_numtransfersin);
			m_genericint2 = r.GetAndCheckIntField(FI.teams_genericint1);
			m_genericint1 = r.GetAndCheckIntField(FI.teams_genericint2);
			if (FifaEnvironment.Language != null)
			{
				m_TeamNameFull = FifaEnvironment.Language.GetTeamString(base.Id, Language.ETeamStringType.Full);
				m_TeamNameAbbr15 = FifaEnvironment.Language.GetTeamString(base.Id, Language.ETeamStringType.Abbr15);
				m_TeamNameAbbr10 = FifaEnvironment.Language.GetTeamString(base.Id, Language.ETeamStringType.Abbr10);
				m_TeamNameAbbr3 = FifaEnvironment.Language.GetTeamString(base.Id, Language.ETeamStringType.Abbr3);
				m_TeamNameAbbr7 = FifaEnvironment.Language.GetTeamString(base.Id, Language.ETeamStringType.Abbr7);
				if (m_TeamNameFull == null || m_TeamNameFull == string.Empty)
				{
					m_TeamNameFull = m_TeamNameAbbr10;
				}
			}
			else
			{
				m_TeamNameFull = string.Empty;
				m_TeamNameAbbr15 = string.Empty;
				m_TeamNameAbbr3 = string.Empty;
				m_TeamNameAbbr7 = string.Empty;
				m_TeamNameAbbr10 = string.Empty;
			}
		}

		public void FillFromStadiumAssignments(Record r)
		{
			m_stadiumcustomname = r.StringField[FI.stadiumassignments_stadiumcustomname];
		}

		public void FillFromManager(Record r)
		{
			m_ManagerFirstName = r.StringField[FI.manager_firstname];
			m_ManagerSurname = r.StringField[FI.manager_surname];
		}

		public void FillFromTeamStadiumLinks(Record r)
		{
			m_stadiumid = r.GetAndCheckIntField(FI.teamstadiumlinks_stadiumid);
		}

		public void FillFromTeamkits(Record r)
		{
			int andCheckIntField = r.GetAndCheckIntField(FI.teamkits_teamkittypetechid);
			if (andCheckIntField < m_teamkitidList.Length && r.GetAndCheckIntField(FI.teamkits_year) == 0)
			{
				m_teamkitidList[andCheckIntField] = r.GetAndCheckIntField(FI.teamkits_teamkitid);
			}
		}

		public void FillFromNewspicweights(Record r)
		{
			m_crestweight = r.GetAndCheckIntField(FI.career_newspicweights_crestweight);
			m_genericweight = r.GetAndCheckIntField(FI.career_newspicweights_genericweight);
			m_teamweight = r.GetAndCheckIntField(FI.career_newspicweights_teamweight);
			m_maxvariationsneg = r.GetAndCheckIntField(FI.career_newspicweights_maxvariationsneg);
			m_maxvariationspos = r.GetAndCheckIntField(FI.career_newspicweights_maxvariationspos);
			m_maxvariationsstd = r.GetAndCheckIntField(FI.career_newspicweights_maxvariationsstd);
		}

		public void FillFromFormations(Record r)
		{
			m_formationid = r.GetAndCheckIntField(FI.formations_formationid);
		}

		public void FillFromTeamFormationLinks(Record r)
		{
			m_formationid = r.GetAndCheckIntField(FI.teamformationteamstylelinks_formationid);
		}

		public void FillFromNations(Record r)
		{
			m_countryid_IfNationalTeam = r.GetAndCheckIntField(FI.nations_nationid);
		}

		public void FillFromTeamNationLinks(Record r)
		{
			m_countryid_IfNationalTeam = r.GetAndCheckIntField(FI.teamnationlinks_nationid);
		}

		public void FillFromLeagueTeamLinks(Record r)
		{
			m_leagueid = r.GetAndCheckIntField(FI.leagueteamlinks_leagueid);
			m_prevleagueid = r.GetAndCheckIntField(FI.leagueteamlinks_prevleagueid);
			m_champion = ((r.GetAndCheckIntField(FI.leagueteamlinks_champion) != 0) ? true : false);
			m_previousyeartableposition = r.GetAndCheckIntField(FI.leagueteamlinks_previousyeartableposition);
			m_currenttableposition = r.GetAndCheckIntField(FI.leagueteamlinks_currenttableposition);
			m_teamshortform = r.GetAndCheckIntField(FI.leagueteamlinks_teamshortform);
			m_teamlongform = r.GetAndCheckIntField(FI.leagueteamlinks_teamlongform);
			m_teamform = r.GetAndCheckIntField(FI.leagueteamlinks_teamform);
			m_hasachievedobjective = ((r.GetAndCheckIntField(FI.leagueteamlinks_hasachievedobjective) != 0) ? true : false);
			m_yettowin = ((r.GetAndCheckIntField(FI.leagueteamlinks_yettowin) != 0) ? true : false);
			m_unbeatenallcomps = ((r.GetAndCheckIntField(FI.leagueteamlinks_unbeatenallcomps) != 0) ? true : false);
			m_unbeatenaway = ((r.GetAndCheckIntField(FI.leagueteamlinks_unbeatenaway) != 0) ? true : false);
			m_unbeatenhome = ((r.GetAndCheckIntField(FI.leagueteamlinks_unbeatenhome) != 0) ? true : false);
			m_unbeatenleague = ((r.GetAndCheckIntField(FI.leagueteamlinks_unbeatenleague) != 0) ? true : false);
			m_highestpossible = r.GetAndCheckIntField(FI.leagueteamlinks_highestpossible);
			m_highestprobable = r.GetAndCheckIntField(FI.leagueteamlinks_highestprobable);
			m_nummatchesplayed = r.GetAndCheckIntField(FI.leagueteamlinks_nummatchesplayed);
			m_gapresult = r.GetAndCheckIntField(FI.leagueteamlinks_gapresult);
			m_grouping = r.GetAndCheckIntField(FI.leagueteamlinks_grouping);
			m_objective = r.GetAndCheckIntField(FI.leagueteamlinks_objective);
			m_actualvsexpectations = r.GetAndCheckIntField(FI.leagueteamlinks_actualvsexpectations);
			m_lastgameresult = r.GetAndCheckIntField(FI.leagueteamlinks_lastgameresult);
			m_homega = r.GetAndCheckIntField(FI.leagueteamlinks_homega);
			m_homegf = r.GetAndCheckIntField(FI.leagueteamlinks_homegf);
			m_points = r.GetAndCheckIntField(FI.leagueteamlinks_points);
			m_awayga = r.GetAndCheckIntField(FI.leagueteamlinks_awayga);
			m_secondarytable = r.GetAndCheckIntField(FI.leagueteamlinks_secondarytable);
			m_homewins = r.GetAndCheckIntField(FI.leagueteamlinks_homewins);
			m_awaywins = r.GetAndCheckIntField(FI.leagueteamlinks_awaywins);
			m_homelosses = r.GetAndCheckIntField(FI.leagueteamlinks_homelosses);
			m_awaylosses = r.GetAndCheckIntField(FI.leagueteamlinks_awaylosses);
			m_awaydraws = r.GetAndCheckIntField(FI.leagueteamlinks_awaydraws);
			m_homedraws = r.GetAndCheckIntField(FI.leagueteamlinks_homedraws);
			m_points = r.GetAndCheckIntField(FI.leagueteamlinks_points);
		}

		public void FillFromRowTeamNationLinks(Record r)
		{
			m_countryid_IfRowTeam = r.GetAndCheckIntField(FI.rowteamnationlinks_nationid);
		}

		public void LinkBall(BallList ballList)
		{
			if (ballList != null)
			{
				m_Ball = (Ball)ballList.SearchId(m_balltype);
			}
		}

		public void LinkKits(KitList kitList)
		{
			if (kitList == null)
			{
				return;
			}
			for (int i = 0; i < m_teamkitidList.Length; i++)
			{
				if (m_teamkitidList[i] >= 0)
				{
					Kit value = (Kit)kitList.SearchId(m_teamkitidList[i]);
					m_KitList.Add(value);
				}
			}
		}

		public void LinkStadium(StadiumList stadiumList)
		{
			if (stadiumList != null)
			{
				m_Stadium = (Stadium)stadiumList.SearchId(m_stadiumid);
			}
		}

		public void LinkTeam(TeamList teamList)
		{
			if (teamList != null)
			{
				m_RivalTeam = (Team)teamList.SearchId(m_rivalteam);
			}
		}

		public void LinkCountry(CountryList countryList)
		{
			if (countryList == null)
			{
				return;
			}
			if (IsNationalTeam())
			{
				m_Country = (Country)countryList.SearchId(m_countryid_IfNationalTeam);
				if (m_Country != null && !IsFemale())
				{
					m_Country.SetNationalTeam(this, base.Id);
				}
			}
			else if (IsRowTeam())
			{
				m_Country = (Country)countryList.SearchId(m_countryid_IfRowTeam);
			}
			else if (m_League != null && m_League.Country != null)
			{
				SetCountryAsLeagueTeam(m_League.Country);
			}
		}

		public void LinkFormation(FormationList formationList)
		{
			if (formationList != null)
			{
				m_Formation = (Formation)formationList.SearchId(m_formationid);
			}
		}

		public void LinkFormation(Formation formation)
		{
			m_Formation = formation;
		}

		public void LinkPlayer(PlayerList playerList)
		{
			if (playerList != null)
			{
				PlayerCaptain = (Player)playerList.SearchId(m_captainid);
				PlayerFreeKick = (Player)playerList.SearchId(m_freekicktakerid);
				PlayerLeftFreeKick = (Player)playerList.SearchId(m_leftfreekicktakerid);
				PlayerRightFreeKick = (Player)playerList.SearchId(m_rightfreekicktakerid);
				PlayerLongKick = (Player)playerList.SearchId(m_longkicktakerid);
				PlayerPenalty = (Player)playerList.SearchId(m_penaltytakerid);
				PlayerLeftCorner = (Player)playerList.SearchId(m_leftcornerkicktakerid);
				PlayerRightCorner = (Player)playerList.SearchId(m_rightcornerkicktakerid);
			}
		}

		public void LinkLeague(LeagueList leagueList)
		{
			if (leagueList != null)
			{
				m_League = (League)leagueList.SearchId(m_leagueid);
				m_PrevLeague = (League)leagueList.SearchId(m_prevleagueid);
			}
		}

		public bool IsPlayingInLeague(League league)
		{
			return m_leagueid == league.Id;
		}

		public void SaveTeam(Record r)
		{
			r.IntField[FI.teams_teamid] = base.Id;
			r.StringField[FI.teams_teamname] = m_teamname;
			r.IntField[FI.teams_balltype] = m_balltype;
			r.IntField[FI.teams_adboardid] = m_adboardid;
			r.IntField[FI.teams_genericbanner] = (m_genericbanner ? 1 : 0);
			r.IntField[FI.teams_jerseytype] = m_jerseytype;
			r.IntField[FI.teams_stafftracksuitcolorcode] = m_stafftracksuitcolorcode;
			r.IntField[FI.teams_fancrowdhairskintexturecode] = m_fancrowdhairskintexturecode;
			r.IntField[FI.teams_physioid_primary] = m_physioid_primary;
			r.IntField[FI.teams_physioid_secondary] = m_physioid_secondary;
			m_teamcolor1r = m_TeamColor1.R;
			m_teamcolor1g = m_TeamColor1.G;
			m_teamcolor1b = m_TeamColor1.B;
			m_teamcolor2r = m_TeamColor2.R;
			m_teamcolor2g = m_TeamColor2.G;
			m_teamcolor2b = m_TeamColor2.B;
			m_teamcolor3r = m_TeamColor3.R;
			m_teamcolor3g = m_TeamColor3.G;
			m_teamcolor3b = m_TeamColor3.B;
			r.IntField[FI.teams_teamcolor1r] = m_teamcolor1r;
			r.IntField[FI.teams_teamcolor1g] = m_teamcolor1g;
			r.IntField[FI.teams_teamcolor1b] = m_teamcolor1b;
			r.IntField[FI.teams_teamcolor2r] = m_teamcolor2r;
			r.IntField[FI.teams_teamcolor2g] = m_teamcolor2g;
			r.IntField[FI.teams_teamcolor2b] = m_teamcolor2b;
			r.IntField[FI.teams_teamcolor3r] = m_teamcolor3r;
			r.IntField[FI.teams_teamcolor3g] = m_teamcolor3g;
			r.IntField[FI.teams_teamcolor3b] = m_teamcolor3b;
			r.IntField[FI.teams_form] = m_form;
			r.IntField[FI.teams_numtransfersin] = m_numtransfersin;
			r.IntField[FI.teams_genericint1] = m_genericint1;
			r.IntField[FI.teams_genericint2] = m_genericint2;
			r.IntField[FI.teams_rivalteam] = m_rivalteam;
			r.IntField[FI.teams_assetid] = m_assetid;
			r.IntField[FI.teams_transferbudget] = m_transferbudget;
			r.IntField[FI.teams_internationalprestige] = m_internationalprestige;
			r.IntField[FI.teams_domesticprestige] = m_domesticprestige;
			r.IntField[FI.teams_busbuildupspeed] = m_busbuildupspeed;
			r.IntField[FI.teams_buspassing] = m_buspassing;
			if (FifaEnvironment.Year == 14)
			{
				r.IntField[FI.teams_buspositioning] = m_buspositioning + 1;
				r.IntField[FI.teams_ccpositioning] = m_ccpositioning + 1;
				r.IntField[FI.teams_defdefenderline] = m_defdefenderline + 1;
			}
			else
			{
				r.IntField[FI.teams_buspositioning] = m_buspositioning;
				r.IntField[FI.teams_ccpositioning] = m_ccpositioning;
				r.IntField[FI.teams_defdefenderline] = m_defdefenderline;
			}
			r.IntField[FI.teams_busdribbling] = m_busdribbling;
			r.IntField[FI.teams_ccpassing] = m_ccpassing;
			r.IntField[FI.teams_cccrossing] = m_cccrossing;
			r.IntField[FI.teams_ccshooting] = m_ccshooting;
			r.IntField[FI.teams_defmentality] = m_defmentality;
			r.IntField[FI.teams_defaggression] = m_defaggression;
			r.IntField[FI.teams_defteamwidth] = m_defteamwidth;
			r.IntField[FI.teams_captainid] = m_captainid;
			r.IntField[FI.teams_penaltytakerid] = m_penaltytakerid;
			r.IntField[FI.teams_freekicktakerid] = m_freekicktakerid;
			if (FI.teams_leftfreekicktakerid >= 0)
			{
				r.IntField[FI.teams_leftfreekicktakerid] = m_leftfreekicktakerid;
			}
			if (FI.teams_rightfreekicktakerid >= 0)
			{
				r.IntField[FI.teams_rightfreekicktakerid] = m_rightfreekicktakerid;
			}
			r.IntField[FI.teams_longkicktakerid] = m_longkicktakerid;
			r.IntField[FI.teams_leftcornerkicktakerid] = m_leftcornerkicktakerid;
			r.IntField[FI.teams_rightcornerkicktakerid] = m_rightcornerkicktakerid;
			r.IntField[FI.teams_latitude] = m_latitude;
			r.IntField[FI.teams_longitude] = m_longitude;
			r.IntField[FI.teams_utcoffset] = m_utcoffset;
			_ = m_powid;
			r.IntField[FI.teams_powid] = m_powid;
			r.IntField[FI.teams_midfieldrating] = m_midfieldrating;
			r.IntField[FI.teams_defenserating] = m_defenserating;
			r.IntField[FI.teams_attackrating] = m_attackrating;
			r.IntField[FI.teams_overallrating] = m_overallrating;
			r.IntField[FI.teams_matchdayoverallrating] = m_matchdayoverallrating;
			r.IntField[FI.teams_matchdaydefenserating] = m_matchdaydefenserating;
			r.IntField[FI.teams_matchdaymidfieldrating] = m_matchdaymidfieldrating;
			r.IntField[FI.teams_matchdayattackrating] = m_matchdayattackrating;
			r.IntField[FI.teams_suitvariationid] = m_suitvariationid;
			r.IntField[FI.teams_suittypeid] = m_suittypeid;
			r.IntField[FI.teams_bodytypeid] = m_bodytypeid;
			r.IntField[FI.teams_ethnicity] = m_ethnicity;
			r.IntField[FI.teams_personalityid] = m_personalityid;
			m_trait1 = 0;
			m_trait1 |= (m_ImpatientBoard ? 1 : 0);
			m_trait1 |= (m_LoyalBoard ? 2 : 0);
			m_trait1 |= (m_SquadRotation ? 4 : 0);
			m_trait1 |= (m_ConsistentLineup ? 8 : 0);
			m_trait1 |= (m_SwitchWingers ? 16 : 0);
			m_trait1 |= (m_CenterBacksSplit ? 32 : 0);
			m_trait1 |= (m_DefendLead ? 64 : 0);
			m_trait1 |= (m_KeepUpPressure ? 128 : 0);
			m_trait1 |= (m_MoreAttackingAtHome ? 256 : 0);
			m_trait1 |= (m_ShortOutBack ? 512 : 0);
			r.IntField[FI.teams_trait1] = m_trait1;
		}

		public void SaveTeamStadiumLinks(Record r)
		{
			r.IntField[FI.teamstadiumlinks_teamid] = base.Id;
			r.IntField[FI.teamstadiumlinks_stadiumid] = m_stadiumid;
		}

		public void SaveTeamNationLinks(Record r)
		{
			r.IntField[FI.teamnationlinks_teamid] = base.Id;
			r.IntField[FI.teamnationlinks_nationid] = m_countryid_IfNationalTeam;
		}

		public void SaveDefaultTeamSheet(Record r)
		{
		}

		public void SaveDefaultTeamData(Record r)
		{
			if (r != null)
			{
				r.IntField[FI.defaultteamdata_teamid] = base.Id;
				r.IntField[FI.defaultteamdata_tacticid] = 0;
				r.IntField[FI.defaultteamdata_busbuildupspeed] = m_busbuildupspeed;
				r.IntField[FI.defaultteamdata_busdribbling] = m_busdribbling;
				r.IntField[FI.defaultteamdata_buspassing] = m_buspassing;
				r.IntField[FI.defaultteamdata_buspositioning] = m_buspositioning;
				r.IntField[FI.defaultteamdata_cccrossing] = m_cccrossing;
				r.IntField[FI.defaultteamdata_ccpassing] = m_ccpassing;
				r.IntField[FI.defaultteamdata_ccpositioning] = m_ccpositioning;
				r.IntField[FI.defaultteamdata_ccshooting] = m_ccshooting;
				r.IntField[FI.defaultteamdata_defaggression] = m_defaggression;
				r.IntField[FI.defaultteamdata_defdefenderline] = m_defdefenderline;
				r.IntField[FI.defaultteamdata_defmentality] = m_defmentality;
				r.IntField[FI.defaultteamdata_defteamwidth] = m_defteamwidth;
				if (m_Formation != null)
				{
					r.FloatField[FI.defaultteamdata_offset0x] = (float)m_Formation.PlayingRoles[0].OffsetX / 100f;
					r.FloatField[FI.defaultteamdata_offset0y] = (float)m_Formation.PlayingRoles[0].OffsetY / 100f;
					r.IntField[FI.defaultteamdata_playerinstruction0_1] = m_Formation.PlayingRoles[0].PlayerInstruction_1;
					r.IntField[FI.defaultteamdata_playerinstruction0_2] = m_Formation.PlayingRoles[0].PlayerInstruction_2;
					r.IntField[FI.defaultteamdata_position0] = m_Formation.PlayingRoles[0].Role.Id;
					r.FloatField[FI.defaultteamdata_offset1x] = (float)m_Formation.PlayingRoles[1].OffsetX / 100f;
					r.FloatField[FI.defaultteamdata_offset1y] = (float)m_Formation.PlayingRoles[1].OffsetY / 100f;
					r.IntField[FI.defaultteamdata_playerinstruction1_1] = m_Formation.PlayingRoles[1].PlayerInstruction_1;
					r.IntField[FI.defaultteamdata_playerinstruction1_2] = m_Formation.PlayingRoles[1].PlayerInstruction_2;
					r.IntField[FI.defaultteamdata_position1] = m_Formation.PlayingRoles[1].Role.Id;
					r.FloatField[FI.defaultteamdata_offset2x] = (float)m_Formation.PlayingRoles[2].OffsetX / 100f;
					r.FloatField[FI.defaultteamdata_offset2y] = (float)m_Formation.PlayingRoles[2].OffsetY / 100f;
					r.IntField[FI.defaultteamdata_playerinstruction2_1] = m_Formation.PlayingRoles[2].PlayerInstruction_1;
					r.IntField[FI.defaultteamdata_playerinstruction2_2] = m_Formation.PlayingRoles[2].PlayerInstruction_2;
					r.IntField[FI.defaultteamdata_position2] = m_Formation.PlayingRoles[2].Role.Id;
					r.FloatField[FI.defaultteamdata_offset3x] = (float)m_Formation.PlayingRoles[3].OffsetX / 100f;
					r.FloatField[FI.defaultteamdata_offset3y] = (float)m_Formation.PlayingRoles[3].OffsetY / 100f;
					r.IntField[FI.defaultteamdata_playerinstruction3_1] = m_Formation.PlayingRoles[3].PlayerInstruction_1;
					r.IntField[FI.defaultteamdata_playerinstruction3_2] = m_Formation.PlayingRoles[3].PlayerInstruction_2;
					r.IntField[FI.defaultteamdata_position3] = m_Formation.PlayingRoles[3].Role.Id;
					r.FloatField[FI.defaultteamdata_offset4x] = (float)m_Formation.PlayingRoles[4].OffsetX / 100f;
					r.FloatField[FI.defaultteamdata_offset4y] = (float)m_Formation.PlayingRoles[4].OffsetY / 100f;
					r.IntField[FI.defaultteamdata_playerinstruction4_1] = m_Formation.PlayingRoles[4].PlayerInstruction_1;
					r.IntField[FI.defaultteamdata_playerinstruction4_2] = m_Formation.PlayingRoles[4].PlayerInstruction_2;
					r.IntField[FI.defaultteamdata_position4] = m_Formation.PlayingRoles[4].Role.Id;
					r.FloatField[FI.defaultteamdata_offset5x] = (float)m_Formation.PlayingRoles[5].OffsetX / 100f;
					r.FloatField[FI.defaultteamdata_offset5y] = (float)m_Formation.PlayingRoles[5].OffsetY / 100f;
					r.IntField[FI.defaultteamdata_playerinstruction5_1] = m_Formation.PlayingRoles[5].PlayerInstruction_1;
					r.IntField[FI.defaultteamdata_playerinstruction5_2] = m_Formation.PlayingRoles[5].PlayerInstruction_2;
					r.IntField[FI.defaultteamdata_position5] = m_Formation.PlayingRoles[5].Role.Id;
					r.FloatField[FI.defaultteamdata_offset6x] = (float)m_Formation.PlayingRoles[6].OffsetX / 100f;
					r.FloatField[FI.defaultteamdata_offset6y] = (float)m_Formation.PlayingRoles[6].OffsetY / 100f;
					r.IntField[FI.defaultteamdata_playerinstruction6_1] = m_Formation.PlayingRoles[6].PlayerInstruction_1;
					r.IntField[FI.defaultteamdata_playerinstruction6_2] = m_Formation.PlayingRoles[6].PlayerInstruction_2;
					r.IntField[FI.defaultteamdata_position6] = m_Formation.PlayingRoles[6].Role.Id;
					r.FloatField[FI.defaultteamdata_offset7x] = (float)m_Formation.PlayingRoles[7].OffsetX / 100f;
					r.FloatField[FI.defaultteamdata_offset7y] = (float)m_Formation.PlayingRoles[7].OffsetY / 100f;
					r.IntField[FI.defaultteamdata_playerinstruction7_1] = m_Formation.PlayingRoles[7].PlayerInstruction_1;
					r.IntField[FI.defaultteamdata_playerinstruction7_2] = m_Formation.PlayingRoles[7].PlayerInstruction_2;
					r.IntField[FI.defaultteamdata_position7] = m_Formation.PlayingRoles[7].Role.Id;
					r.FloatField[FI.defaultteamdata_offset8x] = (float)m_Formation.PlayingRoles[8].OffsetX / 100f;
					r.FloatField[FI.defaultteamdata_offset8y] = (float)m_Formation.PlayingRoles[8].OffsetY / 100f;
					r.IntField[FI.defaultteamdata_playerinstruction8_1] = m_Formation.PlayingRoles[8].PlayerInstruction_1;
					r.IntField[FI.defaultteamdata_playerinstruction8_2] = m_Formation.PlayingRoles[8].PlayerInstruction_2;
					r.IntField[FI.defaultteamdata_position8] = m_Formation.PlayingRoles[8].Role.Id;
					r.FloatField[FI.defaultteamdata_offset9x] = (float)m_Formation.PlayingRoles[9].OffsetX / 100f;
					r.FloatField[FI.defaultteamdata_offset9y] = (float)m_Formation.PlayingRoles[9].OffsetY / 100f;
					r.IntField[FI.defaultteamdata_playerinstruction9_1] = m_Formation.PlayingRoles[9].PlayerInstruction_1;
					r.IntField[FI.defaultteamdata_playerinstruction9_2] = m_Formation.PlayingRoles[9].PlayerInstruction_2;
					r.IntField[FI.defaultteamdata_position9] = m_Formation.PlayingRoles[9].Role.Id;
					r.FloatField[FI.defaultteamdata_offset10x] = (float)m_Formation.PlayingRoles[10].OffsetX / 100f;
					r.FloatField[FI.defaultteamdata_offset10y] = (float)m_Formation.PlayingRoles[10].OffsetY / 100f;
					r.IntField[FI.defaultteamdata_playerinstruction10_1] = m_Formation.PlayingRoles[10].PlayerInstruction_1;
					r.IntField[FI.defaultteamdata_playerinstruction10_2] = m_Formation.PlayingRoles[10].PlayerInstruction_2;
					r.IntField[FI.defaultteamdata_position10] = m_Formation.PlayingRoles[10].Role.Id;
				}
			}
		}

		public void SaveDefaultTeamsheets(Record r)
		{
			if (r == null)
			{
				return;
			}
			r.IntField[FI.default_teamsheets_teamid] = base.Id;
			r.IntField[FI.default_teamsheets_tacticid] = 0;
			r.IntField[FI.default_teamsheets_busbuildupspeed] = m_busbuildupspeed;
			r.IntField[FI.default_teamsheets_busdribbling] = m_busdribbling;
			r.IntField[FI.default_teamsheets_buspassing] = m_buspassing;
			r.IntField[FI.default_teamsheets_buspositioning] = m_buspositioning;
			r.IntField[FI.default_teamsheets_cccrossing] = m_cccrossing;
			r.IntField[FI.default_teamsheets_ccpassing] = m_ccpassing;
			r.IntField[FI.default_teamsheets_ccpositioning] = m_ccpositioning;
			r.IntField[FI.default_teamsheets_ccshooting] = m_ccshooting;
			r.IntField[FI.default_teamsheets_defaggression] = m_defaggression;
			r.IntField[FI.default_teamsheets_defdefenderline] = m_defdefenderline;
			r.IntField[FI.default_teamsheets_defmentality] = m_defmentality;
			r.IntField[FI.default_teamsheets_defteamwidth] = m_defteamwidth;
			if (m_Formation != null)
			{
				r.FloatField[FI.default_teamsheets_offset0x] = (float)m_Formation.PlayingRoles[0].OffsetX / 100f;
				r.FloatField[FI.default_teamsheets_offset0y] = (float)m_Formation.PlayingRoles[0].OffsetY / 100f;
				r.IntField[FI.default_teamsheets_playerinstruction0_1] = m_Formation.PlayingRoles[0].PlayerInstruction_1;
				r.IntField[FI.default_teamsheets_position0] = m_Formation.PlayingRoles[0].Role.Id;
				r.FloatField[FI.default_teamsheets_offset1x] = (float)m_Formation.PlayingRoles[1].OffsetX / 100f;
				r.FloatField[FI.default_teamsheets_offset1y] = (float)m_Formation.PlayingRoles[1].OffsetY / 100f;
				r.IntField[FI.default_teamsheets_playerinstruction1_1] = m_Formation.PlayingRoles[1].PlayerInstruction_1;
				r.IntField[FI.default_teamsheets_position1] = m_Formation.PlayingRoles[1].Role.Id;
				r.FloatField[FI.default_teamsheets_offset2x] = (float)m_Formation.PlayingRoles[2].OffsetX / 100f;
				r.FloatField[FI.default_teamsheets_offset2y] = (float)m_Formation.PlayingRoles[2].OffsetY / 100f;
				r.IntField[FI.default_teamsheets_playerinstruction2_1] = m_Formation.PlayingRoles[2].PlayerInstruction_1;
				r.IntField[FI.default_teamsheets_position2] = m_Formation.PlayingRoles[2].Role.Id;
				r.FloatField[FI.default_teamsheets_offset3x] = (float)m_Formation.PlayingRoles[3].OffsetX / 100f;
				r.FloatField[FI.default_teamsheets_offset3y] = (float)m_Formation.PlayingRoles[3].OffsetY / 100f;
				r.IntField[FI.default_teamsheets_playerinstruction3_1] = m_Formation.PlayingRoles[3].PlayerInstruction_1;
				r.IntField[FI.default_teamsheets_position3] = m_Formation.PlayingRoles[3].Role.Id;
				r.FloatField[FI.default_teamsheets_offset4x] = (float)m_Formation.PlayingRoles[4].OffsetX / 100f;
				r.FloatField[FI.default_teamsheets_offset4y] = (float)m_Formation.PlayingRoles[4].OffsetY / 100f;
				r.IntField[FI.default_teamsheets_playerinstruction4_1] = m_Formation.PlayingRoles[4].PlayerInstruction_1;
				r.IntField[FI.default_teamsheets_position4] = m_Formation.PlayingRoles[4].Role.Id;
				r.FloatField[FI.default_teamsheets_offset5x] = (float)m_Formation.PlayingRoles[5].OffsetX / 100f;
				r.FloatField[FI.default_teamsheets_offset5y] = (float)m_Formation.PlayingRoles[5].OffsetY / 100f;
				r.IntField[FI.default_teamsheets_playerinstruction5_1] = m_Formation.PlayingRoles[5].PlayerInstruction_1;
				r.IntField[FI.default_teamsheets_position5] = m_Formation.PlayingRoles[5].Role.Id;
				r.FloatField[FI.default_teamsheets_offset6x] = (float)m_Formation.PlayingRoles[6].OffsetX / 100f;
				r.FloatField[FI.default_teamsheets_offset6y] = (float)m_Formation.PlayingRoles[6].OffsetY / 100f;
				r.IntField[FI.default_teamsheets_playerinstruction6_1] = m_Formation.PlayingRoles[6].PlayerInstruction_1;
				r.IntField[FI.default_teamsheets_position6] = m_Formation.PlayingRoles[6].Role.Id;
				r.FloatField[FI.default_teamsheets_offset7x] = (float)m_Formation.PlayingRoles[7].OffsetX / 100f;
				r.FloatField[FI.default_teamsheets_offset7y] = (float)m_Formation.PlayingRoles[7].OffsetY / 100f;
				r.IntField[FI.default_teamsheets_playerinstruction7_1] = m_Formation.PlayingRoles[7].PlayerInstruction_1;
				r.IntField[FI.default_teamsheets_position7] = m_Formation.PlayingRoles[7].Role.Id;
				r.FloatField[FI.default_teamsheets_offset8x] = (float)m_Formation.PlayingRoles[8].OffsetX / 100f;
				r.FloatField[FI.default_teamsheets_offset8y] = (float)m_Formation.PlayingRoles[8].OffsetY / 100f;
				r.IntField[FI.default_teamsheets_playerinstruction8_1] = m_Formation.PlayingRoles[8].PlayerInstruction_1;
				r.IntField[FI.default_teamsheets_position8] = m_Formation.PlayingRoles[8].Role.Id;
				r.FloatField[FI.default_teamsheets_offset9x] = (float)m_Formation.PlayingRoles[9].OffsetX / 100f;
				r.FloatField[FI.default_teamsheets_offset9y] = (float)m_Formation.PlayingRoles[9].OffsetY / 100f;
				r.IntField[FI.default_teamsheets_playerinstruction9_1] = m_Formation.PlayingRoles[9].PlayerInstruction_1;
				r.IntField[FI.default_teamsheets_position9] = m_Formation.PlayingRoles[9].Role.Id;
				r.FloatField[FI.default_teamsheets_offset10x] = (float)m_Formation.PlayingRoles[10].OffsetX / 100f;
				r.FloatField[FI.default_teamsheets_offset10y] = (float)m_Formation.PlayingRoles[10].OffsetY / 100f;
				r.IntField[FI.default_teamsheets_playerinstruction10_1] = m_Formation.PlayingRoles[10].PlayerInstruction_1;
				r.IntField[FI.default_teamsheets_position10] = m_Formation.PlayingRoles[10].Role.Id;
			}
			r.IntField[FI.default_teamsheets_captainid] = m_captainid;
			r.IntField[FI.default_teamsheets_freekicktakerid] = m_freekicktakerid;
			r.IntField[FI.default_teamsheets_leftcornerkicktakerid] = m_leftcornerkicktakerid;
			r.IntField[FI.default_teamsheets_leftfreekicktakerid] = m_leftfreekicktakerid;
			r.IntField[FI.default_teamsheets_longkicktakerid] = m_longkicktakerid;
			r.IntField[FI.default_teamsheets_penaltytakerid] = m_penaltytakerid;
			r.IntField[FI.default_teamsheets_rightcornerkicktakerid] = m_rightcornerkicktakerid;
			r.IntField[FI.default_teamsheets_rightfreekicktakerid] = m_rightfreekicktakerid;
			int[] array = new int[42]
			{
				FI.default_teamsheets_playerid0,
				FI.default_teamsheets_playerid1,
				FI.default_teamsheets_playerid2,
				FI.default_teamsheets_playerid3,
				FI.default_teamsheets_playerid4,
				FI.default_teamsheets_playerid5,
				FI.default_teamsheets_playerid6,
				FI.default_teamsheets_playerid7,
				FI.default_teamsheets_playerid8,
				FI.default_teamsheets_playerid9,
				FI.default_teamsheets_playerid10,
				FI.default_teamsheets_playerid11,
				FI.default_teamsheets_playerid12,
				FI.default_teamsheets_playerid13,
				FI.default_teamsheets_playerid14,
				FI.default_teamsheets_playerid15,
				FI.default_teamsheets_playerid16,
				FI.default_teamsheets_playerid17,
				FI.default_teamsheets_playerid18,
				FI.default_teamsheets_playerid19,
				FI.default_teamsheets_playerid20,
				FI.default_teamsheets_playerid21,
				FI.default_teamsheets_playerid22,
				FI.default_teamsheets_playerid23,
				FI.default_teamsheets_playerid24,
				FI.default_teamsheets_playerid25,
				FI.default_teamsheets_playerid26,
				FI.default_teamsheets_playerid27,
				FI.default_teamsheets_playerid28,
				FI.default_teamsheets_playerid29,
				FI.default_teamsheets_playerid30,
				FI.default_teamsheets_playerid31,
				FI.default_teamsheets_playerid32,
				FI.default_teamsheets_playerid33,
				FI.default_teamsheets_playerid34,
				FI.default_teamsheets_playerid35,
				FI.default_teamsheets_playerid36,
				FI.default_teamsheets_playerid37,
				FI.default_teamsheets_playerid38,
				FI.default_teamsheets_playerid39,
				FI.default_teamsheets_playerid40,
				FI.default_teamsheets_playerid41
			};
			for (int i = 0; i < array.Length; i++)
			{
				TeamPlayer teamPlayer = null;
				if (i < m_Roster.Count)
				{
					teamPlayer = (TeamPlayer)m_Roster[i];
				}
				r.IntField[array[i]] = ((teamPlayer != null && teamPlayer.Player != null) ? teamPlayer.Player.Id : (-1));
			}
		}

		public void SaveLangTable()
		{
			if (FifaEnvironment.Language != null)
			{
				FifaEnvironment.Language.SetTeamString(base.Id, Language.ETeamStringType.Full, m_TeamNameFull);
				FifaEnvironment.Language.SetTeamString(base.Id, Language.ETeamStringType.Abbr15, m_TeamNameAbbr15);
				FifaEnvironment.Language.SetTeamString(base.Id, Language.ETeamStringType.Abbr10, m_TeamNameAbbr10);
				FifaEnvironment.Language.SetTeamString(base.Id, Language.ETeamStringType.Abbr7, m_TeamNameAbbr7);
				FifaEnvironment.Language.SetTeamString(base.Id, Language.ETeamStringType.Abbr3, m_TeamNameAbbr3);
			}
		}

		public void SaveTeamCountry(Record r)
		{
		}

		public void SaveStadiumAssignment(Record r)
		{
			r.IntField[FI.stadiumassignments_teamid] = base.Id;
			r.StringField[FI.stadiumassignments_stadiumcustomname] = m_stadiumcustomname;
		}

		public void SaveManager(Record r)
		{
			r.IntField[FI.manager_teamid] = base.Id;
			r.StringField[FI.manager_firstname] = m_ManagerFirstName;
			r.StringField[FI.manager_surname] = m_ManagerSurname;
		}

		public bool HasNewsPictures()
		{
			return m_maxvariationsneg + m_maxvariationspos + m_maxvariationsstd != 0;
		}

		public void SaveNewspicweights(Record r)
		{
			r.IntField[FI.career_newspicweights_teamid] = base.Id;
			r.IntField[FI.career_newspicweights_maxvariationsneg] = m_maxvariationsneg;
			r.IntField[FI.career_newspicweights_maxvariationspos] = m_maxvariationspos;
			r.IntField[FI.career_newspicweights_maxvariationsstd] = m_maxvariationsstd;
			r.IntField[FI.career_newspicweights_crestweight] = m_crestweight;
			r.IntField[FI.career_newspicweights_genericweight] = m_genericweight;
			r.IntField[FI.career_newspicweights_teamweight] = m_teamweight;
		}

		public static void SaveDefaultNewspicweights(Record r)
		{
			r.IntField[FI.career_newspicweights_teamid] = 0;
			r.IntField[FI.career_newspicweights_maxvariationsneg] = 0;
			r.IntField[FI.career_newspicweights_maxvariationspos] = 0;
			r.IntField[FI.career_newspicweights_maxvariationsstd] = 30;
			r.IntField[FI.career_newspicweights_crestweight] = 40;
			r.IntField[FI.career_newspicweights_genericweight] = 60;
			r.IntField[FI.career_newspicweights_teamweight] = 0;
		}

		public void SaveTeamFormationLinks(Record r)
		{
			r.IntField[FI.teamformationteamstylelinks_teamid] = base.Id;
			r.IntField[FI.teamformationteamstylelinks_formationid] = m_formationid;
			r.IntField[FI.teamformationteamstylelinks_teamstyleid] = 0;
			r.IntField[FI.teamformationteamstylelinks_cddl] = 0;
		}

		public void SaveRowTeamNationLinks(Record r)
		{
			r.IntField[FI.rowteamnationlinks_teamid] = base.Id;
			if (m_Country != null)
			{
				r.IntField[FI.rowteamnationlinks_nationid] = m_Country.Id;
			}
		}

		public string CrestTemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/imgassets/crest/l#.dds";
			}
			return "data/ui/imgassets/crest/light/l#.dds";
		}

		public string CrestDdsFileName()
		{
			return CrestDdsFileName(base.Id);
		}

		public static string CrestDdsFileName(int id)
		{
			return CrestDdsFileName(id, FifaEnvironment.Year);
		}

		public static string CrestDdsFileName(int id, int year)
		{
			if (year == 14)
			{
				return "data/ui/imgassets/crest/l" + id.ToString() + ".dds";
			}
			return "data/ui/imgassets/crest/light/l" + id.ToString() + ".dds";
		}

		public string CrestTemplateFileNameDark()
		{
			return "data/ui/imgassets/crest/dark/l#.dds";
		}

		public string CrestDdsFileNameDark()
		{
			return "data/ui/imgassets/crest/dark/l" + base.Id.ToString() + ".dds";
		}

		public static string CrestDdsFileNameDark(int id)
		{
			return "data/ui/imgassets/crest/dark/l" + id.ToString() + ".dds";
		}

		public string Crest50TemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/imgassets/crest50x50/l#.dds";
			}
			return "data/ui/imgassets/crest50x50/light/l#.dds";
		}

		public static string Crest50DdsFileName(int id)
		{
			return Crest50DdsFileName(id, FifaEnvironment.Year);
		}

		public static string Crest50DdsFileName(int id, int year)
		{
			if (year == 14)
			{
				return "data/ui/imgassets/crest50x50/l" + id.ToString() + ".dds";
			}
			return "data/ui/imgassets/crest50x50/light/l" + id.ToString() + ".dds";
		}

		public string Crest50DdsFileName()
		{
			return Crest50DdsFileName(base.Id);
		}

		public string Crest50TemplateFileNameDark()
		{
			return "data/ui/imgassets/crest50x50/dark/l#.dds";
		}

		public string Crest50DdsFileNameDark()
		{
			return "data/ui/imgassets/crest50x50/dark/l" + base.Id.ToString() + ".dds";
		}

		public static string Crest50DdsFileNameDark(int id)
		{
			return "data/ui/imgassets/crest50x50/dark/l" + id.ToString() + ".dds";
		}

		public string Crest32TemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/imgassets/crest32x32/l#.dds";
			}
			return "data/ui/imgassets/crest32x32/light/l#.dds";
		}

		public string Crest32DdsFileName()
		{
			return Crest32DdsFileName(base.Id);
		}

		public static string Crest32DdsFileName(int id)
		{
			return Crest32DdsFileName(id, FifaEnvironment.Year);
		}

		public static string Crest32DdsFileName(int id, int year)
		{
			if (year == 14)
			{
				return "data/ui/imgassets/crest32x32/l" + id.ToString() + ".dds";
			}
			return "data/ui/imgassets/crest32x32/light/l" + id.ToString() + ".dds";
		}

		public string Crest32TemplateFileNameDark()
		{
			return "data/ui/imgassets/crest32x32/dark/l#.dds";
		}

		public string Crest32DdsFileNameDark()
		{
			return "data/ui/imgassets/crest32x32/dark/l" + base.Id.ToString() + ".dds";
		}

		public static string Crest32DdsFileNameDark(int id)
		{
			return "data/ui/imgassets/crest32x32/dark/l" + id.ToString() + ".dds";
		}

		public string Crest16TemplateFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/imgassets/crest16x16/l#.dds";
			}
			return "data/ui/imgassets/crest16x16/light/l#.dds";
		}

		public string Crest16DdsFileName()
		{
			return Crest16DdsFileName(base.Id);
		}

		public static string Crest16DdsFileName(int id)
		{
			return Crest16DdsFileName(id, FifaEnvironment.Year);
		}

		public static string Crest16DdsFileName(int id, int year)
		{
			if (year == 14)
			{
				return "data/ui/imgassets/crest16x16/l" + id.ToString() + ".dds";
			}
			return "data/ui/imgassets/crest16x16/light/l" + id.ToString() + ".dds";
		}

		public string Crest16TemplateFileNameDark()
		{
			return "data/ui/imgassets/crest16x16/dark/l#.dds";
		}

		public string Crest16DdsFileNameDark()
		{
			return "data/ui/imgassets/crest16x16/dark/l" + base.Id.ToString() + ".dds";
		}

		public static string Crest16DdsFileNameDark(int id)
		{
			return "data/ui/imgassets/crest16x16/dark/l" + id.ToString() + ".dds";
		}

		public Bitmap GetCrest()
		{
			return FifaEnvironment.GetDdsArtasset(CrestDdsFileName());
		}

		public bool SetCrest(Bitmap bitmap)
		{
			return FifaEnvironment.SetDdsArtasset(CrestTemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteCrest()
		{
			return FifaEnvironment.DeleteFromZdata(CrestDdsFileName());
		}

		public Bitmap GetCrest50()
		{
			return FifaEnvironment.GetDdsArtasset(Crest50DdsFileName());
		}

		public bool SetCrest50(Bitmap bitmap)
		{
			return FifaEnvironment.SetDdsArtasset(Crest50TemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteCrest50()
		{
			return FifaEnvironment.DeleteFromZdata(Crest50DdsFileName());
		}

		public Bitmap GetCrest32()
		{
			return FifaEnvironment.GetDdsArtasset(Crest32DdsFileName());
		}

		public bool SetCrest32(Bitmap bitmap)
		{
			return FifaEnvironment.SetDdsArtasset(Crest32TemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteCrest32()
		{
			return FifaEnvironment.DeleteFromZdata(Crest32DdsFileName());
		}

		public Bitmap GetCrest16()
		{
			return FifaEnvironment.GetDdsArtasset(Crest16DdsFileName());
		}

		public bool SetCrest16(Bitmap bitmap)
		{
			return FifaEnvironment.SetDdsArtasset(Crest16TemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteCrest16()
		{
			return FifaEnvironment.DeleteFromZdata(Crest16DdsFileName());
		}

		public Bitmap GetCrestDark()
		{
			return FifaEnvironment.GetDdsArtasset(CrestDdsFileNameDark());
		}

		public bool SetCrestDark(Bitmap bitmap)
		{
			return FifaEnvironment.SetDdsArtasset(CrestTemplateFileNameDark(), base.Id, bitmap);
		}

		public bool DeleteCrestDark()
		{
			return FifaEnvironment.DeleteFromZdata(CrestDdsFileNameDark());
		}

		public Bitmap GetCrest50Dark()
		{
			return FifaEnvironment.GetDdsArtasset(Crest50DdsFileNameDark());
		}

		public bool SetCrest50Dark(Bitmap bitmap)
		{
			return FifaEnvironment.SetDdsArtasset(Crest50TemplateFileNameDark(), base.Id, bitmap);
		}

		public bool DeleteCrest50Dark()
		{
			return FifaEnvironment.DeleteFromZdata(Crest50DdsFileNameDark());
		}

		public Bitmap GetCrest32Dark()
		{
			return FifaEnvironment.GetDdsArtasset(Crest32DdsFileNameDark());
		}

		public bool SetCrest32Dark(Bitmap bitmap)
		{
			return FifaEnvironment.SetDdsArtasset(Crest32TemplateFileNameDark(), base.Id, bitmap);
		}

		public bool DeleteCrest32Dark()
		{
			return FifaEnvironment.DeleteFromZdata(Crest32DdsFileNameDark());
		}

		public Bitmap GetCrest16Dark()
		{
			return FifaEnvironment.GetDdsArtasset(Crest16DdsFileNameDark());
		}

		public bool SetCrest16Dark(Bitmap bitmap)
		{
			return FifaEnvironment.SetDdsArtasset(Crest16TemplateFileNameDark(), base.Id, bitmap);
		}

		public bool DeleteCrest16Dark()
		{
			return FifaEnvironment.DeleteFromZdata(Crest16DdsFileNameDark());
		}

		public bool SetAllCrests(Bitmap crestLogo)
		{
			if (crestLogo.Width != 256 || crestLogo.Width != 256)
			{
				return false;
			}
			SetCrest(crestLogo);
			SetCrestDark(crestLogo);
			Rectangle srcRect = new Rectangle(0, 0, 256, 256);
			Bitmap bitmap = new Bitmap(64, 64, PixelFormat.Format32bppPArgb);
			Bitmap bitmap2 = new Bitmap(32, 32, PixelFormat.Format32bppPArgb);
			Bitmap bitmap3 = new Bitmap(16, 16, PixelFormat.Format32bppPArgb);
			Rectangle destRect = new Rectangle(0, 0, 50, 50);
			Rectangle destRect2 = new Rectangle(0, 0, 32, 32);
			Rectangle destRect3 = new Rectangle(0, 0, 16, 16);
			GraphicUtil.RemapRectangle(crestLogo, srcRect, bitmap, destRect);
			GraphicUtil.RemapRectangle(crestLogo, srcRect, bitmap2, destRect2);
			GraphicUtil.RemapRectangle(crestLogo, srcRect, bitmap3, destRect3);
			SetCrest16(bitmap3);
			SetCrest16Dark(bitmap3);
			SetCrest32(bitmap2);
			SetCrest32Dark(bitmap2);
			SetCrest50(bitmap);
			SetCrest50Dark(bitmap);
			return true;
		}

		public string BannerFileName()
		{
			return "data/sceneassets/banner/banner_" + base.Id.ToString() + ".rx3";
		}

		public static string BannerFileName(int id)
		{
			return "data/sceneassets/banner/banner_" + id.ToString() + ".rx3";
		}

		public string BannerTemplateFileName()
		{
			return "data/sceneassets/banner/banner_#.rx3";
		}

		public Bitmap GetBanner()
		{
			return FifaEnvironment.GetBmpFromRx3(BannerFileName());
		}

		public bool SetBanner(Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(BannerTemplateFileName(), base.Id, bitmap, ECompressionMode.Chunkzip);
		}

		public bool SetBanner(string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, BannerFileName(), delete: false, ECompressionMode.Chunkzip);
		}

		public bool DeleteBanner()
		{
			return FifaEnvironment.DeleteFromZdata(BannerFileName());
		}

		public static string GenericFlagFileName(int style)
		{
			return "data/sceneassets/flag/gf" + style.ToString() + ".png";
		}

		public string FlagFileName()
		{
			return "data/sceneassets/flag/flag_" + base.Id.ToString() + ".rx3";
		}

		public static string FlagFileName(int id)
		{
			return "data/sceneassets/flag/flag_" + id.ToString() + ".rx3";
		}

		public string FlagTemplateFileName()
		{
			return "data/sceneassets/flag/flag_#.rx3";
		}

		private Rx3Signatures FlagsSignature()
		{
			return new Rx3Signatures(350544, 22, new string[4]
			{
				"flag_" + base.Id.ToString() + "_0",
				"flag_" + base.Id.ToString() + "_1",
				"flag_" + base.Id.ToString() + "_2",
				"flag_" + base.Id.ToString() + "_3"
			});
		}

		public Bitmap[] GetFlags()
		{
			return FifaEnvironment.GetBmpsFromRx3(FlagFileName());
		}

		public bool SetFlags(Bitmap[] bitmaps)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(FlagTemplateFileName(), base.Id, bitmaps, ECompressionMode.Chunkzip, FlagsSignature());
		}

		public bool DeleteFlag()
		{
			return FifaEnvironment.DeleteFromZdata(FlagFileName());
		}

		public bool SetFlags(string rx3FileName)
		{
			string archivedName = FlagFileName();
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, archivedName, delete: false, ECompressionMode.Chunkzip);
		}

		public bool ExportFlags(string exportFolder)
		{
			return FifaEnvironment.ExportFileFromZdata(FlagFileName(), exportFolder);
		}

		public string ScarfFileName()
		{
			return "data/sceneassets/flag/scarf_" + base.Id.ToString() + ".rx3";
		}

		public static string ScarfFileName(int id)
		{
			return "data/sceneassets/flag/scarf_" + id.ToString() + ".rx3";
		}

		public string ScarfTemplateFileName()
		{
			return "data/sceneassets/flag/scarf_#.rx3";
		}

		private Rx3Signatures ScarfsSignature()
		{
			return new Rx3Signatures(350544, 22, new string[4]
			{
				"flag_" + base.Id.ToString() + "_0",
				"flag_" + base.Id.ToString() + "_1",
				"flag_" + base.Id.ToString() + "_2",
				"flag_" + base.Id.ToString() + "_3"
			});
		}

		public Bitmap[] GetScarfs()
		{
			return FifaEnvironment.GetBmpsFromRx3(ScarfFileName(), verbose: false);
		}

		public bool SetScarfs(Bitmap[] bitmaps)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(ScarfTemplateFileName(), base.Id, bitmaps, ECompressionMode.None, ScarfsSignature());
		}

		public bool DeleteScarf()
		{
			return FifaEnvironment.DeleteFromZdata(ScarfFileName());
		}

		public bool SetScarfs(string rx3FileName)
		{
			string archivedName = ScarfFileName();
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, archivedName, delete: false, ECompressionMode.None);
		}

		public bool ExportScarfs(string exportFolder)
		{
			return FifaEnvironment.ExportFileFromZdata(ScarfFileName(), exportFolder);
		}

		public void RemoveTeamPlayer(TeamPlayer teamPlayer)
		{
			m_Roster.Remove(teamPlayer);
			teamPlayer.Player.NotPlayFor(this);
			if (teamPlayer.position < 28)
			{
				AssignRoleToSubstitute((ERole)teamPlayer.position);
				AssignBench();
			}
			else if (teamPlayer.position == 28)
			{
				AssignBench();
			}
			if (PlayerCaptain == teamPlayer.Player)
			{
				AssignCaptain();
			}
			if (PlayerLeftFreeKick == teamPlayer.Player || PlayerRightFreeKick == teamPlayer.Player || PlayerFreeKick == teamPlayer.Player)
			{
				AssignFreeKick();
			}
			if (PlayerPenalty == teamPlayer.Player)
			{
				AssignPenalty();
			}
			if (PlayerLeftCorner == teamPlayer.Player)
			{
				AssignLeftCorner();
			}
			if (PlayerRightCorner == teamPlayer.Player)
			{
				AssignRightCorner();
			}
		}

		public void RemoveTeamPlayer(Player player)
		{
			TeamPlayer teamPlayer = m_Roster.SearchTeamPlayer(player);
			if (teamPlayer != null)
			{
				RemoveTeamPlayer(teamPlayer);
			}
		}

		public void AssignRoles()
		{
			AssignRoles(m_Formation);
		}

		public void AssignRoles(Formation formation)
		{
			Roster roster = new Roster(32);
			ERole[] array = new ERole[11];
			int num = 11;
			for (int i = 0; i < 11; i++)
			{
				array[i] = formation.PlayingRoles[i].Role.RoleId;
			}
			for (int j = 0; j < 7; j++)
			{
				TeamPlayer bestPlayer = m_Roster.GetBestPlayer();
				if (bestPlayer == null)
				{
					break;
				}
				ERole eRole = (ERole)(bestPlayer.position = (int)bestPlayer.Player.ChooseRole(array, num));
				roster.Add(bestPlayer);
				m_Roster.Remove(bestPlayer);
				for (int k = 0; k < num; k++)
				{
					if (eRole == array[k])
					{
						for (int l = k; l < num - 1; l++)
						{
							array[l] = array[l + 1];
						}
						num--;
					}
				}
			}
			for (int m = 0; m < num; m++)
			{
				ERole eRole2 = array[m];
				TeamPlayer bestPlayer = m_Roster.GetRoleBestPlayer(eRole2);
				if (bestPlayer == null)
				{
					break;
				}
				bestPlayer.position = (int)eRole2;
				roster.Add(bestPlayer);
				m_Roster.Remove(bestPlayer);
			}
			for (int n = 0; n < 7; n++)
			{
				TeamPlayer bestPlayer = m_Roster.GetBestPlayer();
				if (bestPlayer == null)
				{
					break;
				}
				bestPlayer.position = 28;
				roster.Add(bestPlayer);
				m_Roster.Remove(bestPlayer);
			}
			for (int num3 = 0; num3 < 14; num3++)
			{
				TeamPlayer bestPlayer = m_Roster.GetBestPlayer();
				if (bestPlayer == null)
				{
					break;
				}
				bestPlayer.position = 29;
				roster.Add(bestPlayer);
				m_Roster.Remove(bestPlayer);
			}
			foreach (TeamPlayer item in roster)
			{
				m_Roster.Add(item);
			}
		}

		public void AssignTitolarToRoles(Formation formation)
		{
			Roster roster = new Roster(32);
			ERole[] array = new ERole[11];
			int num = 11;
			for (int i = 0; i < 11; i++)
			{
				array[i] = formation.PlayingRoles[i].Role.RoleId;
			}
			for (int j = 0; j < 11; j++)
			{
				TeamPlayer bestTitolar = m_Roster.GetBestTitolar();
				if (bestTitolar == null)
				{
					break;
				}
				ERole eRole = (ERole)(bestTitolar.position = (int)bestTitolar.Player.ChooseRole(array, num));
				roster.Add(bestTitolar);
				m_Roster.Remove(bestTitolar);
				for (int k = 0; k < num; k++)
				{
					if (eRole == array[k])
					{
						for (int l = k; l < num - 1; l++)
						{
							array[l] = array[l + 1];
						}
						num--;
					}
				}
			}
			foreach (TeamPlayer item in roster)
			{
				m_Roster.Add(item);
			}
		}

		public void AssignRoleToSubstitute(ERole role)
		{
			int num = -1;
			TeamPlayer teamPlayer = null;
			foreach (TeamPlayer item in m_Roster)
			{
				if (item.position >= 28)
				{
					int rolePerformance = item.Player.GetRolePerformance(role);
					if (rolePerformance > num)
					{
						num = rolePerformance;
						teamPlayer = item;
					}
				}
			}
			if (teamPlayer != null)
			{
				teamPlayer.position = (int)role;
			}
		}

		public void AssignVacantRolesToSubstitute()
		{
			for (int i = 0; i < 11; i++)
			{
				PlayingRole playingRole = m_Formation.PlayingRoles[i];
				if (Roster.SearchTeamPlayer(playingRole.Role) == null)
				{
					AssignRoleToSubstitute(playingRole.Role.RoleId);
				}
			}
		}

		public void AssignBench()
		{
			int num = 0;
			foreach (TeamPlayer item in m_Roster)
			{
				if (item.position == 28)
				{
					num++;
				}
			}
			for (int i = num; i < 7; i++)
			{
				foreach (TeamPlayer item2 in m_Roster)
				{
					if (item2.position == 29)
					{
						item2.position = 28;
						break;
					}
				}
			}
		}

		public void AssignVacantSpecialPlayers()
		{
			if (m_Roster.SearchPlayer(m_captainid) == null)
			{
				AssignCaptain();
			}
			if (m_Roster.SearchPlayer(m_penaltytakerid) == null)
			{
				AssignPenalty();
			}
			if (m_Roster.SearchPlayer(m_leftfreekicktakerid) == null)
			{
				AssignLeftFreeKick();
			}
			if (m_Roster.SearchPlayer(m_rightfreekicktakerid) == null)
			{
				AssignRightFreeKick();
			}
			if (m_Roster.SearchPlayer(m_longkicktakerid) == null)
			{
				AssignLongFreeKick();
			}
			if (m_Roster.SearchPlayer(m_leftcornerkicktakerid) == null)
			{
				AssignLeftCorner();
			}
			if (m_Roster.SearchPlayer(m_rightcornerkicktakerid) == null)
			{
				AssignRightCorner();
			}
		}

		public void AssignCaptain()
		{
			int num = -1;
			TeamPlayer teamPlayer = null;
			foreach (TeamPlayer item in m_Roster)
			{
				if (item.position < 28)
				{
					int num2 = item.Player.ComputeMeanAttributes(5);
					if (num2 >= num)
					{
						teamPlayer = item;
						num = num2;
					}
				}
			}
			if (teamPlayer != null)
			{
				PlayerCaptain = teamPlayer.Player;
				m_captainid = PlayerCaptain.Id;
			}
		}

		public void AssignPenalty()
		{
			int num = -1;
			TeamPlayer teamPlayer = null;
			foreach (TeamPlayer item in m_Roster)
			{
				if (item.position < 28)
				{
					int num2 = item.Player.ComputeMeanAttributes(3);
					if (num2 >= num)
					{
						teamPlayer = item;
						num = num2;
					}
				}
			}
			if (teamPlayer != null)
			{
				PlayerPenalty = teamPlayer.Player;
				if (PlayerPenalty != null)
				{
					m_penaltytakerid = PlayerPenalty.Id;
				}
			}
		}

		public void AssignFreeKick()
		{
			AssignLeftFreeKick();
			AssignRightFreeKick();
			AssignLongFreeKick();
			m_freekicktakerid = m_longkicktakerid;
		}

		public void AssignRightFreeKick()
		{
			int num = -1;
			TeamPlayer teamPlayer = null;
			foreach (TeamPlayer item in m_Roster)
			{
				if (item.position < 28)
				{
					int num2 = item.Player.freekickaccuracy + item.Player.curve + ((item.Player.preferredfoot == 1) ? 5 : 0);
					if (num2 >= num)
					{
						teamPlayer = item;
						num = num2;
					}
				}
			}
			if (teamPlayer != null)
			{
				PlayerRightFreeKick = teamPlayer.Player;
				m_rightfreekicktakerid = PlayerRightFreeKick.Id;
			}
		}

		public void AssignLeftFreeKick()
		{
			int num = -1;
			TeamPlayer teamPlayer = null;
			foreach (TeamPlayer item in m_Roster)
			{
				if (item.position < 28)
				{
					int num2 = item.Player.freekickaccuracy + item.Player.curve + ((item.Player.preferredfoot == 0) ? 5 : 0);
					if (num2 >= num)
					{
						teamPlayer = item;
						num = num2;
					}
				}
			}
			if (teamPlayer != null)
			{
				PlayerLeftFreeKick = teamPlayer.Player;
				m_leftfreekicktakerid = PlayerLeftFreeKick.Id;
			}
		}

		public void AssignLongFreeKick()
		{
			int num = -1;
			TeamPlayer teamPlayer = null;
			foreach (TeamPlayer item in m_Roster)
			{
				if (item.position < 28)
				{
					int num2 = item.Player.freekickaccuracy + item.Player.longshots;
					if (num2 >= num)
					{
						teamPlayer = item;
						num = num2;
					}
				}
			}
			if (teamPlayer != null)
			{
				PlayerLongKick = teamPlayer.Player;
				m_longkicktakerid = PlayerLongKick.Id;
			}
		}

		public void AssignLeftCorner()
		{
			int num = -1;
			TeamPlayer teamPlayer = null;
			foreach (TeamPlayer item in m_Roster)
			{
				if (item.position < 28)
				{
					int crossing = item.Player.crossing;
					if (crossing >= num)
					{
						teamPlayer = item;
						num = crossing;
					}
				}
			}
			if (teamPlayer != null)
			{
				PlayerLeftCorner = teamPlayer.Player;
				m_leftcornerkicktakerid = PlayerLeftCorner.Id;
			}
		}

		public void AssignRightCorner()
		{
			int num = -1;
			TeamPlayer teamPlayer = null;
			foreach (TeamPlayer item in m_Roster)
			{
				if (item.position < 28)
				{
					int crossing = item.Player.crossing;
					if (crossing >= num)
					{
						teamPlayer = item;
						num = crossing;
					}
				}
			}
			if (teamPlayer != null)
			{
				PlayerRightCorner = teamPlayer.Player;
				m_rightcornerkicktakerid = PlayerRightCorner.Id;
			}
		}

		public void AddTeamPlayer(TeamPlayer teamPlayer)
		{
			teamPlayer.position = 29;
			teamPlayer.m_jerseynumber = m_Roster.GetFreeNumber();
			teamPlayer.Team = this;
			m_Roster.Add(teamPlayer);
			teamPlayer.Player.PlayFor(this);
		}

		public TeamPlayer AddTeamPlayer(Player player, int jerseyNumber)
		{
			TeamPlayer teamPlayer = AddTeamPlayer(player);
			if (jerseyNumber != 0)
			{
				teamPlayer.jerseynumber = jerseyNumber;
			}
			return teamPlayer;
		}

		public TeamPlayer AddTeamPlayer(Player player)
		{
			TeamPlayer teamPlayer = new TeamPlayer(player);
			AddTeamPlayer(teamPlayer);
			return teamPlayer;
		}

		public void FixPlayersWithTwoClubs(bool keppInThisTeam)
		{
			if (!IsClub())
			{
				return;
			}
			for (int i = 0; i < Roster.Count; i++)
			{
				Player player = ((TeamPlayer)Roster[i]).Player;
				bool flag = false;
				foreach (Team playingForTeam in player.m_PlayingForTeams)
				{
					if (playingForTeam.IsClub() && playingForTeam != this)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				for (int j = 0; j < player.m_PlayingForTeams.Count; j++)
				{
					Team team2 = (Team)player.m_PlayingForTeams[j];
					if (team2.IsClub() && team2 != this)
					{
						if (keppInThisTeam)
						{
							team2.RemoveTeamPlayer(player);
							break;
						}
						RemoveTeamPlayer(player);
						i--;
					}
				}
			}
		}

		public Kit GetKit(int kitType)
		{
			return m_KitList.GetKit(base.Id, kitType);
		}
	}
}
