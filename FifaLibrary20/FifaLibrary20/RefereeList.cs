using System;
using System.Collections;

namespace FifaLibrary
{
	public class RefereeList : IdArrayList
	{
		public static KitList s_RefereeKits;

		public RefereeList()
			: base(typeof(Referee))
		{
		}

		public RefereeList(DbFile fifaDbFile)
			: base(typeof(Referee))
		{
			Load(fifaDbFile);
		}

		public void Load(DbFile fifaDbFile)
		{
			int minId = FifaEnvironment.FifaDb.Table[TI.referee].TableDescriptor.MinValues[FI.referee_refereeid];
			int maxId = FifaEnvironment.FifaDb.Table[TI.referee].TableDescriptor.MaxValues[FI.referee_refereeid];
			Table t = fifaDbFile.Table[TI.referee];
			Load(t, minId, maxId);
			t = fifaDbFile.Table[TI.leaguerefereelinks];
			FillFromLeagueRefereeLinks(t);
		}

		public void Load(Table t, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			Clear();
			for (int i = 0; i < t.NRecords; i++)
			{
				Referee value = new Referee(t.Records[i]);
				Add(value);
			}
			SortId();
		}

		public void FillFromLeagueRefereeLinks(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.leaguerefereelinks_refereeid];
				((Referee)SearchId(id))?.FillFromLeagueRefereeLinks(record);
			}
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.referee];
			table.ResizeRecords(Count);
			Table table2 = fifaDbFile.Table[TI.leaguerefereelinks];
			int num = 0;
			int num2 = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Referee referee = (Referee)enumerator.Current;
					Record r = table.Records[num2];
					referee.SaveReferee(r);
					num2++;
					num += referee.CntLeagues();
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
			int num3 = 0;
			enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Referee referee2 = (Referee)enumerator.Current;
					for (int i = 0; i < referee2.Leagues.Length; i++)
					{
						if (referee2.Leagues[i] != null)
						{
							Record r2 = table2.Records[num3];
							referee2.SaveLeagueRefereeLinks(r2, i);
							num3++;
						}
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
		}

		public int GetNewRefereeHeadId()
		{
			int result = 0;
			int num = FifaEnvironment.FifaDb.Table[TI.referee].TableDescriptor.MinValues[FI.referee_refereeid];
			for (int num2 = FifaEnvironment.FifaDb.Table[TI.referee].TableDescriptor.MaxValues[FI.referee_refereeid]; num2 >= num; num2--)
			{
				bool flag = false;
				{
					IEnumerator enumerator = GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							Referee referee = (Referee)enumerator.Current;
							if (num2 == referee.Id)
							{
								flag = true;
								break;
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
				}
				if (!flag)
				{
					result = num2;
					break;
				}
			}
			return result;
		}

		public Referee FitReferee(string name, int id)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Referee referee = (Referee)enumerator.Current;
					if (referee.ToString() == name)
					{
						return referee;
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

		public void DeleteReferee(Referee referee)
		{
			RemoveId(referee);
		}

		public void LinkCountry(CountryList countryList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Referee)enumerator.Current).LinkCountry(countryList);
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

		public void LinkLeague(LeagueList leagueList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Referee)enumerator.Current).LinkLeague(leagueList);
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

		public override IdArrayList Filter(IdObject filter)
		{
			if (filter == null)
			{
				return this;
			}
			RefereeList refereeList = new RefereeList();
			if (filter.GetType().Name == "Country")
			{
				Country country = (Country)filter;
				for (int i = 0; i < Count; i++)
				{
					Referee referee = (Referee)this[i];
					if (referee.Country == country)
					{
						refereeList.Add(referee);
					}
				}
				return refereeList;
			}
			if (filter.GetType().Name == "League")
			{
				League league = (League)filter;
				for (int j = 0; j < Count; j++)
				{
					Referee referee2 = (Referee)this[j];
					if (referee2.IsInLeague(league))
					{
						refereeList.Add(referee2);
					}
				}
				return refereeList;
			}
			return this;
		}
	}
}
