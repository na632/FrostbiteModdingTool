namespace FifaLibrary
{
	public class GkGlovesList : IdArrayList
	{
		public GkGlovesList()
			: base(typeof(GkGloves))
		{
		}

		public GkGlovesList(DbFile fifaDb, FifaFat fatFile)
			: base(typeof(GkGloves))
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
				string fileName = GkGloves.GkGlovesTextureFileName(i);
				if (fatFile.IsArchivedFilePresent(fileName) || fatFile.IsPhisycalFilePresent(fileName))
				{
					GkGloves idObject = new GkGloves(i);
					InsertId(idObject);
				}
			}
		}
	}
}
