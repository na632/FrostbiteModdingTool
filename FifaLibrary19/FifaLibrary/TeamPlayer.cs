namespace FifaLibrary
{
	public class TeamPlayer
	{
		public int m_jerseynumber;

		private int m_position;

		private Player m_Player;

		private Team m_Team;

		private int m_leaguegoals;

		private bool m_isamongtopscorers;

		private int m_yellows;

		private int m_reds;

		private bool m_isamongtopscorersinteam;

		private int m_injury;

		private int m_leagueappearances;

		private int m_leaguegoalsprevmatch;

		private int m_leaguegoalsprevthreematches;

		private bool m_istopscorer;

		private int m_form;

		private int m_prevform;

		private int m_State;

		public int jerseynumber
		{
			get
			{
				return m_jerseynumber;
			}
			set
			{
				m_jerseynumber = value;
			}
		}

		public int position
		{
			get
			{
				return m_position;
			}
			set
			{
				m_position = value;
			}
		}

		public Player Player
		{
			get
			{
				return m_Player;
			}
			set
			{
				m_Player = value;
			}
		}

		public Team Team
		{
			get
			{
				return m_Team;
			}
			set
			{
				m_Team = value;
			}
		}

		public int State
		{
			get
			{
				return m_State;
			}
			set
			{
				m_State = value;
			}
		}

		public TeamPlayer(Record r, Player player, Team team)
		{
			m_jerseynumber = r.GetAndCheckIntField(FI.teamplayerlinks_jerseynumber);
			m_position = r.GetAndCheckIntField(FI.teamplayerlinks_position);
			m_Player = player;
			m_Team = team;
			m_leaguegoals = r.GetAndCheckIntField(FI.teamplayerlinks_leaguegoals);
			m_leagueappearances = r.GetAndCheckIntField(FI.teamplayerlinks_leagueappearances);
			m_leaguegoalsprevmatch = r.GetAndCheckIntField(FI.teamplayerlinks_leaguegoalsprevmatch);
			m_leaguegoalsprevthreematches = r.GetAndCheckIntField(FI.teamplayerlinks_leaguegoalsprevthreematches);
			m_prevform = r.GetAndCheckIntField(FI.teamplayerlinks_prevform);
			m_form = r.GetAndCheckIntField(FI.teamplayerlinks_form);
			m_isamongtopscorers = (r.GetAndCheckIntField(FI.teamplayerlinks_isamongtopscorers) != 0);
			m_isamongtopscorersinteam = (r.GetAndCheckIntField(FI.teamplayerlinks_isamongtopscorersinteam) != 0);
			m_istopscorer = (r.GetAndCheckIntField(FI.teamplayerlinks_istopscorer) != 0);
			m_injury = r.GetAndCheckIntField(FI.teamplayerlinks_injury);
			m_yellows = r.GetAndCheckIntField(FI.teamplayerlinks_yellows);
			m_reds = r.GetAndCheckIntField(FI.teamplayerlinks_reds);
			m_State = 0;
		}

		public TeamPlayer(Player player)
		{
			m_jerseynumber = 99;
			m_position = player.preferredposition1;
			m_Player = player;
			m_leaguegoals = 0;
			m_isamongtopscorers = false;
			m_isamongtopscorersinteam = false;
			m_istopscorer = false;
			m_yellows = 0;
			m_reds = 0;
			m_injury = 0;
			m_leagueappearances = 0;
			m_form = 3;
			m_prevform = 3;
			m_State = 0;
		}

		public TeamPlayer(ERole role)
		{
			m_jerseynumber = 0;
			m_position = (int)role;
			m_Player = null;
			m_Team = null;
			m_leaguegoals = 0;
			m_isamongtopscorers = false;
			m_isamongtopscorersinteam = false;
			m_istopscorer = false;
			m_yellows = 0;
			m_reds = 0;
			m_injury = 0;
			m_leagueappearances = 0;
			m_form = 3;
			m_prevform = 3;
			m_State = 0;
		}

		public void Save(Record r, int artificialkey)
		{
			if (m_Player != null && m_Team != null)
			{
				r.IntField[FI.teamplayerlinks_artificialkey] = artificialkey;
				r.IntField[FI.teamplayerlinks_teamid] = m_Team.Id;
				r.IntField[FI.teamplayerlinks_playerid] = m_Player.Id;
				r.IntField[FI.teamplayerlinks_jerseynumber] = m_jerseynumber;
				r.IntField[FI.teamplayerlinks_position] = m_position;
				r.IntField[FI.teamplayerlinks_leaguegoals] = m_leaguegoals;
				r.IntField[FI.teamplayerlinks_leagueappearances] = m_leagueappearances;
				r.IntField[FI.teamplayerlinks_leaguegoalsprevthreematches] = m_leaguegoalsprevthreematches;
				r.IntField[FI.teamplayerlinks_leaguegoalsprevmatch] = m_leaguegoalsprevmatch;
				r.IntField[FI.teamplayerlinks_prevform] = (m_prevform = r.GetAndCheckIntField(FI.teamplayerlinks_prevform));
				r.IntField[FI.teamplayerlinks_form] = (m_form = r.GetAndCheckIntField(FI.teamplayerlinks_form));
				r.IntField[FI.teamplayerlinks_isamongtopscorers] = (m_isamongtopscorers ? 1 : 0);
				r.IntField[FI.teamplayerlinks_isamongtopscorersinteam] = (m_isamongtopscorersinteam ? 1 : 0);
				r.IntField[FI.teamplayerlinks_istopscorer] = (m_istopscorer ? 1 : 0);
				r.IntField[FI.teamplayerlinks_injury] = m_injury;
				r.IntField[FI.teamplayerlinks_yellows] = m_yellows;
				r.IntField[FI.teamplayerlinks_reds] = m_reds;
			}
		}

		public override string ToString()
		{
			return m_jerseynumber.ToString() + " " + ((m_Player != null) ? m_Player.ToString() : string.Empty);
		}

		public int AssignFreeNumber(int guessNumber)
		{
			int jerseynumber = m_jerseynumber;
			int step = (guessNumber > jerseynumber) ? 1 : (-1);
			m_jerseynumber = RecursiveNumber(jerseynumber, step);
			return m_jerseynumber;
		}

		private int RecursiveNumber(int number, int step)
		{
			if (!m_Team.Roster.IsNumberFree(number))
			{
				switch (number)
				{
				case 99:
					number = 1;
					break;
				case 1:
					number = 99;
					break;
				default:
					number += step;
					break;
				}
				return RecursiveNumber(number, step);
			}
			return number;
		}
	}
}
