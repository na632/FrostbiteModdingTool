namespace FifaLibrary
{
	public class InitTeam : IdObject
	{
		private int m_teamid;

		private Team m_Team;

		public int teamid => m_teamid;

		public Team Team
		{
			get
			{
				return m_Team;
			}
			set
			{
				m_Team = value;
				m_teamid = ((m_Team != null) ? m_Team.Id : (-1));
			}
		}

		public InitTeam(int orderId, int teamId)
		{
			base.Id = orderId;
			m_teamid = teamId;
		}

		public void LinkTeam(TeamList teamList)
		{
			if (teamList != null)
			{
				m_Team = (Team)teamList.SearchId(m_teamid);
			}
		}
	}
}
