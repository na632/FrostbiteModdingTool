using System;
using System.Collections;

namespace FifaLibrary
{
	public class CareerLastNameList : IdArrayList
	{
		public CareerLastNameList(DbFile fifaDbFile)
			: base(typeof(CareerLastName))
		{
			Load(fifaDbFile);
		}

		public CareerLastNameList(Table careerLastNamesTable, PlayerNames playerNames)
			: base(typeof(CareerLastName))
		{
			Load(careerLastNamesTable);
		}

		public void Load(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.career_lastnames];
			Load(table);
		}

		public void Load(Table table)
		{
			base.MinId = table.TableDescriptor.MinValues[FI.career_lastnames_lastnameid];
			base.MaxId = table.TableDescriptor.MaxValues[FI.career_lastnames_lastnameid];
			CareerLastName[] array = new CareerLastName[table.NRecords];
			Clear();
			for (int i = 0; i < table.NRecords; i++)
			{
				array[i] = new CareerLastName(table.Records[i]);
			}
			AddRange(array);
			SortId();
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.career_lastnames];
			table.ResizeRecords(Count);
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CareerLastName obj = (CareerLastName)enumerator.Current;
					Record r = table.Records[num];
					num++;
					obj.Save(r);
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
