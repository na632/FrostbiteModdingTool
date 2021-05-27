using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System.Collections.Generic;

namespace FrostySdk
{
	public class BaseModResource
	{
		protected int resourceIndex = -1;

		protected string name;

		protected Sha1 sha1;

		protected long size;

		protected byte flags;

		protected int handlerHash;

		protected string userData = "";

		protected List<int> bundlesToModify = new List<int>();

		protected List<int> bundlesToAdd = new List<int>();

		public virtual ModResourceType Type => ModResourceType.Invalid;

		public int ResourceIndex => resourceIndex;

		public string Name => name;

		public Sha1 Sha1 => sha1;

		public long Size => size;

		public int Handler => handlerHash;

		public string UserData => userData;

		public bool IsModified => bundlesToModify.Count != 0;

		public bool IsAdded => bundlesToAdd.Count != 0;

		public bool ShouldInline => (flags & 1) != 0;

		public bool IsTocChunk => (flags & 2) != 0;

		public bool HasHandler => handlerHash != 0;

		public IEnumerable<int> ModifiedBundles => bundlesToModify;

		public IEnumerable<int> AddedBundles => bundlesToAdd;

		public virtual void Read(NativeReader reader)
		{
			resourceIndex = reader.ReadInt();
			if (resourceIndex != -1)
			{
				name = reader.ReadNullTerminatedString();
				sha1 = reader.ReadSha1();
				size = reader.ReadLong();
				flags = reader.ReadByte();
				handlerHash = reader.ReadInt();
				userData = "";
				//if ((reader as FrostyModReader).Version >= 3)
				//{
					userData = reader.ReadNullTerminatedString();
				//}
				int num = reader.ReadInt();
				for (int i = 0; i < num; i++)
				{
					bundlesToModify.Add(reader.ReadInt());
				}
				num = reader.ReadInt();
				for (int j = 0; j < num; j++)
				{
					bundlesToAdd.Add(reader.ReadInt());
				}
			}
		}

		//public virtual void FillAssetEntry(AssetEntry entry)
		//{
		//	entry.Name = name;
		//	entry.Sha1 = sha1;
		//	entry.OriginalSize = size;
		//	entry.IsInline = ShouldInline;
		
		//}

		public virtual void FillAssetEntry(object entry)
		{
			AssetEntry obj = (AssetEntry)entry;
			obj.Name = name;
			obj.Sha1 = sha1;
			obj.OriginalSize = size;
			obj.IsInline = ShouldInline;
		}
	}
}
