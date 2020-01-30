using System.Drawing;

namespace FifaLibrary
{
	public class Country : IdObject
	{
		public enum EConfederation
		{
			None = 1,
			Europe,
			Africa,
			South_America,
			Asia,
			Oceania,
			North_America
		}

		private int m_ChantRegionIndex;

		private int m_PALanguageIndex;

		private int m_CrowdBedsRegionIndex;

		private int m_WhistlesRegionIndex;

		private int m_AmbienceRegionIndex;

		private int m_HecklesRegionIndex;

		private int m_ReactionsRegionIndex;

		private int m_PlayerCallPatchBankIndex;

		private int m_TeamCanWhistleIndex;

		private string m_nationname;

		public int m_confederation;

		private int m_ContinentalCupTarget;

		private int m_WorldCupTarget;

		public int m_Level;

		public bool m_top_tier;

		private int m_nationstartingfirstletter;

		private string m_isocountrycode;

		private string m_DefaultCommLang;

		private string m_LanguageName;

		private string m_LanguageShortName;

		private string m_LanguageAbbreviation;

		private Team m_NationalTeam;

		private int m_NationalTeamId;

		public int ChantRegionIndex
		{
			get
			{
				return m_ChantRegionIndex;
			}
			set
			{
				m_ChantRegionIndex = value;
			}
		}

		public int PALanguageIndex
		{
			get
			{
				return m_PALanguageIndex;
			}
			set
			{
				m_PALanguageIndex = value;
			}
		}

		public int CrowdBedsRegionIndex
		{
			get
			{
				return m_CrowdBedsRegionIndex;
			}
			set
			{
				m_CrowdBedsRegionIndex = value;
			}
		}

		public int WhistlesRegionIndex
		{
			get
			{
				return m_WhistlesRegionIndex;
			}
			set
			{
				m_WhistlesRegionIndex = value;
			}
		}

		public int AmbienceRegionIndex
		{
			get
			{
				return m_AmbienceRegionIndex;
			}
			set
			{
				m_AmbienceRegionIndex = value;
			}
		}

		public int HecklesRegionIndex
		{
			get
			{
				return m_HecklesRegionIndex;
			}
			set
			{
				m_HecklesRegionIndex = value;
			}
		}

		public int ReactionsRegionIndex
		{
			get
			{
				return m_ReactionsRegionIndex;
			}
			set
			{
				m_ReactionsRegionIndex = value;
			}
		}

		public int PlayerCallPatchBankIndex
		{
			get
			{
				return m_PlayerCallPatchBankIndex;
			}
			set
			{
				m_PlayerCallPatchBankIndex = value;
			}
		}

		public int TeamCanWhistleIndex
		{
			get
			{
				return m_TeamCanWhistleIndex;
			}
			set
			{
				m_TeamCanWhistleIndex = value;
			}
		}

		public string DatabaseName
		{
			get
			{
				return m_nationname;
			}
			set
			{
				m_nationname = value;
			}
		}

		public int Confederation
		{
			get
			{
				return m_confederation;
			}
			set
			{
				m_confederation = value;
			}
		}

		public int ContinentalCupTarget
		{
			get
			{
				return m_ContinentalCupTarget;
			}
			set
			{
				m_ContinentalCupTarget = value;
			}
		}

		public int WorldCupTarget
		{
			get
			{
				return m_WorldCupTarget;
			}
			set
			{
				m_WorldCupTarget = value;
			}
		}

		public int Level
		{
			get
			{
				return m_Level;
			}
			set
			{
				m_Level = value;
			}
		}

		public bool Top_tier
		{
			get
			{
				return m_top_tier;
			}
			set
			{
				m_top_tier = value;
			}
		}

		public string IsoCountryCode
		{
			get
			{
				return m_isocountrycode;
			}
			set
			{
				m_isocountrycode = value;
			}
		}

		public string DefaultCommLang
		{
			get
			{
				return m_DefaultCommLang;
			}
			set
			{
				m_DefaultCommLang = value;
			}
		}

		public string LanguageName
		{
			get
			{
				return m_LanguageName;
			}
			set
			{
				m_LanguageName = value;
			}
		}

		public string LanguageShortName
		{
			get
			{
				return m_LanguageShortName;
			}
			set
			{
				m_LanguageShortName = value;
			}
		}

		public string LanguageAbbreviation
		{
			get
			{
				return m_LanguageAbbreviation;
			}
			set
			{
				m_LanguageAbbreviation = value;
			}
		}

		public Team NationalTeam
		{
			get
			{
				return m_NationalTeam;
			}
			set
			{
				m_NationalTeam = value;
				if (m_NationalTeam != null)
				{
					m_NationalTeamId = m_NationalTeam.Id;
				}
				else
				{
					m_NationalTeamId = -1;
				}
			}
		}

		public int NationalTeamId
		{
			get
			{
				return m_NationalTeamId;
			}
			set
			{
				m_NationalTeamId = value;
			}
		}

		public Country(int countryid)
			: base(countryid)
		{
			m_nationname = "Country " + countryid.ToString();
			m_LanguageName = m_nationname;
			m_LanguageShortName = m_nationname;
			m_LanguageAbbreviation = "XXX";
			m_confederation = 0;
			m_top_tier = false;
			m_nationstartingfirstletter = 1;
			m_isocountrycode = "XX";
			m_NationalTeamId = -1;
			m_NationalTeam = null;
			m_WorldCupTarget = 0;
			m_ContinentalCupTarget = 0;
			m_Level = 7;
			m_ChantRegionIndex = 1;
			m_PALanguageIndex = 0;
			m_CrowdBedsRegionIndex = 0;
			m_WhistlesRegionIndex = 0;
			m_AmbienceRegionIndex = 0;
			m_PlayerCallPatchBankIndex = 0;
			m_HecklesRegionIndex = 0;
			m_TeamCanWhistleIndex = 0;
			m_ReactionsRegionIndex = 0;
		}

		public Country(Record r)
			: base(r.IntField[FI.nations_nationid])
		{
			m_WorldCupTarget = 0;
			m_ContinentalCupTarget = 0;
			m_Level = 7;
			Load(r);
		}

		public void Load(Record r)
		{
			m_nationname = r.StringField[FI.nations_nationname];
			m_confederation = r.GetAndCheckIntField(FI.nations_confederation) - 1;
			m_top_tier = (r.GetAndCheckIntField(FI.nations_top_tier) != 0);
			m_nationstartingfirstletter = r.GetAndCheckIntField(FI.nations_nationstartingfirstletter);
			if (FI.nations_isocountrycode >= 0)
			{
				m_isocountrycode = r.StringField[FI.nations_isocountrycode];
			}
			if (FifaEnvironment.Language != null)
			{
				m_LanguageName = FifaEnvironment.Language.GetCountryString(base.Id, Language.ECountryStringType.Full);
				m_LanguageShortName = FifaEnvironment.Language.GetCountryString(base.Id, Language.ECountryStringType.Abbr15);
				m_LanguageAbbreviation = FifaEnvironment.Language.GetCountryString(base.Id, Language.ECountryStringType.Abbr3);
			}
			else
			{
				m_LanguageName = string.Empty;
				m_LanguageShortName = string.Empty;
				m_LanguageAbbreviation = string.Empty;
			}
			if (m_LanguageName == null)
			{
				m_LanguageName = m_nationname;
			}
		}

		public void FillFromAudionation(Record r)
		{
			m_ChantRegionIndex = r.GetAndCheckIntField(FI.audionation_ChantRegionIndex);
			m_PALanguageIndex = r.GetAndCheckIntField(FI.audionation_PALanguageIndex);
			m_DefaultCommLang = r.StringField[FI.audionation_DefaultCommLang];
			m_CrowdBedsRegionIndex = r.GetAndCheckIntField(FI.audionation_CrowdBedsRegionIndex);
			m_WhistlesRegionIndex = r.GetAndCheckIntField(FI.audionation_WhistlesRegionIndex);
			m_AmbienceRegionIndex = r.GetAndCheckIntField(FI.audionation_AmbienceRegionIndex);
			m_PlayerCallPatchBankIndex = r.GetAndCheckIntField(FI.audionation_PlayerCallPatchBankIndex);
			m_HecklesRegionIndex = r.GetAndCheckIntField(FI.audionation_HecklesRegionIndex);
			m_TeamCanWhistleIndex = r.GetAndCheckIntField(FI.audionation_TeamCanWhistleIndex);
			m_ReactionsRegionIndex = r.GetAndCheckIntField(FI.audionation_ReactionsRegionIndex);
		}

		public void SaveAudionation(Record r)
		{
			r.IntField[FI.audionation_nationid] = base.Id;
			r.IntField[FI.audionation_ChantRegionIndex] = m_ChantRegionIndex;
			r.StringField[FI.audionation_DefaultCommLang] = m_DefaultCommLang;
			r.IntField[FI.audionation_PALanguageIndex] = m_PALanguageIndex;
			r.IntField[FI.audionation_CrowdBedsRegionIndex] = m_CrowdBedsRegionIndex;
			r.IntField[FI.audionation_WhistlesRegionIndex] = m_WhistlesRegionIndex;
			r.IntField[FI.audionation_AmbienceRegionIndex] = m_AmbienceRegionIndex;
			r.IntField[FI.audionation_PlayerCallPatchBankIndex] = m_PlayerCallPatchBankIndex;
			r.IntField[FI.audionation_HecklesRegionIndex] = m_HecklesRegionIndex;
			r.IntField[FI.audionation_TeamCanWhistleIndex] = m_TeamCanWhistleIndex;
			r.IntField[FI.audionation_ReactionsRegionIndex] = m_ReactionsRegionIndex;
		}

		public void LinkTeam(TeamList teamList)
		{
			if (teamList != null)
			{
				m_NationalTeam = (Team)teamList.SearchId(m_NationalTeamId);
				_ = m_NationalTeam;
			}
		}

		public void SetNationalTeam(Team nationalTeam, int nationalTeamId)
		{
			if (nationalTeam != null)
			{
				nationalTeamId = nationalTeam.Id;
			}
			if (nationalTeamId <= 0)
			{
				nationalTeam = null;
			}
			Team nationalTeam2 = m_NationalTeam;
			_ = m_NationalTeamId;
			if (nationalTeam2 != null)
			{
				nationalTeam2.NationalTeam = false;
			}
			m_NationalTeam = nationalTeam;
			m_NationalTeamId = nationalTeamId;
			if (m_NationalTeam != null)
			{
				m_NationalTeam.Country = this;
				m_NationalTeam.NationalTeam = true;
			}
		}

		public void SaveCountry(Record r)
		{
			r.IntField[FI.nations_nationid] = base.Id;
			r.StringField[FI.nations_nationname] = m_nationname;
			r.IntField[FI.nations_confederation] = m_confederation + 1;
			r.IntField[FI.nations_top_tier] = (m_top_tier ? 1 : 0);
			r.IntField[FI.nations_nationstartingfirstletter] = m_nationstartingfirstletter;
			r.StringField[FI.nations_isocountrycode] = m_isocountrycode;
		}

		public void SaveLangTable()
		{
			if (FifaEnvironment.Language != null)
			{
				FifaEnvironment.Language.SetCountryString(base.Id, Language.ECountryStringType.Full, m_LanguageName);
				FifaEnvironment.Language.SetCountryString(base.Id, Language.ECountryStringType.Abbr15, m_LanguageShortName);
				FifaEnvironment.Language.SetCountryString(base.Id, Language.ECountryStringType.Abbr3, m_LanguageAbbreviation);
			}
		}

		public override string ToString()
		{
			if (m_LanguageName != null && m_LanguageName != string.Empty)
			{
				return m_LanguageName;
			}
			if (m_nationname != null)
			{
				return m_nationname;
			}
			return string.Empty;
		}

		public string DatabaseString()
		{
			return m_nationname;
		}

		public string FlagBigFileName()
		{
			return FlagBigFileName(base.Id);
		}

		public static string FlagTemplateBigFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/artassets/countryflags/2014_f_#.big";
			}
			return "data/ui/artassets/countryflags/f_#.big";
		}

		public static string FlagTemplateDdsName()
		{
			return "2";
		}

		public static string FlagBigFileName(int id)
		{
			return "data/ui/artassets/countryflags/f_" + id.ToString() + ".big";
		}

		public Bitmap GetFlag()
		{
			return FifaEnvironment.GetArtasset(FlagBigFileName(base.Id));
		}

		public bool SetFlag(Bitmap bitmap)
		{
			return FifaEnvironment.SetArtasset(FlagTemplateBigFileName(), FlagTemplateDdsName(), base.Id, bitmap);
		}

		public bool DeleteFlag()
		{
			return FifaEnvironment.DeleteFromZdata(FlagBigFileName());
		}

		public string Flag512TemplateFileName()
		{
			return "data/ui/imgassets/flags512x512/f_#.dds";
		}

		public string Flag512DdsFileName()
		{
			return "data/ui/imgassets/flags512x512/f_" + base.Id.ToString() + ".dds";
		}

		public static string Flag512DdsFileName(int id)
		{
			return "data/ui/imgassets/flags512x512/f_" + id.ToString() + ".dds";
		}

		public Bitmap GetFlag512()
		{
			return FifaEnvironment.GetDdsArtasset(Flag512DdsFileName());
		}

		public bool SetFlag512(Bitmap bitmap)
		{
			return FifaEnvironment.SetDdsArtasset(Flag512TemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteFlag512()
		{
			return FifaEnvironment.DeleteFromZdata(Flag512DdsFileName());
		}

		public string MiniFlagBigFileName()
		{
			return MiniFlagBigFileName(base.Id);
		}

		public static string MiniFlagTemplateBigFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/artassets/miniflags/2014_flag_#.big";
			}
			return "data/ui/artassets/miniflags/flag_#.big";
		}

		public static string MiniFlagTemplateDdsName()
		{
			return "208";
		}

		public static string MiniFlagBigFileName(int id)
		{
			return "data/ui/artassets/miniflags/flag_" + id.ToString() + ".big";
		}

		public Bitmap GetMiniFlag()
		{
			return FifaEnvironment.GetArtasset(MiniFlagBigFileName(base.Id));
		}

		public bool SetMiniFlag(Bitmap bitmap)
		{
			return FifaEnvironment.SetArtasset(MiniFlagTemplateBigFileName(), MiniFlagTemplateDdsName(), base.Id, bitmap);
		}

		public bool DeleteMiniFlag()
		{
			return FifaEnvironment.DeleteFromZdata(MiniFlagBigFileName());
		}

		public string CardFlagBigFileName()
		{
			return CardFlagBigFileName(base.Id);
		}

		public static string CardFlagTemplateBigFileName()
		{
			if (FifaEnvironment.Year == 14)
			{
				return "data/ui/artassets/cardflags/2014_#.big";
			}
			return "data/ui/artassets/cardflags/#.big";
		}

		public static string CardFlagTemplateDdsName()
		{
			return "2";
		}

		public static string CardFlagBigFileName(int id)
		{
			return "data/ui/artassets/cardflags/" + id.ToString() + ".big";
		}

		public Bitmap GetCardFlag()
		{
			return FifaEnvironment.GetArtasset(CardFlagBigFileName(base.Id));
		}

		public bool SetCardFlag(Bitmap bitmap)
		{
			return FifaEnvironment.SetArtasset(CardFlagTemplateBigFileName(), CardFlagTemplateDdsName(), base.Id, bitmap);
		}

		public bool DeleteCardFlag()
		{
			return FifaEnvironment.DeleteFromZdata(CardFlagBigFileName());
		}

		public bool Fit(string lowerName, int id)
		{
			if ((m_nationname != null && m_nationname.ToLower() == lowerName) || (m_LanguageName != null && m_LanguageName.ToLower() == lowerName))
			{
				return true;
			}
			return false;
		}

		public string ShapeFileName()
		{
			return ShapeFileName(base.Id);
		}

		public static string ShapeFileName(int countryid)
		{
			return "data/ui/imgassets/tiles/careerhub/countryshapes/c" + countryid.ToString() + ".dds";
		}

		public string ShapeTemplateFileName()
		{
			return "data/ui/imgassets/tiles/careerhub/countryshapes/c#.dds";
		}

		public Bitmap GetShape()
		{
			return FifaEnvironment.GetDdsArtasset(ShapeFileName());
		}

		public static Bitmap GetShape(int countryId)
		{
			return FifaEnvironment.GetDdsArtasset(ShapeFileName(countryId));
		}

		public bool SetShape(Bitmap bitmap)
		{
			return FifaEnvironment.SetDdsArtasset(ShapeTemplateFileName(), base.Id, bitmap);
		}

		public bool DeleteShape()
		{
			return FifaEnvironment.DeleteFromZdata(ShapeFileName());
		}
	}
}
