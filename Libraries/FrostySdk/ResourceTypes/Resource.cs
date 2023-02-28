using FMT.FileTools;
using FrostySdk.Managers;

namespace FrostySdk.Resources
{
    public class Resource
    {
        protected byte[] resMeta;

        protected ulong resRid;

        public virtual void Read(NativeReader reader, AssetManager am, ResAssetEntry entry, ModifiedResource modifiedData)
        {
            resMeta = entry.ResMeta;
            resRid = entry.ResRid;
        }

        internal virtual ModifiedResource Save()
        {
            return null;
        }
    }
}
