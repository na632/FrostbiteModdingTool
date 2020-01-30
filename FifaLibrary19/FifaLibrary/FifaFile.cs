using System.Diagnostics;
using System.IO;
using zlib;

namespace FifaLibrary
{
	public class FifaFile
	{
		private static Process s_ProcessUnchunklzma = new Process();

		private static Process s_ProcessUnEASF = new Process();

		private string m_Name;

		private string m_PhysicalName;

		private uint m_StartPosition;

		private MemoryStream m_ReadMemoryStream;

		private MemoryStream m_WriteMemoryStream;

		private int m_CompressedSize;

		private int m_UncompressedSize;

		private int m_MaxBlockUncompressedSize;

		private ECompressionMode m_RequiredCompression;

		private ECompressionMode m_CurrentCompression;

		private bool m_IsArchived;

		private FifaBigFile m_Archive;

		private bool m_IsInMemory;

		private bool m_IsAnArchive;

		public string Name
		{
			get
			{
				return m_Name;
			}
			set
			{
				m_Name = value;
			}
		}

		public string PhysicalName
		{
			get
			{
				return m_PhysicalName;
			}
			set
			{
				m_PhysicalName = value;
			}
		}

		public uint StartPosition
		{
			get
			{
				return m_StartPosition;
			}
			set
			{
				m_StartPosition = value;
			}
		}

		public int CompressedSize => m_CompressedSize;

		public int UncompressedSize => m_UncompressedSize;

		public int BlockInflatedSize => m_MaxBlockUncompressedSize;

		public ECompressionMode CompressionMode
		{
			get
			{
				return m_RequiredCompression;
			}
			set
			{
				m_RequiredCompression = value;
			}
		}

		public bool IsToCompress => m_RequiredCompression != ECompressionMode.None;

		public bool IsCompressed => m_CurrentCompression != ECompressionMode.None;

		public bool IsArchived => m_IsArchived;

		public FifaBigFile Archive => m_Archive;

		public bool IsInMemory => m_IsInMemory;

		public bool IsAnArchive => m_IsAnArchive;

		public override string ToString()
		{
			return m_Name;
		}

		public FifaFile(FifaBigFile archive, byte[] buffer, string name, ECompressionMode compressionMode)
		{
			Load(archive, buffer, name, compressionMode);
		}

		private void Load(FifaBigFile archive, byte[] buffer, string name, ECompressionMode compressionMode)
		{
			m_Archive = archive;
			m_IsArchived = false;
			m_IsInMemory = true;
			m_ReadMemoryStream = new MemoryStream(buffer);
			m_PhysicalName = null;
			m_StartPosition = 0u;
			m_Name = name;
			string extension = Path.GetExtension(name);
			extension.ToLower();
			if (extension == ".big")
			{
				m_IsAnArchive = true;
			}
			else
			{
				m_IsAnArchive = false;
			}
			m_RequiredCompression = compressionMode;
			m_CurrentCompression = ECompressionMode.None;
			m_UncompressedSize = buffer.Length;
			if (IsToCompress)
			{
				m_CompressedSize = -1;
			}
			else
			{
				m_CompressedSize = m_UncompressedSize;
			}
		}

		public FifaFile(FifaFile fifaFile)
		{
			Load(fifaFile);
		}

		private void Load(FifaFile fifaFile)
		{
			m_Name = fifaFile.Name;
			m_Archive = fifaFile.Archive;
			m_IsArchived = fifaFile.IsArchived;
			m_IsInMemory = fifaFile.IsInMemory;
			m_PhysicalName = fifaFile.PhysicalName;
			m_StartPosition = fifaFile.StartPosition;
			m_IsAnArchive = true;
			m_RequiredCompression = fifaFile.m_RequiredCompression;
			m_CurrentCompression = fifaFile.m_CurrentCompression;
			m_CompressedSize = fifaFile.CompressedSize;
			m_UncompressedSize = fifaFile.UncompressedSize;
			m_ReadMemoryStream = fifaFile.m_ReadMemoryStream;
			m_WriteMemoryStream = fifaFile.m_WriteMemoryStream;
		}

		public FifaFile(string path, bool isAnArchive)
		{
			Load(path, isAnArchive);
		}

		private void Load(string path, bool isAnArchive)
		{
			m_Archive = null;
			m_IsArchived = false;
			m_IsInMemory = false;
			m_PhysicalName = path;
			m_StartPosition = 0u;
			m_Name = Path.GetFileName(path);
			m_IsAnArchive = isAnArchive;
			BinaryReader reader = GetReader();
			if (reader != null)
			{
				int uncompressedSize = (int)reader.BaseStream.Length;
				m_CompressedSize = (m_UncompressedSize = uncompressedSize);
				m_CurrentCompression = (m_RequiredCompression = ECompressionMode.None);
				CheckCompressionMode(reader);
				ReleaseReader(reader);
			}
		}

		public FifaFile(FifaFileHeader header, BinaryReader r)
		{
			Load(header.BigFile, header.StartPosition, header.Size, header.Name, isAnArchive: false, r);
		}

		private void Load(FifaBigFile archive, uint startPosition, int size, string name, bool isAnArchive, BinaryReader r)
		{
			m_Name = name;
			m_Archive = archive;
			m_IsArchived = true;
			m_IsInMemory = archive.IsInMemory;
			m_PhysicalName = m_Archive.PhysicalName;
			m_StartPosition = startPosition;
			m_IsAnArchive = isAnArchive;
			m_CompressedSize = (m_UncompressedSize = size);
			m_CurrentCompression = (m_RequiredCompression = ECompressionMode.Unknown);
			if (size == 0)
			{
				m_CurrentCompression = (m_RequiredCompression = ECompressionMode.None);
			}
			else if (r == null)
			{
				r = m_Archive.GetReader();
				r.BaseStream.Position += startPosition;
				CheckCompressionMode(r);
				ReleaseReader(r);
			}
			else
			{
				if (m_Archive != null)
				{
					r.BaseStream.Position = m_Archive.StartPosition + startPosition;
				}
				else
				{
					r.BaseStream.Position = startPosition;
				}
				CheckCompressionMode(r);
			}
		}

		public void Save(Stream outputStream)
		{
			long position = outputStream.Position;
			if (IsToCompress && !IsCompressed)
			{
				Compress(outputStream);
			}
			else if (!IsToCompress && IsCompressed)
			{
				Decompress(outputStream);
			}
			else
			{
				BinaryReader reader = GetReader();
				int num = IsCompressed ? m_CompressedSize : m_UncompressedSize;
				int num2 = 1048576;
				while (num > num2)
				{
					outputStream.Write(reader.ReadBytes(num2), 0, num2);
					num -= num2;
				}
				outputStream.Write(reader.ReadBytes(num), 0, num);
				ReleaseReader(reader);
			}
			if (m_Archive != null)
			{
				m_PhysicalName = m_Archive.PhysicalName;
				m_IsArchived = true;
			}
			m_StartPosition = (uint)position;
			m_ReadMemoryStream = null;
			m_WriteMemoryStream = null;
			m_IsInMemory = false;
		}

		public void Save(BinaryWriter w)
		{
			Stream baseStream = w.BaseStream;
			Save(baseStream);
		}

		private ECompressionMode CheckCompressionMode(BinaryReader r)
		{
			if (r.BaseStream.Length < 8)
			{
				return ECompressionMode.None;
			}
			long position = r.BaseStream.Position;
			byte[] array = r.ReadBytes(8);
			if (array[0] == 16 && array[1] == 251)
			{
				m_CurrentCompression = ECompressionMode.Compressed_10FB;
				m_RequiredCompression = m_CurrentCompression;
				m_UncompressedSize = (array[2] << 16) + (array[3] << 8) + array[4];
			}
			else
			{
				char[] array2 = new char[8];
				for (int i = 0; i < 8; i++)
				{
					array2[i] = (char)array[i];
				}
				string text = new string(array2);
				if (text.StartsWith("EASF"))
				{
					m_CompressedSize = (array[4] << 24) + (array[5] << 16) + (array[6] << 8) + array[7];
					r.ReadBytes(8);
					m_CurrentCompression = ECompressionMode.EASF;
					m_RequiredCompression = m_CurrentCompression;
				}
				else if (text == "chunkzip")
				{
					m_UncompressedSize = FifaUtil.SwapEndian(r.ReadInt32());
					m_CurrentCompression = ECompressionMode.Chunkzip;
					if (m_UncompressedSize == 2)
					{
						m_UncompressedSize = FifaUtil.SwapEndian(r.ReadInt32());
						r.ReadInt32();
						r.ReadInt32();
						r.ReadInt32();
						r.ReadInt32();
						r.ReadInt32();
						r.ReadInt32();
						FifaUtil.SwapEndian(r.ReadInt32());
						r.ReadInt32();
						m_CurrentCompression = ECompressionMode.Chunkzip2;
						m_RequiredCompression = m_CurrentCompression;
					}
					else
					{
						m_MaxBlockUncompressedSize = FifaUtil.SwapEndian(r.ReadInt32());
						m_RequiredCompression = m_CurrentCompression;
					}
				}
				else if (text == "chunkref")
				{
					m_UncompressedSize = FifaUtil.SwapEndian(r.ReadInt32());
					if (m_UncompressedSize == 2)
					{
						m_UncompressedSize = FifaUtil.SwapEndian(r.ReadInt32());
						r.ReadInt32();
						r.ReadInt32();
						r.ReadInt32();
						r.ReadInt32();
						r.ReadInt32();
						r.ReadInt32();
						m_MaxBlockUncompressedSize = FifaUtil.SwapEndian(r.ReadInt32());
						r.ReadInt32();
						m_CurrentCompression = ECompressionMode.Chunkref2;
					}
					else
					{
						FifaUtil.SwapEndian(r.ReadInt32());
						FifaUtil.SwapEndian(r.ReadInt32());
						array = r.ReadBytes(8);
						if (array[0] == 16 && array[1] == 251)
						{
							m_CurrentCompression = ECompressionMode.Chunkref;
							m_RequiredCompression = m_CurrentCompression;
							m_UncompressedSize = (array[2] << 16) + (array[3] << 8) + array[4];
						}
						else
						{
							m_CurrentCompression = ECompressionMode.Unknown;
						}
					}
					m_RequiredCompression = m_CurrentCompression;
				}
				else if (text == "chunlzma")
				{
					m_CurrentCompression = ECompressionMode.Chunklzma;
					m_RequiredCompression = m_CurrentCompression;
				}
				else
				{
					m_CurrentCompression = ECompressionMode.None;
					m_RequiredCompression = m_CurrentCompression;
				}
			}
			r.BaseStream.Position = position;
			return m_CurrentCompression;
		}

		public bool IsDds()
		{
			if (Path.GetExtension(m_Name) == string.Empty)
			{
				BinaryReader reader = GetReader();
				if (CompressedSize >= 16)
				{
					byte[] array = reader.ReadBytes(16);
					if ((array[6] == 68 && array[7] == 68 && array[8] == 83) || (array[0] == 68 && array[1] == 68 && array[2] == 83))
					{
						return true;
					}
				}
				ReleaseReader(reader);
			}
			return false;
		}

		public bool Decompress(Stream outputStream)
		{
			if (m_CurrentCompression == ECompressionMode.None || m_CurrentCompression == ECompressionMode.Unknown)
			{
				outputStream.Write(Read(), 0, m_CompressedSize);
				return false;
			}
			if (m_IsArchived)
			{
				m_Archive.Decompress();
			}
			switch (m_CurrentCompression)
			{
			case ECompressionMode.Chunkref:
				UnChunkref(outputStream);
				break;
			case ECompressionMode.Chunkref2:
				UnChunkref2(outputStream);
				break;
			case ECompressionMode.Chunkzip:
				UnChunkzip(outputStream);
				break;
			case ECompressionMode.Chunkzip2:
				UnChunkZip2(outputStream);
				break;
			case ECompressionMode.Compressed_10FB:
				Uncompress_10FB(outputStream);
				break;
			case ECompressionMode.EASF:
				UnEASF(outputStream);
				break;
			case ECompressionMode.Chunklzma:
				UnChunklzma(outputStream);
				break;
			}
			return true;
		}

		public bool Decompress()
		{
			if (m_CurrentCompression == ECompressionMode.None)
			{
				return false;
			}
			m_ReadMemoryStream = new MemoryStream(m_UncompressedSize);
			bool num = Decompress(m_ReadMemoryStream);
			if (num)
			{
				m_CurrentCompression = ECompressionMode.None;
				m_IsArchived = false;
				m_IsInMemory = true;
				m_StartPosition = 0u;
			}
			return num;
		}

		public bool Compress(Stream outputStream)
		{
			if (m_CurrentCompression != 0)
			{
				outputStream.Write(Read(), 0, m_CompressedSize);
				return true;
			}
			if (m_CurrentCompression == m_RequiredCompression)
			{
				outputStream.Write(Read(), 0, m_CompressedSize);
				return true;
			}
			switch (m_RequiredCompression)
			{
			case ECompressionMode.Chunkref:
				Chunkref(outputStream, m_UncompressedSize);
				break;
			case ECompressionMode.Chunkzip:
				Chunkzip(outputStream);
				break;
			case ECompressionMode.Compressed_10FB:
				Compress_10FB(outputStream);
				break;
			}
			return true;
		}

		public BinaryReader GetReader()
		{
			BinaryReader binaryReader = null;
			if (m_IsInMemory)
			{
				if (m_ReadMemoryStream != null)
				{
					binaryReader = new BinaryReader(m_ReadMemoryStream);
					binaryReader.BaseStream.Position = m_StartPosition;
				}
				else
				{
					if (m_Archive == null)
					{
						return null;
					}
					binaryReader = m_Archive.GetReader();
					binaryReader.BaseStream.Position = m_StartPosition;
				}
			}
			else if (File.Exists(m_PhysicalName))
			{
				binaryReader = new BinaryReader(new FileStream(m_PhysicalName, FileMode.Open, FileAccess.Read));
				binaryReader.BaseStream.Position = m_StartPosition;
				if (m_Archive != null)
				{
					binaryReader.BaseStream.Position += m_Archive.StartPosition;
				}
			}
			return binaryReader;
		}

		public void ReleaseReader(BinaryReader r)
		{
			if (r != null && !m_IsInMemory)
			{
				r.BaseStream.Close();
				r.Close();
			}
		}

		public StreamReader GetStreamReader()
		{
			if (m_ReadMemoryStream != null)
			{
				return new StreamReader(m_ReadMemoryStream);
			}
			StreamReader streamReader = (m_Archive == null) ? new StreamReader(m_PhysicalName) : m_Archive.GetStreamReader();
			streamReader.BaseStream.Position += m_StartPosition;
			if (IsCompressed)
			{
				m_ReadMemoryStream = new MemoryStream();
				Decompress(m_ReadMemoryStream);
				m_ReadMemoryStream.Seek(0L, SeekOrigin.Begin);
				m_StartPosition = 0u;
				m_IsInMemory = true;
				return new StreamReader(m_ReadMemoryStream);
			}
			return streamReader;
		}

		public void ReleaseStreamReader(StreamReader r)
		{
			if (!m_IsInMemory)
			{
				r.BaseStream.Close();
				r.Close();
			}
		}

		protected StreamWriter GetStreamWriter(ECompressionMode compressionMode)
		{
			m_WriteMemoryStream = new MemoryStream();
			StreamWriter result = new StreamWriter(m_WriteMemoryStream);
			m_IsInMemory = true;
			m_IsArchived = false;
			m_CurrentCompression = compressionMode;
			return result;
		}

		protected bool ReleaseStreamWriter(StreamWriter w)
		{
			w.Flush();
			m_CompressedSize = (m_UncompressedSize = (int)m_WriteMemoryStream.Length);
			if (m_IsInMemory)
			{
				if (!IsCompressed)
				{
					if (IsToCompress)
					{
						byte[] array = Compress_10FB(m_WriteMemoryStream.GetBuffer());
						if (array == null)
						{
							m_CurrentCompression = ECompressionMode.None;
							m_RequiredCompression = ECompressionMode.None;
							m_CompressedSize = m_UncompressedSize;
							return false;
						}
						m_RequiredCompression = ECompressionMode.Compressed_10FB;
						m_CompressedSize = array.Length;
					}
				}
				else if (!IsToCompress)
				{
					byte[] array2 = Uncompress_10FB(m_WriteMemoryStream.GetBuffer());
					if (array2 == null)
					{
						m_CurrentCompression = ECompressionMode.None;
						m_RequiredCompression = ECompressionMode.None;
						m_UncompressedSize = m_CompressedSize;
						return false;
					}
					m_RequiredCompression = ECompressionMode.None;
					m_UncompressedSize = array2.Length;
				}
				m_ReadMemoryStream = m_WriteMemoryStream;
				m_StartPosition = 0u;
			}
			else
			{
				w.Close();
				if (File.Exists(PhysicalName + ".tmp"))
				{
					File.Delete(PhysicalName);
					File.Move(PhysicalName + ".tmp", PhysicalName);
				}
			}
			return true;
		}

		private byte[] Read()
		{
			BinaryReader reader = GetReader();
			int count = IsCompressed ? m_CompressedSize : m_UncompressedSize;
			byte[] result = reader.ReadBytes(count);
			ReleaseReader(reader);
			return result;
		}

		protected BinaryWriter GetWriter(int size, ECompressionMode compressionMode)
		{
			m_WriteMemoryStream = new MemoryStream(size);
			BinaryWriter result = new BinaryWriter(m_WriteMemoryStream);
			m_IsInMemory = true;
			m_IsArchived = false;
			m_CurrentCompression = compressionMode;
			if (m_CurrentCompression != 0)
			{
				m_CompressedSize = size;
				m_UncompressedSize = -1;
				return result;
			}
			m_UncompressedSize = size;
			m_CompressedSize = -1;
			return result;
		}

		public BinaryWriter GetWriter()
		{
			if (m_Archive == null && m_PhysicalName != null)
			{
				BinaryWriter result = new BinaryWriter(new FileStream(m_PhysicalName + ".temp", FileMode.Create));
				m_IsInMemory = false;
				m_IsArchived = false;
				return result;
			}
			m_WriteMemoryStream = new MemoryStream();
			BinaryWriter result2 = new BinaryWriter(m_WriteMemoryStream);
			m_IsInMemory = true;
			m_IsArchived = false;
			return result2;
		}

		public bool ReleaseWriter(BinaryWriter w)
		{
			if (m_IsInMemory)
			{
				if (!IsCompressed)
				{
					m_CompressedSize = -1;
					m_UncompressedSize = (int)m_WriteMemoryStream.Length;
				}
				else
				{
					m_UncompressedSize = -1;
					m_CompressedSize = (int)m_WriteMemoryStream.Length;
				}
				m_ReadMemoryStream = m_WriteMemoryStream;
				m_StartPosition = 0u;
			}
			else
			{
				w.Close();
				if ((File.GetAttributes(PhysicalName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					File.SetAttributes(PhysicalName, FileAttributes.Archive);
				}
				File.Delete(m_PhysicalName);
				File.Move(m_PhysicalName + ".temp", m_PhysicalName);
			}
			return true;
		}

		public bool Export(string exportDir)
		{
			return Export(exportDir, decompressionAllowed: true);
		}

		public bool Export(string exportDir, bool decompressionAllowed)
		{
			if (m_Name.StartsWith("C:"))
			{
				return false;
			}
			string path = exportDir + "\\" + m_Name;
			string directoryName = Path.GetDirectoryName(path);
			Path.GetExtension(path);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			if (IsCompressed && decompressionAllowed)
			{
				Decompress();
			}
			BinaryReader reader = GetReader();
			int count = IsCompressed ? m_CompressedSize : m_UncompressedSize;
			binaryWriter.Write(reader.ReadBytes(count));
			binaryWriter.Close();
			fileStream.Close();
			ReleaseReader(reader);
			return true;
		}

		public void Rename(string name)
		{
			m_Name = name;
			if (Archive == null)
			{
				string text = Path.GetDirectoryName(m_PhysicalName) + "\\" + name;
				File.Move(m_PhysicalName, text);
				m_PhysicalName = text;
			}
		}

		public bool SetCompressionMode(ECompressionMode compressionMode)
		{
			if (m_CurrentCompression == compressionMode)
			{
				return true;
			}
			if (m_CurrentCompression != 0 && compressionMode != 0)
			{
				return false;
			}
			if (m_CurrentCompression == ECompressionMode.None && compressionMode != 0)
			{
				m_RequiredCompression = compressionMode;
				m_CompressedSize = -1;
				return true;
			}
			return false;
		}

		private bool Uncompress_10FB(Stream outputStream)
		{
			if (m_CompressedSize == -1)
			{
				return false;
			}
			byte[] array = Uncompress_10FB(Read());
			if (array != null)
			{
				outputStream.Write(array, 0, m_UncompressedSize);
				m_UncompressedSize = array.Length;
				return true;
			}
			return false;
		}

		private bool UnChunkref(Stream outputStream)
		{
			if (m_CompressedSize == -1)
			{
				return false;
			}
			BinaryReader reader = GetReader();
			if (new string(reader.ReadChars(8)) != "chunkref")
			{
				return false;
			}
			m_UncompressedSize = FifaUtil.SwapEndian(reader.ReadInt32());
			FifaUtil.SwapEndian(reader.ReadInt32());
			int num = 20;
			do
			{
				int num2 = FifaUtil.SwapEndian(reader.ReadInt32());
				num += 4 + num2;
				byte[] array = Uncompress_10FB(reader.ReadBytes(num2));
				outputStream.Write(array, 0, array.Length);
			}
			while (num < m_CompressedSize);
			ReleaseReader(reader);
			return true;
		}

		private bool UnChunkref2(Stream outputStream)
		{
			if (m_CompressedSize == -1)
			{
				return false;
			}
			BinaryReader reader = GetReader();
			if (new string(reader.ReadChars(8)) != "chunkref")
			{
				return false;
			}
			reader.ReadInt32();
			m_UncompressedSize = FifaUtil.SwapEndian(reader.ReadInt32());
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			int count = FifaUtil.SwapEndian(reader.ReadInt32());
			reader.ReadInt32();
			byte[] array = Uncompress_10FB(reader.ReadBytes(count));
			outputStream.Write(array, 0, array.Length);
			ReleaseReader(reader);
			return true;
		}

		private bool UnChunkZip2(Stream outputStream)
		{
			if (m_CompressedSize == -1)
			{
				return false;
			}
			BinaryReader reader = GetReader();
			if (new string(reader.ReadChars(8)) != "chunkzip")
			{
				return false;
			}
			reader.ReadInt32();
			m_UncompressedSize = FifaUtil.SwapEndian(reader.ReadInt32());
			m_MaxBlockUncompressedSize = FifaUtil.SwapEndian(reader.ReadInt32());
			FifaUtil.SwapEndian(reader.ReadInt32());
			FifaUtil.SwapEndian(reader.ReadInt32());
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			int num = 40;
			do
			{
				num += 8;
				int num2 = FifaUtil.RoundUp(num, 16) - num;
				if (num2 > 0)
				{
					reader.ReadBytes(num2);
					num += num2;
				}
				int num3 = FifaUtil.SwapEndian(reader.ReadInt32());
				reader.ReadInt32();
				num += num3;
				InflateBlock2(reader.BaseStream, outputStream, num3);
			}
			while (num < m_CompressedSize);
			ReleaseReader(reader);
			return true;
		}

		private bool UnChunkzip(Stream outputStream)
		{
			if (m_CompressedSize == -1)
			{
				return false;
			}
			BinaryReader reader = GetReader();
			if (new string(reader.ReadChars(8)) != "chunkzip")
			{
				return false;
			}
			m_UncompressedSize = FifaUtil.SwapEndian(reader.ReadInt32());
			m_MaxBlockUncompressedSize = FifaUtil.SwapEndian(reader.ReadInt32());
			int num = 16;
			do
			{
				int num2 = FifaUtil.SwapEndian(reader.ReadInt32());
				num += 4 + num2;
				InflateBlock(reader.BaseStream, outputStream, num2);
			}
			while (num < m_CompressedSize);
			ReleaseReader(reader);
			return true;
		}

		private bool UnEASF(Stream outputStream)
		{
			Export(FifaEnvironment.ExportFolder, decompressionAllowed: false);
			string text = FifaEnvironment.ExportFolder + "\\" + Name;
			text = text.Replace("/", "\\");
			if (!File.Exists(text))
			{
				return false;
			}
			if (text != null)
			{
				s_ProcessUnEASF.StartInfo.WorkingDirectory = FifaEnvironment.LaunchDir;
				s_ProcessUnEASF.StartInfo.FileName = "fifa16_decryptor";
				s_ProcessUnEASF.StartInfo.CreateNoWindow = true;
				s_ProcessUnEASF.StartInfo.UseShellExecute = false;
				s_ProcessUnEASF.StartInfo.Arguments = "\"" + text + "\"";
				s_ProcessUnEASF.StartInfo.RedirectStandardOutput = false;
				s_ProcessUnEASF.Start();
				s_ProcessUnEASF.WaitForExit();
			}
			string str = Path.GetDirectoryName(text) + "\\" + Path.GetFileNameWithoutExtension(text);
			str = str + "_decrypted" + Path.GetExtension(text);
			if (!File.Exists(str))
			{
				return false;
			}
			File.Delete(text);
			File.Move(str, text);
			FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read);
			int num = (int)new FileInfo(text).Length;
			byte[] buffer = new byte[num];
			fileStream.Read(buffer, 0, num);
			outputStream.Write(buffer, 0, num);
			fileStream.Close();
			return true;
		}

		private bool UnChunklzma(Stream outputStream)
		{
			Export(FifaEnvironment.ExportFolder, decompressionAllowed: false);
			string text = FifaEnvironment.ExportFolder + "\\" + Name;
			if (!File.Exists(text))
			{
				return false;
			}
			if (text != null)
			{
				s_ProcessUnchunklzma.StartInfo.WorkingDirectory = FifaEnvironment.LaunchDir;
				s_ProcessUnchunklzma.StartInfo.FileName = "un_chunlzma";
				s_ProcessUnchunklzma.StartInfo.CreateNoWindow = true;
				s_ProcessUnchunklzma.StartInfo.UseShellExecute = false;
				s_ProcessUnchunklzma.StartInfo.Arguments = text;
				s_ProcessUnchunklzma.StartInfo.RedirectStandardOutput = false;
				s_ProcessUnchunklzma.Start();
				s_ProcessUnchunklzma.WaitForExit();
			}
			string str = Path.GetDirectoryName(text) + "\\" + Path.GetFileNameWithoutExtension(text);
			str = str + "_decompressed" + Path.GetExtension(text);
			if (!File.Exists(str))
			{
				return false;
			}
			File.Delete(text);
			File.Move(str, text);
			FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read);
			int num = m_UncompressedSize = (int)new FileInfo(text).Length;
			byte[] buffer = new byte[num];
			fileStream.Read(buffer, 0, num);
			outputStream.Write(buffer, 0, num);
			fileStream.Close();
			File.Delete(str);
			return true;
		}

		private void Compress_10FB(Stream outputStream)
		{
			byte[] inputBuffer = Read();
			byte[] array = Compress_10FB(inputBuffer);
			m_CompressedSize = array.Length;
			m_CurrentCompression = ECompressionMode.Compressed_10FB;
			outputStream.Write(array, 0, array.Length);
		}

		private void Chunkzip(Stream outputStream)
		{
			Chunkzip(outputStream, m_UncompressedSize);
		}

		private void Chunkzip(Stream outputStream, int uncompressedSize)
		{
			BinaryReader reader = GetReader();
			m_UncompressedSize = uncompressedSize;
			m_MaxBlockUncompressedSize = 184320;
			BinaryWriter binaryWriter = new BinaryWriter(outputStream);
			long position = outputStream.Position;
			binaryWriter.Write('c');
			binaryWriter.Write('h');
			binaryWriter.Write('u');
			binaryWriter.Write('n');
			binaryWriter.Write('k');
			binaryWriter.Write('z');
			binaryWriter.Write('i');
			binaryWriter.Write('p');
			binaryWriter.Write(FifaUtil.SwapEndian(m_UncompressedSize));
			binaryWriter.Write(FifaUtil.SwapEndian(m_MaxBlockUncompressedSize));
			int num = 0;
			do
			{
				int num2 = (int)outputStream.Position;
				binaryWriter.Write(-1);
				int num3 = m_MaxBlockUncompressedSize;
				if (m_UncompressedSize - num < m_MaxBlockUncompressedSize)
				{
					num3 = m_UncompressedSize - num;
				}
				num += num3;
				int x = DeflateBlock(reader.BaseStream, outputStream, num3);
				int num4 = (int)outputStream.Position;
				outputStream.Position = num2;
				binaryWriter.Write(FifaUtil.SwapEndian(x));
				outputStream.Position = num4;
			}
			while (num < m_UncompressedSize);
			ReleaseReader(reader);
			m_CompressedSize = (int)(outputStream.Position - position);
		}

		private void Chunkref2(Stream outputStream, int uncompressedSize)
		{
		}

		private void Chunkref(Stream outputStream)
		{
			Chunkref(outputStream, m_UncompressedSize);
		}

		private void Chunkref(Stream outputStream, int uncompressedSize)
		{
			BinaryReader reader = GetReader();
			BinaryWriter binaryWriter = new BinaryWriter(outputStream);
			m_UncompressedSize = uncompressedSize;
			m_MaxBlockUncompressedSize = 235520;
			long position = outputStream.Position;
			binaryWriter.Write('c');
			binaryWriter.Write('h');
			binaryWriter.Write('u');
			binaryWriter.Write('n');
			binaryWriter.Write('k');
			binaryWriter.Write('r');
			binaryWriter.Write('e');
			binaryWriter.Write('f');
			binaryWriter.Write(FifaUtil.SwapEndian(m_UncompressedSize));
			binaryWriter.Write(FifaUtil.SwapEndian(m_MaxBlockUncompressedSize));
			int num = 0;
			do
			{
				int num2 = (int)outputStream.Position;
				binaryWriter.Write(-1);
				int num3 = m_MaxBlockUncompressedSize;
				if (m_UncompressedSize - num < m_MaxBlockUncompressedSize)
				{
					num3 = m_UncompressedSize - num;
				}
				num += num3;
				byte[] inputBuffer = reader.ReadBytes(num3);
				byte[] array = Compress_10FB(inputBuffer);
				int num4 = array.Length;
				outputStream.Write(array, 0, num4);
				long position2 = outputStream.Position;
				outputStream.Position = num2;
				binaryWriter.Write(FifaUtil.SwapEndian(num4));
				outputStream.Position = position2;
			}
			while (num < m_UncompressedSize);
			ReleaseReader(reader);
			m_CompressedSize = (int)(outputStream.Position - position);
			m_CurrentCompression = ECompressionMode.Chunkref;
		}

		private void InflateBlock(Stream inputStream, Stream outputStream, int blockCompressedSize)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			ZOutputStream outputStream2 = (ZOutputStream)(object)new ZOutputStream(outputStream);
			CopyStream(inputStream, outputStream2, blockCompressedSize);
		}

		private void InflateBlock2(Stream inputStream, Stream outputStream, int blockCompressedSize)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			ZOutputStream outputStream2 = (ZOutputStream)(object)new ZOutputStream(outputStream);
			CopyStream2(inputStream, outputStream2, blockCompressedSize);
		}

		private int DeflateBlock(Stream inputStream, Stream outputStream, int blockUncompressedSize)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Expected O, but got Unknown
			ZOutputStream outputStream2 = (ZOutputStream)(object)new ZOutputStream(outputStream, -1);
			long position = outputStream.Position;
			CopyStream(inputStream, outputStream2, blockUncompressedSize);
			return (int)(outputStream.Position - position);
		}

		private int Uncompress_10FB_Block(Stream inputStream, Stream outputStream, int blockCompressedSize)
		{
			if (blockCompressedSize == 0)
			{
				return 0;
			}
			byte[] array = new byte[blockCompressedSize];
			inputStream.Read(array, 0, blockCompressedSize);
			byte[] array2 = Uncompress_10FB(array);
			outputStream.Write(array2, 0, array2.Length);
			return array2.Length;
		}

		private int Compress_10FB_Block(Stream inputStream, Stream outputStream, int blockUncompressedSize)
		{
			if (blockUncompressedSize == 0)
			{
				return 0;
			}
			byte[] array = new byte[blockUncompressedSize];
			inputStream.Read(array, 0, blockUncompressedSize);
			byte[] array2 = Compress_10FB(array);
			outputStream.Write(array2, 0, array2.Length);
			return array2.Length;
		}

		private static byte[] Uncompress_10FB(byte[] inputBuffer)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = inputBuffer.Length;
			if (num4 < 8)
			{
				num3 = 1;
				return null;
			}
			int num5 = (inputBuffer[2] << 16) + (inputBuffer[3] << 8) + inputBuffer[4];
			if (num5 > num4 * 128)
			{
				num3 = 2;
				return null;
			}
			byte[] array = new byte[num5];
			int num6 = ((inputBuffer[0] & 1) != 1) ? 5 : 8;
			int num7 = 0;
			while (num6 < num4 && inputBuffer[num6] < 252)
			{
				byte b = inputBuffer[num6++];
				int num8;
				if ((b & 0x80) == 0)
				{
					if (num6 + 1 >= num4)
					{
						num3 = 3;
						break;
					}
					num8 = (b & 3);
					num = ((b & 0x1C) >> 2) + 3;
					num2 = (b >> 5 << 8) + inputBuffer[num6++] + 1;
				}
				else if ((b & 0x40) == 0)
				{
					if (num6 + 2 >= num4)
					{
						num3 = 4;
						break;
					}
					byte num9 = inputBuffer[num6++];
					num8 = ((num9 >> 6) & 3);
					num = (b & 0x3F) + 4;
					num2 = (num9 & 0x3F) * 256 + inputBuffer[num6++] + 1;
				}
				else if ((b & 0x20) == 0)
				{
					if (num6 + 3 >= num4)
					{
						num3 = 5;
						break;
					}
					num8 = (b & 3);
					byte b2 = inputBuffer[num6++];
					num2 = ((b & 0x10) << 12) + 256 * b2 + inputBuffer[num6++] + 1;
					num = ((b >> 2) & 3) * 256 + inputBuffer[num6++] + 5;
				}
				else
				{
					num8 = (b & 0x1F) * 4 + 4;
					num = 0;
				}
				if (num6 + num8 >= num4)
				{
					num3 = 6;
					break;
				}
				if (num7 + num8 + num > num5)
				{
					num3 = 7;
					break;
				}
				if (num7 + num8 - num2 < 0)
				{
					num3 = 8;
					break;
				}
				for (int i = 0; i < num8; i++)
				{
					array[num7++] = inputBuffer[num6++];
				}
				for (int j = 0; j < num; j++)
				{
					array[num7] = array[num7 - num2];
					num7++;
				}
			}
			if (num6 < num4 && num7 < num5)
			{
				int num10 = inputBuffer[num6] & 3;
				if (num6 + num10 >= num4)
				{
					num3 = 9;
					num10 = 0;
				}
				if (num7 + num10 > num5)
				{
					num3 = 10;
					num10 = 0;
				}
				for (int k = 0; k < num10; k++)
				{
					num6++;
					array[num7] = inputBuffer[num6];
					num7++;
				}
			}
			if (num3 != 0 || num7 != num5)
			{
				return null;
			}
			return array;
		}

		private byte[] Compress_10FB(byte[] inputBuffer)
		{
			int num = 0;
			int[] array = new int[131072];
			for (int i = 0; i < 131072; i++)
			{
				array[i] = -1;
			}
			int[,] array2 = new int[256, 256];
			for (int j = 0; j < 256; j++)
			{
				for (int k = 0; k < 256; k++)
				{
					array2[j, k] = -1;
				}
			}
			int num2 = inputBuffer.Length;
			byte[] array3 = new byte[num2 + 1000];
			array3[0] = 16;
			array3[1] = 251;
			array3[2] = (byte)(num2 >> 16);
			array3[3] = (byte)((num2 >> 8) & 0xFF);
			array3[4] = (byte)(num2 & 0xFF);
			int num3 = 5;
			int num4 = 0;
			int num5 = 0;
			int num10;
			while (num4 < num2 - 1)
			{
				byte b = inputBuffer[num4];
				byte b2 = inputBuffer[num4 + 1];
				int num6 = array[num4 & 0x1FFFF] = array2[b, b2];
				array2[b, b2] = num4;
				if (num4 >= num)
				{
					int num7 = 0;
					int num8 = 0;
					int num9 = 0;
					while (num6 >= 0 && num4 - num6 < 131072 && num9++ < 100)
					{
						num10 = 2;
						int num11 = num5 + 2;
						if (num11 >= num2)
						{
							break;
						}
						int num12 = num6 + 2;
						bool flag = false;
						while (inputBuffer[num11++] == inputBuffer[num12++] && num10 < 1028)
						{
							num10++;
							if (num11 == num2)
							{
								flag = true;
								break;
							}
						}
						if (num10 > num7)
						{
							num7 = num10;
							num8 = num4 - num6;
						}
						if (flag || num10 == 1028)
						{
							break;
						}
						num6 = array[num6 & 0x1FFFF];
					}
					if (num7 > num2 - num4)
					{
						num7 = num4 - num2;
					}
					if (num7 <= 2)
					{
						num7 = 0;
					}
					if (num7 == 3 && num8 > 1024)
					{
						num7 = 0;
					}
					if (num7 == 4 && num8 > 16384)
					{
						num7 = 0;
					}
					if (num7 != 0)
					{
						while (num4 - num >= 4)
						{
							num10 = (num4 - num) / 4 - 1;
							if (num10 > 27)
							{
								num10 = 27;
							}
							array3[num3++] = (byte)(224 + num10);
							num10 = 4 * num10 + 4;
							for (int l = 0; l < num10; l++)
							{
								array3[num3 + l] = inputBuffer[num + l];
							}
							num += num10;
							num3 += num10;
						}
						num10 = num4 - num;
						if (num7 <= 10 && num8 <= 1024)
						{
							array3[num3++] = (byte)((num8 - 1 >> 8 << 5) + (num7 - 3 << 2) + num10);
							array3[num3++] = (byte)((num8 - 1) & 0xFF);
							while (num10-- != 0)
							{
								array3[num3++] = inputBuffer[num++];
							}
							num += num7;
						}
						else if (num7 <= 67 && num8 <= 16384)
						{
							array3[num3++] = (byte)(128 + (num7 - 4));
							array3[num3++] = (byte)((num10 << 6) + (num8 - 1 >> 8));
							array3[num3++] = (byte)((num8 - 1) & 0xFF);
							while (num10-- != 0)
							{
								array3[num3++] = inputBuffer[num++];
							}
							num += num7;
						}
						else if (num7 <= 1028 && num8 < 131072)
						{
							num8--;
							array3[num3++] = (byte)(192 + (num8 >> 16 << 4) + (num7 - 5 >> 8 << 2) + num10);
							array3[num3++] = (byte)((num8 >> 8) & 0xFF);
							array3[num3++] = (byte)(num8 & 0xFF);
							array3[num3++] = (byte)((num7 - 5) & 0xFF);
							while (num10-- != 0)
							{
								array3[num3++] = inputBuffer[num++];
							}
							num += num7;
						}
					}
				}
				num4++;
				num5++;
			}
			num4 = num2;
			while (num4 - num >= 4)
			{
				num10 = (num4 - num) / 4 - 1;
				if (num10 > 27)
				{
					num10 = 27;
				}
				array3[num3++] = (byte)(224 + num10);
				num10 = 4 * num10 + 4;
				for (int m = 0; m < num10; m++)
				{
					array3[num3 + m] = inputBuffer[num + m];
				}
				num += num10;
				num3 += num10;
			}
			num10 = num4 - num;
			array3[num3++] = (byte)(252 + num10);
			while (num10-- != 0)
			{
				array3[num3++] = inputBuffer[num++];
			}
			if (num != num2)
			{
				return null;
			}
			byte[] array4 = new byte[num3];
			for (int n = 0; n < num3; n++)
			{
				array4[n] = array3[n];
			}
			return array4;
		}

		private static void CopyStream(Stream inputStream, ZOutputStream outputStream, int size)
		{
			int num = 0;
			byte[] buffer = new byte[size + num];
			inputStream.Read(buffer, num, size);
			((Stream)(object)outputStream).Write(buffer, 0, size + num);
			outputStream.finish();
			((Stream)(object)outputStream).Flush();
		}

		private static void CopyStream2(Stream inputStream, ZOutputStream outputStream, int size)
		{
			int num = 2;
			byte[] array = new byte[size + num];
			array[0] = 120;
			array[1] = 218;
			inputStream.Read(array, num, size);
			((Stream)(object)outputStream).Write(array, 0, size + num);
			((Stream)(object)outputStream).Flush();
		}
	}
}
