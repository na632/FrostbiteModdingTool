using System;
using System.IO;

namespace FifaLibrary
{
	public class BhFile
	{
		private int m_TotalFileSize;

		private int m_NFiles;

		private BhFileReference[] m_BhFileReference;

		private string m_BhName;

		private int m_TotalBigFileSize;

		public int NFiles => m_NFiles;

		private bool Load(string fileName)
		{
			if (!File.Exists(fileName))
			{
				return false;
			}
			m_BhName = fileName;
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			if (binaryReader.BaseStream.Length < 16)
			{
				return false;
			}
			char[] array = binaryReader.ReadChars(4);
			if (array[0] != 'V' || array[1] != 'i' || array[2] != 'V' || array[3] != '4')
			{
				binaryReader.Close();
				fileStream.Close();
				return false;
			}
			m_TotalFileSize = binaryReader.ReadInt32();
			m_NFiles = FifaUtil.SwapEndian(binaryReader.ReadInt32());
			FifaUtil.SwapEndian(binaryReader.ReadInt32());
			m_BhFileReference = new BhFileReference[m_NFiles];
			for (int i = 0; i < m_NFiles; i++)
			{
				m_BhFileReference[i] = new BhFileReference();
				m_BhFileReference[i].Load(binaryReader);
			}
			if (binaryReader.BaseStream.Position <= binaryReader.BaseStream.Length - 4)
			{
				m_TotalBigFileSize = FifaUtil.SwapEndian(binaryReader.ReadInt32());
			}
			binaryReader.Close();
			fileStream.Close();
			return true;
		}

		public bool Load(FifaBigFile bigFile, bool hideExternalFiles)
		{
			string str;
			if (FifaEnvironment.Year == 14)
			{
				int num = bigFile.PhysicalName.IndexOf("\\Game\\", 0, StringComparison.InvariantCultureIgnoreCase);
				str = ((num < 0) ? (Path.GetDirectoryName(bigFile.PhysicalName) + "\\") : bigFile.PhysicalName.Substring(0, num + 6));
			}
			else
			{
				int num = bigFile.PhysicalName.IndexOf("\\FIFA 16\\", 0, StringComparison.InvariantCultureIgnoreCase);
				str = ((num < 0) ? (Path.GetDirectoryName(bigFile.PhysicalName) + "\\") : bigFile.PhysicalName.Substring(0, num + 9));
			}
			m_BhName = Path.GetDirectoryName(bigFile.PhysicalName) + "\\" + Path.GetFileNameWithoutExtension(bigFile.PhysicalName) + ".bh";
			m_NFiles = bigFile.NFiles;
			if (m_BhName.Contains("FIFA 11"))
			{
				m_TotalFileSize = 16 + (m_NFiles + 1) * 20;
			}
			else
			{
				m_TotalFileSize = 16 + m_NFiles * 20;
			}
			m_BhFileReference = new BhFileReference[m_NFiles];
			m_TotalBigFileSize = bigFile.UncompressedSize;
			for (int i = 0; i < m_NFiles; i++)
			{
				uint startPosition = bigFile.Headers[i].StartPosition;
				int size = bigFile.Headers[i].Size;
				int num2 = 0;
				if (bigFile.Files[i] == null)
				{
					bigFile.LoadArchivedFile(i);
				}
				num2 = ((bigFile.Files[i].CompressionMode == ECompressionMode.Compressed_10FB) ? bigFile.Files[i].UncompressedSize : 0);
				string name = bigFile.Headers[i].Name;
				m_BhFileReference[i] = new BhFileReference(startPosition, size, num2, name);
				if (hideExternalFiles && File.Exists(str + name))
				{
					m_BhFileReference[i].Hide();
				}
			}
			return true;
		}

		public bool Hide(string fileName)
		{
			int archivedFileIndex = GetArchivedFileIndex(fileName);
			return Hide(archivedFileIndex);
		}

		public bool Hide(int fileIndex)
		{
			if (fileIndex < 0 || fileIndex >= m_NFiles)
			{
				return false;
			}
			m_BhFileReference[fileIndex].Hide();
			return true;
		}

		public bool IsHidden(int fileIndex)
		{
			if (fileIndex < 0 || fileIndex >= m_NFiles)
			{
				return false;
			}
			return m_BhFileReference[fileIndex].IsHidden();
		}

		public bool IsHidden(string fileName)
		{
			int archivedFileIndex = GetArchivedFileIndex(fileName);
			return IsHidden(archivedFileIndex);
		}

		public bool Restore(string fileName, int fileIndex)
		{
			if (fileIndex < 0 || fileIndex >= m_NFiles)
			{
				return false;
			}
			m_BhFileReference[fileIndex].Restore(fileName);
			return true;
		}

		public BhFile(FifaBigFile bigFile, bool hideExternalFiles)
		{
			Load(bigFile, hideExternalFiles);
		}

		public BhFile(string fileName)
		{
			if (Load(fileName))
			{
				m_BhName = fileName;
			}
			else
			{
				Reset();
			}
		}

		private void Reset()
		{
			m_TotalFileSize = 0;
			m_NFiles = 0;
			m_BhFileReference = null;
			m_BhName = null;
			m_TotalBigFileSize = 0;
		}

		public static void Regenerate(FifaBigFile bigFile, bool hideExternalFiles)
		{
			new BhFile(bigFile, hideExternalFiles).Save();
		}

		public static void Regenerate(string bigFileName, bool hideExternalFiles)
		{
			FifaBigFile fifaBigFile = new FifaBigFile(bigFileName);
			if (fifaBigFile != null)
			{
				fifaBigFile.LoadArchivedFiles();
				Regenerate(fifaBigFile, hideExternalFiles);
			}
		}

		public bool Save()
		{
			string bhName = m_BhName;
			if (m_BhName == null)
			{
				return false;
			}
			if (File.Exists(bhName) && (File.GetAttributes(bhName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
			{
				File.SetAttributes(bhName, FileAttributes.Archive);
			}
			FileStream fileStream = new FileStream(bhName, FileMode.Create, FileAccess.Write);
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			if (m_NFiles == 0)
			{
				binaryWriter.Write((byte)0);
			}
			else
			{
				binaryWriter.Write((byte)86);
				binaryWriter.Write((byte)105);
				binaryWriter.Write((byte)86);
				binaryWriter.Write((byte)52);
				binaryWriter.Write(m_TotalFileSize);
				binaryWriter.Write(FifaUtil.SwapEndian(m_NFiles));
				binaryWriter.Write(FifaUtil.SwapEndian(m_TotalFileSize));
				for (int i = 0; i < m_NFiles; i++)
				{
					m_BhFileReference[i].Save(binaryWriter);
				}
			}
			binaryWriter.Close();
			fileStream.Close();
			return true;
		}

		private bool SaveEmptyReference(BinaryWriter w)
		{
			for (int i = 0; i < 4; i++)
			{
				w.Write(0);
			}
			return true;
		}

		public int GetArchivedFileIndex(ulong hash)
		{
			for (int i = 0; i < m_NFiles; i++)
			{
				if (m_BhFileReference[i].Hash == hash)
				{
					return i;
				}
			}
			return -1;
		}

		public int GetArchivedFileIndex(string fileName)
		{
			ulong hash = FifaUtil.ComputeBhHash(fileName);
			return GetArchivedFileIndex(hash);
		}

		public ulong GetHash(int fileIndex)
		{
			if (fileIndex < m_NFiles)
			{
				return m_BhFileReference[fileIndex].Hash;
			}
			return 0uL;
		}

		public bool IsArchivedFilePresent(string fileName)
		{
			ulong hash = FifaUtil.ComputeBhHash(fileName);
			return GetArchivedFileIndex(hash) >= 0;
		}

		public bool IsArchivedFilePresent(ulong hash)
		{
			return GetArchivedFileIndex(hash) >= 0;
		}

		public bool Delete(int index)
		{
			if (index >= m_NFiles)
			{
				return false;
			}
			m_NFiles--;
			for (int i = index; i < m_NFiles; i++)
			{
				m_BhFileReference[i] = m_BhFileReference[i + 1];
			}
			m_TotalFileSize -= 20;
			return true;
		}
	}
}
