using System;

namespace FifaLibrary
{
	public class Formation : IdObject
	{
		private PlayingRole[] m_PlayingRoles = new PlayingRole[11];

		private Team m_Team;

		private string m_formationname;

		private string m_formationfullname;

		private int m_teamid;

		private int m_relativeformationid;

		private int m_formations_issweeper;

		private int m_offensiverating;

		private int m_formationfullnameid;

		private int m_formationaudioid;

		private float m_defenders;

		private float m_midfielders;

		private float m_attackers;

		public PlayingRole[] PlayingRoles
		{
			get
			{
				return m_PlayingRoles;
			}
			set
			{
				m_PlayingRoles = value;
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
				if (m_Team != null)
				{
					m_teamid = m_Team.Id;
				}
				else
				{
					m_teamid = -1;
				}
			}
		}

		public string formationname
		{
			get
			{
				return m_formationname;
			}
			set
			{
				m_formationname = value;
			}
		}

		public string formationfullname
		{
			get
			{
				return m_formationfullname;
			}
			set
			{
				m_formationfullname = value;
			}
		}

		public int teamid
		{
			get
			{
				return m_teamid;
			}
			set
			{
				m_teamid = value;
			}
		}

		public int relativeformationid
		{
			get
			{
				return m_relativeformationid;
			}
			set
			{
				m_relativeformationid = value;
			}
		}

		public int formations_issweeper
		{
			get
			{
				return m_formations_issweeper;
			}
			set
			{
				m_formations_issweeper = value;
			}
		}

		public int offensiverating
		{
			get
			{
				return m_offensiverating;
			}
			set
			{
				m_offensiverating = value;
			}
		}

		public int formationfullnameid
		{
			get
			{
				return m_formationfullnameid;
			}
			set
			{
				m_formationfullnameid = value;
			}
		}

		public int formationaudioid
		{
			get
			{
				return m_formationaudioid;
			}
			set
			{
				m_formationaudioid = value;
			}
		}

		public float defenders
		{
			get
			{
				return m_defenders;
			}
			set
			{
				m_defenders = value;
			}
		}

		public float midfielders
		{
			get
			{
				return m_midfielders;
			}
			set
			{
				m_midfielders = value;
			}
		}

		public float attackers
		{
			get
			{
				return m_attackers;
			}
			set
			{
				m_attackers = value;
			}
		}

		public string Name
		{
			get
			{
				return m_formationname;
			}
			set
			{
				if (value != null)
				{
					m_formationname = value;
				}
				else
				{
					m_formationname = string.Empty;
				}
			}
		}

		public bool IsGeneric()
		{
			return m_teamid == -1;
		}

		public override string ToString()
		{
			if (m_teamid > 0 && m_Team != null)
			{
				return m_Team.DatabaseName + " " + m_formationname;
			}
			return m_formationname;
		}

		public string DatabaseString()
		{
			return ToString();
		}

		public Formation(int formationid)
			: base(formationid)
		{
			base.Id = formationid;
			InitNewFormation();
		}

		public bool ReInitialize(Formation formation)
		{
			if (formation == null)
			{
				return false;
			}
			m_formationname = formation.m_formationname;
			m_formationfullname = formation.m_formationfullname;
			m_relativeformationid = formation.m_relativeformationid;
			m_formations_issweeper = formation.m_formations_issweeper;
			m_offensiverating = formation.m_offensiverating;
			m_formationfullnameid = formation.m_formationfullnameid;
			m_defenders = formation.m_defenders;
			m_midfielders = formation.m_midfielders;
			m_attackers = formation.m_attackers;
			for (int i = 0; i < 11; i++)
			{
				m_PlayingRoles[i].ReInitialize(formation.m_PlayingRoles[i]);
			}
			return true;
		}

		public bool LinkTeam(TeamList teamList)
		{
			if (teamList == null)
			{
				return false;
			}
			if (m_teamid == -1)
			{
				return true;
			}
			m_Team = (Team)teamList.SearchId(m_teamid);
			if (m_Team == null)
			{
				return false;
			}
			return m_Team.formationid == base.Id;
		}

		private void InitNewFormation()
		{
			m_formationname = "4-4-2";
			m_formationfullname = string.Empty;
			m_formationaudioid = 10;
			m_teamid = -1;
			m_relativeformationid = 24;
			m_formations_issweeper = 0;
			m_offensiverating = 1;
			m_formationfullnameid = -1;
			m_defenders = 4f;
			m_midfielders = 4f;
			m_attackers = 2f;
			m_PlayingRoles[0] = new PlayingRole((Role)FifaEnvironment.Roles[0]);
			m_PlayingRoles[1] = new PlayingRole((Role)FifaEnvironment.Roles[3]);
			m_PlayingRoles[2] = new PlayingRole((Role)FifaEnvironment.Roles[4]);
			m_PlayingRoles[3] = new PlayingRole((Role)FifaEnvironment.Roles[6]);
			m_PlayingRoles[4] = new PlayingRole((Role)FifaEnvironment.Roles[7]);
			m_PlayingRoles[5] = new PlayingRole((Role)FifaEnvironment.Roles[12]);
			m_PlayingRoles[6] = new PlayingRole((Role)FifaEnvironment.Roles[13]);
			m_PlayingRoles[7] = new PlayingRole((Role)FifaEnvironment.Roles[15]);
			m_PlayingRoles[8] = new PlayingRole((Role)FifaEnvironment.Roles[16]);
			m_PlayingRoles[9] = new PlayingRole((Role)FifaEnvironment.Roles[24]);
			m_PlayingRoles[10] = new PlayingRole((Role)FifaEnvironment.Roles[26]);
		}

		public Formation(Record r)
			: base(r.IntField[FI.formations_formationid])
		{
			Load(r);
		}

		public override IdObject Clone(int newId)
		{
			Formation formation = (Formation)base.Clone(newId);
			formation.m_PlayingRoles = new PlayingRole[11];
			for (int i = 0; i < 11; i++)
			{
				int id = m_PlayingRoles[i].Id;
				PlayingRole playingRole = (PlayingRole)m_PlayingRoles[i].Clone(id);
				formation.m_PlayingRoles[i] = playingRole;
			}
			formation.m_formationname = "=" + formation.m_formationname;
			formation.m_Team = null;
			return formation;
		}

		public void Load17(Record r)
		{
			TableDescriptor tableDescriptor = r.TableDescriptor;
			m_formationname = r.StringField[tableDescriptor.GetFieldIndex("formationname")];
			m_teamid = r.IntField[tableDescriptor.GetFieldIndex("teamid")];
			m_offensiverating = r.IntField[tableDescriptor.GetFieldIndex("offensiverating")];
			m_formationfullnameid = r.IntField[tableDescriptor.GetFieldIndex("formationfullnameid")];
			m_formationaudioid = r.IntField[tableDescriptor.GetFieldIndex("formationaudioid")];
			m_defenders = r.FloatField[tableDescriptor.GetFieldIndex("defenders")];
			m_midfielders = r.FloatField[tableDescriptor.GetFieldIndex("midfielders")];
			m_attackers = r.FloatField[tableDescriptor.GetFieldIndex("attackers")];
			m_PlayingRoles[0] = new PlayingRole(r, 0, tableDescriptor.GetFieldIndex("position0"));
			m_PlayingRoles[1] = new PlayingRole(r, 1, tableDescriptor.GetFieldIndex("position1"));
			m_PlayingRoles[2] = new PlayingRole(r, 2, tableDescriptor.GetFieldIndex("position2"));
			m_PlayingRoles[3] = new PlayingRole(r, 3, tableDescriptor.GetFieldIndex("position3"));
			m_PlayingRoles[4] = new PlayingRole(r, 4, tableDescriptor.GetFieldIndex("position4"));
			m_PlayingRoles[5] = new PlayingRole(r, 5, tableDescriptor.GetFieldIndex("position5"));
			m_PlayingRoles[6] = new PlayingRole(r, 6, tableDescriptor.GetFieldIndex("position6"));
			m_PlayingRoles[7] = new PlayingRole(r, 7, tableDescriptor.GetFieldIndex("position7"));
			m_PlayingRoles[8] = new PlayingRole(r, 8, tableDescriptor.GetFieldIndex("position8"));
			m_PlayingRoles[9] = new PlayingRole(r, 9, tableDescriptor.GetFieldIndex("position9"));
			m_PlayingRoles[10] = new PlayingRole(r, 10, tableDescriptor.GetFieldIndex("position10"));
			if (m_formationfullnameid != -1)
			{
				if (FifaEnvironment.Language != null)
				{
					m_formationfullname = FifaEnvironment.Language.GetFormationString(m_formationfullnameid);
				}
			}
			else
			{
				m_formationfullname = m_formationname;
			}
			if (m_formationfullname == null || m_formationfullname == string.Empty)
			{
				m_formationfullname = m_formationname;
			}
			if (m_teamid == -1 && m_formationfullnameid == -1)
			{
				FifaEnvironment.Language.GetFreeFormationFullNameId();
			}
		}

		public void Load(Record r)
		{
			m_formationname = r.StringField[FI.formations_formationname];
			m_teamid = r.IntField[FI.formations_teamid];
			m_offensiverating = r.IntField[FI.formations_offensiverating];
			m_formationfullnameid = r.IntField[FI.formations_formationfullnameid];
			if (FI.formations_formationaudioid != -1)
			{
				m_formationaudioid = r.IntField[FI.formations_formationaudioid];
			}
			m_defenders = r.FloatField[FI.formations_defenders];
			m_midfielders = r.FloatField[FI.formations_midfielders];
			m_attackers = r.FloatField[FI.formations_attackers];
			m_PlayingRoles[0] = new PlayingRole(r, 0, FI.formations_position0);
			m_PlayingRoles[1] = new PlayingRole(r, 1, FI.formations_position1);
			m_PlayingRoles[2] = new PlayingRole(r, 2, FI.formations_position2);
			m_PlayingRoles[3] = new PlayingRole(r, 3, FI.formations_position3);
			m_PlayingRoles[4] = new PlayingRole(r, 4, FI.formations_position4);
			m_PlayingRoles[5] = new PlayingRole(r, 5, FI.formations_position5);
			m_PlayingRoles[6] = new PlayingRole(r, 6, FI.formations_position6);
			m_PlayingRoles[7] = new PlayingRole(r, 7, FI.formations_position7);
			m_PlayingRoles[8] = new PlayingRole(r, 8, FI.formations_position8);
			m_PlayingRoles[9] = new PlayingRole(r, 9, FI.formations_position9);
			m_PlayingRoles[10] = new PlayingRole(r, 10, FI.formations_position10);
			if (m_formationfullnameid != -1)
			{
				if (FifaEnvironment.Language != null)
				{
					m_formationfullname = FifaEnvironment.Language.GetFormationString(m_formationfullnameid);
				}
			}
			else
			{
				m_formationfullname = m_formationname;
			}
			if (m_formationfullname == null || m_formationfullname == string.Empty)
			{
				m_formationfullname = m_formationname;
			}
			if (m_teamid == -1)
			{
				_ = m_formationfullnameid;
				_ = -1;
			}
			if (m_teamid >= 0)
			{
				m_formationfullnameid = -1;
			}
		}

		public void Save(Record r)
		{
			r.IntField[FI.formations_formationid] = base.Id;
			r.StringField[FI.formations_formationname] = m_formationname;
			r.IntField[FI.formations_formationaudioid] = m_formationaudioid;
			if (m_Team != null)
			{
				r.IntField[FI.formations_teamid] = m_Team.Id;
				r.IntField[FI.formations_formationfullnameid] = -1;
			}
			else
			{
				r.IntField[FI.formations_teamid] = -1;
				if (m_formationname == m_formationfullname && m_formationname.Length == 5)
				{
					r.IntField[FI.formations_formationfullnameid] = -1;
				}
				else
				{
					r.IntField[FI.formations_formationfullnameid] = m_formationfullnameid;
				}
			}
			r.IntField[FI.formations_offensiverating] = m_offensiverating;
			r.FloatField[FI.formations_defenders] = m_defenders;
			r.FloatField[FI.formations_midfielders] = m_midfielders;
			r.FloatField[FI.formations_attackers] = m_attackers;
			for (int i = 0; i < 11; i++)
			{
				m_PlayingRoles[i].Save(r, i);
			}
			SaveLanguage();
		}

		public void SaveLanguage()
		{
			if (m_formationfullnameid != -1 && FifaEnvironment.Language != null)
			{
				FifaEnvironment.Language.SetFormationString(m_formationfullnameid, m_formationfullname);
			}
		}

		public static string RoleToString(ERole role)
		{
			if (FifaEnvironment.Language != null)
			{
				return FifaEnvironment.Language.GetRoleLongString((int)role);
			}
			return string.Empty;
		}

		public int ComputeDistance(Formation formation)
		{
			int num = 0;
			for (int i = 0; i < 11; i++)
			{
				num += Math.Abs(m_PlayingRoles[i].Role.RoleId - formation.PlayingRoles[i].Role.RoleId);
			}
			return num;
		}

		public int ComputeSimilarity(Formation formation)
		{
			int num = 0;
			int[] array = new int[11];
			bool[] array2 = new bool[11];
			for (int i = 0; i < 11; i++)
			{
				array[i] = 1;
				array2[i] = false;
			}
			for (int j = 0; j < 11; j++)
			{
				array[j] = 1;
				for (int k = 0; k < 11; k++)
				{
					if (m_PlayingRoles[j].Role.RoleId == formation.PlayingRoles[k].Role.RoleId)
					{
						array[j] = 0;
						array2[k] = true;
						break;
					}
				}
			}
			for (int l = 0; l < 11; l++)
			{
				if (array[l] == 0)
				{
					continue;
				}
				for (int m = 0; m < 11; m++)
				{
					if (!array2[m])
					{
						array2[m] = true;
						num += Math.Abs(m_PlayingRoles[l].Role.RoleId - formation.PlayingRoles[m].Role.RoleId);
					}
				}
			}
			return num;
		}

		public void LinkTeam(Team team)
		{
			if (team != null)
			{
				Team = team;
			}
		}

		public void LinkRoles(RoleList roleList)
		{
			for (int i = 0; i < 11; i++)
			{
				m_PlayingRoles[i].LinkRole(roleList);
			}
		}

		public bool IsRoleUsed(ERole eRole)
		{
			for (int i = 0; i < 11; i++)
			{
				_ = m_PlayingRoles[i].Role;
				if (m_PlayingRoles[i].Role != null && m_PlayingRoles[i].Role.RoleId == eRole)
				{
					return true;
				}
			}
			return false;
		}
	}
}
