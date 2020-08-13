using System;
using System.Collections;
using System.Collections.Generic;

namespace FrostySdk
{
	public class DbObject
	{
		internal IDictionary<string, object> hash;

		internal IList<object> list;

		public int Count
		{
			get
			{
				if (list == null)
				{
					return 0;
				}
				return list.Count;
			}
		}

		public object this[int id]
		{
			get
			{
				if (id >= list.Count)
				{
					return null;
				}
				return list[id];
			}
		}

		public DbObject(bool bObject = true)
		{
			if (bObject)
			{
				hash = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				list = new List<object>();
			}
		}

		internal DbObject(object inVal)
		{
			if (inVal.GetType() == typeof(List<object>))
			{
				list = (IList<object>)inVal;
			}
			else if (inVal.GetType() == typeof(Dictionary<string, object>))
			{
				hash = (IDictionary<string, object>)inVal;
			}
		}

		public static DbObject CreateObject()
		{
			return new DbObject();
		}

		public static DbObject CreateList()
		{
			return new DbObject(bObject: false);
		}

		public T GetValue<T>(string name, T defaultValue = default(T))
		{
			if (hash == null || !hash.ContainsKey(name))
			{
				return defaultValue;
			}
			if (hash[name] is T)
			{
				return (T)hash[name];
			}
			return (T)Convert.ChangeType(hash[name], typeof(T));
		}

		public void SetValue(string name, object newValue)
		{
			if (newValue != null)
			{
				if (!hash.ContainsKey(name))
				{
					AddValue(name, newValue);
				}
				else
				{
					hash[name] = newValue;
				}
			}
		}

		public void AddValue(string name, object value)
		{
			if (!hash.ContainsKey(name))
			{
				hash.Add(name, SanitizeData(value));
			}
		}

		public void RemoveValue(string name)
		{
			if (hash.ContainsKey(name))
			{
				hash.Remove(name);
			}
		}

		public bool HasValue(string name)
		{
			return hash.ContainsKey(name);
		}

		public IEnumerator GetEnumerator()
		{
			int count = (list != null) ? list.Count : 0;
			for (int i = 0; i < count; i++)
			{
				yield return list[i];
			}
		}

		public void Add(object value)
		{
			if (list != null)
			{
				list.Add(SanitizeData(value));
			}
		}

		public void SetAt(int id, object value)
		{
			if (list != null && id < list.Count)
			{
				list[id] = SanitizeData(value);
			}
		}

		public void Insert(int id, object value)
		{
			list.Insert(id, value);
		}

		public void RemoveAt(int id)
		{
			if (list != null && id < list.Count)
			{
				list.RemoveAt(id);
			}
		}

		public int FindIndex(Predicate<object> match)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (match(list[i]))
				{
					return i;
				}
			}
			return -1;
		}

		public T Find<T>(Predicate<object> match)
		{
			if (list == null)
			{
				return default(T);
			}
			return (T)((List<object>)list).Find(match);
		}

		private object SanitizeData(object value)
		{
			if (value.GetType() == typeof(uint))
			{
				value = (int)(uint)value;
			}
			else if (value.GetType() == typeof(ulong))
			{
				value = (long)(ulong)value;
			}
			return value;
		}
	}
}
