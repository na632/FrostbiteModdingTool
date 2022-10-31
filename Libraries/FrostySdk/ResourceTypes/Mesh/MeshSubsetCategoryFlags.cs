using System;

namespace FrostySdk.Resources
{
	[Flags]
    public enum MeshSubsetCategoryFlags
    {
        MeshSubsetCategoryFlags_Opaque = 1,
        MeshSubsetCategoryFlags_Transparent = 2,
        MeshSubsetCategoryFlags_TransparentDecal = 4,
        MeshSubsetCategoryFlags_ZOnly = 8,
        MeshSubsetCategoryFlags_Shadow = 16,
        MeshSubsetCategoryFlags_DynamicReflection = 32,
        MeshSubsetCategoryFlags_PlanarReflection = 64,
        MeshSubsetCategoryFlags_StaticReflection = 128,
        MeshSubsetCategoryFlags_DistantShadowCache = 256,
        MeshSubsetCategoryFlags_ShadowOverride = 512,
        MeshSubsetCategoryFlags_DynamicReflectionOverride = 1024,
        MeshSubsetCategoryFlags_PlanarReflectionOverride = 2048,
        MeshSubsetCategoryFlags_StaticReflectionOverride = 4096,
        MeshSubsetCategoryFlags_DistantShadowCacheOverride = 8192,
        MeshSubsetCategoryFlags_ZPass = 16384,
        MeshSubsetCategoryFlags_PlanarShadow = 32768,
        MeshSubsetCategoryFlags_PlanarShadowOverride = 65536,
        MeshSubsetCategoryFlags_BakedLighting = 131072,
        MeshSubsetCategoryFlags_ForwardDepthPass = 262144,
        MeshSubsetCategoryFlags_NoForwardDepthPass = 524288,
        MeshSubsetCategoryFlags_Normal = 7,
        MeshSubsetCategoryFlags_All = 1048575
    }
}
