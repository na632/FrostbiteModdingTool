using FrostySdk.IO;
using System;
using System.Collections.Generic;

namespace FrostySdk.Resources.Old
{
	public class MeshParamDbBlock : ShaderBlockResource
	{
		public Guid UnknownGuid;

		public int LodIndex;

		public List<ParameterEntry> Parameters = new List<ParameterEntry>();

		public MeshParamDbBlock(NativeReader reader)
			: base(reader)
		{
			reader.ReadULong();
			reader.ReadInt();
			LodIndex = reader.ReadInt();
			UnknownGuid = reader.ReadGuid();
			reader.Pad(16);
			int num = reader.ReadInt();
			for (int i = 0; i < num; i++)
			{
				Parameters.Add(new ParameterEntry(reader));
			}
		}

		public override Resources.ShaderBlockResource Convert()
		{
			return new Resources.MeshParamDbBlock
			{
				Hash = Hash,
				Parameters = Parameters,
				UnknownGuid = UnknownGuid,
				LodIndex = LodIndex,
				IsModified = true
			};
		}
	}
}
