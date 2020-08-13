using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace FrostySdk
{
	public class FrostyModReader : NativeReader
	{
		private class EmbeddedResource : BaseModResource
		{
			public override ModResourceType Type => ModResourceType.Embedded;
		}

		private class EbxResource : BaseModResource
		{
			public override ModResourceType Type => ModResourceType.Ebx;
		}

		private class ResResource : BaseModResource
		{
			private uint resType;

			private ulong resRid;

			private byte[] resMeta;

			public override ModResourceType Type => ModResourceType.Res;

			public override void Read(NativeReader reader)
			{
				base.Read(reader);
				resType = reader.ReadUInt();
				resRid = reader.ReadULong();
				resMeta = reader.ReadBytes(reader.ReadInt());
				foreach (ResCustomHandlerAttribute customAttribute in Assembly.GetExecutingAssembly().GetCustomAttributes<ResCustomHandlerAttribute>())
				{
					if (customAttribute.ResType == (ResourceType)resType && handlerHash == 0)
					{
					}
				}
			}

			public override void FillAssetEntry(AssetEntry entry)
			{
				base.FillAssetEntry(entry);
				ResAssetEntry obj = entry as ResAssetEntry;
				obj.ResType = resType;
				obj.ResRid = resRid;
				obj.ResMeta = resMeta;
			}
		}

		private class ChunkResource : BaseModResource
		{
			private uint rangeStart;

			private uint rangeEnd;

			private uint logicalOffset;

			private uint logicalSize;

			private int h32;

			private int firstMip;

			public override ModResourceType Type => ModResourceType.Chunk;

			public override void Read(NativeReader reader)
			{
				base.Read(reader);
				rangeStart = reader.ReadUInt();
				rangeEnd = reader.ReadUInt();
				logicalOffset = reader.ReadUInt();
				logicalSize = reader.ReadUInt();
				h32 = reader.ReadInt();
				firstMip = reader.ReadInt();
			}

			public override void FillAssetEntry(AssetEntry entry)
			{
				base.FillAssetEntry(entry);
				ChunkAssetEntry chunkAssetEntry = entry as ChunkAssetEntry;
				chunkAssetEntry.Id = new Guid(name);
				chunkAssetEntry.RangeStart = rangeStart;
				chunkAssetEntry.RangeEnd = rangeEnd;
				chunkAssetEntry.LogicalOffset = logicalOffset;
				chunkAssetEntry.LogicalSize = logicalSize;
				chunkAssetEntry.H32 = h32;
				chunkAssetEntry.FirstMip = firstMip;
				chunkAssetEntry.IsTocChunk = base.IsTocChunk;
				if (chunkAssetEntry.FirstMip == -1 && chunkAssetEntry.RangeStart != 0)
				{
					chunkAssetEntry.FirstMip = 0;
				}
			}
		}

		private bool isValid;

		private long dataOffset;

		private int dataCount;

		private int gameVersion;

		private uint version;

		public bool IsValid => isValid;

		public int GameVersion => gameVersion;

		public uint Version => version;

		public FrostyModReader(Stream inStream)
			: base(inStream)
		{
			if (ReadULong() != FrostyMod.Magic)
			{
				return;
			}
			version = ReadUInt();
			if (version <= FrostyMod.Version)
			{
				dataOffset = ReadLong();
				dataCount = ReadInt();
				var pn = ReadSizedString(ReadByte());
				Debug.WriteLine("FrostyModReader::Mod ProfileName::" + pn);
				if (pn == ProfilesLibrary.ProfileName)
				{
					gameVersion = ReadInt();
					isValid = true;
					Debug.WriteLine("FrostyModReader::Mod Game Version::" + gameVersion);
				}
				else
                {
					Debug.WriteLine("FrostyModReader::Cannot match profile " + pn + " to " + ProfilesLibrary.ProfileName);
                }
			}
		}

		public FrostyModDetails ReadModDetails()
		{
			return new FrostyModDetails(ReadNullTerminatedString(), ReadNullTerminatedString(), ReadNullTerminatedString(), ReadNullTerminatedString(), ReadNullTerminatedString());
		}

		public BaseModResource[] ReadResources()
		{
			int num = ReadInt();
			BaseModResource[] array = new BaseModResource[num];
			for (int i = 0; i < num; i++)
			{
				switch (ReadByte())
				{
				case 0:
					array[i] = new EmbeddedResource();
					break;
				case 1:
					array[i] = new EbxResource();
					break;
				case 2:
					array[i] = new ResResource();
					break;
				case 3:
					array[i] = new ChunkResource();
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
			long num = ReadLong();
			long num2 = ReadLong();
			Position = dataOffset + dataCount * 16 + num;
			return ReadBytes((int)num2);
		}
	}
}
