namespace FifaLibrary
{
	public class NumberFontList : IdArrayList
	{
		public NumberFontList()
			: base(typeof(NumberFont))
		{
		}

		public NumberFontList(DbFile fifaDb, FifaFat fatFile)
			: base(typeof(NumberFont))
		{
			Load(fifaDb, fatFile);
		}

		public void Load(DbFile fifaDb, FifaFat fatFile)
		{
			TableDescriptor tableDescriptor = fifaDb.Table[TI.teamkits].TableDescriptor;
			int num = 1;
			int num2 = tableDescriptor.MaxValues[FI.teamkits_numberfonttype];
			int num3 = 0;
			int num4 = 19;
			base.MinId = num * (num4 + 1) + num3;
			base.MaxId = num2 * (num4 + 1) + num4;
			Clear();
			for (int i = num; i <= num2; i++)
			{
				for (int j = num3; j <= num4; j++)
				{
					string fileName = NumberFont.NumberFontFileName(i, j);
					if (fatFile.IsArchivedFilePresent(fileName) || fatFile.IsPhisycalFilePresent(fileName))
					{
						NumberFont value = new NumberFont(i * (num4 + 1) + j);
						Add(value);
					}
				}
			}
		}

		public new NumberFont CreateNewId(int proposedId)
		{
			int num = -1;
			if (SearchId(proposedId) == null)
			{
				num = proposedId;
			}
			if (num < 0)
			{
				for (int i = 20 + proposedId % 20; i <= base.MaxId; i += 20)
				{
					if (SearchId(i) == null)
					{
						num = i;
						break;
					}
				}
			}
			if (num < 0)
			{
				return null;
			}
			NumberFont numberFont = new NumberFont(num);
			InsertId(numberFont);
			return numberFont;
		}
	}
}
