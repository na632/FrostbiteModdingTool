using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace FIFA21Plugin.Plugin2
{
	public class NewTocReader_F21
	{
		private const int SizeOfTocHeader = 556;

		public const int Magic = 60;

		public TocFile_F21 Read(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			NativeReader reader = new NativeReader(stream);
			reader.ReadUInt32LittleEndian();
			reader.ReadUInt32LittleEndian();
			byte[] xorKey = reader.ReadBytes(256);
			reader.Position += 292L;
			int bundleFlagsOffset = reader.ReadInt32BigEndian();
			if (bundleFlagsOffset != 60)
			{
				throw new InvalidDataException($"Invalid header magic in TOC file. Expected \"{60}\" but got \"{bundleFlagsOffset}\"");
			}
			int bundleDataOffset = reader.ReadInt32BigEndian();
			int bundleCount = reader.ReadInt32BigEndian();
			int chunkFlagsOffset = reader.ReadInt32BigEndian();
			int chunkGuidOffset = reader.ReadInt32BigEndian();
			int chunkCount = reader.ReadInt32BigEndian();
			int chunkEntryOffset = reader.ReadInt32BigEndian();
			int unk4 = reader.ReadInt32BigEndian();
			int unk7Offset = reader.ReadInt32BigEndian();
			int unk5 = reader.ReadInt32BigEndian();
			int unk6 = reader.ReadInt32BigEndian();
			int unk2 = reader.ReadInt32BigEndian();
			int unk11Count = reader.ReadInt32BigEndian();
			int unk12Count = reader.ReadInt32BigEndian();
			int unk13Offset = reader.ReadInt32BigEndian();
			List<int> bundleFlags = new List<int>(bundleCount);
			for (int i3 = 0; i3 < bundleCount; i3++)
			{
				bundleFlags.Add(reader.ReadInt32BigEndian());
			}
			reader.Pad(8);
			if (reader.Position != 556 + bundleDataOffset)
			{
				reader.Position = 556 + bundleDataOffset;
			}
			List<(int, int, long)> bundleData = new List<(int, int, long)>(bundleCount);
			for (int i2 = 0; i2 < bundleCount; i2++)
			{
				int unk3 = reader.ReadInt32BigEndian();
				int bundleLength = reader.ReadInt32BigEndian();
				long bundleOffset = reader.ReadInt64BigEndian();
				bundleData.Add((unk3, bundleLength, bundleOffset));
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
			List<(Guid, int)> chunkGuids = new List<(Guid, int)>();
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
					int index = reader.ReadInt32BigEndian();
					chunkGuids.Add((guid, index));
				}
			}
			List<(byte, bool, byte, byte, uint, uint)> chunks = new List<(byte, bool, byte, byte, uint, uint)>();
			if (reader.Position != 556 + chunkEntryOffset)
			{
				reader.Position = 556 + chunkEntryOffset;
			}
			for (int l = 0; l < chunkCount; l++)
			{
				byte unk = reader.ReadByte();
				bool patch = reader.ReadBoolean();
				byte catalog = reader.ReadByte();
				byte cas = reader.ReadByte();
				uint offset = reader.ReadUInt32BigEndian();
				uint size = reader.ReadUInt32BigEndian();
				chunks.Add((unk, patch, catalog, cas, offset, size));
			}
			List<int> offset2Values = new List<int>(unk11Count);
			if (reader.Position != 556 + unk7Offset)
			{
				reader.Position = 556 + unk7Offset;
			}
			for (int k = 0; k < unk11Count; k++)
			{
				offset2Values.Add(reader.ReadInt32BigEndian());
			}
			List<int> offset8Values = new List<int>(unk12Count);
			if (reader.Position != 556 + unk13Offset)
			{
				reader.Position = 556 + unk13Offset;
			}
			for (int j = 0; j < unk12Count; j++)
			{
				offset8Values.Add(reader.ReadInt32BigEndian());
			}
			List<(int, int, bool, int, long, List<(bool, int, int, bool, int, int)>)> casBundles = new List<(int, int, bool, int, long, List<(bool, int, int, bool, int, int)>)>();
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
					int num = reader.ReadInt32BigEndian();
					int e = reader.ReadInt32BigEndian();
					int f = reader.ReadInt32BigEndian();
					bool patch2 = false;
					byte catalog2 = 0;
					byte cas2 = 0;
					if (num == c)
					{
					}
					reader.Position = startPosition + flagsOffset;
					byte[] flags = reader.ReadBytes(entriesCount);
					reader.Position = startPosition + c;
					List<(bool, int, int, bool, int, int)> entries = new List<(bool, int, int, bool, int, int)>(entriesCount - 1);
					for (int j2 = 0; j2 < entriesCount; j2++)
					{
						bool hasCasIdentifier = flags[j2] == 1;
						if (hasCasIdentifier)
						{
							reader.ReadByte();
							patch2 = reader.ReadBoolean();
							catalog2 = reader.ReadByte();
							cas2 = reader.ReadByte();
						}
						int bundleOffsetInCas = reader.ReadInt32BigEndian();
						int bundleSizeInCas = reader.ReadInt32BigEndian();
						if (j2 == 0)
						{
							casBundles.Add((catalog2, cas2, patch2, bundleSizeInCas, bundleOffsetInCas, entries));
						}
						else
						{
							entries.Add((hasCasIdentifier, catalog2, cas2, patch2, bundleOffsetInCas, bundleSizeInCas));
						}
					}
					reader.Position = startPosition + flagsOffset + entriesCount;
				}
			}
			return new TocFile_F21(xorKey, bundleFlagsOffset, bundleDataOffset, bundleCount, chunkFlagsOffset, chunkGuidOffset, chunkCount, chunkEntryOffset, unk4, unk7Offset, unk5, unk6, unk2, unk11Count, unk12Count, unk13Offset, bundleFlags, chunkFlags, bundleData, chunkGuids, chunks, offset2Values, offset8Values, casBundles);
		}
	}
}
