using System;
using System.Collections;
using System.IO;

namespace FifaLibrary
{
	public class ScheduleArray : ArrayList
	{
		private Schedule[] m_Schedules;

		private int m_NSchedules;

		private static ScheduleComparer m_Comparer = new ScheduleComparer();

		public Schedule[] Schedules
		{
			get
			{
				return m_Schedules;
			}
			set
			{
				m_Schedules = value;
			}
		}

		public int NSchedules
		{
			get
			{
				return m_NSchedules;
			}
			set
			{
				m_NSchedules = value;
			}
		}

		public ScheduleArray(int length)
		{
			m_NSchedules = 0;
		}

		public Schedule[] GetLegSchedule(int legId)
		{
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (((Schedule)enumerator.Current).Leg == legId)
					{
						num++;
					}
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			if (num == 0)
			{
				return null;
			}
			Schedule[] array = new Schedule[num];
			num = 0;
			enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Schedule schedule = (Schedule)enumerator.Current;
					if (schedule.Leg == legId)
					{
						array[num++] = schedule;
					}
				}
				return array;
			}
			finally
			{
				IDisposable disposable2 = enumerator as IDisposable;
				if (disposable2 != null)
				{
					disposable2.Dispose();
				}
			}
		}

		public void RenumberLegs()
		{
			Sort(m_Comparer);
			int num = 0;
			int num2 = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Schedule obj = (Schedule)enumerator.Current;
					int leg = obj.Leg;
					if (num2 != leg)
					{
						num2 = leg;
						num++;
					}
					obj.Leg = num;
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}

		public Schedule DuplicatetLeg(int legId, int dayDelay)
		{
			Schedule[] legSchedule = GetLegSchedule(legId);
			if (legSchedule == null)
			{
				return null;
			}
			int num = -1;
			int num2 = 0;
			for (int i = 0; i < Count; i++)
			{
				if (((Schedule)this[i]).Leg == legId)
				{
					if (num < 0)
					{
						num = i;
					}
					num2++;
				}
			}
			if (num2 == 0)
			{
				return null;
			}
			num += num2;
			Schedule schedule = null;
			for (int num3 = legSchedule.Length - 1; num3 >= 0; num3--)
			{
				schedule = new Schedule(legSchedule[num3]);
				schedule.Day += dayDelay;
				Insert(num, schedule);
			}
			for (int j = num; j < Count; j++)
			{
				((Schedule)this[j]).Leg++;
			}
			return schedule;
		}

		public bool RemoveLeg(int legId)
		{
			int num = -1;
			int num2 = 0;
			for (int i = 0; i < Count; i++)
			{
				if (((Schedule)this[i]).Leg == legId)
				{
					if (num < 0)
					{
						num = i;
					}
					num2++;
				}
			}
			if (num2 > 0)
			{
				RemoveRange(num, num2);
				for (int i = num; i < Count; i++)
				{
					((Schedule)this[i]).Leg--;
				}
				return true;
			}
			return false;
		}

		public bool AddScheduleToLeg(Schedule schedule)
		{
			int num = -1;
			int num2 = 0;
			for (int i = 0; i < Count; i++)
			{
				if (((Schedule)this[i]).Leg == schedule.Leg)
				{
					if (num < 0)
					{
						num = i;
					}
					num2++;
				}
			}
			if (num2 > 0)
			{
				Insert(num + num2, schedule);
				return true;
			}
			return false;
		}

		public bool DeleteSchedule(Schedule schedule)
		{
			for (int i = 0; i < Count; i++)
			{
				Schedule schedule2 = (Schedule)this[i];
				if (schedule2.Leg != schedule.Leg || schedule2.Day != schedule.Day || schedule2.Time != schedule.Time || schedule2.MinGames != schedule.MinGames || schedule2.MaxGames != schedule.MaxGames)
				{
					continue;
				}
				bool flag = true;
				if (i > 0)
				{
					Schedule schedule3 = (Schedule)this[i - 1];
					if (schedule.Leg == schedule3.Leg)
					{
						flag = false;
					}
				}
				if (i < Count - 1)
				{
					Schedule schedule4 = (Schedule)this[i + 1];
					if (schedule.Leg == schedule4.Leg)
					{
						flag = false;
					}
				}
				if (flag)
				{
					RemoveLeg(schedule.Leg);
				}
				else
				{
					RemoveAt(i);
				}
				return true;
			}
			return false;
		}

		public void AddSchedule(Schedule schedule)
		{
			Add(schedule);
		}

		public Schedule[] GetLastLegSchedule()
		{
			if (Count <= 0)
			{
				return null;
			}
			int leg = ((Schedule)this[Count - 1]).Leg;
			return GetLegSchedule(leg);
		}

		public bool RemoveLastLeg()
		{
			if (Count <= 0)
			{
				return false;
			}
			int leg = ((Schedule)this[Count - 1]).Leg;
			return RemoveLeg(leg);
		}

		public Schedule AppendLeg(Compobj compobj, int dayDelay)
		{
			if (Count <= 0)
			{
				if (compobj.IsGroup())
				{
					Schedule schedule = new Schedule((Group)compobj, 215, 1, 1, 1, 1500);
					AddSchedule(schedule);
					return schedule;
				}
				if (compobj.IsStage())
				{
					Schedule schedule2 = new Schedule((Stage)compobj, 215, 1, 1, 1, 1500);
					AddSchedule(schedule2);
					return schedule2;
				}
				return null;
			}
			Schedule[] lastLegSchedule = GetLastLegSchedule();
			Schedule schedule3 = null;
			for (int i = 0; i < lastLegSchedule.Length; i++)
			{
				schedule3 = new Schedule(lastLegSchedule[i]);
				schedule3.Leg++;
				schedule3.Day += dayDelay;
				AddSchedule(schedule3);
			}
			return schedule3;
		}

		public void CloneSchedule(Schedule originalSchedule, int timeDelay)
		{
			Schedule schedule = new Schedule(originalSchedule);
			schedule.Time += timeDelay;
			AddScheduleToLeg(schedule);
		}

		public bool SaveToSchedule(StreamWriter w)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Schedule)enumerator.Current).SaveToSchedule(w);
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return true;
		}
	}
}
