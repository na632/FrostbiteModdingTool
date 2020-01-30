using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary
{
	public class FifaFat
	{
		public enum EFifaFatSaveOption
		{
			SaveAlways,
			SaveOnCommand
		}

		private ToolStripProgressBar m_ProgressBar;

		private string m_GamePath;

		private BhFile[] m_BhFiles = new BhFile[100];

		private FifaBigFile[] m_BigFiles = new FifaBigFile[100];

		private bool[] m_NeedToSaveBig = new bool[100];

		private string[] m_BigFileNames = new string[100];

		private string[] m_BhFileNames = new string[100];

		private int m_NBigFiles;

		private EFifaFatSaveOption m_SaveOption;

		private int m_MinZdata = 8;

		private int m_DeafultDataIndex = -1;

		public ToolStripProgressBar ProgressBar
		{
			set
			{
				m_ProgressBar = value;
			}
		}

		public string GamePath => m_GamePath;

		public int NBigFiles => m_NBigFiles;

		public EFifaFatSaveOption SaveOption
		{
			get
			{
				return m_SaveOption;
			}
			set
			{
				m_SaveOption = value;
			}
		}

		public int Min_Zdata
		{
			get
			{
				return m_MinZdata;
			}
			set
			{
				m_MinZdata = value;
			}
		}

		public BhFile GetBhFile(int index)
		{
			if (index < 0 || index >= m_NBigFiles)
			{
				return null;
			}
			return m_BhFiles[index];
		}

		public FifaBigFile GetBigFile(int index)
		{
			if (index < 0 || index >= m_NBigFiles)
			{
				return null;
			}
			if (m_BigFiles[index] != null)
			{
				return m_BigFiles[index];
			}
			m_BigFiles[index] = new FifaBigFile(m_BigFileNames[index]);
			return m_BigFiles[index];
		}

		public static FifaFat Create(string gamePath)
		{
			if (!Directory.Exists(gamePath))
			{
				return null;
			}
			FifaFat fifaFat = new FifaFat();
			fifaFat.m_GamePath = gamePath;
			fifaFat.Load();
			fifaFat.m_SaveOption = EFifaFatSaveOption.SaveAlways;
			return fifaFat;
		}

		private void LoadBigFromFolder(string folder)
		{
			if (!Directory.Exists(folder))
			{
				return;
			}
			string[] files = Directory.GetFiles(folder, "*.big");
			if (files == null)
			{
				return;
			}
			string[] array = files;
			foreach (string text in array)
			{
				string text2 = text;
				string text3 = folder + Path.GetFileNameWithoutExtension(text) + ".bh";
				if (!File.Exists(text2))
				{
					continue;
				}
				m_BigFileNames[m_NBigFiles] = text2;
				m_BhFileNames[m_NBigFiles] = text3;
				if (File.Exists(text3))
				{
					m_BhFiles[m_NBigFiles] = new BhFile(text3);
				}
				else
				{
					m_BigFiles[m_NBigFiles] = new FifaBigFile(text2);
					if (m_BhFiles[m_NBigFiles] == null)
					{
						m_BigFiles[m_NBigFiles].LoadArchivedFiles();
						m_BhFiles[m_NBigFiles] = new BhFile(m_BigFiles[m_NBigFiles], hideExternalFiles: true);
					}
				}
				m_NBigFiles++;
			}
		}

		private void Load()
		{
			m_NBigFiles = 0;
			string gamePath = m_GamePath;
			LoadBigFromFolder(gamePath);
			gamePath = m_GamePath + "data\\ui\\imgAssets\\heads\\";
			LoadBigFromFolder(gamePath);
		}

		public void Save()
		{
			for (int i = 0; i <= m_NBigFiles; i++)
			{
				if (m_NeedToSaveBig[i])
				{
					GetBigFile(i).Save();
				}
				if (m_BhFiles[i] != null)
				{
					m_BhFiles[i].Save();
				}
			}
		}

		public void RegenerateAllBh(bool hideExternalFiles)
		{
			if (m_ProgressBar != null)
			{
				m_ProgressBar.Maximum = m_NBigFiles;
				m_ProgressBar.Value = 0;
			}
			for (int i = 0; i < m_NBigFiles; i++)
			{
				string bigFileNameByIndex = GetBigFileNameByIndex(i);
				if (File.Exists(bigFileNameByIndex))
				{
					BhFile.Regenerate(bigFileNameByIndex, hideExternalFiles);
					if (m_ProgressBar != null)
					{
						m_ProgressBar.Value = i;
					}
				}
			}
			if (m_ProgressBar != null)
			{
				m_ProgressBar.Value = 0;
			}
			Load();
		}

		public bool IsArchivedFilePresent(string fileName)
		{
			return GetArchivingBhFileIndex(fileName) >= 0;
		}

		public bool IsPhisycalFilePresent(string fileName)
		{
			return File.Exists(m_GamePath + fileName);
		}

		public bool IsHeadFilePresent(string fileName)
		{
			for (int num = m_NBigFiles - 1; num >= 0; num--)
			{
				if (m_BhFileNames[num].Contains("heads") && m_BhFiles[num].GetArchivedFileIndex(fileName) >= 0)
				{
					return true;
				}
			}
			return false;
		}

		public string GetBigFileNameByIndex(int index)
		{
			if (index < m_NBigFiles)
			{
				return m_BigFileNames[index];
			}
			return null;
		}

		public string GetBhFileName(int bigIndex)
		{
			return m_GamePath + "data" + bigIndex.ToString() + ".bh";
		}

		public string GetBigFileName(int bigIndex)
		{
			return m_GamePath + "data" + bigIndex.ToString() + ".big";
		}

		public static string GetBhFileName(int bigIndex, string rootPath)
		{
			return rootPath + "data" + bigIndex.ToString() + ".bh";
		}

		public static string GetBigFileName(int bigIndex, string rootPath)
		{
			return rootPath + "\\Game\\data" + bigIndex.ToString() + ".big";
		}

		public FifaFile GetArchivedFile(string fileName)
		{
			ulong hash = FifaUtil.ComputeBhHash(fileName);
			for (int num = m_NBigFiles - 1; num >= 0; num--)
			{
				if (m_BhFiles[num] != null)
				{
					int archivedFileIndex = m_BhFiles[num].GetArchivedFileIndex(hash);
					if (archivedFileIndex >= 0)
					{
						return GetBigFile(num).GetArchivedFile(archivedFileIndex);
					}
				}
			}
			return null;
		}

		public bool DeleteFile(string fileName)
		{
			int archivingBhFileIndex = GetArchivingBhFileIndex(fileName);
			if (archivingBhFileIndex < 0)
			{
				return false;
			}
			int archivedFileIndex = m_BhFiles[archivingBhFileIndex].GetArchivedFileIndex(fileName);
			if (archivedFileIndex < 0)
			{
				return false;
			}
			GetBigFile(archivingBhFileIndex).Delete(archivedFileIndex);
			m_BhFiles[archivingBhFileIndex].Delete(archivedFileIndex);
			if (m_SaveOption == EFifaFatSaveOption.SaveAlways)
			{
				GetBigFile(archivingBhFileIndex).Save();
				m_BhFiles[archivingBhFileIndex].Save();
			}
			else
			{
				m_NeedToSaveBig[archivingBhFileIndex] = true;
			}
			return true;
		}

		public bool HideFile(string fileName)
		{
			int archivingBhFileIndex = GetArchivingBhFileIndex(fileName);
			if (archivingBhFileIndex < 0)
			{
				return false;
			}
			int archivedFileIndex = m_BhFiles[archivingBhFileIndex].GetArchivedFileIndex(fileName);
			return m_BhFiles[archivingBhFileIndex].Hide(archivedFileIndex);
		}

		public bool RestoreFile(string fileName)
		{
			int fileIndex;
			int archivingBigFileIndex = GetArchivingBigFileIndex(fileName, out fileIndex);
			if (archivingBigFileIndex < 0)
			{
				return false;
			}
			return m_BhFiles[archivingBigFileIndex].Restore(fileName, fileIndex);
		}

		public bool ExportFile(string fileName, string exportDir)
		{
			int archivingBhFileIndex = GetArchivingBhFileIndex(fileName);
			if (archivingBhFileIndex < 0)
			{
				return false;
			}
			int archivedFileIndex = m_BhFiles[archivingBhFileIndex].GetArchivedFileIndex(fileName);
			return GetBigFile(archivingBhFileIndex).Export(archivedFileIndex, exportDir);
		}

		public bool ExportFile(string fileName)
		{
			return ExportFile(fileName, m_GamePath);
		}

		public bool ExtractFile(string fileName)
		{
			int archivingBhFileIndex = GetArchivingBhFileIndex(fileName);
			if (archivingBhFileIndex < 0)
			{
				return false;
			}
			int archivedFileIndex = m_BhFiles[archivingBhFileIndex].GetArchivedFileIndex(fileName);
			bool flag = GetBigFile(archivingBhFileIndex).Export(archivedFileIndex, m_GamePath);
			if (flag)
			{
				flag = m_BhFiles[archivingBhFileIndex].Hide(archivedFileIndex);
			}
			return flag;
		}

		public bool ImportFileAs(string fileName, string archivedName, bool delete, ECompressionMode compressionMode)
		{
			delete = (delete && !fileName.Contains("#"));
			archivedName = archivedName.Replace('\\', '/');
			int num = m_DeafultDataIndex;
			if (num < 0)
			{
				num = GetArchivingBhFileIndex(archivedName);
				if (num < 0)
				{
					num = GetAvailableBigFileIndex();
				}
			}
			if (num < 0)
			{
				return false;
			}
			GetBigFile(num).ImportFileAs(fileName, archivedName, compressionMode);
			if (m_SaveOption == EFifaFatSaveOption.SaveAlways)
			{
				GetBigFile(num).Save();
				m_BhFiles[num] = new BhFile(GetBigFile(num), hideExternalFiles: true);
				m_BhFiles[num].Save();
			}
			else
			{
				m_BhFiles[num] = new BhFile(GetBigFile(num), hideExternalFiles: true);
				m_NeedToSaveBig[num] = true;
			}
			if (delete)
			{
				File.Delete(fileName);
			}
			return true;
		}

		public bool ImportFile(string fileName, bool delete, ECompressionMode compressionMode)
		{
			string fileName2 = Path.GetFileName(fileName);
			return ImportFileAs(fileName, fileName2, delete, compressionMode);
		}

		public ECompressionMode GetCompressionMode(string fileName)
		{
			return GetArchivedFile(fileName)?.CompressionMode ?? ECompressionMode.None;
		}

		private FifaBigFile GetArchivingBigFile(string fileName)
		{
			int archivingBhFileIndex = GetArchivingBhFileIndex(fileName);
			if (archivingBhFileIndex >= 0)
			{
				return GetBigFile(archivingBhFileIndex);
			}
			return null;
		}

		private int GetArchivingBhFileIndex(string fileName)
		{
			ulong hash = FifaUtil.ComputeBhHash(fileName);
			for (int num = m_NBigFiles - 1; num >= 0; num--)
			{
				if (m_BhFiles[num] != null && m_BhFiles[num].IsArchivedFilePresent(hash))
				{
					return num;
				}
			}
			return -1;
		}

		private int GetArchivingBigFileIndex(string fileName, out int fileIndex)
		{
			fileIndex = -1;
			for (int num = m_NBigFiles - 1; num >= 0; num--)
			{
				fileIndex = GetBigFile(num).GetArchivedFileIndex(fileName, useFullPath: true);
				if (fileIndex >= 0)
				{
					return num;
				}
			}
			return -1;
		}

		private FifaBigFile GetAvailableBigFile()
		{
			int availableBigFileIndex = GetAvailableBigFileIndex();
			if (availableBigFileIndex < 0)
			{
				return null;
			}
			return GetBigFile(availableBigFileIndex);
		}

		private int GetAvailableBigFileIndex()
		{
			for (int i = m_MinZdata; i < 100; i++)
			{
				if (m_BigFiles[i] != null)
				{
					if (m_BigFiles[i].NFiles < 500)
					{
						return i;
					}
					continue;
				}
				CreateNewBigFile(i);
				return i;
			}
			return -1;
		}

		public void CreateNewBigFile(int index)
		{
			string bigFileName = GetBigFileName(index);
			FileStream fileStream = new FileStream(bigFileName, FileMode.Create, FileAccess.Write);
			fileStream.WriteByte(0);
			fileStream.Close();
			m_BigFiles[index] = new FifaBigFile(bigFileName);
			m_BhFiles[index] = new BhFile(m_BigFiles[index], hideExternalFiles: false);
			m_NeedToSaveBig[index] = false;
			if (index > m_NBigFiles)
			{
				m_NBigFiles = index;
			}
		}

		public void ResetDefaultZdata()
		{
			m_DeafultDataIndex = -1;
		}

		public ArrayList FindDuplicatedFiles()
		{
			ArrayList arrayList = new ArrayList();
			if (m_ProgressBar != null)
			{
				m_ProgressBar.Maximum = 100;
			}
			int num = 0;
			for (int i = 0; i < 100; i++)
			{
				if (m_ProgressBar != null)
				{
					m_ProgressBar.Value = i;
				}
				if (m_BhFiles[i] == null)
				{
					continue;
				}
				for (int j = 0; j < m_BhFiles[i].NFiles; j++)
				{
					ulong hash = m_BhFiles[i].GetHash(j);
					for (int k = i + 1; k < 100; k++)
					{
						if (m_BhFiles[k] != null && m_BhFiles[k].GetArchivedFileIndex(hash) >= 0)
						{
							num++;
							FifaBigFile fifaBigFile = new FifaBigFile(GetBigFileName(i));
							FifaFile fifaFile = fifaBigFile.Files[j];
							string value = fifaFile.Name + " is duplicated in " + fifaBigFile.Name + " and " + Path.GetFileName(GetBigFileName(k)) + "\r\n";
							arrayList.Add(value);
						}
					}
				}
			}
			if (m_ProgressBar != null)
			{
				m_ProgressBar.Value = 0;
			}
			if (num == 0)
			{
				arrayList.Add("No duplicated files found.");
			}
			else
			{
				arrayList.Add(num.ToString() + " duplicated files found.");
			}
			return arrayList;
		}
	}
}
