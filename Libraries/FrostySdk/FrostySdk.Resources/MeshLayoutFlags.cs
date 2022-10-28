using System;

namespace FrostySdk.Resources
{
	[Flags]
	public enum EMeshLayout : ulong
	{
        IsBaseLod = 1u,
        StreamInstancingEnable = 0x10u,
        StreamingEnable = 0x40u,
        VertexAnimationEnable = 0x80u,
        Deformation = 0x100u,
        MultiStreamEnable = 0x200u,
        SubsetSortingEnable = 0x400u,
        Inline = 0x800u,
        AlternateBatchSorting = 0x1000u,
        ProjectedDecalsEnable = 0x8000u,
        SrvEnable = 0x20000u,
        IsMeshFront = 0x10000000u,
        IsDataAvailable = 0x20000000u
    }
}
