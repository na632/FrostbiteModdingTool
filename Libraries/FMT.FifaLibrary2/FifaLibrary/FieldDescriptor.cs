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
                return this.m_TableDescriptor;
            }
            set
            {
                this.m_TableDescriptor = value;
            }
        }

        public EFieldTypes FieldType
        {
            get
            {
                return this.m_FieldType;
            }
            set
            {
                this.m_FieldType = value;
            }
        }

        public int BitOffset
        {
            get
            {
                return this.m_BitOffset;
            }
            set
            {
                this.m_BitOffset = value;
            }
        }

        public int Depth
        {
            get
            {
                return this.m_Depth;
            }
            set
            {
                this.m_Depth = value;
                this.m_Mask = (int)((1L << this.m_Depth) - 1);
            }
        }

        public int Mask => this.m_Mask;

        public string FieldShortName
        {
            get
            {
                return this.m_FieldShortName;
            }
            set
            {
                this.m_FieldShortName = value;
            }
        }

        public string FieldName
        {
            get
            {
                return this.m_FieldName;
            }
            set
            {
                this.m_FieldName = value;
            }
        }

        public int RangeLow
        {
            get
            {
                return this.m_RangeLow;
            }
            set
            {
                this.m_RangeLow = value;
            }
        }

        public int RangeHigh => this.m_RangeHigh;

        public int TypeIndex
        {
            get
            {
                return this.m_TypeIndex;
            }
            set
            {
                this.m_TypeIndex = value;
            }
        }

        public int OrderInTheTable
        {
            get
            {
                return this.m_OrderInTheTable;
            }
            set
            {
                this.m_OrderInTheTable = value;
            }
        }

        public FieldDescriptor(TableDescriptor tableDescriptor)
        {
            this.m_TableDescriptor = tableDescriptor;
        }

        public void Load(DbReader r)
        {
            this.m_FieldType = (EFieldTypes)r.ReadInt32();
            this.m_BitOffset = r.ReadInt32();
            this.m_ShortName = r.ReadBytes(4);
            this.m_Depth = r.ReadInt32();
            this.m_FieldShortName = FifaUtil.ConvertBytesToString(this.m_ShortName);
            this.m_FieldName = this.m_FieldShortName;
        }

        public void Save(BinaryWriter w)
        {
            w.Write((int)this.m_FieldType);
            w.Write(this.m_BitOffset);
            w.Write(this.m_ShortName);
            w.Write(this.m_Depth);
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
                if (this.m_FieldShortName == (string)dataRow["shortname"])
                {
                    int index = (int)dataRow["fields_Id"];
                    int index2 = (int)descriptorDataSet.Tables["fields"].Rows[index]["table_Id"];
                    if ((string)descriptorDataSet.Tables["table"].Rows[index2]["shortname"] == this.m_TableDescriptor.TableShortName)
                    {
                        this.m_FieldName = (string)dataRow["name"];
                        this.m_RangeLow = Convert.ToInt32((string)dataRow["rangelow"]);
                        this.m_RangeHigh = Convert.ToInt32((string)dataRow["rangehigh"]);
                        this.m_XmlDataRow = dataRow;
                        break;
                    }
                }
            }
        }

        public bool Expand(int depth)
        {
            if (depth < this.m_Depth)
            {
                return false;
            }
            if (depth > this.m_Depth)
            {
                _ = this.m_Depth;
                this.m_Depth = depth;
                this.m_XmlDataRow["depth"] = this.m_Depth.ToString();
            }
            int num = 1 << this.m_Depth;
            this.m_RangeHigh = this.m_RangeLow + num - 1;
            this.m_XmlDataRow["rangehigh"] = this.m_RangeHigh.ToString();
            return true;
        }

        public bool Expand(int depth, int minValue)
        {
            this.m_RangeLow = minValue;
            this.m_XmlDataRow["rangelow"] = this.m_RangeLow.ToString();
            return this.Expand(depth);
        }
    }
}
