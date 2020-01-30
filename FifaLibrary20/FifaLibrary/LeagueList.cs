using System;
using System.Collections;

namespace FifaLibrary
{
	public class LeagueList : IdArrayList
	{
		public LeagueList()
			: base(typeof(League))
		{
		}

		public LeagueList(DbFile fifaDbFile)
			: base(typeof(League))
		{
			Load(fifaDbFile);
		}

		public void Load(DbFile fifaDbFile)
		{
			Table obj = FifaEnvironment.FifaDb.Table[TI.leagues];
			int minId = 1;
			int maxId = obj.TableDescriptor.MaxValues[FI.leagues_leagueid];
			Table t = FifaEnvironment.FifaDb.Table[TI.leagues];
			Load(t, minId, maxId);
			t = fifaDbFile.Table[TI.career_boardoutcomes];
			FillFromBoardOutcomes(t);
		}

		public void Load(Table t, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			Clear();
			for (int i = 0; i < t.NRecords; i++)
			{
				League value = new League(t.Records[i]);
				Add(value);
			}
			SortId();
		}

		public void FillFromBoardOutcomes(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.career_boardoutcomes_leagueid];
				((League)SearchId(id))?.FillFromBoardOutcomes(record);
			}
		}

		public League AddNewLeague()
		{
			int num = Trophy.AutoAsset();
			if (num < 0)
			{
				return null;
			}
			return (League)CreateNewId(num);
		}

		public void DeleteLeague(League league)
		{
			RemoveId(league);
		}

		public void FillFromLeagueTeamLinks(DbFile fifaDbFile)
		{
			if (FifaEnvironment.Teams != null)
			{
				Table leagueteamlinksTable = fifaDbFile.Table[TI.leagueteamlinks];
				FillFromLeagueTeamLinks(leagueteamlinksTable);
			}
		}

		public void FillFromLeagueTeamLinks(Table leagueteamlinksTable)
		{
			if (FifaEnvironment.Teams == null)
			{
				return;
			}
			for (int i = 0; i < leagueteamlinksTable.NRecords; i++)
			{
				Record obj = leagueteamlinksTable.Records[i];
				int num = obj.IntField[FI.leagueteamlinks_teamid];
				int num2 = obj.IntField[FI.leagueteamlinks_leagueid];
				Team team = (Team)FifaEnvironment.Teams.SearchId(num);
				if (team == null)
				{
					FifaEnvironment.UserMessages.ShowMessage(5022, num);
					continue;
				}
				League league = (League)SearchId(num2);
				if (league == null)
				{
					FifaEnvironment.UserMessages.ShowMessage(5023, num2);
					continue;
				}
				league.LinkTeam(team);
				team.League = league;
			}
		}

		public League SearchLeague(int leagueid)
		{
			return (League)SearchId(leagueid);
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.leagues];
			Table table2 = fifaDbFile.Table[TI.leagueteamlinks];
			Table table3 = fifaDbFile.Table[TI.career_boardoutcomes];
			table.ResizeRecords(Count);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					League league = (League)enumerator.Current;
					Record r = table.Records[num2];
					num2++;
					league.SaveLeague(r);
					num += league.PlayingTeams.Count;
					if (league.boardoutcomes[0] != 0)
					{
						num3++;
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
			table2.ResizeRecords(num);
			table3.ResizeRecords(num3);
			int num4 = 0;
			num2 = 0;
			int num5 = 0;
			enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					League league2 = (League)enumerator.Current;
					foreach (Team playingTeam in league2.PlayingTeams)
					{
						Record r2 = table2.Records[num2];
						league2.SaveTeamLink(r2, playingTeam, num4);
						num4++;
						num2++;
					}
					if (league2.boardoutcomes[0] != 0)
					{
						Record r3 = table3.Records[num5];
						league2.SaveBoardOutcomes(r3);
						num5++;
					}
				}
			}
			finally
			{
				IDisposable disposable2 = enumerator as IDisposable;
				if (disposable2 != null)
				{
					disposable2.Dispose();
				}
			}
			enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					League league3 = (League)enumerator.Current;
					int id = league3.Id;
					if (FifaEnvironment.Language != null)
					{
						FifaEnvironment.Language.SetLeagueString(id, Language.ELeagueStringType.Abbr15, league3.ShortName);
						FifaEnvironment.Language.SetLeagueString(id, Language.ELeagueStringType.Full, league3.LongName);
					}
				}
			}
			finally
			{
				IDisposable disposable3 = enumerator as IDisposable;
				if (disposable3 != null)
				{
					disposable3.Dispose();
				}
			}
		}

		public League FitLeague(string name, int id)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					League league = (League)enumerator.Current;
					if (league.leaguename == name || league.ShortName == name || league.LongName == name)
					{
						return league;
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
			return null;
		}

		public override IdArrayList Filter(IdObject filter)
		{
			if (filter == null)
			{
				return this;
			}
			LeagueList leagueList = new LeagueList();
			if (filter.GetType().Name == "Country")
			{
				Country country = (Country)filter;
				for (int i = 0; i < Count; i++)
				{
					League league = (League)this[i];
					if (league.Country == country)
					{
						leagueList.Add(league);
					}
				}
				return leagueList;
			}
			return this;
		}

		public void LinkCountry(CountryList countryList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((League)enumerator.Current).LinkCountry(countryList);
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

		public void LinkBall(BallList ballList)
		{
			if (ballList != null)
			{
				{
					IEnumerator enumerator = GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							((Team)enumerator.Current).LinkBall(ballList);
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
	}
}
