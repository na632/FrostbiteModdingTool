using System;
using System.Data;
using System.IO;

namespace FifaLibrary
{
	public class FieldDescriptor
	{
		public enum EFieldTypes
		{
			String = 0,
			Integer = 3,
			Float = 4,
			ShortCompressedString = 13,
			LongCompressedString = 14
		}

		private DataRow m_XmlDataRow;

		private TableDescriptor m_TableDescriptor;

		private EFieldTypes m_FieldType;

		private int m_BitOffset;

		private byte[] m_ShortName;

		private int m_Depth;

		private int m_Mask;

		private string m_FieldShortName;

		private string m_FieldName;

		private int m_RangeLow;

		private int m_RangeHigh;

		private int m_TypeIndex;

		private int m_OrderInTheTable;

		public TableDescriptor TableDescriptor
		{
			get
			{
				return m_TableDescriptor;
			}
			set
			{
				m_TableDescriptor = value;
			}
		}

		public EFieldTypes FieldType
		{
			get
			{
				return m_FieldType;
			}
			set
			{
				m_FieldType = value;
			}
		}

		public int BitOffset
		{
			get
			{
				return m_BitOffset;
			}
			set
			{
				m_BitOffset = value;
			}
		}

		public int Depth
		{
			get
			{
				return m_Depth;
			}
			set
			{
				m_Depth = value;
				m_Mask = (int)((1L << m_Depth) - 1);
			}
		}

		public int Mask => m_Mask;

		public string FieldShortName
		{
			get
			{
				return m_FieldShortName;
			}
			set
			{
				m_FieldShortName = value;
			}
		}

		public string FieldName
		{
			get
			{
				return m_FieldName;
			}
			set
			{
				m_FieldName = value;
			}
		}

		public int RangeLow
		{
			get
			{
				return m_RangeLow;
			}
			set
			{
				m_RangeLow = value;
			}
		}

		public int RangeHigh => m_RangeHigh;

		public int TypeIndex
		{
			get
			{
				return m_TypeIndex;
			}
			set
			{
				m_TypeIndex = value;
			}
		}

		public int OrderInTheTable
		{
			get
			{
				return m_OrderInTheTable;
			}
			set
			{
				m_OrderInTheTable = value;
			}
		}

		public FieldDescriptor(TableDescriptor tableDescriptor)
		{
			m_TableDescriptor = tableDescriptor;
		}

		public void Load(DbReader r)
		{
			m_FieldType = (EFieldTypes)r.ReadInt32();
			m_BitOffset = r.ReadInt32();
			m_ShortName = r.ReadBytes(4);
			m_Depth = r.ReadInt32();
			m_FieldShortName = FifaUtil.ConvertBytesToString(m_ShortName);
			m_FieldName = m_FieldShortName;
		}

		public void Save(BinaryWriter w)
		{
			w.Write((int)m_FieldType);
			w.Write(m_BitOffset);
			w.Write(m_ShortName);
			w.Write(m_Depth);
		}

		public void AssignXmlDescriptor(DataSet descriptorDataSet)
		{
			if (descriptorDataSet == null)
			{
				return;
			}
			int count = descriptorDataSet.Tables["field"].Rows.Count;
			for (int i = 0; i < count; i++)
			{
				DataRow dataRow = descriptorDataSet.Tables["field"].Rows[i];
				if (m_FieldShortName == (string)dataRow["shortname"])
				{
					int index = (int)dataRow["fields_Id"];
					int index2 = (int)descriptorDataSet.Tables["fields"].Rows[index]["table_Id"];
					if ((string)descriptorDataSet.Tables["table"].Rows[index2]["shortname"] == m_TableDescriptor.TableShortName)
					{
						m_FieldName = (string)dataRow["name"];
						m_RangeLow = Convert.ToInt32((string)dataRow["rangelow"]);
						m_RangeHigh = Convert.ToInt32((string)dataRow["rangehigh"]);
						m_XmlDataRow = dataRow;
						break;
					}
				}
			}
		}

		public bool Expand(int depth)
		{
			if (depth < m_Depth)
			{
				return false;
			}
			if (depth > m_Depth)
			{
				_ = m_Depth;
				m_Depth = depth;
				m_XmlDataRow["depth"] = m_Depth.ToString();
			}
			int num = 1 << m_Depth;
			m_RangeHigh = m_RangeLow + num - 1;
			m_XmlDataRow["rangehigh"] = m_RangeHigh.ToString();
			return true;
		}

		public bool Expand(int depth, int minValue)
		{
			m_RangeLow = minValue;
			m_XmlDataRow["rangelow"] = m_RangeLow.ToString();
			return Expand(depth);
		}
	}
}
