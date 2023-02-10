using FMT.FileTools;
using FrostySdk.IO;
using System.Collections.Generic;

namespace FrostySdk.Resources
{
	public class ShaderPersistentParamDbBlock : ShaderBlockResource
	{
		public List<ParameterEntry> Parameters = new List<ParameterEntry>();

		public override void Read(NativeReader reader, List<ShaderBlockResource> shaderBlockEntries)
		{
			base.Read(reader, shaderBlockEntries);
			long position = reader.ReadLong();
			reader.ReadLong();
			reader.Position = position;
			int num = reader.ReadInt();
			for (int i = 0; i < num; i++)
			{
				Parameters.Add(new ParameterEntry(reader));
			}
		}

		public object GetParameterValue(string name)
		{
			object result = null;
			uint num = (uint)Fnv1.HashString(name.ToLower());
			foreach (ParameterEntry parameter in Parameters)
			{
				if (parameter.NameHash == num)
				{
					return parameter.GetValue();
				}
			}
			return result;
		}

		public void SetParameterValue(string name, object value)
		{
			if (!string.IsNullOrEmpty(name))
			{
				uint num = (uint)Fnv1.HashString(name.ToLower());
				foreach (ParameterEntry parameter in Parameters)
				{
					if (parameter.NameHash == num)
					{
						parameter.SetValue(value);
						return;
					}
				}
				Parameters.Add(new ParameterEntry(name, value));
			}
		}

		internal override void Save(NativeWriter writer, List<int> relocTable, out long startOffset)
		{
			long position = writer.BaseStream.Position;
			writer.Write(Parameters.Count);
			foreach (ParameterEntry parameter in Parameters)
			{
				writer.Write(parameter.ToBytes());
			}
			long value = writer.BaseStream.Position - position;
			writer.WritePadding(8);
			base.Save(writer, relocTable, out startOffset);
			writer.Write(position);
			writer.Write(value);
		}
	}
}
