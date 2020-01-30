using System.Drawing;

namespace FifaLibrary
{
	public class Kit : IdObject
	{
		public static Bitmap s_JerseyWrinkle = null;

		public static Bitmap s_ShortsWrinkle = null;

		public static Model3D[] s_JerseyModel3D = new Model3D[22];

		public static Model3D s_ShortsModel3D = null;

		public static Model3D s_SocksModel3D = null;

		private float[] m_Positions = new float[32];

		private Bitmap[] m_KitTextures;

		private bool m_hasadvertisingkit;

		private bool m_jerseybacknameplacementcode;

		private bool m_jerseyfrontnumberplacementcode;

		private bool m_shortsnumberplacementcode;

		private int m_jerseycollargeometrytype;

		private int m_jerseybacknamefontcase;

		private int m_jerseynamefonttype;

		private int m_numberfonttype;

		private int m_numbercolor;

		private int m_shortsnumberfonttype;

		private int m_shortsnumbercolor;

		private int m_jerseynamecolorr;

		private int m_jerseynamecolorg;

		private int m_jerseynamecolorb;

		private Color m_JerseyNameColor;

		private int m_teamcolorprimr;

		private int m_teamcolorprimg;

		private int m_teamcolorprimb;

		private int m_teamcolorsecr;

		private int m_teamcolorsecg;

		private int m_teamcolorsecb;

		private int m_teamcolortertr;

		private int m_teamcolortertg;

		private int m_teamcolortertb;

		private Color m_TeamColor1;

		private Color m_TeamColor2;

		private Color m_TeamColor3;

		private int m_jerseynamelayouttype;

		private int m_isinheritbasedetailmap;

		private int m_jerseyshapestyle;

		private int m_jerseyrenderingdetailmaptype;

		private int m_renderingmaterialtype;

		private int m_shortsrenderingdetailmaptype;

		private int m_dlc;

		private bool m_jerseyfit;

		private int m_year;

		private int m_powid;

		private int m_islocked;

		private int m_teamid;

		private int m_kittype;

		private Team m_Team;

		private static Bitmap s_JerseyShadow = null;

		private static Bitmap s_ShortsShadow = null;

		public float[] Positions
		{
			get
			{
				return m_Positions;
			}
			set
			{
				m_Positions = value;
			}
		}

		public bool hasadvertisingkit
		{
			get
			{
				return m_hasadvertisingkit;
			}
			set
			{
				m_hasadvertisingkit = value;
			}
		}

		public bool jerseyBackName
		{
			get
			{
				return m_jerseybacknameplacementcode;
			}
			set
			{
				m_jerseybacknameplacementcode = value;
			}
		}

		public bool jerseyFrontNumber
		{
			get
			{
				return m_jerseyfrontnumberplacementcode;
			}
			set
			{
				m_jerseyfrontnumberplacementcode = value;
			}
		}

		public bool shortsNumber
		{
			get
			{
				return m_shortsnumberplacementcode;
			}
			set
			{
				m_shortsnumberplacementcode = value;
			}
		}

		public int jerseyCollar
		{
			get
			{
				return m_jerseycollargeometrytype;
			}
			set
			{
				m_jerseycollargeometrytype = value;
			}
		}

		public int jerseyNameFontCase
		{
			get
			{
				return m_jerseybacknamefontcase;
			}
			set
			{
				m_jerseybacknamefontcase = value;
			}
		}

		public int jerseyNameFont
		{
			get
			{
				return m_jerseynamefonttype;
			}
			set
			{
				m_jerseynamefonttype = value;
			}
		}

		public int jerseyNumberFont
		{
			get
			{
				return m_numberfonttype;
			}
			set
			{
				m_numberfonttype = value;
			}
		}

		public int jerseyNumberColor
		{
			get
			{
				return m_numbercolor;
			}
			set
			{
				m_numbercolor = value;
			}
		}

		public int shortsNumberFont
		{
			get
			{
				return m_shortsnumberfonttype;
			}
			set
			{
				m_shortsnumberfonttype = value;
			}
		}

		public int shortsNumberColor
		{
			get
			{
				return m_shortsnumbercolor;
			}
			set
			{
				m_shortsnumbercolor = value;
			}
		}

		public Color JerseyNameColor
		{
			get
			{
				return m_JerseyNameColor;
			}
			set
			{
				m_JerseyNameColor = value;
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

		public int jerseyNameLayout
		{
			get
			{
				return m_jerseynamelayouttype;
			}
			set
			{
				m_jerseynamelayouttype = value;
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

		public int year
		{
			get
			{
				return m_year;
			}
			set
			{
				m_year = value;
			}
		}

		public int teamid
		{
			get
			{
				return m_teamid;
			}
			set
			{
				m_teamid = value;
			}
		}

		public int kittype
		{
			get
			{
				return m_kittype;
			}
			set
			{
				m_kittype = value;
			}
		}

		public Team Team
		{
			get
			{
				return m_Team;
			}
			set
			{
				m_Team = value;
				if (m_Team != null)
				{
					m_teamid = Team.Id;
				}
			}
		}

		public Kit(int kitid, int teamid, int kittype)
		{
			m_teamid = teamid;
			m_kittype = kittype;
			base.Id = kitid;
			InitNewKit();
		}

		public Kit(int kitid)
		{
			m_teamid = 0;
			m_kittype = 0;
			base.Id = kitid;
			InitNewKit();
		}

		public Kit(Record r)
		{
			Load(r);
		}

		public static void Prepare3DModels()
		{
			if (s_JerseyWrinkle != null)
			{
				return;
			}
			s_JerseyWrinkle = new Bitmap(FifaEnvironment.LaunchDir + "\\Templates\\JerseyBump0.png");
			s_ShortsWrinkle = new Bitmap(FifaEnvironment.LaunchDir + "\\Templates\\ShortsBump.png");
			Rx3Vertex.FloatType = Rx3Vertex.EFloatType.Float32;
			Rx3File rx3File;
			for (int i = 0; i < s_JerseyModel3D.Length; i++)
			{
				rx3File = ((FifaEnvironment.Year != 16) ? FifaEnvironment.GetRx3FromZdata("data/sceneassets/body/jersey_1_" + i.ToString() + "_0_0_0_0.rx3") : FifaEnvironment.GetRx3FromZdata("data/sceneassets/body/jersey_1_" + i.ToString() + "_0_0_0_0_0.rx3"));
				if (rx3File != null)
				{
					s_JerseyModel3D[i] = new Model3D(rx3File.Rx3IndexArrays[0], rx3File.Rx3VertexArrays[0], null);
				}
			}
			rx3File = ((FifaEnvironment.Year != 16) ? FifaEnvironment.GetRx3FromZdata("\\Templates\\data\\sceneassets\\body\\shorts_1_0.rx3") : FifaEnvironment.GetRx3FromZdata("data/sceneassets/body/shorts_1_0_0.rx3"));
			if (rx3File != null)
			{
				s_ShortsModel3D = new Model3D(rx3File.Rx3IndexArrays[0], rx3File.Rx3VertexArrays[0], null);
			}
			rx3File = ((FifaEnvironment.Year != 16) ? FifaEnvironment.GetRx3FromZdata("data/sceneassets/body/sock_1_0.rx3") : FifaEnvironment.GetRx3FromZdata("data/sceneassets/body/sock_1_0_0.rx3"));
			if (rx3File != null)
			{
				s_SocksModel3D = new Model3D(rx3File.Rx3IndexArrays[0], rx3File.Rx3VertexArrays[0], null);
			}
		}

		public override string ToString()
		{
			string str = (m_Team == null) ? ("Kit " + m_teamid.ToString()) : m_Team.DatabaseName;
			string empty = string.Empty;
			switch (m_kittype)
			{
			case 0:
				empty = " Home";
				break;
			case 1:
				empty = " Away";
				break;
			case 2:
				empty = " GK";
				break;
			case 3:
				empty = " Third";
				break;
			case 5:
				empty = " Referee";
				break;
			case 30:
				empty = " GK Home";
				break;
			case 31:
				empty = " GK Away";
				break;
			case 32:
				empty = " GK 3rd";
				break;
			case 93:
				empty = " Training Home";
				break;
			case 94:
				empty = " Training Away";
				break;
			default:
				empty = " Type" + m_kittype.ToString();
				break;
			}
			str += empty;
			if (m_year != 0)
			{
				str = str + " " + year.ToString();
			}
			return str + " (" + base.Id.ToString() + ")";
		}

		public static int KitId(int teamid, int kittype)
		{
			return teamid * 10 + kittype;
		}

		public void InitNewKit()
		{
			m_hasadvertisingkit = true;
			m_jerseybacknameplacementcode = true;
			m_jerseyfrontnumberplacementcode = false;
			m_shortsnumberplacementcode = false;
			m_jerseycollargeometrytype = 0;
			m_jerseybacknamefontcase = 0;
			m_jerseynamefonttype = 0;
			m_numberfonttype = 0;
			m_numbercolor = 0;
			m_shortsnumberfonttype = 0;
			m_shortsnumbercolor = 0;
			m_jerseynamecolorr = 0;
			m_jerseynamecolorg = 0;
			m_jerseynamecolorb = 0;
			m_teamcolorprimr = 0;
			m_teamcolorprimg = 0;
			m_teamcolorprimb = 0;
			m_teamcolorsecr = 0;
			m_teamcolorsecg = 0;
			m_teamcolorsecb = 0;
			m_teamcolortertr = 0;
			m_teamcolortertg = 0;
			m_teamcolortertb = 0;
			m_isinheritbasedetailmap = 0;
			m_jerseyshapestyle = 0;
			m_jerseynamelayouttype = 0;
			m_jerseyrenderingdetailmaptype = 0;
			m_renderingmaterialtype = 0;
			m_shortsrenderingdetailmaptype = 0;
			m_dlc = 0;
			m_jerseyfit = false;
			m_year = 0;
			m_powid = -1;
			m_islocked = 0;
		}

		public bool CloneTextures(Kit clone)
		{
			if (clone != null)
			{
				clone.m_KitTextures = null;
				FifaEnvironment.CloneIntoZdata(KitTextureFileName(), KitTextureFileName(clone.teamid, clone.kittype, clone.year));
				FifaEnvironment.CloneIntoZdata(MiniKitDdsFileName(), MiniKitDdsFileName(clone.teamid, clone.kittype, clone.year));
				return true;
			}
			return false;
		}

		public void Load(Record r)
		{
			base.Id = r.IntField[FI.teamkits_teamkitid];
			m_teamid = r.IntField[FI.teamkits_teamtechid];
			m_kittype = r.IntField[FI.teamkits_teamkittypetechid];
			if (m_kittype > 10)
			{
				m_kittype = 10;
			}
			m_hasadvertisingkit = (r.IntField[FI.teamkits_hasadvertisingkit] != 0);
			m_jerseybacknameplacementcode = (r.IntField[FI.teamkits_jerseybacknameplacementcode] != 0);
			m_jerseyfrontnumberplacementcode = (r.IntField[FI.teamkits_jerseyfrontnumberplacementcode] != 0);
			m_shortsnumberplacementcode = (r.IntField[FI.teamkits_shortsnumberplacementcode] != 0);
			m_jerseycollargeometrytype = r.IntField[FI.teamkits_jerseycollargeometrytype];
			m_jerseybacknamefontcase = r.IntField[FI.teamkits_jerseybacknamefontcase];
			m_jerseynamefonttype = r.IntField[FI.teamkits_jerseynamefonttype];
			m_numberfonttype = r.IntField[FI.teamkits_numberfonttype];
			m_numbercolor = ((FI.teamkits_numbercolor >= 0) ? r.IntField[FI.teamkits_numbercolor] : 0);
			m_shortsnumberfonttype = r.IntField[FI.teamkits_shortsnumberfonttype];
			m_shortsnumbercolor = ((FI.teamkits_shortsnumbercolor >= 0) ? r.IntField[FI.teamkits_shortsnumbercolor] : 0);
			m_jerseynamecolorr = r.IntField[FI.teamkits_jerseynamecolorr];
			m_jerseynamecolorg = r.IntField[FI.teamkits_jerseynamecolorg];
			m_jerseynamecolorb = r.IntField[FI.teamkits_jerseynamecolorb];
			m_JerseyNameColor = Color.FromArgb(255, m_jerseynamecolorr, m_jerseynamecolorg, m_jerseynamecolorb);
			m_teamcolorprimr = r.IntField[FI.teamkits_teamcolorprimr];
			m_teamcolorprimg = r.IntField[FI.teamkits_teamcolorprimg];
			m_teamcolorprimb = r.IntField[FI.teamkits_teamcolorprimb];
			m_teamcolorsecr = r.IntField[FI.teamkits_teamcolorsecr];
			m_teamcolorsecg = r.IntField[FI.teamkits_teamcolorsecg];
			m_teamcolorsecb = r.IntField[FI.teamkits_teamcolorsecb];
			m_teamcolortertr = r.IntField[FI.teamkits_teamcolortertr];
			m_teamcolortertg = r.IntField[FI.teamkits_teamcolortertg];
			m_teamcolortertb = r.IntField[FI.teamkits_teamcolortertb];
			m_TeamColor1 = Color.FromArgb(255, m_teamcolorprimr, m_teamcolorprimg, m_teamcolorprimb);
			m_TeamColor2 = Color.FromArgb(255, m_teamcolorsecr, m_teamcolorsecg, m_teamcolorsecb);
			m_TeamColor3 = Color.FromArgb(255, m_teamcolortertr, m_teamcolortertg, m_teamcolortertb);
			m_isinheritbasedetailmap = r.IntField[FI.teamkits_isinheritbasedetailmap];
			m_jerseyshapestyle = r.IntField[FI.teamkits_jerseyshapestyle];
			m_jerseynamelayouttype = r.IntField[FI.teamkits_jerseynamelayouttype];
			m_jerseyrenderingdetailmaptype = r.IntField[FI.teamkits_jerseyrenderingdetailmaptype];
			m_renderingmaterialtype = r.IntField[FI.teamkits_renderingmaterialtype];
			m_shortsrenderingdetailmaptype = r.IntField[FI.teamkits_shortsrenderingdetailmaptype];
			m_dlc = r.IntField[FI.teamkits_dlc];
			if (FI.teamkits_jerseyfit >= 0)
			{
				m_jerseyfit = (r.IntField[FI.teamkits_jerseyfit] != 0);
			}
			m_year = r.IntField[FI.teamkits_year];
			m_powid = r.IntField[FI.teamkits_powid];
			m_islocked = r.IntField[FI.teamkits_islocked];
		}

		public void LinkTeam(TeamList kitList)
		{
			if (kitList != null)
			{
				Team team = (Team)kitList.SearchId(m_teamid);
				if (team != null)
				{
					m_Team = team;
				}
			}
		}

		public void SaveKit(Record r, int artificialKey)
		{
			r.IntField[FI.teamkits_teamkitid] = artificialKey;
			r.IntField[FI.teamkits_teamtechid] = m_teamid;
			r.IntField[FI.teamkits_teamkittypetechid] = m_kittype;
			r.IntField[FI.teamkits_hasadvertisingkit] = (m_hasadvertisingkit ? 1 : 0);
			r.IntField[FI.teamkits_jerseybacknameplacementcode] = (m_jerseybacknameplacementcode ? 1 : 0);
			r.IntField[FI.teamkits_jerseyfrontnumberplacementcode] = (m_jerseyfrontnumberplacementcode ? 1 : 0);
			r.IntField[FI.teamkits_shortsnumberplacementcode] = (m_shortsnumberplacementcode ? 1 : 0);
			r.IntField[FI.teamkits_jerseycollargeometrytype] = m_jerseycollargeometrytype;
			r.IntField[FI.teamkits_jerseybacknamefontcase] = m_jerseybacknamefontcase;
			r.IntField[FI.teamkits_jerseynamefonttype] = m_jerseynamefonttype;
			r.IntField[FI.teamkits_numberfonttype] = m_numberfonttype;
			r.IntField[FI.teamkits_numbercolor] = m_numbercolor;
			r.IntField[FI.teamkits_shortsnumberfonttype] = m_shortsnumberfonttype;
			r.IntField[FI.teamkits_shortsnumbercolor] = m_shortsnumbercolor;
			m_jerseynamecolorr = m_JerseyNameColor.R;
			m_jerseynamecolorg = m_JerseyNameColor.G;
			m_jerseynamecolorb = m_JerseyNameColor.B;
			r.IntField[FI.teamkits_jerseynamecolorr] = m_jerseynamecolorr;
			r.IntField[FI.teamkits_jerseynamecolorg] = m_jerseynamecolorg;
			r.IntField[FI.teamkits_jerseynamecolorb] = m_jerseynamecolorb;
			m_teamcolorprimr = m_TeamColor1.R;
			m_teamcolorprimg = m_TeamColor1.G;
			m_teamcolorprimb = m_TeamColor1.B;
			m_teamcolorsecr = m_TeamColor2.R;
			m_teamcolorsecg = m_TeamColor2.G;
			m_teamcolorsecb = m_TeamColor2.B;
			m_teamcolortertr = m_TeamColor3.R;
			m_teamcolortertg = m_TeamColor3.G;
			m_teamcolortertb = m_TeamColor3.B;
			r.IntField[FI.teamkits_teamcolorprimr] = m_teamcolorprimr;
			r.IntField[FI.teamkits_teamcolorprimg] = m_teamcolorprimg;
			r.IntField[FI.teamkits_teamcolorprimb] = m_teamcolorprimb;
			r.IntField[FI.teamkits_teamcolorsecr] = m_teamcolorsecr;
			r.IntField[FI.teamkits_teamcolorsecg] = m_teamcolorsecg;
			r.IntField[FI.teamkits_teamcolorsecb] = m_teamcolorsecb;
			r.IntField[FI.teamkits_teamcolortertr] = m_teamcolortertr;
			r.IntField[FI.teamkits_teamcolortertg] = m_teamcolortertg;
			r.IntField[FI.teamkits_teamcolortertb] = m_teamcolortertb;
			r.IntField[FI.teamkits_isinheritbasedetailmap] = m_isinheritbasedetailmap;
			r.IntField[FI.teamkits_jerseyshapestyle] = m_jerseyshapestyle;
			r.IntField[FI.teamkits_jerseynamelayouttype] = m_jerseynamelayouttype;
			r.IntField[FI.teamkits_jerseyrenderingdetailmaptype] = m_jerseyrenderingdetailmaptype;
			r.IntField[FI.teamkits_renderingmaterialtype] = m_renderingmaterialtype;
			r.IntField[FI.teamkits_shortsrenderingdetailmaptype] = m_shortsrenderingdetailmaptype;
			r.IntField[FI.teamkits_dlc] = m_dlc;
			if (FI.teamkits_jerseyfit >= 0)
			{
				r.IntField[FI.teamkits_jerseyfit] = (m_jerseyfit ? 1 : 0);
			}
			r.IntField[FI.teamkits_year] = m_year;
			r.IntField[FI.teamkits_islocked] = 0;
			r.IntField[FI.teamkits_powid] = -1;
		}

		public string MiniKitDdsFileName()
		{
			return MiniKitDdsFileName(m_teamid, m_kittype, m_year);
		}

		public static string MiniKitDdsFileName(int teamid, int kittype, int year)
		{
			return "data/ui/imgassets/kits/j" + kittype.ToString() + "_" + teamid.ToString() + "_" + year.ToString() + ".dds";
		}

		public string MiniKitTemplateFileName()
		{
			return "data/ui/imgassets/kits/j#_%_0.dds";
		}

		public Bitmap GetMiniKit()
		{
			return FifaEnvironment.GetDdsArtasset(MiniKitDdsFileName());
		}

		public Bitmap GetMiniKit(int kitType)
		{
			return FifaEnvironment.GetDdsArtasset(MiniKitDdsFileName(m_teamid, kitType, 0));
		}

		public bool SetMiniKit(Bitmap bitmap)
		{
			return FifaEnvironment.SetDdsArtasset(ids: new int[2]
			{
				m_kittype,
				m_teamid
			}, format: new string[2]
			{
				string.Empty,
				string.Empty
			}, templateDdsName: MiniKitTemplateFileName(), bitmap: bitmap);
		}

		public bool DeleteMiniKit()
		{
			return FifaEnvironment.DeleteFromZdata(MiniKitDdsFileName());
		}

		public string KitTextureFileName()
		{
			return KitTextureFileName(m_teamid, m_kittype, m_year);
		}

		public static string KitTextureFileName(int teamid, int kittype, int year)
		{
			return "data/sceneassets/kit/kit_" + teamid.ToString() + "_" + kittype.ToString() + "_" + year.ToString() + ".rx3";
		}

		public string KitTextureTemplateName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data\\sceneassets\\kit\\2014_kit_#_%_@.rx3";
			}
			return "data\\sceneassets\\kit\\kit_#_%_@.rx3";
		}

		public Bitmap[] GetKitTextures()
		{
			if (m_KitTextures != null)
			{
				return m_KitTextures;
			}
			string rx3FileName = KitTextureFileName();
			m_KitTextures = FifaEnvironment.GetKitFromRx3(rx3FileName, out m_Positions);
			return m_KitTextures;
		}

		public bool SetKitTextures(Bitmap[] bitmaps)
		{
			m_KitTextures = bitmaps;
			return FifaEnvironment.ImportKitIntoZdata(ids: new int[3]
			{
				m_teamid,
				m_kittype,
				m_year
			}, templateRx3Name: KitTextureTemplateName(), bitmaps: bitmaps, kitPositions: m_Positions);
		}

		public bool SetKitTextures(string rx3FileName)
		{
			bool num = FifaEnvironment.ImportFileIntoZdataAs(rx3FileName, KitTextureFileName(), delete: false, ECompressionMode.Chunkzip);
			if (num)
			{
				m_KitTextures = null;
				GetKitTextures();
			}
			return num;
		}

		public bool DeleteKitTextures()
		{
			bool num = FifaEnvironment.DeleteFromZdata(KitTextureFileName());
			if (num)
			{
				m_KitTextures = null;
			}
			return num;
		}

		public void DisposeKitTextures()
		{
			if (m_KitTextures == null)
			{
				return;
			}
			for (int i = 0; i < m_KitTextures.Length; i++)
			{
				if (m_KitTextures[i] != null)
				{
					m_KitTextures[i].Dispose();
				}
			}
			m_KitTextures = null;
		}

		public bool ImportKitTextures(string rx3FileName)
		{
			return SetKitTextures(rx3FileName);
		}

		public bool ExportKitTextures(string exportDir)
		{
			return FifaEnvironment.ExportFileFromZdata(KitTextureFileName(), exportDir);
		}

		public Bitmap[] GetJerseyFont()
		{
			return NumberFont.GetNumberFont(m_numberfonttype, m_numbercolor);
		}

		public bool SetJerseyFont(Bitmap[] bitmaps)
		{
			return NumberFont.SetNumberFont(m_numberfonttype, m_numbercolor, bitmaps);
		}

		public bool SetJerseyFont(string rx3FileName)
		{
			return NumberFont.SetNumberFont(m_numberfonttype, m_numbercolor, rx3FileName);
		}

		public bool DeleteJerseyFont()
		{
			return NumberFont.Delete(m_numberfonttype, m_numbercolor);
		}

		public static Bitmap ApplyJerseyShadowedTexture(Bitmap originalTexture)
		{
			if (s_JerseyShadow == null)
			{
				s_JerseyShadow = new Bitmap(FifaEnvironment.LaunchDir + "\\Templates\\JerseyShadow.png");
			}
			if (s_JerseyShadow == null)
			{
				return originalTexture;
			}
			return GraphicUtil.EmbossBitmap(originalTexture, s_JerseyShadow);
		}

		public static Bitmap ApplyShortsShadowedTexture(Bitmap originalTexture)
		{
			if (s_ShortsShadow == null)
			{
				s_ShortsShadow = new Bitmap(FifaEnvironment.LaunchDir + "\\Templates\\ShortsShadow.png");
			}
			if (s_ShortsShadow == null)
			{
				return originalTexture;
			}
			return GraphicUtil.EmbossBitmap(originalTexture, s_ShortsShadow);
		}
	}
}
