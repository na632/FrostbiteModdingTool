using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static FrostySdk.Managers.AssetManager;

namespace BF4Plugin
{
    public class BF4AssetLoader : IAssetLoader
    {
		public void Load(AssetManager parent, BinarySbDataHelper helper)
		{
			foreach (string superBundle in parent.fs.SuperBundles)
			{
				parent.superBundles.Add(new SuperBundleEntry
				{
					Name = superBundle
				});
				bool flag = false;
				DbObject dbObject = parent.ProcessTocChunks($"native_data/{superBundle}.toc", helper, isBase: true);
				if (dbObject == null)
				{
					continue;
				}
				bool flag2 = dbObject.GetValue("alwaysEmitSuperBundle", defaultValue: false);
				if (ProfilesLibrary.DataVersion == 20140225 || ProfilesLibrary.DataVersion == 20150223)
				{
					flag2 = true;
				}
				parent.logger.Log($"Loading data ({superBundle})");
				DbObject dbObject2 = parent.ProcessTocChunks($"native_patch/{superBundle}.toc", helper);
				DbObject value = dbObject.GetValue<DbObject>("bundles");
				DbObject dbObject3 = value;
				if (value.Count == 0)
				{
					continue;
				}
				Dictionary<string, BaseBundleInfo> dictionary = new Dictionary<string, BaseBundleInfo>();
				if (flag2)
				{
					foreach (DbObject item in value)
					{
						BaseBundleInfo baseBundleInfo = new BaseBundleInfo();
						baseBundleInfo.Name = item.GetValue<string>("id");
						baseBundleInfo.Offset = item.GetValue("offset", 0L);
						baseBundleInfo.Size = item.GetValue("size", 0L);
						dictionary.Add(baseBundleInfo.Name.ToLower(), baseBundleInfo);
					}
				}
				if (dbObject2 != null)
				{
					dbObject3 = dbObject2.GetValue<DbObject>("bundles");
					flag = true;
				}
				NativeReader nativeReader = new NativeReader(new FileStream(parent.fs.ResolvePath($"native_data/{superBundle}.sb"), FileMode.Open, FileAccess.Read));
				NativeReader nativeReader2 = nativeReader;
				if (flag)
				{
					nativeReader2 = new NativeReader(new FileStream(parent.fs.ResolvePath($"native_patch/{superBundle}.sb"), FileMode.Open, FileAccess.Read));
				}
				foreach (DbObject item2 in dbObject3)
				{
					string text = item2.GetValue<string>("id").ToLower();
					long value2 = item2.GetValue("offset", 0L);
					long value3 = item2.GetValue("size", 0L);
					bool value4 = item2.GetValue("delta", defaultValue: false);
					bool value5 = item2.GetValue("base", defaultValue: false);
					parent.bundles.Add(new BundleEntry
					{
						Name = text,
						SuperBundleId = parent.superBundles.Count - 1
					});
					int bundleId = parent.bundles.Count - 1;
					Stream stream = (value5 ? nativeReader.CreateViewStream(value2, value3) : nativeReader2.CreateViewStream(value2, value3));
					DbObject dbObject5 = null;
					if (flag2)
					{
						if (value4)
						{
							BaseBundleInfo baseBundleInfo2 = (dictionary.ContainsKey(text) ? dictionary[text] : null);
							using (BinarySbReader binarySbReader = new BinarySbReader((baseBundleInfo2 != null) ? nativeReader.CreateViewStream(baseBundleInfo2.Offset, baseBundleInfo2.Size) : null, stream, parent.fs.CreateDeobfuscator()))
							{
								dbObject5 = binarySbReader.ReadDbObject();
							}
							DbObject baseList = null;
							if (baseBundleInfo2 != null)
							{
								using BinarySbReader binarySbReader2 = new BinarySbReader(nativeReader.CreateViewStream(baseBundleInfo2.Offset, baseBundleInfo2.Size), baseBundleInfo2.Offset, parent.fs.CreateDeobfuscator());
								baseList = binarySbReader2.ReadDbObject();
							}
							helper.FilterAndAddBundleData(baseList, dbObject5);
						}
						else
						{
							using BinarySbReader binarySbReader3 = new BinarySbReader(stream, value2, parent.fs.CreateDeobfuscator());
							dbObject5 = binarySbReader3.ReadDbObject();
						}
					}
					else
					{
						using DbReader dbReader = new DbReader(stream, parent.fs.CreateDeobfuscator());
						dbObject5 = dbReader.ReadDbObject();
					}
					parent.ProcessBundleEbx(dbObject5, bundleId, helper);
					parent.ProcessBundleRes(dbObject5, bundleId, helper);
					parent.ProcessBundleChunks(dbObject5, bundleId, helper);
				}
				nativeReader.Dispose();
				nativeReader2.Dispose();
				if (flag2)
				{
					GC.Collect();
				}
			}
		}
    }
}
