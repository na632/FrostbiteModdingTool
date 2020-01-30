using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace FifaLibrary
{
	public class Rx3File
	{
		private float[] m_Positions = new float[32];

		private int m_Rx3bPosition;

		private Rx3Header m_Rx3Header;

		private bool m_SwapEndian;

		private bool m_IsFifa12;

		private string m_FileName;

		private int m_AlfaFlag;

		private Rx3FileDescriptor[] m_Rx3FileDescriptors;

		private Rx3Signatures m_Rx3Signatures;

		private Rx3ImageDirectory m_Rx3ImageDirectory;

		private Rx3ModelDirectory m_Rx3ModelDirectory;

		private Rx3Image[] m_Rx3Images;

		private Rx3IndexArray[] m_Rx3IndexArrays;

		private Rx3VertexArray[] m_Rx3VertexArrays;

		public float[] Positions
		{
			get
			{
				return m_Positions;
			}
			set
			{
				m_Positions = value;
			}
		}

		public Rx3Header Rx3Headers => m_Rx3Header;

		public bool SwapEndian => m_SwapEndian;

		public bool IsFifa12 => m_IsFifa12;

		public bool IsFifa11 => !m_IsFifa12;

		public string FileName => m_FileName;

		public int HairAlfaFlag => m_AlfaFlag;

		public Rx3FileDescriptor[] Rx3FileDescriptors => m_Rx3FileDescriptors;

		public Rx3Signatures Signatures
		{
			get
			{
				return m_Rx3Signatures;
			}
			set
			{
				m_Rx3Signatures = value;
			}
		}

		public Rx3ImageDirectory Rx3ImageDirectory
		{
			get
			{
				return m_Rx3ImageDirectory;
			}
			set
			{
				m_Rx3ImageDirectory = value;
			}
		}

		public Rx3ModelDirectory Rx3ModelDirectory
		{
			get
			{
				return m_Rx3ModelDirectory;
			}
			set
			{
				m_Rx3ModelDirectory = value;
			}
		}

		public Rx3Image[] Images
		{
			get
			{
				return m_Rx3Images;
			}
			set
			{
				m_Rx3Images = value;
			}
		}

		public Rx3IndexArray[] Rx3IndexArrays => m_Rx3IndexArrays;

		public Rx3VertexArray[] Rx3VertexArrays => m_Rx3VertexArrays;

		public bool Load(FifaFile fifaFile)
		{
			if (fifaFile.IsCompressed)
			{
				fifaFile.Decompress();
			}
			BinaryReader reader = fifaFile.GetReader();
			bool result = Load(reader);
			fifaFile.ReleaseReader(reader);
			return result;
		}

		public bool Load(string fileName)
		{
			bool flag = false;
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			flag = Load(binaryReader);
			fileStream.Close();
			binaryReader.Close();
			m_FileName = fileName;
			return flag;
		}

		public bool Save(string fileName, bool saveBitmaps, bool saveVertex)
		{
			bool flag = false;
			FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			flag = Save(binaryWriter, saveBitmaps, saveVertex);
			fileStream.Close();
			binaryWriter.Close();
			m_FileName = fileName;
			return flag;
		}

		public virtual bool Load(BinaryReader r)
		{
			string a = new string(r.ReadChars(4));
			if (a == "RX3b")
			{
				m_IsFifa12 = true;
				m_SwapEndian = true;
				r.BaseStream.Position -= 4L;
			}
			else if (a == "RX3l")
			{
				m_IsFifa12 = true;
				m_SwapEndian = false;
				r.BaseStream.Position -= 4L;
			}
			else
			{
				m_IsFifa12 = false;
				m_SwapEndian = true;
				r.BaseStream.Position = 68L;
				m_Rx3bPosition = FifaUtil.SwapEndian(r.ReadInt32());
				r.BaseStream.Position = m_Rx3bPosition;
			}
			m_Rx3Header = new Rx3Header(r, m_SwapEndian);
			if (m_Rx3Header.NFiles == 0)
			{
				return false;
			}
			m_Rx3FileDescriptors = new Rx3FileDescriptor[m_Rx3Header.NFiles];
			for (int i = 0; i < m_Rx3Header.NFiles; i++)
			{
				m_Rx3FileDescriptors[i] = new Rx3FileDescriptor(r, m_SwapEndian);
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int j = 0; j < m_Rx3Header.NFiles; j++)
			{
				r.BaseStream.Position = m_Rx3FileDescriptors[j].Offset;
				switch (m_Rx3FileDescriptors[j].Signature)
				{
				case 1808827868u:
					m_Rx3ImageDirectory = new Rx3ImageDirectory(r, m_SwapEndian);
					m_Rx3Images = new Rx3Image[m_Rx3ImageDirectory.NFiles];
					break;
				case 2047566042u:
					m_Rx3Images[num] = new Rx3Image(r, m_SwapEndian);
					num++;
					break;
				case 582139446u:
					m_Rx3ModelDirectory = new Rx3ModelDirectory(r, m_SwapEndian);
					m_Rx3IndexArrays = new Rx3IndexArray[m_Rx3ModelDirectory.NFiles];
					m_Rx3VertexArrays = new Rx3VertexArray[m_Rx3ModelDirectory.NFiles];
					break;
				case 5798132u:
					m_Rx3IndexArrays[num2++] = new Rx3IndexArray(r, m_SwapEndian);
					break;
				case 5798561u:
					m_Rx3VertexArrays[num3++] = new Rx3VertexArray(r, m_SwapEndian);
					break;
				case 1285267122u:
					r.ReadBytes(20);
					m_AlfaFlag = r.ReadByte();
					break;
				case 4116388378u:
					LoadPositions(r);
					break;
				default:
					return false;
				case 3263271920u:
				case 3566041216u:
				case 3751472158u:
				case 4185470741u:
				case 123459928u:
				case 191347397u:
				case 230948820u:
				case 255353250u:
				case 341785025u:
				case 685399266u:
				case 899336811u:
				case 2116321516u:
					break;
				}
			}
			return true;
		}

		private void LoadPositions(BinaryReader r)
		{
			if (m_SwapEndian)
			{
				r.ReadBytes(29);
				m_Positions[0] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[1] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[2] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[3] = FifaUtil.SwapAndConvertToFloat(r);
				r.ReadBytes(11);
				m_Positions[4] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[5] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[6] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[7] = FifaUtil.SwapAndConvertToFloat(r);
				r.ReadBytes(12);
				m_Positions[8] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[9] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[10] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[11] = FifaUtil.SwapAndConvertToFloat(r);
				r.ReadBytes(5);
				m_Positions[12] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[13] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[14] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[15] = FifaUtil.SwapAndConvertToFloat(r);
				r.ReadBytes(15);
				m_Positions[16] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[17] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[18] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[19] = FifaUtil.SwapAndConvertToFloat(r);
				r.ReadBytes(16);
				m_Positions[20] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[21] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[22] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[23] = FifaUtil.SwapAndConvertToFloat(r);
				r.ReadBytes(13);
				m_Positions[24] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[25] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[26] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[27] = FifaUtil.SwapAndConvertToFloat(r);
				r.ReadBytes(7);
				m_Positions[28] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[29] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[30] = FifaUtil.SwapAndConvertToFloat(r);
				m_Positions[31] = FifaUtil.SwapAndConvertToFloat(r);
			}
			else
			{
				r.ReadBytes(29);
				m_Positions[0] = r.ReadSingle();
				m_Positions[1] = r.ReadSingle();
				m_Positions[2] = r.ReadSingle();
				m_Positions[3] = r.ReadSingle();
				r.ReadBytes(11);
				m_Positions[4] = r.ReadSingle();
				m_Positions[5] = r.ReadSingle();
				m_Positions[6] = r.ReadSingle();
				m_Positions[7] = r.ReadSingle();
				r.ReadBytes(12);
				m_Positions[8] = r.ReadSingle();
				m_Positions[9] = r.ReadSingle();
				m_Positions[10] = r.ReadSingle();
				m_Positions[11] = r.ReadSingle();
				r.ReadBytes(5);
				m_Positions[12] = r.ReadSingle();
				m_Positions[13] = r.ReadSingle();
				m_Positions[14] = r.ReadSingle();
				m_Positions[15] = r.ReadSingle();
				r.ReadBytes(15);
				m_Positions[16] = r.ReadSingle();
				m_Positions[17] = r.ReadSingle();
				m_Positions[18] = r.ReadSingle();
				m_Positions[19] = r.ReadSingle();
				r.ReadBytes(16);
				m_Positions[20] = r.ReadSingle();
				m_Positions[21] = r.ReadSingle();
				m_Positions[22] = r.ReadSingle();
				m_Positions[23] = r.ReadSingle();
				r.ReadBytes(13);
				m_Positions[24] = r.ReadSingle();
				m_Positions[25] = r.ReadSingle();
				m_Positions[26] = r.ReadSingle();
				m_Positions[27] = r.ReadSingle();
				r.ReadBytes(7);
				m_Positions[28] = r.ReadSingle();
				m_Positions[29] = r.ReadSingle();
				m_Positions[30] = r.ReadSingle();
				m_Positions[31] = r.ReadSingle();
			}
		}

		private void SavePositions(BinaryWriter w)
		{
			if (m_SwapEndian)
			{
				w.Seek(29, SeekOrigin.Current);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[0]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[1]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[2]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[3]);
				w.Seek(11, SeekOrigin.Current);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[4]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[5]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[6]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[7]);
				w.Seek(12, SeekOrigin.Current);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[8]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[9]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[10]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[11]);
				w.Seek(5, SeekOrigin.Current);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[12]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[13]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[14]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[15]);
				w.Seek(15, SeekOrigin.Current);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[16]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[17]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[18]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[19]);
				w.Seek(16, SeekOrigin.Current);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[20]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[21]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[22]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[23]);
				w.Seek(13, SeekOrigin.Current);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[24]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[25]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[26]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[27]);
				w.Seek(7, SeekOrigin.Current);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[28]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[29]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[30]);
				FifaUtil.SwapAndWriteFloat(w, m_Positions[31]);
			}
			else
			{
				w.Seek(29, SeekOrigin.Current);
				w.Write(m_Positions[0]);
				w.Write(m_Positions[1]);
				w.Write(m_Positions[2]);
				w.Write(m_Positions[3]);
				w.Seek(11, SeekOrigin.Current);
				w.Write(m_Positions[4]);
				w.Write(m_Positions[5]);
				w.Write(m_Positions[6]);
				w.Write(m_Positions[7]);
				w.Seek(12, SeekOrigin.Current);
				w.Write(m_Positions[8]);
				w.Write(m_Positions[9]);
				w.Write(m_Positions[10]);
				w.Write(m_Positions[11]);
				w.Seek(5, SeekOrigin.Current);
				w.Write(m_Positions[12]);
				w.Write(m_Positions[13]);
				w.Write(m_Positions[14]);
				w.Write(m_Positions[15]);
				w.Seek(15, SeekOrigin.Current);
				w.Write(m_Positions[16]);
				w.Write(m_Positions[17]);
				w.Write(m_Positions[18]);
				w.Write(m_Positions[19]);
				w.Seek(16, SeekOrigin.Current);
				w.Write(m_Positions[20]);
				w.Write(m_Positions[21]);
				w.Write(m_Positions[22]);
				w.Write(m_Positions[23]);
				w.Seek(13, SeekOrigin.Current);
				w.Write(m_Positions[24]);
				w.Write(m_Positions[25]);
				w.Write(m_Positions[26]);
				w.Write(m_Positions[27]);
				w.Seek(7, SeekOrigin.Current);
				w.Write(m_Positions[28]);
				w.Write(m_Positions[29]);
				w.Write(m_Positions[30]);
				w.Write(m_Positions[31]);
			}
		}

		public virtual bool Save(BinaryWriter w, bool saveBitmaps, bool saveVertex)
		{
			if (m_Rx3Signatures != null)
			{
				m_Rx3Signatures.Save(w);
			}
			w.BaseStream.Position = m_Rx3bPosition;
			m_Rx3Header.Save(w);
			for (int i = 0; i < m_Rx3Header.NFiles; i++)
			{
				m_Rx3FileDescriptors[i].Save(w);
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int j = 0; j < m_Rx3Header.NFiles; j++)
			{
				w.BaseStream.Position = m_Rx3FileDescriptors[j].Offset;
				switch (m_Rx3FileDescriptors[j].Signature)
				{
				case 2047566042u:
					if (saveBitmaps)
					{
						m_Rx3Images[num].Save(w);
						num++;
					}
					break;
				case 5798132u:
					if (saveVertex)
					{
						m_Rx3IndexArrays[num3++].Save(w);
					}
					break;
				case 5798561u:
					if (saveVertex)
					{
						m_Rx3VertexArrays[num2++].Save(w);
					}
					break;
				case 4116388378u:
					SavePositions(w);
					break;
				}
			}
			return true;
		}

		public Bitmap[] GetBitmaps()
		{
			Bitmap[] array = new Bitmap[m_Rx3Images.Length];
			for (int i = 0; i < m_Rx3Images.Length; i++)
			{
				array[i] = m_Rx3Images[i].GetBitmap();
			}
			return array;
		}

		public bool ReplaceBitmaps(Bitmap[] bitmaps)
		{
			bool flag = true;
			int num = (bitmaps.Length < m_Rx3Images.Length) ? bitmaps.Length : m_Rx3Images.Length;
			for (int i = 0; i < num; i++)
			{
				if (bitmaps[i] != null)
				{
					bool flag2 = m_Rx3Images[i].SetBitmap(bitmaps[i]);
					flag = (flag && flag2);
				}
			}
			return flag;
		}

		public bool CreateXFiles(string baseFileName)
		{
			if (m_Rx3IndexArrays.Length == 0)
			{
				return false;
			}
			for (int i = 0; i < m_Rx3IndexArrays.Length; i++)
			{
				Model3D model3D = new Model3D();
				model3D.Initialize(m_Rx3IndexArrays[i], m_Rx3VertexArrays[i]);
				string path = baseFileName + i.ToString() + ".X";
				Application.CurrentCulture = new CultureInfo("en-us");
				FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
				StreamWriter streamWriter = new StreamWriter(fileStream);
				streamWriter.WriteLine("xof 0303txt 0032");
				model3D.SaveXFile(streamWriter);
				streamWriter.Close();
				fileStream.Close();
			}
			return true;
		}

		public bool ConvertKitFrom11(Rx3File sourceRx3File)
		{
			if (IsFifa11)
			{
				return false;
			}
			if (sourceRx3File.IsFifa12)
			{
				return false;
			}
			return true;
		}

		public bool ConvertKitFrom12(Rx3File sourceRx3File)
		{
			if (IsFifa12)
			{
				return false;
			}
			if (sourceRx3File.IsFifa11)
			{
				return false;
			}
			return true;
		}

		public bool ConvertKit(Rx3File sourceRx3File)
		{
			if (IsFifa12)
			{
				return ConvertKitFrom11(sourceRx3File);
			}
			return ConvertKitFrom12(sourceRx3File);
		}
	}
}
