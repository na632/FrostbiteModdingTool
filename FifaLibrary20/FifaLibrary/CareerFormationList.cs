namespace FifaLibrary
{
	public class CareerFormationList : FormationList
	{
		public CareerFormationList(CareerFile careerFile)
			: base(typeof(CareerFormation))
		{
			Load(careerFile);
		}

		public void Load(CareerFile careerFile)
		{
		}

		private void Load(Table formationsTable, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			Clear();
			for (int i = 0; i < formationsTable.NRecords; i++)
			{
				CareerFormation value = new CareerFormation(formationsTable.Records[i]);
				Add(value);
			}
		}

		private void AdditionalLoad(Table formationsTable)
		{
			for (int i = 0; i < formationsTable.NRecords; i++)
			{
				CareerFormation careerFormation = new CareerFormation(formationsTable.Records[i]);
				careerFormation.IsInCareer = true;
				CareerFormation careerFormation2 = (CareerFormation)SearchId(careerFormation);
				if (careerFormation2 != null)
				{
					RemoveId(careerFormation2);
				}
				InsertId(careerFormation);
			}
		}

		public void Save(DbFile dbFile, CareerFile careerFile)
		{
		}
	}
}
