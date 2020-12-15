using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FIFA21Plugin.Plugin2.Handlers;
using Frosty.Hash;
using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;

namespace FIFA21Plugin.Plugin2
{
    public class LegacyCustomActionHandler : ICustomBundleActionHandler
	{
		internal class LegacyFileEntry
		{
			public int Hash
			{
				get;
				set;
			}

			public Guid OriginalChunkGuid
			{
				get;
				set;
			}

			public Guid ChunkId
			{
				get;
				set;
			}

			public long Offset
			{
				get;
				set;
			}

			public long CompressedOffset
			{
				get;
				set;
			}

			public long CompressedSize
			{
				get;
				set;
			}

			public long Size
			{
				get;
				set;
			}
		}


		public LegacyCustomActionHandler()
		{
		}

		public object Load(object existing, byte[] newData)
		{
			if (newData == null)
			{
				throw new ArgumentNullException("newData");
			}
			List<LegacyFileEntry> legacyEntries = (existing ?? new List<LegacyFileEntry>()) as List<LegacyFileEntry>;
			if (legacyEntries == null)
			{
				throw new ArgumentException("existing must be a List of LegacyFileEntry.", "existing");
			}
			NativeReader nativeReader = new NativeReader(new MemoryStream(newData));
			while (nativeReader.Position < nativeReader.Length)
			{
				int hash = nativeReader.ReadInt32LittleEndian();
				int num = legacyEntries.FindIndex((LegacyFileEntry a) => a.Hash == hash);
				if (num != -1)
				{
					legacyEntries.RemoveAt(num);
				}
				LegacyFileEntry legacyFileEntry = new LegacyFileEntry
				{
					Hash = hash,
					OriginalChunkGuid = nativeReader.ReadGuid() ,
					ChunkId = nativeReader.ReadGuid(),
					Offset = nativeReader.ReadInt64LittleEndian(),
					CompressedOffset = nativeReader.ReadInt64LittleEndian(),
					CompressedSize = nativeReader.ReadInt64LittleEndian(),
					Size = nativeReader.ReadInt64LittleEndian()
				};
				legacyEntries.Add(legacyFileEntry);
			}
			return legacyEntries;
		}

		public AssetEntry Modify(AssetEntry origEntry, Stream baseStream, object data, out byte[] outData)
		{
			if (baseStream == null)
			{
				throw new ArgumentNullException("baseStream");
			}
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			List<LegacyFileEntry> list = data as List<LegacyFileEntry>;
			if (list == null)
			{
				throw new ArgumentException("data must be a List of LegacyFileEntry.", "data");
			}
			NativeReader nativeReader = new NativeReader(baseStream);
			NativeWriter nativeWriter = new NativeWriter(new MemoryStream());
			
				uint count1 = nativeReader.ReadUInt32LittleEndian();
				nativeWriter.WriteUInt32LittleEndian(count1);
				long offset1 = nativeReader.ReadInt64LittleEndian();
				nativeWriter.WriteInt64LittleEndian(offset1);
			int fileEntriesCount = nativeReader.ReadInt32LittleEndian();
			nativeWriter.WriteInt32LittleEndian(fileEntriesCount);
			long fileEntriesOffset = nativeReader.ReadInt64LittleEndian();
			nativeWriter.WriteInt64LittleEndian(fileEntriesOffset);
			uint chunkCount = 0u;
			long chunkOffset = 0L;
				uint count2 = nativeReader.ReadUInt32LittleEndian();
				nativeWriter.WriteUInt32LittleEndian(count2);
				long offset2 = nativeReader.ReadInt64LittleEndian();
				nativeWriter.WriteInt64LittleEndian(offset2);
				chunkCount = nativeReader.ReadUInt32LittleEndian();
				nativeWriter.WriteUInt32LittleEndian(chunkCount);
				chunkOffset = nativeReader.ReadInt64LittleEndian();
				nativeWriter.WriteInt64LittleEndian(chunkOffset);
			int remainingHeaderLength = (int)(fileEntriesOffset - nativeReader.Position);
			nativeWriter.WriteBytes(nativeReader.ReadBytes(remainingHeaderLength));
			for (int j = 0; j < fileEntriesCount; j++)
			{
				long stringOffset = nativeReader.ReadInt64LittleEndian();
				long position = nativeReader.Position;
				nativeReader.Position = stringOffset;
				string name = nativeReader.ReadNullTerminatedString();
				int nameHash = Fnv1.HashString(name);
				nativeReader.Position = position;
				int fileEntryIndex = list.FindIndex((LegacyFileEntry a) => a.Hash == nameHash);
				if (fileEntryIndex != -1)
				{
					LegacyFileEntry legacyFileEntry = list[fileEntryIndex];
					nativeWriter.WriteInt64LittleEndian(stringOffset);
					nativeWriter.WriteInt64LittleEndian(legacyFileEntry.CompressedOffset);
					nativeWriter.WriteInt64LittleEndian(legacyFileEntry.CompressedSize);
					nativeWriter.WriteInt64LittleEndian(legacyFileEntry.Offset);
					nativeWriter.WriteInt64LittleEndian(legacyFileEntry.Size);
					nativeWriter.WriteGuid(legacyFileEntry.ChunkId);
					nativeReader.Position += 48L;
				}
				else
				{
					nativeWriter.WriteInt64LittleEndian(stringOffset);
					nativeWriter.WriteBytes(nativeReader.ReadBytes(48));
				}
			}
				long bytesToSkip = chunkOffset - nativeReader.Position;
				if (bytesToSkip > 0)
				{
				nativeWriter.Position = bytesToSkip;
				nativeReader.BaseStream.CopyTo(nativeWriter.BaseStream);
					//nativeReader.BaseStream.CopyTo(nativeWriter.BaseStream, bytesToSkip);
			}
				_ = nativeReader.Position;
				for (int i = 0; i < chunkCount; i++)
				{
					long chunkDecompressedSize = nativeReader.ReadInt64LittleEndian();
					Guid chunkGuid = nativeReader.ReadGuid();
					long newDecompressedSize = list.Where((LegacyFileEntry lfe) => lfe.ChunkId == chunkGuid).Max((Func<LegacyFileEntry, long?>)((LegacyFileEntry lfe) => lfe.Offset + lfe.Size)) ?? chunkDecompressedSize;
					nativeWriter.WriteInt64LittleEndian(newDecompressedSize);
					nativeWriter.WriteGuid(chunkGuid);
				}
			nativeReader.BaseStream.CopyTo(nativeWriter.BaseStream);
			nativeWriter.BaseStream.Position = 0L;
			NativeReader memOfWriter = new NativeReader(new MemoryStream());
			nativeWriter.BaseStream.CopyTo(memOfWriter.BaseStream);

			//outData = Utils.CompressFile(nativeWriter.BaseStream);
			outData = Utils.CompressFile(nativeReader.ReadToEnd());
			return new ChunkAssetEntry
			{
				Sha1 = Sha1.Create(outData),
				Size = outData.Length,
				IsTocChunk = true
			};
		}
	}
}
