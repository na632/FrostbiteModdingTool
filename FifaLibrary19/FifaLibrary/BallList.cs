using System;
using System.Collections;

namespace FifaLibrary
{
	public class BallList : IdArrayList
	{
		public BallList()
			: base(typeof(Ball))
		{
		}

		public BallList(DbFile fifaDb, FifaFat fatFile)
			: base(typeof(Ball))
		{
			Load(fifaDb, fatFile);
		}

		public void Load(DbFile fifaDb, FifaFat fatFile)
		{
			Table table = fifaDb.Table[TI.teamballs];
			TableDescriptor tableDescriptor = table.TableDescriptor;
			int minId = tableDescriptor.MinValues[FI.teamballs_ballid];
			int maxId = tableDescriptor.MaxValues[FI.teamballs_ballid];
			Load(table, minId, maxId);
		}

		public void Load(Table table, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			Ball[] array = new Ball[table.NRecords];
			Clear();
			for (int i = 0; i < table.NRecords; i++)
			{
				array[i] = new Ball(table.Records[i]);
			}
			AddRange(array);
			SortId();
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.teamballs];
			table.ResizeRecords(Count);
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Ball obj = (Ball)enumerator.Current;
					Record r = table.Records[num];
					num++;
					obj.SaveBall(r);
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}
	}
}
