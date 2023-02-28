namespace FrostySdk.Resources
{
    public enum ShaderDrawOrder
    {
        ShaderDrawOrder_Invalid = 0,
        ShaderDrawOrder_TransparentTerrainLayer = 1,
        ShaderDrawOrder_TerrainDecal = 2,
        ShaderDrawOrder_Decal = 3,
        ShaderDrawOrder_Sky = 4,
        ShaderDrawOrder_EmitterQuadBackground = 5,
        ShaderDrawOrder_Pitch = 10,
        ShaderDrawOrder_Crowd = 11,
        ShaderDrawOrder_Default = 31,
        ShaderDrawOrder_MeshScattering = 32,
        ShaderDrawOrder_TerrainDecalZPass = 33,
        ShaderDrawOrder_TerrainPatch = 34,
        ShaderDrawOrder_Water = 35,
        ShaderDrawOrder_EmitterQuad = 36,
        ShaderDrawOrder_EmitterQuadForeground = 37,
        ShaderDrawOrder_EmitterMeshForeground = 38,
        ShaderDrawOrder_LensFlareQuad = 39
    }
}
