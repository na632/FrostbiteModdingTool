namespace FifaLibrary
{
	public class Confederation : Compobj
	{
		private World m_World;

		public World World
		{
			get
			{
				return m_World;
			}
			set
			{
				m_World = value;
			}
		}

		public Confederation(int id, string typeString, string description, Compobj parentObj)
			: base(id, 1, typeString, description, parentObj)
		{
			m_Nations = new NationList();
			m_Trophies = new TrophyList();
		}

		public bool AddTrophy(int assetId)
		{
			if (assetId >= 1000)
			{
				return false;
			}
			_ = base.Trophies.Count;
			string typeString = "C" + assetId.ToString();
			string description = "TrophyName_Abbr15_" + assetId.ToString();
			Trophy trophy = new Trophy(FifaEnvironment.CompetitionObjects.GetNewId(), typeString, description, this);
			if (trophy == null)
			{
				return false;
			}
			base.Trophies.Add(trophy);
			FifaEnvironment.CompetitionObjects.Add(trophy);
			trophy.AddStage();
			return true;
		}

		public bool RemoveTrophy(int trophyId)
		{
			if (m_Trophies.Count < 1)
			{
				return false;
			}
			Trophy trophy = (Trophy)FifaEnvironment.CompetitionObjects.SearchId(trophyId);
			if (trophy == null)
			{
				return false;
			}
			m_Trophies.Remove(trophy);
			FifaEnvironment.CompetitionObjects.Remove(trophy);
			trophy.RemoveAllStages();
			return true;
		}

		public bool AddNation(Country country)
		{
			_ = base.Nations.Count;
			string typeString = country.DatabaseName.Substring(0, 4);
			string description = "NationName_" + country.Id.ToString();
			Nation nation = new Nation(FifaEnvironment.CompetitionObjects.GetNewId(), typeString, description, this);
			if (nation == null)
			{
				return false;
			}
			base.Nations.Add(nation);
			FifaEnvironment.CompetitionObjects.Add(nation);
			return true;
		}

		public bool RemoveNation()
		{
			if (m_Nations.Count < 1)
			{
				return false;
			}
			Nation nation = (Nation)m_Nations[m_Nations.Count - 1];
			m_Nations.Remove(nation);
			FifaEnvironment.CompetitionObjects.Remove(nation);
			nation.RemoveAllTrophies();
			return true;
		}

		public bool RemoveAllNations()
		{
			int count = m_Nations.Count;
			for (int i = 0; i < count; i++)
			{
				RemoveNation();
			}
			return true;
		}

		public override void Normalize()
		{
		}
	}
}
