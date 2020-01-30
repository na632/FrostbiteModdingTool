using System;
using System.Collections;

namespace FifaLibrary
{
	public class RoleList : IdArrayList
	{
		public RoleList()
			: base(typeof(Role))
		{
		}

		public RoleList(DbFile fifaDbFile)
			: base(typeof(Role))
		{
			Load(fifaDbFile);
		}

		public RoleList(Table boundingBoxTable)
			: base(typeof(Role))
		{
			Load(boundingBoxTable);
		}

		public void Load(DbFile fifaDbFile)
		{
			Table boundingBoxTable = FifaEnvironment.FifaDb.Table[TI.fieldpositionboundingboxes];
			Load(boundingBoxTable);
		}

		public void Load(Table boundingBoxTable)
		{
			base.MinId = 0;
			base.MaxId = 32;
			Clear();
			for (int i = 0; i < boundingBoxTable.NRecords; i++)
			{
				Role value = new Role(boundingBoxTable.Records[i]);
				Add(value);
			}
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = FifaEnvironment.FifaDb.Table[TI.fieldpositionboundingboxes];
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Role obj = (Role)enumerator.Current;
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
