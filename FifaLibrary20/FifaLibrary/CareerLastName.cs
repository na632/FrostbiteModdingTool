namespace FifaLibrary
{
	public class CareerLastName : IdObject
	{
		private string m_Text;

		private int m_lastname;

		private int m_groupid;

		private static PlayerNames s_PlayerNames;

		public string Text
		{
			get
			{
				return m_Text;
			}
			set
			{
				m_Text = value;
			}
		}

		public int lastname
		{
			get
			{
				return m_lastname;
			}
			set
			{
				m_lastname = value;
			}
		}

		public int groupid
		{
			get
			{
				return m_groupid;
			}
			set
			{
				m_groupid = value;
			}
		}

		public static PlayerNames PlayerNames
		{
			get
			{
				return s_PlayerNames;
			}
			set
			{
				s_PlayerNames = value;
			}
		}

		public CareerLastName(Record r)
			: base(r.IntField[FI.career_lastnames_lastnameid])
		{
			Load(r);
		}

		public void Load(Record r)
		{
			m_lastname = r.IntField[FI.career_lastnames_lastname];
			m_groupid = r.IntField[FI.career_firstnames_groupid];
			if (s_PlayerNames == null || !s_PlayerNames.TryGetValue(m_lastname, out m_Text, isUsed: true))
			{
				m_Text = string.Empty;
			}
		}

		public void Save(Record r)
		{
			string name = null;
			s_PlayerNames.TryGetValue(m_lastname, out name, isUsed: true);
			if (name != m_Text)
			{
				m_lastname = s_PlayerNames.GetKey(m_Text);
			}
			r.IntField[FI.career_lastnames_lastnameid] = base.Id;
			r.IntField[FI.career_lastnames_lastname] = m_lastname;
			r.IntField[FI.career_lastnames_groupid] = m_groupid;
		}
	}
}
