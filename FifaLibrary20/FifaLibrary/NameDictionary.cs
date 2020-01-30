using System.Collections.Generic;
using System.Text;

namespace FifaLibrary
{
	public class NameDictionary : Dictionary<int, string>
	{
		public NameDictionary(DbFile fifaDbFile)
		{
			Table commentaryNamesTable = fifaDbFile.Table[TI.commentarynames];
			Load(commentaryNamesTable);
			Table playernamesTable = fifaDbFile.Table[TI.playernames];
			FillFromPlayernames(playernamesTable);
		}

		public void Load(Table commentaryNamesTable)
		{
			Clear();
			for (int i = 0; i < commentaryNamesTable.NRecords; i++)
			{
				Record record = commentaryNamesTable.Records[i];
				int key = record.IntField[FI.commentarynames_commentaryid];
				if (!ContainsKey(key))
				{
					string text = record.CompressedString[FI.commentarynames_commentarystring];
					string @string = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(text));
					if (@string.Length > 0)
					{
						_ = @string.ToUpper()[0];
					}
					Add(key, text);
				}
			}
		}

		public void FillFromPlayernames(Table playernamesTable)
		{
			for (int i = 0; i < playernamesTable.NRecords; i++)
			{
				Record record = playernamesTable.Records[i];
				int num = record.IntField[FI.playernames_commentaryid];
				if (num != 900000 && !ContainsKey(num))
				{
					string s = record.CompressedString[FI.playernames_name];
					string @string = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(s));
					if (@string.Length > 0)
					{
						_ = @string.ToUpper()[0];
					}
					Add(num, @string);
				}
			}
		}

		public void Save(DbFile fifaDbFile)
		{
			Table commentaryNamesTable = fifaDbFile.Table[TI.commentarynames];
			Save(commentaryNamesTable);
		}

		public void Save(Table commentaryNamesTable)
		{
			commentaryNamesTable.ResizeRecords(base.Count);
			commentaryNamesTable.NValidRecords = base.Count;
			int num = 0;
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<int, string> current = enumerator.Current;
					Record obj = commentaryNamesTable.Records[num];
					num++;
					string value = current.Value;
					char c = 'Z';
					string @string = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(value));
					if (@string.Length > 0)
					{
						c = @string.ToUpper()[0];
					}
					obj.IntField[FI.commentarynames_commentarystartingletter] = c - 65 + 1;
					obj.IntField[FI.commentarynames_commentaryid] = current.Key;
					obj.IntField[FI.commentarynames_commentarypreview] = 1;
					obj.CompressedString[FI.commentarynames_commentarystring] = value;
				}
			}
		}
	}
}
