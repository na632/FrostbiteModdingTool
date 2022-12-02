using FMT.FileTools;
using FrostySdk.IO;
using System.IO;
using System.Text;

namespace FrostySdk.Resources
{
	public class IesResource
	{
		private int size;

		private float unknown1;

		private float unknown2;

		private MemoryStream data;

		public int Size => size;

		public Stream Data => data;

		public IesResource(Stream stream)
		{
			using (NativeReader nativeReader = new NativeReader(stream))
			{
				size = nativeReader.ReadInt();
				unknown1 = nativeReader.ReadFloat();
				unknown2 = nativeReader.ReadFloat();
				nativeReader.ReadInt();
				nativeReader.Position += 16L;
				data = new MemoryStream(nativeReader.ReadBytes(size * size * 2));
				nativeReader.ReadInt();
			}
		}

		public string ToDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Size: " + size);
			stringBuilder.AppendLine("Unknown1: " + unknown1);
			stringBuilder.AppendLine("Unknown2: " + unknown2);
			return stringBuilder.ToString();
		}
	}
}
