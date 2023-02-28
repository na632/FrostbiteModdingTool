namespace FrostySdk.IO
{
    public struct CatPatchEntry
    {
        public FMT.FileTools.Sha1 Sha1;

        public FMT.FileTools.Sha1 BaseSha1;

        public FMT.FileTools.Sha1 DeltaSha1;
    }
}
