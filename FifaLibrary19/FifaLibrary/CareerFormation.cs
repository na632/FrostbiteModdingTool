namespace FifaLibrary
{
	public class CareerFormation : Formation
	{
		private bool m_IsInCareer;

		public bool IsInCareer
		{
			get
			{
				return m_IsInCareer;
			}
			set
			{
				m_IsInCareer = value;
			}
		}

		public CareerFormation(Record r)
			: base(r)
		{
		}

		public CareerFormation(int formationid)
			: base(formationid)
		{
		}
	}
}
