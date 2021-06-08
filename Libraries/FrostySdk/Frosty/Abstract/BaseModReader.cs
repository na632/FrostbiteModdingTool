using FrostbiteSdk.FrostbiteSdk.Managers;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrostbiteSdk.Frosty.Abstract
{

	public abstract class BaseModReader : NativeReader
	{
		public BaseModReader(Stream stream)
			: base(stream, skipObfuscation: false)
		{
		}

		public class EmbeddedResource : BaseModResource
		{
			public override ModResourceType Type => ModResourceType.Embedded;
		}

		public class EbxResource : BaseModResource
		{
			public override ModResourceType Type => ModResourceType.Ebx;
		}

		public class ResResource : BaseModResource
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
				resMeta = reader.ReadBytes(reader.ReadInt32LittleEndian());
			}

			public override void FillAssetEntry(object entry)
			{
				base.FillAssetEntry(entry);
				ResAssetEntry obj = (ResAssetEntry)entry;
				obj.ResType = resType;
				obj.ResRid = resRid;
				obj.ResMeta = resMeta;
			}
		}

		public class ChunkResource : BaseModResource
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
				rangeStart = reader.ReadUInt32LittleEndian();
				rangeEnd = reader.ReadUInt32LittleEndian();
				logicalOffset = reader.ReadUInt32LittleEndian();
				logicalSize = reader.ReadUInt32LittleEndian();
				h32 = reader.ReadInt32LittleEndian();
				firstMip = reader.ReadInt32LittleEndian();
			}

			public override void FillAssetEntry(object entry)
			{
				base.FillAssetEntry(entry);
				ChunkAssetEntry chunkAssetEntry = (ChunkAssetEntry)entry;
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

		public class LegacyFileResource : BaseModResource
		{
			private string name;

			public override ModResourceType Type => ModResourceType.Legacy;

			public override void Read(NativeReader reader)
			{
				base.Read(reader);
				name = reader.ReadLengthPrefixedString();
			}

			public override void FillAssetEntry(object entry)
			{
				base.FillAssetEntry(entry);
				LegacyFileEntry legAssetEntry = (LegacyFileEntry)entry;
				legAssetEntry.Name = name;
			}
		}

		public class EmbeddedFileResource : BaseModResource
		{
			private string name;

			private string exportedLocation;

			public override ModResourceType Type => ModResourceType.EmbeddedFile;

			public override void Read(NativeReader reader)
			{
				base.Read(reader);
				name = reader.ReadLengthPrefixedString();
			}

			public override void FillAssetEntry(object entry)
			{
				base.FillAssetEntry(entry);
				EmbeddedFileEntry assetEntry = (EmbeddedFileEntry)entry;
				assetEntry.Name = name;
				assetEntry.ExportedRelativePath = exportedLocation;
			}
		}
	}

}
