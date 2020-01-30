using System.IO;

namespace FifaLibrary
{
	public class Task
	{
		private string m_When;

		private string m_Action;

		private EQualifyingRule m_Rule;

		private int m_TargetCompObjId;

		private int m_Parameter1;

		private int m_Parameter2;

		private int m_Parameter3;

		private Group m_Group;

		private Stage m_Stage;

		private Trophy m_Trophy;

		private Trophy m_Trophy1;

		private Trophy m_Trophy2;

		private League m_League;

		private Team m_Team;

		public string When
		{
			get
			{
				return m_When;
			}
			set
			{
				m_When = value;
			}
		}

		public string Action
		{
			get
			{
				return m_Action;
			}
			set
			{
				m_Action = value;
			}
		}

		public EQualifyingRule Rule
		{
			get
			{
				return m_Rule;
			}
			set
			{
				m_Rule = value;
				m_Action = m_Rule.ToString();
			}
		}

		public int GroupId => m_TargetCompObjId;

		public int Parameter1
		{
			get
			{
				return m_Parameter1;
			}
			set
			{
				m_Parameter1 = value;
			}
		}

		public int Parameter2
		{
			get
			{
				return m_Parameter2;
			}
			set
			{
				m_Parameter2 = value;
			}
		}

		public int Parameter3
		{
			get
			{
				return m_Parameter3;
			}
			set
			{
				m_Parameter3 = value;
			}
		}

		public Group Group
		{
			get
			{
				return m_Group;
			}
			set
			{
				m_Group = value;
				m_TargetCompObjId = m_Group.Id;
			}
		}

		public Stage Stage
		{
			get
			{
				return m_Stage;
			}
			set
			{
				m_Stage = value;
				m_TargetCompObjId = m_Stage.Id;
			}
		}

		public Trophy Trophy
		{
			get
			{
				return m_Trophy;
			}
			set
			{
				m_Trophy = value;
				m_TargetCompObjId = m_Trophy.Id;
			}
		}

		public Trophy Trophy1
		{
			get
			{
				return m_Trophy1;
			}
			set
			{
				m_Trophy1 = value;
			}
		}

		public Trophy Trophy2
		{
			get
			{
				return m_Trophy2;
			}
			set
			{
				m_Trophy2 = value;
			}
		}

		public League League
		{
			get
			{
				return m_League;
			}
			set
			{
				m_League = value;
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

		public Task(string when, string action, int targetCompObjId, int par1, int par2, int par3)
		{
			m_When = when;
			m_Action = action;
			m_TargetCompObjId = targetCompObjId;
			m_Parameter1 = par1;
			m_Parameter2 = par2;
			m_Parameter3 = par3;
		}

		public Task(Task currentTask)
		{
			m_When = currentTask.m_When;
			m_Action = currentTask.m_Action;
			m_TargetCompObjId = currentTask.m_TargetCompObjId;
			m_Parameter1 = currentTask.m_Parameter1;
			m_Parameter2 = currentTask.m_Parameter2;
			m_Parameter3 = currentTask.m_Parameter3;
		}

		public void LinkCompetitions(Compobj ownerCompobj)
		{
			if (ownerCompobj.IsGroup())
			{
				LinkGroup((Group)ownerCompobj);
			}
			else if (ownerCompobj.IsStage())
			{
				LinkStage((Stage)ownerCompobj);
			}
			else if (ownerCompobj.IsTrophy())
			{
				LinkTrophy((Trophy)ownerCompobj);
			}
		}

		public void LinkGroup(Group ownerGroup)
		{
			m_Group = ownerGroup;
			m_TargetCompObjId = ownerGroup.Id;
			if (m_Action == "FillFromCompTable")
			{
				m_Rule = EQualifyingRule.FillFromCompTable;
				Compobj compobj = (Compobj)FifaEnvironment.CompetitionObjects.SearchId(m_Parameter1);
				if (compobj != null && compobj.IsTrophy())
				{
					m_Trophy1 = (Trophy)compobj;
				}
				else
				{
					m_Trophy1 = null;
				}
				m_Trophy2 = null;
				m_League = null;
				m_Team = null;
			}
			else if (m_Action == "FillFromCompTableBackup")
			{
				m_Rule = EQualifyingRule.FillFromCompTableBackup;
				Compobj compobj2 = (Compobj)FifaEnvironment.CompetitionObjects.SearchId(m_Parameter1);
				if (compobj2 != null && compobj2.IsTrophy())
				{
					m_Trophy1 = (Trophy)compobj2;
				}
				else
				{
					m_Trophy1 = null;
				}
				compobj2 = (Compobj)FifaEnvironment.CompetitionObjects.SearchId(m_Parameter2);
				if (compobj2 != null && compobj2.IsTrophy())
				{
					m_Trophy2 = (Trophy)compobj2;
				}
				else
				{
					m_Trophy2 = null;
				}
				m_League = null;
				m_Team = null;
			}
			else if (m_Action == "FillFromCompTableBackupLeague")
			{
				m_Rule = EQualifyingRule.FillFromCompTableBackupLeague;
				Compobj compobj3 = (Compobj)FifaEnvironment.CompetitionObjects.SearchId(m_Parameter1);
				if (compobj3 != null && compobj3.IsTrophy())
				{
					m_Trophy1 = (Trophy)compobj3;
				}
				m_Trophy2 = null;
				m_League = (League)FifaEnvironment.Leagues.SearchId(m_Parameter2);
				m_Team = null;
			}
			else if (m_Action == "FillFromLeague")
			{
				m_Rule = EQualifyingRule.FillFromLeague;
				m_Trophy1 = null;
				m_Trophy2 = null;
				m_League = (League)FifaEnvironment.Leagues.SearchId(m_Parameter1);
				m_Team = null;
			}
			else if (m_Action == "FillFromLeagueMaxFromCountry")
			{
				m_Rule = EQualifyingRule.FillFromLeagueMaxFromCountry;
				m_Trophy1 = null;
				m_Trophy2 = null;
				m_League = (League)FifaEnvironment.Leagues.SearchId(m_Parameter1);
				m_Team = null;
			}
			else if (m_Action == "FillFromSpecialTeams")
			{
				m_Rule = EQualifyingRule.FillFromSpecialTeams;
				m_Trophy1 = null;
				m_Trophy2 = null;
				m_League = null;
				m_Team = null;
			}
			else if (m_Action == "FillWithTeam")
			{
				m_Rule = EQualifyingRule.FillWithTeam;
				m_Trophy1 = null;
				m_Trophy2 = null;
				m_League = null;
				Team team = m_Team = (Team)FifaEnvironment.Teams.SearchId(m_Parameter2);
			}
			else if (m_Action == "FillFromLeagueInOrder")
			{
				m_Rule = EQualifyingRule.FillFromLeagueInOrder;
				m_Trophy1 = null;
				m_Trophy2 = null;
				m_League = (League)FifaEnvironment.Leagues.SearchId(m_Parameter1);
				m_Team = null;
			}
		}

		public void LinkStage(Stage ownerStage)
		{
			m_Stage = ownerStage;
			m_TargetCompObjId = ownerStage.Id;
			if (m_Action == "UpdateLeagueTable")
			{
				m_Rule = EQualifyingRule.NoRule;
				m_Trophy1 = null;
				m_Trophy2 = null;
				m_League = (League)FifaEnvironment.Leagues.SearchId(m_Parameter1);
				m_Team = null;
			}
			else if (m_Action == "UpdateLeagueStats")
			{
				m_Rule = EQualifyingRule.NoRule;
				m_Trophy1 = null;
				m_Trophy2 = null;
				m_League = (League)FifaEnvironment.Leagues.SearchId(m_Parameter1);
				m_Team = null;
			}
			else if (m_Action == "ClearLeagueStats")
			{
				m_Rule = EQualifyingRule.NoRule;
				m_Trophy1 = null;
				m_Trophy2 = null;
				m_League = (League)FifaEnvironment.Leagues.SearchId(m_Parameter1);
				m_Team = null;
			}
		}

		public void LinkTrophy(Trophy ownerTrophy)
		{
			m_Trophy = ownerTrophy;
			m_TargetCompObjId = ownerTrophy.Id;
			if (m_Action == "UpdateTable")
			{
				m_Rule = EQualifyingRule.NoRule;
				m_Trophy1 = null;
				m_Trophy2 = null;
				m_League = null;
				m_Team = null;
				Compobj compobj = (Compobj)FifaEnvironment.CompetitionObjects.SearchId(m_Parameter1);
				if (compobj != null && compobj.IsGroup())
				{
					m_Group = (Group)FifaEnvironment.CompetitionObjects.SearchId(m_Parameter1);
				}
			}
		}

		public override string ToString()
		{
			string text = null;
			text = "To be defined";
			if (m_Action == "UpdateTable")
			{
				if (m_Group != null)
				{
					text = "Team n." + m_Parameter2.ToString() + " of " + m_Group.ParentStage.ToString() + " - " + m_Group.ToString();
				}
			}
			else if (m_Action == "ClearLeagueStats")
			{
				if (m_League != null)
				{
					text = "Clear Stats of league: " + m_League.ToString();
				}
			}
			else if (m_Action == "UpdateLeagueTable")
			{
				if (m_League != null)
				{
					text = "Update Table of league: " + m_League.ToString();
				}
			}
			else if (m_Action == "UpdateLeagueStats")
			{
				if (m_League != null)
				{
					text = "Update Stats of league: " + m_League.ToString();
				}
			}
			else
			{
				switch (m_Rule)
				{
				case EQualifyingRule.FillFromCompTable:
					if (m_Trophy1 != null)
					{
						text = ((m_Parameter2 != 1) ? ("Get best " + m_Parameter2.ToString() + " teams of " + m_Trophy1.ToString()) : ("Get winner of " + m_Trophy1.ToString()));
						m_Parameter1 = m_Trophy1.Id;
					}
					break;
				case EQualifyingRule.FillFromCompTableBackup:
					if (m_Trophy1 != null && m_Trophy2 != null)
					{
						text = "Get winner of " + m_Trophy1.ToString() + " or a team from " + m_Trophy2.ToString();
						m_Parameter1 = m_Trophy1.Id;
						m_Parameter2 = m_Trophy2.Id;
					}
					break;
				case EQualifyingRule.FillFromCompTableBackupLeague:
					if (m_Trophy1 != null && m_League != null)
					{
						text = "Get winner of " + m_Trophy1.ToString() + " or a team from league: " + m_League.ToString();
						m_Parameter1 = m_Trophy1.Id;
						m_Parameter2 = m_League.Id;
					}
					break;
				case EQualifyingRule.FillFromLeague:
					if (m_League != null)
					{
						text = "Get teams from league: " + m_League.ToString();
						m_Parameter1 = m_League.Id;
					}
					break;
				case EQualifyingRule.FillFromLeagueInOrder:
					if (m_League != null)
					{
						text = "Get teams in order from league: " + m_League.ToString();
						m_Parameter1 = m_League.Id;
					}
					break;
				case EQualifyingRule.FillFromLeagueMaxFromCountry:
					if (m_League != null)
					{
						text = "Get up to " + m_Parameter2.ToString() + " team(s) from league: " + m_League.ToString() + " (max " + m_Parameter3.ToString() + ")";
						m_Parameter1 = m_League.Id;
					}
					break;
				case EQualifyingRule.FillFromSpecialTeams:
					text = "Get " + m_Parameter1.ToString() + " team(s) from Special Teams Group";
					break;
				case EQualifyingRule.FillWithTeam:
					if (m_Team != null)
					{
						text = "Get Specific Team: " + m_Team.ToString();
						if (m_Parameter1 != 0)
						{
							text = text + " at position " + m_Parameter1.ToString();
						}
						m_Parameter2 = m_Team.Id;
					}
					break;
				}
			}
			return text;
		}

		public bool SaveToTasks(StreamWriter w)
		{
			if (w == null)
			{
				return false;
			}
			string text = null;
			if (m_When == "start")
			{
				if (m_Group != null)
				{
					text = m_Group.ParentTrophy.Id.ToString() + ",start," + m_Action + "," + m_Group.Id.ToString() + ",";
				}
				else if (m_Stage != null)
				{
					text = m_Stage.Trophy.Id.ToString() + ",start," + m_Action + "," + m_Stage.Id.ToString() + ",";
				}
				if (m_Action == "FillFromCompTable")
				{
					text = text + ((m_Trophy1 != null) ? m_Trophy1.Id.ToString() : m_Parameter1.ToString()) + "," + m_Parameter2.ToString() + "," + m_Parameter3.ToString();
				}
				else if (m_Action == "FillFromCompTableBackup")
				{
					text = text + ((m_Trophy1 != null) ? m_Trophy1.Id.ToString() : m_Parameter1.ToString()) + "," + ((m_Trophy2 != null) ? m_Trophy2.Id.ToString() : m_Parameter2.ToString()) + "," + m_Parameter3.ToString();
				}
				else if (m_Action == "FillFromCompTableBackupLeague")
				{
					text = text + ((m_Trophy1 != null) ? m_Trophy1.Id.ToString() : m_Parameter1.ToString()) + "," + ((m_League != null) ? m_League.Id.ToString() : m_Parameter2.ToString()) + "," + m_Parameter3.ToString();
				}
				else if (m_Action == "FillFromLeague")
				{
					text = text + ((m_League != null) ? m_League.Id.ToString() : m_Parameter1.ToString()) + "," + m_Parameter2.ToString() + "," + m_Parameter3.ToString();
				}
				else if (m_Action == "FillFromLeagueMaxFromCountry")
				{
					text = text + ((m_League != null) ? m_League.Id.ToString() : m_Parameter1.ToString()) + "," + m_Parameter2.ToString() + "," + m_Parameter3.ToString();
				}
				else if (m_Action == "FillFromSpecialTeams")
				{
					text = text + m_Parameter1.ToString() + "," + m_Parameter2.ToString() + "," + m_Parameter3.ToString();
				}
				else if (m_Action == "FillWithTeam")
				{
					text = text + m_Parameter1.ToString() + "," + ((m_Team != null) ? m_Team.Id.ToString() : m_Parameter2.ToString()) + "," + m_Parameter3.ToString();
				}
				else if (m_Action == "FillFromLeagueInOrder")
				{
					text = text + ((m_League != null) ? m_League.Id.ToString() : m_Parameter1.ToString()) + "," + m_Parameter2.ToString() + "," + m_Parameter3.ToString();
				}
				else if (m_Action == "ClearLeagueStats")
				{
					text = text + ((m_League != null) ? m_League.Id.ToString() : m_Parameter1.ToString()) + "," + m_Parameter2.ToString() + "," + m_Parameter3.ToString();
				}
			}
			else if (m_Action == "UpdateTable")
			{
				if (m_Trophy != null && m_Group != null)
				{
					text = m_Trophy.Id.ToString() + ",end," + m_Action + "," + m_Trophy.Id.ToString() + "," + m_Group.Id.ToString() + "," + m_Parameter2.ToString() + "," + m_Parameter3.ToString();
				}
				else
				{
					FifaEnvironment.UserMessages.ShowMessage(14999, "Debug Trap: Task::SaveToTasks()");
				}
			}
			else if (m_Action == "UpdateLeagueTable")
			{
				FifaEnvironment.UserMessages.ShowMessage(14999, "Debug Trap: Task::SaveToTasks() UpdateLeagueTable unexpected");
				if (m_Stage != null)
				{
					text = ((m_League == null) ? (m_Stage.Trophy.Id.ToString() + ",end," + m_Action + "," + m_Stage.Id.ToString() + "," + m_Parameter1.ToString() + "," + m_Parameter2.ToString() + "," + m_Parameter3.ToString()) : (m_Stage.Trophy.Id.ToString() + ",end," + m_Action + "," + m_Stage.Id.ToString() + "," + m_League.Id.ToString() + "," + m_Parameter2.ToString() + "," + m_Parameter3.ToString()));
				}
				else if (m_Group != null && m_League != null)
				{
					text = m_Group.ParentTrophy.Id.ToString() + ",end," + m_Action + "," + m_Group.ParentStage.Id.ToString() + "," + m_League.Id.ToString() + "," + m_Parameter2.ToString() + "," + m_Parameter3.ToString();
				}
				else
				{
					FifaEnvironment.UserMessages.ShowMessage(14999, "Debug Trap: Task::SaveToTasks()");
				}
			}
			else if (m_Action == "UpdateLeagueStats")
			{
				if (m_Stage != null && m_League != null)
				{
					text = m_Stage.Trophy.Id.ToString() + ",end," + m_Action + "," + m_Stage.Id.ToString() + "," + m_League.Id.ToString() + "," + m_Parameter2.ToString() + "," + m_Parameter3.ToString();
				}
				else
				{
					FifaEnvironment.UserMessages.ShowMessage(14999, "Debug Trap: Task::SaveToTasks()");
				}
			}
			else
			{
				FifaEnvironment.UserMessages.ShowMessage(14999, "Debug Trap: Task::SaveToTasks()");
			}
			w.WriteLine(text);
			return true;
		}

		public Task CopyTask(Compobj newTargetObj, League targetLeague)
		{
			Task task = new Task(this);
			task.m_TargetCompObjId = newTargetObj.Id;
			if (!(m_Action == "FillFromCompTable") && !(m_Action == "FillFromCompTableBackup"))
			{
				if (m_Action == "FillFromCompTableBackupLeague")
				{
					if (targetLeague != null)
					{
						task.Parameter2 = targetLeague.Id;
					}
				}
				else if (m_Action == "FillFromLeague")
				{
					if (targetLeague != null)
					{
						task.Parameter1 = targetLeague.Id;
					}
				}
				else if (m_Action == "FillFromLeagueMaxFromCountry")
				{
					if (targetLeague != null)
					{
						task.Parameter1 = targetLeague.Id;
					}
				}
				else if (!(m_Action == "FillFromSpecialTeams") && !(m_Action == "FillWithTeam"))
				{
					if (m_Action == "FillFromLeagueInOrder")
					{
						if (targetLeague != null)
						{
							task.Parameter1 = targetLeague.Id;
						}
					}
					else if (m_Action == "UpdateLeagueTable")
					{
						if (targetLeague != null)
						{
							task.Parameter1 = targetLeague.Id;
						}
					}
					else if (m_Action == "UpdateLeagueStats")
					{
						if (targetLeague != null)
						{
							task.Parameter1 = targetLeague.Id;
						}
					}
					else if (m_Action == "ClearLeagueStats")
					{
						if (targetLeague != null)
						{
							task.Parameter1 = targetLeague.Id;
						}
					}
					else if (m_Action == "UpdateTable")
					{
						task.Parameter1 = Parameter1 + task.m_TargetCompObjId - m_TargetCompObjId;
					}
				}
			}
			if (newTargetObj.IsTrophy())
			{
				task.LinkTrophy((Trophy)newTargetObj);
			}
			else if (newTargetObj.IsStage())
			{
				task.LinkStage((Stage)newTargetObj);
			}
			else if (newTargetObj.IsGroup())
			{
				task.LinkGroup((Group)newTargetObj);
			}
			return task;
		}
	}
}
