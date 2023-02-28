namespace Modding.Categories
{
    [ModCategory(ModMainCategory.Faces)]
    public enum ModFacesSubCategory : byte
    {
        [System.ComponentModel.Description("Face Update")]
        FaceUpdate = 1,
        [System.ComponentModel.Description("New Face")]
        NewFace = 2,
        Facepack = 3,
        None = 0,
        Custom = byte.MaxValue
    }
}
