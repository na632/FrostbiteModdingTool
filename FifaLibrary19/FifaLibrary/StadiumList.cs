using System;
using System.Collections;

namespace FifaLibrary
{
	public class StadiumList : IdArrayList
	{
		public StadiumList()
			: base(typeof(Stadium))
		{
		}

		public StadiumList(DbFile fifaDbFile)
			: base(typeof(Stadium))
		{
			Load(fifaDbFile);
		}

		public void Load(DbFile fifaDbFile)
		{
			Table table = FifaEnvironment.FifaDb.Table[TI.teamstadiumlinks];
			int minId = table.TableDescriptor.MinValues[FI.teamstadiumlinks_stadiumid];
			int maxId = table.TableDescriptor.MaxValues[FI.teamstadiumlinks_stadiumid];
			table = FifaEnvironment.FifaDb.Table[TI.stadiums];
			Load(table, minId, maxId);
		}

		public void Load(Table t, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			Clear();
			for (int i = 0; i < t.NRecords; i++)
			{
				Stadium value = new Stadium(t.Records[i]);
				Add(value);
			}
			SortId();
		}

		public void LinkTeam(TeamList teamList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Stadium)enumerator.Current).LinkTeam(teamList);
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

		public void LinkCountry(CountryList countryList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Stadium)enumerator.Current).LinkCountry(countryList);
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

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.stadiums];
			table.ResizeRecords(Count);
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Stadium obj = (Stadium)enumerator.Current;
					Record r = table.Records[num];
					num++;
					obj.SaveStadium(r);
					obj.SaveLangTable();
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

		public void DeleteStadium(Stadium stadium)
		{
			RemoveId(stadium);
		}

		public Stadium FitStadium(string name, int id)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Stadium stadium = (Stadium)enumerator.Current;
					if (stadium.name == name || stadium.LocalName == name)
					{
						return stadium;
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
			StadiumList stadiumList = new StadiumList();
			if (filter == null)
			{
				return this;
			}
			if (filter.GetType().Name == "Country")
			{
				Country country = (Country)filter;
				for (int i = 0; i < Count; i++)
				{
					Stadium stadium = (Stadium)this[i];
					if (stadium.Country == country)
					{
						stadiumList.Add(stadium);
					}
				}
				return stadiumList;
			}
			return this;
		}
	}
}
