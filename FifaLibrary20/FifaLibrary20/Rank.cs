namespace FifaLibrary
{
	public class Rank : IdObject
	{
		private Group m_Group;

		private Rank m_MoveFrom;

		private Rank m_MoveTo;

		public Group Group
		{
			get
			{
				return m_Group;
			}
			set
			{
				m_Group = value;
			}
		}

		public Rank MoveFrom
		{
			get
			{
				return m_MoveFrom;
			}
			set
			{
				m_MoveFrom = value;
			}
		}

		public Rank MoveTo
		{
			get
			{
				return m_MoveTo;
			}
			set
			{
				m_MoveTo = value;
			}
		}

		public Rank(Group group, int orderId)
		{
			base.Id = orderId;
			m_Group = group;
		}

		public override string ToString()
		{
			string text = null;
			Stage parentStage = m_Group.ParentStage;
			Trophy trophy = parentStage.Trophy;
			if (base.Id != 0)
			{
				return "Team n." + base.Id.ToString() + " of " + parentStage.ToString() + " / " + m_Group.ToString() + " of " + trophy.ToString();
			}
			return "A team from " + parentStage.ToString() + " / " + m_Group.ToString() + " of " + trophy.ToString();
		}

		public string GetFromRankString()
		{
			if (m_MoveFrom == null || m_MoveFrom.Group == null)
			{
				return "To be defined";
			}
			if (!FifaEnvironment.CompetitionObjects.Contains(m_MoveFrom.Group))
			{
				m_MoveFrom = null;
				return "To be defined";
			}
			if (m_MoveTo != null && !FifaEnvironment.CompetitionObjects.Contains(m_MoveTo.Group))
			{
				m_MoveTo = null;
			}
			string text = null;
			Trophy trophy = m_Group.ParentStage.Trophy;
			Trophy trophy2 = m_MoveFrom.Group.ParentStage.Trophy;
			if (trophy.Id != trophy2.Id)
			{
				if (m_MoveFrom.Id != 0)
				{
					return "Team n." + m_MoveFrom.Id.ToString() + " of " + m_MoveFrom.Group.ParentStage.ToString() + " / " + m_MoveFrom.Group.ToString() + " of " + m_MoveFrom.Group.ParentStage.Trophy.ToString();
				}
				return "A team from " + m_MoveFrom.Group.ParentStage.ToString() + " / " + m_MoveFrom.Group.ToString() + " of " + m_MoveFrom.Group.ParentStage.Trophy.ToString();
			}
			if (m_MoveFrom.Id != 0)
			{
				return "Team n." + m_MoveFrom.Id.ToString() + " of " + m_MoveFrom.Group.ParentStage.ToString() + " / " + m_MoveFrom.Group.ToString();
			}
			return "A team from " + m_MoveFrom.Group.ParentStage.ToString() + " / " + m_MoveFrom.Group.ToString();
		}

		public string GetToRankString()
		{
			string text = null;
			if (m_MoveTo == null)
			{
				return "Undefined";
			}
			Trophy trophy = m_Group.ParentStage.Trophy;
			Trophy trophy2 = m_MoveTo.Group.ParentStage.Trophy;
			if (trophy.Id != trophy2.Id)
			{
				if (m_MoveFrom.Id != 0)
				{
					return "Team n." + m_MoveTo.Id.ToString() + " fof " + m_MoveTo.Group.ParentStage.ToString() + " / " + m_MoveTo.Group.ToString() + " of " + m_MoveFrom.Group.ParentStage.Trophy.ToString();
				}
				return "A team from " + m_MoveTo.Group.ParentStage.ToString() + " / " + m_MoveTo.Group.ToString() + " of " + m_MoveTo.Group.ParentStage.Trophy.ToString();
			}
			if (m_MoveFrom.Id != 0)
			{
				return "Team n." + m_MoveTo.Id.ToString() + " of " + m_MoveTo.Group.ParentStage.ToString() + " / " + m_MoveTo.Group.ToString();
			}
			return "A team from " + m_MoveTo.Group.ParentStage.ToString() + " / " + m_MoveTo.Group.ToString();
		}
	}
}
