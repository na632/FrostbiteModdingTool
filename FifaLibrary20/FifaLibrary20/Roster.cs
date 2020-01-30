using System;
using System.Collections;

namespace FifaLibrary
{
	public class Roster : ArrayList
	{
		private RosterComparer m_Comparer = new RosterComparer();

		public void SortRoster()
		{
			Sort(m_Comparer);
		}

		public Roster(int capacity)
		{
			Capacity = capacity;
		}

		public Player SearchPlayer(int playerid)
		{
			for (int i = 0; i < Count; i++)
			{
				if (playerid == ((TeamPlayer)this[i]).Player.Id)
				{
					return ((TeamPlayer)this[i]).Player;
				}
			}
			return null;
		}

		public TeamPlayer SearchTeamPlayer(Player player)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TeamPlayer teamPlayer = (TeamPlayer)enumerator.Current;
					if (player == teamPlayer.Player)
					{
						return teamPlayer;
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

		public int GetTeamPosition(Player player)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TeamPlayer teamPlayer = (TeamPlayer)enumerator.Current;
					if (player == teamPlayer.Player)
					{
						return teamPlayer.position;
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
			return 29;
		}

		public int GetTeamPosition(int playerId)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TeamPlayer teamPlayer = (TeamPlayer)enumerator.Current;
					if (playerId == teamPlayer.Player.Id)
					{
						return teamPlayer.position;
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
			return 29;
		}

		public TeamPlayer SearchTeamPlayer(Role role)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TeamPlayer teamPlayer = (TeamPlayer)enumerator.Current;
					if (role.Id == teamPlayer.position)
					{
						return teamPlayer;
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

		public TeamPlayer GetRoleBestPlayer(ERole requestedRole)
		{
			TeamPlayer result = null;
			int num = -1;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TeamPlayer teamPlayer = (TeamPlayer)enumerator.Current;
					if (teamPlayer != null)
					{
						int rolePerformance = teamPlayer.Player.GetRolePerformance(requestedRole);
						if (rolePerformance > num)
						{
							num = rolePerformance;
							result = teamPlayer;
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

		public TeamPlayer GetRoleBestTitolar(ERole requestedRole)
		{
			TeamPlayer result = null;
			int num = -1;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TeamPlayer teamPlayer = (TeamPlayer)enumerator.Current;
					if (teamPlayer != null && teamPlayer.position < 28)
					{
						int rolePerformance = teamPlayer.Player.GetRolePerformance(requestedRole);
						if (rolePerformance > num)
						{
							num = rolePerformance;
							result = teamPlayer;
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

		public TeamPlayer GetBestTitolar()
		{
			TeamPlayer result = null;
			int num = -1;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TeamPlayer teamPlayer = (TeamPlayer)enumerator.Current;
					if (teamPlayer != null && teamPlayer.position < 28)
					{
						int overallrating = teamPlayer.Player.overallrating;
						if (overallrating > num)
						{
							num = overallrating;
							result = teamPlayer;
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

		public TeamPlayer GetBestPlayer()
		{
			TeamPlayer result = null;
			int num = -1;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TeamPlayer teamPlayer = (TeamPlayer)enumerator.Current;
					if (teamPlayer != null)
					{
						int averageRoleAttribute = teamPlayer.Player.GetAverageRoleAttribute();
						if (averageRoleAttribute > num)
						{
							num = averageRoleAttribute;
							result = teamPlayer;
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

		public bool IsNumberFree(int n)
		{
			if (n < 1 || n > 99)
			{
				return false;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					if (((TeamPlayer)enumerator.Current).m_jerseynumber == n)
					{
						return false;
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

		public TeamPlayer IsNumberUsed(int n)
		{
			if (n < 1 || n > 99)
			{
				return null;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TeamPlayer teamPlayer = (TeamPlayer)enumerator.Current;
					if (teamPlayer.m_jerseynumber == n)
					{
						return teamPlayer;
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

		public int GetFreeNumber()
		{
			for (int i = 1; i < 99; i++)
			{
				if (IsNumberFree(i))
				{
					return i;
				}
			}
			return 99;
		}

		public string[] GetFreeNumbers()
		{
			string[] array = new string[99];
			int num = 0;
			for (int i = 1; i < 100; i++)
			{
				if (IsNumberFree(i))
				{
					array[num] = i.ToString();
					num++;
				}
			}
			Array.Resize(ref array, num);
			return array;
		}

		public void ChangeRole(Role oldRole, Role newRole)
		{
			TeamPlayer teamPlayer = SearchTeamPlayer(oldRole);
			if (teamPlayer != null)
			{
				teamPlayer.position = newRole.Id;
			}
		}

		public int EstimateOverall()
		{
			int[] array = new int[17];
			int num = 0;
			for (int i = 0; i < 16; i++)
			{
				array[i] = 0;
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					int overallrating = ((TeamPlayer)enumerator.Current).Player.overallrating;
					for (int j = 0; j < 16; j++)
					{
						if (overallrating > array[j])
						{
							for (int num2 = num; num2 > j; num2--)
							{
								array[num2] = array[num2 - 1];
							}
							array[j] = overallrating;
							if (num < 16)
							{
								num++;
							}
							break;
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
			int num3 = 0;
			for (int k = 0; k < num; k++)
			{
				num3 += array[k];
			}
			if (num != 0)
			{
				return num3 / num;
			}
			return 50;
		}

		public void ResetToEmpty()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TeamPlayer teamPlayer = (TeamPlayer)enumerator.Current;
					teamPlayer.Player.NotPlayFor(teamPlayer.Team);
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
			Clear();
		}

		public void PresetToEmpty(Formation formation, Player[] players)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					TeamPlayer teamPlayer = (TeamPlayer)enumerator.Current;
					teamPlayer.Player.NotPlayFor(teamPlayer.Team);
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
			Clear();
			foreach (Player player in players)
			{
				TeamPlayer teamPlayer2 = new TeamPlayer(player);
				if (player.preferredNumber != 0)
				{
					teamPlayer2.jerseynumber = player.preferredNumber;
				}
				else
				{
					teamPlayer2.jerseynumber = GetFreeNumber();
				}
			}
		}
	}
}
