using FrostySdk;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FIFA21Plugin.Plugin2
{
	public class BundleWriter_F21
	{
		public long Write(Stream stream, DbObject bundle)
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
			NativeWriter writer = new NativeWriter(stream);
			long bundleStartOffset = writer.Position;
			int ebxEntries = bundle.GetValue<DbObject>("ebx").List.Count;
			int resEntries = bundle.GetValue<DbObject>("res").List.Count;
			int chunkEntries = bundle.GetValue<DbObject>("chunks").List.Count;
			int totalEntries = ebxEntries + resEntries + chunkEntries;
			writer.WriteUInt32BigEndian(3599661469u);
			writer.WriteInt32LittleEndian(totalEntries);
			writer.WriteInt32LittleEndian(ebxEntries);
			writer.WriteInt32LittleEndian(resEntries);
			writer.WriteInt32LittleEndian(chunkEntries);
			long placeholderPosition = writer.Position;
			writer.WriteUInt32LittleEndian(0u);
			writer.WriteUInt32LittleEndian(0u);
			writer.WriteUInt32LittleEndian(0u);
			foreach (DbObject item in bundle.GetValue<DbObject>("ebx").List.Concat(bundle.GetValue<DbObject>("res").List).Concat(bundle.GetValue<DbObject>("chunks").List))
			{
				Sha1 hash = item.GetValue<Sha1>("sha1");
				writer.Write(hash);
			}
			uint entryNamesOffset = 0u;
			WriteEbx(writer, bundle.GetValue<DbObject>("ebx").List.Cast<DbObject>(), ref entryNamesOffset);
			WriteRes(writer, bundle.GetValue<DbObject>("res").List.Cast<DbObject>(), ref entryNamesOffset);
			WriteChunks(writer, bundle.GetValue<DbObject>("chunks").List.Cast<DbObject>());
			long stringsOffset = writer.Position - bundleStartOffset;
			foreach (DbObject entry in bundle.GetValue<DbObject>("ebx").List.Concat(bundle.GetValue<DbObject>("res").List))
			{
				writer.WriteNullTerminatedString(entry.GetValue<string>("name"));
			}
			long chunkMetaOffset = 0L;
			long chunkMetaSize = 0L;
			if (chunkEntries > 0)
			{
				chunkMetaOffset = writer.Position - bundleStartOffset;
				new TocWriter().WriteObject(stream, "chunkMeta", bundle.GetValue<DbObject>("chunkMeta"));
				chunkMetaSize = writer.Position - bundleStartOffset - chunkMetaOffset;
			}
			long endPosition = writer.Position;
			writer.Position = placeholderPosition;
			writer.WriteUInt32LittleEndian((uint)stringsOffset);
			if (chunkMetaOffset != 0L)
			{
				writer.WriteUInt32LittleEndian((uint)chunkMetaOffset);
				writer.WriteUInt32LittleEndian((uint)chunkMetaSize);
			}
			else
			{
				writer.Write(0uL);
			}
			writer.Position = endPosition;
			writer.WritePadding(4);
			return endPosition;
		}

		private void WriteEbx(NativeWriter writer, IEnumerable<DbObject> ebxEntries, ref uint stringsOffset)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			if (ebxEntries == null)
			{
				throw new ArgumentNullException("ebxEntries");
			}
			foreach (DbObject ebxEntry in ebxEntries)
			{
				writer.WriteUInt32LittleEndian(stringsOffset);
				stringsOffset += (uint)(Encoding.ASCII.GetByteCount(ebxEntry.GetValue<string>("name")) + 1);
				writer.WriteUInt32LittleEndian(ebxEntry.GetValue<uint>("originalSize"));
			}
		}

		private void WriteRes(NativeWriter writer, IEnumerable<DbObject> resEntries, ref uint stringsOffset)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			if (resEntries == null)
			{
				throw new ArgumentNullException("resEntries");
			}
			foreach (DbObject resEntry4 in resEntries)
			{
				writer.WriteUInt32LittleEndian(stringsOffset);
				stringsOffset += (uint)(Encoding.ASCII.GetByteCount(resEntry4.GetValue<string>("name")) + 1);
				writer.WriteUInt32LittleEndian(resEntry4.GetValue<uint>("originalSize"));
			}
			foreach (DbObject resEntry3 in resEntries)
			{
				writer.WriteUInt32LittleEndian((uint)resEntry3.GetValue<long>("resType"));
			}
			foreach (DbObject resEntry2 in resEntries)
			{
				writer.WriteBytes(resEntry2.GetValue<byte[]>("resMeta"));
			}
			foreach (DbObject resEntry in resEntries)
			{
				writer.WriteInt64LittleEndian(resEntry.GetValue<long>("resRid"));
			}
		}

		private void WriteChunks(NativeWriter writer, IEnumerable<DbObject> chunkEntries)
		{
			if (writer == null)
			{
				throw new ArgumentNullException("writer");
			}
			if (chunkEntries == null)
			{
				throw new ArgumentNullException("chunkEntries");
			}
			foreach (DbObject chunkEntry in chunkEntries)
			{
				writer.WriteGuid(chunkEntry.GetValue<Guid>("id"));
				writer.WriteUInt32LittleEndian(chunkEntry.GetValue<uint>("logicalOffset"));
				writer.WriteUInt32LittleEndian(chunkEntry.GetValue<uint>("logicalSize"));
			}
		}
	}
}
