namespace FifaLibrary
{
	public class NameFontList : IdArrayList
	{
		public NameFontList()
			: base(typeof(NameFont))
		{
		}

		public NameFontList(DbFile fifaDb, FifaFat fatFile)
			: base(typeof(NameFont))
		{
			Load(fifaDb, fatFile);
		}

		public void Load(DbFile fifaDb, FifaFat fatFile)
		{
			TableDescriptor tableDescriptor = fifaDb.Table[TI.teamkits].TableDescriptor;
			base.MinId = tableDescriptor.MinValues[FI.teamkits_jerseynamefonttype];
			base.MaxId = tableDescriptor.MaxValues[FI.teamkits_jerseynamefonttype];
			Clear();
			for (int i = base.MinId; i <= base.MaxId; i++)
			{
				string fileName = NameFont.NameFontFileName(i);
				if (fatFile.IsArchivedFilePresent(fileName) || fatFile.IsPhisycalFilePresent(fileName))
				{
					NameFont value = new NameFont(i);
					Add(value);
				}
			}
		}
	}
}
