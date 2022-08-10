using FrostySdk;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BFVPlugin
{
    public class BF5AssetLoader : IAssetLoader
    {
        public void Load(AssetManager parent, BinarySbDataHelper helper)
        {
			parent.logger.Log("Loading data from manifest");
			parent.superBundles.Add(new SuperBundleEntry
			{
				Name = "<none>"
			});
			foreach (DbObject bundle in parent.fs.EnumerateBundles())
			{
				BundleEntry item = new BundleEntry
				{
					Name = bundle.GetValue<string>("name"),
					SuperBundleId = 0
				};
				parent.bundles.Add(item);
				if (bundle != null)
				{
					parent.ProcessBundleEbx(bundle, parent.bundles.Count - 1, helper);
					parent.ProcessBundleRes(bundle, parent.bundles.Count - 1, helper);
					parent.ProcessBundleChunks(bundle, parent.bundles.Count - 1, helper);
				}
			}
			foreach (ChunkAssetEntry chunk in parent.fs.ProcessManifestChunks())
			{
				if (!parent.Chunks.ContainsKey(chunk.Id))
				{
					parent.Chunks.TryAdd(chunk.Id, chunk);
				}
			}
		}
    }
}
