using System;
using System.Data;
using System.IO;

namespace FifaLibrary
{
	public class TableDescriptor
	{
		private char[] m_ShortName;

		private uint m_NBitRecords;

		private int m_NFields;

		private FieldDescriptor[] m_FieldDescriptors;

		private HuffmannTree m_HuffmannTree;

		private static DataSet s_DescriptorDataSet;

		private string m_TableName;

		private string m_TableShortName;

		private int m_RecordSize;

		private int m_NKeyFields;

		private int[] m_KeyIndex;

		private int m_NFloatFields;

		private int m_NStringFields;

		private int m_NCompressedStringFields;

		private int m_NIntFields;

		private int[] m_MinValues;

		private int[] m_MaxValues;

		private static FieldComparer s_FieldComparer = new FieldComparer();

		public char[] ShortName
		{
			get
			{
				return m_ShortName;
			}
			set
			{
				m_ShortName = value;
			}
		}

		public uint NBitRecords
		{
			get
			{
				return m_NBitRecords;
			}
			set
			{
				m_NBitRecords = value;
			}
		}

		public int NFields
		{
			get
			{
				return m_NFields;
			}
			set
			{
				m_NFields = value;
			}
		}

		public FieldDescriptor[] FieldDescriptors
		{
			get
			{
				return m_FieldDescriptors;
			}
			set
			{
				m_FieldDescriptors = value;
			}
		}

		public HuffmannTree HuffmannTree
		{
			get
			{
				return m_HuffmannTree;
			}
			set
			{
				m_HuffmannTree = value;
			}
		}

		public static DataSet DescriptorDataSet
		{
			get
			{
				return s_DescriptorDataSet;
			}
			set
			{
				s_DescriptorDataSet = value;
			}
		}

		public string TableName
		{
			get
			{
				return m_TableName;
			}
			set
			{
				m_TableName = value;
			}
		}

		public string TableShortName
		{
			get
			{
				return m_TableShortName;
			}
			set
			{
				m_TableShortName = value;
			}
		}

		public int RecordSize
		{
			get
			{
				return m_RecordSize;
			}
			set
			{
				m_RecordSize = value;
			}
		}

		public int NKeyFields
		{
			get
			{
				return m_NKeyFields;
			}
			set
			{
				m_NKeyFields = value;
			}
		}

		public int[] KeyIndex
		{
			get
			{
				return m_KeyIndex;
			}
			set
			{
				m_KeyIndex = value;
			}
		}

		public int NFloatFields => m_NFloatFields;

		public int NStringFields => m_NStringFields;

		public int NCompressedStringFields => m_NCompressedStringFields;

		public int NIntFields => m_NIntFields;

		public int[] MinValues => m_MinValues;

		public int[] MaxValues => m_MaxValues;

		public void SortFields()
		{
			if (m_FieldDescriptors != null && m_FieldDescriptors.Length >= 2)
			{
				Array.Sort(m_FieldDescriptors, s_FieldComparer);
			}
		}

		public void LoadTableName(DbReader r)
		{
			m_ShortName = r.ReadChars(4);
			m_TableShortName = new string(m_ShortName);
			AssignXmlTableName();
		}

		public void LoadFieldDescriptors(DbReader r)
		{
			m_FieldDescriptors = new FieldDescriptor[m_NFields];
			m_NKeyFields = 0;
			m_NFloatFields = 0;
			m_NIntFields = 0;
			m_NStringFields = 0;
			m_NCompressedStringFields = 0;
			m_MinValues = new int[m_NFields];
			m_MaxValues = new int[m_NFields];
			m_KeyIndex = new int[4];
			for (int i = 0; i < m_NFields; i++)
			{
				m_FieldDescriptors[i] = new FieldDescriptor(this);
				m_FieldDescriptors[i].Load(r);
				m_FieldDescriptors[i].OrderInTheTable = i;
				m_FieldDescriptors[i].AssignXmlDescriptor(s_DescriptorDataSet);
				switch (m_FieldDescriptors[i].FieldType)
				{
				case FieldDescriptor.EFieldTypes.String:
					m_FieldDescriptors[i].TypeIndex = m_NStringFields;
					m_NStringFields++;
					break;
				case FieldDescriptor.EFieldTypes.Integer:
					m_FieldDescriptors[i].TypeIndex = m_NIntFields;
					m_MinValues[m_NIntFields] = m_FieldDescriptors[i].RangeLow;
					m_MaxValues[m_NIntFields] = m_FieldDescriptors[i].RangeHigh;
					m_NIntFields++;
					break;
				case FieldDescriptor.EFieldTypes.Float:
					m_FieldDescriptors[i].TypeIndex = m_NFloatFields;
					m_NFloatFields++;
					break;
				case FieldDescriptor.EFieldTypes.ShortCompressedString:
				case FieldDescriptor.EFieldTypes.LongCompressedString:
					m_FieldDescriptors[i].TypeIndex = m_NCompressedStringFields;
					m_NCompressedStringFields++;
					break;
				}
			}
			AssignXmlTableIndex();
		}

		public void SaveFieldDescriptors(BinaryWriter w)
		{
			for (int i = 0; i < m_NFields; i++)
			{
				for (int j = 0; j < m_NFields; j++)
				{
					if (m_FieldDescriptors[j].OrderInTheTable == i)
					{
						m_FieldDescriptors[j].Save(w);
						break;
					}
				}
			}
		}

		private void AssignXmlTableName()
		{
			if (s_DescriptorDataSet == null)
			{
				return;
			}
			int count = s_DescriptorDataSet.Tables["table"].Rows.Count;
			for (int i = 0; i < count; i++)
			{
				DataRow dataRow = s_DescriptorDataSet.Tables["table"].Rows[i];
				if ((string)dataRow["shortname"] == m_TableShortName)
				{
					string text = (m_TableName = (string)dataRow["name"]);
					break;
				}
			}
		}

		private void AssignXmlTableIndex()
		{
			if (s_DescriptorDataSet == null)
			{
				return;
			}
			int count = s_DescriptorDataSet.Tables["index"].Rows.Count;
			int count2 = s_DescriptorDataSet.Tables["indexfield"].Rows.Count;
			for (int i = 0; i < count; i++)
			{
				DataRow dataRow = s_DescriptorDataSet.Tables["index"].Rows[i];
				if (!((string)dataRow["tableshortname"] == m_TableShortName))
				{
					continue;
				}
				int num = (int)dataRow.ItemArray[0];
				for (int j = 0; j < count2; j++)
				{
					DataRow dataRow2 = s_DescriptorDataSet.Tables["indexfield"].Rows[j];
					if ((int)dataRow2[2] != num)
					{
						continue;
					}
					string text = (string)dataRow2[0];
					int num2 = 0;
					for (int k = 0; k < m_NFields; k++)
					{
						if (m_FieldDescriptors[k].FieldShortName == text)
						{
							m_KeyIndex[m_NKeyFields++] = num2;
							break;
						}
						if (m_FieldDescriptors[k].FieldType == FieldDescriptor.EFieldTypes.Integer)
						{
							num2++;
						}
					}
				}
				break;
			}
		}

		public FieldDescriptor GetFieldDescriptor(string longName)
		{
			for (int i = 0; i < m_NFields; i++)
			{
				if (m_FieldDescriptors[i].FieldName == longName)
				{
					return m_FieldDescriptors[i];
				}
			}
			return null;
		}

		public int GetFieldIndex(string fieldName)
		{
			for (int i = 0; i < m_NFields; i++)
			{
				if (fieldName == FieldDescriptors[i].FieldName)
				{
					return FieldDescriptors[i].TypeIndex;
				}
			}
			return -1;
		}

		public void RecalculateFieldOffset()
		{
			int[] array = new int[NFields];
			int num = 0;
			uint num2 = 0u;
			uint nRecords = 0u;
			do
			{
				nRecords = m_NBitRecords;
				for (int i = 0; i < NFields; i++)
				{
					if (FieldDescriptors[i].BitOffset >= num2 && FieldDescriptors[i].BitOffset < nRecords)
					{
						array[num] = i;
						nRecords = (uint)FieldDescriptors[i].BitOffset;
					}
				}
				num2 = nRecords + 1;
				num++;
			}
			while (num < NFields);
			num2 = 0u;
			for (int j = 0; j < NFields; j++)
			{
				switch (FieldDescriptors[array[j]].FieldType)
				{
				case FieldDescriptor.EFieldTypes.Float:
				case FieldDescriptor.EFieldTypes.ShortCompressedString:
				case FieldDescriptor.EFieldTypes.LongCompressedString:
					if (num2 % 32u != 0)
					{
						num2 = (num2 + 31) & 0xFFFFFFE0u;
					}
					break;
				case FieldDescriptor.EFieldTypes.String:
					if (num2 % 8u != 0)
					{
						num2 = (num2 + 7) & 0xFFFFFFF8u;
					}
					break;
				}
				FieldDescriptors[array[j]].BitOffset = (int)num2;
				switch (FieldDescriptors[array[j]].FieldType)
				{
				case FieldDescriptor.EFieldTypes.Float:
				case FieldDescriptor.EFieldTypes.ShortCompressedString:
				case FieldDescriptor.EFieldTypes.LongCompressedString:
					num2 += 32;
					break;
				case FieldDescriptor.EFieldTypes.String:
				case FieldDescriptor.EFieldTypes.Integer:
					num2 += (uint)FieldDescriptors[array[j]].Depth;
					break;
				}
			}
			int l = (int)((num2 + 7) / 8u);
			if (l > m_RecordSize)
			{
				m_RecordSize = (l + 3) / 4 * 4;
				m_NBitRecords = (uint)(m_RecordSize * 8 - 1);
			}
		}

        public override string ToString()
        {
			if(!string.IsNullOrEmpty(TableName))
				return TableName;

            return base.ToString();
        }
    }
}
