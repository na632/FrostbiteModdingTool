namespace FifaLibrary
{
	public class IdString : IdObject
	{
		private string m_String;

		public string String
		{
			get
			{
				return String;
			}
			set
			{
				String = value;
			}
		}

		public IdString(int id)
			: base(id)
		{
		}

		public IdString(int id, string value)
			: base(id)
		{
			m_String = value;
		}

		public override string ToString()
		{
			return m_String;
		}
	}
}
