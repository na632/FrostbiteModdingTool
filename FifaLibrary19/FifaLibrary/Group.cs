using System.IO;

namespace FifaLibrary
{
	public class Group : Compobj
	{
		private GroupList m_SubGroups;

		private ScheduleArray m_Schedule;

		private int m_NSchedule;

		private RankList m_Ranks;

		private string m_ConventionalDescription;

		private string m_LanguageName;

		public Task[] m_StartTask;

		public int m_NStartTasks;

		public Task[] m_EndTask;

		public int m_NEndTasks;

		public Stage ParentStage
		{
			get
			{
				if (base.ParentObj.IsStage())
				{
					return (Stage)base.ParentObj;
				}
				if (base.ParentObj.IsGroup() && base.ParentObj.ParentObj.IsStage())
				{
					return (Stage)base.ParentObj.ParentObj;
				}
				return null;
			}
		}

		public Trophy ParentTrophy
		{
			get
			{
				if (ParentStage.ParentObj.IsTrophy())
				{
					return (Trophy)ParentStage.ParentObj;
				}
				return null;
			}
		}

		public GroupList SubGroups => m_SubGroups;

		public ScheduleArray Schedules
		{
			get
			{
				return m_Schedule;
			}
			set
			{
				m_Schedule = value;
			}
		}

		public int NSchedule => m_Schedule.Count;

		public RankList Ranks => m_Ranks;

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

		public override string ToString()
		{
			if (m_LanguageName != null && m_LanguageName != string.Empty)
			{
				return m_LanguageName;
			}
			if (base.Description != null && base.Description != string.Empty && base.Description != " ")
			{
				if (!base.Description.StartsWith("FCE_"))
				{
					return base.Description.Replace('_', ' ');
				}
				return base.Description.Substring(4).Replace('_', ' ');
			}
			return base.TypeString;
		}

		public Group(int id, string typeString, string description, Compobj parentObj)
			: base(id, 5, typeString, description, parentObj)
		{
			m_ConventionalDescription = description;
			m_Ranks = new RankList();
			Rank value = new Rank(this, 0);
			m_Ranks.Add(value);
			m_Groups = new GroupList();
		}

		public void AddSchedule(Schedule schedule)
		{
			if (m_Schedule == null)
			{
				m_Schedule = new ScheduleArray(256);
				m_NSchedule = 0;
			}
			m_Schedule.AddSchedule(schedule);
		}

		public Schedule[] GetLegSchedule(int legId)
		{
			if (m_Schedule == null)
			{
				return null;
			}
			return m_Schedule.GetLegSchedule(legId);
		}

		public Schedule[] GetLastLegSchedule()
		{
			if (m_Schedule == null)
			{
				return null;
			}
			return m_Schedule.GetLastLegSchedule();
		}

		public bool RemoveLastLeg()
		{
			if (m_Schedule == null)
			{
				return false;
			}
			return m_Schedule.RemoveLastLeg();
		}

		public Schedule AppendLeg(int dayDelay)
		{
			if (m_Schedule == null)
			{
				m_Schedule = new ScheduleArray(8);
			}
			return m_Schedule.AppendLeg(this, dayDelay);
		}

		public void CloneSchedule(Schedule originalSchedule, int timeDelay)
		{
			if (m_Schedule != null)
			{
				m_Schedule.CloneSchedule(originalSchedule, timeDelay);
			}
		}

		public void DeleteSchedule(Schedule originalSchedule)
		{
			if (m_Schedule != null)
			{
				m_Schedule.DeleteSchedule(originalSchedule);
			}
		}

		public override bool SaveToStandings(StreamWriter w)
		{
			for (int i = 1; i < m_Ranks.Count; i++)
			{
				Rank rank = (Rank)m_Ranks[i];
				string value = base.Id.ToString() + "," + (rank.Id - 1).ToString();
				w.WriteLine(value);
			}
			return true;
		}

		public override bool SaveToAdvancement(StreamWriter w)
		{
			for (int i = 1; i < m_Ranks.Count; i++)
			{
				Rank rank = (Rank)m_Ranks[i];
				if (rank.MoveFrom != null)
				{
					string value = rank.MoveFrom.Group.Id.ToString() + "," + rank.MoveFrom.Id.ToString() + "," + base.Id.ToString() + "," + rank.Id.ToString();
					w.WriteLine(value);
				}
			}
			return true;
		}

		public override bool SaveToSchedule(StreamWriter w)
		{
			if (m_Schedule == null)
			{
				return false;
			}
			return m_Schedule.SaveToSchedule(w);
		}

		public override void LinkCompetitions()
		{
			for (int i = 0; i < m_NStartTasks; i++)
			{
				m_StartTask[i].LinkGroup(this);
			}
			for (int j = 0; j < m_NEndTasks; j++)
			{
				m_EndTask[j].LinkGroup(this);
			}
		}

		public override bool SaveToTasks(StreamWriter w)
		{
			if (w == null)
			{
				return false;
			}
			if (ParentTrophy == null)
			{
				return false;
			}
			for (int i = 0; i < m_NStartTasks; i++)
			{
				m_StartTask[i].SaveToTasks(w);
			}
			for (int j = 0; j < m_NEndTasks; j++)
			{
				m_EndTask[j].SaveToTasks(w);
			}
			return true;
		}

		public override bool FillFromLanguage()
		{
			if (FifaEnvironment.Language != null)
			{
				if (m_ConventionalDescription != null && m_ConventionalDescription != string.Empty && m_ConventionalDescription != " ")
				{
					m_LanguageName = FifaEnvironment.Language.GetString(m_ConventionalDescription);
					if (m_LanguageName == null)
					{
						m_LanguageName = m_ConventionalDescription;
					}
				}
			}
			else
			{
				m_LanguageName = string.Empty;
			}
			return true;
		}

		public override bool SaveToLanguage()
		{
			if (FifaEnvironment.Language != null)
			{
				FifaEnvironment.Language.SetString(m_ConventionalDescription, m_LanguageName);
				return true;
			}
			return false;
		}

		public bool AddRank()
		{
			int count = m_Ranks.Count;
			Rank rank = new Rank(this, count);
			if (rank == null)
			{
				return false;
			}
			m_Ranks.Add(rank);
			return true;
		}

		public bool RemoveRank()
		{
			if (m_Ranks.Count < 1)
			{
				return false;
			}
			Rank obj = (Rank)m_Ranks[m_Ranks.Count - 1];
			m_Ranks.Remove(obj);
			return true;
		}

		public bool RemoveAllRanks()
		{
			int count = m_Ranks.Count;
			for (int i = 0; i < count; i++)
			{
				RemoveRank();
			}
			return true;
		}

		public override void Normalize()
		{
			Stage stage = null;
			if (base.Settings.Advance_pointskeep != -1)
			{
				if (base.ParentObj.IsStage())
				{
					stage = (Stage)base.ParentObj;
				}
				if (stage.Settings.Advance_pointskeep == -1)
				{
					stage.Settings.Advance_pointskeep = base.Settings.Advance_pointskeep;
				}
				base.Settings.Advance_pointskeep = -1;
			}
			if (base.Settings.m_advance_pointskeeppercentage != -1)
			{
				if (base.ParentObj.IsStage())
				{
					stage = (Stage)base.ParentObj;
				}
				if (stage.Settings.m_advance_pointskeeppercentage == -1)
				{
					stage.Settings.m_advance_pointskeeppercentage = base.Settings.m_advance_pointskeeppercentage;
				}
				base.Settings.m_advance_pointskeeppercentage = -1;
			}
			if (base.Settings.Advance_standingsrank != -1)
			{
				if (base.ParentObj.IsStage())
				{
					stage = (Stage)base.ParentObj;
				}
				if (stage.Settings.Advance_standingsrank == -1)
				{
					stage.Settings.Advance_standingsrank = base.Settings.Advance_standingsrank;
				}
				base.Settings.Advance_standingsrank = -1;
			}
			if (base.Settings.m_info_prize_money != -1)
			{
				if (base.ParentObj.IsStage())
				{
					stage = (Stage)base.ParentObj;
				}
				if (stage.Settings.m_info_prize_money == -1)
				{
					stage.Settings.m_info_prize_money = base.Settings.m_info_prize_money;
				}
				base.Settings.m_info_prize_money = -1;
			}
			if (base.Settings.m_match_canusefancards != null)
			{
				if (base.ParentObj.IsStage())
				{
					stage = (Stage)base.ParentObj;
				}
				if (stage.Settings.m_match_canusefancards == null)
				{
					stage.Settings.m_match_canusefancards = base.Settings.m_match_canusefancards;
				}
				base.Settings.m_match_canusefancards = null;
			}
			if (base.Settings.m_match_stadium != null && base.Settings.m_match_stadium.Length != 0)
			{
				if (base.ParentObj.IsStage())
				{
					stage = (Stage)base.ParentObj;
				}
				if (stage.Settings.m_match_stadium == null || stage.Settings.m_match_stadium.Length == 0)
				{
					stage.Settings.m_match_stadium = new int[12];
					for (int i = 0; i < stage.Settings.m_match_stadium.Length; i++)
					{
						stage.Settings.m_match_stadium[i] = -1;
					}
					for (int j = 0; j < base.Settings.m_match_stadium.Length; j++)
					{
						stage.Settings.m_match_stadium[j] = base.Settings.m_match_stadium[j];
					}
				}
				for (int k = 0; k < base.Settings.m_match_stadium.Length; k++)
				{
					base.Settings.m_match_stadium[k] = -1;
				}
			}
			if (base.Settings.m_StandingsSort != -1)
			{
				if (base.ParentObj.IsStage())
				{
					stage = (Stage)base.ParentObj;
				}
				if (stage.Settings.m_StandingsSort == -1)
				{
					stage.Settings.m_StandingsSort = base.Settings.m_StandingsSort;
				}
				base.Settings.m_StandingsSort = -1;
			}
			if (!base.ParentObj.IsStage())
			{
				return;
			}
			stage = (Stage)base.ParentObj;
			int num = 0;
			while (true)
			{
				if (num < m_NEndTasks)
				{
					if (m_EndTask[num].Action == "UpdateLeagueTable")
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			stage.AddTask(m_EndTask[num]);
			RemoveTask(m_EndTask[num].When, num);
		}

		public bool RemoveAllSchedules()
		{
			if (m_Schedule == null)
			{
				return false;
			}
			m_Schedule.Clear();
			return true;
		}

		public bool AddTask(Task action)
		{
			if (action.When == "start")
			{
				if (m_NStartTasks == 0)
				{
					m_StartTask = new Task[48];
				}
				if (m_NStartTasks < m_StartTask.Length)
				{
					m_StartTask[m_NStartTasks] = action;
					m_NStartTasks++;
					return true;
				}
				return false;
			}
			if (action.When == "end")
			{
				if (m_NEndTasks == 0)
				{
					m_EndTask = new Task[48];
				}
				if (m_NEndTasks < m_EndTask.Length)
				{
					m_EndTask[m_NEndTasks] = action;
					m_NEndTasks++;
					return true;
				}
				return false;
			}
			return false;
		}

		public bool RemoveLastTask(string when)
		{
			if (when == "start")
			{
				if (m_NStartTasks > 0)
				{
					m_NStartTasks--;
					m_StartTask[m_NStartTasks] = null;
					return true;
				}
				return false;
			}
			if (when == "end")
			{
				if (m_NEndTasks > 0)
				{
					m_NEndTasks--;
					m_EndTask[m_NEndTasks] = null;
					return true;
				}
				return false;
			}
			return false;
		}

		public int SearchTaskIndex(string when, string action, int par1, int par2, int par3)
		{
			if (when == "start")
			{
				for (int i = 0; i < m_NStartTasks; i++)
				{
					if ((action == null || m_StartTask[i].Action == action) && (par1 < 0 || m_StartTask[i].Parameter1 == par1) && (par2 < 0 || m_StartTask[i].Parameter2 == par2) && (par3 < 0 || m_StartTask[i].Parameter3 == par3))
					{
						return i;
					}
				}
			}
			else if (when == "end")
			{
				for (int j = 0; j < m_NEndTasks; j++)
				{
					if ((action == null || m_EndTask[j].Action == action) && (par1 < 0 || m_EndTask[j].Parameter1 == par1) && (par2 < 0 || m_EndTask[j].Parameter2 == par2) && (par3 < 0 || m_EndTask[j].Parameter3 == par3))
					{
						return j;
					}
				}
			}
			return -1;
		}

		public Task SearchTask(string when, string action, int par1, int par2, int par3)
		{
			int num = SearchTaskIndex(when, action, par1, par2, par3);
			if (num >= 0)
			{
				return GetTask(when, num);
			}
			return null;
		}

		public bool RemoveTask(string when, int index)
		{
			if (when == "start")
			{
				if (index < m_NStartTasks)
				{
					m_NStartTasks--;
					for (int i = index; i < m_NStartTasks; i++)
					{
						m_StartTask[i] = m_StartTask[i + 1];
					}
					return true;
				}
			}
			else if (index < m_NEndTasks)
			{
				m_NEndTasks--;
				for (int j = index; j < m_NEndTasks; j++)
				{
					m_EndTask[j] = m_EndTask[j + 1];
				}
				return true;
			}
			return false;
		}

		public bool RemoveTask(string when, string action, int par1, int par2, int par3)
		{
			int num = SearchTaskIndex(when, action, par1, par2, par3);
			if (num >= 0)
			{
				return RemoveTask(when, num);
			}
			return false;
		}

		public bool ReplaceTask(Task task, int index)
		{
			if (task.When == "start")
			{
				if (index < m_NStartTasks)
				{
					m_StartTask[index] = task;
					return true;
				}
			}
			else if (index < m_NEndTasks)
			{
				m_EndTask[index] = task;
				return true;
			}
			return false;
		}

		public Task GetTask(string when, int index)
		{
			if (when == "start")
			{
				if (index < m_NStartTasks)
				{
					return m_StartTask[index];
				}
			}
			else if (index < m_NEndTasks)
			{
				return m_EndTask[index];
			}
			return null;
		}

		public void CopyTasks(Group newGroup, League targetLeague)
		{
			for (int i = 0; i < m_NStartTasks; i++)
			{
				newGroup.AddTask(m_StartTask[i].CopyTask(newGroup, targetLeague));
			}
			for (int j = 0; j < m_NEndTasks; j++)
			{
				newGroup.AddTask(m_EndTask[j].CopyTask(newGroup, targetLeague));
			}
		}
	}
}
