using System;
using System.Drawing;

namespace FifaLibrary
{
	public class Role : IdObject
	{
		private int m_Xmin;

		private int m_Xmax;

		private int m_Ymin;

		private int m_Ymax;

		public ERole RoleId
		{
			get
			{
				return (ERole)base.Id;
			}
			set
			{
				base.Id = (int)value;
			}
		}

		public int Xmin
		{
			get
			{
				return m_Xmin;
			}
			set
			{
				m_Xmin = value;
			}
		}

		public int Xmax
		{
			get
			{
				return m_Xmax;
			}
			set
			{
				m_Xmax = value;
			}
		}

		public int Ymin
		{
			get
			{
				return m_Ymin;
			}
			set
			{
				m_Ymin = value;
			}
		}

		public int Ymax
		{
			get
			{
				return m_Ymax;
			}
			set
			{
				m_Ymax = value;
			}
		}

		public Role(ERole eRole)
			: base((int)eRole)
		{
		}

		public Role(int roleId)
			: base(roleId)
		{
		}

		public override string ToString()
		{
			string str = string.Empty;
			if (FifaEnvironment.Language != null)
			{
				str = FifaEnvironment.Language.GetRoleShortString(base.Id) + " - ";
			}
			switch (RoleId)
			{
			case ERole.Goalkeeper:
				return str + "Goalkeeper";
			case ERole.Sweeper:
				return str + "Sweeper";
			case ERole.Right_Wing_Back:
				return str + "Right Wing Back";
			case ERole.Right_Back:
				return str + "Right Back";
			case ERole.Right_Central_Back:
				return str + "Right Central Back";
			case ERole.Central_Back:
				return str + "Central Back";
			case ERole.Left_Central_Back:
				return str + "Left Central Back";
			case ERole.Left_Back:
				return str + "Left Back";
			case ERole.Left_Wing_Back:
				return str + "Left Wing Back";
			case ERole.Right_Defensive_Midfielder:
				return str + "Right Defensive Midfielder";
			case ERole.Central_Defensive_Midfielder:
				return str + "Central Defensive Midfielder";
			case ERole.Left_Defensive_Midfielder:
				return str + "Left Defensive Midfielder";
			case ERole.Right_Midfielder:
				return str + "Right Midfielder";
			case ERole.Right_Central_Midfielder:
				return str + "Right Central Midfielder";
			case ERole.Central_Midfielder:
				return str + "Central Midfielder";
			case ERole.Left_Central_Midfielder:
				return str + "Left Central Midfielder";
			case ERole.Left_Midfielder:
				return str + "Left Midfielder";
			case ERole.Right_Advanced_Midfielder:
				return str + "Right Advanced Midfielder";
			case ERole.Central_Advanced_Midfielder:
				return str + "Central Advanced Midfielder";
			case ERole.Left_Advanced_Midfielder:
				return str + "Left Advanced Midfielder";
			case ERole.Right_Forward:
				return str + "Right Forward";
			case ERole.Central_Forward:
				return str + "Central Forward";
			case ERole.Left_Forward:
				return str + "Left Forward";
			case ERole.Right_Wing:
				return str + "Right Wing";
			case ERole.Right_Striker:
				return str + "Right Striker";
			case ERole.Central_Striker:
				return str + "Central Striker";
			case ERole.Left_Striker:
				return str + "Left Striker";
			case ERole.Left_Wing:
				return str + "Left Wing";
			case ERole.Substitute:
				return str + "Substitute";
			case ERole.Tribune:
				return str + "Tribune";
			default:
				return string.Empty;
			}
		}

		public string ToShortString()
		{
			if (FifaEnvironment.Language != null)
			{
				return FifaEnvironment.Language.GetRoleShortString(base.Id);
			}
			return string.Empty;
		}

		public string ToLongString()
		{
			if (FifaEnvironment.Language != null)
			{
				return FifaEnvironment.Language.GetRoleLongString(base.Id);
			}
			return string.Empty;
		}

		public void SetShortString(string shortName)
		{
			if (FifaEnvironment.Language != null)
			{
				FifaEnvironment.Language.SetRoleShortString(base.Id, shortName);
			}
		}

		public Role(Record r)
			: base(r.IntField[r.TableDescriptor.GetFieldIndex("positionid")])
		{
			Load(r);
		}

		public void Load(Record r)
		{
			float val = r.FloatField[FI.fieldpositionboundingboxes_pointx0];
			float val2 = r.FloatField[FI.fieldpositionboundingboxes_pointx1];
			float val3 = r.FloatField[FI.fieldpositionboundingboxes_pointx2];
			float val4 = r.FloatField[FI.fieldpositionboundingboxes_pointx3];
			float val5 = r.FloatField[FI.fieldpositionboundingboxes_pointy0];
			float val6 = r.FloatField[FI.fieldpositionboundingboxes_pointy1];
			float val7 = r.FloatField[FI.fieldpositionboundingboxes_pointy2];
			float val8 = r.FloatField[FI.fieldpositionboundingboxes_pointy3];
			m_Xmin = Convert.ToInt32(Math.Min(Math.Min(val, val2), Math.Min(val3, val4)) * 100f);
			m_Xmax = Convert.ToInt32(Math.Max(Math.Max(val, val2), Math.Max(val3, val4)) * 100f);
			m_Ymin = Convert.ToInt32(Math.Min(Math.Min(val5, val6), Math.Min(val7, val8)) * 100f);
			m_Ymax = Convert.ToInt32(Math.Max(Math.Max(val5, val6), Math.Max(val7, val8)) * 100f);
		}

		public void Save(Record r)
		{
			r.IntField[FI.fieldpositionboundingboxes_positionid] = base.Id;
			r.FloatField[FI.fieldpositionboundingboxes_pointx0] = (float)m_Xmin / 100f;
			r.FloatField[FI.fieldpositionboundingboxes_pointy0] = (float)m_Ymin / 100f;
			r.FloatField[FI.fieldpositionboundingboxes_pointx1] = (float)m_Xmin / 100f;
			r.FloatField[FI.fieldpositionboundingboxes_pointy1] = (float)m_Ymax / 100f;
			r.FloatField[FI.fieldpositionboundingboxes_pointx2] = (float)m_Xmax / 100f;
			r.FloatField[FI.fieldpositionboundingboxes_pointy2] = (float)m_Ymax / 100f;
			r.FloatField[FI.fieldpositionboundingboxes_pointx3] = (float)m_Xmax / 100f;
			r.FloatField[FI.fieldpositionboundingboxes_pointy3] = (float)m_Ymin / 100f;
		}

		public Point GetCenter()
		{
			return new Point((m_Xmax + m_Xmin) / 2, (m_Ymax + m_Ymin) / 2);
		}

		public static ERole ConvertToERole(string s)
		{
			for (int i = 0; i < 29; i++)
			{
				ERole result = (ERole)i;
				if (result.ToString() == s)
				{
					return result;
				}
			}
			return ERole.Tribune;
		}
	}
}
