using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary
{
	public class FifaEnvironment
	{
		private static ToolStripStatusLabel m_Status = null;

		private static FifaFat m_FifaFat;

		private static CareerFile m_CareerFile;

		private static string m_CareerFileName;

		private static CareerFile m_TournamentFile;

		private static string m_TournamentFileName;

		private static CareerFile m_MyTeamsFile;

		private static string m_MyTeamsFileName;

		private static DbFile m_OriginalFifaDb;

		private static bool m_IsRevModInstalled;

		private static DbFile m_FifaDb;

		private static string m_FifaDbFileName;

		private static string m_FifaXmlFileName;

		private static string m_FifaDbPartialFileName;

		private static string m_FifaXmlPartialFileName;

		private static int m_Year;

		private static string m_GameKey = null;

		private static DbFile m_LangDb;

		private static string m_LangDbFileName;

		private static string m_LangXmlFileName;

		private static Language m_Language = null;

		private static string m_RootDir;

		private static string m_GameDir;

		private static string m_ExportFolder;

		private static string m_TempFolder;

		private static string m_LaunchDir;

		private static UserMessage m_UserMessages;

		private static UserOptions m_UserOptions;

		private static bool m_NeedToSaveMiniHeads = false;

		private static CountryList s_CountryList = null;

		private static LeagueList s_LeagueList = null;

		private static TeamList s_TeamList = null;

		private static FreeAgentList s_FreeAgentList = null;

		private static PlayerList s_PlayerList = null;

		private static PlayerNames s_OriginalPlayerNamesList = null;

		private static PlayerNames s_PlayerNamesList = null;

		private static NameDictionary s_NameDictionary = null;

		private static CareerFirstNameList s_CareerFirstNameList = null;

		private static CareerLastNameList s_CareerLastNameList = null;

		private static CareerCommonNameList s_CareerCommonNameList = null;

		private static StadiumList s_StadiumList = null;

		private static RefereeList s_RefereeList = null;

		private static KitList s_KitList = null;

		private static CompobjList s_CompetitionObjects = null;

		private static FormationList s_FormationList = null;

		private static FormationList s_GenericFormationList = null;

		private static RoleList s_RoleList = null;

		private static BallList s_BallList = null;

		private static AdboardList s_AdboardList = null;

		private static ShoesList s_ShoesList = null;

		private static GkGlovesList s_GkGlovesList = null;

		private static NetList s_NetList = null;

		private static MowingPatternList s_MowingPatternList = null;

		private static NumberFontList s_NumberFontList = null;

		private static NameFontList s_NameFontList = null;

		private static string[] s_RevModFileNames = new string[17]
		{
			"data/fifarna/lua/assets.lua",
			"data/fifarna/lua/assets/accessory.lua",
			"data/fifarna/lua/assets/ball.lua",
			"data/fifarna/lua/assets/cornerflags.lua",
			"data/fifarna/lua/assets/crowd.lua",
			"data/fifarna/lua/assets/fancards.lua",
			"data/fifarna/lua/assets/goalnet.lua",
			"data/fifarna/lua/assets/grass.lua",
			"data/fifarna/lua/assets/player.lua",
			"data/fifarna/lua/assets/rm_common.lua",
			"data/fifarna/lua/assets/sle.lua",
			"data/fifarna/lua/assets/stadium.lua",
			"data/fifarna/lua/assets/trophy.lua",
			"data/fifarna/lua/assets/wipe3D.lua",
			"data/fifarna/lua/assignments/general.lua",
			"data/sceneassets/kit/kit_7000_93_0.rx3",
			"data/sceneassets/kit/kit_7000_94_0.rx3"
		};

		public static FifaFat FifaFat => m_FifaFat;

		public static CareerFile CareerFile => m_CareerFile;

		public static string CareerFileName
		{
			get
			{
				return m_CareerFileName;
			}
			set
			{
				m_CareerFileName = value;
			}
		}

		public static CareerFile TournamentFile => m_TournamentFile;

		public static string TournamentFileName
		{
			get
			{
				return m_TournamentFileName;
			}
			set
			{
				m_TournamentFileName = value;
			}
		}

		public static CareerFile MyTeamsFile => m_MyTeamsFile;

		public static string MyTeamsFileName
		{
			get
			{
				return m_MyTeamsFileName;
			}
			set
			{
				m_MyTeamsFileName = value;
			}
		}

		public static DbFile OriginalFifaDb => m_OriginalFifaDb;

		public static bool IsRevModInstalled
		{
			get
			{
				return m_IsRevModInstalled;
			}
			set
			{
				m_IsRevModInstalled = value;
			}
		}

		public static DbFile FifaDb => m_FifaDb;

		public static string FifaDbFileName
		{
			get
			{
				return m_FifaDbFileName;
			}
			set
			{
				m_FifaDbFileName = value;
			}
		}

		public static string FifaXmlFileName
		{
			get
			{
				return m_FifaXmlFileName;
			}
			set
			{
				m_FifaXmlFileName = value;
			}
		}

		public static string FifaDbPartialFileName
		{
			get
			{
				return m_FifaDbPartialFileName;
			}
			set
			{
				m_FifaDbPartialFileName = value;
			}
		}

		public static string FifaXmlPartialFileName
		{
			get
			{
				return m_FifaXmlPartialFileName;
			}
			set
			{
				m_FifaXmlPartialFileName = value;
			}
		}

		public static int Year
		{
			get
			{
				return m_Year;
			}
			set
			{
				m_Year = value;
			}
		}

		public static DbFile LangDb => m_LangDb;

		public static string LangDbFileName
		{
			get
			{
				return m_LangDbFileName;
			}
			set
			{
				m_LangDbFileName = value;
			}
		}

		public static string LangXmlFileName
		{
			get
			{
				return m_LangXmlFileName;
			}
			set
			{
				m_LangXmlFileName = value;
			}
		}

		public static Language Language => m_Language;

		public static string RootDir => m_RootDir;

		public static string GameDir => m_GameDir;

		public static string ExportFolder => m_ExportFolder;

		public static string TempFolder => m_TempFolder;

		public static string LaunchDir => m_LaunchDir;

		public static UserMessage UserMessages => m_UserMessages;

		public static UserOptions UserOptions => m_UserOptions;

		public static CountryList Countries => s_CountryList;

		public static LeagueList Leagues => s_LeagueList;

		public static TeamList Teams => s_TeamList;

		public static FreeAgentList FreeAgents => s_FreeAgentList;

		public static PlayerList Players => s_PlayerList;

		public static PlayerNames OriginalPlayerNamesList => s_OriginalPlayerNamesList;

		public static PlayerNames PlayerNamesList => s_PlayerNamesList;

		public static NameDictionary NameDictionary => s_NameDictionary;

		public static CareerFirstNameList CareerFirstNameList => s_CareerFirstNameList;

		public static CareerLastNameList CareerLastNameList => s_CareerLastNameList;

		public static CareerCommonNameList CareerCommonNameList => s_CareerCommonNameList;

		public static StadiumList Stadiums => s_StadiumList;

		public static RefereeList Referees => s_RefereeList;

		public static KitList Kits => s_KitList;

		public static CompobjList CompetitionObjects => s_CompetitionObjects;

		public static FormationList Formations => s_FormationList;

		public static FormationList GenericFormations => s_GenericFormationList;

		public static RoleList Roles => s_RoleList;

		public static BallList Balls
		{
			get
			{
				if (s_BallList == null)
				{
					s_BallList = new BallList(m_FifaDb, m_FifaFat);
				}
				return s_BallList;
			}
		}

		public static AdboardList Adboards
		{
			get
			{
				if (s_AdboardList == null)
				{
					s_AdboardList = new AdboardList(m_FifaDb, m_FifaFat);
				}
				return s_AdboardList;
			}
		}

		public static ShoesList Shoes
		{
			get
			{
				if (s_ShoesList == null)
				{
					s_ShoesList = new ShoesList(m_FifaDb, m_FifaFat);
				}
				return s_ShoesList;
			}
		}

		public static GkGlovesList GkGloves
		{
			get
			{
				if (s_GkGlovesList == null)
				{
					s_GkGlovesList = new GkGlovesList(m_FifaDb, m_FifaFat);
				}
				return s_GkGlovesList;
			}
		}

		public static NetList Nets
		{
			get
			{
				if (s_NetList == null)
				{
					s_NetList = new NetList(m_FifaDb, m_FifaFat);
				}
				return s_NetList;
			}
		}

		public static MowingPatternList MowingPatterns
		{
			get
			{
				if (s_MowingPatternList == null)
				{
					s_MowingPatternList = new MowingPatternList(m_FifaDb, m_FifaFat);
				}
				return s_MowingPatternList;
			}
		}

		public static NumberFontList NumberFonts
		{
			get
			{
				if (s_NumberFontList == null)
				{
					s_NumberFontList = new NumberFontList(m_FifaDb, m_FifaFat);
				}
				return s_NumberFontList;
			}
		}

		public static NameFontList NameFonts
		{
			get
			{
				if (s_NameFontList == null)
				{
					s_NameFontList = new NameFontList(m_FifaDb, m_FifaFat);
				}
				return s_NameFontList;
			}
		}

		public static void InitializeLaunchFolder()
		{
			int num = Environment.CommandLine.IndexOf(".exe");
			for (int num2 = num; num2 >= 0; num2--)
			{
				if (Environment.CommandLine[num2] == '\\')
				{
					num = num2;
					break;
				}
			}
			if (num >= 0)
			{
				m_LaunchDir = Environment.CommandLine.Substring(1, num - 1);
			}
		}

		public static bool InitializeDefault()
		{
			InitializeLaunchFolder();
			return true;
		}

		private static bool Initialize14(string rootDir)
		{
			if (m_UserMessages == null)
			{
				m_UserMessages = new UserMessage();
			}
			if (m_UserOptions == null)
			{
				m_UserOptions = new UserOptions();
			}
			m_FifaFat = null;
			m_FifaDb = null;
			m_LangDb = null;
			InitializeLaunchFolder();
			m_Year = 14;
			m_GameKey = RegistryInfo.GetFifaKey(m_Year);
			if (rootDir == null)
			{
				if (!RegistryInfo.IsFifaInstalled(m_GameKey))
				{
					return false;
				}
				m_RootDir = RegistryInfo.GetInstallDir(m_GameKey);
			}
			else
			{
				m_RootDir = rootDir;
			}
			m_GameDir = m_RootDir + "\\Game\\";
			m_FifaDbPartialFileName = "data/db/fifa_ng_db.db";
			m_FifaDbFileName = m_GameDir + m_FifaDbPartialFileName;
			m_FifaXmlPartialFileName = "data/db/fifa_ng_db-meta.xml";
			m_FifaXmlFileName = m_GameDir + m_FifaXmlPartialFileName;
			m_LangDbFileName = GetLanguageDbFilename(m_GameKey);
			m_LangXmlFileName = GetLanguageXmlFilename(m_GameKey);
			m_TempFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			m_TempFolder += "\\FM_temp";
			m_ExportFolder = (m_UserOptions.m_AutoExportFolder ? m_TempFolder : m_UserOptions.m_ExportFolder);
			if (!Directory.Exists(m_TempFolder))
			{
				Directory.CreateDirectory(m_TempFolder);
			}
			if (!Directory.Exists(m_ExportFolder))
			{
				Directory.CreateDirectory(m_TempFolder);
			}
			s_NameFontList = null;
			s_NumberFontList = null;
			s_MowingPatternList = null;
			s_NetList = null;
			s_GkGlovesList = null;
			s_ShoesList = null;
			s_AdboardList = null;
			s_BallList = null;
			s_ShoesList = null;
			return true;
		}

		private static bool Initialize15(string rootDir)
		{
			if (m_UserMessages == null)
			{
				m_UserMessages = new UserMessage();
			}
			if (m_UserOptions == null)
			{
				m_UserOptions = new UserOptions();
			}
			m_FifaFat = null;
			m_FifaDb = null;
			m_LangDb = null;
			InitializeLaunchFolder();
			m_Year = 15;
			m_GameKey = RegistryInfo.GetFifaKey(m_Year);
			if (rootDir == null)
			{
				if (!RegistryInfo.IsFifaInstalled(m_GameKey))
				{
					return false;
				}
				m_RootDir = RegistryInfo.GetInstallDir(m_GameKey);
			}
			else
			{
				m_RootDir = rootDir;
			}
			m_GameDir = m_RootDir + "\\";
			m_FifaDbPartialFileName = "data/db/fifa_ng_db.db";
			m_FifaDbFileName = m_GameDir + m_FifaDbPartialFileName;
			m_FifaXmlPartialFileName = "data/db/fifa_ng_db-meta.xml";
			m_FifaXmlFileName = m_GameDir + m_FifaXmlPartialFileName;
			m_LangDbFileName = GetLanguageDbFilename(m_GameKey);
			m_LangXmlFileName = GetLanguageXmlFilename(m_GameKey);
			m_TempFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			m_TempFolder += "\\FM_temp";
			m_ExportFolder = (m_UserOptions.m_AutoExportFolder ? m_TempFolder : m_UserOptions.m_ExportFolder);
			if (!Directory.Exists(m_TempFolder))
			{
				Directory.CreateDirectory(m_TempFolder);
			}
			if (!Directory.Exists(m_ExportFolder))
			{
				Directory.CreateDirectory(m_TempFolder);
			}
			s_NameFontList = null;
			s_NumberFontList = null;
			s_MowingPatternList = null;
			s_NetList = null;
			s_GkGlovesList = null;
			s_ShoesList = null;
			s_AdboardList = null;
			s_BallList = null;
			s_ShoesList = null;
			return true;
		}

		private static bool Initialize16(string rootDir)
		{
			if (m_UserMessages == null)
			{
				m_UserMessages = new UserMessage();
			}
			if (m_UserOptions == null)
			{
				m_UserOptions = new UserOptions();
			}
			m_FifaFat = null;
			m_FifaDb = null;
			m_LangDb = null;
			m_IsRevModInstalled = false;
			InitializeLaunchFolder();
			m_Year = 16;
			m_GameKey = RegistryInfo.GetFifaKey(m_Year);
			if (rootDir == null)
			{
				if (!RegistryInfo.IsFifaInstalled(m_GameKey))
				{
					return false;
				}
				m_RootDir = RegistryInfo.GetInstallDir(m_GameKey);
			}
			else
			{
				m_RootDir = rootDir;
			}
			m_GameDir = m_RootDir + "\\";
			m_FifaDbPartialFileName = "data/db/fifa_ng_db.db";
			m_FifaDbFileName = m_GameDir + m_FifaDbPartialFileName;
			m_FifaXmlPartialFileName = "data/db/fifa_ng_db-meta.xml";
			m_FifaXmlFileName = m_GameDir + m_FifaXmlPartialFileName;
			m_LangDbFileName = GetLanguageDbFilename(m_GameKey);
			m_LangXmlFileName = GetLanguageXmlFilename(m_GameKey);
			m_TempFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			m_TempFolder += "\\FM_temp";
			m_ExportFolder = (m_UserOptions.m_AutoExportFolder ? m_TempFolder : m_UserOptions.m_ExportFolder);
			if (!Directory.Exists(m_TempFolder))
			{
				Directory.CreateDirectory(m_TempFolder);
			}
			if (!Directory.Exists(m_ExportFolder))
			{
				Directory.CreateDirectory(m_TempFolder);
			}
			s_NameFontList = null;
			s_NumberFontList = null;
			s_MowingPatternList = null;
			s_NetList = null;
			s_GkGlovesList = null;
			s_ShoesList = null;
			s_AdboardList = null;
			s_BallList = null;
			s_ShoesList = null;
			return true;
		}

		public static bool Initialize(int year, string rootDir)
		{
			switch (year)
			{
			case 14:
				return Initialize14(rootDir);
			case 15:
				return Initialize15(rootDir);
			case 16:
				return Initialize16(rootDir);
			default:
				return false;
			}
		}

		public static string GetLanguageDbFilename(string game)
		{
			string gameDir = m_GameDir;
			return string.Concat(str1: ConvertLanguageToFileName(RegistryInfo.GetLocale(game)), str0: gameDir + "data\\loc\\", str2: ".db");
		}

		public static string GetLanguageXmlFilename(string game)
		{
			string gameDir = m_GameDir;
			return string.Concat(str1: ConvertLanguageToFileName(RegistryInfo.GetLocale(game)), str0: gameDir + "data\\loc\\", str2: "-meta.xml");
		}

		private static string ConvertLanguageToFileName(string locale)
		{
			string result = "eng_us";
			if (locale != null && locale != string.Empty)
			{
				result = (locale.StartsWith("it") ? "ita_it" : (locale.StartsWith("sa") ? "ara_sa" : (locale.StartsWith("cz") ? "cze_cz" : (locale.StartsWith("dk") ? "dan_dk" : (locale.StartsWith("nl") ? "dut_nl" : (locale.StartsWith("en") ? "eng_us" : (locale.StartsWith("fr") ? "fre_fr" : (locale.StartsWith("de") ? "ger_de" : (locale.StartsWith("hu") ? "hun_hu" : (locale.StartsWith("jp") ? "jpn_jp" : (locale.StartsWith("kr") ? "kor_kr" : (locale.StartsWith("no") ? "nor_no" : (locale.StartsWith("pl") ? "pol_pl" : (locale.StartsWith("pt") ? "por_pt" : (locale.StartsWith("br") ? "por_br" : (locale.StartsWith("ru") ? "rus_ru" : (locale.StartsWith("es") ? "spa_es" : (locale.StartsWith("mx") ? "spa_mx" : ((!locale.StartsWith("se")) ? "eng_us" : "swe_se")))))))))))))))))));
			}
			return result;
		}

		public static string GetEnglishFilename(string game)
		{
			return m_GameDir + "\\data\\loc\\eng_us.db";
		}

		public static string GetEnglishXmlFilename(string game)
		{
			return m_GameDir + "\\data\\loc\\eng_us-meta.xml";
		}

		public static void Close()
		{
			if (s_AdboardList != null)
			{
				s_AdboardList.Clear();
			}
			if (s_BallList != null)
			{
				s_BallList.Clear();
			}
			if (s_ShoesList != null)
			{
				s_ShoesList.Clear();
			}
			if (s_CountryList != null)
			{
				s_CountryList.Clear();
			}
			if (s_FormationList != null)
			{
				s_FormationList.Clear();
			}
			if (s_GenericFormationList != null)
			{
				s_GenericFormationList.Clear();
			}
			if (s_FreeAgentList != null)
			{
				s_FreeAgentList.Clear();
			}
			if (s_NumberFontList != null)
			{
				s_NumberFontList.Clear();
			}
			if (s_NameFontList != null)
			{
				s_NameFontList.Clear();
			}
			if (s_KitList != null)
			{
				s_KitList.Clear();
			}
			if (s_LeagueList != null)
			{
				s_LeagueList.Clear();
			}
			if (s_NetList != null)
			{
				s_NetList.Clear();
			}
			if (s_MowingPatternList != null)
			{
				s_MowingPatternList.Clear();
			}
			if (s_PlayerList != null)
			{
				s_PlayerList.Clear();
			}
			if (s_PlayerNamesList != null)
			{
				s_PlayerNamesList.Clear();
			}
			if (s_NameDictionary != null)
			{
				s_NameDictionary.Clear();
			}
			if (s_CareerFirstNameList != null)
			{
				s_CareerFirstNameList.Clear();
			}
			if (s_CareerLastNameList != null)
			{
				s_CareerLastNameList.Clear();
			}
			if (s_CareerCommonNameList != null)
			{
				s_CareerCommonNameList.Clear();
			}
			if (s_RefereeList != null)
			{
				s_RefereeList.Clear();
			}
			if (s_RoleList != null)
			{
				s_RoleList.Clear();
			}
			if (s_ShoesList != null)
			{
				s_ShoesList.Clear();
			}
			if (s_GkGlovesList != null)
			{
				s_GkGlovesList.Clear();
			}
			if (s_StadiumList != null)
			{
				s_StadiumList.Clear();
			}
			if (s_TeamList != null)
			{
				s_TeamList.Clear();
			}
			if (s_CompetitionObjects != null)
			{
				s_CompetitionObjects.Clear();
			}
			m_FifaDb = null;
			m_CareerFile = null;
			m_FifaFat = null;
			m_LangDb = null;
		}

		public static bool Open(ToolStripStatusLabel statusBar)
		{
			m_Status = statusBar;
			if (m_FifaFat == null)
			{
				if (m_Status != null)
				{
					m_Status.Text = "Opening data#.big files";
					m_Status.GetCurrentParent().Refresh();
				}
				if (!OpenFat())
				{
					return false;
				}
			}
			if (m_FifaDb == null)
			{
				if (m_Status != null)
				{
					m_Status.Text = "Opening main database";
					m_Status.GetCurrentParent().Refresh();
				}
				ExtractMainDatabase();
				if (!OpenFifaDb())
				{
					return false;
				}
			}
			if (m_LangDb == null)
			{
				if (m_Status != null)
				{
					m_Status.Text = "Opening language database";
					m_Status.GetCurrentParent().Refresh();
				}
				ExtractLangDatabase();
				if (!OpenLangDb())
				{
					return false;
				}
			}
			if (m_Year == 14)
			{
				string dbFileName = m_LaunchDir + "\\Templates\\2014\\" + m_FifaDbPartialFileName;
				string xmlFileName = m_LaunchDir + "\\Templates\\2014\\" + m_FifaXmlPartialFileName;
				m_OriginalFifaDb = new DbFile(dbFileName, xmlFileName);
			}
			else if (m_Year == 15)
			{
				string dbFileName2 = m_LaunchDir + "\\Templates\\2015\\" + m_FifaDbPartialFileName;
				string xmlFileName2 = m_LaunchDir + "\\Templates\\2015\\" + m_FifaXmlPartialFileName;
				m_OriginalFifaDb = new DbFile(dbFileName2, xmlFileName2);
			}
			else
			{
				string text = m_LaunchDir + "\\Templates\\" + m_FifaDbPartialFileName;
				string text2 = m_LaunchDir + "\\Templates\\" + m_FifaXmlPartialFileName;
				if (File.Exists(text) && File.Exists(text2))
				{
					m_OriginalFifaDb = new DbFile(text, text2);
				}
			}
			ExtractCompetitionFiles();
			if (File.Exists(m_GameDir + s_RevModFileNames[0]))
			{
				m_IsRevModInstalled = true;
			}
			if (m_Status != null)
			{
				m_Status.Text = "Loading...";
				m_Status.GetCurrentParent().Refresh();
			}
			LoadLists();
			Record[] records = m_OriginalFifaDb.Table[TI.rowteamnationlinks].Records;
			foreach (Record obj in records)
			{
				int andCheckIntField = obj.GetAndCheckIntField(FI.rowteamnationlinks_nationid);
				int andCheckIntField2 = obj.GetAndCheckIntField(FI.rowteamnationlinks_teamid);
				Team team = (Team)s_TeamList.SearchId(andCheckIntField2);
				if (team != null && (team.Country == null || team.Country.Id != andCheckIntField))
				{
					team.Country = (Country)s_CountryList.SearchId(andCheckIntField);
				}
			}
			Table obj2 = m_OriginalFifaDb.Table[TI.playerboots];
			int num = 0;
			records = obj2.Records;
			for (int i = 0; i < records.Length; i++)
			{
				Shoes shoes = new Shoes(records[i]);
				if (shoes.Id == 0 && num < 23)
				{
					ShoesList.s_GenericShoes[num++] = shoes;
				}
			}
			if (m_Status != null)
			{
				m_Status.Text = "Ready";
				m_Status.GetCurrentParent().Refresh();
			}
			return true;
		}

		public static bool Save(ToolStripStatusLabel statusBar)
		{
			m_Status = statusBar;
			if (m_Status != null)
			{
				m_Status.Text = "Saving lists";
				m_Status.GetCurrentParent().Refresh();
			}
			SaveLists();
			if (m_Status != null)
			{
				m_Status.Text = "Saving main database";
				m_Status.GetCurrentParent().Refresh();
			}
			SaveFifaDb();
			if (m_Status != null)
			{
				m_Status.Text = "Saving language";
				m_Status.GetCurrentParent().Refresh();
			}
			SaveLangDb();
			if (m_Status != null)
			{
				m_Status.Text = "Saving big files";
				m_Status.GetCurrentParent().Refresh();
			}
			m_FifaFat.Save();
			return true;
		}

		public static bool OpenFat()
		{
			m_FifaFat = null;
			m_FifaFat = FifaFat.Create(m_GameDir);
			if (m_FifaFat == null)
			{
				m_UserMessages.ShowMessage(10003);
				return false;
			}
			m_FifaFat.SaveOption = FifaFat.EFifaFatSaveOption.SaveOnCommand;
			m_FifaFat.ResetDefaultZdata();
			return true;
		}

		public static bool ExtractMainDatabase()
		{
			if (m_FifaFat == null)
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			if (m_Year == 14)
			{
				while (m_FifaFat.IsArchivedFilePresent(m_FifaDbPartialFileName))
				{
					if (!flag)
					{
						flag = m_FifaFat.ExtractFile(m_FifaDbPartialFileName);
					}
					else
					{
						m_FifaFat.HideFile(m_FifaDbPartialFileName);
					}
				}
				while (m_FifaFat.IsArchivedFilePresent(m_FifaXmlPartialFileName))
				{
					if (!flag2)
					{
						flag2 = m_FifaFat.ExtractFile(m_FifaXmlPartialFileName);
					}
					else
					{
						m_FifaFat.HideFile(m_FifaXmlPartialFileName);
					}
				}
			}
			else if (m_Year == 15)
			{
				string directoryName = Path.GetDirectoryName(m_FifaDbFileName);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				if (!File.Exists(m_FifaDbFileName))
				{
					File.Copy(m_LaunchDir + "\\Templates\\2015\\data\\db\\fifa_ng_db.db", m_FifaDbFileName);
				}
				if (!File.Exists(m_FifaXmlFileName))
				{
					File.Copy(m_LaunchDir + "\\Templates\\2015\\data\\db\\fifa_ng_db-meta.xml", m_FifaXmlFileName);
				}
				BhFile bhFile = m_FifaFat.GetBhFile(17);
				if (bhFile != null)
				{
					if (!bhFile.IsHidden(47))
					{
						bhFile.Hide(47);
					}
					if (!bhFile.IsHidden(48))
					{
						bhFile.Hide(48);
					}
				}
			}
			else if (m_Year == 16)
			{
				string directoryName2 = Path.GetDirectoryName(m_FifaDbFileName);
				if (!Directory.Exists(directoryName2))
				{
					Directory.CreateDirectory(directoryName2);
				}
				if (!File.Exists(m_FifaDbFileName))
				{
					File.Copy(m_LaunchDir + "\\Templates\\data\\db\\fifa_ng_db.db", m_FifaDbFileName);
				}
				if (!File.Exists(m_FifaXmlFileName))
				{
					File.Copy(m_LaunchDir + "\\Templates\\data\\db\\fifa_ng_db-meta.xml", m_FifaXmlFileName);
				}
				m_FifaFat.HideFile(m_FifaDbPartialFileName);
				m_FifaFat.HideFile(m_FifaXmlPartialFileName);
			}
			return flag && flag2;
		}

		public static bool ExtractLangDatabase()
		{
			string fileName = m_GameDir + "data\\loc\\locale.big";
			string gameDir = m_GameDir;
			FifaBigFile fifaBigFile = new FifaBigFile(fileName);
			string[] archivedFileNames = fifaBigFile.GetArchivedFileNames("*.db", useFullPath: true);
			string[] archivedFileNames2 = fifaBigFile.GetArchivedFileNames("*.xml", useFullPath: true);
			bool num = fifaBigFile.Export(archivedFileNames, gameDir);
			if (num)
			{
				fifaBigFile.Delete(archivedFileNames);
			}
			bool flag = fifaBigFile.Export(archivedFileNames2, gameDir);
			if (flag)
			{
				fifaBigFile.Delete(archivedFileNames2);
			}
			if (num | flag)
			{
				fifaBigFile.Save();
			}
			return num | flag;
		}

		public static bool ExtractCompetitionFiles()
		{
			if (m_FifaFat == null)
			{
				return false;
			}
			string[] fileNames = CompobjList.GetFileNames();
			bool result = false;
			string str = m_LaunchDir + "\\Templates\\";
			for (int i = 0; i < fileNames.Length; i++)
			{
				string text = m_GameDir + fileNames[i];
				if (!File.Exists(text))
				{
					string directoryName = Path.GetDirectoryName(text);
					if (!Directory.Exists(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
					File.Copy(str + fileNames[i], text, overwrite: true);
					m_FifaFat.HideFile(fileNames[i]);
					result = true;
				}
				else
				{
					m_FifaFat.HideFile(fileNames[i]);
				}
			}
			return result;
		}

		public static bool ExtractRevModFiles()
		{
			if (m_FifaFat == null)
			{
				return false;
			}
			DialogResult dialogResult = UserMessages.ShowMessage(31);
			if (dialogResult == DialogResult.No || dialogResult == DialogResult.Cancel)
			{
				return false;
			}
			bool result = false;
			string str = m_LaunchDir + "\\Templates\\";
			for (int i = 0; i < s_RevModFileNames.Length; i++)
			{
				m_IsRevModInstalled = true;
				string text = m_GameDir + s_RevModFileNames[i];
				string directoryName = Path.GetDirectoryName(text);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				if (!File.Exists(text))
				{
					File.Copy(str + s_RevModFileNames[i], text, overwrite: true);
					m_FifaFat.HideFile(s_RevModFileNames[i]);
					result = true;
				}
				else
				{
					m_FifaFat.HideFile(s_RevModFileNames[i]);
				}
			}
			m_IsRevModInstalled = true;
			m_UserMessages.ShowMessage(15008);
			return result;
		}

		public static bool OpenFifaDb()
		{
			m_FifaDb = null;
			if (m_FifaDbFileName != null && m_FifaXmlFileName != null && File.Exists(m_FifaDbFileName) && File.Exists(m_FifaXmlFileName))
			{
				m_FifaDb = new DbFile(m_FifaDbFileName, m_FifaXmlFileName);
			}
			if (m_FifaDb == null)
			{
				m_UserMessages.ShowMessage(10000);
				return false;
			}
			if (m_Year == 14)
			{
				TI.InitTI(m_FifaDb);
				FI.InitFI(m_FifaDb);
			}
			if (m_Year == 15)
			{
				TI.InitTI(m_FifaDb);
				FI.InitFI(m_FifaDb);
			}
			if (m_Year == 16)
			{
				TI.InitTI(m_FifaDb);
				FI.InitFI(m_FifaDb);
			}
			return m_FifaDb != null;
		}

		public static bool OpenLangDb()
		{
			m_LangDb = null;
			if (m_LangDbFileName != null && m_LangXmlFileName != null && File.Exists(m_LangDbFileName) && File.Exists(m_LangXmlFileName))
			{
				m_LangDb = new DbFile(m_LangDbFileName, m_LangXmlFileName);
			}
			if (m_LangDb != null)
			{
				m_Language = new Language(m_LangDb.Table[TI.lang]);
			}
			return m_LangDb != null;
		}

		public static void SaveFifaDb()
		{
			File.Copy(m_FifaDbFileName, m_FifaDbFileName + ".bak", overwrite: true);
			m_FifaDb.SaveDb(m_FifaDbFileName);
			m_FifaDb.SaveXml(m_FifaXmlFileName);
		}

		public static void SaveLangDb()
		{
			File.Copy(m_LangDbFileName, m_LangDbFileName + ".bak", overwrite: true);
			m_Language.Save(m_LangDb.Table[TI.lang]);
			m_LangDb.SaveDb(m_LangDbFileName);
		}

		public static Rx3File GetRx3FromZdata(string rx3FileName, bool verbose)
		{
			if (m_FifaFat == null)
			{
				return null;
			}
			Rx3File rx3File = null;
			FifaFile archivedFile = m_FifaFat.GetArchivedFile(rx3FileName);
			if (archivedFile != null)
			{
				if (archivedFile.IsCompressed)
				{
					archivedFile.Decompress();
				}
				BinaryReader reader = archivedFile.GetReader();
				rx3File = new Rx3File();
				rx3File.Load(reader);
				archivedFile.ReleaseReader(reader);
			}
			else
			{
				string path = m_GameDir + rx3FileName;
				if (File.Exists(path))
				{
					archivedFile = new FifaFile(path, isAnArchive: false);
					if (archivedFile != null)
					{
						rx3File = new Rx3File();
						rx3File.Load(archivedFile);
					}
				}
			}
			if (rx3File == null && verbose)
			{
				m_UserMessages.ShowMessage(3000, rx3FileName, merge: true);
				return null;
			}
			return rx3File;
		}

		public static Rx3File GetRx3FromZdata(string rx3FileName)
		{
			return GetRx3FromZdata(rx3FileName, verbose: true);
		}

		public static KitFile GetKitFromZdata(string rx3FileName)
		{
			if (m_FifaFat == null)
			{
				return null;
			}
			KitFile kitFile = null;
			FifaFile archivedFile = m_FifaFat.GetArchivedFile(rx3FileName);
			if (archivedFile != null)
			{
				if (archivedFile.IsCompressed)
				{
					archivedFile.Decompress();
				}
				BinaryReader reader = archivedFile.GetReader();
				kitFile = new KitFile();
				kitFile.Load(reader);
				archivedFile.ReleaseReader(reader);
			}
			else
			{
				string path = m_GameDir + rx3FileName;
				if (File.Exists(path))
				{
					archivedFile = new FifaFile(path, isAnArchive: false);
					if (archivedFile != null)
					{
						kitFile = new KitFile();
						kitFile.Load(archivedFile);
					}
				}
			}
			if (kitFile == null)
			{
				m_UserMessages.ShowMessage(3000, rx3FileName, merge: true);
				return null;
			}
			return kitFile;
		}

		public static bool IsFilePresent(string fileName)
		{
			if (m_FifaFat == null)
			{
				return false;
			}
			if (m_FifaFat.IsArchivedFilePresent(fileName))
			{
				return true;
			}
			if (File.Exists(m_GameDir + fileName))
			{
				return true;
			}
			return false;
		}

		public static Bitmap GetBmpFromRx3(string rx3FileName, int imageIndex)
		{
			Rx3File rx3FromZdata = GetRx3FromZdata(rx3FileName, verbose: false);
			if (rx3FromZdata == null)
			{
				return null;
			}
			if (rx3FromZdata.Images.Length > imageIndex && imageIndex >= 0)
			{
				return rx3FromZdata.Images[imageIndex].GetBitmap();
			}
			return null;
		}

		public static Bitmap[] GetBmpsFromRx3(string rx3FileName, bool verbose)
		{
			Rx3File rx3FromZdata = GetRx3FromZdata(rx3FileName, verbose);
			if (rx3FromZdata == null)
			{
				return null;
			}
			if (rx3FromZdata.Images != null && rx3FromZdata.Images.Length != 0)
			{
				return rx3FromZdata.GetBitmaps();
			}
			return null;
		}

		public static Bitmap[] GetBmpsFromRx3(string rx3FileName)
		{
			return GetBmpsFromRx3(rx3FileName, verbose: true);
		}

		public static Bitmap[] GetBitmapsFromRx3File(string rx3FileName)
		{
			if (rx3FileName == null)
			{
				return null;
			}
			if (!File.Exists(rx3FileName))
			{
				return null;
			}
			Rx3File rx3File = new Rx3File();
			if (!rx3File.Load(rx3FileName))
			{
				return null;
			}
			if (rx3File.Images.Length != 0)
			{
				return rx3File.GetBitmaps();
			}
			return null;
		}

		public static Bitmap[] GetKitFromRx3(string rx3FileName, out float[] positions)
		{
			KitFile kitFromZdata = GetKitFromZdata(rx3FileName);
			if (kitFromZdata == null)
			{
				positions = null;
				return null;
			}
			positions = kitFromZdata.Positions;
			if (kitFromZdata.Images.Length != 0)
			{
				return kitFromZdata.GetBitmaps();
			}
			return null;
		}

		public static Bitmap GetBmpFromRx3(string rx3FileName, bool verbose)
		{
			Rx3File rx3FromZdata = GetRx3FromZdata(rx3FileName, verbose);
			if (rx3FromZdata == null)
			{
				return null;
			}
			if (rx3FromZdata.Images.Length != 0)
			{
				return rx3FromZdata.Images[0].GetBitmap();
			}
			return null;
		}

		public static Bitmap GetBmpFromRx3(string rx3FileName)
		{
			return GetBmpFromRx3(rx3FileName, verbose: true);
		}

		public static Bitmap GetArtasset(string bigFileName)
		{
			FifaBigFile bigFromZdata = GetBigFromZdata(bigFileName);
			if (bigFromZdata == null)
			{
				return null;
			}
			FifaFile firstDds = bigFromZdata.GetFirstDds();
			if (firstDds == null)
			{
				return null;
			}
			return new DdsFile(firstDds).GetBitmap();
		}

		public static Bitmap GetArtasset(string bigFileName, string ddsFileName)
		{
			FifaBigFile bigFromZdata = GetBigFromZdata(bigFileName);
			if (bigFromZdata == null)
			{
				return null;
			}
			FifaFile ddsByName = bigFromZdata.GetDdsByName(ddsFileName);
			if (ddsByName == null)
			{
				return null;
			}
			return new DdsFile(ddsByName).GetBitmap();
		}

		public static Bitmap GetDdsArtasset(string ddsFileName)
		{
			FifaFile fileFromZdata = GetFileFromZdata(ddsFileName);
			if (fileFromZdata == null)
			{
				return null;
			}
			DdsFile ddsFile = new DdsFile(fileFromZdata);
			if (fileFromZdata == null)
			{
				return null;
			}
			return ddsFile.GetBitmap();
		}

		public static Bitmap GetBitmapFromDdsFile(string ddsFileName)
		{
			if (ddsFileName == null)
			{
				return null;
			}
			if (!File.Exists(ddsFileName))
			{
				return null;
			}
			return new DdsFile(ddsFileName).GetBitmap();
		}

		public static Bitmap GetBitmapFromBigFile(string bigFileName)
		{
			if (bigFileName == null)
			{
				return null;
			}
			if (!File.Exists(bigFileName))
			{
				return null;
			}
			FifaBigFile fifaBigFile = new FifaBigFile(bigFileName);
			if (fifaBigFile == null)
			{
				return null;
			}
			FifaFile firstDds = fifaBigFile.GetFirstDds();
			if (firstDds == null)
			{
				return null;
			}
			return new DdsFile(firstDds).GetBitmap();
		}

		public static Bitmap Get2dHead(string ddsFileName)
		{
			return GetDdsArtasset(ddsFileName);
		}

		private static FifaBigFile GetBigFromZdata(string bigFileName)
		{
			if (m_FifaFat == null)
			{
				return null;
			}
			FifaBigFile fifaBigFile = null;
			FifaFile archivedFile = m_FifaFat.GetArchivedFile(bigFileName);
			if (archivedFile != null)
			{
				if (archivedFile.IsCompressed)
				{
					archivedFile.Decompress();
				}
				fifaBigFile = new FifaBigFile(archivedFile);
			}
			else
			{
				string text = m_GameDir + bigFileName;
				if (File.Exists(text))
				{
					fifaBigFile = new FifaBigFile(text);
				}
			}
			if (fifaBigFile == null)
			{
				m_UserMessages.ShowMessage(3000, bigFileName, merge: true);
				return null;
			}
			return fifaBigFile;
		}

		public static bool SetArtasset(string templateBigName, string ddsName, int id, Bitmap bitmap)
		{
			string text = CreateAssetFromTemplate(templateBigName, ddsName, id, bitmap);
			if (text == null)
			{
				return false;
			}
			m_FifaFat.HideFile(text);
			return true;
		}

		public static bool SetArtasset(string templateBigName, string[] ddsNames, string fileBigName, Bitmap[] bitmaps)
		{
			string text = CreateMultipleAssetFromTemplate(templateBigName, ddsNames, fileBigName, bitmaps);
			if (text == null)
			{
				return false;
			}
			m_FifaFat.HideFile(text);
			return true;
		}

		public static bool SetArtasset(string templateBigName, string ddsName, int[] ids, Bitmap bitmap)
		{
			string text = CreateAssetFromTemplate(templateBigName, ddsName, ids, bitmap);
			if (text == null)
			{
				return false;
			}
			m_FifaFat.HideFile(text);
			return true;
		}

		public static bool SetArtasset(string templateBigName, string ddsName, int[] ids, Bitmap bitmap, string[] format)
		{
			string text = CreateAssetFromTemplate(templateBigName, ddsName, ids, bitmap, format);
			if (text == null)
			{
				return false;
			}
			m_FifaFat.HideFile(text);
			return true;
		}

		public static bool SetDdsArtasset(string templateDdsName, int id, Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			string text = CreateDdsAssetFromTemplate(templateDdsName, id, bitmap);
			if (text == null)
			{
				return false;
			}
			m_FifaFat.HideFile(text);
			return true;
		}

		public static bool SetDdsArtasset(string templateDdsName, string newDdsName, Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			string text = CreateDdsAssetFromTemplate(templateDdsName, newDdsName, bitmap);
			if (text == null)
			{
				return false;
			}
			m_FifaFat.HideFile(text);
			return true;
		}

		public static bool SetDdsArtasset(string templateDdsName, int[] ids, Bitmap bitmap, string[] format)
		{
			string text = CreateDdsAssetFromTemplate(templateDdsName, ids, bitmap, format);
			if (text == null)
			{
				return false;
			}
			m_FifaFat.HideFile(text);
			return true;
		}

		public static bool Set2dHead(string templateDdsName, int id, Bitmap bitmap)
		{
			return SetDdsArtasset(templateDdsName, id, bitmap);
		}

		public static bool Delete2dHead(string fileName)
		{
			return DeleteFromZdata(fileName);
		}

		private static string CreateAssetFromTemplate(string templateBigName, string ddsName, int id, Bitmap bitmap)
		{
			string format = null;
			Path.GetDirectoryName(templateBigName);
			string text = templateBigName.Replace("#", (id >= 0) ? id.ToString(format) : "");
			text = text.Replace("2014_", "");
			return CreateAssetFromTemplate(templateBigName, ddsName, text, bitmap);
		}

		private static string CreateAssetFromTemplate(string templateBigName, string ddsName, int[] ids, Bitmap bitmap)
		{
			string format = null;
			Path.GetDirectoryName(templateBigName);
			string text = templateBigName.Replace("#", (ids[0] >= 0) ? ids[0].ToString(format) : "");
			text = text.Replace("%", ids[1].ToString(format));
			return CreateAssetFromTemplate(templateBigName, ddsName, text, bitmap);
		}

		private static string CreateAssetFromTemplate(string templateBigName, string ddsName, int[] ids, Bitmap bitmap, string[] format)
		{
			Path.GetDirectoryName(templateBigName);
			string text = templateBigName.Replace("#", (ids[0] >= 0) ? ids[0].ToString(format[0]) : "");
			text = text.Replace("%", ids[1].ToString(format[1]));
			return CreateAssetFromTemplate(templateBigName, ddsName, text, bitmap);
		}

		private static string CreateAssetFromTemplate(string templateBigName, string ddsName, string newBigFileName, Bitmap bitmap)
		{
			string directoryName = Path.GetDirectoryName(templateBigName);
			string text = m_LaunchDir + "\\Templates\\" + templateBigName;
			string text2 = m_LaunchDir + "\\Templates\\" + newBigFileName;
			if (!File.Exists(text))
			{
				m_UserMessages.ShowMessage(5026);
				return null;
			}
			File.Copy(text, text2, overwrite: true);
			FifaBigFile fifaBigFile = new FifaBigFile(text2);
			if (fifaBigFile == null)
			{
				m_UserMessages.ShowMessage(5027);
				return null;
			}
			fifaBigFile.LoadArchivedFiles();
			FifaFile archivedFile = fifaBigFile.GetArchivedFile(ddsName, useFullPath: true);
			if (archivedFile == null)
			{
				m_UserMessages.ShowMessage(5027);
				return null;
			}
			if (archivedFile.IsCompressed)
			{
				archivedFile.Decompress();
			}
			DdsFile ddsFile = new DdsFile(archivedFile);
			if (ddsFile == null)
			{
				m_UserMessages.ShowMessage(5027);
				return null;
			}
			ddsFile.ReplaceBitmap(bitmap);
			BinaryWriter writer = archivedFile.GetWriter();
			if (writer == null)
			{
				m_UserMessages.ShowMessage(5027);
				return null;
			}
			ddsFile.Save(writer);
			archivedFile.ReleaseWriter(writer);
			fifaBigFile.Save();
			string text3 = m_GameDir + directoryName + "\\";
			string destFileName = text3 + Path.GetFileName(newBigFileName);
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			File.Copy(text2, destFileName, overwrite: true);
			if (!text2.Contains("#"))
			{
				File.Delete(text2);
			}
			return newBigFileName;
		}

		private static string CreateMultipleAssetFromTemplate(string templateBigName, string[] ddsNames, string newBigFileName, Bitmap[] bitmaps)
		{
			if (ddsNames.Length != bitmaps.Length)
			{
				return null;
			}
			string directoryName = Path.GetDirectoryName(templateBigName);
			string text = m_LaunchDir + "\\Templates\\" + templateBigName;
			string text2 = m_LaunchDir + "\\Templates\\" + newBigFileName;
			if (!File.Exists(text))
			{
				m_UserMessages.ShowMessage(5026);
				return null;
			}
			File.Copy(text, text2, overwrite: true);
			FifaBigFile fifaBigFile = new FifaBigFile(text2);
			if (fifaBigFile == null)
			{
				m_UserMessages.ShowMessage(5027);
				return null;
			}
			fifaBigFile.LoadArchivedFiles();
			for (int i = 0; i < ddsNames.Length; i++)
			{
				FifaFile archivedFile = fifaBigFile.GetArchivedFile(ddsNames[i], useFullPath: true);
				if (archivedFile == null)
				{
					continue;
				}
				if (archivedFile.IsCompressed)
				{
					archivedFile.Decompress();
				}
				DdsFile ddsFile = new DdsFile(archivedFile);
				if (ddsFile != null)
				{
					ddsFile.ReplaceBitmap(bitmaps[i]);
					BinaryWriter writer = archivedFile.GetWriter();
					if (writer != null)
					{
						ddsFile.Save(writer);
						archivedFile.ReleaseWriter(writer);
					}
				}
			}
			fifaBigFile.Save();
			string text3 = m_GameDir + directoryName + "\\";
			string destFileName = text3 + Path.GetFileName(newBigFileName);
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			File.Copy(text2, destFileName, overwrite: true);
			if (!text2.Contains("#"))
			{
				File.Delete(text2);
			}
			return newBigFileName;
		}

		private static string CreateDdsAssetFromTemplate(string templateDdsName, int id, Bitmap bitmap)
		{
			if (templateDdsName == null)
			{
				return null;
			}
			Path.GetDirectoryName(templateDdsName);
			string newDdsFileName = templateDdsName.Replace("#", id.ToString());
			return CreateDdsAssetFromTemplate(templateDdsName, newDdsFileName, bitmap);
		}

		private static string CreateDdsAssetFromTemplate(string templateDdsName, int id, Bitmap bitmap, string[] format)
		{
			return CreateDdsAssetFromTemplate(templateDdsName, new int[2]
			{
				id,
				0
			}, bitmap, format);
		}

		private static string CreateDdsAssetFromTemplate(string templateDdsName, int[] ids, Bitmap bitmap, string[] format)
		{
			Path.GetDirectoryName(templateDdsName);
			string text = templateDdsName.Replace("2014_", "");
			text = text.Replace("#", ids[0].ToString(format[0]));
			if (ids.Length >= 2)
			{
				text = text.Replace("%", ids[1].ToString(format[1]));
			}
			if (ids.Length >= 3)
			{
				text = text.Replace("@", ids[2].ToString(format[2]));
			}
			return CreateDdsAssetFromTemplate(templateDdsName, text, bitmap);
		}

		private static string CreateDdsAssetFromTemplate(string templateDdsName, string newDdsFileName, Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return null;
			}
			string directoryName = Path.GetDirectoryName(templateDdsName);
			string text = m_LaunchDir + "\\Templates\\" + templateDdsName;
			string text2 = m_LaunchDir + "\\Templates\\" + newDdsFileName;
			if (!File.Exists(text))
			{
				m_UserMessages.ShowMessage(5026);
				return null;
			}
			File.Copy(text, text2, overwrite: true);
			DdsFile ddsFile = new DdsFile(text2);
			if (ddsFile == null)
			{
				m_UserMessages.ShowMessage(5027);
				return null;
			}
			ddsFile.ReplaceBitmap(bitmap);
			ddsFile.Save(text2);
			string text3 = m_GameDir + directoryName + "\\";
			string destFileName = text3 + Path.GetFileName(newDdsFileName);
			if (!Directory.Exists(text3))
			{
				Directory.CreateDirectory(text3);
			}
			File.Copy(text2, destFileName, overwrite: true);
			if (!text2.Contains("#"))
			{
				File.Delete(text2);
			}
			return newDdsFileName;
		}

		public static bool IsPatched(string fileName)
		{
			return File.Exists(m_GameDir + fileName);
		}

		public static FifaFile GetFileFromZdata(string fileName)
		{
			if (m_FifaFat == null || fileName == null)
			{
				return null;
			}
			FifaFile archivedFile = m_FifaFat.GetArchivedFile(fileName);
			if (archivedFile != null)
			{
				return archivedFile;
			}
			string path = m_GameDir + fileName;
			if (File.Exists(path))
			{
				return new FifaFile(path, isAnArchive: false);
			}
			return null;
		}

		public static bool ExportFileFromZdata(string fileName, string path)
		{
			if (m_FifaFat == null || fileName == null)
			{
				return false;
			}
			bool flag = m_FifaFat.ExportFile(fileName, path);
			if (flag)
			{
				return true;
			}
			string text = m_GameDir + fileName;
			string text2 = path + "\\" + fileName;
			if (File.Exists(text) && text != text2)
			{
				string directoryName = Path.GetDirectoryName(text2);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				File.Copy(text, text2, overwrite: true);
				return true;
			}
			m_UserMessages.ShowMessage(1028, " " + fileName, merge: true);
			return flag;
		}

		public static bool ExportTtfFromZdata(string fileName, string path)
		{
			if (m_FifaFat == null || fileName == null)
			{
				return false;
			}
			string text = m_GameDir + fileName;
			string text2 = path + "\\" + fileName;
			if (File.Exists(text))
			{
				if (text != text2)
				{
					File.Copy(text, text2, overwrite: true);
				}
				return true;
			}
			m_UserMessages.ShowMessage(1028, " " + fileName, merge: true);
			return false;
		}

		public static bool AskAndExportFromZdata(string fileName, ref string path)
		{
			if (m_FifaFat == null)
			{
				m_UserMessages.ShowMessage(10002);
				return false;
			}
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			folderBrowserDialog.SelectedPath = path;
			folderBrowserDialog.Description = "Select the export folder";
			folderBrowserDialog.ShowNewFolderButton = true;
			if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
			{
				folderBrowserDialog.Dispose();
				return false;
			}
			path = folderBrowserDialog.SelectedPath;
			folderBrowserDialog.Dispose();
			return ExportFileFromZdata(fileName, path);
		}

		public static bool ImportKitIntoZdata(string templateRx3Name, int[] ids, Bitmap[] bitmaps, float[] kitPositions)
		{
			if (templateRx3Name == null)
			{
				return false;
			}
			if (!m_UserOptions.m_SaveZdata)
			{
				m_UserMessages.ShowMessage(5035);
				return false;
			}
			if (m_FifaFat == null)
			{
				m_UserMessages.ShowMessage(10002);
				return false;
			}
			string text = m_LaunchDir + "\\Templates\\" + templateRx3Name;
			string text2 = templateRx3Name;
			string text3 = text;
			if (text3.Contains("2014_"))
			{
				text2 = text2.Replace("2014_", "");
				text3 = text3.Replace("2014_", "");
			}
			if (ids.Length != 0)
			{
				text2 = text2.Replace("#", ids[0].ToString());
				text3 = text3.Replace("#", ids[0].ToString());
			}
			if (ids.Length > 1)
			{
				text2 = text2.Replace("%", ids[1].ToString());
				text3 = text3.Replace("%", ids[1].ToString());
			}
			if (ids.Length > 2)
			{
				text2 = text2.Replace("@", ids[2].ToString());
				text3 = text3.Replace("@", ids[2].ToString());
			}
			else
			{
				text2 = text2.Replace("@", "0");
				text3 = text3.Replace("@", "0");
			}
			if (text != text3)
			{
				File.Copy(text, text3, overwrite: true);
			}
			KitFile kitFile = new KitFile();
			kitFile.Load(text3);
			if (kitFile.Images.Length != bitmaps.Length)
			{
				m_UserMessages.ShowMessage(5025);
				return false;
			}
			kitFile.ReplaceBitmaps(bitmaps);
			kitFile.Positions = kitPositions;
			kitFile.Save(text3, saveBitmaps: true, saveVertex: false);
			bool num = ImportFileIntoZdataAs(text3, text2, delete: true, ECompressionMode.Chunkzip);
			if (!num)
			{
				m_UserMessages.ShowMessage(10007);
			}
			return num;
		}

		public static bool ImportBmpsIntoStadium(string stadiumRx3Name, Bitmap[] bitmaps)
		{
			if (stadiumRx3Name == null)
			{
				return false;
			}
			if (!m_UserOptions.m_SaveZdata)
			{
				m_UserMessages.ShowMessage(5035);
				return false;
			}
			if (m_FifaFat == null)
			{
				m_UserMessages.ShowMessage(10002);
				return false;
			}
			string text = m_GameDir + stadiumRx3Name;
			if (!File.Exists(text) && !m_FifaFat.ExtractFile(stadiumRx3Name))
			{
				return false;
			}
			Rx3File rx3File = new Rx3File();
			rx3File.Load(text);
			if (rx3File.Images.Length != bitmaps.Length)
			{
				m_UserMessages.ShowMessage(5025);
				return false;
			}
			rx3File.ReplaceBitmaps(bitmaps);
			rx3File.Save(text, saveBitmaps: true, saveVertex: false);
			return true;
		}

		public static bool ImportBmpsIntoZdata(string templateRx3Name, string rx3FileName, Bitmap[] bitmaps, ECompressionMode compressionMode, Rx3Signatures signatures)
		{
			if (templateRx3Name == null)
			{
				return false;
			}
			if (!m_UserOptions.m_SaveZdata)
			{
				m_UserMessages.ShowMessage(5035);
				return false;
			}
			if (m_FifaFat == null)
			{
				m_UserMessages.ShowMessage(10002);
				return false;
			}
			string text = m_LaunchDir + "\\Templates\\" + templateRx3Name;
			string text2 = m_LaunchDir + "\\Templates\\" + rx3FileName;
			if (text != text2)
			{
				File.Copy(text, text2, overwrite: true);
			}
			Rx3File rx3File = new Rx3File();
			rx3File.Load(text2);
			rx3File.ReplaceBitmaps(bitmaps);
			rx3File.Signatures = signatures;
			rx3File.Save(text2, saveBitmaps: true, saveVertex: false);
			bool num = ImportFileIntoZdataAs(text2, rx3FileName, delete: true, compressionMode);
			if (!num)
			{
				m_UserMessages.ShowMessage(10007);
			}
			return num;
		}

		public static bool ImportBmpsIntoZdata(string templateRx3Name, int[] ids, Bitmap[] bitmaps, ECompressionMode compressionMode, Rx3Signatures signatures)
		{
			if (templateRx3Name == null)
			{
				return false;
			}
			if (!m_UserOptions.m_SaveZdata)
			{
				m_UserMessages.ShowMessage(5035);
				return false;
			}
			if (m_FifaFat == null)
			{
				m_UserMessages.ShowMessage(10002);
				return false;
			}
			string text = m_LaunchDir + "\\Templates\\" + templateRx3Name;
			string text2 = templateRx3Name;
			string text3 = text;
			if (text3.Contains("2014_"))
			{
				text2 = text2.Replace("2014_", "");
				text3 = text3.Replace("2014_", "");
			}
			if (ids.Length != 0)
			{
				text2 = text2.Replace("#", ids[0].ToString());
				text3 = text3.Replace("#", ids[0].ToString());
			}
			if (ids.Length > 1)
			{
				text2 = text2.Replace("%", ids[1].ToString());
				text3 = text3.Replace("%", ids[1].ToString());
			}
			if (text != text3)
			{
				File.Copy(text, text3, overwrite: true);
			}
			Rx3File rx3File = new Rx3File();
			rx3File.Load(text3);
			if (rx3File.Images.Length != bitmaps.Length)
			{
				m_UserMessages.ShowMessage(5025);
				return false;
			}
			rx3File.ReplaceBitmaps(bitmaps);
			rx3File.Signatures = signatures;
			rx3File.Save(text3, saveBitmaps: true, saveVertex: false);
			bool num = ImportFileIntoZdataAs(text3, text2, delete: true, compressionMode);
			if (!num)
			{
				m_UserMessages.ShowMessage(10007);
			}
			return num;
		}

		public static bool ImportBmpsIntoZdata(string templateRx3Name, int[] ids, Bitmap[] bitmaps, ECompressionMode compressionMode)
		{
			return ImportBmpsIntoZdata(templateRx3Name, ids, bitmaps, compressionMode, null);
		}

		public static bool ImportBmpsIntoZdata(string templateRx3Name, int id, Bitmap bitmap, ECompressionMode compressionMode)
		{
			return ImportBmpsIntoZdata(bitmaps: new Bitmap[1]
			{
				bitmap
			}, templateRx3Name: templateRx3Name, ids: new int[1]
			{
				id
			}, compressionMode: compressionMode, signatures: null);
		}

		public static bool ImportBmpsIntoZdata(string templateRx3Name, int id, Bitmap bitmap, ECompressionMode compressionMode, Rx3Signatures signatures)
		{
			return ImportBmpsIntoZdata(bitmaps: new Bitmap[1]
			{
				bitmap
			}, templateRx3Name: templateRx3Name, ids: new int[1]
			{
				id
			}, compressionMode: compressionMode, signatures: signatures);
		}

		public static bool ImportBmpsIntoZdata(string templateRx3Name, int id, Bitmap[] bitmaps, ECompressionMode compressionMode)
		{
			return ImportBmpsIntoZdata(templateRx3Name, new int[1]
			{
				id
			}, bitmaps, compressionMode, null);
		}

		public static bool ImportBmpsIntoZdata(string templateRx3Name, int id, Bitmap[] bitmaps, ECompressionMode compressionMode, Rx3Signatures signatures)
		{
			return ImportBmpsIntoZdata(templateRx3Name, new int[1]
			{
				id
			}, bitmaps, compressionMode, signatures);
		}

		public static bool ImportBmpsIntoZdata(string templateRx3Name, int[] ids, Bitmap bitmap, ECompressionMode compressionMode)
		{
			return ImportBmpsIntoZdata(templateRx3Name, ids, new Bitmap[1]
			{
				bitmap
			}, compressionMode, null);
		}

		public static bool ImportBmpsIntoZdata(string templateRx3Name, int[] ids, Bitmap bitmap, ECompressionMode compressionMode, Rx3Signatures signatures)
		{
			return ImportBmpsIntoZdata(templateRx3Name, ids, new Bitmap[1]
			{
				bitmap
			}, compressionMode, signatures);
		}

		public static bool ImportFileIntoZdataAs(string fileName, string archivedName, bool delete, ECompressionMode compressionMode)
		{
			return ImportFileIntoZdataAs(fileName, archivedName, delete, compressionMode, null);
		}

		public static bool ImportFileIntoZdataAs(string fileName, string archivedName, bool delete, ECompressionMode compressionMode, Rx3Signatures signatures)
		{
			delete = (delete && !fileName.Contains("#"));
			archivedName = archivedName.Replace('\\', '/');
			if (fileName == null)
			{
				m_UserMessages.ShowMessage(10010);
				return false;
			}
			if (!m_UserOptions.m_SaveZdata)
			{
				m_UserMessages.ShowMessage(5035);
				if (delete)
				{
					File.Delete(fileName);
				}
				return false;
			}
			if (m_FifaFat == null)
			{
				m_UserMessages.ShowMessage(10002);
				if (delete)
				{
					File.Delete(fileName);
				}
				return false;
			}
			if (!File.Exists(fileName))
			{
				m_UserMessages.ShowMessage(10011);
				return false;
			}
			if (m_UserOptions.m_SaveZdataInFolder)
			{
				string text = m_GameDir + archivedName;
				string directoryName = Path.GetDirectoryName(text);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				File.Copy(fileName, text, overwrite: true);
				if (signatures != null)
				{
					FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.ReadWrite);
					BinaryWriter binaryWriter = new BinaryWriter(fileStream);
					signatures.Save(binaryWriter);
					binaryWriter.Close();
					fileStream.Close();
				}
				if (delete)
				{
					File.Delete(fileName);
				}
				m_FifaFat.HideFile(archivedName);
				return true;
			}
			return m_FifaFat.ImportFileAs(fileName, archivedName, delete, compressionMode);
		}

		public static bool ImportTtfIntoZdataAs(string fileName, string archivedName, bool delete)
		{
			delete = (delete && !fileName.Contains("#"));
			archivedName = archivedName.Replace('\\', '/');
			if (fileName == null)
			{
				m_UserMessages.ShowMessage(10010);
				return false;
			}
			if (!m_UserOptions.m_SaveZdata)
			{
				m_UserMessages.ShowMessage(5035);
				if (delete)
				{
					File.Delete(fileName);
				}
				return false;
			}
			if (m_FifaFat == null)
			{
				m_UserMessages.ShowMessage(10002);
				if (delete)
				{
					File.Delete(fileName);
				}
				return false;
			}
			if (!File.Exists(fileName))
			{
				m_UserMessages.ShowMessage(10011);
				return false;
			}
			if (m_UserOptions.m_SaveZdataInFolder)
			{
				string text = m_GameDir + archivedName;
				string directoryName = Path.GetDirectoryName(text);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				File.Copy(fileName, text, overwrite: true);
				if (delete)
				{
					File.Delete(fileName);
				}
				m_FifaFat.HideFile(archivedName);
				return true;
			}
			return false;
		}

		public static bool DeleteFromZdata(string fileName)
		{
			if (fileName == null)
			{
				m_UserMessages.ShowMessage(10010);
				return false;
			}
			if (!m_UserOptions.m_SaveZdata)
			{
				m_UserMessages.ShowMessage(5035);
				return false;
			}
			if (m_FifaFat == null)
			{
				m_UserMessages.ShowMessage(10002);
				return false;
			}
			string path = m_GameDir + fileName;
			if (File.Exists(path))
			{
				File.Delete(path);
				m_FifaFat.RestoreFile(fileName);
				return true;
			}
			return m_FifaFat.HideFile(fileName);
		}

		public static bool CloneIntoZdata(string srcFileName, string destFileName)
		{
			ECompressionMode eCompressionMode = ECompressionMode.None;
			string str = m_TempFolder + "\\";
			if (!ExportFileFromZdata(srcFileName, m_TempFolder))
			{
				return false;
			}
			string text = str + srcFileName;
			if (!File.Exists(text))
			{
				return false;
			}
			eCompressionMode = m_FifaFat.GetCompressionMode(srcFileName);
			if (!ImportFileIntoZdataAs(text, destFileName, delete: true, eCompressionMode))
			{
				return false;
			}
			return true;
		}

		public static bool AskAndSaveBitmap(Bitmap bitmap)
		{
			if (bitmap == null)
			{
				return false;
			}
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "bmp files (*.bmp)|*.bmp|png files (*.png)|*.png";
			saveFileDialog.InitialDirectory = m_ExportFolder;
			saveFileDialog.FilterIndex = 2;
			saveFileDialog.Title = "Save picture as .bmp or .png";
			if (saveFileDialog.ShowDialog() != DialogResult.OK)
			{
				saveFileDialog.Dispose();
				return false;
			}
			string extension = Path.GetExtension(saveFileDialog.FileName);
			ImageFormat format;
			if (extension.ToLower() == ".bmp")
			{
				format = ImageFormat.Bmp;
				Color pixel = bitmap.GetPixel(0, 0);
				for (int num = bitmap.Width - 1; num >= 0; num--)
				{
					for (int num2 = bitmap.Height - 1; num2 >= 0; num2--)
					{
						if (bitmap.GetPixel(num, num2).A < 192)
						{
							bitmap.SetPixel(num, num2, pixel);
						}
					}
				}
			}
			else
			{
				if (!(extension.ToLower() == ".png"))
				{
					m_UserMessages.ShowMessage(5034);
					return false;
				}
				format = ImageFormat.Png;
			}
			bitmap.Save(saveFileDialog.FileName, format);
			saveFileDialog.Dispose();
			return true;
		}

		public static Bitmap BrowseAndCheckBitmap(int width, int height, int sizeMultiplier, int transparentMode)
		{
			DialogResult dialogResult = m_UserMessages.ShowMessage(12);
			if (dialogResult == DialogResult.No || dialogResult == DialogResult.Cancel)
			{
				return null;
			}
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.CheckFileExists = true;
			openFileDialog.Multiselect = false;
			openFileDialog.InitialDirectory = m_ExportFolder;
			openFileDialog.RestoreDirectory = true;
			openFileDialog.Filter = "Image Files (*.bmp;*.png)|*.bmp;*.png";
			openFileDialog.FilterIndex = 1;
			openFileDialog.Title = "Open Image File";
			if (openFileDialog.ShowDialog() != DialogResult.OK)
			{
				openFileDialog.Dispose();
				return null;
			}
			string fileName = openFileDialog.FileName;
			openFileDialog.Dispose();
			Bitmap bitmap = new Bitmap(fileName);
			if (bitmap == null)
			{
				m_UserMessages.ShowMessage(10006);
				return null;
			}
			if (bitmap.Width != width || bitmap.Height != height)
			{
				switch (sizeMultiplier)
				{
				case 1:
					width += width / 2;
					height += height / 2;
					break;
				case 2:
					width *= 2;
					height *= 2;
					break;
				}
				if (bitmap.Width != width || bitmap.Height != height)
				{
					m_UserMessages.ShowMessage(5015);
					return null;
				}
			}
			if (transparentMode != 0 && Path.GetExtension(fileName).ToLower() == ".bmp")
			{
				Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
				Color pixel = bitmap.GetPixel(0, 0);
				Color color = Color.FromArgb(0, 0, 0, 0);
				for (int i = 0; i < bitmap.Width; i++)
				{
					for (int j = 0; j < bitmap.Height; j++)
					{
						Color pixel2 = bitmap.GetPixel(i, j);
						if (pixel2 == pixel)
						{
							bitmap2.SetPixel(i, j, color);
						}
						else
						{
							bitmap2.SetPixel(i, j, pixel2);
						}
					}
				}
				return bitmap2;
			}
			return bitmap;
		}

		public static string BrowseAndCheckModel(ref string path, string title, string filter)
		{
			DialogResult dialogResult = m_UserMessages.ShowMessage(12);
			if (dialogResult == DialogResult.No || dialogResult == DialogResult.Cancel)
			{
				return null;
			}
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.CheckFileExists = true;
			openFileDialog.Multiselect = false;
			openFileDialog.InitialDirectory = path;
			openFileDialog.Filter = filter;
			openFileDialog.FilterIndex = 1;
			openFileDialog.Title = title;
			if (openFileDialog.ShowDialog() != DialogResult.OK)
			{
				openFileDialog.Dispose();
				return null;
			}
			string fileName = openFileDialog.FileName;
			path = Path.GetFullPath(fileName);
			openFileDialog.Dispose();
			return fileName;
		}

		public static string BrowseAndCheckTtf(ref string path)
		{
			DialogResult dialogResult = m_UserMessages.ShowMessage(12);
			if (dialogResult == DialogResult.No || dialogResult == DialogResult.Cancel)
			{
				return null;
			}
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.CheckFileExists = true;
			openFileDialog.Multiselect = false;
			openFileDialog.InitialDirectory = path;
			openFileDialog.Filter = "Font files(*.ttf)|*.ttf";
			openFileDialog.FilterIndex = 1;
			openFileDialog.Title = "Open font file";
			if (openFileDialog.ShowDialog() != DialogResult.OK)
			{
				openFileDialog.Dispose();
				return null;
			}
			string fileName = openFileDialog.FileName;
			path = Path.GetFullPath(fileName);
			openFileDialog.Dispose();
			return fileName;
		}

		public static void LoadLists(EFifaObjects fifaObjects)
		{
			_ = m_FifaFat;
			if (m_FifaDb == null)
			{
				return;
			}
			if ((fifaObjects & EFifaObjects.FifaRole) != 0)
			{
				s_RoleList = new RoleList(m_FifaDb);
			}
			if ((fifaObjects & EFifaObjects.FifaFormation) != 0)
			{
				s_FormationList = new FormationList(m_FifaDb);
				s_GenericFormationList = new FormationList();
				foreach (Formation s_Formation in s_FormationList)
				{
					if (s_Formation.IsGeneric())
					{
						s_GenericFormationList.Add(s_Formation);
					}
				}
			}
			if ((fifaObjects & EFifaObjects.FifaCountry) != 0)
			{
				s_CountryList = new CountryList(m_FifaDb);
			}
			if ((fifaObjects & EFifaObjects.FifaKit) != 0)
			{
				s_KitList = new KitList(m_FifaDb);
			}
			if ((fifaObjects & EFifaObjects.FifaReferee) != 0)
			{
				s_RefereeList = new RefereeList(m_FifaDb);
			}
			if ((fifaObjects & EFifaObjects.FifaLeague) != 0)
			{
				s_LeagueList = new LeagueList(m_FifaDb);
			}
			if ((fifaObjects & EFifaObjects.FifaTeam) != 0)
			{
				s_TeamList = new TeamList(m_FifaDb);
				s_FreeAgentList = new FreeAgentList();
				s_TeamList.LinkOpponent(s_TeamList);
			}
			if (s_RefereeList != null && s_CountryList != null)
			{
				s_RefereeList.LinkCountry(s_CountryList);
			}
			if (s_RefereeList != null && s_LeagueList != null)
			{
				s_RefereeList.LinkLeague(s_LeagueList);
			}
			if (s_KitList != null && s_TeamList != null)
			{
				s_TeamList.LinkKits(s_KitList);
				s_KitList.LinkTeam(s_TeamList);
			}
			if (s_LeagueList != null && s_TeamList != null)
			{
				s_LeagueList.FillFromLeagueTeamLinks(m_FifaDb.Table[TI.leagueteamlinks]);
			}
			if (s_LeagueList != null && s_CountryList != null)
			{
				s_LeagueList.LinkCountry(s_CountryList);
			}
			if (s_LeagueList != null && s_BallList != null)
			{
				s_LeagueList.LinkBall(s_BallList);
			}
			if ((fifaObjects & EFifaObjects.FifaPlayer) != 0)
			{
				s_PlayerNamesList = new PlayerNames(m_FifaDb);
				if (m_OriginalFifaDb != null)
				{
					s_OriginalPlayerNamesList = new PlayerNames(m_OriginalFifaDb);
				}
				Player.PlayerNames = s_PlayerNamesList;
				s_NameDictionary = new NameDictionary(m_FifaDb);
				CareerFirstName.PlayerNames = s_PlayerNamesList;
				CareerLastName.PlayerNames = s_PlayerNamesList;
				CareerCommonName.PlayerNames = s_PlayerNamesList;
				s_PlayerList = new PlayerList(m_FifaDb);
				s_CareerFirstNameList = new CareerFirstNameList(m_FifaDb);
				s_CareerLastNameList = new CareerLastNameList(m_FifaDb);
				s_CareerCommonNameList = new CareerCommonNameList(m_FifaDb);
			}
			if (s_TeamList != null && s_PlayerList != null)
			{
				s_TeamList.FillFromTeamPlayerLinks(m_FifaDb);
				s_TeamList.LinkPlayer(s_PlayerList);
			}
			if (s_PlayerList != null && s_CountryList != null)
			{
				s_PlayerList.LinkCountry(s_CountryList);
			}
			if (s_PlayerList != null && s_TeamList != null)
			{
				s_PlayerList.LinkTeam(s_TeamList);
			}
			if (s_TeamList != null && s_CountryList != null)
			{
				s_TeamList.LinkCountry(s_CountryList);
			}
			if (s_TeamList != null && s_CountryList != null)
			{
				s_CountryList.LinkTeam(s_TeamList);
			}
			if (s_TeamList != null && s_FormationList != null)
			{
				s_TeamList.LinkFormation(s_FormationList);
				s_FormationList.LinkTeam(s_TeamList);
			}
			if ((fifaObjects & EFifaObjects.FifaStadium) != 0)
			{
				s_StadiumList = new StadiumList(m_FifaDb);
			}
			if (s_TeamList != null && s_StadiumList != null)
			{
				s_TeamList.LinkStadiums(s_StadiumList);
				s_StadiumList.LinkTeam(s_TeamList);
			}
			if (s_CountryList != null && s_StadiumList != null)
			{
				s_StadiumList.LinkCountry(s_CountryList);
			}
			if (s_TeamList != null && s_LeagueList != null)
			{
				s_TeamList.LinkLeague(s_LeagueList);
			}
			if ((fifaObjects & EFifaObjects.FifaTournament) != 0)
			{
				s_CompetitionObjects = new CompobjList(m_GameDir, m_FifaDb);
			}
			if (s_CompetitionObjects != null)
			{
				s_CompetitionObjects.Link();
			}
		}

		public static void LoadLists()
		{
			LoadLists((EFifaObjects)26214399);
		}

		public static void SaveLists()
		{
			if (s_RoleList != null)
			{
				s_RoleList.Save(m_FifaDb);
			}
			if (s_FormationList != null)
			{
				s_FormationList.Save(m_FifaDb);
			}
			if (s_CountryList != null)
			{
				s_CountryList.Save(m_FifaDb);
			}
			if (s_KitList != null)
			{
				s_KitList.Save(m_FifaDb);
			}
			if (s_RefereeList != null)
			{
				s_RefereeList.Save(m_FifaDb);
			}
			if (s_LeagueList != null)
			{
				s_LeagueList.Save(m_FifaDb);
			}
			if (s_TeamList != null)
			{
				s_TeamList.Save(m_FifaDb);
			}
			if (s_PlayerList != null)
			{
				s_PlayerNamesList.ClearUsedFlags();
				s_PlayerList.Save(m_FifaDb);
				s_CareerFirstNameList.Save(m_FifaDb);
				s_CareerLastNameList.Save(m_FifaDb);
				s_CareerCommonNameList.Save(m_FifaDb);
				s_PlayerNamesList.Save(m_FifaDb);
				s_NameDictionary.Save(m_FifaDb);
			}
			if (s_StadiumList != null)
			{
				s_StadiumList.Save(m_FifaDb);
			}
			if (s_CompetitionObjects != null)
			{
				s_CompetitionObjects.Save(m_GameDir, m_FifaDb);
			}
			if (s_BallList != null)
			{
				s_BallList.Save(m_FifaDb);
			}
			if (s_ShoesList != null)
			{
				s_ShoesList.Save(m_FifaDb);
			}
		}

		public static void SaveLists(EFifaObjects fifaObjects)
		{
			if (s_RoleList != null && (fifaObjects & EFifaObjects.FifaRole) != 0)
			{
				s_RoleList.Save(m_FifaDb);
			}
			if (s_FormationList != null && (fifaObjects & EFifaObjects.FifaFormation) != 0)
			{
				s_FormationList.Save(m_FifaDb);
			}
			if (s_CountryList != null && (fifaObjects & EFifaObjects.FifaCountry) != 0)
			{
				s_CountryList.Save(m_FifaDb);
			}
			if (s_KitList != null && (fifaObjects & EFifaObjects.FifaKit) != 0)
			{
				s_KitList.Save(m_FifaDb);
			}
			if (s_RefereeList != null && (fifaObjects & EFifaObjects.FifaReferee) != 0)
			{
				s_RefereeList.Save(m_FifaDb);
			}
			if (s_LeagueList != null && (fifaObjects & EFifaObjects.FifaLeague) != 0)
			{
				s_LeagueList.Save(m_FifaDb);
			}
			if (s_TeamList != null && (fifaObjects & EFifaObjects.FifaTeam) != 0)
			{
				s_TeamList.Save(m_FifaDb);
			}
			if (s_PlayerList != null && (fifaObjects & EFifaObjects.FifaPlayer) != 0)
			{
				s_PlayerNamesList.ClearUsedFlags();
				s_PlayerList.Save(m_FifaDb);
				s_CareerFirstNameList.Save(m_FifaDb);
				s_CareerLastNameList.Save(m_FifaDb);
				s_CareerCommonNameList.Save(m_FifaDb);
				s_PlayerNamesList.Save(m_FifaDb);
				s_NameDictionary.Save(m_FifaDb);
			}
			if (s_StadiumList != null && (fifaObjects & EFifaObjects.FifaStadium) != 0)
			{
				s_StadiumList.Save(m_FifaDb);
			}
			if (s_CompetitionObjects != null && (fifaObjects & EFifaObjects.FifaTournament) != 0)
			{
				s_CompetitionObjects.Save(m_GameDir, m_FifaDb);
			}
			if (s_BallList != null && (fifaObjects & EFifaObjects.FifaBall) != 0)
			{
				s_BallList.Save(m_FifaDb);
			}
			if (s_ShoesList != null && (fifaObjects & EFifaObjects.FifaShoes) != 0)
			{
				s_ShoesList.Save(m_FifaDb);
			}
		}

		public static void SaveCareerLists()
		{
			if (s_RoleList != null)
			{
				s_RoleList.Save(m_FifaDb);
			}
		}

		public static ArrayList FindMissedFiles()
		{
			ArrayList arrayList = new ArrayList();
			if (arrayList.Count == 0)
			{
				arrayList.Add("No missed files found.");
			}
			else
			{
				arrayList.Add(arrayList.Count.ToString() + " missed files found.");
			}
			return arrayList;
		}

		public static void LoadCareerLists(EFifaObjects fifaObjects)
		{
			_ = m_FifaFat;
			if (m_CareerFile != null)
			{
				if ((fifaObjects & EFifaObjects.FifaRole) != 0)
				{
					s_RoleList = new RoleList(m_FifaDb);
				}
				if ((fifaObjects & EFifaObjects.FifaLeague) != 0)
				{
					s_LeagueList = new LeagueList(m_FifaDb);
				}
			}
		}

		public static void ShowOptions()
		{
			if (m_UserOptions.ShowOptions() == DialogResult.OK)
			{
				if (m_UserOptions.m_AutoExportFolder)
				{
					m_ExportFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				}
				else
				{
					m_ExportFolder = m_UserOptions.m_ExportFolder;
				}
			}
		}
	}
}
