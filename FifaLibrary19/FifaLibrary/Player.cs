using Microsoft.DirectX.Direct3D;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary
{
	public class Player : IdObject
	{
		private static int[,] c_RolesMap = new int[28, 28]
		{
			{
				100,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0
			},
			{
				0,
				100,
				30,
				85,
				90,
				95,
				90,
				85,
				80,
				50,
				50,
				50,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				50,
				100,
				80,
				70,
				50,
				50,
				50,
				75,
				5,
				5,
				5,
				50,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				80,
				50,
				100,
				95,
				90,
				85,
				80,
				50,
				50,
				50,
				30,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				85,
				40,
				95,
				100,
				95,
				90,
				85,
				50,
				50,
				50,
				30,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				90,
				30,
				90,
				95,
				100,
				95,
				90,
				30,
				50,
				50,
				50,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				85,
				20,
				85,
				90,
				95,
				100,
				95,
				60,
				30,
				30,
				50,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				80,
				20,
				80,
				85,
				90,
				95,
				100,
				70,
				30,
				30,
				50,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				50,
				75,
				70,
				50,
				50,
				70,
				80,
				100,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				50,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				5,
				20,
				15,
				10,
				5,
				5,
				5,
				5,
				100,
				90,
				85,
				90,
				85,
				80,
				75,
				70,
				25,
				25,
				25,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				5,
				10,
				15,
				20,
				10,
				5,
				5,
				5,
				95,
				100,
				95,
				80,
				85,
				90,
				85,
				80,
				25,
				25,
				25,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				5,
				5,
				5,
				10,
				15,
				20,
				15,
				10,
				85,
				90,
				100,
				70,
				75,
				80,
				95,
				90,
				25,
				25,
				25,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				85,
				70,
				60,
				100,
				95,
				90,
				85,
				80,
				90,
				70,
				70,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				90,
				70,
				60,
				95,
				100,
				95,
				90,
				85,
				85,
				80,
				75,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				80,
				90,
				80,
				90,
				95,
				100,
				95,
				90,
				80,
				90,
				80,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				60,
				80,
				90,
				85,
				90,
				95,
				100,
				95,
				75,
				80,
				85,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				5,
				40,
				5,
				5,
				5,
				5,
				30,
				70,
				60,
				70,
				80,
				80,
				85,
				90,
				95,
				100,
				70,
				70,
				90,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				25,
				25,
				25,
				75,
				80,
				75,
				70,
				65,
				100,
				95,
				90,
				90,
				85,
				80,
				60,
				20,
				20,
				20,
				20
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				25,
				25,
				25,
				70,
				75,
				80,
				75,
				70,
				95,
				100,
				95,
				85,
				90,
				85,
				20,
				20,
				30,
				20,
				20
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				25,
				25,
				25,
				65,
				70,
				75,
				80,
				75,
				90,
				95,
				100,
				80,
				85,
				90,
				20,
				20,
				20,
				20,
				60
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				15,
				10,
				5,
				10,
				15,
				55,
				55,
				50,
				100,
				95,
				90,
				70,
				100,
				95,
				90,
				55
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				15,
				10,
				5,
				10,
				15,
				50,
				60,
				50,
				95,
				100,
				95,
				50,
				95,
				100,
				95,
				50
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				15,
				10,
				5,
				10,
				15,
				50,
				55,
				55,
				90,
				95,
				100,
				55,
				90,
				95,
				100,
				70
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				50,
				10,
				5,
				10,
				15,
				60,
				40,
				40,
				60,
				50,
				50,
				100,
				90,
				80,
				85,
				95
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				15,
				10,
				5,
				10,
				15,
				40,
				40,
				40,
				50,
				50,
				50,
				70,
				100,
				95,
				95,
				70
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				15,
				10,
				5,
				10,
				15,
				40,
				40,
				40,
				50,
				50,
				50,
				60,
				95,
				100,
				95,
				60
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				15,
				10,
				5,
				10,
				15,
				40,
				40,
				40,
				50,
				50,
				50,
				70,
				95,
				95,
				100,
				70
			},
			{
				0,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				5,
				15,
				10,
				5,
				10,
				50,
				40,
				40,
				60,
				50,
				50,
				60,
				95,
				85,
				80,
				90,
				100
			}
		};

		private static int[] c_GenericModels = new int[147]
		{
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7,
			8,
			9,
			10,
			11,
			12,
			13,
			14,
			15,
			16,
			17,
			18,
			19,
			20,
			21,
			22,
			23,
			24,
			25,
			2000,
			2001,
			2002,
			2003,
			2004,
			2005,
			2006,
			2007,
			2008,
			2009,
			2010,
			2012,
			2013,
			2014,
			3500,
			3501,
			3502,
			3503,
			3504,
			3505,
			4000,
			4001,
			4002,
			4003,
			500,
			501,
			502,
			503,
			504,
			505,
			506,
			507,
			508,
			509,
			510,
			511,
			512,
			513,
			514,
			515,
			516,
			517,
			518,
			519,
			520,
			521,
			522,
			1500,
			1501,
			1502,
			1503,
			1504,
			1505,
			1506,
			1507,
			1508,
			1509,
			1510,
			1511,
			1512,
			1513,
			1514,
			1515,
			1516,
			1517,
			1518,
			1519,
			1520,
			1521,
			1522,
			1523,
			1524,
			1525,
			1526,
			1527,
			1528,
			2500,
			2501,
			2502,
			2503,
			2504,
			2505,
			2506,
			2507,
			2508,
			2509,
			2510,
			2511,
			2512,
			1000,
			1001,
			1002,
			1003,
			1004,
			1005,
			1006,
			1007,
			1008,
			1009,
			1010,
			1011,
			1012,
			1013,
			1014,
			1015,
			1016,
			1017,
			1018,
			3000,
			3001,
			3002,
			3003,
			3004,
			3005,
			4500,
			4501,
			4502,
			5000,
			5001,
			5002,
			5003
		};

		private static int[,] c_Attributes = new int[28, 4]
		{
			{
				99,
				20,
				20,
				20
			},
			{
				20,
				99,
				70,
				50
			},
			{
				20,
				90,
				70,
				50
			},
			{
				20,
				95,
				75,
				50
			},
			{
				20,
				99,
				75,
				50
			},
			{
				20,
				99,
				75,
				50
			},
			{
				20,
				99,
				75,
				50
			},
			{
				20,
				95,
				75,
				50
			},
			{
				20,
				90,
				70,
				50
			},
			{
				20,
				90,
				90,
				70
			},
			{
				20,
				90,
				90,
				70
			},
			{
				20,
				90,
				90,
				70
			},
			{
				20,
				75,
				99,
				85
			},
			{
				20,
				75,
				99,
				85
			},
			{
				20,
				75,
				99,
				85
			},
			{
				20,
				75,
				99,
				85
			},
			{
				20,
				75,
				99,
				85
			},
			{
				20,
				50,
				99,
				90
			},
			{
				20,
				50,
				99,
				90
			},
			{
				20,
				50,
				99,
				90
			},
			{
				20,
				50,
				80,
				99
			},
			{
				20,
				50,
				80,
				99
			},
			{
				20,
				50,
				80,
				99
			},
			{
				20,
				70,
				90,
				95
			},
			{
				20,
				50,
				80,
				99
			},
			{
				20,
				50,
				80,
				99
			},
			{
				20,
				50,
				80,
				99
			},
			{
				20,
				70,
				90,
				95
			}
		};

		public static Color[] s_GenericColors = new Color[13]
		{
			Color.FromArgb(180, 156, 114),
			Color.FromArgb(32, 32, 32),
			Color.FromArgb(135, 126, 100),
			Color.FromArgb(42, 35, 24),
			Color.FromArgb(168, 160, 128),
			Color.FromArgb(111, 91, 69),
			Color.FromArgb(75, 63, 49),
			Color.FromArgb(120, 85, 58),
			Color.FromArgb(160, 162, 155),
			Color.FromArgb(110, 115, 121),
			Color.FromArgb(77, 115, 70),
			Color.FromArgb(51, 80, 126),
			Color.FromArgb(110, 40, 30)
		};

		public static int s_GenericColorsDivisor = 96;

		private static Random m_Randomizer = new Random();

		private static PlayerNames s_PlayerNames = null;

		private Bitmap m_EyesTextureBitmap;

		private Bitmap[] m_FaceTextureBitmaps;

		private Bitmap m_HairColorTextureBitmap;

		private Bitmap m_HairAlfaTextureBitmap;

		private Rx3File m_HeadModelFile;

		private Rx3File m_HairModelFile;

		public static Model3D s_Model3DHead = null;

		public static Model3D s_Model3DEyes = null;

		public static Model3D s_Model3DHairPart4 = null;

		public static Model3D s_Model3DHairPart5 = null;

		public bool m_HasSpecificPhoto;

		public TeamList m_PlayingForTeams = new TeamList();

		public int m_assetid;

		private string m_firstname;

		private string m_lastname;

		private string m_audioname;

		private int m_commentaryid;

		public string m_commonname;

		public string m_playerjerseyname;

		private int m_firstnameid;

		private int m_lastnameid;

		private int m_commonnameid;

		private int m_playerjerseynameid;

		private DateTime m_birthdate;

		public DateTime m_playerjointeamdate;

		private int m_contractvaliduntil;

		private int m_height;

		private int m_weight;

		private int m_nationality;

		private Country m_Country;

		private int m_eyecolorcode;

		private int m_eyebrowcode;

		private int m_bodytypecode;

		private int m_hairtypecode;

		private int m_headtypecode;

		private int m_headclasscode;

		private int m_haircolorcode;

		private int m_facialhairtypecode;

		private int m_facialhaircolorcode;

		private int m_sideburnscode;

		private int m_skintypecode;

		private int m_skintonecode;

		private int m_jerseysleevelengthcode;

		private int m_jerseystylecode;

		private int m_hasseasonaljersey;

		private int m_animfreekickstartposcode;

		private int m_animpenaltieskickstylecode;

		private int m_animpenaltiesmotionstylecode;

		private int m_animpenaltiesstartposcode;

		private int m_accessorycode1;

		private int m_accessorycolourcode1;

		private int m_accessorycode2;

		private int m_accessorycolourcode2;

		private int m_accessorycode3;

		private int m_accessorycolourcode3;

		private int m_accessorycode4;

		private int m_accessorycolourcode4;

		private int m_shoetypecode;

		private int m_shoedesigncode;

		private int m_shoecolorcode1;

		private int m_shoecolorcode2;

		private int m_preferredposition1;

		private int m_preferredposition2;

		private int m_preferredposition3;

		private int m_preferredposition4;

		private int m_preferredfoot;

		private int m_weakfootabilitytypecode;

		private int m_acceleration;

		private int m_aggression;

		private int m_gkglovetypecode;

		private int m_agility;

		private int m_balance;

		private int m_gkkicking;

		private int m_gkkickstyle;

		private int m_jumping;

		private int m_penalties;

		private int m_vision;

		private int m_volleys;

		private int m_skillmoves;

		private int m_usercaneditname;

		private int m_sprintspeed;

		private int m_stamina;

		private int m_strength;

		private int m_marking;

		private int m_standingtackle;

		private int m_slidingtackle;

		private int m_ballcontrol;

		private int m_dribbling;

		private int m_crossing;

		private int m_headingaccuracy;

		private int m_shortpassing;

		private int m_longpassing;

		private int m_longshots;

		private int m_finishing;

		private int m_shotpower;

		private int m_reactions;

		private int m_gkreflexes;

		private int m_gkhandling;

		private int m_gkdiving;

		private int m_gkpositioning;

		private int m_freekickaccuracy;

		private int m_potential;

		private int m_positioning;

		private int m_overallrating;

		private bool m_Inflexible;

		private bool m_GkOneOnOne;

		private bool m_CrowdFavorite;

		private bool m_SecondWind;

		private bool m_AcrobaticClearance;

		private bool m_Longthrows;

		private bool m_PowerfulFreeKicks;

		private bool m_Diver;

		private bool m_InjuryFree;

		private bool m_InjuryProne;

		private bool m_AvoidsWeakFoot;

		private bool m_Divesintotackles;

		private bool m_BeatDefensiveLine;

		private bool m_Selfish;

		private bool m_Leadership;

		private bool m_ArguesWithOfficials;

		private bool m_Earlycrosser;

		private bool m_FinesseShot;

		private bool m_Flair;

		private bool m_LongPasser;

		private bool m_LongShotTaker;

		private bool m_Technicaldribbler;

		private bool m_Playmaker;

		private bool m_Pushesupforcorners;

		private bool m_Puncher;

		private bool m_GkLongThrower;

		private bool m_PowerHeader;

		private bool m_GiantThrow;

		private bool m_OutsideFootShot;

		private bool m_SwervePasser;

		private bool m_HighClubIdentification;

		private bool m_TeamPlayer;

		private bool m_FancyFeet;

		private bool m_FancyPasses;

		private bool m_FancyFlicks;

		private bool m_StutterPenalty;

		private bool m_ChipperPenalty;

		private bool m_BycicleKick;

		private bool m_DivingHeader;

		private bool m_DrivenPass;

		private bool m_GkFlatKick;

		private int m_curve;

		private int m_internationalrep;

		private bool m_gender;

		private int m_emotion;

		private int m_finishingcode1;

		private int m_finishingcode2;

		private int m_runningcode1;

		private int m_runningcode2;

		private int m_gksavetype;

		private int m_faceposercode;

		private int m_isretiring;

		private int m_socklengthcode;

		private bool m_hashighqualityhead;

		private int m_attackingworkrate;

		private int m_defensiveworkrate;

		private bool m_shortstyle;

		private int m_interceptions;

		private bool m_jerseyfit;

		private int m_preferredNumber;

		private int m_teamidloanedfrom;

		private Team m_TeamLoanedFrom;

		private int m_previousteamid;

		private Team m_PreviousTeam;

		private bool m_IsLoaned;

		private DateTime m_loandateend;

		private static float[] c_MarketValuesGoalkeeper = new float[40]
		{
			0.01f,
			0.02f,
			0.03f,
			0.04f,
			0.05f,
			0.06f,
			0.07f,
			0.075f,
			0.08f,
			0.09f,
			0.1f,
			0.15f,
			0.2f,
			0.25f,
			0.3f,
			0.35f,
			0.4f,
			0.45f,
			0.5f,
			0.75f,
			1f,
			1.25f,
			1.5f,
			2f,
			2.75f,
			3.5f,
			4.25f,
			5f,
			5.5f,
			6f,
			7f,
			8f,
			10f,
			14f,
			18f,
			20f,
			22f,
			24f,
			32f,
			40f
		};

		private static float[] c_MarketValuesDefender = new float[40]
		{
			0.01f,
			0.02f,
			0.025f,
			0.03f,
			0.04f,
			0.05f,
			0.06f,
			0.075f,
			0.085f,
			0.1f,
			0.125f,
			0.15f,
			0.2f,
			0.25f,
			0.33f,
			0.41f,
			0.5f,
			0.625f,
			0.75f,
			0.825f,
			1f,
			1.5f,
			2f,
			2.7f,
			3.4f,
			4f,
			5f,
			6f,
			7f,
			8f,
			10f,
			14f,
			20f,
			24f,
			28f,
			32f,
			36f,
			40f,
			50f,
			60f
		};

		private static float[] c_MarketValuesMidfielder = new float[40]
		{
			0.01f,
			0.02f,
			0.025f,
			0.03f,
			0.04f,
			0.05f,
			0.06f,
			0.075f,
			0.085f,
			0.1f,
			0.075f,
			0.1f,
			0.15f,
			0.25f,
			0.35f,
			0.5f,
			0.65f,
			0.825f,
			1f,
			1.25f,
			1.5f,
			2f,
			3f,
			4f,
			5f,
			6f,
			8f,
			10f,
			15f,
			8f,
			20f,
			25f,
			30f,
			33f,
			37f,
			50f,
			60f,
			65f,
			75f,
			90f
		};

		private static float[] c_MarketValuesAttacker = new float[40]
		{
			0.01f,
			0.02f,
			0.025f,
			0.03f,
			0.04f,
			0.05f,
			0.06f,
			0.07f,
			0.08f,
			0.09f,
			0.1f,
			0.125f,
			0.15f,
			0.2f,
			0.25f,
			0.35f,
			0.5f,
			0.75f,
			1f,
			1.25f,
			1.5f,
			2f,
			2.75f,
			3.5f,
			4f,
			6f,
			8f,
			10f,
			12f,
			15f,
			20f,
			25f,
			30f,
			35f,
			40f,
			50f,
			65f,
			80f,
			90f,
			99f
		};

		private static int[] s_MeanLevels = new int[7]
		{
			52,
			59,
			66,
			72,
			77,
			82,
			88
		};

		public static PlayerNames PlayerNames
		{
			get
			{
				return s_PlayerNames;
			}
			set
			{
				s_PlayerNames = value;
			}
		}

		public bool HasSpecificHeadModel => m_headclasscode == 0;

		public Bitmap EyesTextureBitmap => m_EyesTextureBitmap;

		public Bitmap[] FaceTextureBitmap => m_FaceTextureBitmaps;

		public Bitmap HairColorTextureBitmap => m_HairColorTextureBitmap;

		public Bitmap HairAlfaTextureBitmap => m_HairAlfaTextureBitmap;

		public string firstname
		{
			get
			{
				return m_firstname;
			}
			set
			{
				m_firstname = value;
			}
		}

		public string lastname
		{
			get
			{
				return m_lastname;
			}
			set
			{
				m_lastname = value;
			}
		}

		public string audioname
		{
			get
			{
				return m_audioname;
			}
			set
			{
				m_audioname = value;
			}
		}

		public int commentaryid
		{
			get
			{
				return m_commentaryid;
			}
			set
			{
				m_commentaryid = value;
			}
		}

		public string Name
		{
			get
			{
				if (m_commonname != null && m_commonname != string.Empty)
				{
					return m_commonname;
				}
				return m_lastname;
			}
		}

		public string commonname
		{
			get
			{
				return m_commonname;
			}
			set
			{
				m_commonname = value;
			}
		}

		public string playerjerseyname
		{
			get
			{
				return m_playerjerseyname;
			}
			set
			{
				m_playerjerseyname = value;
			}
		}

		public int firstnameid
		{
			get
			{
				return m_firstnameid;
			}
			set
			{
				m_firstnameid = value;
			}
		}

		public int lastnameid
		{
			get
			{
				return m_lastnameid;
			}
			set
			{
				m_lastnameid = value;
			}
		}

		public int commonnameid
		{
			get
			{
				return m_commonnameid;
			}
			set
			{
				m_commonnameid = value;
			}
		}

		public int playerjerseynameid
		{
			get
			{
				return m_playerjerseynameid;
			}
			set
			{
				m_playerjerseynameid = value;
			}
		}

		public DateTime birthdate
		{
			get
			{
				return m_birthdate;
			}
			set
			{
				m_birthdate = value;
			}
		}

		public DateTime joindate
		{
			get
			{
				return m_playerjointeamdate;
			}
			set
			{
				m_playerjointeamdate = value;
			}
		}

		public int contractvaliduntil
		{
			get
			{
				return m_contractvaliduntil;
			}
			set
			{
				m_contractvaliduntil = value;
			}
		}

		public int height
		{
			get
			{
				return m_height;
			}
			set
			{
				m_height = value;
			}
		}

		public int weight
		{
			get
			{
				return m_weight;
			}
			set
			{
				m_weight = value;
			}
		}

		public int nationality
		{
			get
			{
				return m_nationality;
			}
			set
			{
				m_nationality = value;
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
					m_nationality = Country.Id;
				}
				else
				{
					m_nationality = 0;
				}
			}
		}

		public int eyecolorcode
		{
			get
			{
				return m_eyecolorcode;
			}
			set
			{
				m_eyecolorcode = value;
				m_EyesTextureBitmap = null;
			}
		}

		public int eyebrowcode
		{
			get
			{
				return m_eyebrowcode;
			}
			set
			{
				m_eyebrowcode = value;
				m_FaceTextureBitmaps = null;
			}
		}

		public int bodytypecode
		{
			get
			{
				return m_bodytypecode;
			}
			set
			{
				m_bodytypecode = value;
			}
		}

		public int hairtypecode
		{
			get
			{
				return m_hairtypecode;
			}
			set
			{
				m_hairtypecode = value;
				m_HairModelFile = null;
			}
		}

		public int headtypecode
		{
			get
			{
				return m_headtypecode;
			}
			set
			{
				m_headtypecode = value;
				m_HeadModelFile = null;
			}
		}

		public int headclasscode
		{
			get
			{
				return m_headclasscode;
			}
			set
			{
				m_headclasscode = value;
				m_HairAlfaTextureBitmap = null;
				m_HairColorTextureBitmap = null;
				m_FaceTextureBitmaps = null;
				m_EyesTextureBitmap = null;
				m_HeadModelFile = null;
				m_HairModelFile = null;
			}
		}

		public int haircolorcode
		{
			get
			{
				return m_haircolorcode;
			}
			set
			{
				m_haircolorcode = value;
				m_HairColorTextureBitmap = null;
				m_HairAlfaTextureBitmap = null;
			}
		}

		public int facialhairtypecode
		{
			get
			{
				return m_facialhairtypecode;
			}
			set
			{
				m_facialhairtypecode = value;
				m_FaceTextureBitmaps = null;
			}
		}

		public int facialhaircolorcode
		{
			get
			{
				return m_facialhaircolorcode;
			}
			set
			{
				m_facialhaircolorcode = value;
				m_FaceTextureBitmaps = null;
			}
		}

		public int sideburnscode
		{
			get
			{
				return m_sideburnscode;
			}
			set
			{
				m_sideburnscode = value;
				m_FaceTextureBitmaps = null;
			}
		}

		public int skintypecode
		{
			get
			{
				return m_skintypecode;
			}
			set
			{
				m_skintypecode = value;
				m_FaceTextureBitmaps = null;
			}
		}

		public int skintonecode
		{
			get
			{
				return m_skintonecode;
			}
			set
			{
				m_skintonecode = value;
				m_FaceTextureBitmaps = null;
			}
		}

		public int jerseysleevelengthcode
		{
			get
			{
				return m_jerseysleevelengthcode;
			}
			set
			{
				m_jerseysleevelengthcode = value;
			}
		}

		public int jerseystylecode
		{
			get
			{
				return m_jerseystylecode;
			}
			set
			{
				m_jerseystylecode = value;
			}
		}

		public int hasseasonaljersey
		{
			get
			{
				return m_hasseasonaljersey;
			}
			set
			{
				m_hasseasonaljersey = value;
			}
		}

		public int animfreekickstartposcode
		{
			get
			{
				return m_animfreekickstartposcode;
			}
			set
			{
				m_animfreekickstartposcode = value;
			}
		}

		public int animpenaltieskickstylecode
		{
			get
			{
				return m_animpenaltieskickstylecode;
			}
			set
			{
				m_animpenaltieskickstylecode = value;
			}
		}

		public int animpenaltiesmotionstylecode
		{
			get
			{
				return m_animpenaltiesmotionstylecode;
			}
			set
			{
				m_animpenaltiesmotionstylecode = value;
			}
		}

		public int animpenaltiesstartposcode
		{
			get
			{
				return m_animpenaltiesstartposcode;
			}
			set
			{
				m_animpenaltiesstartposcode = value;
			}
		}

		public int accessorycode1
		{
			get
			{
				return m_accessorycode1;
			}
			set
			{
				m_accessorycode1 = value;
			}
		}

		public int accessorycolourcode1
		{
			get
			{
				return m_accessorycolourcode1;
			}
			set
			{
				m_accessorycolourcode1 = value;
			}
		}

		public int accessorycode2
		{
			get
			{
				return m_accessorycode2;
			}
			set
			{
				m_accessorycode2 = value;
			}
		}

		public int accessorycolourcode2
		{
			get
			{
				return m_accessorycolourcode2;
			}
			set
			{
				m_accessorycolourcode2 = value;
			}
		}

		public int accessorycode3
		{
			get
			{
				return m_accessorycode3;
			}
			set
			{
				m_accessorycode3 = value;
			}
		}

		public int accessorycolourcode3
		{
			get
			{
				return m_accessorycolourcode3;
			}
			set
			{
				m_accessorycolourcode3 = value;
			}
		}

		public int accessorycode4
		{
			get
			{
				return m_accessorycode4;
			}
			set
			{
				m_accessorycode4 = value;
			}
		}

		public int accessorycolourcode4
		{
			get
			{
				return m_accessorycolourcode4;
			}
			set
			{
				m_accessorycolourcode4 = value;
			}
		}

		public int shoetypecode
		{
			get
			{
				return m_shoetypecode;
			}
			set
			{
				m_shoetypecode = value;
			}
		}

		public int shoedesigncode
		{
			get
			{
				return m_shoedesigncode;
			}
			set
			{
				m_shoedesigncode = value;
			}
		}

		public int shoecolorcode1
		{
			get
			{
				return m_shoecolorcode1;
			}
			set
			{
				m_shoecolorcode1 = value;
			}
		}

		public int shoecolorcode2
		{
			get
			{
				return m_shoecolorcode2;
			}
			set
			{
				m_shoecolorcode2 = value;
			}
		}

		public int preferredposition1
		{
			get
			{
				return m_preferredposition1;
			}
			set
			{
				m_preferredposition1 = value;
			}
		}

		public int preferredposition2
		{
			get
			{
				return m_preferredposition2;
			}
			set
			{
				m_preferredposition2 = value;
			}
		}

		public int preferredposition3
		{
			get
			{
				return m_preferredposition3;
			}
			set
			{
				m_preferredposition3 = value;
			}
		}

		public int preferredposition4
		{
			get
			{
				return m_preferredposition4;
			}
			set
			{
				m_preferredposition4 = value;
			}
		}

		public int preferredfoot
		{
			get
			{
				return m_preferredfoot;
			}
			set
			{
				m_preferredfoot = value;
			}
		}

		public int weakfootabilitytypecode
		{
			get
			{
				return m_weakfootabilitytypecode - 1;
			}
			set
			{
				m_weakfootabilitytypecode = value + 1;
			}
		}

		public int acceleration
		{
			get
			{
				return m_acceleration;
			}
			set
			{
				m_acceleration = value;
			}
		}

		public int aggression
		{
			get
			{
				return m_aggression;
			}
			set
			{
				m_aggression = value;
			}
		}

		public int gkglovetypecode
		{
			get
			{
				return m_gkglovetypecode;
			}
			set
			{
				m_gkglovetypecode = value;
			}
		}

		public int agility
		{
			get
			{
				return m_agility;
			}
			set
			{
				m_agility = value;
			}
		}

		public int balance
		{
			get
			{
				return m_balance;
			}
			set
			{
				m_balance = value;
			}
		}

		public int gkkicking
		{
			get
			{
				return m_gkkicking;
			}
			set
			{
				m_gkkicking = value;
			}
		}

		public int gkkickstyle
		{
			get
			{
				return m_gkkickstyle;
			}
			set
			{
				m_gkkickstyle = value;
			}
		}

		public int jumping
		{
			get
			{
				return m_jumping;
			}
			set
			{
				m_jumping = value;
			}
		}

		public int penalties
		{
			get
			{
				return m_penalties;
			}
			set
			{
				m_penalties = value;
			}
		}

		public int vision
		{
			get
			{
				return m_vision;
			}
			set
			{
				m_vision = value;
			}
		}

		public int volleys
		{
			get
			{
				return m_volleys;
			}
			set
			{
				m_volleys = value;
			}
		}

		public int skillmoves
		{
			get
			{
				return m_skillmoves;
			}
			set
			{
				m_skillmoves = value;
			}
		}

		public int usercaneditname
		{
			get
			{
				return m_usercaneditname;
			}
			set
			{
				m_usercaneditname = value;
			}
		}

		public int sprintspeed
		{
			get
			{
				return m_sprintspeed;
			}
			set
			{
				m_sprintspeed = value;
			}
		}

		public int stamina
		{
			get
			{
				return m_stamina;
			}
			set
			{
				m_stamina = value;
			}
		}

		public int strength
		{
			get
			{
				return m_strength;
			}
			set
			{
				m_strength = value;
			}
		}

		public int marking
		{
			get
			{
				return m_marking;
			}
			set
			{
				m_marking = value;
			}
		}

		public int standingtackle
		{
			get
			{
				return m_standingtackle;
			}
			set
			{
				m_standingtackle = value;
			}
		}

		public int slidingtackle
		{
			get
			{
				return m_slidingtackle;
			}
			set
			{
				m_slidingtackle = value;
			}
		}

		public int ballcontrol
		{
			get
			{
				return m_ballcontrol;
			}
			set
			{
				m_ballcontrol = value;
			}
		}

		public int dribbling
		{
			get
			{
				return m_dribbling;
			}
			set
			{
				m_dribbling = value;
			}
		}

		public int crossing
		{
			get
			{
				return m_crossing;
			}
			set
			{
				m_crossing = value;
			}
		}

		public int headingaccuracy
		{
			get
			{
				return m_headingaccuracy;
			}
			set
			{
				m_headingaccuracy = value;
			}
		}

		public int shortpassing
		{
			get
			{
				return m_shortpassing;
			}
			set
			{
				m_shortpassing = value;
			}
		}

		public int longpassing
		{
			get
			{
				return m_longpassing;
			}
			set
			{
				m_longpassing = value;
			}
		}

		public int longshots
		{
			get
			{
				return m_longshots;
			}
			set
			{
				m_longshots = value;
			}
		}

		public int finishing
		{
			get
			{
				return m_finishing;
			}
			set
			{
				m_finishing = value;
			}
		}

		public int shotpower
		{
			get
			{
				return m_shotpower;
			}
			set
			{
				m_shotpower = value;
			}
		}

		public int reactions
		{
			get
			{
				return m_reactions;
			}
			set
			{
				m_reactions = value;
			}
		}

		public int gkreflexes
		{
			get
			{
				return m_gkreflexes;
			}
			set
			{
				m_gkreflexes = value;
			}
		}

		public int gkhandling
		{
			get
			{
				return m_gkhandling;
			}
			set
			{
				m_gkhandling = value;
			}
		}

		public int gkdiving
		{
			get
			{
				return m_gkdiving;
			}
			set
			{
				m_gkdiving = value;
			}
		}

		public int gkpositioning
		{
			get
			{
				return m_gkpositioning;
			}
			set
			{
				m_gkpositioning = value;
			}
		}

		public int freekickaccuracy
		{
			get
			{
				return m_freekickaccuracy;
			}
			set
			{
				m_freekickaccuracy = value;
			}
		}

		public int potential
		{
			get
			{
				return m_potential;
			}
			set
			{
				m_potential = value;
			}
		}

		public int positioning
		{
			get
			{
				return m_positioning;
			}
			set
			{
				m_positioning = value;
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

		public bool Inflexible
		{
			get
			{
				return m_Inflexible;
			}
			set
			{
				m_Inflexible = value;
			}
		}

		public bool GkOneOnOne
		{
			get
			{
				return m_GkOneOnOne;
			}
			set
			{
				m_GkOneOnOne = value;
			}
		}

		public bool CrowdFavorite
		{
			get
			{
				return m_CrowdFavorite;
			}
			set
			{
				m_CrowdFavorite = value;
			}
		}

		public bool SecondWind
		{
			get
			{
				return m_SecondWind;
			}
			set
			{
				m_SecondWind = value;
			}
		}

		public bool AcrobaticClearance
		{
			get
			{
				return m_AcrobaticClearance;
			}
			set
			{
				m_AcrobaticClearance = value;
			}
		}

		public bool Longthrows
		{
			get
			{
				return m_Longthrows;
			}
			set
			{
				m_Longthrows = value;
			}
		}

		public bool PowerfulFreeKicks
		{
			get
			{
				return m_PowerfulFreeKicks;
			}
			set
			{
				m_PowerfulFreeKicks = value;
			}
		}

		public bool Diver
		{
			get
			{
				return m_Diver;
			}
			set
			{
				m_Diver = value;
			}
		}

		public bool InjuryFree
		{
			get
			{
				return m_InjuryFree;
			}
			set
			{
				m_InjuryFree = value;
			}
		}

		public bool InjuryProne
		{
			get
			{
				return m_InjuryProne;
			}
			set
			{
				m_InjuryProne = value;
			}
		}

		public bool AvoidsWeakFoot
		{
			get
			{
				return m_AvoidsWeakFoot;
			}
			set
			{
				m_AvoidsWeakFoot = value;
			}
		}

		public bool Divesintotackles
		{
			get
			{
				return m_Divesintotackles;
			}
			set
			{
				m_Divesintotackles = value;
			}
		}

		public bool BeatDefensiveLine
		{
			get
			{
				return m_BeatDefensiveLine;
			}
			set
			{
				m_BeatDefensiveLine = value;
			}
		}

		public bool Selfish
		{
			get
			{
				return m_Selfish;
			}
			set
			{
				m_Selfish = value;
			}
		}

		public bool Leadership
		{
			get
			{
				return m_Leadership;
			}
			set
			{
				m_Leadership = value;
			}
		}

		public bool ArguesWithOfficials
		{
			get
			{
				return m_ArguesWithOfficials;
			}
			set
			{
				m_ArguesWithOfficials = value;
			}
		}

		public bool Earlycrosser
		{
			get
			{
				return m_Earlycrosser;
			}
			set
			{
				m_Earlycrosser = value;
			}
		}

		public bool FinesseShot
		{
			get
			{
				return m_FinesseShot;
			}
			set
			{
				m_FinesseShot = value;
			}
		}

		public bool Flair
		{
			get
			{
				return m_Flair;
			}
			set
			{
				m_Flair = value;
			}
		}

		public bool LongPasser
		{
			get
			{
				return m_LongPasser;
			}
			set
			{
				m_LongPasser = value;
			}
		}

		public bool LongShotTaker
		{
			get
			{
				return m_LongShotTaker;
			}
			set
			{
				m_LongShotTaker = value;
			}
		}

		public bool Technicaldribbler
		{
			get
			{
				return m_Technicaldribbler;
			}
			set
			{
				m_Technicaldribbler = value;
			}
		}

		public bool Playmaker
		{
			get
			{
				return m_Playmaker;
			}
			set
			{
				m_Playmaker = value;
			}
		}

		public bool Pushesupforcorners
		{
			get
			{
				return m_Pushesupforcorners;
			}
			set
			{
				m_Pushesupforcorners = value;
			}
		}

		public bool Puncher
		{
			get
			{
				return m_Puncher;
			}
			set
			{
				m_Puncher = value;
			}
		}

		public bool GkLongThrower
		{
			get
			{
				return m_GkLongThrower;
			}
			set
			{
				m_GkLongThrower = value;
			}
		}

		public bool PowerHeader
		{
			get
			{
				return m_PowerHeader;
			}
			set
			{
				m_PowerHeader = value;
			}
		}

		public bool GiantThrow
		{
			get
			{
				return m_GiantThrow;
			}
			set
			{
				m_GiantThrow = value;
			}
		}

		public bool OutsideFootShot
		{
			get
			{
				return m_OutsideFootShot;
			}
			set
			{
				m_OutsideFootShot = value;
			}
		}

		public bool SwervePasser
		{
			get
			{
				return m_SwervePasser;
			}
			set
			{
				m_SwervePasser = value;
			}
		}

		public bool HighClubIdentification
		{
			get
			{
				return m_HighClubIdentification;
			}
			set
			{
				m_HighClubIdentification = value;
			}
		}

		public bool TeamPlayer
		{
			get
			{
				return m_TeamPlayer;
			}
			set
			{
				m_TeamPlayer = value;
			}
		}

		public bool FancyFeet
		{
			get
			{
				return m_FancyFeet;
			}
			set
			{
				m_FancyFeet = value;
			}
		}

		public bool FancyPasses
		{
			get
			{
				return m_FancyPasses;
			}
			set
			{
				m_FancyPasses = value;
			}
		}

		public bool FancyFlicks
		{
			get
			{
				return m_FancyFlicks;
			}
			set
			{
				m_FancyFlicks = value;
			}
		}

		public bool StutterPenalty
		{
			get
			{
				return m_StutterPenalty;
			}
			set
			{
				m_StutterPenalty = value;
			}
		}

		public bool ChipperPenalty
		{
			get
			{
				return m_ChipperPenalty;
			}
			set
			{
				m_ChipperPenalty = value;
			}
		}

		public bool BycicleKick
		{
			get
			{
				return m_BycicleKick;
			}
			set
			{
				m_BycicleKick = value;
			}
		}

		public bool DivingHeader
		{
			get
			{
				return m_DivingHeader;
			}
			set
			{
				m_DivingHeader = value;
			}
		}

		public bool DrivenPass
		{
			get
			{
				return m_DrivenPass;
			}
			set
			{
				m_DrivenPass = value;
			}
		}

		public bool GkFlatKick
		{
			get
			{
				return m_GkFlatKick;
			}
			set
			{
				m_GkFlatKick = value;
			}
		}

		public int curve
		{
			get
			{
				return m_curve;
			}
			set
			{
				m_curve = value;
			}
		}

		public int internationalrep
		{
			get
			{
				return m_internationalrep;
			}
			set
			{
				m_internationalrep = value;
			}
		}

		public bool Female
		{
			get
			{
				return m_gender;
			}
			set
			{
				m_gender = value;
			}
		}

		public bool Male
		{
			get
			{
				return !m_gender;
			}
			set
			{
				m_gender = !value;
			}
		}

		public int emotion
		{
			get
			{
				return m_emotion;
			}
			set
			{
				m_emotion = value;
			}
		}

		public int finishingcode1
		{
			get
			{
				return m_finishingcode1;
			}
			set
			{
				m_finishingcode1 = value;
			}
		}

		public int finishingcode2
		{
			get
			{
				return m_finishingcode2;
			}
			set
			{
				m_finishingcode2 = value;
			}
		}

		public int runningcode1
		{
			get
			{
				return m_runningcode1;
			}
			set
			{
				m_runningcode1 = value;
			}
		}

		public int runningcode2
		{
			get
			{
				return m_runningcode2;
			}
			set
			{
				m_runningcode2 = value;
			}
		}

		public int gksavetype
		{
			get
			{
				return m_gksavetype;
			}
			set
			{
				m_gksavetype = value;
			}
		}

		public int faceposercode
		{
			get
			{
				return m_faceposercode;
			}
			set
			{
				m_faceposercode = value;
			}
		}

		public int isretiring
		{
			get
			{
				return m_isretiring;
			}
			set
			{
				m_isretiring = value;
			}
		}

		public int socklengthcode
		{
			get
			{
				return m_socklengthcode;
			}
			set
			{
				m_socklengthcode = value;
			}
		}

		public bool hashighqualityhead
		{
			get
			{
				return m_hashighqualityhead;
			}
			set
			{
				m_hashighqualityhead = value;
			}
		}

		public int attackingworkrate
		{
			get
			{
				return m_attackingworkrate;
			}
			set
			{
				m_attackingworkrate = value;
			}
		}

		public int defensiveworkrate
		{
			get
			{
				return m_defensiveworkrate;
			}
			set
			{
				m_defensiveworkrate = value;
			}
		}

		public bool TrainingPants
		{
			get
			{
				return m_shortstyle;
			}
			set
			{
				m_shortstyle = value;
			}
		}

		public int interceptions
		{
			get
			{
				return m_interceptions;
			}
			set
			{
				m_interceptions = value;
			}
		}

		public bool jerseyfit
		{
			get
			{
				return m_jerseyfit;
			}
			set
			{
				m_jerseyfit = value;
			}
		}

		public int preferredNumber
		{
			get
			{
				return m_preferredNumber;
			}
			set
			{
				m_preferredNumber = value;
			}
		}

		public Team TeamLoanedFrom
		{
			get
			{
				return m_TeamLoanedFrom;
			}
			set
			{
				m_TeamLoanedFrom = value;
				m_teamidloanedfrom = ((m_TeamLoanedFrom != null) ? m_TeamLoanedFrom.Id : 0);
			}
		}

		public Team PreviousTeam
		{
			get
			{
				return m_PreviousTeam;
			}
			set
			{
				m_PreviousTeam = value;
				m_previousteamid = ((m_PreviousTeam != null) ? m_PreviousTeam.Id : 0);
			}
		}

		public bool IsLoaned
		{
			get
			{
				return m_IsLoaned;
			}
			set
			{
				m_IsLoaned = value;
			}
		}

		public DateTime loandateend
		{
			get
			{
				return m_loandateend;
			}
			set
			{
				m_loandateend = value;
			}
		}

		public Team IsInTopLeague()
		{
			return m_PlayingForTeams.IsInTopLeague();
		}

		public string FastRename(string newName)
		{
			string value = " ";
			string firstname = null;
			string text = null;
			if (newName == null)
			{
				return null;
			}
			if (newName.Contains(value))
			{
				int num = newName.IndexOf(' ');
				firstname = newName.Substring(0, num);
				text = newName.Substring(num + 1);
				newName = null;
			}
			if (newName == null)
			{
				m_firstname = firstname;
				m_commonname = string.Empty;
				m_lastname = text;
				m_playerjerseyname = text;
				m_audioname = text;
				m_commentaryid = -1;
				return text;
			}
			if (m_commonname != null && m_commonname != string.Empty)
			{
				string commonname = m_commonname;
				m_commonname = newName;
				m_playerjerseyname = m_commonname;
				m_audioname = m_commonname;
				m_commentaryid = -1;
				if (m_firstname == commonname)
				{
					m_firstname = m_commonname;
				}
				if (m_lastname == commonname)
				{
					m_lastname = m_commonname;
				}
				return m_commonname;
			}
			_ = m_lastname;
			m_lastname = newName;
			m_commonname = string.Empty;
			m_playerjerseyname = m_lastname;
			m_audioname = m_lastname;
			m_commentaryid = -1;
			return m_lastname;
		}

		public string WabName()
		{
			if (m_commonname != null && m_commonname != string.Empty)
			{
				return m_commonname;
			}
			if (m_firstname != null && m_firstname != string.Empty)
			{
				return m_firstname + " " + m_lastname;
			}
			return m_lastname;
		}

		public override string ToString()
		{
			if (m_commonname != null && m_commonname != string.Empty)
			{
				return m_commonname;
			}
			if (m_firstname != null && m_firstname != string.Empty)
			{
				return m_lastname + " " + m_firstname;
			}
			return m_lastname + " [" + base.Id.ToString() + "]";
		}

		public string DatabaseString()
		{
			return ToString();
		}

		public Player(Record r)
			: base(r.IntField[FI.players_playerid])
		{
			Load(r);
		}

		public void UpdateNamesAndCommentary()
		{
			m_commentaryid = 900000;
			if (!PlayerNames.TryGetValue(m_firstnameid, out m_firstname, isUsed: true))
			{
				m_firstname = string.Empty;
			}
			if (!PlayerNames.TryGetValue(m_lastnameid, out m_lastname, out int commentaryid, isUsed: true))
			{
				m_lastname = string.Empty;
			}
			if (!PlayerNames.TryGetValue(m_commonnameid, out m_commonname, out int commentaryid2, isUsed: true))
			{
				m_commonname = string.Empty;
			}
			if (m_commonname != string.Empty)
			{
				m_commentaryid = commentaryid2;
				m_audioname = m_commonname;
			}
			else if (m_lastname != string.Empty)
			{
				m_commentaryid = commentaryid;
				m_audioname = m_lastname;
			}
			else
			{
				m_commentaryid = 900000;
				m_audioname = string.Empty;
			}
			if (!PlayerNames.TryGetValue(m_playerjerseynameid, out m_playerjerseyname, isUsed: true))
			{
				m_playerjerseyname = string.Empty;
			}
		}

		public void Load(Record r)
		{
			m_headclasscode = r.GetAndCheckIntField(FI.players_headclasscode);
			m_firstnameid = r.IntField[FI.players_firstnameid];
			m_lastnameid = r.IntField[FI.players_lastnameid];
			m_commonnameid = r.IntField[FI.players_commonnameid];
			m_playerjerseynameid = r.IntField[FI.players_playerjerseynameid];
			UpdateNamesAndCommentary();
			DateTime birthdate = FifaUtil.ConvertToDate(r.GetAndCheckIntField(FI.players_birthdate));
			if (birthdate.Year < 1900)
			{
				birthdate = new DateTime(1980, 1, 1);
			}
			m_birthdate = birthdate;
			m_playerjointeamdate = FifaUtil.ConvertToDate(r.GetAndCheckIntField(FI.players_playerjointeamdate));
			m_contractvaliduntil = r.GetAndCheckIntField(FI.players_contractvaliduntil);
			m_height = r.GetAndCheckIntField(FI.players_height);
			m_weight = r.GetAndCheckIntField(FI.players_weight);
			m_preferredposition1 = r.GetAndCheckIntField(FI.players_preferredposition1);
			m_preferredposition2 = r.GetAndCheckIntField(FI.players_preferredposition2);
			m_preferredposition3 = r.GetAndCheckIntField(FI.players_preferredposition3);
			m_preferredposition4 = r.GetAndCheckIntField(FI.players_preferredposition4);
			m_preferredfoot = r.GetAndCheckIntField(FI.players_preferredfoot) - 1;
			m_bodytypecode = r.GetAndCheckIntField(FI.players_bodytypecode) - 1;
			m_shoecolorcode1 = r.GetAndCheckIntField(FI.players_shoecolorcode1);
			m_shoetypecode = r.GetAndCheckIntField(FI.players_shoetypecode);
			m_jerseysleevelengthcode = r.GetAndCheckIntField(FI.players_jerseysleevelengthcode);
			m_eyecolorcode = r.GetAndCheckIntField(FI.players_eyecolorcode);
			m_eyebrowcode = r.GetAndCheckIntField(FI.players_eyebrowcode);
			m_facialhairtypecode = r.GetAndCheckIntField(FI.players_facialhairtypecode);
			m_facialhaircolorcode = r.GetAndCheckIntField(FI.players_facialhaircolorcode);
			m_hairtypecode = r.GetAndCheckIntField(FI.players_hairtypecode);
			m_haircolorcode = r.GetAndCheckIntField(FI.players_haircolorcode);
			m_headtypecode = r.GetAndCheckIntField(FI.players_headtypecode);
			m_sideburnscode = r.GetAndCheckIntField(FI.players_sideburnscode);
			m_skintypecode = r.GetAndCheckIntField(FI.players_skintypecode);
			m_skintonecode = r.GetAndCheckIntField(FI.players_skintonecode);
			m_overallrating = r.GetAndCheckIntField(FI.players_overallrating);
			m_jerseystylecode = r.GetAndCheckIntField(FI.players_jerseystylecode);
			m_hasseasonaljersey = r.GetAndCheckIntField(FI.players_hasseasonaljersey);
			m_animfreekickstartposcode = r.GetAndCheckIntField(FI.players_animfreekickstartposcode);
			m_animpenaltieskickstylecode = r.GetAndCheckIntField(FI.players_animpenaltieskickstylecode);
			m_animpenaltiesmotionstylecode = r.GetAndCheckIntField(FI.players_animpenaltiesmotionstylecode);
			m_animpenaltiesstartposcode = r.GetAndCheckIntField(FI.players_animpenaltiesstartposcode);
			m_accessorycode1 = r.GetAndCheckIntField(FI.players_accessorycode1);
			m_accessorycolourcode1 = r.GetAndCheckIntField(FI.players_accessorycolourcode1);
			m_accessorycode2 = r.GetAndCheckIntField(FI.players_accessorycode2);
			m_accessorycolourcode2 = r.GetAndCheckIntField(FI.players_accessorycolourcode2);
			m_accessorycode3 = r.GetAndCheckIntField(FI.players_accessorycode3);
			m_accessorycolourcode3 = r.GetAndCheckIntField(FI.players_accessorycolourcode3);
			m_accessorycode4 = r.GetAndCheckIntField(FI.players_accessorycode4);
			m_accessorycolourcode4 = r.GetAndCheckIntField(FI.players_accessorycolourcode4);
			m_acceleration = r.GetAndCheckIntField(FI.players_acceleration);
			m_aggression = r.GetAndCheckIntField(FI.players_aggression);
			m_ballcontrol = r.GetAndCheckIntField(FI.players_ballcontrol);
			m_crossing = r.GetAndCheckIntField(FI.players_crossing);
			m_dribbling = r.GetAndCheckIntField(FI.players_dribbling);
			m_finishing = r.GetAndCheckIntField(FI.players_finishing);
			m_freekickaccuracy = r.GetAndCheckIntField(FI.players_freekickaccuracy);
			m_headingaccuracy = r.GetAndCheckIntField(FI.players_headingaccuracy);
			m_longpassing = r.GetAndCheckIntField(FI.players_longpassing);
			m_longshots = r.GetAndCheckIntField(FI.players_longshots);
			m_marking = r.GetAndCheckIntField(FI.players_marking);
			m_positioning = r.GetAndCheckIntField(FI.players_positioning);
			m_potential = r.GetAndCheckIntField(FI.players_potential);
			m_reactions = r.GetAndCheckIntField(FI.players_reactions);
			m_shortpassing = r.GetAndCheckIntField(FI.players_shortpassing);
			m_shotpower = r.GetAndCheckIntField(FI.players_shotpower);
			m_sprintspeed = r.GetAndCheckIntField(FI.players_sprintspeed);
			m_stamina = r.GetAndCheckIntField(FI.players_stamina);
			m_strength = r.GetAndCheckIntField(FI.players_strength);
			m_standingtackle = r.GetAndCheckIntField(FI.players_standingtackle);
			m_slidingtackle = r.GetAndCheckIntField(FI.players_slidingtackle);
			m_gkdiving = r.GetAndCheckIntField(FI.players_gkdiving);
			m_gkpositioning = r.GetAndCheckIntField(FI.players_gkpositioning);
			m_gkhandling = r.GetAndCheckIntField(FI.players_gkhandling);
			m_gkreflexes = r.GetAndCheckIntField(FI.players_gkreflexes);
			m_gkglovetypecode = r.GetAndCheckIntField(FI.players_gkglovetypecode);
			m_agility = r.GetAndCheckIntField(FI.players_agility);
			m_balance = r.GetAndCheckIntField(FI.players_balance);
			m_gkkicking = r.GetAndCheckIntField(FI.players_gkkicking);
			m_gkkickstyle = r.GetAndCheckIntField(FI.players_gkkickstyle);
			m_jumping = r.GetAndCheckIntField(FI.players_jumping);
			m_penalties = r.GetAndCheckIntField(FI.players_penalties);
			m_vision = r.GetAndCheckIntField(FI.players_vision);
			m_volleys = r.GetAndCheckIntField(FI.players_volleys);
			m_gender = ((r.GetAndCheckIntField(FI.players_gender) != 0) ? true : false);
			m_emotion = r.GetAndCheckIntField(FI.players_emotion);
			m_skillmoves = r.GetAndCheckIntField(FI.players_skillmoves) + 1;
			m_usercaneditname = r.GetAndCheckIntField(FI.players_usercaneditname);
			m_finishingcode1 = r.GetAndCheckIntField(FI.players_finishingcode1);
			m_finishingcode2 = r.GetAndCheckIntField(FI.players_finishingcode2);
			m_runningcode1 = r.GetAndCheckIntField(FI.players_runningcode1);
			m_runningcode2 = r.GetAndCheckIntField(FI.players_runningcode2);
			m_gksavetype = r.GetAndCheckIntField(FI.players_gksavetype);
			m_faceposercode = r.GetAndCheckIntField(FI.players_faceposercode);
			m_isretiring = r.GetAndCheckIntField(FI.players_isretiring);
			m_socklengthcode = r.GetAndCheckIntField(FI.players_socklengthcode);
			m_hashighqualityhead = ((r.GetAndCheckIntField(FI.players_hashighqualityhead) != 0) ? true : false);
			m_attackingworkrate = r.GetAndCheckIntField(FI.players_attackingworkrate);
			m_defensiveworkrate = r.GetAndCheckIntField(FI.players_defensiveworkrate);
			m_shortstyle = ((r.GetAndCheckIntField(FI.players_shortstyle) != 0) ? true : false);
			int andCheckIntField = r.GetAndCheckIntField(FI.players_trait1);
			m_Inflexible = (((andCheckIntField & 1) != 0) ? true : false);
			m_Longthrows = (((andCheckIntField & 2) != 0) ? true : false);
			m_PowerfulFreeKicks = (((andCheckIntField & 4) != 0) ? true : false);
			m_Diver = (((andCheckIntField & 8) != 0) ? true : false);
			m_InjuryProne = (((andCheckIntField & 0x10) != 0) ? true : false);
			m_InjuryFree = (((andCheckIntField & 0x20) != 0) ? true : false);
			m_AvoidsWeakFoot = (((andCheckIntField & 0x40) != 0) ? true : false);
			m_Divesintotackles = (((andCheckIntField & 0x80) != 0) ? true : false);
			m_BeatDefensiveLine = (((andCheckIntField & 0x100) != 0) ? true : false);
			m_Selfish = (((andCheckIntField & 0x200) != 0) ? true : false);
			m_Leadership = (((andCheckIntField & 0x400) != 0) ? true : false);
			m_ArguesWithOfficials = (((andCheckIntField & 0x800) != 0) ? true : false);
			m_Earlycrosser = (((andCheckIntField & 0x1000) != 0) ? true : false);
			m_FinesseShot = (((andCheckIntField & 0x2000) != 0) ? true : false);
			m_Flair = (((andCheckIntField & 0x4000) != 0) ? true : false);
			m_LongPasser = (((andCheckIntField & 0x8000) != 0) ? true : false);
			m_LongShotTaker = (((andCheckIntField & 0x10000) != 0) ? true : false);
			m_Technicaldribbler = (((andCheckIntField & 0x20000) != 0) ? true : false);
			m_Playmaker = (((andCheckIntField & 0x40000) != 0) ? true : false);
			m_Pushesupforcorners = (((andCheckIntField & 0x80000) != 0) ? true : false);
			m_Puncher = (((andCheckIntField & 0x100000) != 0) ? true : false);
			m_GkLongThrower = (((andCheckIntField & 0x200000) != 0) ? true : false);
			m_PowerHeader = (((andCheckIntField & 0x400000) != 0) ? true : false);
			m_GkOneOnOne = (((andCheckIntField & 0x800000) != 0) ? true : false);
			m_GiantThrow = (((andCheckIntField & 0x1000000) != 0) ? true : false);
			m_OutsideFootShot = (((andCheckIntField & 0x2000000) != 0) ? true : false);
			m_CrowdFavorite = (((andCheckIntField & 0x4000000) != 0) ? true : false);
			m_SwervePasser = (((andCheckIntField & 0x8000000) != 0) ? true : false);
			m_SecondWind = (((andCheckIntField & 0x10000000) != 0) ? true : false);
			m_AcrobaticClearance = (((andCheckIntField & 0x20000000) != 0) ? true : false);
			int andCheckIntField2 = r.GetAndCheckIntField(FI.players_trait2);
			m_FancyFeet = (((andCheckIntField2 & 1) != 0) ? true : false);
			m_FancyPasses = (((andCheckIntField2 & 2) != 0) ? true : false);
			m_FancyFlicks = (((andCheckIntField2 & 4) != 0) ? true : false);
			m_StutterPenalty = (((andCheckIntField2 & 8) != 0) ? true : false);
			m_ChipperPenalty = (((andCheckIntField2 & 0x10) != 0) ? true : false);
			m_BycicleKick = (((andCheckIntField2 & 0x20) != 0) ? true : false);
			m_DivingHeader = (((andCheckIntField2 & 0x40) != 0) ? true : false);
			m_DrivenPass = (((andCheckIntField2 & 0x80) != 0) ? true : false);
			m_GkFlatKick = (((andCheckIntField2 & 0x100) != 0) ? true : false);
			m_HighClubIdentification = (((andCheckIntField2 & 0x200) != 0) ? true : false);
			m_TeamPlayer = (((andCheckIntField2 & 0x400) != 0) ? true : false);
			m_assetid = base.Id;
			m_nationality = r.GetAndCheckIntField(FI.players_nationality);
			m_weakfootabilitytypecode = r.GetAndCheckIntField(FI.players_weakfootabilitytypecode);
			m_curve = r.GetAndCheckIntField(FI.players_curve);
			m_internationalrep = r.GetAndCheckIntField(FI.players_internationalrep) - 1;
			m_interceptions = r.GetAndCheckIntField(FI.players_interceptions);
			m_shoecolorcode2 = r.GetAndCheckIntField(FI.players_shoecolorcode2);
			m_jerseyfit = ((r.GetAndCheckIntField(FI.players_jerseyfit) != 0) ? true : false);
			m_shoedesigncode = r.GetAndCheckIntField(FI.players_shoedesigncode);
		}

		public void UpdateFromOnlineRecord(Record r)
		{
			DateTime birthdate = FifaUtil.ConvertToDate(r.GetAndCheckIntField(FI.players_birthdate));
			if (birthdate.Year < 1900)
			{
				birthdate = new DateTime(1980, 1, 1);
			}
			m_birthdate = birthdate;
			m_playerjointeamdate = FifaUtil.ConvertToDate(r.GetAndCheckIntField(FI.players_playerjointeamdate));
			m_contractvaliduntil = r.GetAndCheckIntField(FI.players_contractvaliduntil);
			m_height = r.GetAndCheckIntField(FI.players_height);
			m_weight = r.GetAndCheckIntField(FI.players_weight);
			m_preferredposition1 = r.GetAndCheckIntField(FI.players_preferredposition1);
			m_preferredposition2 = r.GetAndCheckIntField(FI.players_preferredposition2);
			m_preferredposition3 = r.GetAndCheckIntField(FI.players_preferredposition3);
			m_preferredposition4 = r.GetAndCheckIntField(FI.players_preferredposition4);
			m_preferredfoot = r.GetAndCheckIntField(FI.players_preferredfoot) - 1;
			m_bodytypecode = r.GetAndCheckIntField(FI.players_bodytypecode) - 1;
			m_shoecolorcode1 = r.GetAndCheckIntField(FI.players_shoecolorcode1);
			m_shoetypecode = r.GetAndCheckIntField(FI.players_shoetypecode);
			m_jerseysleevelengthcode = r.GetAndCheckIntField(FI.players_jerseysleevelengthcode);
			m_eyecolorcode = r.GetAndCheckIntField(FI.players_eyecolorcode);
			m_eyebrowcode = r.GetAndCheckIntField(FI.players_eyebrowcode);
			m_facialhairtypecode = r.GetAndCheckIntField(FI.players_facialhairtypecode);
			m_facialhaircolorcode = r.GetAndCheckIntField(FI.players_facialhaircolorcode);
			m_hairtypecode = r.GetAndCheckIntField(FI.players_hairtypecode);
			m_haircolorcode = r.GetAndCheckIntField(FI.players_haircolorcode);
			m_headtypecode = r.GetAndCheckIntField(FI.players_headtypecode);
			m_sideburnscode = r.GetAndCheckIntField(FI.players_sideburnscode);
			m_skintypecode = r.GetAndCheckIntField(FI.players_skintypecode);
			m_skintonecode = r.GetAndCheckIntField(FI.players_skintonecode);
			m_overallrating = r.GetAndCheckIntField(FI.players_overallrating);
			m_jerseystylecode = r.GetAndCheckIntField(FI.players_jerseystylecode);
			m_hasseasonaljersey = r.GetAndCheckIntField(FI.players_hasseasonaljersey);
			m_animfreekickstartposcode = r.GetAndCheckIntField(FI.players_animfreekickstartposcode);
			m_animpenaltieskickstylecode = r.GetAndCheckIntField(FI.players_animpenaltieskickstylecode);
			m_animpenaltiesmotionstylecode = r.GetAndCheckIntField(FI.players_animpenaltiesmotionstylecode);
			m_animpenaltiesstartposcode = r.GetAndCheckIntField(FI.players_animpenaltiesstartposcode);
			m_accessorycode1 = r.GetAndCheckIntField(FI.players_accessorycode1);
			m_accessorycolourcode1 = r.GetAndCheckIntField(FI.players_accessorycolourcode1);
			m_accessorycode2 = r.GetAndCheckIntField(FI.players_accessorycode2);
			m_accessorycolourcode2 = r.GetAndCheckIntField(FI.players_accessorycolourcode2);
			m_accessorycode3 = r.GetAndCheckIntField(FI.players_accessorycode3);
			m_accessorycolourcode3 = r.GetAndCheckIntField(FI.players_accessorycolourcode3);
			m_accessorycode4 = r.GetAndCheckIntField(FI.players_accessorycode4);
			m_accessorycolourcode4 = r.GetAndCheckIntField(FI.players_accessorycolourcode4);
			m_acceleration = r.GetAndCheckIntField(FI.players_acceleration);
			m_aggression = r.GetAndCheckIntField(FI.players_aggression);
			m_ballcontrol = r.GetAndCheckIntField(FI.players_ballcontrol);
			m_crossing = r.GetAndCheckIntField(FI.players_crossing);
			m_dribbling = r.GetAndCheckIntField(FI.players_dribbling);
			m_finishing = r.GetAndCheckIntField(FI.players_finishing);
			m_freekickaccuracy = r.GetAndCheckIntField(FI.players_freekickaccuracy);
			m_headingaccuracy = r.GetAndCheckIntField(FI.players_headingaccuracy);
			m_longpassing = r.GetAndCheckIntField(FI.players_longpassing);
			m_longshots = r.GetAndCheckIntField(FI.players_longshots);
			m_marking = r.GetAndCheckIntField(FI.players_marking);
			m_positioning = r.GetAndCheckIntField(FI.players_positioning);
			m_potential = r.GetAndCheckIntField(FI.players_potential);
			m_reactions = r.GetAndCheckIntField(FI.players_reactions);
			m_shortpassing = r.GetAndCheckIntField(FI.players_shortpassing);
			m_shotpower = r.GetAndCheckIntField(FI.players_shotpower);
			m_sprintspeed = r.GetAndCheckIntField(FI.players_sprintspeed);
			m_stamina = r.GetAndCheckIntField(FI.players_stamina);
			m_strength = r.GetAndCheckIntField(FI.players_strength);
			m_standingtackle = r.GetAndCheckIntField(FI.players_standingtackle);
			m_slidingtackle = r.GetAndCheckIntField(FI.players_slidingtackle);
			m_gkdiving = r.GetAndCheckIntField(FI.players_gkdiving);
			m_gkpositioning = r.GetAndCheckIntField(FI.players_gkpositioning);
			m_gkhandling = r.GetAndCheckIntField(FI.players_gkhandling);
			m_gkreflexes = r.GetAndCheckIntField(FI.players_gkreflexes);
			m_gkglovetypecode = r.GetAndCheckIntField(FI.players_gkglovetypecode);
			m_agility = r.GetAndCheckIntField(FI.players_agility);
			m_balance = r.GetAndCheckIntField(FI.players_balance);
			m_gkkicking = r.GetAndCheckIntField(FI.players_gkkicking);
			m_gkkickstyle = r.GetAndCheckIntField(FI.players_gkkickstyle);
			m_jumping = r.GetAndCheckIntField(FI.players_jumping);
			m_penalties = r.GetAndCheckIntField(FI.players_penalties);
			m_vision = r.GetAndCheckIntField(FI.players_vision);
			m_volleys = r.GetAndCheckIntField(FI.players_volleys);
			m_gender = ((r.GetAndCheckIntField(FI.players_gender) != 0) ? true : false);
			m_emotion = r.GetAndCheckIntField(FI.players_emotion);
			m_skillmoves = r.GetAndCheckIntField(FI.players_skillmoves) + 1;
			m_usercaneditname = r.GetAndCheckIntField(FI.players_usercaneditname);
			m_finishingcode1 = r.GetAndCheckIntField(FI.players_finishingcode1);
			m_finishingcode2 = r.GetAndCheckIntField(FI.players_finishingcode2);
			m_runningcode1 = r.GetAndCheckIntField(FI.players_runningcode1);
			m_runningcode2 = r.GetAndCheckIntField(FI.players_runningcode2);
			m_gksavetype = r.GetAndCheckIntField(FI.players_gksavetype);
			m_faceposercode = r.GetAndCheckIntField(FI.players_faceposercode);
			m_isretiring = r.GetAndCheckIntField(FI.players_isretiring);
			m_socklengthcode = r.GetAndCheckIntField(FI.players_socklengthcode);
			m_hashighqualityhead = ((r.GetAndCheckIntField(FI.players_hashighqualityhead) != 0) ? true : false);
			m_attackingworkrate = r.GetAndCheckIntField(FI.players_attackingworkrate);
			m_defensiveworkrate = r.GetAndCheckIntField(FI.players_defensiveworkrate);
			m_shortstyle = ((r.GetAndCheckIntField(FI.players_shortstyle) != 0) ? true : false);
			int andCheckIntField = r.GetAndCheckIntField(FI.players_trait1);
			m_Inflexible = (((andCheckIntField & 1) != 0) ? true : false);
			m_Longthrows = (((andCheckIntField & 2) != 0) ? true : false);
			m_PowerfulFreeKicks = (((andCheckIntField & 4) != 0) ? true : false);
			m_Diver = (((andCheckIntField & 8) != 0) ? true : false);
			m_InjuryProne = (((andCheckIntField & 0x10) != 0) ? true : false);
			m_InjuryFree = (((andCheckIntField & 0x20) != 0) ? true : false);
			m_AvoidsWeakFoot = (((andCheckIntField & 0x40) != 0) ? true : false);
			m_Divesintotackles = (((andCheckIntField & 0x80) != 0) ? true : false);
			m_BeatDefensiveLine = (((andCheckIntField & 0x100) != 0) ? true : false);
			m_Selfish = (((andCheckIntField & 0x200) != 0) ? true : false);
			m_Leadership = (((andCheckIntField & 0x400) != 0) ? true : false);
			m_ArguesWithOfficials = (((andCheckIntField & 0x800) != 0) ? true : false);
			m_Earlycrosser = (((andCheckIntField & 0x1000) != 0) ? true : false);
			m_FinesseShot = (((andCheckIntField & 0x2000) != 0) ? true : false);
			m_Flair = (((andCheckIntField & 0x4000) != 0) ? true : false);
			m_LongPasser = (((andCheckIntField & 0x8000) != 0) ? true : false);
			m_LongShotTaker = (((andCheckIntField & 0x10000) != 0) ? true : false);
			m_Technicaldribbler = (((andCheckIntField & 0x20000) != 0) ? true : false);
			m_Playmaker = (((andCheckIntField & 0x40000) != 0) ? true : false);
			m_Pushesupforcorners = (((andCheckIntField & 0x80000) != 0) ? true : false);
			m_Puncher = (((andCheckIntField & 0x100000) != 0) ? true : false);
			m_GkLongThrower = (((andCheckIntField & 0x200000) != 0) ? true : false);
			m_PowerHeader = (((andCheckIntField & 0x400000) != 0) ? true : false);
			m_GkOneOnOne = (((andCheckIntField & 0x800000) != 0) ? true : false);
			m_GiantThrow = (((andCheckIntField & 0x1000000) != 0) ? true : false);
			m_OutsideFootShot = (((andCheckIntField & 0x2000000) != 0) ? true : false);
			m_CrowdFavorite = (((andCheckIntField & 0x4000000) != 0) ? true : false);
			m_SwervePasser = (((andCheckIntField & 0x8000000) != 0) ? true : false);
			m_SecondWind = (((andCheckIntField & 0x10000000) != 0) ? true : false);
			m_AcrobaticClearance = (((andCheckIntField & 0x20000000) != 0) ? true : false);
			int andCheckIntField2 = r.GetAndCheckIntField(FI.players_trait2);
			m_FancyFeet = (((andCheckIntField2 & 1) != 0) ? true : false);
			m_FancyPasses = (((andCheckIntField2 & 2) != 0) ? true : false);
			m_FancyFlicks = (((andCheckIntField2 & 4) != 0) ? true : false);
			m_StutterPenalty = (((andCheckIntField2 & 8) != 0) ? true : false);
			m_ChipperPenalty = (((andCheckIntField2 & 0x10) != 0) ? true : false);
			m_BycicleKick = (((andCheckIntField2 & 0x20) != 0) ? true : false);
			m_DivingHeader = (((andCheckIntField2 & 0x40) != 0) ? true : false);
			m_DrivenPass = (((andCheckIntField2 & 0x80) != 0) ? true : false);
			m_GkFlatKick = (((andCheckIntField2 & 0x100) != 0) ? true : false);
			m_HighClubIdentification = (((andCheckIntField2 & 0x200) != 0) ? true : false);
			m_TeamPlayer = (((andCheckIntField2 & 0x400) != 0) ? true : false);
			m_assetid = base.Id;
			m_nationality = r.GetAndCheckIntField(FI.players_nationality);
			m_weakfootabilitytypecode = r.GetAndCheckIntField(FI.players_weakfootabilitytypecode);
			m_curve = r.GetAndCheckIntField(FI.players_curve);
			m_internationalrep = r.GetAndCheckIntField(FI.players_internationalrep) - 1;
			m_interceptions = r.GetAndCheckIntField(FI.players_interceptions);
			m_shoecolorcode2 = r.GetAndCheckIntField(FI.players_shoecolorcode2);
			m_jerseyfit = ((r.GetAndCheckIntField(FI.players_jerseyfit) != 0) ? true : false);
			m_shoedesigncode = r.GetAndCheckIntField(FI.players_shoedesigncode);
			m_IsLoaned = false;
		}

		public void UpdateFromOnlineRecord17(Record r, TableDescriptor td)
		{
			DateTime birthdate = FifaUtil.ConvertToDate(r.GetAndCheckIntField(td.GetFieldIndex("birthdate")));
			if (birthdate.Year < 1900)
			{
				birthdate = new DateTime(1980, 1, 1);
			}
			m_birthdate = birthdate;
			m_playerjointeamdate = FifaUtil.ConvertToDate(r.GetAndCheckIntField(td.GetFieldIndex("playerjointeamdate")));
			m_contractvaliduntil = r.GetAndCheckIntField(td.GetFieldIndex("contractvaliduntil"));
			m_height = r.GetAndCheckIntField(td.GetFieldIndex("height"));
			m_weight = r.GetAndCheckIntField(td.GetFieldIndex("weight"));
			m_preferredposition1 = r.GetAndCheckIntField(td.GetFieldIndex("preferredposition1"));
			m_preferredposition2 = r.GetAndCheckIntField(td.GetFieldIndex("preferredposition2"));
			m_preferredposition3 = r.GetAndCheckIntField(td.GetFieldIndex("preferredposition3"));
			m_preferredposition4 = r.GetAndCheckIntField(td.GetFieldIndex("preferredposition4"));
			m_preferredfoot = r.GetAndCheckIntField(td.GetFieldIndex("preferredfoot")) - 1;
			m_bodytypecode = r.GetAndCheckIntField(td.GetFieldIndex("bodytypecode")) - 1;
			m_shoecolorcode1 = r.GetAndCheckIntField(td.GetFieldIndex("shoecolorcode1"));
			m_shoetypecode = r.GetAndCheckIntField(td.GetFieldIndex("shoetypecode"));
			m_jerseysleevelengthcode = r.GetAndCheckIntField(td.GetFieldIndex("jerseysleevelengthcode"));
			m_eyecolorcode = r.GetAndCheckIntField(td.GetFieldIndex("eyecolorcode"));
			m_eyebrowcode = r.GetAndCheckIntField(td.GetFieldIndex("eyebrowcode"));
			m_facialhairtypecode = r.GetAndCheckIntField(td.GetFieldIndex("facialhairtypecode"));
			if (m_facialhairtypecode == 16)
			{
				m_facialhairtypecode = 0;
			}
			else if (m_facialhairtypecode == 17)
			{
				m_facialhairtypecode = 9;
			}
			m_facialhaircolorcode = r.GetAndCheckIntField(td.GetFieldIndex("facialhaircolorcode"));
			m_hairtypecode = r.GetAndCheckIntField(td.GetFieldIndex("hairtypecode"));
			if (m_hairtypecode >= 125)
			{
				switch (m_hairtypecode)
				{
				case 125:
					m_hairtypecode = 20;
					break;
				case 126:
					m_hairtypecode = 509;
					break;
				case 127:
					m_hairtypecode = 107;
					break;
				case 128:
					m_hairtypecode = 14;
					break;
				case 129:
					m_hairtypecode = 115;
					break;
				case 130:
					m_hairtypecode = 509;
					break;
				case 131:
					m_hairtypecode = 514;
					break;
				case 132:
					m_hairtypecode = 120;
					break;
				case 133:
					m_hairtypecode = 119;
					break;
				case 134:
					m_hairtypecode = 77;
					break;
				case 135:
					m_hairtypecode = 42;
					break;
				case 136:
					m_hairtypecode = 95;
					break;
				case 137:
					m_hairtypecode = 78;
					break;
				case 138:
					m_hairtypecode = 101;
					break;
				case 139:
					m_hairtypecode = 67;
					break;
				case 140:
					m_hairtypecode = 42;
					break;
				case 141:
					m_hairtypecode = 115;
					break;
				case 142:
					m_hairtypecode = 116;
					break;
				case 143:
					m_hairtypecode = 108;
					break;
				case 144:
					m_hairtypecode = 122;
					break;
				case 145:
					m_hairtypecode = 58;
					break;
				case 146:
					m_hairtypecode = 64;
					break;
				case 147:
					m_hairtypecode = 39;
					break;
				case 148:
					m_hairtypecode = 90;
					break;
				case 149:
					m_hairtypecode = 118;
					break;
				case 150:
					m_hairtypecode = 114;
					break;
				case 151:
					m_hairtypecode = 78;
					break;
				}
			}
			m_haircolorcode = r.GetAndCheckIntField(td.GetFieldIndex("haircolorcode"));
			if (m_haircolorcode == 13)
			{
				m_facialhairtypecode = 3;
			}
			m_headtypecode = r.GetAndCheckIntField(td.GetFieldIndex("headtypecode"));
			if (m_headtypecode >= 533 && m_headtypecode <= 547)
			{
				m_headtypecode = m_Randomizer.Next(500, 533);
			}
			if (m_headtypecode >= 533 && m_headtypecode <= 547)
			{
				m_headtypecode = m_Randomizer.Next(500, 533);
			}
			m_sideburnscode = r.GetAndCheckIntField(td.GetFieldIndex("sideburnscode"));
			m_skintypecode = r.GetAndCheckIntField(td.GetFieldIndex("skintypecode"));
			m_skintonecode = r.GetAndCheckIntField(td.GetFieldIndex("skintonecode"));
			m_overallrating = r.GetAndCheckIntField(td.GetFieldIndex("overallrating"));
			m_jerseystylecode = r.GetAndCheckIntField(td.GetFieldIndex("jerseystylecode"));
			m_hasseasonaljersey = r.GetAndCheckIntField(td.GetFieldIndex("hasseasonaljersey"));
			m_animfreekickstartposcode = r.GetAndCheckIntField(td.GetFieldIndex("animfreekickstartposcode"));
			m_animpenaltieskickstylecode = r.GetAndCheckIntField(td.GetFieldIndex("animpenaltieskickstylecode"));
			m_animpenaltiesmotionstylecode = r.GetAndCheckIntField(td.GetFieldIndex("animpenaltiesmotionstylecode"));
			m_animpenaltiesstartposcode = r.GetAndCheckIntField(td.GetFieldIndex("animpenaltiesstartposcode"));
			m_accessorycode1 = r.GetAndCheckIntField(td.GetFieldIndex("accessorycode1"));
			m_accessorycolourcode1 = r.GetAndCheckIntField(td.GetFieldIndex("accessorycolourcode1"));
			m_accessorycode2 = r.GetAndCheckIntField(td.GetFieldIndex("accessorycode2"));
			m_accessorycolourcode2 = r.GetAndCheckIntField(td.GetFieldIndex("accessorycolourcode2"));
			m_accessorycode3 = r.GetAndCheckIntField(td.GetFieldIndex("accessorycode3"));
			m_accessorycolourcode3 = r.GetAndCheckIntField(td.GetFieldIndex("accessorycolourcode3"));
			m_accessorycode4 = r.GetAndCheckIntField(td.GetFieldIndex("accessorycode4"));
			m_accessorycolourcode4 = r.GetAndCheckIntField(td.GetFieldIndex("accessorycolourcode4"));
			m_acceleration = r.GetAndCheckIntField(td.GetFieldIndex("acceleration"));
			m_aggression = r.GetAndCheckIntField(td.GetFieldIndex("aggression"));
			m_ballcontrol = r.GetAndCheckIntField(td.GetFieldIndex("ballcontrol"));
			m_crossing = r.GetAndCheckIntField(td.GetFieldIndex("crossing"));
			m_dribbling = r.GetAndCheckIntField(td.GetFieldIndex("dribbling"));
			m_finishing = r.GetAndCheckIntField(td.GetFieldIndex("finishing"));
			m_freekickaccuracy = r.GetAndCheckIntField(td.GetFieldIndex("freekickaccuracy"));
			m_headingaccuracy = r.GetAndCheckIntField(td.GetFieldIndex("headingaccuracy"));
			m_longpassing = r.GetAndCheckIntField(td.GetFieldIndex("longpassing"));
			m_longshots = r.GetAndCheckIntField(td.GetFieldIndex("longshots"));
			m_marking = r.GetAndCheckIntField(td.GetFieldIndex("marking"));
			m_positioning = r.GetAndCheckIntField(td.GetFieldIndex("positioning"));
			m_potential = r.GetAndCheckIntField(td.GetFieldIndex("potential"));
			m_reactions = r.GetAndCheckIntField(td.GetFieldIndex("reactions"));
			m_shortpassing = r.GetAndCheckIntField(td.GetFieldIndex("shortpassing"));
			m_shotpower = r.GetAndCheckIntField(td.GetFieldIndex("shotpower"));
			m_sprintspeed = r.GetAndCheckIntField(td.GetFieldIndex("sprintspeed"));
			m_stamina = r.GetAndCheckIntField(td.GetFieldIndex("stamina"));
			m_strength = r.GetAndCheckIntField(td.GetFieldIndex("strength"));
			m_standingtackle = r.GetAndCheckIntField(td.GetFieldIndex("standingtackle"));
			m_slidingtackle = r.GetAndCheckIntField(td.GetFieldIndex("slidingtackle"));
			m_gkdiving = r.GetAndCheckIntField(td.GetFieldIndex("gkdiving"));
			m_gkpositioning = r.GetAndCheckIntField(td.GetFieldIndex("gkpositioning"));
			m_gkhandling = r.GetAndCheckIntField(td.GetFieldIndex("gkhandling"));
			m_gkreflexes = r.GetAndCheckIntField(td.GetFieldIndex("gkreflexes"));
			m_gkglovetypecode = r.GetAndCheckIntField(td.GetFieldIndex("gkglovetypecode"));
			m_agility = r.GetAndCheckIntField(td.GetFieldIndex("agility"));
			m_balance = r.GetAndCheckIntField(td.GetFieldIndex("balance"));
			m_gkkicking = r.GetAndCheckIntField(td.GetFieldIndex("gkkicking"));
			m_gkkickstyle = r.GetAndCheckIntField(td.GetFieldIndex("gkkickstyle"));
			m_jumping = r.GetAndCheckIntField(td.GetFieldIndex("jumping"));
			m_penalties = r.GetAndCheckIntField(td.GetFieldIndex("penalties"));
			m_vision = r.GetAndCheckIntField(td.GetFieldIndex("vision"));
			m_volleys = r.GetAndCheckIntField(td.GetFieldIndex("volleys"));
			m_gender = ((r.GetAndCheckIntField(td.GetFieldIndex("gender")) != 0) ? true : false);
			m_emotion = r.GetAndCheckIntField(td.GetFieldIndex("emotion"));
			m_skillmoves = r.GetAndCheckIntField(td.GetFieldIndex("skillmoves")) + 1;
			m_usercaneditname = r.GetAndCheckIntField(td.GetFieldIndex("usercaneditname"));
			m_finishingcode1 = r.GetAndCheckIntField(td.GetFieldIndex("finishingcode1"));
			m_finishingcode2 = r.GetAndCheckIntField(td.GetFieldIndex("finishingcode2"));
			m_runningcode1 = r.GetAndCheckIntField(td.GetFieldIndex("runningcode1"));
			m_runningcode2 = r.GetAndCheckIntField(td.GetFieldIndex("runningcode2"));
			m_gksavetype = r.GetAndCheckIntField(td.GetFieldIndex("gksavetype"));
			m_faceposercode = r.GetAndCheckIntField(td.GetFieldIndex("faceposercode"));
			m_isretiring = r.GetAndCheckIntField(td.GetFieldIndex("isretiring"));
			m_socklengthcode = r.GetAndCheckIntField(td.GetFieldIndex("socklengthcode"));
			m_hashighqualityhead = ((r.GetAndCheckIntField(td.GetFieldIndex("hashighqualityhead")) != 0) ? true : false);
			m_attackingworkrate = r.GetAndCheckIntField(td.GetFieldIndex("attackingworkrate"));
			m_defensiveworkrate = r.GetAndCheckIntField(td.GetFieldIndex("defensiveworkrate"));
			m_shortstyle = ((r.GetAndCheckIntField(td.GetFieldIndex("shortstyle")) != 0) ? true : false);
			int andCheckIntField = r.GetAndCheckIntField(td.GetFieldIndex("trait1"));
			m_Inflexible = (((andCheckIntField & 1) != 0) ? true : false);
			m_Longthrows = (((andCheckIntField & 2) != 0) ? true : false);
			m_PowerfulFreeKicks = (((andCheckIntField & 4) != 0) ? true : false);
			m_Diver = (((andCheckIntField & 8) != 0) ? true : false);
			m_InjuryProne = (((andCheckIntField & 0x10) != 0) ? true : false);
			m_InjuryFree = (((andCheckIntField & 0x20) != 0) ? true : false);
			m_AvoidsWeakFoot = (((andCheckIntField & 0x40) != 0) ? true : false);
			m_Divesintotackles = (((andCheckIntField & 0x80) != 0) ? true : false);
			m_BeatDefensiveLine = (((andCheckIntField & 0x100) != 0) ? true : false);
			m_Selfish = (((andCheckIntField & 0x200) != 0) ? true : false);
			m_Leadership = (((andCheckIntField & 0x400) != 0) ? true : false);
			m_ArguesWithOfficials = (((andCheckIntField & 0x800) != 0) ? true : false);
			m_Earlycrosser = (((andCheckIntField & 0x1000) != 0) ? true : false);
			m_FinesseShot = (((andCheckIntField & 0x2000) != 0) ? true : false);
			m_Flair = (((andCheckIntField & 0x4000) != 0) ? true : false);
			m_LongPasser = (((andCheckIntField & 0x8000) != 0) ? true : false);
			m_LongShotTaker = (((andCheckIntField & 0x10000) != 0) ? true : false);
			m_Technicaldribbler = (((andCheckIntField & 0x20000) != 0) ? true : false);
			m_Playmaker = (((andCheckIntField & 0x40000) != 0) ? true : false);
			m_Pushesupforcorners = (((andCheckIntField & 0x80000) != 0) ? true : false);
			m_Puncher = (((andCheckIntField & 0x100000) != 0) ? true : false);
			m_GkLongThrower = (((andCheckIntField & 0x200000) != 0) ? true : false);
			m_PowerHeader = (((andCheckIntField & 0x400000) != 0) ? true : false);
			m_GkOneOnOne = (((andCheckIntField & 0x800000) != 0) ? true : false);
			m_GiantThrow = (((andCheckIntField & 0x1000000) != 0) ? true : false);
			m_OutsideFootShot = (((andCheckIntField & 0x2000000) != 0) ? true : false);
			m_CrowdFavorite = (((andCheckIntField & 0x4000000) != 0) ? true : false);
			m_SwervePasser = (((andCheckIntField & 0x8000000) != 0) ? true : false);
			m_SecondWind = (((andCheckIntField & 0x10000000) != 0) ? true : false);
			m_AcrobaticClearance = (((andCheckIntField & 0x20000000) != 0) ? true : false);
			int andCheckIntField2 = r.GetAndCheckIntField(td.GetFieldIndex("trait2"));
			m_FancyFeet = (((andCheckIntField2 & 1) != 0) ? true : false);
			m_FancyPasses = (((andCheckIntField2 & 2) != 0) ? true : false);
			m_FancyFlicks = (((andCheckIntField2 & 4) != 0) ? true : false);
			m_StutterPenalty = (((andCheckIntField2 & 8) != 0) ? true : false);
			m_ChipperPenalty = (((andCheckIntField2 & 0x10) != 0) ? true : false);
			m_BycicleKick = (((andCheckIntField2 & 0x20) != 0) ? true : false);
			m_DivingHeader = (((andCheckIntField2 & 0x40) != 0) ? true : false);
			m_DrivenPass = (((andCheckIntField2 & 0x80) != 0) ? true : false);
			m_GkFlatKick = (((andCheckIntField2 & 0x100) != 0) ? true : false);
			m_HighClubIdentification = (((andCheckIntField2 & 0x200) != 0) ? true : false);
			m_TeamPlayer = (((andCheckIntField2 & 0x400) != 0) ? true : false);
			m_assetid = base.Id;
			m_nationality = r.GetAndCheckIntField(td.GetFieldIndex("nationality"));
			m_weakfootabilitytypecode = r.GetAndCheckIntField(td.GetFieldIndex("weakfootabilitytypecode"));
			m_curve = r.GetAndCheckIntField(td.GetFieldIndex("curve"));
			m_internationalrep = r.GetAndCheckIntField(td.GetFieldIndex("internationalrep")) - 1;
			m_interceptions = r.GetAndCheckIntField(td.GetFieldIndex("interceptions"));
			m_shoecolorcode2 = r.GetAndCheckIntField(td.GetFieldIndex("shoecolorcode2"));
			m_jerseyfit = ((r.GetAndCheckIntField(td.GetFieldIndex("jerseyfit")) != 0) ? true : false);
			m_shoedesigncode = r.GetAndCheckIntField(td.GetFieldIndex("shoedesigncode"));
			m_IsLoaned = false;
		}

		public void FillFromPlayerloans(Record r)
		{
			m_teamidloanedfrom = r.GetAndCheckIntField(FI.playerloans_teamidloanedfrom);
			m_loandateend = FifaUtil.ConvertToDate(r.GetAndCheckIntField(FI.playerloans_loandateend));
			m_IsLoaned = true;
		}

		public void FillFromPreviousTeam(Record r)
		{
			m_previousteamid = r.GetAndCheckIntField(FI.previousteam_previousteamid);
		}

		public void FillFromEditedPlayerNames(Record r)
		{
			m_firstname = r.GetAndCheckStringField(FI.editedplayernames_firstname);
			m_lastname = r.GetAndCheckStringField(FI.editedplayernames_surname);
			m_commonname = r.GetAndCheckStringField(FI.editedplayernames_commonname);
			m_playerjerseyname = r.GetAndCheckStringField(FI.editedplayernames_playerjerseyname);
			m_IsLoaned = true;
		}

		public void SavePlayerloans(Record r)
		{
			r.IntField[FI.playerloans_playerid] = base.Id;
			r.IntField[FI.playerloans_teamidloanedfrom] = m_teamidloanedfrom;
			r.IntField[FI.playerloans_loandateend] = FifaUtil.ConvertFromDate(m_loandateend);
		}

		public void SavePreviousTeam(Record r)
		{
			r.IntField[FI.previousteam_playerid] = base.Id;
			r.IntField[FI.previousteam_previousteamid] = m_previousteamid;
		}

		public void LinkCountry(CountryList countryList)
		{
			if (countryList == null)
			{
				return;
			}
			m_Country = (Country)countryList.SearchId(m_nationality);
			if (m_Country == null)
			{
				if (countryList.Count > 0)
				{
					m_Country = (Country)countryList[0];
					m_nationality = m_Country.Id;
				}
				else
				{
					m_nationality = 0;
				}
			}
		}

		public void LinkTeam(TeamList teamList)
		{
			if (teamList == null)
			{
				return;
			}
			if (m_teamidloanedfrom != 0)
			{
				m_TeamLoanedFrom = (Team)teamList.SearchId(m_teamidloanedfrom);
				if (m_TeamLoanedFrom == null)
				{
					m_teamidloanedfrom = 0;
				}
			}
			if (m_previousteamid != 0)
			{
				m_PreviousTeam = (Team)teamList.SearchId(m_previousteamid);
				if (m_PreviousTeam == null)
				{
					m_previousteamid = 0;
				}
			}
		}

		public void SearchCountry()
		{
			if (FifaEnvironment.Countries == null)
			{
				m_Country = null;
				return;
			}
			m_Country = (Country)FifaEnvironment.Countries.SearchId(m_nationality);
			if (m_Country == null)
			{
				if (FifaEnvironment.Countries.Count > 0)
				{
					m_Country = (Country)FifaEnvironment.Countries[0];
					m_nationality = m_Country.Id;
				}
				else
				{
					m_nationality = 0;
				}
			}
		}

		public void SearchWouldTeams()
		{
		}

		public void InitNewPlayer()
		{
			m_firstname = string.Empty;
			m_lastname = "New Player";
			m_commentaryid = 900000;
			m_commonname = string.Empty;
			m_playerjerseyname = "New Player";
			m_playerjerseynameid = 0;
			m_firstnameid = 0;
			m_commonnameid = 0;
			m_lastnameid = 0;
			m_birthdate = new DateTime(1980, 6, 15);
			m_playerjointeamdate = new DateTime(2014, 1, 1);
			m_contractvaliduntil = 2017;
			m_height = 180;
			m_weight = 80;
			m_preferredposition1 = 0;
			m_preferredposition2 = -1;
			m_preferredposition3 = -1;
			m_preferredposition4 = -1;
			m_preferredfoot = 0;
			m_jerseysleevelengthcode = 0;
			m_jerseystylecode = 0;
			m_hasseasonaljersey = 0;
			m_animfreekickstartposcode = 0;
			m_animpenaltieskickstylecode = 0;
			m_animpenaltiesmotionstylecode = 0;
			m_animpenaltiesstartposcode = 0;
			m_accessorycode1 = 0;
			m_accessorycolourcode1 = 1;
			m_accessorycode2 = 0;
			m_accessorycolourcode2 = 1;
			m_accessorycode3 = 0;
			m_accessorycolourcode3 = 1;
			m_accessorycode4 = 0;
			m_accessorycolourcode4 = 1;
			m_acceleration = 50;
			m_aggression = 50;
			m_sprintspeed = 50;
			m_stamina = 50;
			m_strength = 50;
			m_marking = 50;
			m_standingtackle = 50;
			m_slidingtackle = 50;
			m_ballcontrol = 50;
			m_dribbling = 50;
			m_crossing = 50;
			m_headingaccuracy = 50;
			m_shortpassing = 50;
			m_longpassing = 50;
			m_longshots = 50;
			m_finishing = 50;
			m_shotpower = 50;
			m_reactions = 50;
			m_gkreflexes = 50;
			m_gkglovetypecode = 0;
			m_agility = 50;
			m_balance = 50;
			m_gkkicking = 50;
			m_gkkickstyle = 0;
			m_jumping = 50;
			m_penalties = 50;
			m_vision = 50;
			m_volleys = 50;
			m_skillmoves = 1;
			m_usercaneditname = 0;
			m_gkhandling = 50;
			m_gkdiving = 50;
			m_gkpositioning = 50;
			m_freekickaccuracy = 50;
			m_positioning = 50;
			m_InjuryFree = false;
			m_HighClubIdentification = false;
			m_TeamPlayer = false;
			m_Leadership = false;
			m_ArguesWithOfficials = false;
			m_AvoidsWeakFoot = false;
			m_InjuryProne = false;
			m_Puncher = false;
			m_Pushesupforcorners = false;
			m_Technicaldribbler = false;
			m_Selfish = false;
			m_Playmaker = false;
			m_Diver = false;
			m_Divesintotackles = false;
			m_LongShotTaker = false;
			m_Earlycrosser = false;
			m_Inflexible = false;
			m_GkOneOnOne = false;
			m_Longthrows = false;
			m_OutsideFootShot = false;
			m_LongPasser = false;
			m_GiantThrow = false;
			m_Flair = false;
			m_PowerfulFreeKicks = false;
			m_FinesseShot = false;
			m_PowerHeader = false;
			m_SwervePasser = false;
			m_BeatDefensiveLine = false;
			m_GkLongThrower = false;
			m_CrowdFavorite = false;
			m_SecondWind = false;
			m_AcrobaticClearance = false;
			m_FancyFeet = false;
			m_FancyPasses = false;
			m_FancyFlicks = false;
			m_StutterPenalty = false;
			m_ChipperPenalty = false;
			m_BycicleKick = false;
			m_DivingHeader = false;
			m_DrivenPass = false;
			m_GkFlatKick = false;
			m_assetid = base.Id;
			m_potential = 50;
			m_nationality = 0;
			SearchCountry();
			m_bodytypecode = 1;
			m_weakfootabilitytypecode = 3;
			m_curve = 50;
			m_internationalrep = 2;
			m_eyecolorcode = 1;
			m_eyebrowcode = 0;
			m_hairtypecode = 1;
			m_headtypecode = 1;
			m_headclasscode = 1;
			m_haircolorcode = 1;
			m_facialhairtypecode = 0;
			m_facialhaircolorcode = 0;
			m_sideburnscode = 1;
			m_skintypecode = 0;
			m_skintonecode = 2;
			m_shoecolorcode1 = 1;
			m_shoetypecode = 0;
			m_overallrating = 50;
			m_finishingcode1 = 0;
			m_finishingcode2 = 0;
			m_runningcode1 = 0;
			m_runningcode2 = 0;
			m_gksavetype = 0;
			m_faceposercode = 0;
			m_isretiring = 0;
			m_socklengthcode = 0;
			m_hashighqualityhead = false;
			m_gender = false;
			m_emotion = 3;
			m_attackingworkrate = 0;
			m_defensiveworkrate = 0;
			m_shortstyle = false;
			m_interceptions = 50;
			m_shoecolorcode2 = 31;
			m_jerseyfit = false;
			m_shoedesigncode = 0;
			m_preferredNumber = 0;
			m_IsLoaned = false;
		}

		public Player(int playerId)
			: base(playerId)
		{
			base.Id = playerId;
			InitNewPlayer();
		}

		public override IdObject Clone(int playerid)
		{
			Player player = (Player)base.Clone(playerid);
			player.m_lastname = "Player_" + player.Id.ToString();
			player.m_commentaryid = player.m_commentaryid;
			player.m_firstname = string.Empty;
			player.m_assetid = playerid;
			player.m_PlayingForTeams = new TeamList();
			player.m_HasSpecificPhoto = false;
			player.m_HeadModelFile = null;
			player.m_HairModelFile = null;
			player.m_FaceTextureBitmaps = null;
			player.m_HairColorTextureBitmap = null;
			player.m_HairAlfaTextureBitmap = null;
			return player;
		}

		public Player Copy(Player clone)
		{
			clone.m_birthdate = m_birthdate;
			clone.m_playerjointeamdate = m_playerjointeamdate;
			clone.m_contractvaliduntil = m_contractvaliduntil;
			clone.m_nationality = m_nationality;
			clone.m_Country = m_Country;
			clone.m_height = m_height;
			clone.m_weight = m_weight;
			clone.m_bodytypecode = m_bodytypecode;
			clone.m_jerseysleevelengthcode = m_jerseysleevelengthcode;
			clone.m_jerseystylecode = m_jerseystylecode;
			clone.m_hasseasonaljersey = m_hasseasonaljersey;
			clone.m_animfreekickstartposcode = m_animfreekickstartposcode;
			clone.m_animpenaltieskickstylecode = m_animpenaltieskickstylecode;
			clone.m_animpenaltiesmotionstylecode = m_animpenaltiesmotionstylecode;
			clone.m_animpenaltiesstartposcode = m_animpenaltiesstartposcode;
			clone.m_accessorycode1 = m_accessorycode1;
			clone.m_accessorycolourcode1 = m_accessorycolourcode1;
			clone.m_accessorycode2 = m_accessorycode2;
			clone.m_accessorycolourcode2 = m_accessorycolourcode2;
			clone.m_accessorycode3 = m_accessorycode3;
			clone.m_accessorycolourcode3 = m_accessorycolourcode3;
			clone.m_accessorycode4 = m_accessorycode4;
			clone.m_accessorycolourcode4 = m_accessorycolourcode4;
			clone.m_preferredposition1 = m_preferredposition1;
			clone.m_preferredposition2 = m_preferredposition2;
			clone.m_preferredposition3 = m_preferredposition3;
			clone.m_preferredposition4 = m_preferredposition4;
			clone.m_preferredfoot = m_preferredfoot;
			clone.m_weakfootabilitytypecode = m_weakfootabilitytypecode;
			clone.m_acceleration = m_acceleration;
			clone.m_aggression = m_aggression;
			clone.m_sprintspeed = m_sprintspeed;
			clone.m_stamina = m_stamina;
			clone.m_strength = m_strength;
			clone.m_marking = m_marking;
			clone.m_interceptions = m_interceptions;
			clone.m_standingtackle = m_standingtackle;
			clone.m_slidingtackle = m_slidingtackle;
			clone.m_ballcontrol = m_ballcontrol;
			clone.m_dribbling = m_dribbling;
			clone.m_crossing = m_crossing;
			clone.m_headingaccuracy = m_headingaccuracy;
			clone.m_shortpassing = m_shortpassing;
			clone.m_longpassing = m_longpassing;
			clone.m_longshots = m_longshots;
			clone.m_finishing = m_finishing;
			clone.m_shotpower = m_shotpower;
			clone.m_reactions = m_reactions;
			clone.m_gkreflexes = m_gkreflexes;
			clone.m_gkglovetypecode = m_gkglovetypecode;
			clone.m_agility = m_agility;
			clone.m_balance = m_balance;
			clone.m_gkkicking = m_gkkicking;
			clone.m_gkkickstyle = m_gkkickstyle;
			clone.m_jumping = m_jumping;
			clone.m_penalties = m_penalties;
			clone.m_vision = m_vision;
			clone.m_volleys = m_volleys;
			clone.m_skillmoves = m_skillmoves;
			clone.m_usercaneditname = m_usercaneditname;
			clone.m_gkhandling = m_gkhandling;
			clone.m_gkdiving = m_gkdiving;
			clone.m_gkpositioning = m_gkpositioning;
			clone.m_positioning = m_positioning;
			clone.m_freekickaccuracy = m_freekickaccuracy;
			clone.m_potential = m_potential;
			clone.m_InjuryFree = m_InjuryFree;
			clone.m_HighClubIdentification = m_HighClubIdentification;
			clone.m_TeamPlayer = m_TeamPlayer;
			clone.m_Leadership = m_Leadership;
			clone.m_ArguesWithOfficials = m_ArguesWithOfficials;
			clone.m_AvoidsWeakFoot = m_AvoidsWeakFoot;
			clone.m_InjuryProne = m_InjuryProne;
			clone.m_Puncher = m_Puncher;
			clone.m_Pushesupforcorners = m_Pushesupforcorners;
			clone.m_Technicaldribbler = m_Technicaldribbler;
			clone.m_Selfish = m_Selfish;
			clone.m_Playmaker = m_Playmaker;
			clone.m_Diver = m_Diver;
			clone.m_Divesintotackles = m_Divesintotackles;
			clone.m_LongShotTaker = m_LongShotTaker;
			clone.m_Earlycrosser = m_Earlycrosser;
			clone.m_Inflexible = m_Inflexible;
			clone.m_GkOneOnOne = m_GkOneOnOne;
			clone.m_Longthrows = m_Longthrows;
			clone.m_OutsideFootShot = m_OutsideFootShot;
			clone.m_LongPasser = m_LongPasser;
			clone.m_GiantThrow = m_GiantThrow;
			clone.m_Flair = m_Flair;
			clone.m_PowerfulFreeKicks = m_PowerfulFreeKicks;
			clone.m_FinesseShot = m_FinesseShot;
			clone.m_PowerHeader = m_PowerHeader;
			clone.m_SwervePasser = m_SwervePasser;
			clone.m_BeatDefensiveLine = m_BeatDefensiveLine;
			clone.m_GkLongThrower = m_GkLongThrower;
			clone.m_FancyFeet = m_FancyFeet;
			clone.m_FancyPasses = m_FancyPasses;
			clone.m_FancyFlicks = m_FancyFlicks;
			clone.m_StutterPenalty = m_StutterPenalty;
			clone.m_ChipperPenalty = m_ChipperPenalty;
			clone.m_BycicleKick = m_BycicleKick;
			clone.m_DivingHeader = m_DivingHeader;
			clone.m_DrivenPass = m_DrivenPass;
			clone.m_GkFlatKick = m_GkFlatKick;
			clone.m_curve = m_curve;
			clone.m_internationalrep = m_internationalrep + 1;
			clone.m_eyecolorcode = m_eyecolorcode;
			clone.m_eyebrowcode = m_eyebrowcode;
			clone.m_hairtypecode = m_hairtypecode;
			clone.m_headtypecode = m_headtypecode;
			clone.m_headclasscode = 1;
			clone.m_haircolorcode = m_haircolorcode;
			clone.m_facialhairtypecode = m_facialhairtypecode;
			clone.m_facialhaircolorcode = m_facialhaircolorcode;
			clone.m_sideburnscode = m_sideburnscode;
			clone.m_skintypecode = m_skintypecode;
			clone.m_skintonecode = m_skintonecode;
			clone.m_shoecolorcode1 = m_shoecolorcode1;
			clone.m_shoecolorcode2 = m_shoecolorcode2;
			clone.m_shoetypecode = m_shoetypecode;
			clone.m_shoedesigncode = m_shoedesigncode;
			clone.m_overallrating = m_overallrating;
			clone.m_HasSpecificPhoto = false;
			return clone;
		}

		public string SpecificFaceTextureFileName()
		{
			return SpecificFaceTextureFileName(base.Id);
		}

		public static string SpecificFaceTextureFileName(int id)
		{
			return "data/sceneassets/faces/face_" + id.ToString() + "_0_0_0_0_0_0_0_0_textures.rx3";
		}

		public string GenericFaceTextureFileName()
		{
			return "data/sceneassets/genheadtex/skin_" + m_skintonecode.ToString() + "_" + m_skintypecode.ToString() + ".rx3";
		}

		public string GenericSkinTextureFileName()
		{
			return "data/sceneassets/genheadtex/skin_" + m_skintonecode.ToString() + "_" + m_skintypecode.ToString() + ".rx3";
		}

		public static string GenericSkinTextureFileName(int skintone, int skintype)
		{
			return "data/sceneassets/genheadtex/skin_" + skintone.ToString() + "_" + skintype.ToString() + ".rx3";
		}

		public string GenericBearTextureFileName()
		{
			return "data/sceneassets/genheadtex/beard_" + m_skintonecode.ToString() + "_" + m_facialhairtypecode.ToString() + "_" + m_facialhaircolorcode.ToString() + ".rx3";
		}

		public static string GenericBearTextureFileName(int skintone, int facialhairtype, int facialhaircolor)
		{
			return "data/sceneassets/genheadtex/beard_" + skintone.ToString() + "_" + facialhairtype.ToString() + "_" + facialhaircolor.ToString() + ".rx3";
		}

		public string GenericBrowTextureFileName()
		{
			return "data/sceneassets/genheadtex/brow_" + m_skintonecode.ToString() + "_" + m_eyebrowcode.ToString() + "_" + m_facialhaircolorcode.ToString() + ".rx3";
		}

		public static string GenericBrowTextureFileName(int skintone, int eyebrow, int facialhaircolor)
		{
			return "data/sceneassets/genheadtex/brow_" + skintone.ToString() + "_" + eyebrow.ToString() + "_" + facialhaircolor.ToString() + ".rx3";
		}

		public string GenericSideburnTextureFileName()
		{
			return "data/sceneassets/genheadtex/brow_" + m_skintonecode.ToString() + "_" + m_sideburnscode.ToString() + "_" + m_facialhaircolorcode.ToString() + ".rx3";
		}

		public string SpecificFaceTextureTemplateName()
		{
			int num = 2;
			if (m_FaceTextureBitmaps != null)
			{
				num = m_FaceTextureBitmaps.Length;
			}
			switch (num)
			{
			case 4:
				File.Copy(FifaEnvironment.LaunchDir + "/Templates/data/sceneassets/faces/face4_#_0_0_0_0_0_0_0_0_textures.rx3", FifaEnvironment.LaunchDir + "/Templates/data/sceneassets/faces/face_#_0_0_0_0_0_0_0_0_textures.rx3", overwrite: true);
				break;
			case 3:
				File.Copy(FifaEnvironment.LaunchDir + "/Templates/data/sceneassets/faces/face3_#_0_0_0_0_0_0_0_0_textures.rx3", FifaEnvironment.LaunchDir + "/Templates/data/sceneassets/faces/face_#_0_0_0_0_0_0_0_0_textures.rx3", overwrite: true);
				break;
			default:
				File.Copy(FifaEnvironment.LaunchDir + "/Templates/data/sceneassets/faces/face2_#_0_0_0_0_0_0_0_0_textures.rx3", FifaEnvironment.LaunchDir + "/Templates/data/sceneassets/faces/face_#_0_0_0_0_0_0_0_0_textures.rx3", overwrite: true);
				break;
			}
			return "data/sceneassets/faces/face_#_0_0_0_0_0_0_0_0_textures.rx3";
		}

		public string GenericHeadModelFileName()
		{
			return "data/sceneassets/heads/head_" + m_headtypecode.ToString() + "_1.rx3";
		}

		public string GenericHeadModelTemplateName()
		{
			return "data/sceneassets/heads/head_#_1.rx3";
		}

		public string SpecificHeadModelFileName()
		{
			return SpecificHeadModelFileName(base.Id);
		}

		public static string SpecificHeadModelFileName(int id)
		{
			return "data/sceneassets/heads/head_" + id.ToString() + "_0.rx3";
		}

		public string GenericHairModelFileName()
		{
			return GenericHairModelFileName(m_hairtypecode);
		}

		public string GenericHairModelFileName(int hairtypecode)
		{
			return "data/sceneassets/hair/hair_" + hairtypecode.ToString() + "_1_0.rx3";
		}

		public static string GenericHairLodModelFileName(int hairtypecode)
		{
			return "data/sceneassets/hairlod/hairlod_" + hairtypecode.ToString() + "_1_0.rx3";
		}

		public string GenericHairLodModelFileName()
		{
			return GenericHairLodModelFileName(m_hairtypecode);
		}

		public static string SpecificHairLodModelFileName(int id)
		{
			return "data/sceneassets/hairlod/hairlod_" + id.ToString() + "_0_0.rx3";
		}

		public string SpecificHairLodModelFileName()
		{
			return SpecificHairLodModelFileName(base.Id);
		}

		public string GenericHairModelTemplateName()
		{
			return "data/sceneassets/hair/hair_#_1_0.rx3";
		}

		public string SpecificHairModelFileName()
		{
			return SpecificHairModelFileName(base.Id);
		}

		public static string SpecificHairModelFileName(int id)
		{
			return "data/sceneassets/hair/hair_" + id.ToString() + "_0_0.rx3";
		}

		public string HeadModelFileName()
		{
			if (headclasscode == 0)
			{
				return SpecificHeadModelFileName();
			}
			return GenericHeadModelFileName();
		}

		public string HairModelFileName()
		{
			if (headclasscode == 0)
			{
				return SpecificHairModelFileName();
			}
			return GenericHairModelFileName();
		}

		public string HairLodModelFileName()
		{
			if (headclasscode == 0)
			{
				return SpecificHairLodModelFileName();
			}
			return GenericHairLodModelFileName();
		}

		public string FaceTextureFileName()
		{
			if (HasSpecificHeadModel)
			{
				return SpecificFaceTextureFileName();
			}
			return GenericFaceTextureFileName();
		}

		public string SpecificEyesTextureFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return SpecificEyesTextureFileName(base.Id);
			}
			return null;
		}

		public static string SpecificEyesTextureFileName(int id)
		{
			return "data/sceneassets/heads/eyes_" + id.ToString() + "_0_textures.rx3";
		}

		public string SpecificEyesTextureTemplateName()
		{
			return "data/sceneassets/heads/eyes_#_0_textures.rx3";
		}

		public string GenericEyesTextureFileName()
		{
			return "data/sceneassets/heads/eyes_" + m_eyecolorcode.ToString() + "_1_textures.rx3";
		}

		public string SkinTextureTemplateName()
		{
			return "data/sceneassets/body/skin_#_0_textures.rx3";
		}

		public string RevModSkinTextureTemplateName()
		{
			return "data/sceneassets/body/playerskin_#_textures.rx3";
		}

		public string SkinTextureFileName()
		{
			return "data/sceneassets/body/skin_" + m_skintonecode.ToString() + "_" + (Female ? 1 : 0) + "_textures.rx3";
		}

		public string TattoTextureFileName()
		{
			return "data/sceneassets/tattoo/tattoo_" + base.Id.ToString() + "_0.rx3";
		}

		public string RevModSkinTextureFileName()
		{
			return "data/sceneassets/body/playerskin_" + base.Id.ToString() + "_textures.rx3";
		}

		public string SpecificPhotoTemplateFileName()
		{
			return "data/ui/imgassets/heads/p#.dds";
		}

		public string SpecificPhotoDdsFileName()
		{
			return "data/ui/imgassets/heads/p" + base.Id.ToString() + ".dds";
		}

		public static string SpecificPhotoDdsFileName(int id)
		{
			return "data/ui/imgassets/heads/p" + id.ToString() + ".dds";
		}

		public bool IsPlayingFor(Team team)
		{
			if (m_PlayingForTeams == null)
			{
				return false;
			}
			foreach (Team playingForTeam in m_PlayingForTeams)
			{
				if (playingForTeam == team)
				{
					return true;
				}
			}
			return false;
		}

		public void PlayFor(Team team)
		{
			if (m_PlayingForTeams == null)
			{
				m_PlayingForTeams = new TeamList();
			}
			foreach (Team playingForTeam in m_PlayingForTeams)
			{
				if (playingForTeam == team)
				{
					return;
				}
			}
			m_PlayingForTeams.Add(team);
		}

		public void NotPlayFor(Team team)
		{
			if (m_PlayingForTeams != null)
			{
				foreach (Team playingForTeam in m_PlayingForTeams)
				{
					if (playingForTeam == team)
					{
						m_PlayingForTeams.Remove(team);
						break;
					}
				}
			}
		}

		public bool IsFreeAgent()
		{
			int num = 0;
			if (m_PlayingForTeams != null)
			{
				foreach (Team playingForTeam in m_PlayingForTeams)
				{
					if (playingForTeam.IsClub())
					{
						num++;
					}
				}
			}
			return num == 0;
		}

		public bool IsMultiClub()
		{
			int num = 0;
			if (m_PlayingForTeams != null)
			{
				foreach (Team playingForTeam in m_PlayingForTeams)
				{
					if (playingForTeam.IsClub())
					{
						num++;
					}
				}
			}
			return num > 1;
		}

		public Team GetClub()
		{
			if (m_PlayingForTeams != null)
			{
				foreach (Team playingForTeam in m_PlayingForTeams)
				{
					if (playingForTeam.IsClub())
					{
						return playingForTeam;
					}
				}
			}
			return null;
		}

		public Bitmap GetPhoto()
		{
			Bitmap bitmap = FifaEnvironment.Get2dHead(SpecificPhotoDdsFileName());
			m_HasSpecificPhoto = (bitmap != null);
			return bitmap;
		}

		public int EstimateSkills(float marketValue, int age, ERole role)
		{
			float[] array;
			switch (role)
			{
			case ERole.Right_Wing_Back:
			case ERole.Right_Back:
			case ERole.Central_Back:
			case ERole.Left_Back:
			case ERole.Left_Wing_Back:
				array = c_MarketValuesDefender;
				break;
			case ERole.Central_Defensive_Midfielder:
			case ERole.Right_Midfielder:
			case ERole.Central_Midfielder:
			case ERole.Left_Midfielder:
				array = c_MarketValuesDefender;
				break;
			case ERole.Central_Advanced_Midfielder:
			case ERole.Central_Forward:
			case ERole.Right_Wing:
			case ERole.Central_Striker:
			case ERole.Left_Wing:
				array = c_MarketValuesDefender;
				break;
			case ERole.Goalkeeper:
				array = c_MarketValuesGoalkeeper;
				break;
			default:
				array = c_MarketValuesGoalkeeper;
				break;
			}
			int num = 50;
			for (int i = 0; i < array.Length && marketValue >= array[i]; i++)
			{
				num++;
			}
			if (age < 21)
			{
				num -= 4;
			}
			else if (age >= 21 && age <= 24)
			{
				num -= 2;
			}
			else if (age >= 31 && age <= 35)
			{
				num += 2;
			}
			return num;
		}

		public int EstimateBody()
		{
			int num = height - 100 - weight;
			if (height <= 173)
			{
				if (num > 4)
				{
					m_bodytypecode = 7;
				}
				else if (num > 2)
				{
					m_bodytypecode = 8;
				}
				else
				{
					m_bodytypecode = 9;
				}
			}
			else if (height <= 184)
			{
				if (num > 8)
				{
					m_bodytypecode = 1;
				}
				else if (num > 4)
				{
					m_bodytypecode = 2;
				}
				else
				{
					m_bodytypecode = 3;
				}
			}
			else if (num > 9)
			{
				m_bodytypecode = 4;
			}
			else if (num > 5)
			{
				m_bodytypecode = 5;
			}
			else
			{
				m_bodytypecode = 6;
			}
			return m_bodytypecode;
		}

		public bool SetPhoto(Bitmap bitmap)
		{
			return FifaEnvironment.Set2dHead(SpecificPhotoTemplateFileName(), base.Id, bitmap);
		}

		public bool DeletePhoto()
		{
			return FifaEnvironment.Delete2dHead(SpecificPhotoDdsFileName());
		}

		private void ChangeHairColor(Bitmap bitmap, int color)
		{
			int num = 128;
			int num2 = 128;
			int num3 = 128;
			switch (color)
			{
			case 3:
				return;
			case 1:
				num = 64;
				num2 = 64;
				num3 = 64;
				break;
			case 2:
				num = 92;
				num2 = 92;
				num3 = 92;
				break;
			case 4:
				num = 160;
				num2 = 180;
				num3 = 128;
				break;
			case 5:
				num = 170;
				num2 = 128;
				num3 = 128;
				break;
			case 6:
				num = 192;
				num2 = 256;
				num3 = 256;
				break;
			case 7:
				num = 128;
				num2 = 150;
				num3 = 150;
				break;
			}
			for (int i = 0; i < 128; i++)
			{
				for (int j = 0; j < 128; j++)
				{
					Color pixel = bitmap.GetPixel(i, j);
					if (pixel.R != 128 || pixel.G != 128 || pixel.B != 128)
					{
						int num4 = pixel.R * num / 128;
						int num5 = pixel.G * num2 / 128;
						int num6 = pixel.B * num3 / 128;
						if (num4 > 255)
						{
							num4 = 255;
						}
						if (num5 > 255)
						{
							num5 = 255;
						}
						if (num6 > 255)
						{
							num6 = 255;
						}
						if (color == 6)
						{
							num4 = (num5 = num6);
						}
						bitmap.SetPixel(i, j, Color.FromArgb(255, num4, num5, num6));
					}
				}
			}
		}

		private void OverlapBitmaps(Bitmap lowerBitmap, Bitmap upperBitmap)
		{
			for (int i = 0; i < 128; i++)
			{
				for (int j = 0; j < 128; j++)
				{
					Color pixel = upperBitmap.GetPixel(i, j);
					if (pixel.R != 128 || pixel.G != 128 || pixel.B != 128)
					{
						lowerBitmap.SetPixel(i, j, pixel);
					}
				}
			}
		}

		public void ChangeId()
		{
			m_assetid = base.Id;
			headclasscode = ((!FifaEnvironment.IsFilePresent(SpecificHeadModelFileName())) ? 1 : 0);
			m_FaceTextureBitmaps = null;
			m_HairColorTextureBitmap = null;
			m_HairAlfaTextureBitmap = null;
			m_HeadModelFile = null;
		}

		public void CleanFaceTextures()
		{
			if (m_FaceTextureBitmaps != null)
			{
				for (int i = 0; i < m_FaceTextureBitmaps.Length; i++)
				{
					if (m_FaceTextureBitmaps[i] != null)
					{
						m_FaceTextureBitmaps[i].Dispose();
					}
				}
			}
			m_FaceTextureBitmaps = null;
		}

		public Bitmap GetFaceTexture()
		{
			if (m_FaceTextureBitmaps != null)
			{
				return m_FaceTextureBitmaps[0];
			}
			GetFaceTextures();
			if (m_FaceTextureBitmaps != null)
			{
				return m_FaceTextureBitmaps[0];
			}
			return null;
		}

		public static int GetFaceTexturesNumber()
		{
			if (FifaEnvironment.Year == 14)
			{
				return 1;
			}
			if (FifaEnvironment.Year == 15)
			{
				return 2;
			}
			if (FifaEnvironment.Year == 16)
			{
				return 4;
			}
			return 0;
		}

		public int GetFaceTexturesNumber16()
		{
			if (m_FaceTextureBitmaps != null)
			{
				return m_FaceTextureBitmaps.Length;
			}
			return 0;
		}

		public Bitmap[] GetFaceTextures()
		{
			if (m_FaceTextureBitmaps != null)
			{
				return m_FaceTextureBitmaps;
			}
			string text = null;
			if (HasSpecificHeadModel)
			{
				text = SpecificFaceTextureFileName();
				m_FaceTextureBitmaps = FifaEnvironment.GetBmpsFromRx3(text, HasSpecificHeadModel);
			}
			else
			{
				m_FaceTextureBitmaps = new Bitmap[2];
				m_FaceTextureBitmaps[0] = BuildGenericFaceTexture();
				m_FaceTextureBitmaps[1] = null;
			}
			return m_FaceTextureBitmaps;
		}

		private Bitmap BuildGenericFaceTexture()
		{
			Bitmap bmpFromRx = FifaEnvironment.GetBmpFromRx3(GenericSkinTextureFileName());
			if (bmpFromRx == null)
			{
				return null;
			}
			Rectangle destRectangle = new Rectangle(0, 0, bmpFromRx.Width, bmpFromRx.Height);
			Bitmap bmpFromRx2 = FifaEnvironment.GetBmpFromRx3(GenericBearTextureFileName());
			if (bmpFromRx2 == null)
			{
				return null;
			}
			bmpFromRx = GraphicUtil.Overlap(bmpFromRx, bmpFromRx2, destRectangle);
			bmpFromRx2 = FifaEnvironment.GetBmpFromRx3(GenericBrowTextureFileName());
			if (bmpFromRx2 == null)
			{
				return null;
			}
			return GraphicUtil.Overlap(bmpFromRx, bmpFromRx2, destRectangle);
		}

		public bool SetFaceTextures(Bitmap[] bitmaps)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(SpecificFaceTextureTemplateName(), base.Id, bitmaps, ECompressionMode.Chunkzip2);
		}

		public bool SetFaceTextures(string rx3FileName)
		{
			CleanFaceTextures();
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, SpecificFaceTextureFileName(), delete: false, ECompressionMode.Chunkzip, null);
		}

		public bool DeleteFaceTexture()
		{
			if (!HasSpecificHeadModel)
			{
				FifaEnvironment.UserMessages.ShowMessage(5039);
				return false;
			}
			bool num = FifaEnvironment.DeleteFromZdata(SpecificFaceTextureFileName());
			if (num)
			{
				m_FaceTextureBitmaps = null;
			}
			return num;
		}

		public bool SetSkinTextures(Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(RevModSkinTextureTemplateName(), base.Id, bitmap, ECompressionMode.Chunkzip, SkinSignature());
		}

		public Bitmap GetSkinTexture()
		{
			Bitmap bmpFromRx = FifaEnvironment.GetBmpFromRx3(RevModSkinTextureFileName(), 0);
			if (bmpFromRx == null)
			{
				bmpFromRx = FifaEnvironment.GetBmpFromRx3(SkinTextureFileName(), 0);
			}
			return bmpFromRx;
		}

		public bool DeleteSkinTexture()
		{
			return FifaEnvironment.DeleteFromZdata(RevModSkinTextureFileName());
		}

		private Rx3Signatures SkinSignature()
		{
			return new Rx3Signatures(175072, 24, new string[1]
			{
				"body_" + m_skintonecode.ToString() + "_cm.Raster"
			});
		}

		public bool SetTattoos(Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata(TattoTextureFileName(), base.Id, bitmap, ECompressionMode.Chunkzip);
		}

		public Bitmap GetTattoos()
		{
			return FifaEnvironment.GetBmpFromRx3(TattoTextureFileName(), 0);
		}

		public bool DeleteTattoos()
		{
			return FifaEnvironment.DeleteFromZdata(TattoTextureFileName());
		}

		public bool SetRevModSkinTextures(Bitmap bitmap)
		{
			return FifaEnvironment.ImportBmpsIntoZdata("data\\sceneassets\\body\\playerskin_#_textures.rx3", base.Id, bitmap, ECompressionMode.None, RevModSkinSignature());
		}

		public Bitmap GetRevModSkinTexture()
		{
			return FifaEnvironment.GetBmpFromRx3(RevModSkinTextureFileName(), verbose: false);
		}

		public bool DeleteRevModSkinTexture()
		{
			return FifaEnvironment.DeleteFromZdata(RevModSkinTextureFileName());
		}

		private Rx3Signatures RevModSkinSignature()
		{
			return new Rx3Signatures(175072, 24, new string[1]
			{
				"body_" + m_skintonecode.ToString() + "_cm.Raster"
			});
		}

		public Bitmap GetEyesTexture()
		{
			if (m_EyesTextureBitmap != null)
			{
				return m_EyesTextureBitmap;
			}
			string text = null;
			text = ((!HasSpecificHeadModel) ? GenericEyesTextureFileName() : ((FifaEnvironment.Year != 14) ? GenericEyesTextureFileName() : SpecificEyesTextureFileName()));
			m_EyesTextureBitmap = FifaEnvironment.GetBmpFromRx3(text, 0);
			return m_EyesTextureBitmap;
		}

		private Rx3Signatures EyesSignature()
		{
			return new Rx3Signatures(11200, 24, new string[1]
			{
				"eyes_" + base.Id.ToString() + "_0_cm.Raster"
			});
		}

		public bool SetEyesTextures(Bitmap bitmap)
		{
			if (FifaEnvironment.Year == 14)
			{
				if (!HasSpecificHeadModel)
				{
					FifaEnvironment.UserMessages.ShowMessage(5038);
					return false;
				}
				CleanEyesTexture();
				return FifaEnvironment.ImportBmpsIntoZdata(SpecificEyesTextureTemplateName(), base.Id, bitmap, ECompressionMode.Chunkzip, EyesSignature());
			}
			return false;
		}

		public bool SetEyesTextures(string rx3FileName)
		{
			if (FifaEnvironment.Year == 14)
			{
				if (!HasSpecificHeadModel)
				{
					FifaEnvironment.UserMessages.ShowMessage(5038);
					return false;
				}
				CleanEyesTexture();
				return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, SpecificEyesTextureFileName(), delete: false, ECompressionMode.Chunkzip, EyesSignature());
			}
			return false;
		}

		public void CleanEyesTexture()
		{
			_ = m_EyesTextureBitmap;
			m_EyesTextureBitmap = null;
		}

		public bool DeleteEyesTexture()
		{
			if (!HasSpecificHeadModel)
			{
				FifaEnvironment.UserMessages.ShowMessage(5039);
				return false;
			}
			bool num = FifaEnvironment.DeleteFromZdata(SpecificEyesTextureFileName());
			if (num)
			{
				m_EyesTextureBitmap = null;
			}
			return num;
		}

		public string HairTexturesFileName()
		{
			if (m_headclasscode == 0)
			{
				return SpecificHairTexturesFileName();
			}
			return GenericHairTexturesFileName();
		}

		public string SpecificHairTexturesFileName()
		{
			return SpecificHairTexturesFileName(base.Id);
		}

		public static string SpecificHairTexturesFileName(int id)
		{
			return "data/sceneassets/hair/hair_" + id.ToString() + "_0_textures.rx3";
		}

		public string GenericHairTexturesFileName()
		{
			return "data/sceneassets/hair/hair_" + m_hairtypecode.ToString() + "_1_textures.rx3";
		}

		public string HairTexturesTemplateName()
		{
			return "data/sceneassets/hair/hair_#_0_textures.rx3";
		}

		public void CleanHairTextures()
		{
			if (m_HairAlfaTextureBitmap != null)
			{
				m_HairAlfaTextureBitmap.Dispose();
			}
			m_HairAlfaTextureBitmap = null;
			if (m_HairColorTextureBitmap != null)
			{
				m_HairColorTextureBitmap.Dispose();
			}
			m_HairColorTextureBitmap = null;
		}

		public Bitmap GetGenericHairColorTexture()
		{
			if (!HasSpecificHeadModel)
			{
				return m_HairColorTextureBitmap;
			}
			Bitmap[] bmpsFromRx = FifaEnvironment.GetBmpsFromRx3(GenericHairTexturesFileName());
			if (bmpsFromRx != null)
			{
				return GraphicUtil.MultiplyColorToBitmap(bmpsFromRx[1], s_GenericColors[m_haircolorcode], s_GenericColorsDivisor, preserveAlfa: false);
			}
			return null;
		}

		public Bitmap[] GetHairTextures()
		{
			if (m_HairAlfaTextureBitmap != null && m_HairColorTextureBitmap != null)
			{
				return new Bitmap[2]
				{
					m_HairAlfaTextureBitmap,
					m_HairColorTextureBitmap
				};
			}
			Bitmap[] bmpsFromRx;
			if (HasSpecificHeadModel)
			{
				bmpsFromRx = FifaEnvironment.GetBmpsFromRx3(SpecificHairTexturesFileName(), verbose: false);
				if (bmpsFromRx != null)
				{
					m_HairAlfaTextureBitmap = bmpsFromRx[0];
					m_HairColorTextureBitmap = bmpsFromRx[1];
				}
				else
				{
					m_HairAlfaTextureBitmap = null;
					m_HairColorTextureBitmap = null;
				}
			}
			else
			{
				bmpsFromRx = FifaEnvironment.GetBmpsFromRx3(GenericHairTexturesFileName());
				if (bmpsFromRx != null)
				{
					m_HairAlfaTextureBitmap = bmpsFromRx[0];
					m_HairColorTextureBitmap = GraphicUtil.MultiplyColorToBitmap(bmpsFromRx[1], s_GenericColors[m_haircolorcode], s_GenericColorsDivisor, preserveAlfa: false);
				}
				else
				{
					m_HairColorTextureBitmap = null;
				}
			}
			return bmpsFromRx;
		}

		public bool SetHairTextures(Bitmap[] bitmaps)
		{
			if (!HasSpecificHeadModel)
			{
				FifaEnvironment.UserMessages.ShowMessage(5038);
				return false;
			}
			return FifaEnvironment.ImportBmpsIntoZdata(HairTexturesTemplateName(), base.Id, bitmaps, ECompressionMode.Chunkzip2);
		}

		public bool SetHairTextures(string rx3FileName)
		{
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, SpecificHairTexturesFileName(), delete: false, ECompressionMode.Chunkzip2);
		}

		public bool DeleteHairTextures()
		{
			if (!HasSpecificHeadModel)
			{
				FifaEnvironment.UserMessages.ShowMessage(5039);
				return false;
			}
			bool num = FifaEnvironment.DeleteFromZdata(SpecificHairTexturesFileName());
			if (num)
			{
				m_HairColorTextureBitmap = null;
				m_HairAlfaTextureBitmap = null;
			}
			return num;
		}

		public Bitmap GetHairColorTexture()
		{
			if (m_HairColorTextureBitmap != null)
			{
				return m_HairColorTextureBitmap;
			}
			GetHairTextures();
			return m_HairColorTextureBitmap;
		}

		public Bitmap GetHairAlfaTexture()
		{
			if (m_HairAlfaTextureBitmap != null)
			{
				return m_HairAlfaTextureBitmap;
			}
			GetHairTextures();
			return m_HairAlfaTextureBitmap;
		}

		public Rx3File GetHeadModel()
		{
			if (m_HeadModelFile != null)
			{
				return m_HeadModelFile;
			}
			Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float32;
			if (m_headclasscode == 0)
			{
				m_HeadModelFile = FifaEnvironment.GetRx3FromZdata(SpecificHeadModelFileName());
			}
			else
			{
				m_HeadModelFile = FifaEnvironment.GetRx3FromZdata(GenericHeadModelFileName());
			}
			return m_HeadModelFile;
		}

		public bool SetHeadModel(string rx3FileName)
		{
			rx3FileName = ConvertHeadModel(rx3FileName);
			if (rx3FileName == null)
			{
				return false;
			}
			bool num = FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, SpecificHeadModelFileName(), delete: false, ECompressionMode.Chunkzip);
			if (num)
			{
				CleanHeadModel();
				m_HeadModelFile = FifaEnvironment.GetRx3FromZdata(SpecificHeadModelFileName());
			}
			return num;
		}

		private string ConvertHeadModel(string rx3FileName)
		{
			Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float32;
			Rx3File rx3File = new Rx3File();
			rx3File.Load(rx3FileName);
			int num = 0;
			_ = FifaEnvironment.LaunchDir + "\\Templates\\data\\sceneassets\\heads\\head_16_1.rx3";
			string result = FifaEnvironment.LaunchDir + "\\Templates\\data\\sceneassets\\heads\\head_16_0.rx3";
			for (int i = 0; i < rx3File.Rx3Headers.NFiles; i++)
			{
				if (rx3File.Rx3FileDescriptors[i].IsVertexVector())
				{
					if (rx3File.Rx3FileDescriptors[i].Size == 6352)
					{
						num |= 1;
					}
					if (rx3File.Rx3FileDescriptors[i].Size == 6880)
					{
						num |= 2;
					}
					if (rx3File.Rx3FileDescriptors[i].Size == 151552)
					{
						num |= 4;
					}
					if (rx3File.Rx3FileDescriptors[i].Size == 101040)
					{
						num |= 8;
					}
				}
			}
			switch (num)
			{
			case 5:
			{
				DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(32);
				if (dialogResult == DialogResult.Yes || dialogResult == DialogResult.OK)
				{
					if (ImportHeadModelFromOtherFifa(rx3File))
					{
						return result;
					}
					return null;
				}
				return null;
			}
			case 6:
				return rx3FileName;
			default:
				FifaEnvironment.UserMessages.ShowMessage(1037);
				return null;
			}
		}

		private bool ImportHeadModelFromOtherFifa(Rx3File rx3File)
		{
			string sourceFileName = FifaEnvironment.LaunchDir + "\\Templates\\data\\sceneassets\\heads\\head_16_1.rx3";
			string text = FifaEnvironment.LaunchDir + "\\Templates\\data\\sceneassets\\heads\\head_16_0.rx3";
			File.Copy(sourceFileName, text, overwrite: true);
			Rx3File rx3File2 = new Rx3File();
			rx3File2.Load(text);
			Rx3RawData rx3RawData = new Rx3RawData(rx3File.FileName);
			new Rx3RawData(text);
			_ = rx3RawData.Rx3Headers.NFiles;
			for (int i = 0; i < rx3File2.Rx3VertexArrays.Length; i++)
			{
				for (int j = 0; j < rx3File.Rx3VertexArrays.Length; j++)
				{
					if (rx3File2.Rx3VertexArrays[i].nVertex == 3157 && rx3File.Rx3VertexArrays[j].nVertex == 3157)
					{
						for (int k = 0; k < rx3File2.Rx3VertexArrays[i].nVertex; k++)
						{
							float num = float.MaxValue;
							int num2 = -1;
							for (int l = 0; l < rx3File2.Rx3VertexArrays[i].nVertex; l++)
							{
								float num3 = (rx3File2.Rx3VertexArrays[i].Vertexes[k].U - rx3File.Rx3VertexArrays[j].Vertexes[l].U) * (rx3File2.Rx3VertexArrays[i].Vertexes[k].U - rx3File.Rx3VertexArrays[j].Vertexes[l].U) + (rx3File2.Rx3VertexArrays[i].Vertexes[k].V - rx3File.Rx3VertexArrays[j].Vertexes[l].V) * (rx3File2.Rx3VertexArrays[i].Vertexes[k].V - rx3File.Rx3VertexArrays[j].Vertexes[l].V);
								if (num3 < num)
								{
									num = num3;
									num2 = l;
								}
							}
							if (num2 != -1)
							{
								rx3File2.Rx3VertexArrays[i].Vertexes[k].X = rx3File.Rx3VertexArrays[j].Vertexes[num2].X;
								rx3File2.Rx3VertexArrays[i].Vertexes[k].Y = rx3File.Rx3VertexArrays[j].Vertexes[num2].Y;
								rx3File2.Rx3VertexArrays[i].Vertexes[k].Z = rx3File.Rx3VertexArrays[j].Vertexes[num2].Z;
							}
						}
					}
					if (rx3File2.Rx3VertexArrays[i].nVertex != 132 || rx3File.Rx3VertexArrays[j].nVertex != 132)
					{
						continue;
					}
					int num4 = 0;
					int num5 = 0;
					float num6 = 0f;
					float num7 = 0f;
					float num8 = 0f;
					float num9 = 0f;
					float num10 = 0f;
					float num11 = 0f;
					for (int m = 0; m < 132; m++)
					{
						if (rx3File2.Rx3VertexArrays[i].Vertexes[m].X > 0f)
						{
							num6 += rx3File2.Rx3VertexArrays[i].Vertexes[m].X;
							num7 += rx3File2.Rx3VertexArrays[i].Vertexes[m].Y;
							num8 += rx3File2.Rx3VertexArrays[i].Vertexes[m].Z;
							num4++;
						}
						else
						{
							num9 += rx3File2.Rx3VertexArrays[i].Vertexes[m].X;
							num10 += rx3File2.Rx3VertexArrays[i].Vertexes[m].Y;
							num11 += rx3File2.Rx3VertexArrays[i].Vertexes[m].Z;
							num5++;
						}
					}
					num6 /= (float)num4;
					num7 /= (float)num4;
					num8 /= (float)num4;
					num9 /= (float)num5;
					num10 /= (float)num5;
					num11 /= (float)num5;
					int num12 = 0;
					int num13 = 0;
					float num14 = 0f;
					float num15 = 0f;
					float num16 = 0f;
					float num17 = 0f;
					float num18 = 0f;
					float num19 = 0f;
					for (int n = 0; n < 132; n++)
					{
						if (rx3File.Rx3VertexArrays[j].Vertexes[n].X > 0f)
						{
							num14 += rx3File.Rx3VertexArrays[j].Vertexes[n].X;
							num15 += rx3File.Rx3VertexArrays[j].Vertexes[n].Y;
							num16 += rx3File.Rx3VertexArrays[j].Vertexes[n].Z;
							num12++;
						}
						else
						{
							num17 += rx3File.Rx3VertexArrays[j].Vertexes[n].X;
							num18 += rx3File.Rx3VertexArrays[j].Vertexes[n].Y;
							num19 += rx3File.Rx3VertexArrays[j].Vertexes[n].Z;
							num13++;
						}
					}
					num14 /= (float)num12;
					num15 /= (float)num12;
					num16 /= (float)num12;
					num17 /= (float)num13;
					num18 /= (float)num13;
					num19 /= (float)num13;
					float num20 = num14 - num6;
					float num21 = num15 - num7;
					float num22 = num16 - num8;
					float num23 = num17 - num9;
					float num24 = num18 - num10;
					float num25 = num19 - num11;
					for (int num26 = 0; num26 < 132; num26++)
					{
						if (rx3File2.Rx3VertexArrays[i].Vertexes[num26].X > 0f)
						{
							rx3File2.Rx3VertexArrays[i].Vertexes[num26].X += num20;
							rx3File2.Rx3VertexArrays[i].Vertexes[num26].Y += num21;
							rx3File2.Rx3VertexArrays[i].Vertexes[num26].Z += num22;
						}
						else
						{
							rx3File2.Rx3VertexArrays[i].Vertexes[num26].X += num23;
							rx3File2.Rx3VertexArrays[i].Vertexes[num26].Y += num24;
							rx3File2.Rx3VertexArrays[i].Vertexes[num26].Z += num25;
						}
					}
				}
			}
			rx3File2.Save(text, saveBitmaps: false, saveVertex: true);
			return true;
		}

		public bool DeleteHeadModel()
		{
			bool num = FifaEnvironment.DeleteFromZdata(SpecificHeadModelFileName());
			if (num)
			{
				m_HeadModelFile = null;
			}
			return num;
		}

		public void CleanHeadModel()
		{
			m_HeadModelFile = null;
		}

		public void CleanHead()
		{
			CleanFaceTextures();
			CleanEyesTexture();
			CleanHeadModel();
		}

		public Rx3File GetHairModel()
		{
			if (m_HairModelFile != null)
			{
				return m_HairModelFile;
			}
			Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float16;
			if (m_headclasscode == 0)
			{
				m_HairModelFile = FifaEnvironment.GetRx3FromZdata(SpecificHairModelFileName(), verbose: false);
			}
			else
			{
				m_HairModelFile = FifaEnvironment.GetRx3FromZdata(GenericHairModelFileName());
			}
			return m_HairModelFile;
		}

		public bool SetHairLodModel(string rx3FileName)
		{
			if (!HasSpecificHeadModel)
			{
				FifaEnvironment.UserMessages.ShowMessage(5062);
				return false;
			}
			return FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, SpecificHairLodModelFileName(), delete: false, ECompressionMode.Chunkzip);
		}

		public bool SetHairModel(string rx3FileName)
		{
			if (!HasSpecificHeadModel)
			{
				FifaEnvironment.UserMessages.ShowMessage(5062);
				return false;
			}
			bool num = FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, SpecificHairModelFileName(), delete: false, ECompressionMode.Chunkzip);
			if (num)
			{
				m_HairModelFile = FifaEnvironment.GetRx3FromZdata(SpecificHairModelFileName());
			}
			return num;
		}

		public bool UpdateHairVertex(CustomVertex.PositionNormalTextured[] newVertex4, CustomVertex.PositionNormalTextured[] newVertex5)
		{
			if (!HasSpecificHeadModel)
			{
				FifaEnvironment.UserMessages.ShowMessage(5062);
				return false;
			}
			FifaEnvironment.FifaFat.ExtractFile(SpecificHairModelFileName());
			Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float16;
			m_HairModelFile = new Rx3File();
			m_HairModelFile.Load(FifaEnvironment.GameDir + SpecificHairModelFileName());
			if (m_HairModelFile.Rx3VertexArrays == null)
			{
				return false;
			}
			if (newVertex4 != null && m_HairModelFile.Rx3VertexArrays.Length >= 1 && m_HairModelFile.Rx3VertexArrays[0] != null)
			{
				m_HairModelFile.Rx3VertexArrays[0].SetVertex(newVertex4);
			}
			if (newVertex5 != null && m_HairModelFile.Rx3VertexArrays.Length >= 2 && m_HairModelFile.Rx3VertexArrays[1] != null)
			{
				m_HairModelFile.Rx3VertexArrays[1].SetVertex(newVertex5);
			}
			m_HairModelFile.Save(FifaEnvironment.GameDir + SpecificHairModelFileName(), saveBitmaps: false, saveVertex: true);
			return true;
		}

		public bool UpdateHeadVertex(CustomVertex.PositionNormalTextured[] newVertexHead, CustomVertex.PositionNormalTextured[] newVertexEyes)
		{
			if (!HasSpecificHeadModel)
			{
				FifaEnvironment.UserMessages.ShowMessage(5062);
				return false;
			}
			FifaEnvironment.FifaFat.ExtractFile(SpecificHeadModelFileName());
			Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float32;
			m_HeadModelFile = new Rx3File();
			m_HeadModelFile.Load(FifaEnvironment.GameDir + SpecificHeadModelFileName());
			if (m_HeadModelFile.Rx3VertexArrays == null)
			{
				return false;
			}
			for (int i = 0; i < m_HeadModelFile.Rx3VertexArrays.Length; i++)
			{
				if (m_HeadModelFile.Rx3VertexArrays[i].nVertex == newVertexHead.Length)
				{
					m_HeadModelFile.Rx3VertexArrays[i].SetVertex(newVertexHead);
				}
				else if (m_HeadModelFile.Rx3VertexArrays[i].nVertex == newVertexEyes.Length)
				{
					m_HeadModelFile.Rx3VertexArrays[i].SetVertex(newVertexEyes);
				}
			}
			m_HeadModelFile.Save(FifaEnvironment.GameDir + SpecificHeadModelFileName(), saveBitmaps: false, saveVertex: true);
			return true;
		}

		public bool DeleteHairModel()
		{
			if (!HasSpecificHeadModel)
			{
				FifaEnvironment.UserMessages.ShowMessage(5062);
				return false;
			}
			bool num = FifaEnvironment.DeleteFromZdata(SpecificHairModelFileName());
			if (num)
			{
				m_HairModelFile = null;
			}
			return num;
		}

		public bool DeleteHairLodModel()
		{
			if (!HasSpecificHeadModel)
			{
				FifaEnvironment.UserMessages.ShowMessage(5062);
				return false;
			}
			return FifaEnvironment.DeleteFromZdata(SpecificHairLodModelFileName());
		}

		public void CleanHairModel()
		{
			m_HairModelFile = null;
		}

		public void CleanHair()
		{
			CleanHairModel();
			CleanHairTextures();
		}

		public void CleanAllHead()
		{
			CleanHair();
			CleanHead();
		}

		public bool SetHairTextures(Bitmap colorBitmap, Bitmap alfaBitmap)
		{
			if (!HasSpecificHeadModel)
			{
				FifaEnvironment.UserMessages.ShowMessage(5038);
				return false;
			}
			return SetHairTextures(new Bitmap[2]
			{
				alfaBitmap,
				colorBitmap
			});
		}

		public void SavePlayer(Record r)
		{
			string name = null;
			s_PlayerNames.TryGetValue(m_firstnameid, out name, isUsed: true);
			if (name != m_firstname)
			{
				m_firstnameid = s_PlayerNames.GetKey(m_firstname);
			}
			name = null;
			s_PlayerNames.TryGetValue(m_lastnameid, out name, isUsed: true);
			if (name != m_lastname)
			{
				m_lastnameid = s_PlayerNames.GetKey(m_lastname, m_commentaryid);
			}
			name = null;
			s_PlayerNames.TryGetValue(m_commonnameid, out name, isUsed: true);
			if (name != m_commonname)
			{
				m_commonnameid = s_PlayerNames.GetKey(m_commonname);
			}
			if (m_commentaryid > 900000)
			{
				if (m_audioname == m_commonname)
				{
					s_PlayerNames.SetCommentaryId(m_commonnameid, m_commentaryid);
				}
				else if (m_audioname == m_lastname)
				{
					s_PlayerNames.SetCommentaryId(m_lastnameid, m_commentaryid);
				}
			}
			name = null;
			s_PlayerNames.TryGetValue(m_playerjerseynameid, out name, isUsed: true);
			if (name != m_playerjerseyname)
			{
				m_playerjerseynameid = s_PlayerNames.GetKey(m_playerjerseyname);
			}
			r.IntField[FI.players_playerid] = base.Id;
			r.IntField[FI.players_firstnameid] = m_firstnameid;
			r.IntField[FI.players_lastnameid] = m_lastnameid;
			r.IntField[FI.players_commonnameid] = m_commonnameid;
			r.IntField[FI.players_playerjerseynameid] = m_playerjerseynameid;
			r.IntField[FI.players_birthdate] = FifaUtil.ConvertFromDate(m_birthdate);
			r.IntField[FI.players_playerjointeamdate] = FifaUtil.ConvertFromDate(m_playerjointeamdate);
			r.IntField[FI.players_contractvaliduntil] = m_contractvaliduntil;
			r.IntField[FI.players_height] = m_height;
			r.IntField[FI.players_weight] = m_weight;
			r.IntField[FI.players_preferredposition1] = m_preferredposition1;
			r.IntField[FI.players_preferredposition2] = m_preferredposition2;
			r.IntField[FI.players_preferredposition3] = m_preferredposition3;
			r.IntField[FI.players_preferredposition4] = m_preferredposition4;
			r.IntField[FI.players_preferredfoot] = m_preferredfoot + 1;
			r.IntField[FI.players_jerseysleevelengthcode] = m_jerseysleevelengthcode;
			r.IntField[FI.players_jerseystylecode] = m_jerseystylecode;
			r.IntField[FI.players_hasseasonaljersey] = m_hasseasonaljersey;
			r.IntField[FI.players_animfreekickstartposcode] = m_animfreekickstartposcode;
			r.IntField[FI.players_animpenaltieskickstylecode] = m_animpenaltieskickstylecode;
			r.IntField[FI.players_animpenaltiesmotionstylecode] = m_animpenaltiesmotionstylecode;
			r.IntField[FI.players_animpenaltiesstartposcode] = m_animpenaltiesstartposcode;
			r.IntField[FI.players_accessorycode1] = m_accessorycode1;
			r.IntField[FI.players_accessorycolourcode1] = m_accessorycolourcode1;
			r.IntField[FI.players_accessorycode2] = m_accessorycode2;
			r.IntField[FI.players_accessorycolourcode2] = m_accessorycolourcode2;
			r.IntField[FI.players_accessorycode3] = m_accessorycode3;
			r.IntField[FI.players_accessorycolourcode3] = m_accessorycolourcode3;
			r.IntField[FI.players_accessorycode4] = m_accessorycode4;
			r.IntField[FI.players_accessorycolourcode4] = m_accessorycolourcode4;
			r.IntField[FI.players_acceleration] = m_acceleration;
			r.IntField[FI.players_aggression] = m_aggression;
			r.IntField[FI.players_sprintspeed] = m_sprintspeed;
			r.IntField[FI.players_stamina] = m_stamina;
			r.IntField[FI.players_strength] = m_strength;
			r.IntField[FI.players_marking] = m_marking;
			r.IntField[FI.players_standingtackle] = m_standingtackle;
			r.IntField[FI.players_slidingtackle] = m_slidingtackle;
			r.IntField[FI.players_ballcontrol] = m_ballcontrol;
			r.IntField[FI.players_dribbling] = m_dribbling;
			r.IntField[FI.players_crossing] = m_crossing;
			r.IntField[FI.players_headingaccuracy] = m_headingaccuracy;
			r.IntField[FI.players_shortpassing] = m_shortpassing;
			r.IntField[FI.players_longpassing] = m_longpassing;
			r.IntField[FI.players_longshots] = m_longshots;
			r.IntField[FI.players_finishing] = m_finishing;
			r.IntField[FI.players_shotpower] = m_shotpower;
			r.IntField[FI.players_reactions] = m_reactions;
			r.IntField[FI.players_gkreflexes] = m_gkreflexes;
			r.IntField[FI.players_gkglovetypecode] = m_gkglovetypecode;
			r.IntField[FI.players_agility] = m_agility;
			r.IntField[FI.players_balance] = m_balance;
			r.IntField[FI.players_gkkicking] = m_gkkicking;
			r.IntField[FI.players_gkkickstyle] = m_gkkickstyle;
			r.IntField[FI.players_jumping] = m_jumping;
			r.IntField[FI.players_penalties] = m_penalties;
			r.IntField[FI.players_vision] = m_vision;
			r.IntField[FI.players_volleys] = m_volleys;
			r.IntField[FI.players_skillmoves] = m_skillmoves - 1;
			r.IntField[FI.players_usercaneditname] = m_usercaneditname;
			r.IntField[FI.players_gender] = (m_gender ? 1 : 0);
			r.IntField[FI.players_emotion] = m_emotion;
			r.IntField[FI.players_gkhandling] = m_gkhandling;
			r.IntField[FI.players_gkdiving] = m_gkdiving;
			r.IntField[FI.players_gkpositioning] = m_gkpositioning;
			r.IntField[FI.players_positioning] = m_positioning;
			r.IntField[FI.players_potential] = m_potential;
			r.IntField[FI.players_freekickaccuracy] = m_freekickaccuracy;
			r.IntField[FI.players_nationality] = m_nationality;
			r.IntField[FI.players_finishingcode1] = m_finishingcode1;
			r.IntField[FI.players_finishingcode2] = m_finishingcode2;
			r.IntField[FI.players_runningcode1] = m_runningcode1;
			r.IntField[FI.players_runningcode2] = m_runningcode2;
			r.IntField[FI.players_gksavetype] = m_gksavetype;
			r.IntField[FI.players_faceposercode] = m_faceposercode;
			r.IntField[FI.players_isretiring] = m_isretiring;
			r.IntField[FI.players_socklengthcode] = m_socklengthcode;
			r.IntField[FI.players_hashighqualityhead] = (m_hashighqualityhead ? 1 : 0);
			r.IntField[FI.players_attackingworkrate] = m_attackingworkrate;
			r.IntField[FI.players_defensiveworkrate] = m_defensiveworkrate;
			r.IntField[FI.players_shortstyle] = (m_shortstyle ? 1 : 0);
			int num = 0;
			num |= (m_Inflexible ? 1 : 0);
			num |= (m_Longthrows ? 2 : 0);
			num |= (m_PowerfulFreeKicks ? 4 : 0);
			num |= (m_Diver ? 8 : 0);
			num |= (m_InjuryProne ? 16 : 0);
			num |= (m_InjuryFree ? 32 : 0);
			num |= (m_AvoidsWeakFoot ? 64 : 0);
			num |= (m_Divesintotackles ? 128 : 0);
			num |= (m_BeatDefensiveLine ? 256 : 0);
			num |= (m_Selfish ? 512 : 0);
			num |= (m_Leadership ? 1024 : 0);
			num |= (m_ArguesWithOfficials ? 2048 : 0);
			num |= (m_Earlycrosser ? 4096 : 0);
			num |= (m_FinesseShot ? 8192 : 0);
			num |= (m_Flair ? 16384 : 0);
			num |= (m_LongPasser ? 32768 : 0);
			num |= (m_LongShotTaker ? 65536 : 0);
			num |= (m_Technicaldribbler ? 131072 : 0);
			num |= (m_Playmaker ? 262144 : 0);
			num |= (m_Pushesupforcorners ? 524288 : 0);
			num |= (m_Puncher ? 1048576 : 0);
			num |= (m_GkLongThrower ? 2097152 : 0);
			num |= (m_PowerHeader ? 4194304 : 0);
			num |= (m_GkOneOnOne ? 8388608 : 0);
			num |= (m_GiantThrow ? 16777216 : 0);
			num |= (m_OutsideFootShot ? 33554432 : 0);
			num |= (m_CrowdFavorite ? 67108864 : 0);
			num |= (m_SwervePasser ? 134217728 : 0);
			num |= (m_SecondWind ? 268435456 : 0);
			num |= (m_AcrobaticClearance ? 536870912 : 0);
			num |= (r.IntField[FI.players_trait1] = num);
			int num2 = 0;
			num2 |= (m_FancyFeet ? 1 : 0);
			num2 |= (m_FancyPasses ? 2 : 0);
			num2 |= (m_FancyFlicks ? 4 : 0);
			num2 |= (m_StutterPenalty ? 8 : 0);
			num2 |= (m_ChipperPenalty ? 16 : 0);
			num2 |= (m_BycicleKick ? 32 : 0);
			num2 |= (m_DivingHeader ? 64 : 0);
			num2 |= (m_DrivenPass ? 128 : 0);
			num2 |= (m_GkFlatKick ? 256 : 0);
			num2 |= (m_HighClubIdentification ? 512 : 0);
			num2 |= (m_TeamPlayer ? 1024 : 0);
			r.IntField[FI.players_trait2] = num2;
			r.IntField[FI.players_bodytypecode] = m_bodytypecode + 1;
			r.IntField[FI.players_weakfootabilitytypecode] = m_weakfootabilitytypecode;
			r.IntField[FI.players_curve] = m_curve;
			r.IntField[FI.players_internationalrep] = m_internationalrep + 1;
			r.IntField[FI.players_eyecolorcode] = m_eyecolorcode;
			r.IntField[FI.players_eyebrowcode] = m_eyebrowcode;
			r.IntField[FI.players_hairtypecode] = m_hairtypecode;
			r.IntField[FI.players_headtypecode] = m_headtypecode;
			r.IntField[FI.players_headclasscode] = m_headclasscode;
			r.IntField[FI.players_haircolorcode] = m_haircolorcode;
			r.IntField[FI.players_facialhairtypecode] = m_facialhairtypecode;
			r.IntField[FI.players_facialhaircolorcode] = m_facialhaircolorcode;
			r.IntField[FI.players_sideburnscode] = 0;
			r.IntField[FI.players_skintypecode] = m_skintypecode;
			r.IntField[FI.players_skintonecode] = m_skintonecode;
			r.IntField[FI.players_shoecolorcode1] = m_shoecolorcode1;
			r.IntField[FI.players_shoetypecode] = m_shoetypecode;
			r.IntField[FI.players_overallrating] = m_overallrating;
			r.IntField[FI.players_interceptions] = m_interceptions;
			r.IntField[FI.players_shoecolorcode2] = m_shoecolorcode2;
			r.IntField[FI.players_jerseyfit] = (m_jerseyfit ? 1 : 0);
			r.IntField[FI.players_shoedesigncode] = m_shoedesigncode;
		}

		public int RandomizeWeight()
		{
			int num = m_height - 110;
			m_weight = m_Randomizer.Next(num, num + 11);
			return m_weight;
		}

		public bool RandomizeSkillsAround(int overall, int error)
		{
			if (overall < 10 || overall > 99)
			{
				return false;
			}
			int level = s_MeanLevels.Length - 1;
			for (int i = 0; i < s_MeanLevels.Length; i++)
			{
				if (overall <= s_MeanLevels[i])
				{
					level = i;
					break;
				}
			}
			do
			{
				RandomizeAttributes(level);
			}
			while (m_overallrating < overall - error || m_overallrating > overall + error);
			return true;
		}

		public bool RandomizeSkillsExactly(int overall)
		{
			if (overall < 10 || overall > 99)
			{
				return false;
			}
			int level = s_MeanLevels.Length - 1;
			for (int i = 0; i < s_MeanLevels.Length; i++)
			{
				if (overall <= s_MeanLevels[i])
				{
					level = i;
					break;
				}
			}
			RandomizeAttributes(level);
			if (overall != m_overallrating)
			{
				ChangeSkills(overall - m_overallrating);
			}
			return true;
		}

		public void RandomizeAttributes(int level)
		{
			int num = s_MeanLevels[level];
			int preferredposition = m_preferredposition1;
			int val = c_Attributes[preferredposition, 0] * num / 100;
			val = FifaUtil.Limit(val, 11, 90);
			int val2 = c_Attributes[preferredposition, 1] * num / 100;
			val2 = FifaUtil.Limit(val2, 11, 90);
			int val3 = c_Attributes[preferredposition, 2] * num / 100;
			val3 = FifaUtil.Limit(val3, 11, 90);
			int val4 = c_Attributes[preferredposition, 3] * num / 100;
			val4 = FifaUtil.Limit(val4, 11, 90);
			m_gkreflexes = m_Randomizer.Next(val - 10, val + 10);
			m_gkdiving = m_Randomizer.Next(val - 10, val + 10);
			m_gkpositioning = m_Randomizer.Next(val - 10, val + 10);
			m_gkhandling = m_Randomizer.Next(val - 10, val + 10);
			m_gkkicking = m_Randomizer.Next(val - 10, val + 10);
			int num2 = 20 + level * 8 + (m_height - 175) * 2;
			if (num2 > 90)
			{
				num2 = 90;
			}
			if (num2 < 20)
			{
				num2 = 20;
			}
			m_headingaccuracy = m_Randomizer.Next(num2 - 10, num2 + 10);
			m_marking = m_Randomizer.Next(val2 - 7, val2 + 7);
			m_interceptions = m_Randomizer.Next(val2 - 7, val2 + 7);
			m_standingtackle = m_Randomizer.Next(val2 - 7, val2 + 7);
			m_slidingtackle = m_Randomizer.Next(val2 - 7, val2 + 7);
			m_aggression = m_Randomizer.Next(val2 - 10, val2 + 10);
			m_shortpassing = m_Randomizer.Next(val3 - 7, val3 + 7);
			m_longpassing = m_Randomizer.Next(val3 - 7, val3 + 7);
			m_crossing = m_Randomizer.Next(val3 - 7, val3 + 7);
			m_ballcontrol = m_Randomizer.Next(val3 - 7, val3 + 7);
			m_vision = m_Randomizer.Next(val3 - 10, val3 + 10);
			m_weakfootabilitytypecode = m_Randomizer.Next(1, 6);
			m_curve = m_Randomizer.Next(val3 - 10, val3 + 10);
			m_finishing = m_Randomizer.Next(val4 - 7, val4 + 7);
			m_shotpower = m_Randomizer.Next(val4 - 10, val4 + 10);
			m_longshots = m_Randomizer.Next(val4 - 10, val4 + 10);
			m_dribbling = m_Randomizer.Next(val4 - 7, val4 + 7);
			m_volleys = m_Randomizer.Next(val4 - 10, val4 + 10);
			m_penalties = m_Randomizer.Next(val4 - 10, val4 + 10);
			m_positioning = m_Randomizer.Next(val4 - 10, val4 + 10);
			int minValue = 25 + level * 9;
			int num3 = 35 + level * 9;
			int maxValue = 72 + level * 4;
			m_acceleration = m_Randomizer.Next(minValue, maxValue);
			m_sprintspeed = m_Randomizer.Next(minValue, maxValue);
			m_stamina = m_Randomizer.Next(minValue, maxValue);
			m_strength = m_Randomizer.Next(minValue, maxValue);
			m_freekickaccuracy = m_Randomizer.Next(num3 - 10, num3 + 10);
			m_reactions = m_Randomizer.Next(num3 - 10, num3 + 10);
			m_agility = m_Randomizer.Next(num3 - 10, num3 + 10);
			m_balance = m_Randomizer.Next(num3 - 10, num3 + 10);
			m_jumping = m_Randomizer.Next(num3 - 10, num3 + 10);
			m_overallrating = GetAverageRoleAttribute();
			int num4 = 2000 + FifaEnvironment.Year - m_birthdate.Year;
			if (num4 > 30)
			{
				m_potential = m_overallrating;
			}
			else
			{
				if (num4 < 16)
				{
					num4 = 16;
				}
				if (num4 > 25)
				{
					num4 = 25;
				}
				minValue = 1 + (25 - num4) / 1;
				maxValue = 5 + (25 - num4) * 4;
				m_potential = m_Randomizer.Next(m_overallrating + minValue, m_overallrating + maxValue);
				if (m_potential > 96)
				{
					m_potential = 96;
				}
			}
			m_InjuryFree = (m_Randomizer.Next(0, 100) <= 5);
			if (m_InjuryFree)
			{
				m_InjuryProne = false;
			}
			else
			{
				m_InjuryProne = (m_Randomizer.Next(0, 100) <= 5);
			}
			if (level >= 4)
			{
				m_Leadership = (m_Randomizer.Next(0, 100) <= 5);
			}
			m_ArguesWithOfficials = (m_Randomizer.Next(0, 100) <= 5);
			m_AvoidsWeakFoot = (m_Randomizer.Next(0, 100) <= 5);
			if (m_preferredposition1 == 0)
			{
				m_Pushesupforcorners = (m_Randomizer.Next(0, 100) <= 5);
				m_Puncher = (m_Randomizer.Next(0, 100) <= 5);
				m_GkLongThrower = (m_Randomizer.Next(0, 100) <= 5);
				m_gksavetype = ((m_Randomizer.Next(0, 100) <= 20) ? 1 : 0);
				m_GkOneOnOne = (m_Randomizer.Next(0, 100) <= 5);
			}
			else
			{
				if (val4 >= 75)
				{
					m_Selfish = (m_Randomizer.Next(0, 100) <= 5);
				}
				if (val3 >= 70)
				{
					m_Playmaker = (m_Randomizer.Next(0, 100) <= 5);
				}
				else
				{
					m_Playmaker = false;
				}
				m_Diver = (m_Randomizer.Next(0, 100) <= 5);
				if (preferredposition < 12)
				{
					m_Divesintotackles = (m_Randomizer.Next(0, 100) <= 5);
				}
				m_LongShotTaker = (m_Randomizer.Next(0, 100) <= 5);
				m_Earlycrosser = (m_Randomizer.Next(0, 100) <= 5);
				m_LongPasser = (m_Randomizer.Next(0, 100) <= 5);
				m_Longthrows = (m_Randomizer.Next(0, 100) <= 2);
				m_Inflexible = (m_Randomizer.Next(0, 100) <= 2);
				if (m_dribbling > 75)
				{
					m_Technicaldribbler = (m_Randomizer.Next(0, 100) <= m_dribbling - 70);
				}
				if (m_curve > 75)
				{
					m_OutsideFootShot = (m_Randomizer.Next(0, 100) <= m_curve - 70);
				}
				if (val3 >= 80 || val4 >= 85)
				{
					m_Flair = (m_Randomizer.Next(0, 100) <= 10);
					m_FinesseShot = (m_Randomizer.Next(0, 100) <= 10);
				}
				m_PowerfulFreeKicks = (m_Randomizer.Next(0, 100) <= 5);
				if ((double)m_height > 1.85)
				{
					m_PowerHeader = (m_Randomizer.Next(0, 100) <= m_height - 180);
				}
				m_BeatDefensiveLine = (m_Randomizer.Next(0, 100) <= 5);
			}
			m_animfreekickstartposcode = m_Randomizer.Next(0, 10);
			m_animpenaltieskickstylecode = m_Randomizer.Next(0, 3);
			m_animpenaltiesmotionstylecode = m_Randomizer.Next(0, 7);
			m_animpenaltiesstartposcode = m_Randomizer.Next(0, 9);
			int num5 = level - 1;
			m_internationalrep = ((num5 < 1) ? 1 : num5);
			m_overallrating = GetAverageRoleAttribute();
		}

		public void RandomizeIdentity()
		{
			m_height = m_Randomizer.Next(165, 196);
			m_bodytypecode = m_Randomizer.Next(0, 3);
			m_weight = m_height - 110 + m_bodytypecode * 5 + m_Randomizer.Next(0, 5);
			m_preferredfoot = ((m_Randomizer.Next(0, 100) < 10) ? 1 : 0);
			m_weakfootabilitytypecode = m_Randomizer.Next(1, 6);
			m_jerseysleevelengthcode = ((m_Randomizer.Next(0, 100) < 5) ? 1 : 0);
			m_jerseystylecode = ((m_Randomizer.Next(0, 100) < 10) ? 1 : 0);
			m_hasseasonaljersey = ((m_Randomizer.Next(0, 100) < 40) ? 1 : 0);
			int days = m_Randomizer.Next(1, 7665);
			m_birthdate = new DateTime(1974, 1, 1) + new TimeSpan(days, 0, 0, 0);
		}

		public void RandomizeAppearanceSameRace()
		{
			_ = new int[10]
			{
				1,
				0,
				1,
				0,
				1,
				3,
				2,
				4,
				3,
				3
			};
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < GenericHead.c_CaucasicModels.Length; i++)
			{
				if (GenericHead.c_CaucasicModels[i] == m_headtypecode)
				{
					m_headtypecode = GenericHead.c_CaucasicModels[m_Randomizer.Next(0, GenericHead.c_CaucasicModels.Length)];
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				for (int j = 0; j < GenericHead.c_AfricanModels.Length; j++)
				{
					if (GenericHead.c_AfricanModels[j] == m_headtypecode)
					{
						m_headtypecode = GenericHead.c_AfricanModels[m_Randomizer.Next(0, GenericHead.c_AfricanModels.Length)];
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				for (int k = 0; k < GenericHead.c_AsiaticModels.Length; k++)
				{
					if (GenericHead.c_AsiaticModels[k] == m_headtypecode)
					{
						m_headtypecode = GenericHead.c_AsiaticModels[m_Randomizer.Next(0, GenericHead.c_AsiaticModels.Length)];
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				for (int l = 0; l < GenericHead.c_LatinModels.Length; l++)
				{
					if (GenericHead.c_LatinModels[l] == m_headtypecode)
					{
						m_headtypecode = GenericHead.c_LatinModels[m_Randomizer.Next(0, GenericHead.c_LatinModels.Length)];
						flag = true;
						break;
					}
				}
			}
			for (int m = 0; m < GenericHead.c_ShavenModels.Length; m++)
			{
				if (GenericHead.c_ShavenModels[m] == m_hairtypecode)
				{
					m_hairtypecode = GenericHead.c_ShavenModels[m_Randomizer.Next(0, GenericHead.c_ShavenModels.Length)];
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				for (int n = 0; n < GenericHead.c_VeryShortModels.Length; n++)
				{
					if (GenericHead.c_VeryShortModels[n] == m_hairtypecode)
					{
						m_hairtypecode = GenericHead.c_VeryShortModels[m_Randomizer.Next(0, GenericHead.c_VeryShortModels.Length)];
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				for (int num = 0; num < GenericHead.c_ShortModels.Length; num++)
				{
					if (GenericHead.c_ShortModels[num] == m_hairtypecode)
					{
						m_hairtypecode = GenericHead.c_ShortModels[m_Randomizer.Next(0, GenericHead.c_ShortModels.Length)];
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				for (int num2 = 0; num2 < GenericHead.c_ModernModels.Length; num2++)
				{
					if (GenericHead.c_ModernModels[num2] == m_hairtypecode)
					{
						m_hairtypecode = GenericHead.c_ModernModels[m_Randomizer.Next(0, GenericHead.c_ModernModels.Length)];
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				for (int num3 = 0; num3 < GenericHead.c_MediumModels.Length; num3++)
				{
					if (GenericHead.c_MediumModels[num3] == m_hairtypecode)
					{
						m_hairtypecode = GenericHead.c_MediumModels[m_Randomizer.Next(0, GenericHead.c_MediumModels.Length)];
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				for (int num4 = 0; num4 < GenericHead.c_LongModels.Length; num4++)
				{
					if (GenericHead.c_LongModels[num4] == m_hairtypecode)
					{
						m_hairtypecode = GenericHead.c_LongModels[m_Randomizer.Next(0, GenericHead.c_LongModels.Length)];
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				for (int num5 = 0; num5 < GenericHead.c_HeadbendModels.Length; num5++)
				{
					if (GenericHead.c_HeadbendModels[num5] == m_hairtypecode)
					{
						m_hairtypecode = GenericHead.c_HeadbendModels[m_Randomizer.Next(0, GenericHead.c_HeadbendModels.Length)];
						flag2 = true;
						break;
					}
				}
			}
			if (flag2)
			{
				return;
			}
			int num6 = 0;
			while (true)
			{
				if (num6 < GenericHead.c_AfroModels.Length)
				{
					if (GenericHead.c_AfroModels[num6] == m_hairtypecode)
					{
						break;
					}
					num6++;
					continue;
				}
				return;
			}
			m_hairtypecode = GenericHead.c_AfroModels[m_Randomizer.Next(0, GenericHead.c_AfroModels.Length)];
			flag2 = true;
		}

		public void RandomizeAppearanceSimilarTo(Player similarPlayer)
		{
			m_headtypecode = similarPlayer.headtypecode;
			m_hairtypecode = similarPlayer.hairtypecode;
			m_haircolorcode = similarPlayer.m_haircolorcode;
			m_facialhaircolorcode = similarPlayer.m_facialhaircolorcode;
			m_facialhairtypecode = similarPlayer.m_facialhairtypecode;
			m_skintonecode = similarPlayer.m_skintonecode;
			m_skintypecode = similarPlayer.m_skintypecode;
			m_eyebrowcode = similarPlayer.m_eyebrowcode;
			m_eyecolorcode = similarPlayer.m_eyecolorcode;
			RandomizeAppearanceSameRace();
		}

		public void RandomizeCaucasianAppearance()
		{
			int[] array = new int[10]
			{
				1,
				0,
				1,
				0,
				1,
				3,
				2,
				4,
				3,
				3
			};
			if (m_Randomizer.Next(1, 11) <= 7)
			{
				m_headtypecode = GenericHead.c_CaucasicModels[m_Randomizer.Next(0, GenericHead.c_CaucasicModels.Length)];
			}
			else
			{
				m_headtypecode = GenericHead.c_LatinModels[m_Randomizer.Next(0, GenericHead.c_LatinModels.Length)];
			}
			m_hairtypecode = m_Randomizer.Next(0, 125);
			m_haircolorcode = m_Randomizer.Next(0, 8);
			m_sideburnscode = 0;
			m_skintonecode = m_Randomizer.Next(1, 5);
			if (m_skintonecode == 1)
			{
				m_skintonecode = 2;
			}
			if (m_skintonecode == 3)
			{
				m_skintonecode = 2;
			}
			m_skintypecode = m_Randomizer.Next(0, 3);
			m_eyecolorcode = m_Randomizer.Next(1, 11);
			m_facialhairtypecode = m_Randomizer.Next(0, 20);
			if (m_facialhairtypecode == 13 || m_facialhairtypecode > 15)
			{
				m_facialhairtypecode = 0;
			}
			m_facialhaircolorcode = array[m_haircolorcode];
		}

		public void RandomizeAsiaticAppearance()
		{
			int[] array = new int[10]
			{
				1,
				0,
				1,
				0,
				1,
				3,
				2,
				4,
				3,
				3
			};
			if (m_Randomizer.Next(1, 11) <= 9)
			{
				m_headtypecode = GenericHead.c_AsiaticModels[m_Randomizer.Next(0, GenericHead.c_AsiaticModels.Length)];
			}
			else
			{
				m_headtypecode = GenericHead.c_CaucasicModels[m_Randomizer.Next(0, GenericHead.c_CaucasicModels.Length)];
			}
			m_hairtypecode = m_Randomizer.Next(0, 125);
			m_haircolorcode = m_Randomizer.Next(1, 6);
			if (m_haircolorcode == 3)
			{
				m_haircolorcode = 1;
			}
			m_sideburnscode = 0;
			m_skintonecode = m_Randomizer.Next(3, 7);
			if (m_skintonecode == 3)
			{
				m_skintonecode = 4;
			}
			m_skintypecode = m_Randomizer.Next(0, 3);
			m_eyecolorcode = m_Randomizer.Next(3, 11);
			m_facialhairtypecode = m_Randomizer.Next(0, 20);
			if (m_facialhairtypecode == 11 || m_facialhairtypecode == 13 || m_facialhairtypecode > 15)
			{
				m_facialhairtypecode = 0;
			}
			m_facialhaircolorcode = array[m_haircolorcode];
		}

		public void RandomizeAfricanAppearance()
		{
			int[] array = new int[10]
			{
				1,
				0,
				1,
				0,
				1,
				3,
				2,
				4,
				3,
				3
			};
			if (m_Randomizer.Next(1, 11) <= 7)
			{
				m_headtypecode = GenericHead.c_AfricanModels[m_Randomizer.Next(0, GenericHead.c_AfricanModels.Length)];
			}
			else
			{
				m_headtypecode = GenericHead.c_LatinModels[m_Randomizer.Next(0, GenericHead.c_LatinModels.Length)];
			}
			m_hairtypecode = m_Randomizer.Next(0, 125);
			m_haircolorcode = 1;
			m_sideburnscode = 0;
			m_skintonecode = m_Randomizer.Next(6, 11);
			if (m_skintonecode == 7)
			{
				m_skintonecode = 8;
			}
			m_skintypecode = m_Randomizer.Next(0, 3);
			m_eyecolorcode = m_Randomizer.Next(3, 5);
			m_facialhairtypecode = m_Randomizer.Next(0, 20);
			if (m_facialhairtypecode == 2 || m_facialhairtypecode > 15)
			{
				m_facialhairtypecode = 0;
			}
			m_facialhaircolorcode = array[m_haircolorcode];
		}

		public void RandomizeLatinAppearance()
		{
			int[] array = new int[10]
			{
				1,
				0,
				1,
				0,
				1,
				3,
				2,
				4,
				3,
				3
			};
			if (m_Randomizer.Next(1, 11) <= 7)
			{
				m_headtypecode = GenericHead.c_LatinModels[m_Randomizer.Next(0, GenericHead.c_LatinModels.Length)];
			}
			else
			{
				m_headtypecode = GenericHead.c_CaucasicModels[m_Randomizer.Next(0, GenericHead.c_CaucasicModels.Length)];
			}
			m_hairtypecode = m_Randomizer.Next(0, 125);
			m_haircolorcode = 1;
			m_sideburnscode = 0;
			m_skintonecode = m_Randomizer.Next(4, 7);
			m_skintypecode = m_Randomizer.Next(0, 3);
			m_eyecolorcode = m_Randomizer.Next(3, 11);
			m_facialhairtypecode = m_Randomizer.Next(0, 20);
			if (m_facialhairtypecode == 13 || m_facialhairtypecode > 15)
			{
				m_facialhairtypecode = 0;
			}
			m_facialhaircolorcode = array[m_haircolorcode];
		}

		public int ComputeMeanAttributes(int type)
		{
			switch (type)
			{
			case 0:
				return (m_gkpositioning + m_gkdiving + m_gkreflexes + m_gkhandling + m_gkkicking) / 5;
			case 1:
				return (m_aggression + m_standingtackle + m_slidingtackle + m_marking + m_interceptions) / 5;
			case 2:
				return (m_crossing + m_shortpassing + m_longpassing + m_ballcontrol + m_vision + m_curve) / 6;
			case 3:
				return (m_finishing + m_shotpower + m_longshots + m_dribbling + m_headingaccuracy + m_volleys) / 6;
			case 4:
				return (m_acceleration + m_sprintspeed + m_stamina + m_strength + m_agility + m_jumping + m_reactions + m_balance) / 8;
			case 5:
				return (m_potential + m_positioning) / 2;
			case 6:
				return (m_freekickaccuracy + m_penalties) / 2;
			default:
				return 0;
			}
		}

		public void IncreaseAllAttributes()
		{
			m_gkpositioning += ((m_gkpositioning < 99) ? 1 : 0);
			m_gkdiving += ((m_gkdiving < 99) ? 1 : 0);
			m_gkreflexes += ((m_gkreflexes < 99) ? 1 : 0);
			m_gkhandling += ((m_gkhandling < 99) ? 1 : 0);
			m_gkkicking += ((m_gkkicking < 99) ? 1 : 0);
			m_aggression += ((m_aggression < 99) ? 1 : 0);
			m_standingtackle += ((m_standingtackle < 99) ? 1 : 0);
			m_slidingtackle += ((m_slidingtackle < 99) ? 1 : 0);
			m_marking += ((m_marking < 99) ? 1 : 0);
			m_interceptions += ((m_interceptions < 99) ? 1 : 0);
			m_crossing += ((m_crossing < 99) ? 1 : 0);
			m_shortpassing += ((m_shortpassing < 99) ? 1 : 0);
			m_longpassing += ((m_longpassing < 99) ? 1 : 0);
			m_ballcontrol += ((m_ballcontrol < 99) ? 1 : 0);
			m_vision += ((m_vision < 99) ? 1 : 0);
			m_curve += ((m_curve < 99) ? 1 : 0);
			m_finishing += ((m_finishing < 99) ? 1 : 0);
			m_shotpower += ((m_shotpower < 99) ? 1 : 0);
			m_longshots += ((m_longshots < 99) ? 1 : 0);
			m_dribbling += ((m_dribbling < 99) ? 1 : 0);
			m_headingaccuracy += ((m_headingaccuracy < 99) ? 1 : 0);
			m_volleys += ((m_volleys < 99) ? 1 : 0);
			m_acceleration += ((m_acceleration < 99) ? 1 : 0);
			m_sprintspeed += ((m_sprintspeed < 99) ? 1 : 0);
			m_stamina += ((m_stamina < 99) ? 1 : 0);
			m_strength += ((m_strength < 99) ? 1 : 0);
			m_agility += ((m_agility < 99) ? 1 : 0);
			m_jumping += ((m_jumping < 99) ? 1 : 0);
			m_reactions += ((m_reactions < 99) ? 1 : 0);
			m_balance += ((m_balance < 99) ? 1 : 0);
			m_potential += ((m_potential < 99) ? 1 : 0);
			m_positioning += ((m_positioning < 99) ? 1 : 0);
			m_freekickaccuracy += ((m_freekickaccuracy < 99) ? 1 : 0);
			m_penalties += ((m_penalties < 99) ? 1 : 0);
		}

		public void DecreaseAllAttributes()
		{
			m_gkpositioning -= ((m_gkpositioning > 9) ? 1 : 0);
			m_gkdiving -= ((m_gkdiving > 9) ? 1 : 0);
			m_gkreflexes -= ((m_gkreflexes > 9) ? 1 : 0);
			m_gkhandling -= ((m_gkhandling > 9) ? 1 : 0);
			m_gkkicking -= ((m_gkkicking > 9) ? 1 : 0);
			m_aggression -= ((m_aggression > 9) ? 1 : 0);
			m_standingtackle -= ((m_standingtackle > 9) ? 1 : 0);
			m_slidingtackle -= ((m_slidingtackle > 9) ? 1 : 0);
			m_marking -= ((m_marking > 9) ? 1 : 0);
			m_interceptions -= ((m_interceptions > 9) ? 1 : 0);
			m_crossing -= ((m_crossing > 9) ? 1 : 0);
			m_shortpassing -= ((m_shortpassing > 9) ? 1 : 0);
			m_longpassing -= ((m_longpassing > 9) ? 1 : 0);
			m_ballcontrol -= ((m_ballcontrol > 9) ? 1 : 0);
			m_vision -= ((m_vision > 9) ? 1 : 0);
			m_curve -= ((m_curve > 9) ? 1 : 0);
			m_finishing -= ((m_finishing > 9) ? 1 : 0);
			m_shotpower -= ((m_shotpower > 9) ? 1 : 0);
			m_longshots -= ((m_longshots > 9) ? 1 : 0);
			m_dribbling -= ((m_dribbling > 9) ? 1 : 0);
			m_headingaccuracy -= ((m_headingaccuracy > 9) ? 1 : 0);
			m_volleys -= ((m_volleys > 9) ? 1 : 0);
			m_acceleration -= ((m_acceleration > 9) ? 1 : 0);
			m_sprintspeed -= ((m_sprintspeed > 9) ? 1 : 0);
			m_stamina -= ((m_stamina > 9) ? 1 : 0);
			m_strength -= ((m_strength > 9) ? 1 : 0);
			m_agility -= ((m_agility > 9) ? 1 : 0);
			m_jumping -= ((m_jumping > 9) ? 1 : 0);
			m_reactions -= ((m_reactions > 9) ? 1 : 0);
			m_balance -= ((m_balance > 9) ? 1 : 0);
			m_potential -= ((m_potential > 9) ? 1 : 0);
			m_positioning -= ((m_positioning > 9) ? 1 : 0);
			m_freekickaccuracy -= ((m_freekickaccuracy > 9) ? 1 : 0);
			m_penalties -= ((m_penalties > 9) ? 1 : 0);
		}

		public int GetAverageRoleAttribute()
		{
			int num = 0;
			switch (m_preferredposition1)
			{
			case 0:
				num = (m_gkreflexes * 21 + m_gkhandling * 21 + m_gkdiving * 21 + m_gkpositioning * 21 + m_gkkicking * 5 + m_reactions * 11 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			case 1:
				num = (m_interceptions * 20 + m_standingtackle * 14 + m_marking * 12 + m_slidingtackle * 12 + m_headingaccuracy * 8 + m_ballcontrol * 7 + m_shortpassing * 6 + m_reactions * 6 + m_vision * 6 + m_longpassing * 5 + m_aggression * 4 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			case 3:
			case 7:
				num = (m_acceleration * 5 + m_sprintspeed * 7 + m_stamina * 8 + m_reactions * 8 + m_ballcontrol * 7 + m_interceptions * 12 + m_crossing * 9 + m_headingaccuracy * 4 + m_shortpassing * 7 + m_marking * 8 + m_standingtackle * 11 + m_slidingtackle * 14 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			case 4:
			case 5:
			case 6:
				num = (m_sprintspeed * 2 + m_jumping * 3 + m_strength * 10 + m_reactions * 5 + m_aggression * 7 + m_interceptions * 13 + m_ballcontrol * 4 + m_headingaccuracy * 10 + m_shortpassing * 5 + m_marking * 14 + m_standingtackle * 17 + m_slidingtackle * 10 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			case 2:
			case 8:
				num = (m_acceleration * 4 + m_sprintspeed * 6 + m_stamina * 10 + m_reactions * 8 + m_interceptions * 12 + m_ballcontrol * 8 + m_crossing * 12 + m_dribbling * 4 + m_shortpassing * 10 + m_marking * 7 + m_standingtackle * 8 + m_slidingtackle * 11 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			case 9:
			case 10:
			case 11:
				num = (m_stamina * 6 + m_strength * 4 + m_reactions * 7 + m_aggression * 5 + m_interceptions * 14 + m_vision * 4 + m_ballcontrol * 10 + m_longpassing * 10 + m_shortpassing * 14 + m_marking * 9 + m_standingtackle * 12 + m_slidingtackle * 5 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			case 13:
			case 14:
			case 15:
				num = (m_stamina * 6 + m_reactions * 8 + m_interceptions * 5 + m_positioning * 6 + m_vision * 13 + m_ballcontrol * 14 + m_dribbling * 7 + m_finishing * 2 + m_longpassing * 13 + m_shortpassing * 17 + m_longshots * 4 + m_strength * 5 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			case 23:
			case 27:
				num = (m_acceleration * 7 + m_sprintspeed * 6 + m_agility * 3 + m_reactions * 7 + m_positioning * 9 + m_vision * 6 + m_ballcontrol * 14 + m_crossing * 9 + m_dribbling * 16 + m_finishing * 10 + m_shortpassing * 9 + m_longshots * 4 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			case 12:
			case 16:
				num = (m_acceleration * 7 + m_sprintspeed * 6 + m_stamina * 5 + m_reactions * 7 + m_positioning * 8 + m_vision * 7 + m_ballcontrol * 13 + m_crossing * 10 + m_dribbling * 15 + m_finishing * 6 + m_longpassing * 5 + m_shortpassing * 11 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			case 17:
			case 18:
			case 19:
				num = (m_acceleration * 4 + m_sprintspeed * 3 + m_agility * 3 + m_reactions * 7 + m_positioning * 9 + m_vision * 14 + m_ballcontrol * 15 + m_dribbling * 13 + m_finishing * 7 + m_longpassing * 4 + m_shortpassing * 16 + m_longshots * 5 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			case 20:
			case 21:
			case 22:
				num = (m_acceleration * 5 + m_sprintspeed * 5 + m_reactions * 9 + m_positioning * 13 + m_vision * 8 + m_ballcontrol * 15 + m_dribbling * 14 + m_finishing * 11 + m_headingaccuracy * 2 + m_shortpassing * 9 + m_shotpower * 5 + m_longshots * 4 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			case 24:
			case 25:
			case 26:
				num = (m_acceleration * 4 + m_sprintspeed * 5 + m_strength * 5 + m_reactions * 8 + m_positioning * 13 + m_ballcontrol * 10 + m_dribbling * 7 + m_finishing * 18 + m_headingaccuracy * 10 + m_shortpassing * 5 + m_shotpower * 10 + m_longshots * 3 + m_volleys * 2 + 50) / 100;
				if (num > 99)
				{
					num = 99;
				}
				break;
			default:
				return 0;
			}
			switch (m_internationalrep)
			{
			case 2:
				if (num > 50)
				{
					num++;
				}
				break;
			case 3:
				if (num > 68)
				{
					num += 2;
				}
				else if (num > 34)
				{
					num++;
				}
				break;
			case 4:
				if (num > 77)
				{
					num += 3;
				}
				else if (num > 51)
				{
					num += 2;
				}
				else if (num > 25)
				{
					num++;
				}
				break;
			}
			return num;
		}

		public int GetRolePerformance(ERole requestedRole)
		{
			int preferredposition = m_preferredposition1;
			return GetAverageRoleAttribute() * c_RolesMap[preferredposition, (int)requestedRole] / 100;
		}

		public ERole ChooseRole(ERole[] availableRoles, int nRoles)
		{
			ERole eRole = ERole.Tribune;
			int num = -1;
			for (int i = 0; i < nRoles; i++)
			{
				int rolePerformance = GetRolePerformance(availableRoles[i]);
				if (rolePerformance > num)
				{
					eRole = availableRoles[i];
					num = rolePerformance;
				}
			}
			if ((m_preferredposition1 == 0 && eRole != 0) || (m_preferredposition1 != 0 && eRole == ERole.Goalkeeper))
			{
				return ERole.Tribune;
			}
			return eRole;
		}

		public string GetRoleAcronym()
		{
			if (FifaEnvironment.Language != null)
			{
				return FifaEnvironment.Language.GetRoleShortString(m_preferredposition1);
			}
			return string.Empty;
		}

		public string GetRoleString()
		{
			if (FifaEnvironment.Language != null)
			{
				return FifaEnvironment.Language.GetRoleLongString(m_preferredposition1);
			}
			return string.Empty;
		}

		public void BuildHairPartsTextures()
		{
			if (m_HairColorTextureBitmap != null && m_HairAlfaTextureBitmap != null && m_HairColorTextureBitmap != null && m_HairAlfaTextureBitmap != null)
			{
				GraphicUtil.GetAlfaFromChannel((Bitmap)m_HairColorTextureBitmap.Clone(), m_HairAlfaTextureBitmap, 2);
				GraphicUtil.GetAlfaFromChannel((Bitmap)m_HairColorTextureBitmap.Clone(), m_HairAlfaTextureBitmap, 1);
			}
		}

		public bool CreateHead3D(string xFileName)
		{
			BuildHairPartsTextures();
			Rx3File headModel = GetHeadModel();
			if (m_FaceTextureBitmaps == null || headModel == null)
			{
				return false;
			}
			new Model3D(headModel.Rx3IndexArrays[0], headModel.Rx3VertexArrays[0], m_FaceTextureBitmaps[0]);
			if (m_EyesTextureBitmap == null || headModel == null)
			{
				return false;
			}
			new Model3D(headModel.Rx3IndexArrays[0], headModel.Rx3VertexArrays[0], m_EyesTextureBitmap);
			headModel = GetHairModel();
			if (m_FaceTextureBitmaps == null || headModel == null)
			{
				return false;
			}
			new Model3D(headModel.Rx3IndexArrays[0], headModel.Rx3VertexArrays[0], m_HairColorTextureBitmap);
			return true;
		}

		public void ChangeSkills(int delta)
		{
			if (delta != 0)
			{
				m_acceleration = ChangeSkill(m_acceleration, delta);
				m_aggression = ChangeSkill(m_aggression, delta);
				m_agility = ChangeSkill(m_agility, delta);
				m_balance = ChangeSkill(m_balance, delta);
				m_ballcontrol = ChangeSkill(m_ballcontrol, delta);
				m_crossing = ChangeSkill(m_crossing, delta);
				m_curve = ChangeSkill(m_curve, delta);
				m_dribbling = ChangeSkill(m_dribbling, delta);
				m_finishing = ChangeSkill(m_finishing, delta);
				m_freekickaccuracy = ChangeSkill(m_freekickaccuracy, delta);
				m_gkdiving = ChangeSkill(m_gkdiving, delta);
				m_gkhandling = ChangeSkill(m_gkhandling, delta);
				m_gkkicking = ChangeSkill(m_gkkicking, delta);
				m_gkpositioning = ChangeSkill(m_gkpositioning, delta);
				m_gkreflexes = ChangeSkill(m_gkreflexes, delta);
				m_interceptions = ChangeSkill(m_interceptions, delta);
				m_jumping = ChangeSkill(m_jumping, delta);
				m_longpassing = ChangeSkill(m_longpassing, delta);
				m_longshots = ChangeSkill(m_longshots, delta);
				m_marking = ChangeSkill(m_marking, delta);
				m_overallrating = ChangeSkill(m_overallrating, delta);
				m_penalties = ChangeSkill(m_penalties, delta);
				m_positioning = ChangeSkill(m_positioning, delta);
				m_potential = ChangeSkill(m_potential, delta);
				m_reactions = ChangeSkill(m_reactions, delta);
				m_shortpassing = ChangeSkill(m_shortpassing, delta);
				m_shotpower = ChangeSkill(m_shotpower, delta);
				m_sprintspeed = ChangeSkill(m_sprintspeed, delta);
				m_stamina = ChangeSkill(m_stamina, delta);
				m_standingtackle = ChangeSkill(m_standingtackle, delta);
				m_strength = ChangeSkill(m_strength, delta);
				m_vision = ChangeSkill(m_vision, delta);
				m_volleys = ChangeSkill(m_volleys, delta);
			}
		}

		private int ChangeSkill(int skillValue, int delta)
		{
			if (skillValue + delta < 10)
			{
				return 10;
			}
			if (skillValue + delta > 99)
			{
				return 99;
			}
			return skillValue + delta;
		}

		public void UpdatePlayername(Table dcplayernamesTable, Table originalPlayernamesTable)
		{
			if (firstnameid >= 29000)
			{
				firstname = GetDcPlayername(dcplayernamesTable, firstnameid);
			}
			else
			{
				firstname = GetPlayerName(originalPlayernamesTable, firstnameid);
			}
			if (lastnameid >= 29000)
			{
				lastname = GetDcPlayername(dcplayernamesTable, lastnameid);
			}
			else
			{
				lastname = GetPlayerName(originalPlayernamesTable, lastnameid);
			}
			if (commonnameid >= 29000)
			{
				commonname = GetDcPlayername(dcplayernamesTable, commonnameid);
			}
			else
			{
				commonname = GetPlayerName(originalPlayernamesTable, commonnameid);
			}
			if (playerjerseynameid >= 29000)
			{
				playerjerseyname = GetDcPlayername(dcplayernamesTable, playerjerseynameid);
			}
			else
			{
				playerjerseyname = GetPlayerName(originalPlayernamesTable, playerjerseynameid);
			}
		}

		private string GetDcPlayername(Table dcplayernamesTable, int nameId)
		{
			if (nameId >= 29000)
			{
				for (int i = 0; i < dcplayernamesTable.NValidRecords; i++)
				{
					Record record = dcplayernamesTable.Records[i];
					if (nameId == record.IntField[FI.dcplayernames_nameid])
					{
						return record.StringField[FI.dcplayernames_name];
					}
				}
			}
			return string.Empty;
		}

		private string GetPlayerName(Table playernamesTable, int nameId)
		{
			if (nameId < 29000)
			{
				for (int i = 0; i < playernamesTable.NValidRecords; i++)
				{
					Record record = playernamesTable.Records[i];
					if (nameId == record.IntField[FI.playernames_nameid])
					{
						return record.CompressedString[FI.playernames_name];
					}
				}
			}
			return string.Empty;
		}

		public void ConvertFaceTexturesFrom15To16()
		{
			if (m_headclasscode == 1)
			{
				return;
			}
			GetFaceTextures();
			if (m_FaceTextureBitmaps[0].Width != 1024 || m_FaceTextureBitmaps[0].Height != 1024 || m_FaceTextureBitmaps.Length != 2)
			{
				Bitmap[] array = new Bitmap[2];
				if (m_FaceTextureBitmaps[0].Width != 1024 || m_FaceTextureBitmaps[0].Height != 1024)
				{
					array[0] = GraphicUtil.ResizeBitmap(m_FaceTextureBitmaps[0], 1024, 1024, InterpolationMode.HighQualityBicubic);
				}
				else
				{
					array[0] = (Bitmap)m_FaceTextureBitmaps[0].Clone();
				}
				if (m_FaceTextureBitmaps.Length != 2)
				{
					array[1] = new Bitmap(1024, 1024, PixelFormat.Format32bppArgb);
				}
				else
				{
					array[1] = (Bitmap)m_FaceTextureBitmaps[1].Clone();
				}
				SetFaceTextures(array);
			}
		}
	}
}
