namespace FifaLibrary
{
	public class NetList : IdArrayList
	{
		public NetList()
			: base(typeof(Net))
		{
		}

		public NetList(DbFile fifaDb, FifaFat fatFile)
			: base(typeof(Net))
		{
			Load(fifaDb, fatFile);
		}

		public void Load(DbFile fifaDb, FifaFat fatFile)
		{
			TableDescriptor tableDescriptor = fifaDb.Table[TI.stadiums].TableDescriptor;
			base.MinId = tableDescriptor.MinValues[FI.stadiums_stadiumgoalnetstyle];
			base.MaxId = tableDescriptor.MaxValues[FI.stadiums_stadiumgoalnetstyle];
			Clear();
			for (int i = base.MinId; i <= base.MaxId; i++)
			{
				string fileName = Net.NetFileName(i);
				if (fatFile.IsArchivedFilePresent(fileName) || fatFile.IsPhisycalFilePresent(fileName))
				{
					Net value = new Net(i);
					Add(value);
				}
			}
		}
	}
}
