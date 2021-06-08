using FrostbiteSdk.Frosty.Abstract;
using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace FrostySdk
{
	public class FrostbiteModReader : BaseModReader
	{
		public class BundleResource : BaseModResource
		{
			private int superBundleName;

			public override ModResourceType Type => ModResourceType.Bundle;

			public override void Read(NativeReader reader)
			{
				base.Read(reader);
				name = reader.ReadNullTerminatedString();
				superBundleName = reader.ReadInt32LittleEndian();
			}

			public void FillAssetEntry(object entry)
			{
				BundleEntry obj = (BundleEntry)entry;
				obj.Name = name;
				obj.SuperBundleId = superBundleName;
			}
		}


		//public class EbxResource : BaseModResource
		//{
		//	public override ModResourceType Type => ModResourceType.Ebx;
		//}

		//public class ResResource : BaseModResource
		//{
		//	private uint resType;

		//	private ulong resRid;

		//	private byte[] resMeta;

		//	public override ModResourceType Type => ModResourceType.Res;

		//	public override void Read(NativeReader reader)
		//	{
		//		base.Read(reader);
		//		resType = reader.ReadUInt();
		//		var rType = (ResourceType)resType;
		//		resRid = reader.ReadULong();
		//		resMeta = reader.ReadBytes(reader.ReadInt());
		//		foreach (ResCustomHandlerAttribute customAttribute in Assembly.GetExecutingAssembly().GetCustomAttributes<ResCustomHandlerAttribute>())
		//		{
		//			if (customAttribute.ResType == (ResourceType)resType && handlerHash == 0)
		//			{
		//			}
		//		}
		//	}

		//	public override void FillAssetEntry(AssetEntry entry)
		//	{
		//		base.FillAssetEntry(entry);
		//		ResAssetEntry obj = entry as ResAssetEntry;
		//		obj.ResType = resType;
		//		obj.ResRid = resRid;
		//		obj.ResMeta = resMeta;
		//	}
		//}

		//public class ChunkResource : BaseModResource
		//{
		//	private uint rangeStart;

		//	private uint rangeEnd;

		//	private uint logicalOffset;

		//	private uint logicalSize;

		//	private int h32;

		//	private int firstMip;

		//	public override ModResourceType Type => ModResourceType.Chunk;

		//	public override void Read(NativeReader reader)
		//	{
		//		base.Read(reader);
		//		rangeStart = reader.ReadUInt();
		//		rangeEnd = reader.ReadUInt();
		//		logicalOffset = reader.ReadUInt();
		//		logicalSize = reader.ReadUInt();
		//		h32 = reader.ReadInt();
		//		firstMip = reader.ReadInt();
		//	}

		//	public override void FillAssetEntry(AssetEntry entry)
		//	{
		//		base.FillAssetEntry(entry);
		//		ChunkAssetEntry chunkAssetEntry = entry as ChunkAssetEntry;
		//		chunkAssetEntry.Id = new Guid(name);
		//		chunkAssetEntry.RangeStart = rangeStart;
		//		chunkAssetEntry.RangeEnd = rangeEnd;
		//		chunkAssetEntry.LogicalOffset = logicalOffset;
		//		chunkAssetEntry.LogicalSize = logicalSize;
		//		chunkAssetEntry.H32 = h32;
		//		chunkAssetEntry.FirstMip = firstMip;
		//		chunkAssetEntry.IsTocChunk = base.IsTocChunk;
		//		if (chunkAssetEntry.FirstMip == -1 && chunkAssetEntry.RangeStart != 0)
		//		{
		//			chunkAssetEntry.FirstMip = 0;
		//		}
		//	}
		//}

		private bool isValid;

		public long dataOffset;

		public int dataCount;

		private int gameVersion;

		private uint version;

		public bool IsValid => isValid;

		public int GameVersion => gameVersion;

		public uint Version => version;

		public bool IsFETMod => Version > 3;

		public FrostbiteModReader(Stream inStream)
			: base(inStream)
		{
			var viewableBytes = new NativeReader(inStream).ReadToEnd();
			inStream.Position = 0;
			if (ReadULong() != FrostbiteMod.Magic)
			{
				return;
			}
			version = ReadUInt();
			//if (version <= FrostbiteMod.Version)
			{
				dataOffset = ReadLong();
				dataCount = ReadInt();
				//var pn = ReadSizedString(ReadByte());
				var pn = ReadLengthPrefixedString();
				//Debug.WriteLine("FrostyModReader::Mod ProfileName::" + pn);
				if (pn == ProfilesLibrary.ProfileName)
				{
					gameVersion = ReadInt();
					isValid = true;
					//Debug.WriteLine("FrostyModReader::Mod Game Version::" + gameVersion);
				}
				else
                {
					throw new Exception("FrostyModReader::Cannot match profile " + pn + " to " + ProfilesLibrary.ProfileName);
                }
			}
		}

		public FrostbiteModDetails ReadModDetails()
		{
			if (Version >= 5)
            {
				return new FrostbiteModDetails(ReadNullTerminatedString(), ReadNullTerminatedString(), ReadNullTerminatedString(), ReadNullTerminatedString(), ReadNullTerminatedString(), ReadInt());
			}
			else 
			{
				return new FrostbiteModDetails(ReadNullTerminatedString(), ReadNullTerminatedString(), ReadNullTerminatedString(), ReadNullTerminatedString(), ReadNullTerminatedString());
			}
		}

		public BaseModResource[] ReadResources()
		{
			int num = ReadInt();
			BaseModResource[] array = new BaseModResource[num];
			for (int i = 0; i < num; i++)
			{
				switch ((ModResourceType)ReadByte())
				{
				case ModResourceType.Embedded:
					array[i] = new EmbeddedResource();
					break;
				case ModResourceType.Ebx:
					array[i] = new EbxResource();
					break;
				case ModResourceType.Res:
					array[i] = new ResResource();
					break;
				case ModResourceType.Chunk:
					array[i] = new ChunkResource();
					break;
				case ModResourceType.Legacy:
					array[i] = new LegacyFileResource();
					break;
				case ModResourceType.EmbeddedFile:
					array[i] = new EmbeddedFileResource();
					break;
				}
				array[i].Read(this);
			}
			return array;
		}

		public byte[] GetResourceData(BaseModResource resource)
		{
			if (resource.ResourceIndex == -1)
			{
				return null;
			}

			Position = dataOffset + resource.ResourceIndex * 16;
			long offset = ReadLong();
			long size = ReadLong();

			Position = dataOffset + dataCount * 16 + offset;
			var data = ReadBytes((int)size);
		 
			//if(resource is ResResource)
   //         {
			//	using (MemoryStream memoryStream = new MemoryStream(data))
			//	{
			//		ResAssetEntry resAssetEntry = new ResAssetEntry();
			//		resource.FillAssetEntry(resAssetEntry);
			//	}
   //         }

			return data;
		}
	}
}
