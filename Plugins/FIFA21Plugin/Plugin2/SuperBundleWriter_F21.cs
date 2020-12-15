using FrostySdk;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
//using System.Buffers;
using System.IO;
using System.Linq;

namespace FIFA21Plugin.Plugin2
{
	public class SuperBundleWriter_F21
	{
		public void Write(Stream stream, DbObject bundle)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanWrite)
			{
				throw new ArgumentException("Stream must support writing.", "stream");
			}
			if (!stream.CanSeek)
			{
				throw new ArgumentException("Stream must support seeking.", "stream");
			}
			if (bundle == null)
			{
				throw new ArgumentNullException("bundle");
			}
			long sbStartingPosition = stream.Position;
			NativeWriter writer = new NativeWriter(stream);
			int entriesCount = bundle.GetValue<DbObject>("ebx").List.Count + bundle.GetValue<DbObject>("res").List.Count + bundle.GetValue<DbObject>("chunks").List.Count;
			writer.WriteInt32BigEndian(32);
			long headerStartPosition = stream.Position;
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian(entriesCount);
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian(0);
			long bundleEndPosition = new BundleWriter_F21().Write(stream, bundle);
			//byte[] flags = ArrayPool<byte>.Shared.Rent(entriesCount);
			byte[] flags = new byte[entriesCount];
			int currentCasIdentifier = -1;
			long startOfEntryDataOffset = writer.Position - 32 - sbStartingPosition;
			int entryIndex = 0;
			foreach (DbObject entry in bundle.GetValue<DbObject>("ebx").List.Concat(bundle.GetValue<DbObject>("res").List).Concat(bundle.GetValue<DbObject>("chunks").List))
			{
				int entryCasIdentifier = CasFile.CreateCasIdentifier(entry.GetValue<byte>("unk"), entry.HasValue("patch"), entry.GetValue<byte>("catalog"), entry.GetValue<byte>("cas"));
				if (entryCasIdentifier != currentCasIdentifier)
				{
					writer.WriteInt32BigEndian(entryCasIdentifier);
					flags[entryIndex] = 1;
					currentCasIdentifier = entryCasIdentifier;
				}
				else
				{
					flags[entryIndex] = 0;
				}
				writer.WriteInt32BigEndian(entry.GetValue<int>("offset"));
				writer.WriteInt32BigEndian(entry.GetValue<int>("size"));
				entryIndex++;
			}
			long startOfFlagsOffset = writer.Position - sbStartingPosition;
			writer.WriteBytes(flags);
			//ArrayPool<byte>.Shared.Return(flags);
			long endPosition = writer.Position;
			writer.Position = headerStartPosition;
			writer.WriteInt32BigEndian((int)startOfEntryDataOffset);
			writer.WriteInt32BigEndian((int)startOfFlagsOffset);
			writer.WriteInt32BigEndian(entriesCount);
			writer.WriteInt32BigEndian((int)(startOfEntryDataOffset + 32));
			writer.WriteInt32BigEndian((int)(startOfEntryDataOffset + 32));
			writer.WriteInt32BigEndian((int)(startOfEntryDataOffset + 32));
			writer.WriteInt32BigEndian(0);
			writer.WriteInt32BigEndian((int)bundleEndPosition - 36);
			writer.Position = endPosition;
		}
	}
}
