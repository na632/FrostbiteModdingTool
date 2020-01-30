namespace FifaLibrary
{
	public class CareerCommonName : IdObject
	{
		private string m_Text1;

		private string m_Text2;

		private int m_firstname;

		private int m_lastname;

		private int m_groupid;

		private static PlayerNames s_PlayerNames;

		public string Text1
		{
			get
			{
				return m_Text1;
			}
			set
			{
				m_Text1 = value;
			}
		}

		public string Text2
		{
			get
			{
				return m_Text2;
			}
			set
			{
				m_Text2 = value;
			}
		}

		public int firstname
		{
			get
			{
				return m_firstname;
			}
			set
			{
				m_firstname = value;
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

		public CareerCommonName(Record r)
			: base(r.IntField[FI.career_commonnames_commonnameid])
		{
			Load(r);
		}

		public void Load(Record r)
		{
			m_firstname = r.IntField[FI.career_commonnames_firstname];
			m_lastname = r.IntField[FI.career_commonnames_lastname];
			m_groupid = r.IntField[FI.career_commonnames_groupid];
			if (s_PlayerNames == null || !s_PlayerNames.TryGetValue(m_firstname, out m_Text1, isUsed: true))
			{
				m_Text1 = string.Empty;
			}
			if (s_PlayerNames == null || !s_PlayerNames.TryGetValue(m_lastname, out m_Text2, isUsed: true))
			{
				m_Text2 = string.Empty;
			}
		}

		public void Save(Record r)
		{
			string name = null;
			s_PlayerNames.TryGetValue(m_firstname, out name, isUsed: true);
			if (name != m_Text1)
			{
				m_firstname = s_PlayerNames.GetKey(m_Text1);
			}
			s_PlayerNames.TryGetValue(m_lastname, out name, isUsed: true);
			if (name != m_Text2)
			{
				m_lastname = s_PlayerNames.GetKey(m_Text2);
			}
			r.IntField[FI.career_commonnames_commonnameid] = base.Id;
			r.IntField[FI.career_commonnames_firstname] = m_firstname;
			r.IntField[FI.career_commonnames_lastname] = m_lastname;
			r.IntField[FI.career_commonnames_groupid] = m_groupid;
		}
	}
}
