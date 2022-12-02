using FMT.FileTools;
using FrostySdk.IO;
using System.Collections.Generic;

namespace FrostySdk.Resources
{
	public class ShaderBlockResource
	{
		public int Index;

		public ulong Hash;

		public bool IsModified;

		public virtual void Read(NativeReader reader, List<ShaderBlockResource> shaderBlockEntries)
		{
			Hash = reader.ReadULong();
			IsModified = false;
		}

		internal virtual void Save(NativeWriter writer, List<int> relocTable, out long startOffset)
		{
			startOffset = writer.BaseStream.Position;
			writer.Write(Hash);
			relocTable.Add((int)writer.BaseStream.Position);
		}
	}
}
