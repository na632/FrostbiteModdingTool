using FrostySdk.IO;
using System.Buffers;
using System.IO;
using System.Linq;

namespace FIFA21Plugin.Plugin2
{
	public class TocWriter_F21
	{
		public void Write(string path, TocFile_F21 tocFile, bool writeHeader = false)
		{
			using (FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 65536, FileOptions.SequentialScan))
			{
				Write(stream, tocFile, writeHeader);
			}
		}

		public void Write(Stream stream, TocFile_F21 tocFile, bool writeHeader = false)
		{
			NativeWriter writer = new NativeWriter(stream);
			if (writeHeader)
			{
				writer.WriteInt32BigEndian(13749761);
				writer.WriteUInt32BigEndian(0u);
				writer.WriteBytes(tocFile.XorKey);
				stream.Seek(292L, SeekOrigin.Current);
			}
			long startPosition = stream.Position;
			writer.WriteInt32BigEndian(60);
			long bundleDataOffsetPosition = writer.Position;
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian(tocFile.Bundles.Count);
			long chunkFlagsOffsetPosition = writer.Position;
			writer.WriteInt32BigEndian(0);
			long chunkGuidOffsetPosition = writer.Position;
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian(tocFile.Chunks.Count);
			long chunkEntryOffsetPosition = writer.Position;
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian(0);
			long offset2Position = writer.Position;
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian(tocFile.UnknownValue4);
			writer.WriteInt32BigEndian(tocFile.UnknownValue5);
			writer.WriteInt32BigEndian(tocFile.offset2Values.Count);
			writer.WriteInt32BigEndian(tocFile.offset8Values.Count);
			long offset8Position = writer.Position;
			writer.WriteInt32BigEndian(0);
			foreach (int bundleFlag in tocFile.bundleFlags)
			{
				writer.WriteInt32BigEndian(bundleFlag);
			}
			while ((writer.Position - startPosition) % 8 != 0L)
			{
				writer.Write((byte)0);
			}
			long bundleDataOffset = writer.Position;
			foreach (var bundle in tocFile.Bundles)
			{
				writer.WriteInt32BigEndian(bundle.unk);
				writer.WriteInt32BigEndian(bundle.length);
				writer.WriteInt64BigEndian(bundle.offset);
			}
			long chunkFlagsOffset = writer.Position;
			foreach (int chunkFlag in tocFile.ChunkFlags)
			{
				writer.WriteInt32BigEndian(chunkFlag);
			}
			long chunkGuidOffset = writer.Position;
			foreach (var (chunkGuid, chunkIndex) in tocFile.chunkGuids)
			{
				foreach (byte guidByte in chunkGuid.ToByteArray().Reverse())
				{
					writer.Write(guidByte);
				}
				writer.WriteInt32BigEndian(chunkIndex);
			}
			long chunkEntryOffset = writer.Position;
			foreach (var (unk, patch, catalog, cas, offset, size) in tocFile.Chunks)
			{
				writer.Write(unk);
				writer.Write((sbyte)(patch ? 1 : 0));
				writer.Write(catalog);
				writer.Write(cas);
				writer.WriteUInt32BigEndian(offset);
				writer.WriteUInt32BigEndian(size);
			}
			long offset2 = writer.Position;
			foreach (int offset2Value in tocFile.offset2Values)
			{
				writer.WriteInt32BigEndian(offset2Value);
			}
			long offset3 = writer.Position;
			foreach (int offset8Value in tocFile.offset8Values)
			{
				writer.WriteInt32BigEndian(offset8Value);
			}
			foreach (TocFile_F21.CasBundle casBundle in tocFile.CasBundles)
			{
				byte[] flags = ArrayPool<byte>.Shared.Rent(casBundle.Entries.Count + 1);
				int entryIndex = 0;
				int currentCasIdentifier = CasFile.CreateCasIdentifier(0, casBundle.InPatch, (byte)casBundle.CasCatalog, (byte)casBundle.CasIndex);
				flags[entryIndex] = 1;
				foreach (TocFile_F21.CasBundleEntry entry2 in casBundle.Entries)
				{
					entryIndex++;
					int num = CasFile.CreateCasIdentifier(0, entry2.InPatch, (byte)entry2.CasCatalog, (byte)entry2.CasIndex);
					bool num2 = num != currentCasIdentifier;
					if (num2)
					{
						_ = entry2.HasCasIdentifier;
					}
					if (num2 || entry2.HasCasIdentifier)
					{
						flags[entryIndex] = 1;
					}
					else
					{
						flags[entryIndex] = 0;
					}
					currentCasIdentifier = num;
				}
				long placeholderPosition = writer.Position;
				writer.WriteInt64BigEndian(0L);
				writer.WriteInt32BigEndian(0);
				writer.WriteInt32BigEndian(casBundle.Entries.Count + 1);
				writer.WriteInt32BigEndian(32);
				writer.WriteInt32BigEndian(32);
				writer.WriteInt32BigEndian(32);
				writer.WriteInt32BigEndian(0);
				int casIdentifier = CasFile.CreateCasIdentifier(0, casBundle.InPatch, (byte)casBundle.CasCatalog, (byte)casBundle.CasIndex);
				writer.WriteInt32BigEndian(casIdentifier);
				writer.WriteInt32BigEndian((int)casBundle.BundleOffset);
				writer.WriteInt32BigEndian(casBundle.BundleLength);
				entryIndex = 0;
				foreach (TocFile_F21.CasBundleEntry entry in casBundle.Entries)
				{
					entryIndex++;
					if (flags[entryIndex] == 1)
					{
						casIdentifier = CasFile.CreateCasIdentifier(0, entry.InPatch, (byte)entry.CasCatalog, (byte)entry.CasIndex);
						writer.WriteInt32BigEndian(casIdentifier);
					}
					writer.WriteInt32BigEndian(entry.EntryOffset);
					writer.WriteInt32BigEndian(entry.EntrySize);
				}
				int flagsOffset = (int)(writer.Position - placeholderPosition);
				writer.WriteBytes(flags, 0, casBundle.Entries.Count + 1);
				long endPosition = writer.Position;
				writer.Position = placeholderPosition;
				writer.WriteInt64BigEndian(0L);
				writer.WriteInt32BigEndian(flagsOffset);
				writer.WriteInt32BigEndian(casBundle.Entries.Count + 1);
				writer.WriteInt32BigEndian(32);
				writer.WriteInt32BigEndian(32);
				writer.WriteInt32BigEndian(32);
				writer.Position = endPosition;
				ArrayPool<byte>.Shared.Return(flags);
			}
			writer.Position = bundleDataOffsetPosition;
			writer.WriteInt32BigEndian((int)(bundleDataOffset - startPosition));
			writer.Position = chunkFlagsOffsetPosition;
			writer.WriteInt32BigEndian((int)(chunkFlagsOffset - startPosition));
			writer.Position = chunkGuidOffsetPosition;
			writer.WriteInt32BigEndian((int)(chunkGuidOffset - startPosition));
			writer.Position = chunkEntryOffsetPosition;
			writer.WriteInt32BigEndian((int)(chunkEntryOffset - startPosition));
			writer.WriteInt32BigEndian((int)(chunkEntryOffset - startPosition));
			writer.Position = offset2Position;
			writer.WriteInt32BigEndian((int)(offset2 - startPosition));
			writer.WriteInt32BigEndian((int)(chunkEntryOffset - startPosition));
			writer.Position = offset8Position;
			writer.WriteInt32BigEndian((int)(offset3 - startPosition));
		}
	}
}
