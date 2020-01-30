using System;
using System.IO;

namespace FifaLibrary
{
	public class Schedule
	{
		private Stage m_Stage;

		private Group m_Group;

		private int m_Leg;

		private int m_MinGames;

		private int m_MaxGames;

		private int m_Time;

		private int m_Day;

		private int m_Year;

		public static DateTime s_BaseDate = new DateTime(2012, 12, 30, 0, 0, 0);

		public int Leg
		{
			get
			{
				return m_Leg;
			}
			set
			{
				m_Leg = value;
			}
		}

		public int MinGames
		{
			get
			{
				return m_MinGames;
			}
			set
			{
				m_MinGames = value;
			}
		}

		public int MaxGames
		{
			get
			{
				return m_MaxGames;
			}
			set
			{
				m_MaxGames = value;
			}
		}

		public int Time
		{
			get
			{
				return m_Time;
			}
			set
			{
				m_Time = value;
			}
		}

		public int TimeIndex
		{
			get
			{
				return (m_Time / 100 - 12) * 4 + m_Time % 100 / 15;
			}
			set
			{
				m_Time = value / 4 * 100 + 1200;
				m_Time += value % 4 * 15;
			}
		}

		public int Day
		{
			get
			{
				return m_Day;
			}
			set
			{
				m_Day = value;
			}
		}

		public int Year => m_Year;

		public DateTime Date
		{
			get
			{
				TimeSpan value = new TimeSpan(m_Day, m_Time / 100, m_Time % 100, 0);
				if (m_Day < 0)
				{
					return s_BaseDate;
				}
				DateTime result = s_BaseDate.Add(value);
				result.AddHours(m_Time / 100);
				result.AddMinutes(m_Time % 100);
				return result;
			}
			set
			{
				m_Day = (value - s_BaseDate).Days;
			}
		}

		public DateTime ConvertToDate(int gregorian)
		{
			DateTime result = s_BaseDate;
			if (gregorian < 0)
			{
				return result;
			}
			return result.AddDays(gregorian);
		}

		public int ConvertFromDate(DateTime date)
		{
			DateTime d = s_BaseDate;
			return (date - d).Days;
		}

		public Schedule(Stage stage, int day, int leg, int minGames, int maxGames, int time)
		{
			m_Stage = stage;
			m_Leg = leg;
			m_MinGames = minGames;
			m_MaxGames = maxGames;
			m_Time = time;
			m_Day = day;
			m_Year = 2012;
			bool isSpecific;
			string property = stage.Settings.GetProperty("schedule_year_start", 0, out isSpecific);
			if (property != null)
			{
				m_Year = Convert.ToInt32(property);
			}
		}

		public Schedule(Group group, int day, int leg, int minGames, int maxGames, int time)
		{
			m_Group = group;
			m_Leg = leg;
			m_MinGames = minGames;
			m_MaxGames = maxGames;
			m_Time = time;
			m_Day = day;
			m_Year = 2012;
			bool isSpecific;
			string property = group.Settings.GetProperty("schedule_year_start", 0, out isSpecific);
			if (property != null)
			{
				m_Year = Convert.ToInt32(property);
			}
		}

		public Schedule(Schedule srcSchedule)
		{
			m_Day = srcSchedule.m_Day;
			m_Leg = srcSchedule.m_Leg;
			m_MinGames = srcSchedule.m_MinGames;
			m_MaxGames = srcSchedule.m_MaxGames;
			m_Time = srcSchedule.m_Time;
			m_Group = srcSchedule.m_Group;
			m_Stage = srcSchedule.m_Stage;
			m_Year = srcSchedule.m_Year;
		}

		public bool SaveToSchedule(StreamWriter w)
		{
			if (w == null)
			{
				return false;
			}
			string text;
			if (m_Group != null)
			{
				text = m_Group.Id.ToString() + ",";
			}
			else
			{
				if (m_Stage == null)
				{
					return false;
				}
				text = m_Stage.Id.ToString() + ",";
			}
			text = text + m_Day.ToString() + "," + m_Leg.ToString() + "," + m_MinGames.ToString() + "," + m_MaxGames.ToString() + "," + m_Time.ToString();
			w.WriteLine(text);
			return true;
		}
	}
}
