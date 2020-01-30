using System;
using System.Collections;
using System.IO;

namespace FifaLibrary
{
	public class FifaBigFile : FifaFile
	{
		public class StringComparer : IComparer
		{
			int IComparer.Compare(object x, object y)
			{
				string strA = (string)x;
				string strB = (string)y;
				return string.Compare(strA, strB, StringComparison.Ordinal);
			}
		}

		private int m_TotalFileSize;

		private int m_NFiles;

		private int m_HeaderSize;

		private FifaFileHeader[] m_Headers;

		private static StringComparer s_StringComparer = new StringComparer();

		private FifaFile[] m_Files;

		private byte[] m_EndSignature;

		private int m_Alignement = 16;

		public int NFiles => m_NFiles;

		public FifaFileHeader[] Headers => m_Headers;

		public FifaFile[] Files => m_Files;

		private int EstimateAlignement()
		{
			int num = 256;
			for (int i = 0; i < m_NFiles; i++)
			{
				int num2 = FifaUtil.ComputeAlignementLong(m_Headers[i].StartPosition);
				if (num2 < num)
				{
					num = num2;
				}
			}
			return num;
		}

		public FifaBigFile(FifaFile fifaFile)
			: base(fifaFile)
		{
			if (base.IsCompressed)
			{
				Decompress();
			}
			BinaryReader reader = GetReader();
			Load(reader);
			ReleaseReader(reader);
		}

		public FifaBigFile(string fileName)
			: base(fileName, isAnArchive: true)
		{
			if (base.IsCompressed)
			{
				Decompress();
			}
			BinaryReader reader = GetReader();
			Load(reader);
			ReleaseReader(reader);
		}

		private bool Load(BinaryReader r)
		{
			if (r == null)
			{
				return false;
			}
			if (r.BaseStream.Length < 16)
			{
				return false;
			}
			char[] array = r.ReadChars(4);
			if (array[0] != 'B' || array[1] != 'I' || array[2] != 'G' || (array[3] != 'F' && array[3] != '4'))
			{
				return false;
			}
			m_TotalFileSize = r.ReadInt32();
			m_NFiles = FifaUtil.SwapEndian(r.ReadInt32());
			m_HeaderSize = FifaUtil.SwapEndian(r.ReadInt32());
			m_Headers = new FifaFileHeader[m_NFiles];
			m_Files = new FifaFile[m_NFiles];
			for (int i = 0; i < m_NFiles; i++)
			{
				m_Headers[i] = new FifaFileHeader(this);
				m_Headers[i].Load(r);
			}
			if (m_HeaderSize == (int)r.BaseStream.Position + 8)
			{
				m_EndSignature = r.ReadBytes(8);
			}
			m_Alignement = EstimateAlignement();
			return true;
		}

		public bool LoadArchivedFiles()
		{
			BinaryReader reader = GetReader();
			m_Files = new FifaFile[m_NFiles];
			for (int i = 0; i < m_NFiles; i++)
			{
				m_Files[i] = new FifaFile(m_Headers[i], reader);
			}
			ReleaseReader(reader);
			return true;
		}

		public bool LoadArchivedFile(int fileIndex)
		{
			if (fileIndex < 0 || fileIndex >= m_NFiles)
			{
				return false;
			}
			BinaryReader reader = GetReader();
			m_Files[fileIndex] = new FifaFile(m_Headers[fileIndex], reader);
			ReleaseReader(reader);
			return true;
		}

		public FifaFile[] GetArchivedFiles(string searchPattern, bool useFullPath)
		{
			bool[] array = new bool[m_NFiles];
			int num = 0;
			for (int i = 0; i < m_NFiles; i++)
			{
				string text = m_Headers[i].Name;
				if (!useFullPath)
				{
					text = Path.GetFileName(text);
				}
				bool flag;
				array[i] = (flag = FifaUtil.CompareWildcardString(searchPattern, text));
				if (flag)
				{
					num++;
				}
			}
			FifaFile[] array2 = new FifaFile[num];
			if (num == 0)
			{
				return array2;
			}
			BinaryReader binaryReader = null;
			for (int j = 0; j < m_NFiles; j++)
			{
				if (array[j] && m_Files[j] == null)
				{
					binaryReader = GetReader();
					break;
				}
			}
			num = 0;
			for (int k = 0; k < m_NFiles; k++)
			{
				if (array[k])
				{
					if (m_Files[k] == null)
					{
						m_Files[k] = new FifaFile(m_Headers[k], binaryReader);
					}
					array2[num++] = m_Files[k];
				}
			}
			if (binaryReader != null)
			{
				ReleaseReader(binaryReader);
			}
			return array2;
		}

		public string[] GetArchivedFileNames(string searchPattern, bool useFullPath)
		{
			bool[] array = new bool[m_NFiles];
			int num = 0;
			for (int i = 0; i < m_NFiles; i++)
			{
				string text = m_Headers[i].Name;
				if (!useFullPath)
				{
					text = Path.GetFileName(text);
				}
				bool flag;
				array[i] = (flag = FifaUtil.CompareWildcardString(searchPattern, text));
				if (flag)
				{
					num++;
				}
			}
			string[] array2 = new string[num];
			num = 0;
			for (int j = 0; j < m_NFiles; j++)
			{
				if (array[j])
				{
					array2[num++] = m_Headers[j].Name;
				}
			}
			return array2;
		}

		public FifaFile GetArchivedFile(string fileName, bool useFullPath)
		{
			FifaFile[] archivedFiles = GetArchivedFiles(fileName, useFullPath);
			if (archivedFiles.Length == 0)
			{
				return null;
			}
			if (archivedFiles.Length > 1)
			{
				return null;
			}
			return archivedFiles[0];
		}

		public int GetArchivedFileIndex(string fileName, bool useFullPath)
		{
			if (!useFullPath)
			{
				fileName = Path.GetFileName(fileName);
			}
			for (int i = 0; i < m_NFiles; i++)
			{
				if (FifaUtil.CompareWildcardString(fileName, m_Headers[i].Name))
				{
					return i;
				}
			}
			return -1;
		}

		public FifaFile GetArchivedFile(int fileIndex)
		{
			if (fileIndex >= 0 && fileIndex < NFiles)
			{
				if (m_Files[fileIndex] == null)
				{
					BinaryReader reader = GetReader();
					m_Files[fileIndex] = new FifaFile(m_Headers[fileIndex], reader);
					ReleaseReader(reader);
				}
				return m_Files[fileIndex];
			}
			return null;
		}

		public bool Export(string fileName, string exportDir)
		{
			FifaFile archivedFile = GetArchivedFile(fileName, useFullPath: true);
			if (archivedFile == null)
			{
				return false;
			}
			if (archivedFile.UncompressedSize == 0)
			{
				return false;
			}
			return archivedFile.Export(exportDir);
		}

		public bool Export(int fileIndex, string exportDir)
		{
			FifaFile archivedFile = GetArchivedFile(fileIndex);
			if (archivedFile == null)
			{
				return false;
			}
			if (archivedFile.UncompressedSize == 0)
			{
				return false;
			}
			return archivedFile.Export(exportDir);
		}

		public bool Export(string[] fileNames, string exportDir)
		{
			bool flag = false;
			if (fileNames.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < fileNames.Length; i++)
			{
				bool flag2 = Export(fileNames[i], exportDir);
				flag |= flag2;
			}
			return flag;
		}

		public void ImportReplacingFile(string path, int fileIndex)
		{
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			byte[] buffer = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
			fileStream.Close();
			binaryReader.Close();
			FifaFile archivedFile = GetArchivedFile(fileIndex);
			if (archivedFile.CompressionMode == ECompressionMode.Chunkzip2)
			{
				m_Files[fileIndex] = new FifaFile(this, buffer, archivedFile.Name, ECompressionMode.None);
			}
			else
			{
				m_Files[fileIndex] = new FifaFile(this, buffer, archivedFile.Name, archivedFile.CompressionMode);
			}
		}

		public int ImportNewFile(string path, ECompressionMode compressionMode)
		{
			string fileName = Path.GetFileName(path);
			return ImportNewFileAs(path, fileName, compressionMode);
		}

		public int ImportNewFileAs(string path, string archivedName, ECompressionMode compressionMode)
		{
			if (m_Files == null || m_Files.Length <= m_NFiles + 1)
			{
				Resize(m_NFiles + 32);
			}
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			byte[] buffer = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
			fileStream.Close();
			binaryReader.Close();
			m_Files[m_NFiles] = new FifaFile(this, buffer, archivedName, compressionMode);
			m_NFiles++;
			return m_NFiles - 1;
		}

		public void ImportFile(string path, ECompressionMode compressionMode)
		{
			string fileName = Path.GetFileName(path);
			int archivedFileIndex = GetArchivedFileIndex(fileName, useFullPath: true);
			if (archivedFileIndex != -1)
			{
				ImportReplacingFile(path, archivedFileIndex);
			}
			else
			{
				ImportNewFile(path, compressionMode);
			}
		}

		public void ImportFileAs(string path, string archivedName, ECompressionMode compressionMode)
		{
			int archivedFileIndex = GetArchivedFileIndex(archivedName, useFullPath: true);
			if (archivedFileIndex != -1)
			{
				ImportReplacingFile(path, archivedFileIndex);
			}
			else
			{
				ImportNewFileAs(path, archivedName, compressionMode);
			}
		}

		public void Resize(int nFiles)
		{
			if (m_NFiles == 0)
			{
				m_Files = new FifaFile[nFiles];
				return;
			}
			_ = m_NFiles;
			FifaFile[] sourceArray = (FifaFile[])m_Files.Clone();
			m_Files = new FifaFile[nFiles];
			if (nFiles < m_NFiles)
			{
				Array.Copy(sourceArray, 0, m_Files, 0, nFiles);
			}
			if (nFiles > m_NFiles)
			{
				Array.Copy(sourceArray, 0, m_Files, 0, m_NFiles);
			}
		}

		private int ComputeHeaderSize()
		{
			int num = 16;
			for (int i = 0; i < m_NFiles; i++)
			{
				num += m_Headers[i].Name.Length + 1;
				num += 8;
			}
			m_HeaderSize = num;
			if (m_EndSignature != null)
			{
				m_HeaderSize += m_EndSignature.Length;
			}
			return FifaUtil.RoundUp(num, m_Alignement);
		}

		public void Save()
		{
			BinaryWriter binaryWriter = null;
			BinaryReader r = null;
			FileStream fileStream = null;
			if (NFiles == 0)
			{
				binaryWriter = GetWriter();
				binaryWriter.Write(0);
			}
			else
			{
				binaryWriter = GetWriter();
				r = GetReader();
				ComputeHeaderSize();
				binaryWriter.BaseStream.Position = m_HeaderSize;
				for (int i = 0; i < m_NFiles; i++)
				{
					if (m_Files[i] == null)
					{
						m_Files[i] = new FifaFile(m_Headers[i], r);
					}
					m_Headers[i].StartPosition = (uint)binaryWriter.BaseStream.Position;
					m_Headers[i].Name = m_Files[i].Name;
					m_Files[i].Save(binaryWriter);
					if (m_Files[i].IsToCompress)
					{
						m_Headers[i].Size = m_Files[i].CompressedSize;
					}
					else
					{
						m_Headers[i].Size = m_Files[i].UncompressedSize;
					}
					int v = (int)binaryWriter.BaseStream.Position;
					v = FifaUtil.RoundUp(v, m_Alignement);
					binaryWriter.BaseStream.Position = v;
				}
				m_TotalFileSize = (int)binaryWriter.BaseStream.Position;
				binaryWriter.Seek(0, SeekOrigin.Begin);
				binaryWriter.Write('B');
				binaryWriter.Write('I');
				binaryWriter.Write('G');
				binaryWriter.Write('4');
				binaryWriter.Write(m_TotalFileSize);
				binaryWriter.Write(FifaUtil.SwapEndian(m_NFiles));
				binaryWriter.Write(FifaUtil.SwapEndian(m_HeaderSize));
				for (int j = 0; j < m_NFiles; j++)
				{
					m_Headers[j].Save(binaryWriter);
				}
				if (m_EndSignature != null)
				{
					binaryWriter.Write(m_EndSignature);
				}
			}
			ReleaseReader(r);
			ReleaseWriter(binaryWriter);
			if (base.Archive == null)
			{
				if (base.IsInMemory)
				{
					if (File.Exists(base.PhysicalName) && (File.GetAttributes(base.PhysicalName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					{
						File.SetAttributes(base.PhysicalName, FileAttributes.Archive);
					}
					fileStream = new FileStream(base.PhysicalName, FileMode.Create, FileAccess.Write);
					new BinaryWriter(fileStream);
					Save(fileStream);
					fileStream.Close();
				}
			}
			else
			{
				base.Archive.Save();
			}
		}

		public void Sort()
		{
			string[] array = new string[m_NFiles];
			string[] array2 = new string[m_NFiles];
			for (int i = 0; i < m_NFiles; i++)
			{
				array[i] = Files[i].Name;
				array2[i] = Files[i].Name;
			}
			Array.Sort(array, m_Files, s_StringComparer);
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
				m_Files[i] = m_Files[i + 1];
			}
			for (int j = index; j < m_NFiles; j++)
			{
				m_Headers[j] = m_Headers[j + 1];
			}
			return true;
		}

		public bool Delete(string fileName)
		{
			int archivedFileIndex = GetArchivedFileIndex(fileName, useFullPath: true);
			if (archivedFileIndex >= 0)
			{
				return Delete(archivedFileIndex);
			}
			return false;
		}

		public bool Delete(FifaFile fifaFile)
		{
			string name = fifaFile.Name;
			return Delete(name);
		}

		public int Delete(string[] fileNames)
		{
			int num = 0;
			for (int i = 0; i < fileNames.Length; i++)
			{
				if (Delete(fileNames[i]))
				{
					num++;
				}
			}
			return num;
		}

		public void Rename(string originalName, string newName)
		{
			GetArchivedFile(originalName, useFullPath: true)?.Rename(newName);
		}

		public FifaFile GetFirstDds()
		{
			for (int i = 0; i < m_NFiles; i++)
			{
				if (m_Files[i] == null)
				{
					LoadArchivedFiles();
					break;
				}
			}
			for (int j = 0; j < m_NFiles; j++)
			{
				if (m_Files[j].IsDds())
				{
					return m_Files[j];
				}
			}
			return null;
		}

		public FifaFile GetDdsByName(string fileNameWithoutExtension)
		{
			for (int i = 0; i < m_NFiles; i++)
			{
				if (m_Files[i] == null)
				{
					LoadArchivedFiles();
					break;
				}
			}
			for (int j = 0; j < m_NFiles; j++)
			{
				if (m_Files[j].IsDds() && m_Files[j].Name == fileNameWithoutExtension)
				{
					return m_Files[j];
				}
			}
			return null;
		}
	}
}
