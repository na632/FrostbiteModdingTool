using System.Drawing;
using System.IO;

namespace FifaLibrary
{
	public class FshImage
	{
		private FshSection m_MainSection;

		private Fsh m_Parent;

		public Bitmap Bitmap => m_MainSection.m_Bitmap;

		public FshImage(Fsh parent, BinaryReader r, int maxSize)
		{
			m_Parent = parent;
			m_MainSection = new FshSection(parent, r, maxSize);
			m_MainSection.RawDataToBmp();
		}

		public void Save(BinaryWriter w)
		{
			m_MainSection.Save(w);
		}

		public bool ReplaceBitmap(Bitmap bitmap)
		{
			return m_MainSection.ReplaceBitmap(bitmap);
		}

		public bool ReplaceRawData(byte[] rawData, int width, int height)
		{
			return m_MainSection.ReplaceRawData(rawData, width, height);
		}

		public byte[] GetRawData()
		{
			return m_MainSection.m_RawData;
		}

		public void Hash(string fileName)
		{
			FshSection nextSection = m_MainSection.NextSection;
			while (true)
			{
				if (nextSection != null)
				{
					if (nextSection.Type == 111)
					{
						break;
					}
					nextSection = nextSection.NextSection;
					continue;
				}
				return;
			}
			nextSection.Hash(fileName);
		}

		public int ComputeLength()
		{
			int num = m_MainSection.m_Size;
			for (FshSection nextSection = m_MainSection.NextSection; nextSection != null; nextSection = nextSection.NextSection)
			{
				num += nextSection.m_Size;
			}
			return num;
		}
	}
}
