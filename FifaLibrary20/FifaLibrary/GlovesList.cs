namespace FifaLibrary
{
	public class GlovesList : IdArrayList
	{
		public GlovesList(DbFile fifaDb, FifaFat fatFile)
			: base(typeof(Gloves))
		{
			Load(fifaDb, fatFile);
		}

		public void Load(DbFile fifaDb, FifaFat fatFile)
		{
			TableDescriptor tableDescriptor = fifaDb.Table[TI.players].TableDescriptor;
			base.MinId = tableDescriptor.MinValues[FI.players_gkglovetypecode];
			base.MaxId = tableDescriptor.MaxValues[FI.players_gkglovetypecode];
			Clear();
			for (int i = base.MinId; i <= base.MaxId; i++)
			{
				string fileName = Gloves.GlovesFileName(i);
				if (fatFile.IsArchivedFilePresent(fileName) || fatFile.IsPhisycalFilePresent(fileName))
				{
					Gloves value = new Gloves(i);
					Add(value);
				}
			}
		}
	}
}
