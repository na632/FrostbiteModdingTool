namespace FifaLibrary
{
	public class AdboardList : IdArrayList
	{
		public AdboardList()
			: base(typeof(Adboard))
		{
		}

		public AdboardList(DbFile fifaDb, FifaFat fatFile)
			: base(typeof(Adboard))
		{
			Load(fifaDb, fatFile);
		}

		public void Load(DbFile fifaDb, FifaFat fatFile)
		{
			TableDescriptor tableDescriptor = fifaDb.Table[TI.teams].TableDescriptor;
			base.MinId = tableDescriptor.MinValues[FI.teams_adboardid];
			base.MaxId = tableDescriptor.MaxValues[FI.teams_adboardid];
			Clear();
			for (int i = base.MinId; i <= base.MaxId; i++)
			{
				string fileName = Adboard.AdboardFileName(i);
				if (fatFile.IsArchivedFilePresent(fileName) || fatFile.IsPhisycalFilePresent(fileName))
				{
					Adboard value = new Adboard(i);
					Add(value);
				}
			}
		}
	}
}
