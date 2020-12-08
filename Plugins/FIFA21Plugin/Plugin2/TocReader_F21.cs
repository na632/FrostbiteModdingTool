using System;
using System.Collections.Generic;
using System.IO;
using FrostySdk.IO;
using FrostySdk.Managers;

namespace FIFA21Plugin.Plugin2
{
	public class TocReader_F21
	{
		private const int SizeOfTocHeader = 556;

		public const int Magic = 60;

		public (List<ChunkAssetEntry> chunks, (int unk1, int bundleLength, long bundleOffset)[] bundles, List<(string casFile, int casCatalog, int casIndex, bool inPatch, int bundleLength, long bundleOffset, List<(string casFile, int casCatalog, int casIndex, bool inPatch, int entryOffset, int entrySize)>)> casBundles) Read(Stream stream, Func<int, int, bool, string> casLookup)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead)
			{
				throw new ArgumentException("Stream must support reading.", "stream");
			}
			if (!stream.CanSeek)
			{
				throw new ArgumentException("Stream must support seeking.", "stream");
			}
			if (casLookup == null)
			{
				throw new ArgumentNullException("casLookup");
			}
			NativeReader reader = new NativeReader(stream);
			stream.Position = 556L;
			int magic = reader.ReadInt32BigEndian();
			if (magic != 60)
			{
				throw new InvalidDataException($"Invalid header magic in TOC file. Expected \"{60}\" but got \"{magic}\"");
			}
			int bundleDataOffset = reader.ReadInt32BigEndian();
			int bundleCount = reader.ReadInt32BigEndian();
			int chunkFlagsOffset = reader.ReadInt32BigEndian();
			int chunkGuidOffset = reader.ReadInt32BigEndian();
			int chunkCount = reader.ReadInt32BigEndian();
			int chunkEntryOffset = reader.ReadInt32BigEndian();
			reader.ReadInt32BigEndian();
			int unk7Offset = reader.ReadInt32BigEndian();
			reader.ReadInt32BigEndian();
			reader.ReadInt32BigEndian();
			reader.ReadInt32BigEndian();
			int unk11Count = reader.ReadInt32BigEndian();
			int unk12Count = reader.ReadInt32BigEndian();
			int unk13Offset = reader.ReadInt32BigEndian();
			int sizeOfUnkBlock = (bundleDataOffset - magic) / 4;
			int[] unk14 = new int[sizeOfUnkBlock];
			for (int i3 = 0; i3 < sizeOfUnkBlock; i3++)
			{
				unk14[i3] = reader.ReadInt32BigEndian();
			}
			if (reader.Position != 556 + bundleDataOffset)
			{
				reader.Position = 556 + bundleDataOffset;
			}
			(int, int, long)[] bundleData = new(int, int, long)[bundleCount];
			for (int i2 = 0; i2 < bundleCount; i2++)
			{
				int unk15 = reader.ReadInt32BigEndian();
				int bundleLength = reader.ReadInt32BigEndian();
				long bundleOffset = reader.ReadInt64BigEndian();
				bundleData[i2] = (unk15, bundleLength, bundleOffset);
			}
			List<int> chunkFlags = new List<int>();
			if (chunkFlagsOffset != 0)
			{
				if (reader.Position != 556 + chunkFlagsOffset)
				{
					reader.Position = 556 + chunkFlagsOffset;
				}
				for (int n = 0; n < chunkCount; n++)
				{
					int value = reader.ReadInt32BigEndian();
					chunkFlags.Add(value);
				}
			}
			List<ChunkAssetEntry> chunkData = new List<ChunkAssetEntry>();
			if (chunkCount > 0)
			{
				if (reader.Position != 556 + chunkGuidOffset)
				{
					reader.Position = 556 + chunkGuidOffset;
				}
				for (int m = 0; m < chunkCount; m++)
				{
					byte[] numArray2 = reader.ReadBytes(16);
					Guid guid = new Guid(new byte[16]
					{
						numArray2[15],
						numArray2[14],
						numArray2[13],
						numArray2[12],
						numArray2[11],
						numArray2[10],
						numArray2[9],
						numArray2[8],
						numArray2[7],
						numArray2[6],
						numArray2[5],
						numArray2[4],
						numArray2[3],
						numArray2[2],
						numArray2[1],
						numArray2[0]
					});
					int order = reader.ReadInt32BigEndian() & 0xFFFFFF;
					while (chunkData.Count <= order / 3)
					{
						chunkData.Add(null);
					}
					chunkData[order / 3] = new ChunkAssetEntry
					{
						Id = guid
					};
				}
			}
			if (reader.Position != 556 + chunkEntryOffset)
			{
				reader.Position = 556 + chunkEntryOffset;
			}
			for (int l = 0; l < chunkCount; l++)
			{
				reader.ReadByte();
				bool patch = reader.ReadBoolean();
				byte catalog = reader.ReadByte();
				byte cas = reader.ReadByte();
				uint offset = reader.ReadUInt32BigEndian();
				uint size = reader.ReadUInt32BigEndian();
				ChunkAssetEntry chunkAssetEntry = chunkData[l];
				chunkAssetEntry.Size = size;
				chunkAssetEntry.Location = AssetDataLocation.CasNonIndexed;
				chunkAssetEntry.ExtraData = new AssetExtraData
				{
					CasPath = casLookup(catalog, cas, patch),
					DataOffset = offset
				};
				chunkAssetEntry.IsTocChunk = true;
			}
			int[] unk7Values = new int[unk11Count];
			if (reader.Position != 556 + unk7Offset)
			{
				reader.Position = 556 + unk7Offset;
			}
			for (int k = 0; k < unk11Count; k++)
			{
				unk7Values[k] = reader.ReadInt32BigEndian();
			}
			int[] unk13Values = new int[unk12Count];
			if (reader.Position != 556 + unk13Offset)
			{
				reader.Position = 556 + unk13Offset;
			}
			for (int j = 0; j < unk12Count; j++)
			{
				unk13Values[j] = reader.ReadInt32BigEndian();
			}
			List<(string, int, int, bool, int, long, List<(string, int, int, bool, int, int)>)> casBundles = new List<(string, int, int, bool, int, long, List<(string, int, int, bool, int, int)>)>();
			if (reader.Position != reader.Length)
			{
				for (int i = 0; i < bundleCount; i++)
				{
					long startPosition = reader.Position;
					reader.ReadInt32BigEndian();
					reader.ReadInt32BigEndian();
					int flagsOffset = reader.ReadInt32BigEndian();
					int entriesCount = reader.ReadInt32BigEndian();
					int c = reader.ReadInt32BigEndian();
					reader.ReadInt32BigEndian();
					reader.ReadInt32BigEndian();
					reader.ReadInt32BigEndian();
					bool patch2 = false;
					byte catalog2 = 0;
					byte cas2 = 0;
					string casFile = null;
					reader.Position = startPosition + flagsOffset;
					byte[] flags = reader.ReadBytes(entriesCount);
					reader.Position = startPosition + c;
					List<(string, int, int, bool, int, int)> entries = new List<(string, int, int, bool, int, int)>(entriesCount - 1);
					for (int j2 = 0; j2 < entriesCount; j2++)
					{
						if (flags[j2] == 1)
						{
							reader.ReadByte();
							patch2 = reader.ReadBoolean();
							catalog2 = reader.ReadByte();
							cas2 = reader.ReadByte();
							casFile = casLookup(catalog2, cas2, patch2);
						}
						int bundleOffsetInCas = reader.ReadInt32BigEndian();
						int bundleSizeInCas = reader.ReadInt32BigEndian();
						if (j2 == 0)
						{
							casBundles.Add((casFile, catalog2, cas2, patch2, bundleSizeInCas, bundleOffsetInCas, entries));
						}
						else
						{
							entries.Add((casFile, catalog2, cas2, patch2, bundleOffsetInCas, bundleSizeInCas));
						}
					}
					reader.Position = startPosition + flagsOffset + entriesCount;
				}
			}
			return (chunkData, bundleData, casBundles);
		}
	}
}
