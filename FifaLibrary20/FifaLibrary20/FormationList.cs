using System;
using System.Collections;

namespace FifaLibrary
{
	public class FormationList : IdArrayList
	{
		public FormationList()
			: base(typeof(Formation))
		{
		}

		public FormationList(Type type)
			: base(type)
		{
		}

		public FormationList(DbFile fifaDbFile)
			: base(typeof(Formation))
		{
			Load(fifaDbFile);
		}

		public FormationList(Table formationsTable, int minId, int maxId)
			: base(typeof(Formation))
		{
			Load(formationsTable, minId, maxId);
		}

		public void Load(DbFile fifaDbFile)
		{
			Table table = FifaEnvironment.FifaDb.Table[TI.formations];
			int maxId = table.TableDescriptor.MaxValues[FI.formations_formationid];
			int minId = 1;
			Load(table, minId, maxId);
		}

		private void Load(Table formationsTable, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			Clear();
			for (int i = 0; i < formationsTable.NRecords; i++)
			{
				Formation value = new Formation(formationsTable.Records[i]);
				Add(value);
			}
			SortId();
		}

		public void Save(DbFile fifaDbFile)
		{
			Table formationsTable = FifaEnvironment.FifaDb.Table[TI.formations];
			Save(formationsTable);
		}

		public void Save(Table formationsTable)
		{
			formationsTable.ResizeRecords(Count);
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Formation obj = (Formation)enumerator.Current;
					Record r = formationsTable.Records[num];
					num++;
					obj.Save(r);
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
			for (int i = 0; i < Count; i++)
			{
				Formation formation = (Formation)this[i];
				if (!formation.LinkTeam(teamList))
				{
					RemoveId(formation);
					i--;
				}
			}
		}

		public Formation SearchByTeamId(int teamId)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Formation formation = (Formation)enumerator.Current;
					if (formation.teamid == teamId)
					{
						return formation;
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

		public Formation GetNearestFormation(Formation formation)
		{
			int num = 384;
			Formation result = null;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Formation formation2 = (Formation)enumerator.Current;
					if (formation2 != formation)
					{
						int num2 = formation.ComputeDistance(formation2);
						if (num2 < num)
						{
							num = num2;
							result = formation2;
						}
					}
				}
				return result;
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

		public Formation GetClosestFormation(Formation formation)
		{
			int num = 384;
			Formation result = null;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Formation formation2 = (Formation)enumerator.Current;
					if (formation2 != formation)
					{
						int num2 = formation.ComputeSimilarity(formation2);
						if (num2 < num)
						{
							num = num2;
							result = formation2;
						}
					}
				}
				return result;
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

		public Formation GetExactFormation(Formation formation)
		{
			int num = 384;
			Formation result = null;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Formation formation2 = (Formation)enumerator.Current;
					if (formation2 != formation)
					{
						int num2 = formation.ComputeDistance(formation2);
						if (num2 < num)
						{
							num = num2;
							result = formation2;
						}
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
			if (num != 0)
			{
				return null;
			}
			return result;
		}

		public Formation FitFormationByTeamId(int teamid)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Formation formation = (Formation)enumerator.Current;
					if (formation.teamid == teamid)
					{
						return formation;
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

		public Formation FitFormation(string name, int id)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Formation formation = (Formation)enumerator.Current;
					if (formation.ToString() == name)
					{
						return formation;
					}
					if (formation.Team != null)
					{
						string text = formation.Team.DatabaseName + " ";
						if (name.StartsWith(text))
						{
							bool flag = true;
							for (int i = text.Length; i < name.Length; i++)
							{
								if (name[i] != '-' && name[i] != '(' && name[i] != ')' && name[i] != 'S' && name[i] != 'W' && (name[i] < '0' || name[i] > '9'))
								{
									flag = false;
									break;
								}
							}
							if (flag)
							{
								return formation;
							}
						}
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

		public void LinkRoles(RoleList roleList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Formation)enumerator.Current).LinkRoles(roleList);
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

		public Formation CreateNewFormation(int newId)
		{
			if (SearchId(newId) != null)
			{
				return null;
			}
			Formation formation = (Formation)CreateNewId(newId);
			InsertId(formation);
			return formation;
		}

		public Formation CreateNewFormation()
		{
			int newId = GetNewId();
			return (Formation)CreateNewId(newId);
		}

		public override IdArrayList Filter(IdObject filter)
		{
			if (filter == null)
			{
				return this;
			}
			FormationList formationList = new FormationList();
			if (filter.GetType().Name == "League")
			{
				League league = (League)filter;
				for (int i = 0; i < Count; i++)
				{
					Formation formation = (Formation)this[i];
					if (formation.Team != null && formation.Team.League == league)
					{
						formationList.Add(formation);
					}
				}
				return formationList;
			}
			if (filter.GetType().Name == "Country")
			{
				Country country = (Country)filter;
				for (int j = 0; j < Count; j++)
				{
					Formation formation2 = (Formation)this[j];
					if (formation2.Team != null && formation2.Team.Country == country)
					{
						formationList.Add(formation2);
					}
				}
				return formationList;
			}
			if (filter.GetType().Name == "Team" || filter.GetType().Name == "CareerTeam")
			{
				Team team = (Team)filter;
				for (int k = 0; k < Count; k++)
				{
					Formation formation3 = (Formation)this[k];
					if (formation3.Team != null && formation3.Team == team)
					{
						formationList.Add(formation3);
					}
				}
				return formationList;
			}
			return this;
		}

		public bool DeleteFormation(Formation formation)
		{
			return RemoveId(formation);
		}
	}
}
