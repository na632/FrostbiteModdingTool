using System;
using System.Collections;

namespace FifaLibrary
{
	public class CareerFirstNameList : IdArrayList
	{
		public CareerFirstNameList(DbFile fifaDbFile)
			: base(typeof(CareerFirstName))
		{
			Load(fifaDbFile);
		}

		public CareerFirstNameList(Table careerFirstNamesTable)
			: base(typeof(CareerFirstName))
		{
			Load(careerFirstNamesTable);
		}

		public void Load(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.career_firstnames];
			Load(table);
		}

		public void Load(Table table)
		{
			base.MinId = table.TableDescriptor.MinValues[FI.career_firstnames_firstnameid];
			base.MaxId = table.TableDescriptor.MaxValues[FI.career_firstnames_firstnameid];
			CareerFirstName[] array = new CareerFirstName[table.NRecords];
			Clear();
			for (int i = 0; i < table.NRecords; i++)
			{
				array[i] = new CareerFirstName(table.Records[i]);
			}
			AddRange(array);
			SortId();
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.career_firstnames];
			table.ResizeRecords(Count);
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CareerFirstName obj = (CareerFirstName)enumerator.Current;
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
