using System.Collections.Generic;

namespace FifaLibrary
{
	public class Language : Dictionary<int, string>
	{
		public enum ETournamentStringType
		{
			Full,
			Abbr15
		}

		public enum ELeagueStringType
		{
			Full,
			Abbr15
		}

		public enum ECountryStringType
		{
			Full,
			Abbr3,
			Abbr15
		}

		public enum ETeamStringType
		{
			Full = 0,
			Abbr3 = 1,
			Abbr7 = 4,
			Abbr10 = 2,
			Abbr15 = 3
		}

		private Dictionary<int, string> m_Conventional;

		private Table m_LangTable;

		public Language(Table langTable)
		{
			m_LangTable = langTable;
			m_Conventional = new Dictionary<int, string>();
			Load(m_LangTable);
		}

		public void Load(Table langTable)
		{
			Clear();
			m_Conventional.Clear();
			for (int i = 0; i < langTable.NRecords; i++)
			{
				Record record = langTable.Records[i];
				int key = record.IntField[FI.language_hashid];
				if (!ContainsKey(key))
				{
					string value = record.CompressedString[FI.language_sourcetext];
					Add(key, value);
					value = record.CompressedString[FI.language_stringid];
					m_Conventional.Add(key, value);
				}
			}
		}

		public void Save(Table langTable)
		{
			langTable.ResizeRecords(base.Count);
			langTable.NValidRecords = base.Count;
			int num = 0;
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<int, string> current = enumerator.Current;
					Record record = langTable.Records[num];
					num++;
					record.IntField[FI.language_hashid] = current.Key;
					if (!m_Conventional.TryGetValue(current.Key, out string value))
					{
						value = string.Empty;
					}
					if (current.Value != null)
					{
						record.CompressedString[FI.language_sourcetext] = current.Value;
					}
					else
					{
						record.CompressedString[FI.language_sourcetext] = string.Empty;
					}
					record.CompressedString[FI.language_stringid] = value;
				}
			}
		}

		public uint GetTournamentHash(int assetId, ETournamentStringType stringType)
		{
			return FifaUtil.ComputeLanguageHash(GetTournamentConventionalString(assetId, stringType));
		}

		public string GetTournamentConventionalString(int assetId, ETournamentStringType stringType)
		{
			string str;
			switch (stringType)
			{
			case ETournamentStringType.Full:
				str = "TrophyName_";
				break;
			case ETournamentStringType.Abbr15:
				str = "TrophyName_Abbr15_";
				break;
			default:
				return null;
			}
			return str + assetId.ToString();
		}

		public string GetTournamentString(int assetId, ETournamentStringType stringType)
		{
			string tournamentConventionalString = GetTournamentConventionalString(assetId, stringType);
			if (tournamentConventionalString == null)
			{
				return string.Empty;
			}
			return GetString(tournamentConventionalString);
		}

		public void SetTournamentString(int assetId, ETournamentStringType stringType, string name)
		{
			string tournamentConventionalString = GetTournamentConventionalString(assetId, stringType);
			if (tournamentConventionalString != null)
			{
				SetString(tournamentConventionalString, name);
			}
		}

		public void RemoveTournamentString(int assetId, ETournamentStringType stringType)
		{
			string tournamentConventionalString = GetTournamentConventionalString(assetId, stringType);
			if (tournamentConventionalString != null)
			{
				RemoveString(tournamentConventionalString);
			}
		}

		public uint GetFormationtHash(int formationFullNameId)
		{
			return FifaUtil.ComputeLanguageHash(GetFormationConventionalString(formationFullNameId));
		}

		public string GetFormationConventionalString(int formationFullNameId)
		{
			return "Formation_FullName_" + formationFullNameId.ToString();
		}

		public int GetFreeFormationFullNameId()
		{
			for (int i = 0; i < 31; i++)
			{
				if (GetFormationString(i) == null)
				{
					return i;
				}
			}
			return -1;
		}

		public int SearchFormationFullName(string fullName)
		{
			for (int i = 0; i < 31; i++)
			{
				if (GetFormationString(i) == fullName)
				{
					return i;
				}
			}
			return -1;
		}

		public string GetFormationString(int formationFullNameId)
		{
			if (formationFullNameId < 0)
			{
				return null;
			}
			string formationConventionalString = GetFormationConventionalString(formationFullNameId);
			if (formationConventionalString == null)
			{
				return string.Empty;
			}
			return GetString(formationConventionalString);
		}

		public void SetFormationString(int formationFullNameId, string name)
		{
			string formationConventionalString = GetFormationConventionalString(formationFullNameId);
			if (formationConventionalString != null)
			{
				SetString(formationConventionalString, name);
			}
		}

		public void RemoveFormationString(int assetId)
		{
			string formationConventionalString = GetFormationConventionalString(assetId);
			if (formationConventionalString != null)
			{
				RemoveString(formationConventionalString);
			}
		}

		public uint GetLeagueHash(int assetId, ELeagueStringType stringType)
		{
			return FifaUtil.ComputeLanguageHash(GetLeagueConventionalString(assetId, stringType));
		}

		private string GetLeagueConventionalString(int assetId, ELeagueStringType stringType)
		{
			string str;
			switch (stringType)
			{
			case ELeagueStringType.Full:
				str = "LeagueName_";
				break;
			case ELeagueStringType.Abbr15:
				str = "LeagueName_Abbr15_";
				break;
			default:
				return null;
			}
			return str + assetId.ToString();
		}

		public string GetLeagueString(int assetId, ELeagueStringType stringType)
		{
			string leagueConventionalString = GetLeagueConventionalString(assetId, stringType);
			if (leagueConventionalString == null)
			{
				return string.Empty;
			}
			return GetString(leagueConventionalString);
		}

		public void SetLeagueString(int assetId, ELeagueStringType stringType, string name)
		{
			string leagueConventionalString = GetLeagueConventionalString(assetId, stringType);
			if (leagueConventionalString != null)
			{
				SetString(leagueConventionalString, name);
			}
		}

		public void RemoveLeagueString(int assetId, ELeagueStringType stringType)
		{
			string leagueConventionalString = GetLeagueConventionalString(assetId, stringType);
			if (leagueConventionalString != null)
			{
				RemoveString(leagueConventionalString);
			}
		}

		public uint GetStadiumHash(int id)
		{
			return FifaUtil.ComputeLanguageHash(GetStadiumConventionalString(id));
		}

		private string GetStadiumConventionalString(int stadiumId)
		{
			return "StadiumName_" + stadiumId.ToString();
		}

		public string GetStadiumName(int stadiumId)
		{
			string stadiumConventionalString = GetStadiumConventionalString(stadiumId);
			return GetString(stadiumConventionalString);
		}

		public void SetStadiumName(int stadiumId, string stadiumName)
		{
			string stadiumConventionalString = GetStadiumConventionalString(stadiumId);
			SetString(stadiumConventionalString, stadiumName);
		}

		public void RemoveStadiumName(int stadiumId)
		{
			string stadiumConventionalString = GetStadiumConventionalString(stadiumId);
			RemoveString(stadiumConventionalString);
		}

		public uint GetBallHash(int id)
		{
			return FifaUtil.ComputeLanguageHash(GetBallConventionalString(id, isGeneric: true));
		}

		private string GetBallConventionalString(int ballId, bool isGeneric)
		{
			if (isGeneric)
			{
				return "ballname_" + ballId.ToString();
			}
			return "BallName_" + ballId.ToString();
		}

		public string GetBallName(int ballId, out bool isGeneric)
		{
			string ballConventionalString = GetBallConventionalString(ballId, isGeneric: true);
			int key = (int)FifaUtil.ComputeLanguageHash(ballConventionalString);
			string @string = GetString(key);
			string conventionalString = GetConventionalString(key);
			isGeneric = (conventionalString == ballConventionalString);
			return @string;
		}

		public void SetBallName(int ballId, string ballName, bool isGeneric)
		{
			string ballConventionalString = GetBallConventionalString(ballId, isGeneric);
			SetString(ballConventionalString, ballName);
		}

		public void RemoveBallName(int ballId)
		{
			string ballConventionalString = GetBallConventionalString(ballId, isGeneric: true);
			RemoveString(ballConventionalString);
		}

		public uint GetShoesHash(int id)
		{
			return FifaUtil.ComputeLanguageHash(GetShoesConventionalString(id));
		}

		private string GetShoesConventionalString(int ShoesId)
		{
			return "CreatePlayerBoot_" + ShoesId.ToString();
		}

		public string GetShoesName(int ShoesId)
		{
			int key = (int)FifaUtil.ComputeLanguageHash(GetShoesConventionalString(ShoesId));
			return GetString(key);
		}

		public void SetShoesName(int ShoesId, string ShoesName)
		{
			string shoesConventionalString = GetShoesConventionalString(ShoesId);
			SetString(shoesConventionalString, ShoesName);
		}

		public void RemoveShoesName(int ShoesId)
		{
			string shoesConventionalString = GetShoesConventionalString(ShoesId);
			RemoveString(shoesConventionalString);
		}

		public uint GetCountryHash(int countryId, ECountryStringType stringType)
		{
			return FifaUtil.ComputeLanguageHash(GetCountryConventionalString(countryId, stringType));
		}

		public string GetCountryConventionalString(int countryId, ECountryStringType stringType)
		{
			switch (stringType)
			{
			case ECountryStringType.Full:
				return "NationName_" + countryId.ToString();
			case ECountryStringType.Abbr15:
				return "NationName_" + countryId.ToString() + "_abbr_15";
			case ECountryStringType.Abbr3:
				return "nationname_abbr3_" + countryId.ToString();
			default:
				return null;
			}
		}

		public string GetCountryString(int countryId, ECountryStringType stringType)
		{
			string countryConventionalString = GetCountryConventionalString(countryId, stringType);
			return GetString(countryConventionalString);
		}

		public void SetCountryString(int countryId, ECountryStringType stringType, string countryName)
		{
			string countryConventionalString = GetCountryConventionalString(countryId, stringType);
			SetString(countryConventionalString, countryName);
		}

		public void RemoveCountryStrings(int countryId)
		{
			string countryConventionalString = GetCountryConventionalString(countryId, ECountryStringType.Abbr15);
			RemoveString(countryConventionalString);
			countryConventionalString = GetCountryConventionalString(countryId, ECountryStringType.Abbr3);
			RemoveString(countryConventionalString);
			countryConventionalString = GetCountryConventionalString(countryId, ECountryStringType.Full);
			RemoveString(countryConventionalString);
		}

		public void RemoveCountryString(int countryId, ECountryStringType stringType)
		{
			string countryConventionalString = GetCountryConventionalString(countryId, stringType);
			RemoveString(countryConventionalString);
		}

		private string GetRoleLongConventionalString(int roleId)
		{
			return "SoccerFormationPosFull_" + roleId.ToString();
		}

		private string GetRoleShortConventionalString(int roleId)
		{
			string str = "SoccerFormationPos_Abbr4_";
			switch (roleId)
			{
			case 0:
				return str + "GK";
			case 1:
				return str + "SW";
			case 2:
				return str + "RWB";
			case 3:
				return str + "RB";
			case 4:
				return str + "RCB";
			case 5:
				return str + "CB";
			case 6:
				return str + "LCB";
			case 7:
				return str + "LB";
			case 8:
				return str + "LWB";
			case 9:
				return str + "RDM";
			case 10:
				return str + "CDM";
			case 11:
				return str + "LDM";
			case 12:
				return str + "RM";
			case 13:
				return str + "RCM";
			case 14:
				return str + "CM";
			case 15:
				return str + "LCM";
			case 16:
				return str + "LM";
			case 17:
				return str + "RAM";
			case 18:
				return str + "CAM";
			case 19:
				return str + "LAM";
			case 20:
				return str + "RF";
			case 21:
				return str + "CF";
			case 22:
				return str + "LF";
			case 23:
				return str + "RW";
			case 24:
				return str + "RS";
			case 25:
				return str + "ST";
			case 26:
				return str + "LS";
			case 27:
				return str + "LW";
			default:
				return null;
			}
		}

		public string GetRoleShortString(int roleId)
		{
			string text = null;
			switch (roleId)
			{
			case 28:
				text = "Substitute";
				break;
			case 29:
				text = "Reserve";
				break;
			default:
				text = GetRoleShortConventionalString(roleId);
				break;
			}
			string text2 = GetString(text);
			if (text2 == null || text2 == string.Empty)
			{
				text2 = text;
			}
			return text2;
		}

		public void SetRoleShortString(int roleId, string roleShortName)
		{
			string roleShortConventionalString = GetRoleShortConventionalString(roleId);
			SetString(roleShortConventionalString, roleShortName);
		}

		public string GetRoleLongString(int roleId)
		{
			string roleLongConventionalString = GetRoleLongConventionalString(roleId);
			return GetString(roleLongConventionalString);
		}

		public void SetRoleLongString(int roleId, string roleLongName)
		{
			string roleLongConventionalString = GetRoleLongConventionalString(roleId);
			SetString(roleLongConventionalString, roleLongName);
		}

		public uint GetSponsorDescriptionHash(int id)
		{
			return FifaUtil.ComputeLanguageHash(GetSponsorDescrConventionalString(id));
		}

		public uint GetSponsorNameHash(int id)
		{
			return FifaUtil.ComputeLanguageHash(GetSponsorNameConventionalString(id));
		}

		private string GetSponsorNameConventionalString(int sponsorId)
		{
			return "mm_Sponsor" + sponsorId.ToString();
		}

		private string GetSponsorDescrConventionalString(int sponsorId)
		{
			return "mm_SponsorBio" + sponsorId.ToString();
		}

		public string GetSponsorName(int sponsorId)
		{
			string sponsorNameConventionalString = GetSponsorNameConventionalString(sponsorId);
			return GetString(sponsorNameConventionalString);
		}

		public string GetSponsorDescription(int sponsorId)
		{
			string sponsorDescrConventionalString = GetSponsorDescrConventionalString(sponsorId);
			return GetString(sponsorDescrConventionalString);
		}

		public void SetSponsorName(int sponsorId, string sponsorName)
		{
			string sponsorNameConventionalString = GetSponsorNameConventionalString(sponsorId);
			SetString(sponsorNameConventionalString, sponsorName);
		}

		public void SetSponsorDescription(int sponsorId, string sponsorDesc)
		{
			string sponsorDescrConventionalString = GetSponsorDescrConventionalString(sponsorId);
			SetString(sponsorDescrConventionalString, sponsorDesc);
		}

		public void RemoveSponsorName(int sponsorId)
		{
			string sponsorNameConventionalString = GetSponsorNameConventionalString(sponsorId);
			RemoveString(sponsorNameConventionalString);
		}

		public void RemoveSponsorDescription(int sponsorId)
		{
			string sponsorDescrConventionalString = GetSponsorDescrConventionalString(sponsorId);
			RemoveString(sponsorDescrConventionalString);
		}

		public uint GetTeamHash(int teamId, ETeamStringType stringType)
		{
			return FifaUtil.ComputeLanguageHash(GetTeamConventionalString(teamId, stringType));
		}

		public string GetTeamConventionalString(int teamId, ETeamStringType stringType)
		{
			string str;
			switch (stringType)
			{
			case ETeamStringType.Full:
				str = "TeamName_";
				break;
			case ETeamStringType.Abbr10:
				str = "TeamName_Abbr10_";
				break;
			case ETeamStringType.Abbr15:
				str = "TeamName_Abbr15_";
				break;
			case ETeamStringType.Abbr3:
				str = "TeamName_Abbr3_";
				break;
			case ETeamStringType.Abbr7:
				str = "TeamName_Abbr7_";
				break;
			default:
				return null;
			}
			return str + teamId.ToString();
		}

		public string GetTeamString(int teamId, ETeamStringType stringType)
		{
			string teamConventionalString = GetTeamConventionalString(teamId, stringType);
			return GetString(teamConventionalString);
		}

		public void SetTeamString(int teamId, ETeamStringType stringType, string teamName)
		{
			string teamConventionalString = GetTeamConventionalString(teamId, stringType);
			SetString(teamConventionalString, teamName);
		}

		public void RemoveTeamStrings(int teamId)
		{
			string teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Abbr10);
			RemoveString(teamConventionalString);
			teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Abbr15);
			RemoveString(teamConventionalString);
			teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Abbr3);
			RemoveString(teamConventionalString);
			teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Abbr7);
			RemoveString(teamConventionalString);
			teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Full);
			RemoveString(teamConventionalString);
		}

		public void RemoveTeamString(int teamId, ETeamStringType stringType)
		{
			string teamConventionalString = GetTeamConventionalString(teamId, ETeamStringType.Abbr10);
			RemoveString(teamConventionalString);
		}

		public string GetString(int key)
		{
			if (TryGetValue(key, out string value))
			{
				return value;
			}
			return null;
		}

		public string GetConventionalString(int key)
		{
			if (m_Conventional.TryGetValue(key, out string value))
			{
				return value;
			}
			return null;
		}

		public string GetString(string stringConventional)
		{
			int key = (int)FifaUtil.ComputeLanguageHash(stringConventional);
			if (TryGetValue(key, out string value))
			{
				return value;
			}
			return null;
		}

		public void SetString(string stringConventional, string stringValue)
		{
			if (stringValue != null && !(stringValue == string.Empty) && stringConventional != null && !(stringConventional == string.Empty))
			{
				int key = (int)FifaUtil.ComputeLanguageHash(stringConventional);
				SetString(key, stringConventional, stringValue);
			}
		}

		public void SetString(int key, string stringConventional, string stringValue)
		{
			if (ContainsKey(key))
			{
				Remove(key);
			}
			Add(key, stringValue);
			if (m_Conventional.ContainsKey(key))
			{
				m_Conventional.Remove(key);
			}
			m_Conventional.Add(key, stringConventional);
		}

		public void RemoveString(int key)
		{
			if (ContainsKey(key))
			{
				Remove(key);
			}
			if (m_Conventional.ContainsKey(key))
			{
				m_Conventional.Remove(key);
			}
		}

		public void RemoveString(string stringConventional)
		{
			int key = (int)FifaUtil.ComputeLanguageHash(stringConventional);
			RemoveString(key);
		}
	}
}
