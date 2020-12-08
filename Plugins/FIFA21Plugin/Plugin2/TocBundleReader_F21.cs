using FrostySdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FIFA21Plugin.Plugin2
{
	public class TocBundleReader_F21
	{
		public DbObject Read(Stream bundleDataStream, IEnumerable<TocFile_F21.CasBundleEntry> entries)
		{
			if (bundleDataStream == null)
			{
				throw new ArgumentNullException("bundleDataStream");
			}
			if (entries == null)
			{
				throw new ArgumentNullException("entries");
			}
			bundleDataStream.Position = 0L;
			DbObject dbObject = new BundleReader_F21().Read(bundleDataStream);
			IEnumerable<DbObject> ebxEntries = dbObject.GetValue<DbObject>("ebx").List.Cast<DbObject>();
			IEnumerable<DbObject> resEntries = dbObject.GetValue<DbObject>("res").List.Cast<DbObject>();
			IEnumerable<DbObject> chunkEntries = dbObject.GetValue<DbObject>("chunks").List.Cast<DbObject>();
			foreach (var (entry, otherEntry) in ebxEntries.Cast<DbObject>().Union(resEntries.Cast<DbObject>()).Union(chunkEntries.Cast<DbObject>())
				.Zip(entries, (DbObject e1, TocFile_F21.CasBundleEntry e2) => (e1, e2)))
			{
				entry.AddValue("bundleEntry", otherEntry);
				entry.AddValue("offset", otherEntry.EntryOffset);
				entry.AddValue("size", otherEntry.EntrySize);
				entry.AddValue("catalog", otherEntry.CasCatalog);
				entry.AddValue("cas", otherEntry.CasIndex);
				if (otherEntry.InPatch)
				{
					entry.AddValue("patch", true);
				}
			}
			return dbObject;
		}
	}
}
