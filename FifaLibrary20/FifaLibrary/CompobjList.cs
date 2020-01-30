using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary
{
	public class CompobjList : IdArrayList
	{
		private static string[] s_FileNames = new string[10];

		public static ArrayList s_Descriptions = new ArrayList();

		public TrophyList Trophies
		{
			get
			{
				TrophyList trophyList = new TrophyList();
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Compobj compobj = (Compobj)enumerator.Current;
						if (compobj.IsTrophy())
						{
							trophyList.Add(compobj);
						}
					}
					return trophyList;
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

		public World World
		{
			get
			{
				IEnumerator enumerator = GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Compobj compobj = (Compobj)enumerator.Current;
						if (compobj.IsWorld())
						{
							return (World)compobj;
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

		public CompobjList()
			: base(typeof(Compobj))
		{
			base.MinId = 0;
			base.MaxId = 99999;
		}

		public CompobjList(string path, DbFile dbFile)
			: base(typeof(Compobj))
		{
			base.MinId = 0;
			base.MaxId = 99999;
			Load(path, dbFile);
		}

		public static string[] GetFileNames()
		{
			s_FileNames[0] = "dlc/dlc_footballcompeng/dlc/footballcompeng/data/compdata/compobj.txt";
			s_FileNames[1] = "dlc/dlc_footballcompeng/dlc/footballcompeng/data/compdata/settings.txt";
			s_FileNames[2] = "dlc/dlc_footballcompeng/dlc/footballcompeng/data/compdata/standings.txt";
			s_FileNames[3] = "dlc/dlc_footballcompeng/dlc/footballcompeng/data/compdata/advancement.txt";
			s_FileNames[4] = "dlc/dlc_footballcompeng/dlc/footballcompeng/data/compdata/schedule.txt";
			s_FileNames[5] = "dlc/dlc_footballcompeng/dlc/footballcompeng/data/compdata/weather.txt";
			s_FileNames[6] = "dlc/dlc_footballcompeng/dlc/footballcompeng/data/compdata/tasks.txt";
			s_FileNames[7] = "dlc/dlc_footballcompeng/dlc/footballcompeng/data/compdata/initteams.txt";
			s_FileNames[8] = "dlc/dlc_footballcompeng/dlc/footballcompeng/data/compdata/compids.txt";
			s_FileNames[9] = "dlc/dlc_footballcompeng/dlc/footballcompeng/data/internationals.txt";
			return s_FileNames;
		}

		public void Load(string path, DbFile fifaDbFile)
		{
			Clear();
			_ = FifaEnvironment.LaunchDir + "\\Templates\\";
			bool flag = false;
			for (int i = 0; i < s_FileNames.Length; i++)
			{
				if (!File.Exists(path + s_FileNames[0]))
				{
					flag = true;
				}
			}
			if (flag)
			{
				FifaEnvironment.ExtractCompetitionFiles();
			}
			LoadFromCompobj(path + s_FileNames[0]);
			LoadFromSettings(path + s_FileNames[1]);
			LoadFromStandings(path + s_FileNames[2]);
			LoadFromAdvancement(path + s_FileNames[3]);
			LoadFromSchedule(path + s_FileNames[4]);
			LoadFromWeather(path + s_FileNames[5]);
			LoadFromTasks(path + s_FileNames[6]);
			LoadFromInitteams(path + s_FileNames[7]);
			LoadFromInternationals(path + s_FileNames[9]);
			FillFromLanguage();
			Table t = fifaDbFile.Table[TI.competition];
			FillFromCompetition(t);
			Normalize();
			CollectDescriptions();
		}

		public bool LoadFromCompobj(string fileName)
		{
			return LoadFromCompobj(fileName, null);
		}

		public bool LoadFromCompobj(string fileName, Compobj parentObject)
		{
			bool flag = true;
			bool flag2 = false;
			Compobj compobj = null;
			if (!File.Exists(fileName))
			{
				return false;
			}
			StreamReader streamReader = new StreamReader(fileName);
			if (streamReader == null)
			{
				return false;
			}
			string text = null;
			char[] separator = new char[1]
			{
				','
			};
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split(separator);
				if (array.Length == 5)
				{
					int id = Convert.ToInt32(array[0]);
					int num = Convert.ToInt32(array[1]);
					string typeString = array[2];
					string description = array[3];
					Convert.ToInt32(array[4]);
					Compobj compobj2 = null;
					switch (num)
					{
					case 0:
					{
						World idObject = new World(id, typeString, description);
						InsertId(idObject);
						break;
					}
					case 1:
						compobj2 = new Confederation(id, typeString, description, null);
						InsertId(compobj2);
						break;
					case 2:
						compobj2 = new Nation(id, typeString, description, null);
						InsertId(compobj2);
						break;
					case 3:
						compobj2 = new Trophy(id, typeString, description, null);
						InsertId(compobj2);
						break;
					case 4:
						compobj2 = new Stage(id, typeString, description, null);
						InsertId(compobj2);
						break;
					case 5:
						compobj2 = new Group(id, typeString, description, null);
						InsertId(compobj2);
						break;
					}
				}
			}
			streamReader.BaseStream.Seek(0L, SeekOrigin.Begin);
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array2 = text.Split(separator);
				if (array2.Length != 5)
				{
					continue;
				}
				int id2 = Convert.ToInt32(array2[0]);
				Convert.ToInt32(array2[1]);
				_ = array2[2];
				_ = array2[3];
				int num2 = Convert.ToInt32(array2[4]);
				if (num2 == -1)
				{
					continue;
				}
				Compobj compobj3 = (Compobj)SearchId(id2);
				Compobj compobj4 = (Compobj)SearchId(num2);
				if (compobj4 == null)
				{
					compobj4 = parentObject;
					if (compobj3.IsConfederation())
					{
						if (!compobj4.IsWorld())
						{
							flag = false;
						}
						else
						{
							foreach (Confederation confederation in compobj4.Confederations)
							{
								if (compobj3.TypeString == confederation.TypeString)
								{
									compobj = confederation;
								}
							}
						}
					}
					else if (compobj3.IsNation())
					{
						if (!compobj4.IsConfederation())
						{
							flag = false;
						}
						else
						{
							foreach (Nation nation in compobj4.Nations)
							{
								if (compobj3.TypeString == nation.TypeString)
								{
									compobj = nation;
								}
							}
						}
					}
					else if (compobj3.IsTrophy())
					{
						foreach (Trophy trophy in compobj4.Trophies)
						{
							if (compobj3.TypeString == trophy.TypeString)
							{
								compobj = trophy;
							}
						}
					}
					else
					{
						flag = false;
					}
					if (!flag)
					{
						FifaEnvironment.UserMessages.ShowMessage(1038);
						flag2 = true;
					}
					else
					{
						DialogResult dialogResult = FifaEnvironment.UserMessages.ShowMessage(34, compobj3.TypeString, merge: true);
						if (dialogResult == DialogResult.Yes || dialogResult == DialogResult.OK)
						{
							flag2 = false;
						}
					}
				}
				if (flag2)
				{
					break;
				}
				if (compobj3 != null && compobj4 != null)
				{
					if (compobj == null)
					{
						compobj3.ParentObj = compobj4;
						compobj4.AddChild(compobj3);
					}
					else if (compobj3.IsConfederation())
					{
						compobj4.Confederations.RemoveId(compobj);
						compobj3.ParentObj = compobj4;
						compobj4.AddChild(compobj3);
						compobj = null;
					}
					else if (compobj3.IsNation())
					{
						compobj4.Nations.RemoveId(compobj);
						compobj3.ParentObj = compobj4;
						compobj4.AddChild(compobj3);
						compobj = null;
					}
					else if (compobj3.IsTrophy())
					{
						compobj4.Trophies.RemoveId(compobj);
						compobj3.ParentObj = compobj4;
						compobj4.AddChild(compobj3);
						compobj = null;
					}
				}
				else
				{
					FifaEnvironment.UserMessages.ShowMessage(5064);
					flag = false;
				}
			}
			streamReader.Close();
			return flag;
		}

		public bool LoadFromSettings(string fileName)
		{
			StreamReader streamReader = new StreamReader(fileName);
			if (streamReader == null)
			{
				return false;
			}
			string text = null;
			char[] array = new char[1];
			char[] anyOf = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
			array[0] = ',';
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array2 = text.Split(array);
				if (array2.Length == 3)
				{
					int id = Convert.ToInt32(array2[0]);
					_ = array2[1];
					array2[2].IndexOfAny(anyOf);
					_ = 0;
					((Compobj)SearchId(id))?.SetProperty(array2[1], array2[2]);
				}
			}
			streamReader.Close();
			return true;
		}

		public bool LoadFromStandings(string fileName)
		{
			StreamReader streamReader = new StreamReader(fileName);
			if (streamReader == null)
			{
				return false;
			}
			string text = null;
			char[] separator = new char[1]
			{
				','
			};
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split(separator);
				if (array.Length != 2)
				{
					continue;
				}
				int id = Convert.ToInt32(array[0]);
				int num = Convert.ToInt32(array[1]);
				Compobj compobj = (Compobj)SearchId(id);
				if (compobj == null)
				{
					FifaEnvironment.UserMessages.ShowMessage(5064);
				}
				else if (compobj.IsGroup())
				{
					Group group = (Group)compobj;
					if (group != null)
					{
						Rank value = new Rank(group, num + 1);
						group.Ranks.Add(value);
					}
				}
			}
			streamReader.Close();
			return true;
		}

		public bool LoadFromSchedule(string fileName)
		{
			StreamReader streamReader = new StreamReader(fileName);
			if (streamReader == null)
			{
				return false;
			}
			string text = null;
			char[] separator = new char[1]
			{
				','
			};
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split(separator);
				if (array.Length != 6)
				{
					continue;
				}
				int id = Convert.ToInt32(array[0]);
				int day = Convert.ToInt32(array[1]);
				int leg = Convert.ToInt32(array[2]);
				int minGames = Convert.ToInt32(array[3]);
				int maxGames = Convert.ToInt32(array[4]);
				int time = Convert.ToInt32(array[5]);
				Compobj compobj = (Compobj)SearchId(id);
				if (compobj == null)
				{
					continue;
				}
				if (compobj.IsStage())
				{
					Stage stage = (Stage)SearchId(id);
					if (stage != null)
					{
						Schedule schedule = new Schedule(stage, day, leg, minGames, maxGames, time);
						stage.AddSchedule(schedule);
					}
				}
				else if (compobj.IsGroup())
				{
					Group group = (Group)SearchId(id);
					if (group != null)
					{
						Schedule schedule2 = new Schedule(group, day, leg, minGames, maxGames, time);
						group.AddSchedule(schedule2);
					}
				}
			}
			streamReader.Close();
			return true;
		}

		public bool LoadFromWeather(string fileName)
		{
			StreamReader streamReader = new StreamReader(fileName);
			if (streamReader == null)
			{
				return false;
			}
			string text = null;
			char[] separator = new char[1]
			{
				','
			};
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split(separator);
				if (array.Length != 13)
				{
					continue;
				}
				int id = Convert.ToInt32(array[0]);
				int num = Convert.ToInt32(array[1]);
				int num2 = Convert.ToInt32(array[2]);
				int num3 = Convert.ToInt32(array[3]);
				int num4 = Convert.ToInt32(array[4]);
				int num5 = Convert.ToInt32(array[9]);
				int num6 = Convert.ToInt32(array[10]);
				int num7 = Convert.ToInt32(array[5]);
				int num8 = Convert.ToInt32(array[6]);
				int num9 = Convert.ToInt32(array[7]);
				int num10 = Convert.ToInt32(array[8]);
				int num11 = Convert.ToInt32(array[11]);
				int num12 = Convert.ToInt32(array[12]);
				Compobj compobj = (Compobj)SearchId(id);
				if (compobj == null)
				{
					continue;
				}
				if (compobj.IsNation())
				{
					Nation nation = (Nation)SearchId(id);
					if (nation != null && num >= 1 && num <= 12)
					{
						num--;
						nation.ClearProb[num] = num2;
						nation.HazyProb[num] = num3;
						nation.CloudyProb[num] = num4;
						nation.RainProb[num] = num7;
						nation.ShowersProb[num] = num8;
						nation.SnowProb[num] = num9;
						nation.FlurriesProb[num] = num10;
						nation.OvercastProb[num] = num5;
						nation.FoggyProb[num] = num6;
						nation.SunsetTime[num] = num11;
						nation.DarkTime[num] = num12;
					}
				}
				else if (compobj.IsWorld())
				{
					World world = (World)SearchId(id);
					if (world != null && num >= 1 && num <= 12)
					{
						num--;
						world.ClearProb[num] = num2;
						world.HazyProb[num] = num3;
						world.CloudyProb[num] = num4;
						world.RainProb[num] = num7;
						world.ShowersProb[num] = num8;
						world.SnowProb[num] = num9;
						world.FlurriesProb[num] = num10;
						world.OvercastProb[num] = num5;
						world.FoggyProb[num] = num6;
						world.SunsetTime[num] = num11;
						world.DarkTime[num] = num12;
					}
				}
			}
			streamReader.Close();
			return true;
		}

		public bool LoadFromTasks(string fileName)
		{
			StreamReader streamReader = new StreamReader(fileName);
			if (streamReader == null)
			{
				return false;
			}
			string text = null;
			char[] separator = new char[1]
			{
				','
			};
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split(separator);
				if (array.Length != 7)
				{
					continue;
				}
				int id = Convert.ToInt32(array[0]);
				string when = array[1];
				string action = array[2];
				int num = Convert.ToInt32(array[3]);
				int num2 = Convert.ToInt32(array[4]);
				int par = Convert.ToInt32(array[5]);
				int par2 = Convert.ToInt32(array[6]);
				Compobj compobj = (Compobj)SearchId(id);
				if (compobj == null || !compobj.IsTrophy())
				{
					continue;
				}
				Trophy trophy = (Trophy)compobj;
				if (trophy == null)
				{
					continue;
				}
				Task task = new Task(when, action, num, num2, par, par2);
				compobj = (Compobj)SearchId(num);
				if (compobj.IsGroup())
				{
					((Group)compobj).AddTask(task);
				}
				else if (compobj.IsStage())
				{
					((Stage)compobj).AddTask(task);
				}
				else
				{
					if (!compobj.IsTrophy())
					{
						continue;
					}
					trophy = (Trophy)compobj;
					if (trophy != null)
					{
						compobj = (Compobj)SearchId(num2);
						if (compobj != null && compobj.IsGroup())
						{
							task.Group = (Group)compobj;
						}
						trophy.AddTask(task);
					}
				}
			}
			streamReader.Close();
			return true;
		}

		public bool LoadFromInitteams(string fileName)
		{
			StreamReader streamReader = new StreamReader(fileName);
			if (streamReader == null)
			{
				return false;
			}
			string text = null;
			char[] separator = new char[1]
			{
				','
			};
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split(separator);
				if (array.Length != 3)
				{
					continue;
				}
				int id = Convert.ToInt32(array[0]);
				int num = Convert.ToInt32(array[1]);
				int teamId = Convert.ToInt32(array[2]);
				Compobj compobj = (Compobj)SearchId(id);
				if (compobj != null && compobj.IsTrophy())
				{
					Trophy trophy = (Trophy)compobj;
					if (trophy != null)
					{
						InitTeam initTeam = new InitTeam(num, teamId);
						trophy.InitTeamArray[num] = initTeam;
					}
				}
			}
			streamReader.Close();
			return true;
		}

		public bool LoadFromAdvancement(string fileName)
		{
			StreamReader streamReader = new StreamReader(fileName);
			if (streamReader == null)
			{
				return false;
			}
			string text = null;
			char[] separator = new char[1]
			{
				','
			};
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split(separator);
				if (array.Length != 4)
				{
					continue;
				}
				int id = Convert.ToInt32(array[0]);
				int id2 = Convert.ToInt32(array[1]);
				int id3 = Convert.ToInt32(array[2]);
				int id4 = Convert.ToInt32(array[3]);
				IdObject idObject = SearchId(id);
				Group group = (idObject != null) ? ((Group)idObject) : null;
				idObject = SearchId(id3);
				Group group2 = (idObject != null) ? ((Group)idObject) : null;
				if (group == null || group2 == null)
				{
					continue;
				}
				Rank rank = (Rank)group.Ranks.SearchId(id2);
				Rank rank2 = (Rank)group2.Ranks.SearchId(id4);
				if (rank != null && rank2 != null)
				{
					if (rank.Id != 0)
					{
						rank.MoveTo = rank2;
					}
					rank2.MoveFrom = rank;
				}
			}
			streamReader.Close();
			return true;
		}

		public bool LoadFromInternationals(string fileName)
		{
			bool flag = false;
			bool flag2 = true;
			StreamReader streamReader = new StreamReader(fileName);
			if (streamReader == null)
			{
				return false;
			}
			string text = null;
			char[] separator = new char[3]
			{
				',',
				'[',
				']'
			};
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split(separator);
				if (array.Length == 3)
				{
					if (array[1] == "TIER_AND_OBJECTIVES")
					{
						flag = true;
						flag2 = true;
					}
					else if (array[1] == "LEAGUE_RANKINGS")
					{
						flag = true;
						flag2 = false;
					}
					else
					{
						flag = false;
					}
				}
				else
				{
					if (!flag)
					{
						continue;
					}
					if (flag2)
					{
						if (array.Length != 8)
						{
							continue;
						}
						int level = Convert.ToInt32(array[0]);
						int countryid = Convert.ToInt32(array[1]);
						Convert.ToInt32(array[2]);
						Convert.ToInt32(array[3]);
						string a = array[5];
						string a2 = array[7];
						Country country = FifaEnvironment.Countries.SearchCountry(countryid);
						if (country != null)
						{
							if (a == "N/A")
							{
								country.WorldCupTarget = 0;
							}
							else if (a == "WIN")
							{
								country.WorldCupTarget = 1;
							}
							else if (a == "FINAL")
							{
								country.WorldCupTarget = 2;
							}
							else if (a == "SEMI")
							{
								country.WorldCupTarget = 3;
							}
							else if (a == "QUARTER")
							{
								country.WorldCupTarget = 4;
							}
							else if (a == "KNOCKOUT")
							{
								country.WorldCupTarget = 5;
							}
							else if (a == "QUALIFY")
							{
								country.WorldCupTarget = 6;
							}
							if (a2 == "N/A")
							{
								country.ContinentalCupTarget = 0;
							}
							else if (a2 == "WIN")
							{
								country.ContinentalCupTarget = 1;
							}
							else if (a2 == "FINAL")
							{
								country.ContinentalCupTarget = 2;
							}
							else if (a2 == "SEMI")
							{
								country.ContinentalCupTarget = 3;
							}
							else if (a2 == "QUARTER")
							{
								country.ContinentalCupTarget = 4;
							}
							else if (a2 == "KNOCKOUT")
							{
								country.ContinentalCupTarget = 5;
							}
							else if (a2 == "QUALIFY")
							{
								country.ContinentalCupTarget = 6;
							}
							country.Level = level;
						}
					}
					else
					{
						if (array.Length != 6)
						{
							continue;
						}
						int leagueid = Convert.ToInt32(array[0]);
						Convert.ToInt32(array[1]);
						Convert.ToInt32(array[2]);
						Convert.ToInt32(array[3]);
						Convert.ToInt32(array[4]);
						Convert.ToInt32(array[5]);
						League league = FifaEnvironment.Leagues.SearchLeague(leagueid);
						if (league != null)
						{
							switch (text.Substring(array[0].Length + 1))
							{
							case "1,2,3,4,7":
								league.Prestige = ELeaguePrestige.England_Spain_Germany_Italy;
								break;
							case "2,3,4,6,7":
								league.Prestige = ELeaguePrestige.France;
								break;
							case "2,4,5,6,7":
								league.Prestige = ELeaguePrestige.Argentina_Brazil;
								break;
							case "3,5,6,7,7":
								league.Prestige = ELeaguePrestige.Russia_Portugal_Turkey;
								break;
							case "4,5,6,7,7":
								league.Prestige = ELeaguePrestige.Holland;
								break;
							case "4,6,7,7,7":
								league.Prestige = ELeaguePrestige.Mexico_England2;
								break;
							case "5,6,7,7,7":
								league.Prestige = ELeaguePrestige.Belgium_Germany2_Colombia;
								break;
							case "5,6,7,7,-1":
								league.Prestige = ELeaguePrestige.USA_Chile;
								break;
							case "5,6,7,-1,-1":
								league.Prestige = ELeaguePrestige.Scotland_Italy2_Spain2;
								break;
							case "6,7,-1,-1,-1":
								league.Prestige = ELeaguePrestige.France2_Denmark_Norway_Switzerland;
								break;
							case "7,7,-1,-1,-1":
								league.Prestige = ELeaguePrestige.Poland_Austria_Korea;
								break;
							case "7,-1,-1,-1,-1":
								league.Prestige = ELeaguePrestige.Australia_Sweden_England3_England4_Ireland;
								break;
							default:
								league.Prestige = ELeaguePrestige.Undefined;
								break;
							}
						}
					}
				}
			}
			streamReader.Close();
			return true;
		}

		private bool FillFromLanguage()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Compobj)enumerator.Current).FillFromLanguage();
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
			return true;
		}

		public bool FillFromCompetition(Table t)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Compobj compobj = (Compobj)enumerator.Current;
					if (compobj.IsTrophy())
					{
						((Trophy)compobj).FillFromCompetition(t);
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
			return true;
		}

		private bool SaveToLanguage()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Compobj)enumerator.Current).SaveToLanguage();
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
			return true;
		}

		private bool SaveToCompetition(Table t)
		{
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Compobj compobj = (Compobj)enumerator.Current;
					if (compobj.IsTrophy() && ((Trophy)compobj).ballid >= 0)
					{
						num++;
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
			t.ResizeRecords(num);
			num = 0;
			enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Compobj compobj2 = (Compobj)enumerator.Current;
					if (compobj2.IsTrophy())
					{
						Trophy trophy = (Trophy)compobj2;
						if (trophy.ballid >= 0)
						{
							Record r = t.Records[num];
							trophy.SaveCompetition(r);
							num++;
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
			t.SortByKeys();
			return true;
		}

		public string[] GetFceDescriptors()
		{
			int num = 0;
			string[] array = new string[256];
			if (FifaEnvironment.LangDb == null)
			{
				return null;
			}
			Table table = FifaEnvironment.LangDb.Table[0];
			if (table == null)
			{
				return null;
			}
			for (int i = 0; i < table.NRecords; i++)
			{
				string text = table.Records[i].CompressedString[FI.language_stringid];
				if (text.StartsWith("FCE_") && num < array.Length)
				{
					array[num++] = text;
				}
			}
			string[] array2 = new string[num];
			for (int j = 0; j < num; j++)
			{
				array2[j] = array[j];
			}
			return array2;
		}

		private void CollectDescriptions()
		{
			s_Descriptions.Clear();
			string[] fceDescriptors = GetFceDescriptors();
			s_Descriptions.AddRange(fceDescriptors);
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Compobj compobj = (Compobj)enumerator.Current;
					if (compobj.IsStage() && !compobj.Description.StartsWith("FCE_") && !s_Descriptions.Contains(compobj.Description))
					{
						s_Descriptions.Add(compobj.Description);
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
			s_Descriptions.Sort();
		}

		public void Save(string path, DbFile fifaDbFile)
		{
			Renumber();
			SaveToCompobj(path + "\\dlc\\dlc_FootballCompEng\\dlc\\FootballCompEng\\data\\compdata\\compobj.txt");
			SaveToCompids(path + "\\dlc\\dlc_FootballCompEng\\dlc\\FootballCompEng\\data\\compdata\\compids.txt");
			SaveToSettings(path + "\\dlc\\dlc_FootballCompEng\\dlc\\FootballCompEng\\data\\compdata\\settings.txt");
			SaveToStandings(path + "\\dlc\\dlc_FootballCompEng\\dlc\\FootballCompEng\\data\\compdata\\standings.txt");
			SaveToAdvancement(path + "\\dlc\\dlc_FootballCompEng\\dlc\\FootballCompEng\\data\\compdata\\advancement.txt");
			SaveToSchedule(path + "\\dlc\\dlc_FootballCompEng\\dlc\\FootballCompEng\\data\\compdata\\schedule.txt");
			SaveToWeather(path + "\\dlc\\dlc_FootballCompEng\\dlc\\FootballCompEng\\data\\compdata\\weather.txt");
			SaveToTasks(path + "\\dlc\\dlc_FootballCompEng\\dlc\\FootballCompEng\\data\\compdata\\tasks.txt");
			SaveToInitteams(path + "\\dlc\\dlc_FootballCompEng\\dlc\\FootballCompEng\\data\\compdata\\initteams.txt");
			SaveToInternationals(path + "\\dlc\\dlc_FootballCompEng\\dlc\\FootballCompEng\\data\\internationals.txt");
			SaveToLanguage();
			Table t = fifaDbFile.Table[TI.competition];
			SaveToCompetition(t);
		}

		public int Renumber()
		{
			if (World != null)
			{
				return World.Renumber(0);
			}
			return 0;
		}

		private bool SaveToCompobj(string fileName)
		{
			if (World == null)
			{
				return false;
			}
			File.Copy(fileName, fileName + ".bak", overwrite: true);
			StreamWriter streamWriter = new StreamWriter(fileName, append: false);
			if (streamWriter == null)
			{
				return false;
			}
			World.SaveRecursivelyToCompobj(streamWriter);
			streamWriter.Close();
			return true;
		}

		private bool SaveToCompids(string fileName)
		{
			if (World == null)
			{
				return false;
			}
			if (!File.Exists(fileName))
			{
				return false;
			}
			File.Copy(fileName, fileName + ".bak", overwrite: true);
			StreamWriter streamWriter = new StreamWriter(fileName, append: false);
			if (streamWriter == null)
			{
				return false;
			}
			World.SaveRecursivelyToCompids(streamWriter);
			streamWriter.Close();
			return true;
		}

		private bool SaveToSettings(string fileName)
		{
			if (World == null)
			{
				return false;
			}
			File.Copy(fileName, fileName + ".bak", overwrite: true);
			StreamWriter streamWriter = new StreamWriter(fileName, append: false);
			if (streamWriter == null)
			{
				return false;
			}
			World.SaveRecursivelyToSettings(streamWriter);
			streamWriter.Close();
			return true;
		}

		private bool SaveToStandings(string fileName)
		{
			if (World == null)
			{
				return false;
			}
			File.Copy(fileName, fileName + ".bak", overwrite: true);
			StreamWriter streamWriter = new StreamWriter(fileName, append: false);
			if (streamWriter == null)
			{
				return false;
			}
			World.SaveRecursivelyToStandings(streamWriter);
			streamWriter.Close();
			return true;
		}

		private bool SaveToAdvancement(string fileName)
		{
			if (World == null)
			{
				return false;
			}
			File.Copy(fileName, fileName + ".bak", overwrite: true);
			StreamWriter streamWriter = new StreamWriter(fileName, append: false);
			if (streamWriter == null)
			{
				return false;
			}
			World.SaveRecursivelyToAdvancement(streamWriter);
			streamWriter.Close();
			return true;
		}

		private bool SaveToSchedule(string fileName)
		{
			if (World == null)
			{
				return false;
			}
			File.Copy(fileName, fileName + ".bak", overwrite: true);
			StreamWriter streamWriter = new StreamWriter(fileName, append: false);
			if (streamWriter == null)
			{
				return false;
			}
			World.SaveRecursivelyToSchedule(streamWriter);
			streamWriter.Close();
			return true;
		}

		private bool SaveToWeather(string fileName)
		{
			if (World == null)
			{
				return false;
			}
			File.Copy(fileName, fileName + ".bak", overwrite: true);
			StreamWriter streamWriter = new StreamWriter(fileName, append: false);
			if (streamWriter == null)
			{
				return false;
			}
			World.SaveRecursivelyToWeather(streamWriter);
			streamWriter.Close();
			return true;
		}

		private bool SaveToInitteams(string fileName)
		{
			if (World == null)
			{
				return false;
			}
			File.Copy(fileName, fileName + ".bak", overwrite: true);
			StreamWriter streamWriter = new StreamWriter(fileName, append: false);
			if (streamWriter == null)
			{
				return false;
			}
			World.SaveRecursivelyToInitteams(streamWriter);
			streamWriter.Close();
			return true;
		}

		private bool SaveToInternationals(string fileName)
		{
			int num = 0;
			string text = null;
			char[] separator = new char[3]
			{
				',',
				'[',
				']'
			};
			if (!File.Exists(fileName))
			{
				return false;
			}
			File.Copy(fileName, fileName + ".bak", overwrite: true);
			StreamReader streamReader = new StreamReader(fileName + ".bak");
			if (streamReader == null)
			{
				return false;
			}
			StreamWriter streamWriter = new StreamWriter(fileName, append: false);
			if (streamWriter == null)
			{
				return false;
			}
			while ((text = streamReader.ReadLine()) != null)
			{
				string[] array = text.Split(separator);
				if (array.Length == 3)
				{
					if (array[1] == "TIER_AND_OBJECTIVES")
					{
						streamWriter.WriteLine(text);
						num = 1;
						foreach (Country country in FifaEnvironment.Countries)
						{
							if (country.NationalTeam != null)
							{
								int level = country.Level;
								int id = country.Id;
								int id2 = country.NationalTeam.Id;
								int num2 = country.Confederation + 1;
								string text2 = CupTarget(country.WorldCupTarget);
								string text3 = CupTarget(country.ContinentalCupTarget);
								text = level.ToString() + "," + id.ToString() + "," + id2.ToString() + "," + num2.ToString() + "," + text2 + "," + text2 + "," + text3 + "," + text3;
								streamWriter.WriteLine(text);
							}
						}
						continue;
					}
					if (array[1] == "LEAGUE_RANKINGS")
					{
						streamWriter.WriteLine("\r\n");
						streamWriter.WriteLine(text);
						num = 1;
						foreach (League league in FifaEnvironment.Leagues)
						{
							text = league.Id.ToString() + ",";
							switch (league.Prestige)
							{
							case ELeaguePrestige.England_Spain_Germany_Italy:
								text += "1,2,3,4,7";
								break;
							case ELeaguePrestige.France:
								text += "2,3,4,6,7";
								break;
							case ELeaguePrestige.Argentina_Brazil:
								text += "2,4,5,6,7";
								break;
							case ELeaguePrestige.Russia_Portugal_Turkey:
								text += "3,5,6,7,7";
								break;
							case ELeaguePrestige.Holland:
								text += "4,5,6,7,7";
								break;
							case ELeaguePrestige.Mexico_England2:
								text += "4,6,7,7,7";
								break;
							case ELeaguePrestige.Belgium_Germany2_Colombia:
								text += "5,6,7,7,7";
								break;
							case ELeaguePrestige.USA_Chile:
								text += "5,6,7,7,-1";
								break;
							case ELeaguePrestige.Scotland_Italy2_Spain2:
								text += "5,6,7,-1,-1";
								break;
							case ELeaguePrestige.France2_Denmark_Norway_Switzerland:
								text += "6,7,-1,-1,-1";
								break;
							case ELeaguePrestige.Poland_Austria_Korea:
								text += "7,7,-1,-1,-1";
								break;
							case ELeaguePrestige.Australia_Sweden_England3_England4_Ireland:
								text += "7,-1,-1,-1,-1";
								break;
							}
							if (league.Prestige != ELeaguePrestige.Undefined)
							{
								streamWriter.WriteLine(text);
							}
						}
						continue;
					}
					if (num == 1)
					{
						streamWriter.WriteLine();
						num = 2;
					}
				}
				if (num != 1)
				{
					streamWriter.WriteLine(text);
				}
			}
			streamReader.Close();
			streamWriter.Close();
			return true;
		}

		private string CupTarget(int index)
		{
			switch (index)
			{
			case 1:
				return "WIN";
			case 2:
				return "FINAL";
			case 3:
				return "SEMI";
			case 4:
				return "QUARTER";
			case 5:
				return "KNOCKOUT";
			case 6:
				return "QUALIFY";
			default:
				return "N/A";
			}
		}

		private bool SaveToTasks(string fileName)
		{
			if (World == null)
			{
				return false;
			}
			File.Copy(fileName, fileName + ".bak", overwrite: true);
			StreamWriter streamWriter = new StreamWriter(fileName, append: false);
			if (streamWriter == null)
			{
				return false;
			}
			World.SaveRecursivelyToTasks(streamWriter);
			streamWriter.Close();
			return true;
		}

		public void Link()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Compobj compobj = (Compobj)enumerator.Current;
					if (compobj.IsTrophy())
					{
						compobj.LinkLeague(FifaEnvironment.Leagues);
						compobj.LinkTeam(FifaEnvironment.Teams);
						compobj.LinkCompetitions();
					}
					if (compobj.IsNation())
					{
						compobj.LinkCountry(FifaEnvironment.Countries);
					}
					if (compobj.IsStage())
					{
						compobj.LinkStadium(FifaEnvironment.Stadiums);
						compobj.LinkCompetitions();
					}
					if (compobj.IsGroup())
					{
						compobj.LinkCompetitions();
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

		private void Normalize()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((Compobj)enumerator.Current).Normalize();
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

		public Trophy SearchTrophy(int assetId)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Compobj compobj = (Compobj)enumerator.Current;
					if (compobj.IsTrophy() && compobj.Settings.m_asset_id == assetId)
					{
						return (Trophy)compobj;
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

		public int GetInternationalFriendlyId()
		{
			foreach (Trophy trophy in Trophies)
			{
				if (trophy.Settings.m_comp_type == "INTERFRIENDLY")
				{
					return trophy.Id;
				}
			}
			return -1;
		}
	}
}
