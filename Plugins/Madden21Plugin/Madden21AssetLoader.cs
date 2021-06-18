using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static FrostySdk.Managers.AssetManager;

namespace Madden21Plugin
{

	public class Madden21AssetLoader : IAssetLoader
	{
		public class BundleFileInfo
		{
			public class BundleItem : IEquatable<BundleItem>
            {
				public string EbxResOrChunk { get; set; }
				public string NameOrGuid { get; set; }

				public BundleItem(string type, string name)
                {
					EbxResOrChunk = type;
					NameOrGuid = name;
                }

				public override bool Equals(object obj)
				{
					if (obj is BundleItem)
					{
						return Equals(obj);
					}
					return base.Equals(obj);
				}

                public override int GetHashCode()
                {
                    return base.GetHashCode();
                }

                public bool Equals(BundleItem other)
                {
					return (other.EbxResOrChunk == this.EbxResOrChunk && other.NameOrGuid == this.NameOrGuid);
                }

                public override string ToString()
                {
					return $"{EbxResOrChunk} - {NameOrGuid}";
                }
            }


			/// <summary>
			/// A list of all the items included in the bundle by name / guid
			/// </summary>
			public List<BundleItem> ItemsInBundle;

			public DbObject BundleObject = new DbObject();

			public int Index;

			public int TocIndex 
			{
				get
                {

					var mangledCasIndex = (Index + int.MinValue);
					return mangledCasIndex;
				}
			}

			public int Offset;

			public int Size;

			public long OffsetPosition;

			public long SizePosition;

			public long CasIndexPosition;

			public bool IsParent;

			public BundleFileInfo Parent = null;

			public BundleFileInfo(int index, int offset, int size, long offset_pos, long size_pos, long casIndex_pos, bool isParent, BundleFileInfo parent)
			{
				Index = index;
				Offset = offset;
				Size = size;
				OffsetPosition = offset_pos;
				SizePosition = size_pos;
				CasIndexPosition = casIndex_pos;
				ItemsInBundle = new List<BundleItem>();
				IsParent = isParent;
				Parent = parent;
			}

            public override string ToString()
            {
				return Index.ToString() + "-offset:(" + Offset + ")-size:(" + Size.ToString() + ")";
            }

            public override bool Equals(object obj)
            {
				if (obj is BundleFileInfo)
				{
					var other = obj as BundleFileInfo;
					if (other != null)
					{
					}
				}
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

		public List<TOCFile> LoadedTOCFiles = new List<TOCFile>();

		public void Load(AssetManager parent, BinarySbDataHelper helper)
		{
			byte[] key = KeyManager.Instance.GetKey("Key2");
			foreach (Catalog item2 in parent.fs.EnumerateCatalogInfos())
			{
				foreach (string sbName in item2.SuperBundles.Keys)
				{
					SuperBundleEntry superBundleEntry = parent.superBundles.Find((SuperBundleEntry a) => a.Name == sbName);
					int sbIndex = -1;
					if (superBundleEntry != null)
					{
						sbIndex = parent.superBundles.IndexOf(superBundleEntry);
					}
					else
					{
						parent.superBundles.Add(new SuperBundleEntry
						{
							Name = sbName
						});
						sbIndex = parent.superBundles.Count - 1;
					}
					parent.logger.Log($"Loading data ({sbName})");

					parent.superBundles.Add(new SuperBundleEntry
					{
						Name = sbName
					});
					string tocPath = sbName;
					if (item2.SuperBundles[sbName])
					{
						tocPath = sbName.Replace("win32", item2.Name);
					}


      //              if (
      //                  tocPath.Contains("attribsys") // GP
      //                  ||
      //                  tocPath.Contains("splash") // Splash
      //                  || tocPath.Contains("globals") // Legacy
      //                  || tocPath.Contains("frontendscene")
      //                  || tocPath.Contains("teamwipe_sb")
      //                  || tocPath.Contains("playercontent_sb") // Player uniforms / kits
      //                  || tocPath.Contains("playercontentlaunch_sb") // Player uniforms / kits
						//)
                    {

                        TOCFile tocFile = new TOCFile(tocPath);
						tocFile.Read(tocPath, parent, helper, sbIndex);
						sbIndex++;
						LoadedTOCFiles.Add(tocFile);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    }
                }
			}
		}
	}


}
