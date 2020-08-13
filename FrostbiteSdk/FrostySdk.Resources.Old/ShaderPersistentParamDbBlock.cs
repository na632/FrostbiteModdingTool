using FrostySdk.IO;
using System.Collections.Generic;

namespace FrostySdk.Resources.Old
{
	public class ShaderPersistentParamDbBlock : ShaderBlockResource
	{
		public List<ParameterEntry> Parameters = new List<ParameterEntry>();

		public ShaderPersistentParamDbBlock(NativeReader reader)
			: base(reader)
		{
			reader.ReadULong();
			reader.ReadInt();
			reader.Pad(16);
			int num = reader.ReadInt();
			for (int i = 0; i < num; i++)
			{
				Parameters.Add(new ParameterEntry(reader));
			}
		}

		public override Resources.ShaderBlockResource Convert()
		{
			return new Resources.ShaderPersistentParamDbBlock
			{
				Hash = Hash,
				Parameters = Parameters,
				IsModified = true
			};
		}
	}
}
