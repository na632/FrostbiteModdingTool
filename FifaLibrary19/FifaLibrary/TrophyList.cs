using System;
using System.Collections;

namespace FifaLibrary
{
	public class TrophyList : IdArrayList
	{
		public TrophyList()
			: base(typeof(Trophy))
		{
		}

		public void LinkLeague(LeagueList leagueList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Trophy)enumerator.Current).LinkLeague(leagueList);
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
	}
}
