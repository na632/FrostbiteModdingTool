using System;
using System.Collections;

namespace FifaLibrary
{
	public class CountryList : IdArrayList
	{
		public CountryList()
			: base(typeof(Country))
		{
		}

		public CountryList(DbFile fifaDbFile)
			: base(typeof(Country))
		{
			Load(fifaDbFile);
		}

		public void Load(DbFile fifaDbFile)
		{
			Table obj = FifaEnvironment.FifaDb.Table[TI.nations];
			int minId = obj.TableDescriptor.MinValues[FI.nations_nationid];
			int maxId = obj.TableDescriptor.MaxValues[FI.nations_nationid];
			Table t = FifaEnvironment.FifaDb.Table[TI.nations];
			Load(t, minId, maxId);
		}

		public void Load(Table t, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			Country[] array = new Country[t.NRecords];
			Clear();
			for (int i = 0; i < t.NRecords; i++)
			{
				array[i] = new Country(t.Records[i]);
			}
			AddRange(array);
			SortId();
		}

		public void DeleteCountry(Country country)
		{
			RemoveId(country);
		}

		public Country SearchCountry(int countryid)
		{
			return (Country)SearchId(countryid);
		}

		public Country SearchCountry(string countryString)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Country country = (Country)enumerator.Current;
					if (countryString == country.ToString())
					{
						return country;
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

		public Country SearchCountryByDatabaseName(string countryString)
		{
			if (countryString.Contains("Bosnia"))
			{
				return SearchCountry(8);
			}
			if (countryString.Contains("Macedonia"))
			{
				return SearchCountry(19);
			}
			if (countryString == "Ireland")
			{
				return SearchCountry(25);
			}
			if (countryString == "Curacao")
			{
				return SearchCountry(34);
			}
			if (countryString == "Netherlands")
			{
				return SearchCountry(34);
			}
			if (countryString.Contains("Verde"))
			{
				return SearchCountry(104);
			}
			if (countryString.Contains("voire"))
			{
				return SearchCountry(108);
			}
			if (countryString.Contains("China"))
			{
				return SearchCountry(155);
			}
			if (countryString.Contains("Korea, North"))
			{
				return SearchCountry(166);
			}
			if (countryString.Contains("Korea, South"))
			{
				return SearchCountry(167);
			}
			if (countryString.Contains("aledon"))
			{
				return SearchCountry(215);
			}
			if (countryString == "French Guiana")
			{
				return SearchCountry(79);
			}
			if (countryString.Contains("Gambia"))
			{
				return SearchCountry(116);
			}
			if (countryString.Contains("Martinique"))
			{
				return SearchCountry(18);
			}
			if (countryString.Contains("Guadeloupe"))
			{
				return SearchCountry(18);
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Country country = (Country)enumerator.Current;
					if (countryString == country.DatabaseString())
					{
						return country;
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

		public Country SearchNationalTeamId(int nationalTeamId)
		{
			switch (nationalTeamId)
			{
			case 3145:
				return (Country)SearchId(34);
			case 1800:
				return null;
			default:
			{
				for (int i = 0; i < Count; i++)
				{
					Country country = (Country)this[i];
					if (nationalTeamId == country.NationalTeamId)
					{
						return country;
					}
				}
				return null;
			}
			}
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.nations];
			table.ResizeRecords(Count);
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Country obj = (Country)enumerator.Current;
					Record r = table.Records[num];
					obj.SaveCountry(r);
					obj.SaveLangTable();
					num++;
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

		public void FillFromAudionation(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				int id = t.Records[i].IntField[FI.audionation_nationid];
				_ = (Country)SearchId(id);
			}
		}

		public Country FitCountry(string name, int id)
		{
			string lowerName = name.ToLower();
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Country country = (Country)enumerator.Current;
					if (country.Fit(lowerName, id))
					{
						return country;
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

		public void LinkTeam(TeamList teamList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Country)enumerator.Current).LinkTeam(teamList);
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
