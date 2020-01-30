using System;
using System.IO;

namespace FifaLibrary
{
	public class CompetitionSettings
	{
		private Compobj m_OwnerCompObj;

		public CompetitionSettings m_ParentSettings;

		public string m_comp_type;

		public int m_nation_id = -1;

		public int m_asset_id = -1;

		public string m_rule_bookings;

		public string m_rule_offsides;

		public string m_rule_injuries;

		public int m_rule_numsubsbench = -1;

		public int m_rule_numsubsmatch = -1;

		public int m_rule_suspension = -1;

		public int m_rule_numyellowstored = -1;

		public int m_rule_numgamesbanredmax = -1;

		public int m_rule_numgamesbanredmin = -1;

		public int m_rule_numgamesbandoubleyellowmax = -1;

		public int m_rule_numgamesbandoubleyellowmin = -1;

		public int m_rule_numgamesbanyellowsmax = -1;

		public int m_rule_numgamesbanyellowsmin = -1;

		public int m_standings_pointswin = -1;

		public int m_standings_pointsdraw = -1;

		public int m_standings_pointsloss = -1;

		public string[] m_standings_sort = new string[6];

		public int m_N_standings_sort;

		public int m_StandingsSort = -1;

		private Stage m_StageAdvanceMaxteamsStageRef;

		private Stage m_StageAdvanceStandingsKeep;

		private Stage m_StageAdvanceStandingsRank;

		private Stage m_StageStandingsCheckRank;

		private Stage m_StageAdvancePointsKeep;

		public int m_match_matchimportance = -1;

		public string m_match_stagetype;

		public string m_match_matchsituation;

		public string m_match_endruleleague;

		public string[] m_match_endruleko1leg = new string[6];

		public int m_N_endruleko1leg;

		public int m_EndRuleKo1Leg = -1;

		public string m_match_endruleko2leg1;

		public string[] m_match_endruleko2leg2 = new string[6];

		public int m_EndRuleKo2Leg2 = -1;

		public int m_N_endruleko2leg2;

		public string m_match_endrulefriendly;

		public string m_match_canusefancards;

		public string m_match_celebrationlevel;

		public int[] m_match_stadium;

		public int m_N_match_stadium;

		public int m_info_prize_money = -1;

		public int m_info_prize_money_drop = -1;

		public int m_info_color_slot_champ = -1;

		public int[] m_info_color_slot_champ_cup = new int[4];

		public int m_N_info_color_slot_champ_cup;

		public int[] m_info_color_slot_euro_league = new int[4];

		public int m_N_info_color_slot_euro_league;

		public int[] m_info_color_slot_promo = new int[4];

		public int m_N_info_color_slot_promo;

		public int[] m_info_color_slot_promo_poss = new int[6];

		public int m_N_info_color_slot_promo_poss;

		public int[] m_info_color_slot_releg = new int[4];

		public int m_N_info_color_slot_releg;

		public int[] m_info_color_slot_releg_poss = new int[4];

		public int m_N_info_color_slot_releg_poss;

		public int[] m_info_color_slot_adv_group = new int[8];

		public int m_N_info_color_slot_adv_group;

		public int m_info_slot_champ = -1;

		public int[] m_info_slot_promo = new int[4];

		public int m_N_info_slot_promo;

		public int[] m_info_slot_promo_poss = new int[4];

		public int m_N_info_slot_promo_poss;

		public int[] m_info_slot_releg = new int[4];

		public int m_N_info_slot_releg;

		public int[] m_info_slot_releg_poss = new int[4];

		public int m_N_info_slot_releg_poss;

		public int m_info_league_promo = -1;

		public int m_info_league_releg = -1;

		public int[] m_info_special_team_id = new int[32];

		public int m_N_info_special_team_id;

		public string m_schedule_seasonstartmonth;

		public int m_schedule_year_start = -1;

		public int m_schedule_year_offset = -1;

		public int m_schedule_friendlydaysbetweenmin = -1;

		public int m_schedule_friendlydaysbefore = -1;

		public int m_schedule_checkconflict = -1;

		private int m_schedule_compdependency = -1;

		private int m_schedule_forcecomp = -1;

		public int m_schedule_use_dates_comp = -1;

		private int m_schedule_internationaldependency = -1;

		private Trophy m_TrophyCompdependency;

		private Trophy m_TrophyForcecomp;

		public int m_schedule_matchreplay = -1;

		public int m_schedule_reversed = -1;

		public int m_schedule_year_real = -1;

		public int m_num_games = -1;

		private int m_advance_pointskeep = -1;

		public int m_advance_pointskeeppercentage = -1;

		public int m_advance_matchupkeep = -1;

		public int m_advance_random_draw_event = -1;

		public int m_advance_randomdraw = -1;

		public int m_advance_calccompavgs = -1;

		public int m_advance_maxteamsassoc = -1;

		public int m_advance_maxteamsgroup = -1;

		private int m_advance_maxteamsstageref = -1;

		private int m_advance_standingskeep = -1;

		private int m_advance_standingsrank = -1;

		private int m_standings_checkrank = -1;

		public int m_advance_teamcompdependency = -1;

		private League m_LeaguePromo;

		private League m_LeagueReleg;

		public Trophy TrophyCompdependency
		{
			get
			{
				if (m_TrophyCompdependency == null && m_schedule_compdependency != -1)
				{
					m_TrophyCompdependency = (Trophy)FifaEnvironment.CompetitionObjects.SearchId(m_schedule_compdependency);
				}
				return m_TrophyCompdependency;
			}
			set
			{
				m_TrophyCompdependency = value;
				m_schedule_compdependency = ((m_TrophyCompdependency != null) ? m_TrophyCompdependency.Id : (-1));
			}
		}

		public Trophy TrophyForcecomp
		{
			get
			{
				if (m_TrophyForcecomp == null && m_schedule_forcecomp != -1)
				{
					IdObject idObject = FifaEnvironment.CompetitionObjects.SearchId(m_schedule_forcecomp);
					if (idObject != null)
					{
						m_TrophyForcecomp = (Trophy)idObject;
					}
					else
					{
						m_TrophyForcecomp = null;
					}
				}
				return m_TrophyForcecomp;
			}
			set
			{
				m_TrophyForcecomp = value;
				m_schedule_forcecomp = ((m_TrophyForcecomp != null) ? m_TrophyForcecomp.Id : (-1));
			}
		}

		public int Advance_pointskeep
		{
			get
			{
				return m_advance_pointskeep;
			}
			set
			{
				m_advance_pointskeep = value;
				UpdateStageReferenceUsingId();
			}
		}

		public int Advance_maxteamsstageref
		{
			get
			{
				return m_advance_maxteamsstageref;
			}
			set
			{
				m_advance_maxteamsstageref = value;
				UpdateStageReferenceUsingId();
			}
		}

		public int Advance_standingskeep
		{
			get
			{
				return m_advance_standingskeep;
			}
			set
			{
				m_advance_standingskeep = value;
				UpdateStageReferenceUsingId();
			}
		}

		public int Advance_standingsrank
		{
			get
			{
				return m_advance_standingsrank;
			}
			set
			{
				m_advance_standingsrank = value;
				UpdateStageReferenceUsingId();
			}
		}

		public int Standings_checkrank
		{
			get
			{
				return m_standings_checkrank;
			}
			set
			{
				m_standings_checkrank = value;
				UpdateStageReferenceUsingId();
			}
		}

		public League LeaguePromo
		{
			get
			{
				return m_LeaguePromo;
			}
			set
			{
				m_LeaguePromo = value;
				m_info_league_promo = ((m_LeaguePromo != null) ? m_LeaguePromo.Id : (-1));
			}
		}

		public League LeagueReleg
		{
			get
			{
				return m_LeagueReleg;
			}
			set
			{
				m_LeagueReleg = value;
				m_info_league_releg = ((m_LeagueReleg != null) ? m_LeagueReleg.Id : (-1));
			}
		}

		public void GetInfoColorSlotChampCup(out int min, out int max)
		{
			if (m_N_info_color_slot_champ_cup == 0)
			{
				min = -1;
				max = -1;
				return;
			}
			min = m_info_color_slot_champ_cup[0];
			max = m_info_color_slot_champ_cup[m_N_info_color_slot_champ_cup - 1];
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
		}

		public bool SetInfoColorSlotChampCup(int min, int max)
		{
			if (min == -1 || max == -1 || min > max)
			{
				m_N_info_color_slot_champ_cup = 0;
				m_info_color_slot_champ_cup[0] = -1;
				return true;
			}
			for (int i = 0; i < m_info_color_slot_champ_cup.Length; i++)
			{
				if (min + i <= max)
				{
					m_info_color_slot_champ_cup[i] = min + i;
					m_N_info_color_slot_champ_cup = i + 1;
				}
				else
				{
					m_info_color_slot_champ_cup[i] = -1;
				}
			}
			return true;
		}

		public void GetInfoColorSlotEuroLeague(out int min, out int max)
		{
			if (m_N_info_color_slot_euro_league == 0)
			{
				min = -1;
				max = -1;
				return;
			}
			min = m_info_color_slot_euro_league[0];
			max = m_info_color_slot_euro_league[m_N_info_color_slot_euro_league - 1];
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
		}

		public bool SetInfoColorSlotEuroLeague(int min, int max)
		{
			if (min == -1 || max == -1 || min > max)
			{
				m_N_info_color_slot_euro_league = 0;
				m_info_color_slot_euro_league[0] = -1;
				return true;
			}
			for (int i = 0; i < m_info_color_slot_euro_league.Length; i++)
			{
				if (min + i <= max)
				{
					m_info_color_slot_euro_league[i] = min + i;
					m_N_info_color_slot_euro_league = i + 1;
				}
				else
				{
					m_info_color_slot_euro_league[i] = -1;
				}
			}
			return true;
		}

		public void GetInfoColorSlotPromo(out int min, out int max)
		{
			if (m_N_info_color_slot_promo == 0)
			{
				min = -1;
				max = -1;
				return;
			}
			min = m_info_color_slot_promo[0];
			max = m_info_color_slot_promo[m_N_info_color_slot_promo - 1];
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
		}

		public bool SetInfoColorSlotPromo(int min, int max)
		{
			if (min == -1 || max == -1 || min > max)
			{
				m_N_info_color_slot_promo = 0;
				m_info_color_slot_promo[0] = -1;
				return true;
			}
			for (int i = 0; i < m_info_color_slot_promo.Length; i++)
			{
				if (min + i <= max)
				{
					m_info_color_slot_promo[i] = min + i;
					m_N_info_color_slot_promo = i + 1;
				}
				else
				{
					m_info_color_slot_promo[i] = -1;
				}
			}
			return true;
		}

		public void GetInfoColorSlotPromoPoss(out int min, out int max)
		{
			if (m_N_info_color_slot_promo_poss == 0)
			{
				min = -1;
				max = -1;
				return;
			}
			min = m_info_color_slot_promo_poss[0];
			max = m_info_color_slot_promo_poss[m_N_info_color_slot_promo_poss - 1];
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
		}

		public bool SetInfoColorSlotPromoPoss(int min, int max)
		{
			if (min == -1 || max == -1 || min > max)
			{
				m_N_info_color_slot_promo_poss = 0;
				m_info_color_slot_promo_poss[0] = -1;
				return true;
			}
			for (int i = 0; i < m_info_color_slot_promo_poss.Length; i++)
			{
				if (min + i <= max)
				{
					m_info_color_slot_promo_poss[i] = min + i;
					m_N_info_color_slot_promo_poss = i + 1;
				}
				else
				{
					m_info_color_slot_promo_poss[i] = -1;
				}
			}
			return true;
		}

		public void GetInfoColorSlotReleg(out int min, out int max)
		{
			if (m_N_info_color_slot_releg == 0)
			{
				min = -1;
				max = -1;
				return;
			}
			min = m_info_color_slot_releg[0];
			max = m_info_color_slot_releg[m_N_info_color_slot_releg - 1];
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
		}

		public bool SetInfoColorSlotReleg(int min, int max)
		{
			if (min == -1 || max == -1 || min > max)
			{
				m_N_info_color_slot_releg = 0;
				m_info_color_slot_releg[0] = -1;
				return true;
			}
			for (int i = 0; i < m_info_color_slot_releg.Length; i++)
			{
				if (min + i <= max)
				{
					m_info_color_slot_releg[i] = min + i;
					m_N_info_color_slot_releg = i + 1;
				}
				else
				{
					m_info_color_slot_releg[i] = -1;
				}
			}
			return true;
		}

		public void GetInfoColorSlotRelegPoss(out int min, out int max)
		{
			if (m_N_info_color_slot_releg_poss == 0)
			{
				min = -1;
				max = -1;
				return;
			}
			min = m_info_color_slot_releg_poss[0];
			max = m_info_color_slot_releg_poss[m_N_info_color_slot_releg_poss - 1];
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
		}

		public bool SetInfoColorSlotRelegPoss(int min, int max)
		{
			if (min == -1 || max == -1 || min > max)
			{
				m_N_info_color_slot_releg_poss = 0;
				m_info_color_slot_releg_poss[0] = -1;
				return true;
			}
			for (int i = 0; i < m_info_color_slot_releg_poss.Length; i++)
			{
				if (min + i <= max)
				{
					m_info_color_slot_releg_poss[i] = min + i;
					m_N_info_color_slot_releg_poss = i + 1;
				}
				else
				{
					m_info_color_slot_releg_poss[i] = -1;
				}
			}
			return true;
		}

		public void GetInfoColorSlotAdvGroup(out int min, out int max)
		{
			if (m_N_info_color_slot_adv_group == 0)
			{
				min = -1;
				max = -1;
				return;
			}
			min = m_info_color_slot_adv_group[0];
			max = m_info_color_slot_adv_group[m_N_info_color_slot_adv_group - 1];
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
		}

		public bool SetInfoColorSlotAdvGroup(int min, int max)
		{
			if (min == -1 || max == -1 || min > max)
			{
				m_N_info_color_slot_adv_group = 0;
				m_info_color_slot_adv_group[0] = -1;
				return true;
			}
			for (int i = 0; i < m_info_color_slot_adv_group.Length; i++)
			{
				if (min + i <= max)
				{
					m_info_color_slot_adv_group[i] = min + i;
					m_N_info_color_slot_adv_group = i + 1;
				}
				else
				{
					m_info_color_slot_adv_group[i] = -1;
				}
			}
			return true;
		}

		public void GetInfoSlotPromo(out int min, out int max)
		{
			if (m_N_info_slot_promo == 0)
			{
				min = -1;
				max = -1;
				return;
			}
			min = m_info_slot_promo[0];
			max = m_info_slot_promo[m_N_info_slot_promo - 1];
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
		}

		public bool SetInfoSlotPromo(int min, int max)
		{
			if (min == -1 || max == -1 || min > max)
			{
				m_N_info_slot_promo = 0;
				m_info_slot_promo[0] = -1;
				return true;
			}
			for (int i = 0; i < m_info_slot_promo.Length; i++)
			{
				if (min + i <= max)
				{
					m_info_slot_promo[i] = min + i;
					m_N_info_slot_promo = i + 1;
				}
				else
				{
					m_info_slot_promo[i] = -1;
				}
			}
			return true;
		}

		public void GetInfoSlotPromoPoss(out int min, out int max)
		{
			if (m_N_info_slot_promo_poss == 0)
			{
				min = -1;
				max = -1;
				return;
			}
			min = m_info_slot_promo_poss[0];
			max = m_info_slot_promo_poss[m_N_info_slot_promo_poss - 1];
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
		}

		public bool SetInfoSlotPromoPoss(int min, int max)
		{
			if (min == -1 || max == -1 || min > max)
			{
				m_N_info_slot_promo_poss = 0;
				m_info_slot_promo_poss[0] = -1;
				return true;
			}
			for (int i = 0; i < m_info_slot_promo_poss.Length; i++)
			{
				if (min + i <= max)
				{
					m_info_slot_promo_poss[i] = min + i;
					m_N_info_slot_promo_poss = i + 1;
				}
				else
				{
					m_info_slot_promo_poss[i] = -1;
				}
			}
			return true;
		}

		public void GetInfoSlotReleg(out int min, out int max)
		{
			if (m_N_info_slot_releg == 0)
			{
				min = -1;
				max = -1;
				return;
			}
			min = m_info_slot_releg[0];
			max = m_info_slot_releg[m_N_info_slot_releg - 1];
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
		}

		public bool SetInfoSlotReleg(int min, int max)
		{
			if (min == -1 || max == -1 || min > max)
			{
				m_N_info_slot_releg = 0;
				m_info_slot_releg[0] = -1;
				return true;
			}
			for (int i = 0; i < m_info_slot_releg.Length; i++)
			{
				if (min + i <= max)
				{
					m_info_slot_releg[i] = min + i;
					m_N_info_slot_releg = i + 1;
				}
				else
				{
					m_info_slot_releg[i] = -1;
				}
			}
			return true;
		}

		public void GetInfoSlotRelegPoss(out int min, out int max)
		{
			if (m_N_info_slot_releg_poss == 0)
			{
				min = -1;
				max = -1;
				return;
			}
			min = m_info_slot_releg_poss[0];
			max = m_info_slot_releg_poss[m_N_info_slot_releg_poss - 1];
			if (min > max)
			{
				int num = min;
				min = max;
				max = num;
			}
		}

		public bool SetInfoSlotRelegPoss(int min, int max)
		{
			if (min == -1 || max == -1 || min > max)
			{
				m_N_info_slot_releg_poss = 0;
				m_info_slot_releg_poss[0] = -1;
				return true;
			}
			for (int i = 0; i < m_info_slot_releg_poss.Length; i++)
			{
				if (min + i <= max)
				{
					m_info_slot_releg_poss[i] = min + i;
					m_N_info_slot_releg_poss = i + 1;
				}
				else
				{
					m_info_slot_releg_poss[i] = -1;
				}
			}
			return true;
		}

		public CompetitionSettings(Compobj compobj)
		{
			m_OwnerCompObj = compobj;
			if (compobj.ParentObj != null)
			{
				m_ParentSettings = compobj.ParentObj.Settings;
			}
		}

		public void SetProperty(string property, int index, string val)
		{
			if (index == 0)
			{
				LoadProperty(property, val);
			}
			else if (property == "match_endruleko1leg")
			{
				if (index < m_match_endruleko1leg.Length)
				{
					m_match_endruleko1leg[index] = val;
				}
			}
			else if (property == "match_endruleko2leg2")
			{
				if (index < m_match_endruleko2leg2.Length)
				{
					m_match_endruleko2leg2[index] = val;
				}
			}
			else if (property == "standings_sort")
			{
				if (index < m_standings_sort.Length)
				{
					m_standings_sort[index] = val;
				}
			}
			else if (property == "info_slot_promo")
			{
				if (index < m_info_slot_promo.Length)
				{
					m_info_slot_promo[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "info_slot_promo_poss")
			{
				if (index < m_info_slot_promo_poss.Length)
				{
					m_info_slot_promo_poss[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "info_slot_releg")
			{
				if (index < m_info_slot_releg.Length)
				{
					m_info_slot_releg[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "info_slot_releg_poss")
			{
				if (index < m_info_slot_releg_poss.Length)
				{
					m_info_slot_releg_poss[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "info_color_slot_champ_cup")
			{
				if (index < m_info_color_slot_champ_cup.Length)
				{
					m_info_color_slot_champ_cup[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "info_color_slot_euro_league")
			{
				if (index < m_info_color_slot_euro_league.Length)
				{
					m_info_color_slot_euro_league[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "info_color_slot_promo")
			{
				if (index < m_info_color_slot_promo.Length)
				{
					m_info_color_slot_promo[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "info_color_slot_promo_poss")
			{
				if (index < m_info_color_slot_promo_poss.Length)
				{
					m_info_color_slot_promo_poss[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "info_color_slot_releg")
			{
				if (index < m_info_color_slot_releg.Length)
				{
					m_info_color_slot_releg[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "info_color_slot_releg_poss")
			{
				if (index < m_info_color_slot_releg_poss.Length)
				{
					m_info_color_slot_releg_poss[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "match_stadium")
			{
				if (m_match_stadium == null)
				{
					m_match_stadium = new int[12];
					for (int i = 0; i < m_match_stadium.Length; i++)
					{
						m_match_stadium[i] = -1;
					}
				}
				if (index < m_match_stadium.Length)
				{
					m_match_stadium[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "info_color_slot_adv_group")
			{
				if (index < m_info_color_slot_adv_group.Length)
				{
					m_info_color_slot_adv_group[index] = Convert.ToInt32(val);
				}
			}
			else if (property == "info_special_team_id" && index < m_info_special_team_id.Length)
			{
				m_info_special_team_id[index] = Convert.ToInt32(val);
			}
		}

		public void UpdateIdUsingStageReference()
		{
			m_advance_maxteamsstageref = ((m_StageAdvanceMaxteamsStageRef != null) ? m_StageAdvanceMaxteamsStageRef.Id : (-1));
			m_advance_standingskeep = ((m_StageAdvanceStandingsKeep != null) ? m_StageAdvanceStandingsKeep.Id : (-1));
			m_advance_standingsrank = ((m_StageAdvanceStandingsRank != null) ? m_StageAdvanceStandingsRank.Id : (-1));
			m_standings_checkrank = ((m_StageStandingsCheckRank != null) ? m_StageStandingsCheckRank.Id : (-1));
			m_advance_pointskeep = ((m_StageAdvancePointsKeep != null) ? m_StageAdvancePointsKeep.Id : (-1));
		}

		public void UpdateStageReferenceUsingId()
		{
			if (FifaEnvironment.CompetitionObjects == null)
			{
				return;
			}
			Compobj compobj = null;
			if (m_standings_checkrank == -1)
			{
				m_StageStandingsCheckRank = null;
			}
			else
			{
				compobj = (Compobj)FifaEnvironment.CompetitionObjects.SearchId(m_standings_checkrank);
				if (compobj != null && compobj.IsStage())
				{
					m_StageStandingsCheckRank = (Stage)compobj;
				}
				else
				{
					m_StageStandingsCheckRank = null;
				}
			}
			if (m_advance_standingsrank == -1)
			{
				m_StageAdvanceStandingsRank = null;
			}
			else
			{
				compobj = (Compobj)FifaEnvironment.CompetitionObjects.SearchId(m_advance_standingsrank);
				if (compobj != null && compobj.IsStage())
				{
					m_StageAdvanceStandingsRank = (Stage)compobj;
				}
				else
				{
					m_StageAdvanceStandingsRank = null;
				}
			}
			if (m_advance_standingskeep == -1)
			{
				m_StageAdvanceStandingsKeep = null;
			}
			else
			{
				compobj = (Compobj)FifaEnvironment.CompetitionObjects.SearchId(m_advance_standingskeep);
				if (compobj != null && compobj.IsStage())
				{
					m_StageAdvanceStandingsKeep = (Stage)compobj;
				}
				else
				{
					m_StageAdvanceStandingsKeep = null;
				}
			}
			if (m_advance_pointskeep == -1)
			{
				m_StageAdvancePointsKeep = null;
			}
			else
			{
				compobj = (Compobj)FifaEnvironment.CompetitionObjects.SearchId(m_advance_pointskeep);
				if (compobj != null && compobj.IsStage())
				{
					m_StageAdvancePointsKeep = (Stage)compobj;
				}
				else
				{
					m_StageAdvancePointsKeep = null;
				}
			}
			if (m_advance_maxteamsstageref == -1)
			{
				m_StageAdvanceMaxteamsStageRef = null;
				return;
			}
			compobj = (Compobj)FifaEnvironment.CompetitionObjects.SearchId(m_advance_maxteamsstageref);
			if (compobj != null && compobj.IsStage())
			{
				m_StageAdvanceMaxteamsStageRef = (Stage)compobj;
			}
			else
			{
				m_StageAdvanceMaxteamsStageRef = null;
			}
		}

		public bool IsStringProperty(string property)
		{
			if (property == "comp_type")
			{
				return true;
			}
			if (property == "rule_bookings")
			{
				return true;
			}
			if (property == "rule_offsides")
			{
				return true;
			}
			if (property == "rule_injuries")
			{
				return true;
			}
			if (property == "match_stagetype")
			{
				return true;
			}
			if (property == "match_matchsituation")
			{
				return true;
			}
			if (property == "match_endruleleague")
			{
				return true;
			}
			if (property == "match_endruleko1leg")
			{
				return true;
			}
			if (property == "match_endruleko2leg1")
			{
				return true;
			}
			if (property == "match_endruleko2leg2")
			{
				return true;
			}
			if (property == "match_endrulefriendly")
			{
				return true;
			}
			if (property == "match_canusefancards")
			{
				return true;
			}
			if (property == "standings_sort")
			{
				return true;
			}
			if (property == "schedule_seasonstartmonth")
			{
				return true;
			}
			return false;
		}

		public int IsMultipleProperty(string property)
		{
			if (property == "match_endruleko1leg")
			{
				return m_match_endruleko1leg.Length;
			}
			if (property == "match_endruleko2leg2")
			{
				return m_match_endruleko2leg2.Length;
			}
			if (property == "standings_sort")
			{
				return m_standings_sort.Length;
			}
			if (property == "info_slot_promo")
			{
				return m_info_slot_promo.Length;
			}
			if (property == "info_slot_promo_poss")
			{
				return m_info_slot_promo_poss.Length;
			}
			if (property == "info_slot_releg")
			{
				return m_info_slot_releg.Length;
			}
			if (property == "info_slot_releg_poss")
			{
				return m_info_slot_releg_poss.Length;
			}
			if (property == "info_color_slot_champ_cup")
			{
				return m_info_color_slot_champ_cup.Length;
			}
			if (property == "info_color_slot_euro_league")
			{
				return m_info_color_slot_euro_league.Length;
			}
			if (property == "info_color_slot_promo")
			{
				return m_info_color_slot_promo.Length;
			}
			if (property == "info_color_slot_promo_poss")
			{
				return m_info_color_slot_promo_poss.Length;
			}
			if (property == "info_color_slot_releg")
			{
				return m_info_color_slot_releg.Length;
			}
			if (property == "info_color_slot_releg_poss")
			{
				return m_info_color_slot_releg_poss.Length;
			}
			if (property == "match_stadium")
			{
				return m_match_stadium.Length;
			}
			if (property == "info_color_slot_adv_group")
			{
				return m_info_color_slot_adv_group.Length;
			}
			if (property == "info_special_team_id")
			{
				return m_info_special_team_id.Length;
			}
			return 1;
		}

		public string GetProperty(string property, int index, out bool isSpecific)
		{
			string text = GetSpecificProperty(property, index);
			isSpecific = false;
			if (text != null && text != "-1")
			{
				isSpecific = true;
				return text;
			}
			if (m_ParentSettings != null)
			{
				text = m_ParentSettings.GetProperty(property, index, out bool _);
			}
			if (text == null)
			{
				text = string.Empty;
			}
			return text;
		}

		public void LoadProperty(string property, string val)
		{
			if (property == "comp_type")
			{
				m_comp_type = val;
			}
			else if (property == "rule_bookings")
			{
				m_rule_bookings = val;
			}
			else if (property == "rule_offsides")
			{
				m_rule_offsides = val;
			}
			else if (property == "rule_injuries")
			{
				m_rule_injuries = val;
			}
			else if (property == "rule_numsubsbench")
			{
				m_rule_numsubsbench = Convert.ToInt32(val);
			}
			else if (property == "rule_numsubsmatch")
			{
				m_rule_numsubsmatch = Convert.ToInt32(val);
			}
			else if (property == "rule_suspension")
			{
				m_rule_suspension = Convert.ToInt32(val);
			}
			else if (property == "rule_numyellowstored")
			{
				m_rule_numyellowstored = Convert.ToInt32(val);
			}
			else if (property == "rule_numgamesbanredmax")
			{
				m_rule_numgamesbanredmax = Convert.ToInt32(val);
			}
			else if (property == "rule_numgamesbanredmin")
			{
				m_rule_numgamesbanredmin = Convert.ToInt32(val);
			}
			else if (property == "rule_numgamesbandoubleyellowmax")
			{
				m_rule_numgamesbandoubleyellowmax = Convert.ToInt32(val);
			}
			else if (property == "rule_numgamesbandoubleyellowmin")
			{
				m_rule_numgamesbandoubleyellowmin = Convert.ToInt32(val);
			}
			else if (property == "rule_numgamesbanyellowsmax")
			{
				m_rule_numgamesbanyellowsmax = Convert.ToInt32(val);
			}
			else if (property == "rule_numgamesbanyellowsmin")
			{
				m_rule_numgamesbanyellowsmin = Convert.ToInt32(val);
			}
			else if (property == "standings_pointswin")
			{
				m_standings_pointswin = Convert.ToInt32(val);
			}
			else if (property == "standings_pointsdraw")
			{
				m_standings_pointsdraw = Convert.ToInt32(val);
			}
			else if (property == "standings_pointsloss")
			{
				m_standings_pointsloss = Convert.ToInt32(val);
			}
			else if (property == "match_matchimportance")
			{
				m_match_matchimportance = Convert.ToInt32(val);
			}
			else if (property == "match_stagetype")
			{
				m_match_stagetype = val;
			}
			else if (property == "match_matchsituation")
			{
				m_match_matchsituation = val;
			}
			else if (property == "nation_id")
			{
				m_nation_id = Convert.ToInt32(val);
			}
			else if (property == "asset_id")
			{
				m_asset_id = Convert.ToInt32(val);
			}
			else if (property == "match_endruleleague")
			{
				m_match_endruleleague = val;
			}
			else if (property == "match_endruleko1leg")
			{
				m_match_endruleko1leg[m_N_endruleko1leg] = val;
				m_N_endruleko1leg++;
				m_EndRuleKo1Leg = GetKo1Rule(m_match_endruleko1leg);
			}
			else if (property == "match_endruleko2leg1")
			{
				m_match_endruleko2leg1 = val;
			}
			else if (property == "match_endruleko2leg2")
			{
				m_match_endruleko2leg2[m_N_endruleko2leg2] = val;
				m_N_endruleko2leg2++;
				m_EndRuleKo2Leg2 = GetKo2Rule(m_match_endruleko2leg2);
			}
			else if (property == "match_endrulefriendly")
			{
				m_match_endrulefriendly = val;
			}
			else if (property == "match_canusefancards")
			{
				m_match_canusefancards = val;
			}
			else if (property == "match_celebrationlevel")
			{
				m_match_celebrationlevel = val;
			}
			else if (property == "info_prize_money")
			{
				m_info_prize_money = Convert.ToInt32(val);
			}
			else if (property == "info_prize_money_drop")
			{
				m_info_prize_money_drop = Convert.ToInt32(val);
			}
			else if (property == "standings_sort")
			{
				m_standings_sort[m_N_standings_sort] = val;
				m_N_standings_sort++;
				m_StandingsSort = GetStandingRule(m_standings_sort);
			}
			else if (property == "schedule_seasonstartmonth")
			{
				m_schedule_seasonstartmonth = val;
			}
			else if (property == "schedule_year_start")
			{
				m_schedule_year_start = Convert.ToInt32(val);
			}
			else if (property == "schedule_year_offset")
			{
				m_schedule_year_offset = Convert.ToInt32(val);
			}
			else if (property == "schedule_friendlydaysbetweenmin")
			{
				m_schedule_friendlydaysbetweenmin = Convert.ToInt32(val);
			}
			else if (property == "schedule_friendlydaysbefore")
			{
				m_schedule_friendlydaysbefore = Convert.ToInt32(val);
			}
			else if (property == "schedule_use_dates_comp")
			{
				m_schedule_use_dates_comp = Convert.ToInt32(val);
			}
			else if (property == "info_slot_champ")
			{
				m_info_slot_champ = Convert.ToInt32(val);
			}
			else if (property == "info_color_slot_champ")
			{
				m_info_color_slot_champ = Convert.ToInt32(val);
			}
			else if (property == "info_slot_promo")
			{
				if (m_info_slot_promo == null)
				{
					m_info_slot_promo = new int[4];
				}
				if (m_N_info_slot_promo < m_info_slot_promo.Length)
				{
					m_info_slot_promo[m_N_info_slot_promo] = Convert.ToInt32(val);
					m_N_info_slot_promo++;
				}
			}
			else if (property == "info_slot_promo_poss")
			{
				if (m_info_slot_promo_poss == null)
				{
					m_info_slot_promo_poss = new int[4];
				}
				if (m_N_info_slot_promo_poss < m_info_slot_promo_poss.Length)
				{
					m_info_slot_promo_poss[m_N_info_slot_promo_poss] = Convert.ToInt32(val);
					m_N_info_slot_promo_poss++;
				}
			}
			else if (property == "info_slot_releg")
			{
				if (m_info_slot_releg == null)
				{
					m_info_slot_releg = new int[4];
				}
				if (m_N_info_slot_releg < m_info_slot_releg.Length)
				{
					m_info_slot_releg[m_N_info_slot_releg] = Convert.ToInt32(val);
					m_N_info_slot_releg++;
				}
			}
			else if (property == "info_slot_releg_poss")
			{
				if (m_info_slot_releg_poss == null)
				{
					m_info_slot_releg_poss = new int[4];
				}
				if (m_N_info_slot_releg_poss < m_info_slot_releg_poss.Length)
				{
					m_info_slot_releg_poss[m_N_info_slot_releg_poss] = Convert.ToInt32(val);
					m_N_info_slot_releg_poss++;
				}
			}
			else if (property == "info_color_slot_champ_cup")
			{
				if (m_info_color_slot_champ_cup == null)
				{
					m_info_color_slot_champ_cup = new int[3];
				}
				if (m_N_info_color_slot_champ_cup < m_info_color_slot_champ_cup.Length)
				{
					m_info_color_slot_champ_cup[m_N_info_color_slot_champ_cup] = Convert.ToInt32(val);
					m_N_info_color_slot_champ_cup++;
				}
			}
			else if (property == "info_color_slot_euro_league")
			{
				if (m_info_color_slot_euro_league == null)
				{
					m_info_color_slot_euro_league = new int[4];
				}
				if (m_N_info_color_slot_euro_league < m_info_color_slot_euro_league.Length)
				{
					m_info_color_slot_euro_league[m_N_info_color_slot_euro_league] = Convert.ToInt32(val);
					m_N_info_color_slot_euro_league++;
				}
			}
			else if (property == "info_color_slot_promo")
			{
				if (m_info_color_slot_promo == null)
				{
					m_info_color_slot_promo = new int[4];
				}
				if (m_N_info_color_slot_promo < m_info_color_slot_promo.Length)
				{
					m_info_color_slot_promo[m_N_info_color_slot_promo] = Convert.ToInt32(val);
					m_N_info_color_slot_promo++;
				}
			}
			else if (property == "info_color_slot_promo_poss")
			{
				if (m_info_color_slot_promo_poss == null)
				{
					m_info_color_slot_promo_poss = new int[4];
				}
				if (m_N_info_color_slot_promo_poss < m_info_color_slot_promo_poss.Length)
				{
					m_info_color_slot_promo_poss[m_N_info_color_slot_promo_poss] = Convert.ToInt32(val);
					m_N_info_color_slot_promo_poss++;
				}
			}
			else if (property == "info_color_slot_releg")
			{
				if (m_info_color_slot_releg == null)
				{
					m_info_color_slot_releg = new int[4];
				}
				if (m_N_info_color_slot_releg < m_info_color_slot_releg.Length)
				{
					m_info_color_slot_releg[m_N_info_color_slot_releg] = Convert.ToInt32(val);
					m_N_info_color_slot_releg++;
				}
			}
			else if (property == "info_color_slot_releg_poss")
			{
				if (m_info_color_slot_releg_poss == null)
				{
					m_info_color_slot_releg_poss = new int[4];
				}
				if (m_N_info_color_slot_releg_poss < m_info_color_slot_releg_poss.Length)
				{
					m_info_color_slot_releg_poss[m_N_info_color_slot_releg_poss] = Convert.ToInt32(val);
					m_N_info_color_slot_releg_poss++;
				}
			}
			else if (property == "num_games")
			{
				m_num_games = Convert.ToInt32(val);
			}
			else if (property == "advance_pointskeep")
			{
				m_advance_pointskeep = Convert.ToInt32(val);
			}
			else if (property == "advance_pointskeeppercentage")
			{
				m_advance_pointskeeppercentage = Convert.ToInt32(val);
			}
			else if (property == "advance_matchupkeep")
			{
				m_advance_matchupkeep = Convert.ToInt32(val);
			}
			else if (property == "match_stadium")
			{
				if (m_match_stadium == null)
				{
					m_match_stadium = new int[12];
					for (int i = 0; i < m_match_stadium.Length; i++)
					{
						m_match_stadium[i] = -1;
					}
					m_N_match_stadium = 0;
				}
				if (m_N_match_stadium < m_match_stadium.Length)
				{
					m_match_stadium[m_N_match_stadium] = Convert.ToInt32(val);
					m_N_match_stadium++;
				}
			}
			else if (property == "info_color_slot_adv_group")
			{
				if (m_info_color_slot_adv_group == null)
				{
					m_info_color_slot_adv_group = new int[8];
				}
				if (m_N_info_color_slot_adv_group < m_info_color_slot_adv_group.Length)
				{
					m_info_color_slot_adv_group[m_N_info_color_slot_adv_group] = Convert.ToInt32(val);
					m_N_info_color_slot_adv_group++;
				}
			}
			else if (property == "advance_standingsrank")
			{
				Advance_standingsrank = Convert.ToInt32(val);
			}
			else if (property == "asset_id")
			{
				m_asset_id = Convert.ToInt32(val);
			}
			else if (property == "rule_numsubsmatch")
			{
				m_rule_numsubsmatch = Convert.ToInt32(val);
			}
			else if (property == "schedule_checkconflict")
			{
				m_schedule_checkconflict = Convert.ToInt32(val);
			}
			else if (property == "schedule_compdependency")
			{
				m_schedule_compdependency = Convert.ToInt32(val);
			}
			else if (property == "schedule_internationaldependency")
			{
				m_schedule_internationaldependency = Convert.ToInt32(val);
			}
			else if (property == "schedule_forcecomp")
			{
				m_schedule_forcecomp = Convert.ToInt32(val);
			}
			else if (property == "advance_teamcompdependency")
			{
				m_advance_teamcompdependency = Convert.ToInt32(val);
			}
			else if (property == "info_league_promo")
			{
				m_info_league_promo = Convert.ToInt32(val);
			}
			else if (property == "info_league_releg")
			{
				m_info_league_releg = Convert.ToInt32(val);
			}
			else if (property == "info_special_team_id" && m_N_info_special_team_id < m_info_special_team_id.Length)
			{
				m_info_special_team_id[m_N_info_special_team_id] = Convert.ToInt32(val);
				m_N_info_special_team_id++;
			}
			else if (property == "advance_random_draw_event")
			{
				m_advance_random_draw_event = Convert.ToInt32(val);
			}
			else if (property == "advance_randomdraw")
			{
				m_advance_randomdraw = Convert.ToInt32(val);
			}
			else if (property == "advance_calccompavgs")
			{
				m_advance_calccompavgs = Convert.ToInt32(val);
			}
			else if (property == "advance_maxteamsassoc")
			{
				m_advance_maxteamsassoc = Convert.ToInt32(val);
			}
			else if (property == "advance_maxteamsgroup")
			{
				m_advance_maxteamsgroup = Convert.ToInt32(val);
			}
			else if (property == "advance_maxteamsstageref")
			{
				Advance_maxteamsstageref = Convert.ToInt32(val);
			}
			else if (property == "advance_standingskeep")
			{
				Advance_standingskeep = Convert.ToInt32(val);
			}
			else if (property == "advance_standingsrank")
			{
				Advance_standingsrank = Convert.ToInt32(val);
			}
			else if (property == "schedule_matchreplay")
			{
				m_schedule_matchreplay = Convert.ToInt32(val);
			}
			else if (property == "schedule_reversed")
			{
				m_schedule_reversed = Convert.ToInt32(val);
			}
			else if (property == "schedule_year_real")
			{
				m_schedule_year_real = Convert.ToInt32(val);
			}
			else if (property == "standings_checkrank")
			{
				Standings_checkrank = Convert.ToInt32(val);
			}
		}

		public string GetSpecificProperty(string property, int index)
		{
			if (property == "comp_type")
			{
				return m_comp_type;
			}
			if (property == "rule_bookings")
			{
				return m_rule_bookings;
			}
			if (property == "rule_offsides")
			{
				return m_rule_offsides;
			}
			if (property == "rule_injuries")
			{
				return m_rule_injuries;
			}
			if (property == "rule_numsubsbench")
			{
				return m_rule_numsubsbench.ToString();
			}
			if (property == "rule_numsubsmatch")
			{
				return m_rule_numsubsmatch.ToString();
			}
			if (property == "rule_suspension")
			{
				return m_rule_suspension.ToString();
			}
			if (property == "rule_numyellowstored")
			{
				return m_rule_numyellowstored.ToString();
			}
			if (property == "rule_numgamesbanredmax")
			{
				return m_rule_numgamesbanredmax.ToString();
			}
			if (property == "rule_numgamesbanredmin")
			{
				return m_rule_numgamesbanredmin.ToString();
			}
			if (property == "rule_numgamesbandoubleyellowmax")
			{
				return m_rule_numgamesbandoubleyellowmax.ToString();
			}
			if (property == "rule_numgamesbandoubleyellowmin")
			{
				return m_rule_numgamesbandoubleyellowmin.ToString();
			}
			if (property == "rule_numgamesbanyellowsmax")
			{
				return m_rule_numgamesbanyellowsmax.ToString();
			}
			if (property == "rule_numgamesbanyellowsmin")
			{
				return m_rule_numgamesbanyellowsmin.ToString();
			}
			if (property == "standings_pointswin")
			{
				return m_standings_pointswin.ToString();
			}
			if (property == "standings_pointsdraw")
			{
				return m_standings_pointsdraw.ToString();
			}
			if (property == "standings_pointsloss")
			{
				return m_standings_pointsloss.ToString();
			}
			if (property == "match_matchimportance")
			{
				return m_match_matchimportance.ToString();
			}
			if (property == "match_stagetype")
			{
				return m_match_stagetype;
			}
			if (property == "match_matchsituation")
			{
				return m_match_matchsituation;
			}
			if (property == "nation_id")
			{
				return m_nation_id.ToString();
			}
			if (property == "asset_id")
			{
				return m_asset_id.ToString();
			}
			if (property == "match_endruleleague")
			{
				return m_match_endruleleague;
			}
			if (property == "match_endruleko1leg")
			{
				return m_match_endruleko1leg[m_N_endruleko1leg];
			}
			if (property == "match_endruleko2leg1")
			{
				return m_match_endruleko2leg1;
			}
			if (property == "match_endruleko2leg2")
			{
				return m_match_endruleko2leg2[index];
			}
			if (property == "match_endrulefriendly")
			{
				return m_match_endrulefriendly;
			}
			if (property == "match_canusefancards")
			{
				return m_match_canusefancards;
			}
			if (property == "match_celebrationlevel")
			{
				return m_match_celebrationlevel;
			}
			if (property == "info_prize_money")
			{
				return m_info_prize_money.ToString();
			}
			if (property == "info_prize_money_drop")
			{
				return m_info_prize_money_drop.ToString();
			}
			if (property == "standings_sort")
			{
				return m_standings_sort[index];
			}
			if (property == "schedule_seasonstartmonth")
			{
				return m_schedule_seasonstartmonth;
			}
			if (property == "schedule_year_start")
			{
				return m_schedule_year_start.ToString();
			}
			if (property == "schedule_year_offset")
			{
				return m_schedule_year_offset.ToString();
			}
			if (property == "schedule_friendlydaysbetweenmin")
			{
				return m_schedule_friendlydaysbetweenmin.ToString();
			}
			if (property == "schedule_friendlydaysbefore")
			{
				return m_schedule_friendlydaysbetweenmin.ToString();
			}
			if (property == "schedule_use_dates_comp")
			{
				return m_schedule_use_dates_comp.ToString();
			}
			if (property == "info_slot_champ")
			{
				return m_info_slot_champ.ToString();
			}
			if (property == "info_color_slot_champ")
			{
				return m_info_color_slot_champ.ToString();
			}
			if (property == "info_slot_promo")
			{
				if (m_N_info_slot_promo < m_info_slot_promo.Length)
				{
					return m_info_slot_promo[index].ToString();
				}
			}
			else if (property == "info_slot_promo_poss")
			{
				if (m_N_info_slot_promo_poss < m_info_slot_promo_poss.Length)
				{
					return m_info_slot_promo_poss[index].ToString();
				}
			}
			else if (property == "info_slot_releg")
			{
				if (m_N_info_slot_releg < m_info_slot_releg.Length)
				{
					return m_info_slot_releg[index].ToString();
				}
			}
			else if (property == "info_slot_releg_poss")
			{
				if (m_N_info_slot_releg_poss < m_info_slot_releg_poss.Length)
				{
					return m_info_slot_releg_poss[index].ToString();
				}
			}
			else if (property == "info_color_slot_champ_cup")
			{
				if (m_N_info_color_slot_champ_cup < m_info_color_slot_champ_cup.Length)
				{
					return m_info_color_slot_champ_cup[index].ToString();
				}
			}
			else if (property == "info_color_slot_euro_league")
			{
				if (m_N_info_color_slot_euro_league < m_info_color_slot_euro_league.Length)
				{
					return m_info_color_slot_euro_league[index].ToString();
				}
			}
			else if (property == "info_color_slot_promo")
			{
				if (m_N_info_color_slot_promo < m_info_color_slot_promo.Length)
				{
					return m_info_color_slot_promo[index].ToString();
				}
			}
			else if (property == "info_color_slot_promo_poss")
			{
				if (m_N_info_color_slot_promo_poss < m_info_color_slot_promo_poss.Length)
				{
					return m_info_color_slot_promo_poss[index].ToString();
				}
			}
			else if (property == "info_color_slot_releg")
			{
				if (m_N_info_color_slot_releg < m_info_color_slot_releg.Length)
				{
					return m_info_color_slot_releg[index].ToString();
				}
			}
			else if (property == "info_color_slot_releg_poss")
			{
				if (m_N_info_color_slot_releg_poss < m_info_color_slot_releg_poss.Length)
				{
					return m_info_color_slot_releg_poss[index].ToString();
				}
			}
			else
			{
				if (property == "num_games")
				{
					return m_num_games.ToString();
				}
				if (property == "advance_pointskeep")
				{
					return m_advance_pointskeep.ToString();
				}
				if (property == "advance_pointskeeppercentage")
				{
					return m_advance_pointskeeppercentage.ToString();
				}
				if (property == "advance_matchupkeep")
				{
					return m_advance_matchupkeep.ToString();
				}
				if (property == "match_stadium")
				{
					if (index < m_match_stadium.Length)
					{
						return m_match_stadium[index].ToString();
					}
				}
				else if (property == "info_color_slot_adv_group")
				{
					if (m_N_info_color_slot_adv_group < m_info_color_slot_adv_group.Length)
					{
						return m_info_color_slot_adv_group[index].ToString();
					}
				}
				else if (property == "advance_standingsrank")
				{
					return m_advance_standingsrank.ToString();
				}
			}
			if (property == "asset_id")
			{
				return m_asset_id.ToString();
			}
			if (property == "rule_numsubsmatch")
			{
				return m_rule_numsubsmatch.ToString();
			}
			if (property == "schedule_checkconflict")
			{
				return m_schedule_checkconflict.ToString();
			}
			if (property == "schedule_compdependency")
			{
				return m_schedule_compdependency.ToString();
			}
			if (property == "schedule_internationaldependency")
			{
				return m_schedule_internationaldependency.ToString();
			}
			if (property == "schedule_forcecomp")
			{
				return m_schedule_forcecomp.ToString();
			}
			if (property == "advance_teamcompdependency")
			{
				return m_advance_teamcompdependency.ToString();
			}
			if (property == "info_league_promo")
			{
				return m_info_league_promo.ToString();
			}
			if (property == "info_league_releg")
			{
				return m_info_league_releg.ToString();
			}
			if (property == "info_special_team_id")
			{
				return m_info_special_team_id[index].ToString();
			}
			if (property == "advance_random_draw_rvrny")
			{
				return m_advance_random_draw_event.ToString();
			}
			if (property == "advance_randomdraw")
			{
				return m_advance_randomdraw.ToString();
			}
			if (property == "advance_calccompavgs")
			{
				return m_advance_calccompavgs.ToString();
			}
			if (property == "advance_maxteamsassoc")
			{
				return m_advance_maxteamsassoc.ToString();
			}
			if (property == "advance_maxteamsgroup")
			{
				return m_advance_maxteamsgroup.ToString();
			}
			if (property == "advance_maxteamsstageref")
			{
				return m_advance_maxteamsstageref.ToString();
			}
			if (property == "advance_standingskeep")
			{
				return m_advance_standingskeep.ToString();
			}
			if (property == "advance_standingsrank")
			{
				return m_advance_standingsrank.ToString();
			}
			if (property == "schedule_matchreplay")
			{
				return m_schedule_matchreplay.ToString();
			}
			if (property == "schedule_reversed")
			{
				return m_schedule_reversed.ToString();
			}
			if (property == "schedule_year_real")
			{
				return m_schedule_year_real.ToString();
			}
			if (property == "standings_checkrank")
			{
				return m_standings_checkrank.ToString();
			}
			return null;
		}

		public bool SaveToSettings(int id, StreamWriter w)
		{
			if (w == null)
			{
				return false;
			}
			if (m_asset_id != -1)
			{
				string value = id + ",asset_id," + m_asset_id;
				w.WriteLine(value);
			}
			if (m_comp_type != null)
			{
				string value = id + ",comp_type," + m_comp_type;
				w.WriteLine(value);
			}
			if (m_nation_id != -1)
			{
				string value = id + ",nation_id," + m_nation_id;
				w.WriteLine(value);
			}
			if (m_rule_bookings != null)
			{
				string value = id + ",rule_bookings," + m_rule_bookings;
				w.WriteLine(value);
			}
			if (m_rule_offsides != null)
			{
				string value = id + ",rule_offsides," + m_rule_offsides;
				w.WriteLine(value);
			}
			if (m_rule_injuries != null)
			{
				string value = id + ",rule_injuries," + m_rule_injuries;
				w.WriteLine(value);
			}
			if (m_rule_numsubsbench != -1)
			{
				string value = id + ",rule_numsubsbench," + m_rule_numsubsbench;
				w.WriteLine(value);
			}
			if (m_rule_numsubsmatch != -1)
			{
				string value = id + ",rule_numsubsmatch," + m_rule_numsubsmatch;
				w.WriteLine(value);
			}
			if (m_rule_suspension != -1)
			{
				string value = id + ",rule_suspension," + m_rule_suspension;
				w.WriteLine(value);
			}
			if (m_rule_numyellowstored != -1)
			{
				string value = id + ",rule_numyellowstored," + m_rule_numyellowstored;
				w.WriteLine(value);
			}
			if (m_rule_numgamesbanredmax != -1)
			{
				string value = id + ",rule_numgamesbanredmax," + m_rule_numgamesbanredmax;
				w.WriteLine(value);
			}
			if (m_rule_numgamesbanredmin != -1)
			{
				string value = id + ",rule_numgamesbanredmin," + m_rule_numgamesbanredmin;
				w.WriteLine(value);
			}
			if (m_rule_numgamesbandoubleyellowmax != -1)
			{
				string value = id + ",rule_numgamesbandoubleyellowmax," + m_rule_numgamesbandoubleyellowmax;
				w.WriteLine(value);
			}
			if (m_rule_numgamesbandoubleyellowmin != -1)
			{
				string value = id + ",rule_numgamesbandoubleyellowmin," + m_rule_numgamesbandoubleyellowmin;
				w.WriteLine(value);
			}
			if (m_rule_numgamesbanyellowsmax != -1)
			{
				string value = id + ",rule_numgamesbanyellowsmax," + m_rule_numgamesbanyellowsmax;
				w.WriteLine(value);
			}
			if (m_rule_numgamesbanyellowsmin != -1)
			{
				string value = id + ",rule_numgamesbanyellowsmin," + m_rule_numgamesbanyellowsmin;
				w.WriteLine(value);
			}
			if (m_standings_pointswin != -1)
			{
				string value = id + ",standings_pointswin," + m_standings_pointswin;
				w.WriteLine(value);
			}
			if (m_standings_pointsdraw != -1)
			{
				string value = id + ",standings_pointsdraw," + m_standings_pointsdraw;
				w.WriteLine(value);
			}
			if (m_standings_pointsloss != -1)
			{
				string value = id + ",standings_pointsloss," + m_standings_pointsloss;
				w.WriteLine(value);
			}
			if (m_standings_checkrank != -1)
			{
				string value = id + ",standings_checkrank," + m_standings_checkrank;
				w.WriteLine(value);
			}
			if (m_schedule_seasonstartmonth != null)
			{
				string value = id + ",schedule_seasonstartmonth," + m_schedule_seasonstartmonth;
				w.WriteLine(value);
			}
			if (m_schedule_year_start != -1)
			{
				string value = id + ",schedule_year_start," + m_schedule_year_start;
				w.WriteLine(value);
			}
			if (m_schedule_year_offset != -1)
			{
				string value = id + ",schedule_year_offset," + m_schedule_year_offset;
				w.WriteLine(value);
			}
			if (m_schedule_friendlydaysbetweenmin != -1)
			{
				string value = id + ",schedule_friendlydaysbetweenmin," + m_schedule_friendlydaysbetweenmin;
				w.WriteLine(value);
			}
			if (m_schedule_friendlydaysbefore != -1)
			{
				string value = id + ",schedule_friendlydaysbefore," + m_schedule_friendlydaysbefore;
				w.WriteLine(value);
			}
			if (m_schedule_internationaldependency != -1)
			{
				string value = id + ",schedule_internationaldependency," + m_schedule_internationaldependency;
				w.WriteLine(value);
			}
			if (m_schedule_use_dates_comp != -1)
			{
				int internationalFriendlyId = FifaEnvironment.CompetitionObjects.GetInternationalFriendlyId();
				if (internationalFriendlyId != -1)
				{
					string value = id + ",schedule_use_dates_comp," + internationalFriendlyId;
					w.WriteLine(value);
				}
			}
			if (m_schedule_checkconflict == 1)
			{
				string value = id + ",schedule_checkconflict," + m_schedule_checkconflict;
				w.WriteLine(value);
			}
			if (TrophyCompdependency != null)
			{
				string value = id + ",schedule_compdependency," + m_TrophyCompdependency.Id;
				w.WriteLine(value);
			}
			if (TrophyForcecomp != null)
			{
				string value = id + ",schedule_forcecomp," + m_TrophyForcecomp.Id;
				w.WriteLine(value);
			}
			if (m_schedule_matchreplay != -1)
			{
				string value = id + ",schedule_matchreplay," + m_schedule_matchreplay;
				w.WriteLine(value);
			}
			if (m_schedule_reversed != -1)
			{
				string value = id + ",schedule_reversed," + m_schedule_reversed;
				w.WriteLine(value);
			}
			if (m_schedule_year_real != -1)
			{
				string value = id + ",schedule_year_real," + m_schedule_year_real;
				w.WriteLine(value);
			}
			if (m_match_matchimportance != -1)
			{
				string value = id + ",match_matchimportance," + m_match_matchimportance;
				w.WriteLine(value);
			}
			if (m_match_stagetype != null)
			{
				string value = id + ",match_stagetype," + m_match_stagetype;
				w.WriteLine(value);
			}
			if (m_match_matchsituation != null)
			{
				string value = id + ",match_matchsituation," + m_match_matchsituation;
				w.WriteLine(value);
			}
			if (m_match_endruleleague != null)
			{
				string value = id + ",match_endruleleague," + m_match_endruleleague;
				w.WriteLine(value);
			}
			m_N_endruleko1leg = SetKo1Rule(m_EndRuleKo1Leg, ref m_match_endruleko1leg);
			for (int i = 0; i < m_N_endruleko1leg; i++)
			{
				string value = id + ",match_endruleko1leg," + m_match_endruleko1leg[i];
				w.WriteLine(value);
			}
			if (m_match_endruleko2leg1 != null)
			{
				string value = id + ",match_endruleko2leg1," + m_match_endruleko2leg1;
				w.WriteLine(value);
			}
			m_N_endruleko2leg2 = SetKo2Rule(m_EndRuleKo2Leg2, ref m_match_endruleko2leg2);
			for (int j = 0; j < m_N_endruleko2leg2; j++)
			{
				string value = id + ",match_endruleko2leg2," + m_match_endruleko2leg2[j];
				w.WriteLine(value);
			}
			if (m_match_endrulefriendly != null)
			{
				string value = id + ",match_endrulefriendly," + m_match_endrulefriendly;
				w.WriteLine(value);
			}
			if (m_match_canusefancards == "on")
			{
				string value = id + ",match_canusefancards," + m_match_canusefancards;
				w.WriteLine(value);
			}
			if (m_match_celebrationlevel == "LOW")
			{
				string value = id + ",match_celebrationlevel," + m_match_celebrationlevel;
				w.WriteLine(value);
			}
			if (m_match_stadium != null)
			{
				for (int k = 0; k < m_match_stadium.Length; k++)
				{
					if (m_match_stadium[k] >= 0)
					{
						string value = id + ",match_stadium," + m_match_stadium[k];
						w.WriteLine(value);
					}
				}
			}
			if (m_info_prize_money != -1)
			{
				string value = id + ",info_prize_money," + m_info_prize_money;
				w.WriteLine(value);
			}
			if (m_info_prize_money_drop != -1)
			{
				string value = id + ",info_prize_money_drop," + m_info_prize_money_drop;
				w.WriteLine(value);
			}
			m_N_standings_sort = SetStandingRule(m_StandingsSort, ref m_standings_sort);
			for (int l = 0; l < m_N_standings_sort; l++)
			{
				string value = id + ",standings_sort," + m_standings_sort[l];
				w.WriteLine(value);
			}
			if (m_num_games != -1)
			{
				string value = id + ",num_games," + m_num_games;
				w.WriteLine(value);
			}
			if (m_info_color_slot_champ != -1)
			{
				string value = id + ",info_color_slot_champ," + m_info_color_slot_champ;
				w.WriteLine(value);
			}
			for (int m = 0; m < m_N_info_color_slot_champ_cup; m++)
			{
				string value = id + ",info_color_slot_champ_cup," + m_info_color_slot_champ_cup[m];
				w.WriteLine(value);
			}
			for (int n = 0; n < m_N_info_color_slot_euro_league; n++)
			{
				string value = id + ",info_color_slot_euro_league," + m_info_color_slot_euro_league[n];
				w.WriteLine(value);
			}
			for (int num = 0; num < m_N_info_color_slot_promo; num++)
			{
				string value = id + ",info_color_slot_promo," + m_info_color_slot_promo[num];
				w.WriteLine(value);
			}
			for (int num2 = 0; num2 < m_N_info_color_slot_promo_poss; num2++)
			{
				string value = id + ",info_color_slot_promo_poss," + m_info_color_slot_promo_poss[num2];
				w.WriteLine(value);
			}
			for (int num3 = 0; num3 < m_N_info_color_slot_releg; num3++)
			{
				string value = id + ",info_color_slot_releg," + m_info_color_slot_releg[num3];
				w.WriteLine(value);
			}
			for (int num4 = 0; num4 < m_N_info_color_slot_releg_poss; num4++)
			{
				string value = id + ",info_color_slot_releg_poss," + m_info_color_slot_releg_poss[num4];
				w.WriteLine(value);
			}
			for (int num5 = 0; num5 < m_N_info_color_slot_adv_group; num5++)
			{
				string value = id + ",info_color_slot_adv_group," + m_info_color_slot_adv_group[num5];
				w.WriteLine(value);
			}
			if (m_info_slot_champ != -1)
			{
				string value = id + ",info_slot_champ," + m_info_slot_champ;
				w.WriteLine(value);
			}
			for (int num6 = 0; num6 < m_N_info_slot_promo; num6++)
			{
				string value = id + ",info_slot_promo," + m_info_slot_promo[num6];
				w.WriteLine(value);
			}
			for (int num7 = 0; num7 < m_N_info_slot_promo_poss; num7++)
			{
				string value = id + ",info_slot_promo_poss," + m_info_slot_promo_poss[num7];
				w.WriteLine(value);
			}
			for (int num8 = 0; num8 < m_N_info_slot_releg; num8++)
			{
				string value = id + ",info_slot_releg," + m_info_slot_releg[num8];
				w.WriteLine(value);
			}
			for (int num9 = 0; num9 < m_N_info_slot_releg_poss; num9++)
			{
				string value = id + ",info_slot_releg_poss," + m_info_slot_releg_poss[num9];
				w.WriteLine(value);
			}
			if (m_info_league_promo != -1)
			{
				string value = id + ",info_league_promo," + m_info_league_promo;
				w.WriteLine(value);
			}
			if (m_info_league_releg != -1)
			{
				string value = id + ",info_league_releg," + m_info_league_releg;
				w.WriteLine(value);
			}
			for (int num10 = 0; num10 < m_N_info_special_team_id; num10++)
			{
				if (m_info_special_team_id[num10] >= 0)
				{
					string value = id + ",info_special_team_id," + m_info_special_team_id[num10];
					w.WriteLine(value);
				}
			}
			if (m_advance_pointskeep != -1)
			{
				string value = id + ",advance_pointskeep," + m_advance_pointskeep;
				w.WriteLine(value);
			}
			if (m_advance_pointskeeppercentage != -1)
			{
				string value = id + ",advance_pointskeeppercentage," + m_advance_pointskeeppercentage;
				w.WriteLine(value);
			}
			if (m_advance_matchupkeep != -1)
			{
				string value = id + ",advance_matchupkeep," + m_advance_matchupkeep;
				w.WriteLine(value);
			}
			if (m_advance_standingsrank != -1)
			{
				string value = id + ",advance_standingsrank," + m_advance_standingsrank;
				w.WriteLine(value);
			}
			if (m_advance_random_draw_event != -1)
			{
				string value = id + ",advance_random_draw_event," + m_advance_random_draw_event;
				w.WriteLine(value);
			}
			if (m_advance_randomdraw != -1)
			{
				string value = id + ",advance_randomdraw," + m_advance_randomdraw;
				w.WriteLine(value);
			}
			if (m_advance_maxteamsassoc != -1)
			{
				string value = id + ",advance_maxteamsassoc," + m_advance_maxteamsassoc;
				w.WriteLine(value);
			}
			if (m_advance_maxteamsgroup != -1)
			{
				string value = id + ",advance_maxteamsgroup," + m_advance_maxteamsgroup;
				w.WriteLine(value);
			}
			if (m_advance_maxteamsstageref != -1)
			{
				string value = id + ",advance_maxteamsstageref," + m_advance_maxteamsstageref;
				w.WriteLine(value);
			}
			if (m_advance_calccompavgs != -1)
			{
				string value = id + ",advance_calccompavgs," + m_advance_calccompavgs;
				w.WriteLine(value);
			}
			if (m_advance_standingskeep != -1)
			{
				string value = id + ",advance_standingskeep," + m_advance_standingskeep;
				w.WriteLine(value);
			}
			if (m_advance_teamcompdependency != -1)
			{
				string value = id + ",advance_teamcompdependency," + m_advance_teamcompdependency;
				w.WriteLine(value);
			}
			return true;
		}

		public static int GetStandingRule(string[] rules)
		{
			if (rules == null)
			{
				return -1;
			}
			if (rules[0] == "POINTS")
			{
				if (rules[1] == "GOALDIFF")
				{
					if (rules[3] == "H2HPOINTS")
					{
						return 5;
					}
					return 0;
				}
				if (rules[1] == "WINS")
				{
					return 1;
				}
				if (rules[1] == "H2HPOINTS")
				{
					return 2;
				}
				return 0;
			}
			if (rules[0] == "TEAMRATING")
			{
				return 3;
			}
			if (rules[0] == "PREVRANK")
			{
				return 4;
			}
			return 0;
		}

		public static int SetStandingRule(int rulesId, ref string[] rules)
		{
			switch (rulesId)
			{
			case -1:
				return 0;
			default:
				rules[0] = "POINTS";
				rules[1] = "GOALDIFF";
				rules[2] = "GOALSFOR";
				rules[3] = "WINS";
				return 4;
			case 1:
				rules[0] = "POINTS";
				rules[1] = "WINS";
				rules[2] = "GOALDIFF";
				rules[3] = "GOALSFOR";
				return 4;
			case 2:
				rules[0] = "POINTS";
				rules[1] = "H2HPOINTS";
				rules[3] = "H2HGOALDIFF";
				rules[3] = "H2HGOALSFOR";
				rules[4] = "GOALDIFF";
				rules[5] = "GOALSFOR";
				return 6;
			case 3:
				rules[0] = "TEAMRATING";
				return 1;
			case 4:
				rules[0] = "PREVRANK";
				return 1;
			case 5:
				rules[0] = "POINTS";
				rules[1] = "GOALDIFF";
				rules[2] = "GOALSFOR";
				rules[3] = "H2HPOINTS";
				return 4;
			}
		}

		public static int GetKo1Rule(string[] rules)
		{
			if (rules == null)
			{
				return -1;
			}
			if (rules[0] == "ET")
			{
				if (rules[1] == "PENS")
				{
					return 0;
				}
			}
			else
			{
				if (rules[0] == "PENS")
				{
					return 1;
				}
				if (rules[0] == "END")
				{
					return 2;
				}
			}
			return -1;
		}

		public static int SetKo1Rule(int rulesId, ref string[] rules)
		{
			switch (rulesId)
			{
			case -1:
				return 0;
			default:
				rules[0] = "ET";
				rules[1] = "PENS";
				return 2;
			case 1:
				rules[0] = "PENS";
				return 1;
			case 2:
				rules[0] = "END";
				return 1;
			}
		}

		public static int GetKo2Rule(string[] rules)
		{
			if (rules == null)
			{
				return -1;
			}
			if (rules[0] == "AGG")
			{
				if (rules[1] == "AWAY")
				{
					return 0;
				}
				if (rules[1] == "ET")
				{
					return 1;
				}
				if (rules[1] == "PENS")
				{
					return 2;
				}
				return 3;
			}
			return -1;
		}

		public static int SetKo2Rule(int rulesId, ref string[] rules)
		{
			switch (rulesId)
			{
			case -1:
				return 0;
			default:
				rules[0] = "AGG";
				rules[1] = "AWAY";
				rules[2] = "ET";
				rules[3] = "ET_AWAY";
				rules[4] = "PENS";
				return 5;
			case 1:
				rules[0] = "AGG";
				rules[1] = "ET";
				rules[2] = "PENS";
				return 3;
			case 2:
				rules[0] = "AGG";
				rules[1] = "PENS";
				return 2;
			case 3:
				rules[0] = "AGG";
				return 1;
			}
		}

		public void UnsetProperty(string property)
		{
			int num = IsMultipleProperty(property);
			if (num <= 1)
			{
				UnsetProperty(property, 0);
				return;
			}
			for (int i = 0; i < num; i++)
			{
				UnsetProperty(property, i);
			}
		}

		private void UnsetProperty(string property, int index)
		{
			if (IsStringProperty(property))
			{
				SetProperty(property, index, null);
			}
			else
			{
				SetProperty(property, index, "-1");
			}
		}
	}
}
