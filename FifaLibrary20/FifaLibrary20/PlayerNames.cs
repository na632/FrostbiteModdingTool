using System;
using System.Collections;

namespace FifaLibrary
{
	public class PlayerNames : IdArrayList
	{
		private static char[] c_NoLetters = new char[4]
		{
			' ',
			'.',
			'-',
			'\''
		};

		public PlayerNames(DbFile fifaDbFile)
			: base(typeof(PlayerName))
		{
			Load(fifaDbFile);
		}

		public PlayerNames(Table playerNamesTable)
			: base(typeof(PlayerName))
		{
			int minId = 0;
			int num = playerNamesTable.TableDescriptor.MaxValues[FI.playernames_nameid];
			if (num < 32767)
			{
				num = 32767;
			}
			Load(playerNamesTable, minId, num);
		}

		public PlayerNames()
			: base(typeof(PlayerName))
		{
		}

		public void Load(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.playernames];
			int minId = 0;
			int num = table.TableDescriptor.MaxValues[FI.playernames_nameid];
			if (num < 32767)
			{
				num = 32767;
			}
			Load(table, minId, num);
		}

		public void Load(Table table, int minId, int maxId)
		{
			base.MinId = minId;
			base.MaxId = maxId;
			PlayerName[] array = new PlayerName[table.NRecords];
			Clear();
			for (int i = 0; i < table.NRecords; i++)
			{
				array[i] = new PlayerName(table.Records[i]);
				string text = Normalize(array[i].Text);
				if (array[i].Text != text)
				{
					array[i].Text = text;
				}
			}
			AddRange(array);
			SortId();
		}

		public void Save(DbFile fifaDbFile)
		{
			Table table = fifaDbFile.Table[TI.playernames];
			table.ResizeRecords(Count);
			int num = 0;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					PlayerName playerName = (PlayerName)enumerator.Current;
					if (playerName.IsUsed || playerName.IsOriginal)
					{
						Record r = table.Records[num];
						num++;
						playerName.SavePlayerName(r);
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
			table.ResizeRecords(num);
		}

		public bool TryGetValue(int id, out string name, bool isUsed)
		{
			PlayerName playerName = (PlayerName)SearchId(id);
			if (playerName != null)
			{
				name = playerName.Text;
				playerName.IsUsed = isUsed;
				return true;
			}
			name = null;
			return false;
		}

		public bool TryGetValue(int id, out string name, out int commentaryid, bool isUsed)
		{
			PlayerName playerName = (PlayerName)SearchId(id);
			if (playerName != null)
			{
				name = playerName.Text;
				commentaryid = playerName.CommentaryId;
				playerName.IsUsed = isUsed;
				return true;
			}
			name = null;
			commentaryid = 900000;
			return false;
		}

		public bool SetCommentaryId(int nameid, int commentaryid)
		{
			PlayerName playerName = (PlayerName)SearchId(nameid);
			if (playerName != null)
			{
				playerName.CommentaryId = commentaryid;
				return true;
			}
			return false;
		}

		public void ClearUsedFlags()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((PlayerName)enumerator.Current).IsUsed = false;
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

		public void SetUsedFlags()
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					((PlayerName)enumerator.Current).IsUsed = true;
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

		public int GetCommentaryIdFromName(string text)
		{
			string b = Normalize(text);
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					PlayerName playerName = (PlayerName)enumerator.Current;
					if (playerName.Text == b)
					{
						return playerName.CommentaryId;
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
			return 900000;
		}

		public PlayerName SearchName(string text)
		{
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					PlayerName playerName = (PlayerName)enumerator.Current;
					if (playerName.Text == text)
					{
						return playerName;
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
			return null;
		}

		public int GetKey(string text, int commentaryId)
		{
			string text2 = Normalize(text);
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					PlayerName playerName = (PlayerName)enumerator.Current;
					if (playerName.Text == text2)
					{
						playerName.IsUsed = true;
						if (playerName.CommentaryId != commentaryId && commentaryId >= 0)
						{
							playerName.CommentaryId = commentaryId;
						}
						return playerName.Id;
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
			int newId = GetNewId();
			if (newId >= 0)
			{
				PlayerName playerName2 = new PlayerName(newId, text2, isUsed: true);
				if (commentaryId >= 0)
				{
					playerName2.CommentaryId = commentaryId;
				}
				else
				{
					playerName2.CommentaryId = 900000;
				}
				InsertId(playerName2);
				return newId;
			}
			return 0;
		}

		public int GetKey(string text)
		{
			string text2 = Normalize(text);
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					PlayerName playerName = (PlayerName)enumerator.Current;
					if (playerName.Text == text2)
					{
						playerName.IsUsed = true;
						return playerName.Id;
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
			int newId = GetNewId();
			if (newId >= 0)
			{
				PlayerName idObject = new PlayerName(newId, text2, isUsed: true);
				InsertId(idObject);
				return newId;
			}
			return 0;
		}

		public static string Normalize(string text)
		{
			if (text == null)
			{
				return string.Empty;
			}
			if (text.Length <= 0)
			{
				return text;
			}
			string text2 = text;
			if (text2 == text2.ToUpper() && text2.IndexOfAny(c_NoLetters) < 0 && text2.Length > 3)
			{
				text2 = text2.Substring(0, 1).ToUpper() + text2.Substring(1, text2.Length - 1).ToLower();
			}
			if (text2.Length == 0)
			{
				return string.Empty;
			}
			if (text2 == text2.ToLower())
			{
				text2 = text2.Substring(0, 1).ToUpper() + text2.Substring(1, text2.Length - 1);
			}
			return text2;
		}
	}
}
