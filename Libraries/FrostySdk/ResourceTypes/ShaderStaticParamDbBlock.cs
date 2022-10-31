using FrostySdk.IO;
using System.Collections.Generic;

namespace FrostySdk.Resources
{
	public class ShaderStaticParamDbBlock : ShaderBlockResource
	{
		public List<ShaderBlockResource> Resources = new List<ShaderBlockResource>();

		public override void Read(NativeReader reader, List<ShaderBlockResource> shaderBlockEntries)
		{
			base.Read(reader, shaderBlockEntries);
			long position = reader.ReadLong();
			long num = reader.ReadLong();
			reader.Position = position;
			for (long num2 = 0L; num2 < num; num2++)
			{
				int index = reader.ReadInt();
				Resources.Add(shaderBlockEntries[index]);
			}
		}

		internal override void Save(NativeWriter writer, List<int> relocTable, out long startOffset)
		{
			long position = writer.BaseStream.Position;
			for (int i = 0; i < Resources.Count; i++)
			{
				writer.Write(Resources[i].Index);
			}
			writer.WritePadding(8);
			base.Save(writer, relocTable, out startOffset);
			writer.Write(position);
			writer.Write((long)Resources.Count);
		}
	}
}
