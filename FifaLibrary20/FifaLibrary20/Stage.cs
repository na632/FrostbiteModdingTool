using System.IO;

namespace FifaLibrary
{
	public class Stage : Compobj
	{
		private ScheduleArray m_Schedule;

		private int m_NSchedule;

		private string m_ConventionalDescription;

		private Task[] m_StartTask;

		private int m_NStartTasks;

		private Task[] m_EndTask;

		private int m_NEndTasks;

		public Trophy Trophy
		{
			get
			{
				if (base.ParentObj.TypeNumber == 3)
				{
					return (Trophy)base.ParentObj;
				}
				return null;
			}
		}

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

		public string ConventionalDescription
		{
			get
			{
				return m_ConventionalDescription;
			}
			set
			{
				m_ConventionalDescription = value;
			}
		}

		public override string ToString()
		{
			string languageName = GetLanguageName();
			if (languageName != null && languageName != string.Empty)
			{
				return languageName;
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

		public Stage(int id, string typeString, string description, Compobj parentObj)
			: base(id, 4, typeString, description, parentObj)
		{
			m_Groups = new GroupList();
		}

		public override void LinkCompetitions()
		{
			base.Settings.UpdateStageReferenceUsingId();
			for (int i = 0; i < m_NStartTasks; i++)
			{
				m_StartTask[i].LinkStage(this);
			}
			for (int j = 0; j < m_NEndTasks; j++)
			{
				m_EndTask[j].LinkStage(this);
			}
		}

		public void AddSchedule(Schedule schedule)
		{
			if (m_Schedule == null)
			{
				m_Schedule = new ScheduleArray(256);
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
				m_Schedule = new ScheduleArray(48);
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

		public override bool SaveToSchedule(StreamWriter w)
		{
			if (m_Schedule == null)
			{
				return false;
			}
			return m_Schedule.SaveToSchedule(w);
		}

		public string GetLanguageName()
		{
			if (FifaEnvironment.Language != null && base.Description != null)
			{
				string text = FifaEnvironment.Language.GetString(base.Description);
				if (text == null)
				{
					text = string.Empty;
				}
				return text;
			}
			return string.Empty;
		}

		public bool SetLanguageName(string languageName)
		{
			if (FifaEnvironment.Language != null && base.Description != null)
			{
				FifaEnvironment.Language.SetString(base.Description, languageName);
				return true;
			}
			return false;
		}

		public bool InsertGroup(int groupIndex)
		{
			if (groupIndex > base.Groups.Count)
			{
				return false;
			}
			string typeString = "G" + (groupIndex + 1).ToString();
			Group group = new Group(FifaEnvironment.CompetitionObjects.GetNewId(), typeString, " ", this);
			if (group == null)
			{
				return false;
			}
			base.Groups.Insert(groupIndex, group);
			FifaEnvironment.CompetitionObjects.Add(group);
			for (int i = 0; i < base.Groups.Count; i++)
			{
				typeString = "G" + (i + 1).ToString();
				((Group)base.Groups[i]).TypeString = typeString;
			}
			return true;
		}

		public bool RemoveGroup(Group group)
		{
			int num = base.Groups.IndexOf(group);
			if (num < 0)
			{
				return false;
			}
			RemoveGroup(num);
			return true;
		}

		public bool RemoveGroup(int groupIndex)
		{
			if (groupIndex > base.Groups.Count)
			{
				return false;
			}
			Group idObject = (Group)base.Groups[groupIndex];
			base.Groups.RemoveAt(groupIndex);
			FifaEnvironment.CompetitionObjects.RemoveId(idObject);
			for (int i = 0; i < base.Groups.Count; i++)
			{
				string typeString = "G" + (i + 1).ToString();
				((Group)base.Groups[i]).TypeString = typeString;
			}
			return true;
		}

		public bool RemoveAllGroups()
		{
			int count = base.Groups.Count;
			for (int i = 0; i < count; i++)
			{
				RemoveGroup(0);
			}
			return true;
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

		public new void LinkStadium(StadiumList stadiumList)
		{
		}

		public override void Normalize()
		{
			if (base.Settings.m_match_matchimportance != -1 && Trophy != null)
			{
				if (Trophy.Settings.m_match_matchimportance == -1)
				{
					Trophy.Settings.m_match_matchimportance = base.Settings.m_match_matchimportance;
				}
				base.Settings.m_match_matchimportance = -1;
			}
			if (base.Settings.m_N_info_color_slot_adv_group != 0 && base.Groups != null)
			{
				foreach (Group group3 in base.Groups)
				{
					if (group3.Settings.m_N_info_color_slot_adv_group == 0)
					{
						group3.Settings.m_info_color_slot_adv_group = new int[8];
						for (int i = 0; i < base.Settings.m_N_info_color_slot_adv_group; i++)
						{
							group3.Settings.m_info_color_slot_adv_group[i] = base.Settings.m_info_color_slot_adv_group[i];
						}
						group3.Settings.m_N_info_color_slot_adv_group = base.Settings.m_N_info_color_slot_adv_group;
					}
				}
				base.Settings.m_N_info_color_slot_adv_group = 0;
			}
			if (base.Settings.m_match_stagetype == null)
			{
				base.Settings.m_match_stagetype = "LEAGUE";
			}
			if (base.Settings.m_match_stagetype != "SETUP" && base.Settings.m_match_matchsituation == null)
			{
				base.Settings.m_match_matchsituation = "LEAGUE";
			}
			if (base.Settings.m_num_games != -1)
			{
				foreach (Group group4 in base.Groups)
				{
					if (group4.Settings.m_num_games == -1)
					{
						group4.Settings.m_num_games = base.Settings.m_num_games;
					}
				}
				base.Settings.m_num_games = -1;
			}
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
			}
			else if (action.When == "end")
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

		public void CopyTasks(Stage newStage, League targetLeague)
		{
			for (int i = 0; i < m_NStartTasks; i++)
			{
				newStage.AddTask(m_StartTask[i].CopyTask(newStage, targetLeague));
			}
			for (int j = 0; j < m_NEndTasks; j++)
			{
				newStage.AddTask(m_EndTask[j].CopyTask(newStage, targetLeague));
			}
		}

		public override bool SaveToTasks(StreamWriter w)
		{
			if (w == null)
			{
				return false;
			}
			if (Trophy == null)
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
	}
}
