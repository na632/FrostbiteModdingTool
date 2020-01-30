using System;
using System.Collections;

namespace FifaLibrary
{
	public class IdArrayList : ArrayList
	{
		private int m_MinId;

		private int m_MaxId;

		private Type m_Type;

		private IdObjectComparer m_Comparer = new IdObjectComparer();

		public int MinId
		{
			get
			{
				return m_MinId;
			}
			set
			{
				m_MinId = value;
			}
		}

		public int MaxId
		{
			get
			{
				return m_MaxId;
			}
			set
			{
				m_MaxId = value;
			}
		}

		public Type ObjectType
		{
			get
			{
				return m_Type;
			}
			set
			{
				m_Type = value;
			}
		}

		public IdArrayList(Type type, int minId, int maxId)
		{
			m_Type = type;
			m_MinId = minId;
			m_MaxId = maxId;
		}

		public IdArrayList(Type type)
		{
			m_Type = type;
			m_MinId = 0;
			m_MaxId = -1;
		}

		public IdArrayList(int minId, int maxId)
		{
			m_Type = typeof(IdObject);
			m_MinId = minId;
			m_MaxId = maxId;
		}

		public IdArrayList()
		{
			m_Type = typeof(IdObject);
			m_MinId = -1;
			m_MaxId = 0;
		}

		public bool InsertId(IdObject idObject)
		{
			if (idObject == null)
			{
				return false;
			}
			int num = BinarySearch(idObject, m_Comparer);
			if (num < 0)
			{
				num = ~num;
				Insert(num, idObject);
				return true;
			}
			return false;
		}

		public bool RemoveId(IdObject idObject)
		{
			int num = BinarySearch(idObject, m_Comparer);
			if (num < 0 && Count < 10000)
			{
				Sort(m_Comparer);
				num = BinarySearch(idObject, m_Comparer);
			}
			if (num >= 0)
			{
				RemoveAt(num);
				return true;
			}
			return false;
		}

		public bool RemoveId(int id)
		{
			IdObject idObject = new IdObject(id);
			return RemoveId(idObject);
		}

		public void SortId()
		{
			Sort(m_Comparer);
		}

		public bool DeleteId(IdObject idObject)
		{
			if (RemoveId(idObject))
			{
				return idObject.Delete();
			}
			return false;
		}

		public IdObject SearchId(int id)
		{
			IdObject idObject = new IdObject(id);
			return SearchId(idObject);
		}

		public IdObject SearchId(IdObject idObject)
		{
			int num = BinarySearch(idObject, m_Comparer);
			if (num >= 0)
			{
				return (IdObject)this[num];
			}
			return null;
		}

		public bool ChangeId(IdObject idObject, int newId)
		{
			int id = idObject.Id;
			if (!RemoveId(idObject))
			{
				return false;
			}
			idObject.Id = newId;
			if (!InsertId(idObject))
			{
				idObject.Id = id;
				InsertId(idObject);
				return false;
			}
			return true;
		}

		public virtual int GetNewId()
		{
			int result = -1;
			for (int i = m_MinId; i <= m_MaxId; i++)
			{
				if (SearchId(i) == null)
				{
					result = i;
					break;
				}
			}
			return result;
		}

		public virtual int GetNextId(int minId)
		{
			int result = -1;
			for (int i = minId; i <= m_MaxId; i++)
			{
				if (SearchId(i) == null)
				{
					result = i;
					break;
				}
			}
			return result;
		}

		public IdObject CreateNewId()
		{
			int newId = GetNewId();
			if (newId < 0)
			{
				return null;
			}
			IdObject idObject = IdObject.Create(m_Type, newId);
			InsertId(idObject);
			return idObject;
		}

		public IdObject CreateNewId(int newId)
		{
			if (SearchId(newId) != null)
			{
				return null;
			}
			IdObject idObject = IdObject.Create(m_Type, newId);
			InsertId(idObject);
			return idObject;
		}

		public IdObject CloneId(IdObject srcIdObject)
		{
			int newId = GetNewId();
			if (newId < 0)
			{
				return null;
			}
			IdObject idObject = srcIdObject.Clone(newId);
			InsertId(idObject);
			return idObject;
		}

		public IdObject CloneId(int srcId)
		{
			IdObject idObject = SearchId(srcId);
			if (idObject == null)
			{
				return null;
			}
			return CloneId(idObject);
		}

		public IdObject CloneId(IdObject srcIdObject, int newId)
		{
			IdObject idObject = srcIdObject.Clone(newId);
			InsertId(idObject);
			return idObject;
		}

		public IdObject CloneId(IdObject srcIdObject, IdObject newObject)
		{
			IdObject idObject = srcIdObject.Clone(newObject.Id);
			RemoveId(newObject);
			InsertId(idObject);
			return idObject;
		}

		public virtual IdArrayList Filter(IdObject filterValue)
		{
			return this;
		}

		public virtual IdArrayList Filter(IdObject filterValue, bool flag)
		{
			return this;
		}
	}
}
