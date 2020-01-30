using System.Drawing;
using System.IO;

namespace FifaLibrary
{
	public class Fsh
	{
		private string m_FileName;

		private string m_DirId;

		private string m_Signature;

		private int m_FileSize;

		private int m_NImages;

		private ImageDir[] m_ImageDir;

		private byte[] m_DirPad;

		private FshImage[] m_FshImages;

		public string FileName
		{
			get
			{
				return m_FileName;
			}
			set
			{
				m_FileName = value;
			}
		}

		public int FileSize
		{
			get
			{
				return m_FileSize;
			}
			set
			{
				m_FileSize = value;
			}
		}

		public ImageDir[] ImageDir
		{
			get
			{
				return m_ImageDir;
			}
			set
			{
				m_ImageDir = value;
			}
		}

		public FshImage[] FshImages
		{
			get
			{
				return m_FshImages;
			}
			set
			{
				m_FshImages = value;
			}
		}

		public Fsh(BinaryReader r)
		{
			Load(r);
		}

		public Fsh(string filePath)
		{
			m_FileName = filePath;
			FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			if (fileStream != null)
			{
				BinaryReader binaryReader = new BinaryReader(fileStream);
				if (binaryReader != null)
				{
					Load(binaryReader);
					binaryReader.Close();
					fileStream.Close();
				}
			}
		}

		public bool Load(BinaryReader r)
		{
			if ((int)r.BaseStream.Length < 4)
			{
				return false;
			}
			m_Signature = new string(r.ReadChars(4));
			if (m_Signature != "SHPI")
			{
				return false;
			}
			m_FileSize = r.ReadInt32();
			m_NImages = r.ReadInt32();
			m_DirId = new string(r.ReadChars(4));
			m_ImageDir = new ImageDir[m_NImages];
			for (int i = 0; i < m_NImages; i++)
			{
				m_ImageDir[i] = new ImageDir(r);
			}
			int num = m_FileSize;
			for (int j = 0; j < m_NImages; j++)
			{
				if (m_ImageDir[j].Offset < num)
				{
					num = m_ImageDir[j].Offset;
				}
			}
			int num2 = num - (16 + 8 * m_NImages);
			if (num2 > 0)
			{
				m_DirPad = r.ReadBytes(num2);
			}
			m_FshImages = new FshImage[m_NImages];
			for (int k = 0; k < m_NImages; k++)
			{
				int maxSize = (k + 1 < m_NImages) ? (m_ImageDir[k + 1].Offset - m_ImageDir[k].Offset) : (m_FileSize - m_ImageDir[k].Offset);
				m_FshImages[k] = new FshImage(this, r, maxSize);
			}
			return true;
		}

		public bool Save(BinaryWriter w)
		{
			w.Write('S');
			w.Write('H');
			w.Write('P');
			w.Write('I');
			w.Write(m_FileSize);
			w.Write(m_NImages);
			char[] array = m_DirId.ToCharArray();
			w.Write((byte)array[0]);
			w.Write((byte)array[1]);
			w.Write((byte)array[2]);
			w.Write((byte)array[3]);
			for (int i = 0; i < m_NImages; i++)
			{
				m_ImageDir[i].Save(w);
			}
			if (m_DirPad != null)
			{
				w.Write(m_DirPad);
			}
			for (int j = 0; j < m_NImages; j++)
			{
				m_FshImages[j].Save(w);
			}
			return true;
		}

		public bool Save()
		{
			return Save(m_FileName);
		}

		public bool Save(string fileName)
		{
			if (fileName == null)
			{
				return false;
			}
			FileStream fileStream = new FileStream(fileName, FileMode.Create);
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			bool result = Save(binaryWriter);
			binaryWriter.Close();
			fileStream.Close();
			return result;
		}

		public void Hash(string fileName)
		{
			for (int i = 0; i < m_NImages; i++)
			{
				m_FshImages[i].Hash(fileName);
			}
		}

		public void ComputeImageDir()
		{
			int num = m_ImageDir[0].Offset;
			for (int i = 0; i < m_NImages; i++)
			{
				m_ImageDir[i].Offset = num;
				num += m_FshImages[i].ComputeLength();
			}
			m_FileSize = num;
		}

		public void ReplaceBitmaps(Bitmap[] bitmaps)
		{
			int num = (bitmaps.Length <= m_NImages) ? bitmaps.Length : m_NImages;
			for (int i = 0; i < num; i++)
			{
				m_FshImages[i].ReplaceBitmap(bitmaps[i]);
			}
		}

		public bool ReplaceBitmap(Bitmap bitmap, int index)
		{
			if (index < m_NImages)
			{
				return m_FshImages[index].ReplaceBitmap(bitmap);
			}
			return false;
		}

		public Bitmap GetBitmap(int index)
		{
			if (index < m_NImages)
			{
				return m_FshImages[index].Bitmap;
			}
			return null;
		}

		public Bitmap[] GetBitmaps()
		{
			Bitmap[] array = new Bitmap[m_NImages];
			for (int i = 0; i < m_NImages; i++)
			{
				array[i] = m_FshImages[i].Bitmap;
			}
			return array;
		}
	}
}
