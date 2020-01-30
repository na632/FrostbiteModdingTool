using System;
using System.Collections;

namespace FifaLibrary
{
	public class InitTeamList : IdArrayList
	{
		public InitTeamList()
			: base(typeof(InitTeam))
		{
		}

		public void LinkTeam(TeamList teamList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((InitTeam)enumerator.Current).LinkTeam(teamList);
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
