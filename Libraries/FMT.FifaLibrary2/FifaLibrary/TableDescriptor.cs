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
				return this.m_ShortName;
			}
			set
			{
				this.m_ShortName = value;
			}
		}

		public uint NBitRecords
		{
			get
			{
				return this.m_NBitRecords;
			}
			set
			{
				this.m_NBitRecords = value;
			}
		}

		public int NFields
		{
			get
			{
				return this.m_NFields;
			}
			set
			{
				this.m_NFields = value;
			}
		}

		public FieldDescriptor[] FieldDescriptors
		{
			get
			{
				return this.m_FieldDescriptors;
			}
			set
			{
				this.m_FieldDescriptors = value;
			}
		}

		public HuffmannTree HuffmannTree
		{
			get
			{
				return this.m_HuffmannTree;
			}
			set
			{
				this.m_HuffmannTree = value;
			}
		}

		public static DataSet DescriptorDataSet
		{
			get
			{
				return TableDescriptor.s_DescriptorDataSet;
			}
			set
			{
				TableDescriptor.s_DescriptorDataSet = value;
			}
		}

		public string TableName
		{
			get
			{
				return this.m_TableName;
			}
			set
			{
				this.m_TableName = value;
			}
		}

		public string TableShortName
		{
			get
			{
				return this.m_TableShortName;
			}
			set
			{
				this.m_TableShortName = value;
			}
		}

		public int RecordSize
		{
			get
			{
				return this.m_RecordSize;
			}
			set
			{
				this.m_RecordSize = value;
			}
		}

		public int NKeyFields
		{
			get
			{
				return this.m_NKeyFields;
			}
			set
			{
				this.m_NKeyFields = value;
			}
		}

		public int[] KeyIndex
		{
			get
			{
				return this.m_KeyIndex;
			}
			set
			{
				this.m_KeyIndex = value;
			}
		}

		public int NFloatFields => this.m_NFloatFields;

		public int NStringFields => this.m_NStringFields;

		public int NCompressedStringFields => this.m_NCompressedStringFields;

		public int NIntFields => this.m_NIntFields;

		public int[] MinValues => this.m_MinValues;

		public int[] MaxValues => this.m_MaxValues;

		public void SortFields()
		{
			if (this.m_FieldDescriptors != null && this.m_FieldDescriptors.Length >= 2)
			{
				Array.Sort(this.m_FieldDescriptors, TableDescriptor.s_FieldComparer);
			}
		}

		public void LoadTableName(DbReader r)
		{
			this.m_ShortName = r.ReadChars(4);
			this.m_TableShortName = new string(this.m_ShortName);
			this.AssignXmlTableName();
		}

		public void LoadFieldDescriptors(DbReader r)
		{
			this.m_FieldDescriptors = new FieldDescriptor[this.m_NFields];
			this.m_NKeyFields = 0;
			this.m_NFloatFields = 0;
			this.m_NIntFields = 0;
			this.m_NStringFields = 0;
			this.m_NCompressedStringFields = 0;
			this.m_MinValues = new int[this.m_NFields];
			this.m_MaxValues = new int[this.m_NFields];
			this.m_KeyIndex = new int[4];
			for (int i = 0; i < this.m_NFields; i++)
			{
				this.m_FieldDescriptors[i] = new FieldDescriptor(this);
				this.m_FieldDescriptors[i].Load(r);
				this.m_FieldDescriptors[i].OrderInTheTable = i;
				this.m_FieldDescriptors[i].AssignXmlDescriptor(TableDescriptor.s_DescriptorDataSet);
				switch (this.m_FieldDescriptors[i].FieldType)
				{
				case FieldDescriptor.EFieldTypes.String:
					this.m_FieldDescriptors[i].TypeIndex = this.m_NStringFields;
					this.m_NStringFields++;
					break;
				case FieldDescriptor.EFieldTypes.Integer:
					this.m_FieldDescriptors[i].TypeIndex = this.m_NIntFields;
					this.m_MinValues[this.m_NIntFields] = this.m_FieldDescriptors[i].RangeLow;
					this.m_MaxValues[this.m_NIntFields] = this.m_FieldDescriptors[i].RangeHigh;
					this.m_NIntFields++;
					break;
				case FieldDescriptor.EFieldTypes.Float:
					this.m_FieldDescriptors[i].TypeIndex = this.m_NFloatFields;
					this.m_NFloatFields++;
					break;
				case FieldDescriptor.EFieldTypes.ShortCompressedString:
				case FieldDescriptor.EFieldTypes.LongCompressedString:
					this.m_FieldDescriptors[i].TypeIndex = this.m_NCompressedStringFields;
					this.m_NCompressedStringFields++;
					break;
				}
			}
			this.AssignXmlTableIndex();
		}

		public void SaveFieldDescriptors(BinaryWriter w)
		{
			for (int i = 0; i < this.m_NFields; i++)
			{
				for (int j = 0; j < this.m_NFields; j++)
				{
					if (this.m_FieldDescriptors[j].OrderInTheTable == i)
					{
						this.m_FieldDescriptors[j].Save(w);
						break;
					}
				}
			}
		}

		private void AssignXmlTableName()
		{
			if (TableDescriptor.s_DescriptorDataSet == null)
			{
				return;
			}
			int count = TableDescriptor.s_DescriptorDataSet.Tables["table"].Rows.Count;
			for (int i = 0; i < count; i++)
			{
				DataRow dataRow = TableDescriptor.s_DescriptorDataSet.Tables["table"].Rows[i];
				if ((string)dataRow["shortname"] == this.m_TableShortName)
				{
					string text = (this.m_TableName = (string)dataRow["name"]);
					break;
				}
			}
		}

		private void AssignXmlTableIndex()
		{
			if (TableDescriptor.s_DescriptorDataSet == null)
			{
				return;
			}
			int count = TableDescriptor.s_DescriptorDataSet.Tables["index"].Rows.Count;
			int count2 = TableDescriptor.s_DescriptorDataSet.Tables["indexfield"].Rows.Count;
			for (int i = 0; i < count; i++)
			{
				DataRow dataRow = TableDescriptor.s_DescriptorDataSet.Tables["index"].Rows[i];
				if (!((string)dataRow["tableshortname"] == this.m_TableShortName))
				{
					continue;
				}
				int num = (int)dataRow.ItemArray[0];
				for (int j = 0; j < count2; j++)
				{
					DataRow dataRow2 = TableDescriptor.s_DescriptorDataSet.Tables["indexfield"].Rows[j];
					if ((int)dataRow2[2] != num)
					{
						continue;
					}
					string text = (string)dataRow2[0];
					int num2 = 0;
					for (int k = 0; k < this.m_NFields; k++)
					{
						if (this.m_FieldDescriptors[k].FieldShortName == text)
						{
							this.m_KeyIndex[this.m_NKeyFields++] = num2;
							break;
						}
						if (this.m_FieldDescriptors[k].FieldType == FieldDescriptor.EFieldTypes.Integer)
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
			for (int i = 0; i < this.m_NFields; i++)
			{
				if (this.m_FieldDescriptors[i].FieldName == longName)
				{
					return this.m_FieldDescriptors[i];
				}
			}
			return null;
		}

		public int GetFieldIndex(string fieldName)
		{
			for (int i = 0; i < this.m_NFields; i++)
			{
				if (fieldName == this.FieldDescriptors[i].FieldName)
				{
					return this.FieldDescriptors[i].TypeIndex;
				}
			}
			return -1;
		}

		public void RecalculateFieldOffset()
		{
			int[] array = new int[this.NFields];
			int num = 0;
			uint num2 = 0u;
			uint num3 = 0u;
			do
			{
				num3 = this.m_NBitRecords;
				for (int i = 0; i < this.NFields; i++)
				{
					if (this.FieldDescriptors[i].BitOffset >= num2 && this.FieldDescriptors[i].BitOffset < num3)
					{
						array[num] = i;
						num3 = (uint)this.FieldDescriptors[i].BitOffset;
					}
				}
				num2 = num3 + 1;
				num++;
			}
			while (num < this.NFields);
			num2 = 0u;
			for (int j = 0; j < this.NFields; j++)
			{
				switch (this.FieldDescriptors[array[j]].FieldType)
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
				this.FieldDescriptors[array[j]].BitOffset = (int)num2;
				switch (this.FieldDescriptors[array[j]].FieldType)
				{
				case FieldDescriptor.EFieldTypes.Float:
				case FieldDescriptor.EFieldTypes.ShortCompressedString:
				case FieldDescriptor.EFieldTypes.LongCompressedString:
					num2 += 32;
					break;
				case FieldDescriptor.EFieldTypes.String:
				case FieldDescriptor.EFieldTypes.Integer:
					num2 += (uint)this.FieldDescriptors[array[j]].Depth;
					break;
				}
			}
			int num4 = (int)((num2 + 7) / 8u);
			if (num4 > this.m_RecordSize)
			{
				this.m_RecordSize = (num4 + 3) / 4 * 4;
				this.m_NBitRecords = (uint)(this.m_RecordSize * 8 - 1);
			}
		}
	}
}
