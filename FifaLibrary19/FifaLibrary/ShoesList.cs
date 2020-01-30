using System;
using System.Collections;

namespace FifaLibrary
{
	public class ShoesList : IdArrayList
	{
		private bool m_HasGenericShoes;

		public static Shoes[] s_GenericShoes = new Shoes[23];

		public ShoesList(DbFile fifaDb, FifaFat fatFile)
			: base(typeof(Shoes))
		{
			Load(fifaDb, fatFile);
		}

		public void Load(DbFile fifaDb, FifaFat fatFile)
		{
			Table table = fifaDb.Table[TI.playerboots];
			TableDescriptor tableDescriptor = table.TableDescriptor;
			int minId = tableDescriptor.MinValues[FI.playerboots_shoetype];
			int maxId = tableDescriptor.MaxValues[FI.playerboots_shoetype];
			Load(table, minId, maxId);
		}

		public void Load(Table table, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			m_HasGenericShoes = false;
			Shoes[] array = new Shoes[table.NRecords];
			Clear();
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < table.NRecords; i++)
			{
				Shoes shoes = new Shoes(table.Records[i]);
				if (shoes.Id != 0 || !m_HasGenericShoes)
				{
					array[num++] = shoes;
					if (shoes.Id == 0)
					{
						m_HasGenericShoes = true;
						s_GenericShoes[num2++] = shoes;
					}
				}
				else if (num2 < s_GenericShoes.Length)
				{
					s_GenericShoes[num2++] = shoes;
				}
			}
			Shoes[] array2 = new Shoes[num];
			for (int j = 0; j < num; j++)
			{
				array2[j] = array[j];
			}
			AddRange(array2);
			SortId();
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.playerboots];
			table.ResizeRecords(Count - 1 + s_GenericShoes.Length);
			int num = 0;
			Shoes[] array = s_GenericShoes;
			foreach (Shoes shoes in array)
			{
				Record r = table.Records[num];
				num++;
				shoes?.SaveShoes(r);
			}
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Shoes shoes2 = (Shoes)enumerator.Current;
					if (shoes2.Id != 0)
					{
						Record r2 = table.Records[num];
						num++;
						shoes2.SaveShoes(r2);
					}
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
