namespace FifaLibrary
{
	public class PlayerName : IdObject
	{
		private string m_Text;

		private bool m_IsOriginal;

		private bool m_IsUsed;

		private int m_CommentaryId;

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

		public bool IsOriginal
		{
			get
			{
				return m_IsOriginal;
			}
			set
			{
				m_IsOriginal = value;
			}
		}

		public bool IsUsed
		{
			get
			{
				return m_IsUsed;
			}
			set
			{
				m_IsUsed = value;
			}
		}

		public int CommentaryId
		{
			get
			{
				return m_CommentaryId;
			}
			set
			{
				m_CommentaryId = value;
			}
		}

		public PlayerName(Record r)
			: base(r.IntField[FI.playernames_nameid])
		{
			Load(r);
		}

		public PlayerName(int id, string text, bool isUsed)
		{
			base.Id = id;
			m_Text = text;
			m_IsUsed = isUsed;
			m_IsOriginal = false;
			m_CommentaryId = 900000;
		}

		public void Load(Record r)
		{
			m_Text = r.CompressedString[FI.playernames_name];
			m_CommentaryId = r.IntField[FI.playernames_commentaryid];
			m_IsUsed = false;
			m_IsOriginal = false;
		}

		public void SavePlayerName(Record r)
		{
			r.IntField[FI.playernames_nameid] = base.Id;
			r.CompressedString[FI.playernames_name] = Text;
			r.IntField[FI.playernames_commentaryid] = m_CommentaryId;
		}
	}
}
