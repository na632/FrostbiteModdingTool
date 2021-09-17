using System;
using System.Reflection;

namespace FifaLibrary
{
	public class IdObject
	{
		private int m_Id;

		public int Id
		{
			get
			{
				return m_Id;
			}
			set
			{
				m_Id = value;
			}
		}

		public IdObject()
		{
			m_Id = -1;
		}

		public IdObject(int id)
		{
			m_Id = id;
		}

		public virtual IdObject Clone(int newId)
		{
			IdObject obj = (IdObject)MemberwiseClone();
			obj.Id = newId;
			return obj;
		}

		public virtual bool Delete()
		{
			return true;
		}

		public static IdObject Create(Type type, int newId)
		{
			Type[] types = new Type[1] { typeof(int) };
			object[] parameters = new object[1] { newId };
			ConstructorInfo constructor = type.GetConstructor(types);
			if (constructor == null)
			{
				return null;
			}
			return (IdObject)constructor.Invoke(parameters);
		}
	}
}
