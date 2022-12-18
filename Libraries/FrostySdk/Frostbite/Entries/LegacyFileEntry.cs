using Frosty.Hash;
using FrostySdk.FrostbiteSdk.Managers;
using FrostySdk.FrostySdk.Managers;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Xml;

namespace FrostySdk
{
	public class LegacyFileEntry : AssetEntry, IAssetEntry
	{
        public LegacyFileEntry(ModifiedAssetEntry modifiedAssetEntry = null)
        {
			ModifiedEntry = modifiedAssetEntry;
        }

        public class ChunkCollectorInstance
		{
			public EbxAssetEntry Entry { get; set; }

			public Guid ChunkId { get; set; }

			public long Offset { get; set; }

			public long CompressedOffset { get; set; }

			public long CompressedSize { get; set; }

			public long CompressedStartOffset { get; set; }
			public long CompressedEndOffset { get; set; }

		

			public long Size { get; set; }

			public ChunkCollectorInstance ModifiedEntry
			{
				get;
				set;
			}

			public bool IsModified => ModifiedEntry != null;
		}

		public List<ChunkCollectorInstance> CollectorInstances = new List<ChunkCollectorInstance>();
		
		public long CompressedOffset { get; set; }
		public long CompressedSize { get; set; }

		public long CompressedOffsetStart { get; set; }
		public long CompressedOffsetEnd { get; set; }


		public long CompressedOffsetPosition { get; set; }
		public long ActualOffsetPosition { get; set; }
		public long CompressedSizePosition { get; set; }
		public long ActualSizePosition { get; set; }
		public Guid ChunkId
		{
			get;set;
			//get
			//{
			//	if (CollectorInstances.Count == 0)
			//	{
			//		return Guid.Empty;
			//	}
			//	if (CollectorInstances[0].IsModified)
			//	{
			//		return CollectorInstances[0].ModifiedEntry.ChunkId;
			//	}
			//	return CollectorInstances[0].ChunkId;
			//}
		}

		public Guid ParentGuid { get; set; }

		public int NameHash => Fnv1.HashString(Name);


		public Sha1 GetSha1()
		{ 
			if(!string.IsNullOrEmpty(Name))
				return Sha1.Create(Encoding.ASCII.GetBytes(Name));
			return Sha1.Zero;
		}

        public override Sha1 Sha1 => GetSha1();

        public override string AssetType => "legacy";

		public override string Type
		{
			get
			{
				int num = Name.LastIndexOf('.');
				if (num == -1)
				{
					return "";
				}
				return Name.Substring(num + 1).ToUpper();
			}
			set
			{
			}
		}

		public override string Filename
		{
			get
			{
				int num = base.Filename.LastIndexOf('.');
				if (num == -1)
				{
					return base.Filename;
				}
				return base.Filename.Substring(0, num);
			}
		}

        public override string DisplayName
		{
			get
			{
				var result = base.Filename;
				if(Type == "XML")
				{
					var xmlDisplayName = GetFIFAXMLDisplayName();
					if (!xmlDisplayName.IsEmpty)
						result = xmlDisplayName.ToString();
                }
				return result;

            }
		}

		//private XmlDocument XmlDocument { get; set; }

		//public bool GetXmlData(out XmlDocument xmlDocument)
		//{

		//          if (Type.ToUpper() == "XML")
		//          {
		//              var data = AssetManager.Instance.GetCustomAsset("legacy", this);
		//              xmlDocument = new XmlDocument();
		//		try
		//		{
		//                  xmlDocument.Load(data);
		//			return true;
		//		}
		//		catch 
		//		{ 
		//		}
		//          }

		//	xmlDocument = null;
		//          return false;
		//      }

		public ReadOnlySpan<char> GetPathSpan()
		{
			return Path;
        }

        public ReadOnlySpan<char> GetFIFAXMLDisplayName()
		{
			if (Type != "XML")
			{
				return null;
			}

            string result = "";

			var pathSpan = GetPathSpan();


			if (
				ProfileManager.IsGameVersion(ProfileManager.EGame.FIFA23)
				&& (pathSpan.Contains("FolderAsset", StringComparison.OrdinalIgnoreCase) || pathSpan.Contains("StoryAsset", StringComparison.OrdinalIgnoreCase))
				)
			{
				//using (var ms = AssetManager.Instance.GetCustomAsset("legacy", this) as MemoryStream)
				//{
				//	var data = ms.ToArray();
				//                var ros = new ReadOnlySpan<char>(Encoding.UTF8.GetString(data).ToCharArray());
				//	if (ros.Contains("<FolderAsset", StringComparison.OrdinalIgnoreCase))
				//	{
				//		var indexOfName = ros.IndexOf("name=");
				//		var indexOfEnd = ros.IndexOf("deleted=");
				//		if (indexOfEnd == -1)
				//			return null;

				//                    if (indexOfEnd - indexOfName < 1)
				//                        return null;

				//                    var nameAttribute = ros.Slice(indexOfName, indexOfEnd - indexOfName);// xml.LastChild.Attributes["name"];
				//		result = $"[XML Folder]({nameAttribute.Slice(5, nameAttribute.Length - 6)})";
				//	}

				//	if (ros.Contains("<StoryAsset", StringComparison.OrdinalIgnoreCase))
				//	{
				//		//var nameAttribute = ros.Slice(ros.IndexOf("name="), 100);// xml.LastChild.Attributes["name"];
				//		//result = $"[XML Story]({nameAttribute.Slice(5, nameAttribute.LastIndexOf('"'))})";
				//	}
				//}
			}
			return result;
        }

        //public override bool IsModified
        //{
        //	get
        //	{
        //		//if (CollectorInstances.Count == 0)
        //		//{
        //		//	return false;
        //		//}
        //		//return CollectorInstances[0].IsModified;

        //		return ChunkId != Guid.Empty && AssetManager.Instance.GetChunkEntry(ChunkId) != null && AssetManager.Instance.GetChunkEntry(ChunkId).IsModified;
        //	}
        //}
        public override bool IsModified => ModifiedEntry != null && ModifiedEntry.Data != null;

		//private ModifiedLegacyAssetEntry ModifiedLegacyAsset { get; set; }

		//public override IModifiedAssetEntry ModifiedEntry
		//{
		//	get
		//	{
		//		return ModifiedLegacyAsset;
		//	}
		//	set
		//	{
		//		ModifiedLegacyAsset = (ModifiedLegacyAssetEntry)value;
		//	}
		//}

		private long _size;

		public new long Size
		{
			get
			{
                return _size;
			}
			set
			{
				_size = value;
			}
		}

		public ChunkAssetEntry ChunkEntry
        {
            get
            {
				return AssetManager.Instance.GetChunkEntry(ChunkId);
			}
        }

		public override bool IsDirty
		{
			get
			{
				if (ChunkEntry != null) 
				{
					if (ChunkEntry.IsDirty)
					{
						return true;
					}
				}
                return false;
            }
		}

        //public long BatchOffset { get; internal set; }
        public long ChunkIdPosition { get; set; }

		public long FileNameInBatchOffset { get; set; }

        public EbxAssetEntry EbxAssetEntry { get; set; }

        public override void ClearModifications()
		{
			foreach (ChunkCollectorInstance collectorInstance in CollectorInstances)
			{
				collectorInstance.ModifiedEntry = null;
			}
		}

        public override string ToString()
        {
			//if(!string.IsNullOrEmpty(DisplayName))
   //         {
			//	return DisplayName;
   //         }
            return base.ToString();
        }

		public new bool IsLegacy = true;
    }
}
