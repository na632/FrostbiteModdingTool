using System.Data;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary
{
	public class DbFile
	{
		protected char[] m_Signature;

		protected int m_FileLength;

		private long m_SignaturePosition;

		private uint m_CrcHeader;

		private long m_CrcHeaderPosition;

		private uint m_CrcShortNames;

		private long m_CrcShortNamesPosition;

		private uint[] m_TableOffset;

		private Table[] m_Table;

		protected int m_NTables;

		private ToolStripProgressBar m_ProgressBar;

		protected string m_FileName;

		protected string m_XmlFileName;

		protected DataSet m_DescriptorDataSet;

		private bool m_NeedToSaveXmlFile;

		protected FifaPlatform m_Platform;

		public long SignaturePosition
		{
			get
			{
				return m_SignaturePosition;
			}
			set
			{
				m_SignaturePosition = value;
			}
		}

		public Table[] Table
		{
			get
			{
				return m_Table;
			}
			set
			{
				m_Table = value;
			}
		}

		public int NTables => m_NTables;

		public ToolStripProgressBar ProgressBar
		{
			set
			{
				m_ProgressBar = value;
			}
		}

		public string FileName => m_FileName;

		public string XmlFileName => m_XmlFileName;

		public DataSet DescriptorDataSet
		{
			get
			{
				return m_DescriptorDataSet;
			}
			set
			{
				m_DescriptorDataSet = value;
			}
		}

		public FifaPlatform Platform
		{
			get
			{
				return m_Platform;
			}
			set
			{
				m_Platform = value;
			}
		}

		public DbFile()
		{
			m_FileName = null;
			m_XmlFileName = null;
			m_DescriptorDataSet = null;
			m_Platform = FifaPlatform.PC;
		}

		public DbFile(string dbFileName, string xmlFileName, ToolStripProgressBar toolStripProgressBar)
		{
			m_ProgressBar = toolStripProgressBar;
			m_FileName = dbFileName;
			m_XmlFileName = xmlFileName;
			m_DescriptorDataSet = null;
			m_Platform = FifaPlatform.PC;
			Load();
		}

		public DbFile(string dbFileName, string xmlFileName)
		{
			m_ProgressBar = null;
			m_FileName = dbFileName;
			m_XmlFileName = xmlFileName;
			m_DescriptorDataSet = null;
			m_Platform = FifaPlatform.PC;
			Load();
		}

		public void ComputeAllCrc(DbReader r, DbWriter w)
		{
			long signaturePosition = m_SignaturePosition;
			int num = (int)(m_CrcHeaderPosition - signaturePosition);
			ComputeAndWriteCrc(r, w, signaturePosition, num);
			signaturePosition += num + 4;
			num = (int)(m_CrcShortNamesPosition - signaturePosition);
			ComputeAndWriteCrc(r, w, signaturePosition, num);
			for (int i = 0; i < m_NTables; i++)
			{
				signaturePosition += num + 4;
				num = (int)(m_Table[i].CrcTableHeaderPosition - signaturePosition);
				ComputeAndWriteCrc(r, w, signaturePosition, num);
				signaturePosition += num + 4;
				num = (int)(m_Table[i].CrcRecordsPosition - signaturePosition);
				ComputeAndWriteCrc(r, w, signaturePosition, num);
			}
		}

		public bool ComputeAllCrc()
		{
			if (!File.Exists(m_FileName))
			{
				return false;
			}
			FileStream fileStream = new FileStream(m_FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			DbWriter dbWriter = new DbWriter(fileStream, m_Platform);
			DbReader dbReader = new DbReader(fileStream, m_Platform);
			ComputeAllCrc(dbReader, dbWriter);
			dbReader.Close();
			dbWriter.Close();
			fileStream.Close();
			return true;
		}

		public int ComputeAndWriteCrc(DbReader r, DbWriter w, long offset, int count)
		{
			_ = new byte[count];
			r.BaseStream.Seek(offset, SeekOrigin.Begin);
			int num = FifaUtil.ComputeCrcDb11(r.ReadBytes(count));
			w.Write(num);
			return num;
		}

		public bool LoadDb(string fileName)
		{
			if (!File.Exists(fileName))
			{
				return false;
			}
			m_FileName = fileName;
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			DbReader dbReader = new DbReader(fileStream, FifaPlatform.PC);
			bool result = LoadDb(dbReader, skipData: false);
			dbReader.Close();
			fileStream.Close();
			return result;
		}

		public bool LoadDb(DbReader r, bool skipData)
		{
			bool flag = false;
			if (skipData)
			{
				while (r.BaseStream.Position <= r.BaseStream.Length - 4)
				{
					if (r.ReadUInt32() == 134234692)
					{
						flag = true;
						r.BaseStream.Position -= 4L;
						break;
					}
					r.BaseStream.Position -= 3L;
				}
				if (!flag)
				{
					return false;
				}
			}
			flag = true;
			m_SignaturePosition = r.BaseStream.Position;
			m_Signature = r.ReadChars(8);
			if (m_Signature[0] != 'D' || m_Signature[1] != 'B' || m_Signature[2] != 0 || m_Signature[3] != '\b' || m_Signature[5] != 0 || m_Signature[6] != 0 || m_Signature[7] != 0)
			{
				return false;
			}
			if (m_Signature[4] == '\0')
			{
				m_Platform = FifaPlatform.PC;
				r.Platform = m_Platform;
			}
			else
			{
				if (m_Signature[4] != '\u0001')
				{
					return false;
				}
				m_Platform = FifaPlatform.XBox;
				r.Platform = m_Platform;
			}
			m_FileLength = r.ReadInt32();
			if (m_FileLength > r.BaseStream.Length)
			{
				return false;
			}
			r.ReadInt32();
			m_NTables = r.ReadInt32();
			m_TableOffset = new uint[m_NTables];
			TableDescriptor.DescriptorDataSet = m_DescriptorDataSet;
			m_Table = new Table[m_NTables];
			if (m_ProgressBar != null)
			{
				m_ProgressBar.Maximum = m_NTables;
			}
			m_CrcHeaderPosition = r.BaseStream.Position;
			m_CrcHeader = r.ReadUInt32();
			for (int i = 0; i < m_NTables; i++)
			{
				m_Table[i] = new Table();
				m_Table[i].LoadTableName(r);
				m_TableOffset[i] = r.ReadUInt32();
			}
			m_CrcShortNamesPosition = r.BaseStream.Position;
			m_CrcShortNames = r.ReadUInt32();
			long position = r.BaseStream.Position;
			for (int j = 0; j < m_NTables; j++)
			{
				if (m_ProgressBar != null)
				{
					m_ProgressBar.Value = j;
				}
				m_Table[j].Load(r, position + m_TableOffset[j]);
			}
			return flag;
		}

		public bool LoadDb()
		{
			if (m_FileName == null)
			{
				return false;
			}
			return LoadDb(m_FileName);
		}

		public bool LoadXml(string xmlFileName)
		{
			if (!File.Exists(xmlFileName))
			{
				return false;
			}
			m_XmlFileName = xmlFileName;
			m_NeedToSaveXmlFile = false;
			m_DescriptorDataSet = new DataSet("XML_Descriptor");
			m_DescriptorDataSet.ReadXml(m_XmlFileName);
			return true;
		}

		public bool LoadXml()
		{
			if (m_XmlFileName == null)
			{
				return false;
			}
			return LoadXml(m_XmlFileName);
		}

		public bool Load()
		{
			if (!LoadXml())
			{
				return false;
			}
			return LoadDb();
		}

		public bool SaveDb(string fileName)
		{
			FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
			DbWriter dbWriter = new DbWriter(fileStream, m_Platform);
			SaveDb(dbWriter);
			dbWriter.Close();
			fileStream.Close();
			ComputeAllCrc();
			return true;
		}

		public bool SaveDb(DbWriter w)
		{
			if (m_ProgressBar != null)
			{
				m_ProgressBar.Maximum = m_NTables;
			}
			w.Write(m_Signature);
			long position = w.BaseStream.Position;
			w.Write(-1L);
			w.Write(m_NTables);
			m_CrcHeaderPosition = w.BaseStream.Position;
			w.Write(-1);
			long position2 = w.BaseStream.Position + 4;
			for (int i = 0; i < m_NTables; i++)
			{
				w.Write(m_Table[i].TableDescriptor.ShortName);
				w.Write(-1);
			}
			m_CrcShortNamesPosition = w.BaseStream.Position;
			w.Write(-1);
			long position3 = w.BaseStream.Position;
			for (int j = 0; j < m_NTables; j++)
			{
				if (m_ProgressBar != null)
				{
					m_ProgressBar.Value = j;
				}
				m_TableOffset[j] = (uint)(w.BaseStream.Position - position3);
				m_Table[j].Save(w);
			}
			m_FileLength = (int)(w.BaseStream.Position - m_SignaturePosition);
			w.BaseStream.Position = position;
			w.Write(m_FileLength);
			w.Write(0);
			w.BaseStream.Position = position2;
			for (int k = 0; k < m_NTables; k++)
			{
				w.Write(m_TableOffset[k]);
				w.BaseStream.Position += 4L;
			}
			w.Seek(0, SeekOrigin.End);
			if (m_ProgressBar != null)
			{
				m_ProgressBar.Value = 0;
			}
			return true;
		}

		public bool SaveXml(string xmlFileName)
		{
			if (m_NeedToSaveXmlFile)
			{
				File.Copy(xmlFileName, xmlFileName + ".bak", overwrite: true);
				m_DescriptorDataSet.WriteXml(xmlFileName, XmlWriteMode.IgnoreSchema);
			}
			m_NeedToSaveXmlFile = false;
			return true;
		}

		public bool SaveXml()
		{
			if (m_XmlFileName == null)
			{
				return false;
			}
			return SaveXml(m_XmlFileName);
		}

		public bool SaveDb()
		{
			if (m_FileName == null)
			{
				return false;
			}
			return SaveDb(m_FileName);
		}

		public void Document()
		{
			if (m_FileName == null)
			{
				return;
			}
			FileStream fileStream = new FileStream(Path.GetFullPath(m_FileName).Replace(".db", ".txt"), FileMode.Create);
			StreamWriter streamWriter = new StreamWriter(fileStream);
			for (int i = 0; i < m_NTables; i++)
			{
				string tableName = m_Table[i].TableDescriptor.TableName;
				streamWriter.WriteLine("static private string t_" + tableName + " = \"" + tableName + "\";");
			}
			for (int j = 0; j < m_NTables; j++)
			{
				string tableName2 = m_Table[j].TableDescriptor.TableName;
				streamWriter.WriteLine("static public int " + tableName2 + ";");
			}
			for (int k = 0; k < m_NTables; k++)
			{
				string tableName3 = m_Table[k].TableDescriptor.TableName;
				streamWriter.WriteLine(tableName3 + " = fifaDbFile.GetTableIndex(t_" + tableName3 + ");");
			}
			for (int l = 0; l < m_NTables; l++)
			{
				string tableName4 = m_Table[l].TableDescriptor.TableName;
				for (int m = 0; m < m_Table[l].TableDescriptor.NFields; m++)
				{
					string fieldName = m_Table[l].TableDescriptor.FieldDescriptors[m].FieldName;
					_ = m_Table[l].TableDescriptor.FieldDescriptors[m].FieldType;
					streamWriter.WriteLine("public static int " + tableName4 + "_" + fieldName + " = -1;");
				}
			}
			for (int n = 0; n < m_NTables; n++)
			{
				string tableName5 = m_Table[n].TableDescriptor.TableName;
				int num = 0;
				for (int num2 = 0; num2 < m_Table[n].TableDescriptor.NFields; num2++)
				{
					string fieldName2 = m_Table[n].TableDescriptor.FieldDescriptors[num2].FieldName;
					FieldDescriptor.EFieldTypes fieldType = m_Table[n].TableDescriptor.FieldDescriptors[num2].FieldType;
					num = m_Table[n].TableDescriptor.FieldDescriptors[num2].TypeIndex;
					streamWriter.WriteLine(tableName5 + "_" + fieldName2 + " = " + num.ToString() + "; //" + fieldType.ToString());
				}
			}
			streamWriter.Close();
			fileStream.Close();
		}

		public DataSet ConvertToDataSet()
		{
			if (m_NTables == 0)
			{
				return null;
			}
			if (m_ProgressBar != null)
			{
				m_ProgressBar.Maximum = m_NTables;
			}
			DataSet dataSet = new DataSet();
			for (int i = 0; i < m_NTables; i++)
			{
				if (m_ProgressBar != null)
				{
					m_ProgressBar.Value = i;
				}
				dataSet.Tables.Add(Table[i].ConvertToDataTable());
			}
			if (m_ProgressBar != null)
			{
				m_ProgressBar.Value = 0;
			}
			return dataSet;
		}

		public void ConvertFromDataSet(DataSet dataSet)
		{
			if (m_ProgressBar != null)
			{
				m_ProgressBar.Maximum = m_NTables;
			}
			for (int i = 0; i < m_NTables; i++)
			{
				if (m_ProgressBar != null)
				{
					m_ProgressBar.Value = i;
				}
				Table[i].ConvertFromDataTable(dataSet.Tables[i]);
			}
			if (m_ProgressBar != null)
			{
				m_ProgressBar.Value = 0;
			}
		}

		public int GetTableIndex(string tableName)
		{
			for (int i = 0; i < NTables; i++)
			{
				if (Table[i].TableDescriptor.TableName == tableName)
				{
					return i;
				}
			}
			return -1;
		}

		public bool Expand()
		{
			bool flag = ExpandTableField("formations", "formationid", 11);
			bool num = true && !flag;
			RecalculateFieldOffset("formations");
			flag = ExpandTableField("teamformationteamstylelinks", "formationid", 11);
			bool num2 = num && !flag;
			RecalculateFieldOffset("teamformationteamstylelinks");
			flag = ExpandTableField("temp_formations", "formationid", 11);
			bool num3 = num2 && !flag;
			RecalculateFieldOffset("temp_formations");
			flag = ExpandTableField("customformations", "formationid", 11);
			bool num4 = num3 && !flag;
			RecalculateFieldOffset("customformations");
			flag = ExpandTableField("teamsheets", "teamsheetid", 11);
			bool num5 = num4 && !flag;
			RecalculateFieldOffset("teamsheetid");
			flag = ExpandTableField("playernames", "nameid", 16);
			bool num6 = num5 && !flag;
			RecalculateFieldOffset("playernames");
			flag = ExpandTableField("career_lastnames", "lastname", 16);
			bool num7 = num6 && !flag;
			RecalculateFieldOffset("career_lastnames");
			flag = ExpandTableField("career_firstnames", "firstname", 16);
			bool num8 = num7 && !flag;
			RecalculateFieldOffset("career_firstnames");
			flag = ExpandTableField("career_commonnames", "firstname", 16);
			bool num9 = num8 && !flag;
			flag = ExpandTableField("career_lastnames", "lastname", 16);
			bool num10 = num9 && !flag;
			RecalculateFieldOffset("career_commonnames");
			flag = ExpandTableField("trainingteamplayernames", "nameid", 16);
			bool num11 = num10 && !flag;
			RecalculateFieldOffset("trainingteamplayernames");
			flag = ExpandTableField("players", "firstnameid", 16);
			bool num12 = num11 && !flag;
			flag = ExpandTableField("players", "lastnameid", 16);
			bool num13 = num12 && !flag;
			flag = ExpandTableField("players", "commonnameid", 16);
			bool num14 = num13 && !flag;
			flag = ExpandTableField("players", "playerjerseynameid", 16);
			bool num15 = num14 && !flag;
			RecalculateFieldOffset("players");
			flag = ExpandTableField("trainingteamplayernames", "firstnameid", 16);
			bool num16 = num15 && !flag;
			flag = ExpandTableField("trainingteamplayernames", "lastnameid", 16);
			bool num17 = num16 && !flag;
			flag = ExpandTableField("trainingteamplayernames", "commonnameid", 16);
			bool num18 = num17 && !flag;
			flag = ExpandTableField("trainingteamplayernames", "playerjerseynameid", 16);
			bool num19 = num18 && !flag;
			RecalculateFieldOffset("trainingteamplayernames");
			flag = ExpandTableField("referee", "refereeid", 10);
			bool num20 = num19 && !flag;
			RecalculateFieldOffset("referee");
			flag = ExpandTableField("leaguerefereelinks", "refereeid", 10);
			bool result = num20 && !flag;
			RecalculateFieldOffset("leaguerefereelinks");
			m_NeedToSaveXmlFile = true;
			return result;
		}

		private bool ExpandTableField(string tableName, string fieldName, int nBits)
		{
			return GetTable(tableName)?.ExpandField(fieldName, nBits) ?? false;
		}

		private bool ExpandTableField(string tableName, string fieldName, int nBits, int minValue)
		{
			return GetTable(tableName)?.ExpandField(fieldName, nBits, minValue) ?? false;
		}

		private bool RecalculateFieldOffset(string tableName)
		{
			Table table = GetTable(tableName);
			if (table == null)
			{
				return false;
			}
			table.TableDescriptor.RecalculateFieldOffset();
			return true;
		}

		public Table GetTable(string longName)
		{
			for (int i = 0; i < m_NTables; i++)
			{
				if (m_Table[i].TableDescriptor.TableName == longName)
				{
					return m_Table[i];
				}
			}
			return null;
		}
	}
}
