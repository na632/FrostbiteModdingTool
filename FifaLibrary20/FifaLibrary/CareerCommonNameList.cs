using System;
using System.Collections;

namespace FifaLibrary
{
	public class CareerCommonNameList : IdArrayList
	{
		public CareerCommonNameList(DbFile fifaDbFile)
			: base(typeof(CareerCommonName))
		{
			Load(fifaDbFile);
		}

		public CareerCommonNameList(Table careerCommonNamesTable)
			: base(typeof(CareerCommonName))
		{
			Load(careerCommonNamesTable);
		}

		public void Load(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.career_commonnames];
			Load(table);
		}

		public void Load(Table table)
		{
			base.MinId = table.TableDescriptor.MinValues[FI.career_commonnames_commonnameid];
			base.MaxId = table.TableDescriptor.MaxValues[FI.career_commonnames_commonnameid];
			CareerCommonName[] array = new CareerCommonName[table.NRecords];
			Clear();
			for (int i = 0; i < table.NRecords; i++)
			{
				array[i] = new CareerCommonName(table.Records[i]);
			}
			AddRange(array);
			SortId();
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.career_commonnames];
			table.ResizeRecords(Count);
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					CareerCommonName obj = (CareerCommonName)enumerator.Current;
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
