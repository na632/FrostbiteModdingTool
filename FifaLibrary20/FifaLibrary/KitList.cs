using System;
using System.Collections;

namespace FifaLibrary
{
	public class KitList : IdArrayList
	{
		public KitList()
			: base(typeof(Kit))
		{
		}

		public KitList(Type type)
			: base(type)
		{
		}

		public KitList(DbFile fifaDbFile)
			: base(typeof(Kit))
		{
			Load(fifaDbFile);
		}

		public void Load(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.teamkits];
			int minId = table.TableDescriptor.MinValues[FI.teamkits_teamkitid];
			int maxId = table.TableDescriptor.MaxValues[FI.teamkits_teamkitid];
			Load(table, minId, maxId);
		}

		public void Load(Table t, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			Clear();
			for (int i = 0; i < t.NRecords; i++)
			{
				Kit kit = new Kit(t.Records[i]);
				bool flag = false;
				while (SearchId(kit) != null)
				{
					kit.Id--;
					flag = true;
				}
				Add(kit);
				if (flag)
				{
					SortId();
				}
			}
			SortId();
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.teamkits];
			table.ResizeRecords(Count);
			int num = 0;
			int num2 = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Kit obj = (Kit)enumerator.Current;
					Record r = table.Records[num];
					obj.SaveKit(r, num2);
					num++;
					num2++;
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

		public void LinkTeam(TeamList teamList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Kit)enumerator.Current).LinkTeam(teamList);
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

		public bool IsJerseyFontUsed(int jerseyFontId)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (((Kit)enumerator.Current).jerseyNumberFont == jerseyFontId)
					{
						return true;
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
			return false;
		}

		public Kit GetKit(int teamId, int kitType)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Kit kit = (Kit)enumerator.Current;
					if (kit.teamid == teamId && kit.kittype == kitType)
					{
						return kit;
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
			KitList kitList = new KitList();
			if (filter.GetType().Name == "Team")
			{
				Team team = (Team)filter;
				if (team != null)
				{
					for (int i = 0; i < Count; i++)
					{
						Kit kit = (Kit)this[i];
						if (kit.Team == team)
						{
							kitList.Add(kit);
						}
					}
				}
				return kitList;
			}
			if (filter.GetType().Name == "Country")
			{
				Country country = (Country)filter;
				for (int j = 0; j < Count; j++)
				{
					Kit kit2 = (Kit)this[j];
					if (kit2.Team != null && kit2.Team.Country == country)
					{
						kitList.Add(kit2);
					}
				}
				return kitList;
			}
			if (filter.GetType().Name == "League")
			{
				League league = (League)filter;
				for (int k = 0; k < Count; k++)
				{
					Kit kit3 = (Kit)this[k];
					if (kit3.Team != null && kit3.Team.League == league)
					{
						kitList.Add(kit3);
					}
				}
				return kitList;
			}
			return this;
		}

		public bool Exists(int teamid, int kittype, int year)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Kit kit = (Kit)enumerator.Current;
					if (kit.teamid == teamid && kit.kittype == kittype && kit.year == year)
					{
						return true;
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
			return false;
		}

		public void DeleteKit(Kit kit)
		{
			RemoveId(kit);
		}

		public Kit FitKit(string name, int id)
		{
			name = name.Substring(0, name.IndexOf('('));
			int num = id / 10;
			int num2 = id - num * 10;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Kit kit = (Kit)enumerator.Current;
					if (kit.teamid == num && kit.kittype == num2)
					{
						return kit;
					}
					string text = kit.ToString();
					text = text.Substring(0, text.IndexOf('('));
					if (text == name)
					{
						return kit;
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
	}
}
