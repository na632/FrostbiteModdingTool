using System;
using System.Data;

namespace FifaLibrary
{
    public class FifaTable
    {
        private int m_Unknown00;

        private uint m_CompressedStringLength;

        private short m_Unknown14;

        private short m_Unknown16;

        private int m_Unknown1C;

        private uint m_CrcTableHeader;

        private long m_CrcTableHeaderPosition;

        private uint m_CrcRecords;

        private long m_CrcRecordsPosition;

        private int m_NRecords;

        private int m_NWrittenRecords;

        private int m_NCancelledRecords;

        private int m_NValidRecords;

        private int m_NMaxRecords;

        private static RecordComparer s_RecordComparer = new RecordComparer();

        protected Record[] m_Records;

        private TableDescriptor m_TableDescriptor;

        public int Unknown00
        {
            get
            {
                return m_Unknown00;
            }
            set
            {
                m_Unknown00 = value;
            }
        }

        public uint CompressedStringLength
        {
            get
            {
                return m_CompressedStringLength;
            }
            set
            {
                m_CompressedStringLength = value;
            }
        }

        public short Unknown14
        {
            get
            {
                return m_Unknown14;
            }
            set
            {
                m_Unknown14 = value;
            }
        }

        public short Unknown16
        {
            get
            {
                return m_Unknown16;
            }
            set
            {
                m_Unknown16 = value;
            }
        }

        public int Unknown1C
        {
            get
            {
                return m_Unknown1C;
            }
            set
            {
                m_Unknown1C = value;
            }
        }

        public uint CrcTableHeader
        {
            get
            {
                return m_CrcTableHeader;
            }
            set
            {
                m_CrcTableHeader = value;
            }
        }

        public long CrcTableHeaderPosition
        {
            get
            {
                return m_CrcTableHeaderPosition;
            }
            set
            {
                m_CrcTableHeaderPosition = value;
            }
        }

        public uint CrcRecords
        {
            get
            {
                return m_CrcRecords;
            }
            set
            {
                m_CrcRecords = value;
            }
        }

        public long CrcRecordsPosition
        {
            get
            {
                return m_CrcRecordsPosition;
            }
            set
            {
                m_CrcRecordsPosition = value;
            }
        }

        public int NRecords
        {
            get
            {
                return m_NRecords;
            }
            set
            {
                m_NRecords = value;
            }
        }

        public int NValidRecords
        {
            get
            {
                return m_NValidRecords;
            }
            set
            {
                m_NValidRecords = value;
            }
        }

        public int NMaxRecords
        {
            get
            {
                return m_NMaxRecords;
            }
            set
            {
                m_NMaxRecords = value;
            }
        }

        public Record[] Records
        {
            get
            {
                return m_Records;
            }
            set
            {
                m_Records = value;
            }
        }

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

        public FifaTable()
        {
            m_TableDescriptor = new TableDescriptor();
        }

        public FifaTable(TableDescriptor tableDescriptor)
        {
            m_TableDescriptor = tableDescriptor;
        }


        public void LoadTableName(DbReader r)
        {
            m_TableDescriptor.LoadTableName(r);
        }

        public bool Load(DbReader r, long offset)
        {
            bool flag = true;
            r.BaseStream.Position = offset;
            m_Unknown00 = r.ReadInt32();
            m_TableDescriptor.RecordSize = r.ReadInt32();
            m_TableDescriptor.NBitRecords = r.ReadUInt32();
            m_CompressedStringLength = r.ReadUInt32();
            m_NRecords = r.ReadUInt16();
            m_NMaxRecords = m_NRecords;
            m_NWrittenRecords = r.ReadUInt16();
            m_NCancelledRecords = r.ReadUInt16();
            m_NValidRecords = m_NWrittenRecords - m_NCancelledRecords;
            m_Unknown16 = r.ReadInt16();
            _ = m_NCancelledRecords;
            m_TableDescriptor.NFields = r.ReadByte();
            r.ReadBytes(3);
            m_Unknown1C = r.ReadInt32();
            m_CrcTableHeaderPosition = r.BaseStream.Position;
            m_CrcTableHeader = r.ReadUInt32();
            m_TableDescriptor.LoadFieldDescriptors(r);
            m_TableDescriptor.SortFields();
            if (m_NRecords > 0)
            {
                m_Records = new Record[m_NRecords];
                Record.HuffmannTreeSize = int.MaxValue;
                int num = 0;
                int num2 = 0;
                for (int i = 0; i < m_NWrittenRecords; i++)
                {
                    m_Records[num] = new Record(m_TableDescriptor);
                    bool flag2 = m_Records[num].Load(r);
                    flag = flag && flag2;
                    if (m_Records[num].IsInvalid)
                    {
                        m_Records[num].Clean();
                        num2++;
                    }
                    else
                    {
                        num++;
                    }
                }
                if (num != m_NValidRecords)
                {
                    m_NValidRecords = num;
                }
                for (int j = m_NValidRecords; j < m_NMaxRecords; j++)
                {
                    m_Records[j] = new Record(m_TableDescriptor);
                    m_Records[j].Clean();
                }
                if (m_TableDescriptor.NCompressedStringFields > 0)
                {
                    long position = r.BaseStream.Position;
                    int nNodes = Record.HuffmannTreeSize / 4;
                    m_TableDescriptor.HuffmannTree = new HuffmannTree(nNodes);
                    m_TableDescriptor.HuffmannTree.Load(r);
                    for (int k = 0; k < m_NRecords; k++)
                    {
                        r.BaseStream.Position = position;
                        m_Records[k].LoadCompressedStrings(r);
                    }
                    r.BaseStream.Position = m_CompressedStringLength + position;
                }
            }
            m_CrcRecordsPosition = r.BaseStream.Position;
            m_CrcRecords = r.ReadUInt32();
            return flag;
        }

        public void PutValidRecordFirst()
        {
            int num = 0;
            for (int i = 0; i < m_NRecords; i++)
            {
                if (!m_Records[i].IsInvalid)
                {
                    if (i > num)
                    {
                        Record record = m_Records[num];
                        m_Records[num] = m_Records[i];
                        m_Records[i] = record;
                    }
                    num++;
                }
            }
            for (int j = num; j < m_NRecords; j++)
            {
                if (!m_Records[j].IsInvalid)
                {
                    m_Records[j].Clean();
                }
            }
        }

        public void Save(DbWriter w)
        {
            w.Write(m_Unknown00);
            w.Write(m_TableDescriptor.RecordSize);
            w.Write(m_TableDescriptor.NBitRecords);
            long position = w.BaseStream.Position;
            if (m_TableDescriptor.NCompressedStringFields == 0)
            {
                w.Write(0);
            }
            else
            {
                w.Write(-1);
            }
            int num = 0;
            for (int i = 0; i < m_NRecords; i++)
            {
                if (!m_Records[i].IsClean())
                {
                    num++;
                }
            }
            m_NValidRecords = num;
            w.Write((ushort)m_NRecords);
            w.Write((ushort)m_NValidRecords);
            w.Write((short)0);
            w.Write((short)(-1));
            w.Write((byte)m_TableDescriptor.NFields);
            w.Write((byte)0);
            w.Write((short)0);
            w.Write(m_Unknown1C);
            m_CrcTableHeaderPosition = w.BaseStream.Position;
            w.Write(-1);
            m_TableDescriptor.SaveFieldDescriptors(w);
            long num2 = w.BaseStream.Position + m_NRecords * m_TableDescriptor.RecordSize;
            int num3 = 0;
            if (m_TableDescriptor.HuffmannTree != null)
            {
                num3 = m_TableDescriptor.HuffmannTree.Size;
            }
            if (m_NRecords > 0)
            {
                num = 0;
                for (int j = 0; j < m_NRecords; j++)
                {
                    if (!m_Records[j].IsClean())
                    {
                        num3 = m_Records[j].Save(w, num2, num3);
                        num++;
                    }
                }
                Record record = new Record(TableDescriptor);
                record.Clean();
                for (int k = m_NValidRecords; k < m_NRecords; k++)
                {
                    num3 = record.Save(w, num2, num3);
                }
                if (m_TableDescriptor.HuffmannTree != null)
                {
                    w.BaseStream.Position = position;
                    m_CompressedStringLength = (uint)num3;
                    w.Write(num3);
                    w.BaseStream.Position = num2;
                    m_TableDescriptor.HuffmannTree.Save(w);
                }
                num3 = FifaUtil.RoundUp(num3, 8);
                m_CrcRecordsPosition = num2 + num3;
                w.BaseStream.Position = m_CrcRecordsPosition;
                w.Write(-1);
            }
            else
            {
                m_CrcRecordsPosition = w.BaseStream.Position;
                w.Write(-1);
            }
        }

        public void SortByKeys()
        {
            if (m_TableDescriptor.KeyIndex.Length != 0 && m_NRecords >= 2)
            {
                Array.Sort(m_Records, s_RecordComparer);
            }
        }

        public void SortByKeys(int keyIndex)
        {
            if (m_TableDescriptor.KeyIndex.Length != 0)
            {
                int num = m_TableDescriptor.KeyIndex[0];
                m_TableDescriptor.KeyIndex[0] = keyIndex;
                if (m_TableDescriptor.NKeyFields == 0)
                {
                    m_TableDescriptor.NKeyFields = 1;
                }
                if (m_NRecords >= 2)
                {
                    Array.Sort(m_Records, s_RecordComparer);
                }
                m_TableDescriptor.KeyIndex[0] = num;
            }
        }

        public Record SearchByKeys(Record r)
        {
            if (m_Records == null || r == null)
            {
                return null;
            }
            int num = Array.BinarySearch(m_Records, r, s_RecordComparer);
            if (num >= 0)
            {
                return m_Records[num];
            }
            return null;
        }

        public string SearchStringByKey(int key)
        {
            Record record = new Record(m_TableDescriptor);
            record.KeyField[0] = key;
            int num = Array.BinarySearch(m_Records, record, s_RecordComparer);
            if (num >= 0)
            {
                return m_Records[num].StringField[0];
            }
            return null;
        }

        public void ExchangeRecords(int i, int j)
        {
            Record record = new Record(m_TableDescriptor);
            record = m_Records[i];
            m_Records[i] = m_Records[j];
            m_Records[j] = record;
        }

        public Record[] ResizeRecords(int nRecords)
        {
            int num = 0;
            if (m_Records != null)
            {
                num = m_Records.Length;
                Array.Resize(ref m_Records, nRecords);
            }
            else
            {
                m_Records = new Record[nRecords];
            }
            if (num < nRecords)
            {
                for (int i = num; i < nRecords; i++)
                {
                    m_Records[i] = new Record(m_TableDescriptor);
                }
            }
            m_NRecords = nRecords;
            return m_Records;
        }

        public bool ExpandField(string fieldName, int nBits)
        {
            return m_TableDescriptor.GetFieldDescriptor(fieldName)?.Expand(nBits) ?? false;
        }

        public bool ExpandField(string fieldName, int nBits, int minValue)
        {
            return m_TableDescriptor.GetFieldDescriptor(fieldName)?.Expand(nBits, minValue) ?? false;
        }

        public DataTable ConvertToDataTable()
        {
            DataTable dataTable = CreateEmptyDataTable();
            for (int i = 0; i < m_NRecords; i++)
            {
                dataTable.Rows.Add(Records[i].ConvertToDataRow());
            }
            return dataTable;
        }

        public DataTable ConvertToDataTable(int[] keys, string fieldName)
        {
            DataTable dataTable = CreateEmptyDataTable();
            int fieldIndex = m_TableDescriptor.GetFieldIndex(fieldName);
            //Array.Sort(keys);
            //SortByKeys(fieldIndex);
            int num = 0;
            int i = num;
            int num2 = 0;
            while (num2 < keys.Length && i < m_NRecords)
            {
                for (i = num; i < m_NRecords; i++)
                {
                    if (Records[i].IntField[fieldIndex] >= keys[num2])
                    {
                        if (Records[i].IntField[fieldIndex] == keys[num2])
                        {
                            dataTable.Rows.Add(Records[i].ConvertToDataRow());
                            num = i + 1;
                        }
                        else
                        {
                            num = i;
                            num2++;
                        }
                        break;
                    }
                }
            }
            return dataTable;
        }

        public DataTable ConvertToDataTableUsingRtsg(int[] keys, string fieldName)
        {
            DataTable dataTable = CreateEmptyDataTable();
            int fieldIndex = m_TableDescriptor.GetFieldIndex(fieldName);
            Array.Sort(keys);
            for (int i = 0; i < m_NRecords; i++)
            {
                int num = (Records[i].IntField[fieldIndex] >> 20) & 0xFFF;
                for (int j = 0; j < keys.Length; j++)
                {
                    if (keys[j] == num)
                    {
                        dataTable.Rows.Add(Records[i].ConvertToDataRow());
                        break;
                    }
                }
            }
            return dataTable;
        }

        public DataTable ConvertToDataTableUsingIntField(int[] keys, int intFieldIndex)
        {
            DataTable dataTable = CreateEmptyDataTable();
            Array.Sort(keys);
            for (int i = 0; i < m_NRecords; i++)
            {
                for (int j = 0; j < keys.Length; j++)
                {
                    if (keys[j] == Records[i].IntField[intFieldIndex])
                    {
                        dataTable.Rows.Add(Records[i].ConvertToDataRow());
                        break;
                    }
                }
            }
            return dataTable;
        }

        public DataTable CreateEmptyDataTable()
        {
            int num = 0;
            float num2 = 1f;
            Type type = num.GetType();
            Type type2 = num2.GetType();
            Type type3 = "string".GetType();
            DataTable dataTable = new DataTable(m_TableDescriptor.TableName);
            for (int i = 0; i < m_TableDescriptor.NFields; i++)
            {
                switch (m_TableDescriptor.FieldDescriptors[i].FieldType)
                {
                    case FieldDescriptor.EFieldTypes.String:
                        dataTable.Columns.Add(TableDescriptor.FieldDescriptors[i].FieldName, type3);
                        break;
                    case FieldDescriptor.EFieldTypes.Integer:
                        dataTable.Columns.Add(TableDescriptor.FieldDescriptors[i].FieldName, type);
                        break;
                    case FieldDescriptor.EFieldTypes.Float:
                        dataTable.Columns.Add(TableDescriptor.FieldDescriptors[i].FieldName, type2);
                        break;
                    case FieldDescriptor.EFieldTypes.LongCompressedString:
                        dataTable.Columns.Add(TableDescriptor.FieldDescriptors[i].FieldName, type3);
                        break;
                    case FieldDescriptor.EFieldTypes.ShortCompressedString:
                        dataTable.Columns.Add(TableDescriptor.FieldDescriptors[i].FieldName, type3);
                        break;
                }
            }
            return dataTable;
        }

        public void ConvertFromDataTable(DataTable dataTable)
        {
            m_NRecords = dataTable.Rows.Count;
            if (Records != null)
            {
                ResizeRecords(m_NRecords);
            }
            else
            {
                Records = new Record[m_NRecords];
                for (int i = 0; i < m_NRecords; i++)
                {
                    Records[i] = new Record(m_TableDescriptor);
                }
            }
            for (int j = 0; j < m_NRecords; j++)
            {
                Records[j].ConvertFromDataRow(dataTable.Rows[j]);
            }
        }

        public void ConvertFromDataTableFrom15To16(DataTable dataTable)
        {
            m_NRecords = dataTable.Rows.Count;
            m_NValidRecords = m_NRecords;
            if (Records != null)
            {
                ResizeRecords(m_NRecords);
            }
            else
            {
                Records = new Record[m_NRecords];
                for (int i = 0; i < m_NRecords; i++)
                {
                    Records[i] = new Record(m_TableDescriptor);
                }
            }
            for (int j = 0; j < m_NRecords; j++)
            {
                Records[j].ConvertFromDataRowFrom15To16(dataTable.Rows[j]);
            }
        }

        public override string ToString()
        {
            if (m_TableDescriptor != null && !string.IsNullOrEmpty(m_TableDescriptor.TableName))
                return m_TableDescriptor.TableName.ToString();

            return base.ToString();
        }
    }
}
