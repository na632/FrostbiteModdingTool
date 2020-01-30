using System;
using System.Collections;
using System.Text;

namespace FifaLibrary
{
	public class PlayerList : IdArrayList
	{
		public class FreeAgent : IdObject
		{
			public override string ToString()
			{
				return "Free Agents";
			}
		}

		public class FreeAgentList : IdArrayList
		{
			public FreeAgentList()
				: base(typeof(FreeAgent))
			{
				Clear();
				Add(new FreeAgent());
			}
		}

		public class MultiClub : IdObject
		{
			public override string ToString()
			{
				return "Multi Clubs";
			}
		}

		public class MultiClubList : IdArrayList
		{
			public MultiClubList()
				: base(typeof(MultiClub))
			{
				Clear();
				Add(new MultiClub());
			}
		}

		public class SpecificHead : IdObject
		{
			public override string ToString()
			{
				return "Specific Head";
			}
		}

		public class SpecificHeadList : IdArrayList
		{
			public SpecificHeadList()
				: base(typeof(SpecificHead))
			{
				Clear();
				Add(new SpecificHead());
			}
		}

		public PlayerList(Type type)
			: base(type)
		{
		}

		public PlayerList(DbFile fifaDbFile)
			: base(typeof(Player))
		{
			Load(fifaDbFile);
		}

		public PlayerList()
			: base(typeof(Player))
		{
		}

		public void Load(DbFile fifaDbFile)
		{
			Table table = FifaEnvironment.FifaDb.Table[TI.players];
			int minId = 50000;
			int maxId = table.TableDescriptor.MaxValues[FI.players_playerid];
			Load(table, minId, maxId);
			table = fifaDbFile.Table[TI.playerloans];
			FillFromPlayerloans(table);
			table = fifaDbFile.Table[TI.previousteam];
			FillFromPreviousTeam(table);
		}

		public void FillFromPreviousTeam(Table t)
		{
			for (int i = 0; i < t.NValidRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.previousteam_playerid];
				((Player)SearchId(id))?.FillFromPreviousTeam(record);
			}
		}

		public void FillFromPlayerloans(Table t)
		{
			for (int i = 0; i < t.NValidRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.playerloans_playerid];
				((Player)SearchId(id))?.FillFromPlayerloans(record);
			}
		}

		public void FillFromEditedPlayerNames(Table t)
		{
			for (int i = 0; i < t.NRecords; i++)
			{
				Record record = t.Records[i];
				int id = record.IntField[FI.editedplayernames_playerid];
				((Player)SearchId(id))?.FillFromEditedPlayerNames(record);
			}
		}

		public void Load(Table table, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			Player[] array = new Player[table.NRecords];
			Clear();
			for (int i = 0; i < table.NRecords; i++)
			{
				array[i] = new Player(table.Records[i]);
			}
			AddRange(array);
			SortId();
		}

		public Player MatchPlayerByNameBirthday(ref string firstName, ref string lastName, ref string commonName, DateTime birthdate)
		{
			Player player = null;
			int num = 0;
			int num2 = 0;
			bool flag = false;
			bool flag2 = false;
			do
			{
				num = 0;
				num2 = 0;
				if (firstName != null)
				{
					num2 = firstName.Length;
				}
				if (lastName != null)
				{
					num2 += lastName.Length;
				}
				if (commonName != null)
				{
					num2 += commonName.Length;
				}
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Player player2 = (Player)enumerator.Current;
						if (!(birthdate.Date != player2.birthdate.Date))
						{
							int num3 = 0;
							if (player2.commonname != string.Empty && player2.commonname == firstName + " " + lastName)
							{
								flag = true;
								commonName = player2.commonname;
							}
							if (!flag && commonName != null && commonName != string.Empty && player2.commonname != string.Empty)
							{
								if (commonName == player2.commonname)
								{
									flag = true;
								}
								int num4 = FuzzyMatchString(commonName, player2.commonname);
								num3 += num4;
							}
							if (!flag && firstName != null && firstName != string.Empty)
							{
								int num4 = FuzzyMatchString(firstName, player2.firstname);
								num3 += num4;
								if (num4 == 0)
								{
									continue;
								}
							}
							if (!flag && lastName != null && lastName != string.Empty)
							{
								int num4 = FuzzyMatchString(lastName, player2.lastname);
								num3 += num4;
								if (num4 == 0)
								{
									continue;
								}
							}
							if (flag || num3 == num2)
							{
								return player2;
							}
							if (num3 > num)
							{
								num = num3;
								player = player2;
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
				if (player != null)
				{
					return player;
				}
				int num5 = firstName.IndexOf(' ');
				if (num5 > 0)
				{
					lastName = firstName.Substring(num5 + 1) + " " + lastName;
					firstName = firstName.Substring(0, num5);
					flag2 = true;
				}
				else
				{
					flag2 = false;
				}
			}
			while (flag2);
			return player;
		}

		public Player FindSimilarPlayer(Country country, DateTime birthdate)
		{
			Player result = null;
			int num = 100000;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Player player = (Player)enumerator.Current;
					if (country == player.Country)
					{
						int num2 = Math.Abs(birthdate.Year - player.birthdate.Year) * 365;
						num2 += Math.Abs(birthdate.Month - player.birthdate.Month) * 30;
						num2 += Math.Abs(birthdate.Day - player.birthdate.Day);
						if (num2 != 0 && num2 < num)
						{
							num = num2;
							result = player;
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

		private int FuzzyMatchString(string pattern, string actual)
		{
			if (pattern == actual)
			{
				return pattern.Length;
			}
			if (actual.StartsWith(pattern + " "))
			{
				return pattern.Length - 1;
			}
			if (actual.EndsWith(" " + pattern))
			{
				return pattern.Length - 1;
			}
			if (pattern.StartsWith(actual + " "))
			{
				return actual.Length;
			}
			if (pattern.EndsWith(" " + actual))
			{
				return actual.Length;
			}
			if (Math.Abs(pattern.Length - actual.Length) > 1)
			{
				return 0;
			}
			char[] array = pattern.ToCharArray();
			char[] array2 = new char[array.Length + 3];
			char[] array3 = actual.ToCharArray();
			for (int i = 0; i < array3.Length; i++)
			{
				array2[i] = array3[i];
			}
			int length = pattern.Length;
			int num = 0;
			int num2 = 0;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] == array2[j + num2])
				{
					continue;
				}
				if (j + 1 < array.Length && j + num2 + 1 < array2.Length && array[j + 1] == array2[j + num2 + 1])
				{
					num++;
				}
				else if (j + num2 + 1 < array2.Length && array[j] == array2[j + num2 + 1])
				{
					num++;
					num2++;
				}
				else if (j + 1 < array.Length && j + num2 < array2.Length && array[j + 1] == array2[j + num2])
				{
					num++;
					num2--;
				}
				else
				{
					if (j != array.Length - 1)
					{
						return 0;
					}
					num++;
				}
				if (num > 3)
				{
					return 0;
				}
			}
			return length - num;
		}

		public Player FitPlayer(Player player)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Player player2 = (Player)enumerator.Current;
					if (player2.lastname == player.lastname)
					{
						if (player.firstname != null && player.firstname != "")
						{
							if (player2.firstname == player.firstname)
							{
								return player2;
							}
						}
						else if (player2.Id == player.Id)
						{
							return player2;
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

		public Player FitPlayer(string name, int id)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Player player = (Player)enumerator.Current;
					if (player.ToString() == name)
					{
						return player;
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

		public Player FitPlayer(string firstname, string lastname, DateTime birthdate)
		{
			firstname = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(firstname));
			lastname = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(lastname));
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Player player = (Player)enumerator.Current;
					if (birthdate.Date == player.birthdate.Date)
					{
						string @string = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(player.firstname));
						string string2 = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(player.lastname));
						if (firstname == @string && lastname == string2)
						{
							return player;
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

		public PlayerList GetFreeAgent()
		{
			PlayerList playerList = new PlayerList();
			for (int i = 0; i < Count; i++)
			{
				Player player = (Player)this[i];
				if (player.IsFreeAgent())
				{
					playerList.Add(player);
				}
			}
			return playerList;
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.players];
			table.ResizeRecords(Count);
			Table table2 = fifaDbFile.Table[TI.playerloans];
			Table table3 = fifaDbFile.Table[TI.previousteam];
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Player player = (Player)enumerator.Current;
					Record r = table.Records[num];
					if (player.m_assetid != 0)
					{
						num++;
						player.SavePlayer(r);
						if (player.IsLoaned)
						{
							num3++;
						}
						if (player.PreviousTeam != null)
						{
							num4++;
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
			table2.ResizeRecords(num3);
			table3.ResizeRecords(num4);
			num = 0;
			num2 = 0;
			enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Player player2 = (Player)enumerator.Current;
					if (player2.IsLoaned)
					{
						Record r2 = table2.Records[num];
						num++;
						player2.SavePlayerloans(r2);
					}
					if (player2.PreviousTeam != null)
					{
						Record r3 = table3.Records[num2];
						num2++;
						player2.SavePreviousTeam(r3);
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

		public ArrayList FindMissedFiles()
		{
			ArrayList arrayList = new ArrayList();
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Player player = (Player)enumerator.Current;
					if (player.HasSpecificHeadModel)
					{
						if (!FifaEnvironment.FifaFat.IsArchivedFilePresent(player.SpecificFaceTextureFileName()))
						{
							arrayList.Add("Face #" + player.Id.ToString() + " used by player " + player.ToString() + "\r\n");
						}
						if (!FifaEnvironment.FifaFat.IsArchivedFilePresent(player.SpecificHairTexturesFileName()))
						{
							arrayList.Add("Hair Textures #" + player.Id.ToString() + " used by player " + player.ToString() + "\r\n");
						}
					}
				}
				return arrayList;
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
			PlayerList playerList = new PlayerList();
			if (filter.GetType().Name == "SameName")
			{
				for (int i = 0; i < Count; i++)
				{
					Player player = (Player)this[i];
					for (int j = i + 1; j < Count; j++)
					{
						Player player2 = (Player)this[j];
						if (player.firstnameid == player2.firstnameid && player.lastnameid == player2.lastnameid && player.birthdate == player2.birthdate)
						{
							playerList.Add(player);
							playerList.Add(player2);
						}
					}
				}
				return playerList;
			}
			if (filter.GetType().Name == "FreeAgent")
			{
				for (int k = 0; k < Count; k++)
				{
					Player player3 = (Player)this[k];
					if (player3.IsFreeAgent())
					{
						playerList.Add(player3);
					}
				}
				return playerList;
			}
			if (filter.GetType().Name == "Team" || filter.GetType().Name == "CareerTeam")
			{
				Team team = (Team)filter;
				if (team != null)
				{
					for (int l = 0; l < Count; l++)
					{
						Player player4 = (Player)this[l];
						if (player4.IsPlayingFor(team))
						{
							playerList.Add(player4);
						}
					}
				}
				return playerList;
			}
			if (filter.GetType().Name == "MultiClub")
			{
				for (int m = 0; m < Count; m++)
				{
					Player player5 = (Player)this[m];
					if (player5.IsMultiClub())
					{
						playerList.Add(player5);
					}
				}
				return playerList;
			}
			if (filter.GetType().Name == "Country")
			{
				Country country = (Country)filter;
				for (int n = 0; n < Count; n++)
				{
					Player player6 = (Player)this[n];
					if (player6.Country == country)
					{
						playerList.Add(player6);
					}
				}
				return playerList;
			}
			if (filter.GetType().Name == "Role")
			{
				Role role = (Role)filter;
				for (int num = 0; num < Count; num++)
				{
					Player player7 = (Player)this[num];
					if (player7.preferredposition1 == role.Id)
					{
						playerList.Add(player7);
					}
				}
				return playerList;
			}
			if (filter.GetType().Name == "SpecificHead")
			{
				for (int num2 = 0; num2 < Count; num2++)
				{
					Player player8 = (Player)this[num2];
					if (player8.HasSpecificHeadModel)
					{
						playerList.Add(player8);
					}
				}
				return playerList;
			}
			return this;
		}

		public override IdArrayList Filter(IdObject filter, bool excludeYoung)
		{
			PlayerList obj = (PlayerList)Filter(filter);
			PlayerList playerList = new PlayerList();
			foreach (Player item in obj)
			{
				if (item.Id < 400000 || !excludeYoung)
				{
					playerList.Add(item);
				}
			}
			return playerList;
		}

		public bool DeletePlayer(Player player)
		{
			return RemoveId(player);
		}

		public void LinkCountry(CountryList countryList)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Player)enumerator.Current).LinkCountry(countryList);
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
					((Player)enumerator.Current).LinkTeam(teamList);
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

		public int SetGenericAudio(string lastname, int commentaryid)
		{
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Player player = (Player)enumerator.Current;
					if (player.audioname == lastname)
					{
						player.commentaryid = commentaryid;
						num++;
					}
				}
				return num;
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
