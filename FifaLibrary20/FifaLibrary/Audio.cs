using System;
using System.Data;
using System.IO;

namespace FifaLibrary
{
	public class Audio
	{
		private string m_StandardLanguage;

		private string m_XmlFileName;

		private string m_SbrFileName;

		private string m_SbsFileName;

		private string m_BigFileName;

		private string m_BhFileName;

		private DataSet m_AudioXmlDataSet;

		private uint m_SimpleSurnameKey;

		private uint m_PlayerNamesKey;

		private SbrFile m_SbrFile;

		public string StandardLanguage
		{
			get
			{
				return m_StandardLanguage;
			}
			set
			{
				m_StandardLanguage = value;
				if (m_StandardLanguage == null)
				{
					m_XmlFileName = null;
					m_SbrFileName = null;
					m_SbsFileName = null;
					m_BigFileName = null;
				}
				else
				{
					m_XmlFileName = "audiodata/speechdata/" + m_StandardLanguage + "/" + m_StandardLanguage + ".xml";
					m_SbrFileName = "audiodata/speechdata/" + m_StandardLanguage + "/" + m_StandardLanguage + "_bank.sbr";
					m_SbsFileName = "audiodata/speechdata/" + m_StandardLanguage + "/" + m_StandardLanguage + "_bank.sbs";
					m_BigFileName = m_StandardLanguage + ".big";
					m_BhFileName = m_StandardLanguage + ".bh";
				}
			}
		}

		public string XmlFileName => m_XmlFileName;

		public string SbrFileName => m_SbrFileName;

		public string SbsFileName => m_SbsFileName;

		public string BigFileName => m_BigFileName;

		public string BhFileName => m_BhFileName;

		public DataSet DescriptorDataSet
		{
			get
			{
				return m_AudioXmlDataSet;
			}
			set
			{
				m_AudioXmlDataSet = value;
			}
		}

		public SbrFile SbrFile => m_SbrFile;

		public bool IsAudioExtracted()
		{
			if (m_StandardLanguage == null)
			{
				return false;
			}
			if (!File.Exists(FifaEnvironment.GameDir + XmlFileName))
			{
				return false;
			}
			if (!File.Exists(FifaEnvironment.GameDir + SbrFileName))
			{
				return false;
			}
			if (!File.Exists(FifaEnvironment.GameDir + SbsFileName))
			{
				return false;
			}
			FifaEnvironment.FifaFat.HideFile(XmlFileName);
			FifaEnvironment.FifaFat.HideFile(SbrFileName);
			FifaEnvironment.FifaFat.HideFile(SbsFileName);
			return true;
		}

		public bool IsAudioPresent()
		{
			if (m_StandardLanguage == null)
			{
				return false;
			}
			if (!File.Exists(FifaEnvironment.GameDir + BigFileName))
			{
				return false;
			}
			return true;
		}

		public bool ExtractAudio()
		{
			if (m_StandardLanguage == null)
			{
				return false;
			}
			if (!IsAudioPresent())
			{
				return false;
			}
			if (!FifaEnvironment.FifaFat.ExtractFile(XmlFileName))
			{
				return false;
			}
			if (!FifaEnvironment.FifaFat.ExtractFile(SbrFileName))
			{
				return false;
			}
			if (!FifaEnvironment.FifaFat.ExtractFile(SbsFileName))
			{
				return false;
			}
			return true;
		}

		public bool LoadXml()
		{
			string text = FifaEnvironment.GameDir + m_XmlFileName;
			if (!File.Exists(text))
			{
				return false;
			}
			m_AudioXmlDataSet = new DataSet();
			m_AudioXmlDataSet.ReadXml(text);
			DataTable dataTable = m_AudioXmlDataSet.Tables["SampleGroup"];
			string b = "pSIMPLE_SURNAME";
			string b2 = "pPLAYER_NAMES";
			string text2 = null;
			string text3 = null;
			for (int i = 0; i < dataTable.Rows.Count; i++)
			{
				DataRow dataRow = dataTable.Rows[i];
				if (dataRow["Name"].ToString() == b)
				{
					text2 = dataRow["SampleGroupKey"].ToString();
				}
				if (dataRow["Name"].ToString() == b2)
				{
					text3 = dataRow["SampleGroupKey"].ToString();
				}
			}
			if (text2 != null)
			{
				m_SimpleSurnameKey = Convert.ToUInt32(text2, 16);
			}
			if (text3 != null)
			{
				m_PlayerNamesKey = Convert.ToUInt32(text3, 16);
			}
			if (text2 != null)
			{
				return text3 != null;
			}
			return false;
		}

		public bool LoadSbr()
		{
			m_SbrFile = new SbrFile(m_SbrFileName);
			return m_SbrFile.Load();
		}
	}
}
