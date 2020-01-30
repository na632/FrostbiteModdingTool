using System;
using System.Collections;

namespace FifaLibrary
{
	public class TeamList : IdArrayList
	{
		private static char[] s_Separators = new char[3]
		{
			' ',
			'.',
			'-'
		};

		public TeamList(Type teamType)
			: base(teamType)
		{
		}

		public TeamList()
			: base(typeof(Team))
		{
		}

		public TeamList(DbFile fifaDbFile)
			: base(typeof(Team))
		{
			Load(fifaDbFile);
		}

		public void Load(DbFile fifaDbFile)
		{
			int minId = 130020;
			int maxId = 200000;
			Table t = fifaDbFile.Table[TI.teams];
			Load(t, minId, maxId);
			t = fifaDbFile.Table[TI.stadiumassignments];
			FillFromStadiumAssignments(t);
			t = fifaDbFile.Table[TI.manager];
			FillFromManager(t);
			t = fifaDbFile.Table[TI.teamstadiumlinks];
			FillFromTeamStadiumLinks(t);
			t = fifaDbFile.Table[TI.teamkits];
			FillFromTeamkits(t);
			t = fifaDbFile.Table[TI.career_newspicweights];
			FillFromNewspicweights(t);
			t = fifaDbFile.Table[TI.teamformationteamstylelinks];
			FillFromTeamFormationLinks(t);
			t = fifaDbFile.Table[TI.formations];
			FillFromFormations(t);
			t = fifaDbFile.Table[TI.leagueteamlinks];
			FillFromLeagueTeamLinks(t);
			t = fifaDbFile.Table[TI.rowteamnationlinks];
			FillFromRowTeamNationLinks(t);
			if (TI.teamnationlinks >= 0)
			{
				t = fifaDbFile.Table[TI.teamnationlinks];
				FillFromTeamNationLinks(t);
			}
		}

		private void Load(Table t, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			Team[] array = new Team[t.NRecords];
			Clear();
			for (int i = 0; i < t.NRecords; i++)
			{
				array[i] = new Team(t.Records[i]);
			}
			AddRange(array);
			SortId();
		}

		public void FillFromStadiumAssignments(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.stadiumassignments_teamid];
				((Team)SearchId(id))?.FillFromStadiumAssignments(record);
			}
		}

		public void FillFromManager(Table t)
		{
			for (int i = 0; i < t.NValidRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.manager_teamid];
				((Team)SearchId(id))?.FillFromManager(record);
			}
		}

		public void FillFromTeamStadiumLinks(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.teamstadiumlinks_teamid];
				((Team)SearchId(id))?.FillFromTeamStadiumLinks(record);
			}
		}

		public void FillFromTeamkits(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.teamkits_teamtechid];
				((Team)SearchId(id))?.FillFromTeamkits(record);
			}
		}

		public void FillFromNewspicweights(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record record = t.Records[i];
				int num = record.IntField[FI.career_newspicweights_teamid];
				if (num != 0)
				{
					((Team)SearchId(num))?.FillFromNewspicweights(record);
				}
			}
		}

		public void FillFromFormations(Table t)
		{
			int fieldIndex = t.TableDescriptor.GetFieldIndex("teamid");
			for (int i = 0; i < t.NValidRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[fieldIndex];
				((Team)SearchId(id))?.FillFromFormations(record);
			}
		}

		public void FillFromTeamFormationLinks(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.teamformationteamstylelinks_teamid];
				((Team)SearchId(id))?.FillFromTeamFormationLinks(record);
			}
		}

		public void FillFromTeamNationLinks(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.teamnationlinks_teamid];
				((Team)SearchId(id))?.FillFromTeamNationLinks(record);
			}
		}

		public void FillFromLeagueTeamLinks(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.leagueteamlinks_teamid];
				((Team)SearchId(id))?.FillFromLeagueTeamLinks(record);
			}
		}

		public void FillFromRivals(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record obj = t.Records[i];
				int id = obj.IntField[FI.rivals_teamid1];
				Team team = (Team)SearchId(id);
				int id2 = obj.IntField[FI.rivals_teamid2];
				Team team2 = (Team)SearchId(id2);
				if (team != null)
				{
				}
			}
		}

		public void FillFromRowTeamNationLinks(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.rowteamnationlinks_teamid];
				((Team)SearchId(id))?.FillFromRowTeamNationLinks(record);
			}
		}

		public void FillFromTeamPlayerLinks(DbFile fifaDbFile)
		{
			Table t = fifaDbFile.Table[TI.teamplayerlinks];
			FillFromTeamPlayerLinks(t);
		}

		public void FillFromTeamPlayerLinks(Table t)
		{
			if (FifaEnvironment.Players == null)
			{
				return;
			}
			for (int i = 0; i < t.NValidRecords; i++)
			{
				Record record = t.Records[i];
				int num = record.IntField[FI.teamplayerlinks_teamid];
				Team team = (Team)SearchId(num);
				if (team == null)
				{
					FifaEnvironment.UserMessages.ShowMessage(5016, num);
					continue;
				}
				int num2 = record.IntField[FI.teamplayerlinks_playerid];
				Player player = (Player)FifaEnvironment.Players.SearchId(num2);
				if (player == null)
				{
					FifaEnvironment.UserMessages.ShowMessage(5017, num2);
					continue;
				}
				player.PlayFor(team);
				TeamPlayer value = new TeamPlayer(record, player, team);
				team.Roster.Add(value);
			}
		}

		public void LinkKits(KitList kitList)
		{
			if (kitList != null)
			{
				{
					IEnumerator enumerator = GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							((Team)enumerator.Current).LinkKits(kitList);
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

		public void LinkStadiums(StadiumList stadiumList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Team)enumerator.Current).LinkStadium(stadiumList);
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
					((Team)enumerator.Current).LinkLeague(leagueList);
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

		public void LinkOpponent(TeamList teamList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Team)enumerator.Current).LinkTeam(teamList);
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
					((Team)enumerator.Current).LinkCountry(countryList);
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

		public void LinkFormation(FormationList formationList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Team)enumerator.Current).LinkFormation(formationList);
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

		public void LinkPlayer(PlayerList playerList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Team)enumerator.Current).LinkPlayer(playerList);
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
			Table table = fifaDbFile.Table[TI.teams];
			Table table2 = fifaDbFile.Table[TI.teamstadiumlinks];
			Table table3 = fifaDbFile.Table[TI.teamnationlinks];
			Table table4 = fifaDbFile.Table[TI.stadiumassignments];
			Table table5 = fifaDbFile.Table[TI.manager];
			Table table6 = fifaDbFile.Table[TI.rowteamnationlinks];
			Table table7 = fifaDbFile.Table[TI.teamplayerlinks];
			Table table8 = fifaDbFile.Table[TI.teamformationteamstylelinks];
			Table table9 = fifaDbFile.Table[TI.career_newspicweights];
			Table table10 = null;
			if (TI.defaultteamdata >= 0)
			{
				table10 = fifaDbFile.Table[TI.defaultteamdata];
			}
			Table table11 = null;
			if (TI.default_teamsheets >= 0)
			{
				table11 = fifaDbFile.Table[TI.default_teamsheets];
			}
			table.ResizeRecords(Count);
			table10?.ResizeRecords(Count);
			table11?.ResizeRecords(Count);
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			int num9 = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Team team = (Team)enumerator.Current;
					Record r = table.Records[num9];
					Record r2 = null;
					if (table10 != null)
					{
						r2 = table10.Records[num9];
					}
					Record r3 = null;
					if (table11 != null)
					{
						r3 = table11.Records[num9];
					}
					num9++;
					team.SaveTeam(r);
					team.Roster.SortRoster();
					team.SaveDefaultTeamData(r2);
					team.SaveDefaultTeamsheets(r3);
					team.SaveLangTable();
					num += team.Roster.Count;
					if (team.stadiumcustomname != null)
					{
						num2++;
					}
					if (team.stadiumid >= 0)
					{
						num3++;
					}
					if (team.ManagerSurname != null)
					{
						num7++;
					}
					if (team.Formation != null && team.Formation.IsGeneric())
					{
						num6++;
					}
					if (team.NationalTeam)
					{
						num5++;
					}
					else if (team.IsRowTeam())
					{
						num4++;
					}
					if (team.HasNewsPictures())
					{
						num8++;
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
			table7.ResizeRecords(num);
			table4.ResizeRecords(num2);
			table6.ResizeRecords(num4);
			table3.ResizeRecords(num5);
			table8.ResizeRecords(num6);
			table5.ResizeRecords(num7);
			table9.ResizeRecords(num8 + 1);
			table2.ResizeRecords(num3);
			int num10 = 0;
			int num11 = 0;
			int num12 = 0;
			int num13 = 0;
			int num14 = 0;
			int num15 = 0;
			int num16 = 0;
			int num17 = 1;
			Team.SaveDefaultNewspicweights(table9.Records[0]);
			int num18 = 0;
			enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Team team2 = (Team)enumerator.Current;
					foreach (TeamPlayer item in team2.Roster)
					{
						Record r4 = table7.Records[num10];
						num10++;
						item.Save(r4, num18);
						num18++;
					}
					if (team2.NationalTeam)
					{
						Record r5 = table3.Records[num11];
						num11++;
						team2.SaveTeamNationLinks(r5);
					}
					else if (team2.IsRowTeam())
					{
						Record r6 = table6.Records[num13];
						num13++;
						team2.SaveRowTeamNationLinks(r6);
					}
					if (team2.stadiumcustomname != null)
					{
						Record r7 = table4.Records[num12];
						num12++;
						team2.SaveStadiumAssignment(r7);
					}
					if (team2.Formation != null && team2.Formation.IsGeneric())
					{
						Record r8 = table8.Records[num14];
						num14++;
						team2.SaveTeamFormationLinks(r8);
					}
					if (team2.ManagerSurname != null)
					{
						Record r9 = table5.Records[num15];
						num15++;
						team2.SaveManager(r9);
					}
					if (team2.HasNewsPictures())
					{
						Record r10 = table9.Records[num17];
						num17++;
						team2.SaveNewspicweights(r10);
					}
					if (team2.stadiumid >= 0)
					{
						Record r11 = table2.Records[num16];
						team2.SaveTeamStadiumLinks(r11);
						num16++;
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

		public override IdArrayList Filter(IdObject filter)
		{
			TeamList teamList = new TeamList();
			if (filter == null)
			{
				return this;
			}
			if (filter.GetType().Name == "League")
			{
				League league = (League)filter;
				for (int i = 0; i < Count; i++)
				{
					Team team = (Team)this[i];
					if (team.League == league)
					{
						teamList.Add(team);
					}
				}
				return teamList;
			}
			if (filter.GetType().Name == "Country")
			{
				Country country = (Country)filter;
				for (int j = 0; j < Count; j++)
				{
					Team team2 = (Team)this[j];
					if (team2.Country == country)
					{
						teamList.Add(team2);
					}
				}
				return teamList;
			}
			return this;
		}

		public void DeleteTeam(Team team)
		{
			RemoveId(team);
		}

		public Team SearchTeamByCountr(Country country, bool club)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Team team = (Team)enumerator.Current;
					if (team.Country == country)
					{
						if (club && team.IsClub())
						{
							return team;
						}
						if (!club && team.IsNationalTeam())
						{
							return team;
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

		public Team FitTeam(string name, int id)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Team team = (Team)enumerator.Current;
					if (team.DatabaseName == name)
					{
						return team;
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

		public Team IsInTopLeague()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Team team = (Team)enumerator.Current;
					if (team.IsInTopLeague())
					{
						return team;
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

		public Team MatchByname(string matchName)
		{
			string[] array = matchName.Split(s_Separators);
			int num = 0;
			int num2 = 0;
			Team result = null;
			if (matchName == "Inter Milan")
			{
				matchName = "Inter";
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Team team = (Team)enumerator.Current;
					string[] array2 = team.DatabaseName.Split(s_Separators);
					num2 = 0;
					for (int i = 0; i < array.Length; i++)
					{
						for (int j = 0; j < array2.Length; j++)
						{
							if (array2[j].Length > 2 && array[i].Length > 2 && array2[j] == array[i])
							{
								num2++;
							}
						}
					}
					if (num2 > num)
					{
						result = team;
						num = num2;
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
	}
}
