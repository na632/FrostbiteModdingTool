using System;

namespace FrostySdk.Resources
{
	[Flags]
	public enum MeshLayoutFlags
	{
		IsBaseLod = 0x1,
		StreamingEnable = 0x40,
		StreamInstancingEnable = 0x10,
		VertexAnimationEnable = 0x80,
		Inline = 0x800,
		IsDataAvailable = 0x20000000
	}
}
