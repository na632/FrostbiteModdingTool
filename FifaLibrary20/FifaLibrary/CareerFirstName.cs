namespace FifaLibrary
{
	public class CareerFirstName : IdObject
	{
		private string m_Text;

		private int m_firstname;

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

		public CareerFirstName(Record r)
			: base(r.IntField[FI.career_firstnames_firstnameid])
		{
			Load(r);
		}

		public void Load(Record r)
		{
			m_firstname = r.IntField[FI.career_firstnames_firstname];
			m_groupid = r.IntField[FI.career_firstnames_groupid];
			if (s_PlayerNames == null || !s_PlayerNames.TryGetValue(m_firstname, out m_Text, isUsed: true))
			{
				m_Text = string.Empty;
			}
		}

		public void Save(Record r)
		{
			string name = null;
			s_PlayerNames.TryGetValue(m_firstname, out name, isUsed: true);
			if (name != m_Text)
			{
				m_firstname = s_PlayerNames.GetKey(m_Text);
			}
			r.IntField[FI.career_firstnames_firstnameid] = base.Id;
			r.IntField[FI.career_firstnames_firstname] = m_firstname;
			r.IntField[FI.career_firstnames_groupid] = m_groupid;
		}
	}
}
