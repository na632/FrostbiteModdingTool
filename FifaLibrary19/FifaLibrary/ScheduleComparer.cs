using System.Collections;

namespace FifaLibrary
{
	public class ScheduleComparer : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			Schedule schedule = (Schedule)x;
			Schedule schedule2 = (Schedule)y;
			if (schedule.Day != schedule2.Day)
			{
				return schedule.Day - schedule2.Day;
			}
			return schedule.Time - schedule2.Time;
		}
	}
}
