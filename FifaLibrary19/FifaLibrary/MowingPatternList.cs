namespace FifaLibrary
{
	public class MowingPatternList : IdArrayList
	{
		public MowingPatternList()
			: base(typeof(MowingPattern))
		{
		}

		public MowingPatternList(DbFile fifaDb, FifaFat fatFile)
			: base(typeof(MowingPattern))
		{
			Load(fifaDb, fatFile);
		}

		public void Load(DbFile fifaDb, FifaFat fatFile)
		{
			TableDescriptor tableDescriptor = fifaDb.Table[TI.stadiums].TableDescriptor;
			base.MinId = tableDescriptor.MinValues[FI.stadiums_stadiummowpattern_code];
			base.MaxId = tableDescriptor.MaxValues[FI.stadiums_stadiummowpattern_code];
			Clear();
			for (int i = base.MinId; i <= base.MaxId; i++)
			{
				string fileName = MowingPattern.MowingPatternFileName(i);
				if (fatFile.IsArchivedFilePresent(fileName) || fatFile.IsPhisycalFilePresent(fileName))
				{
					MowingPattern value = new MowingPattern(i);
					Add(value);
				}
			}
		}
	}
}
