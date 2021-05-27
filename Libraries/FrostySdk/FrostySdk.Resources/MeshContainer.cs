// Sdk.IO.MeshContainer
using System;
using System.Collections.Generic;
using FrostySdk;
using FrostySdk.IO;

public class MeshContainer
{
	public class RelocPtr
	{
		public string Type;

		public long Offset;

		public object Data;

		public long DataOffset;

		public RelocPtr(string type, object data)
		{
			Type = type;
			Data = data;
		}
	}

	public class RelocArray : RelocPtr
	{
		public int Count;

		public RelocArray(string type, int count, object arrayData)
			: base(type, arrayData)
		{
			Count = count;
		}
	}

	private List<RelocPtr> relocPtrs = new List<RelocPtr>();

	private Dictionary<RelocPtr, string> strings = new Dictionary<RelocPtr, string>();

	public void AddString(object obj, string data, bool ignoreNull = false)
	{
		RelocPtr relocPtr = new RelocPtr("STR", obj);
		relocPtrs.Add(relocPtr);
		strings.Add(relocPtr, data + (ignoreNull ? "" : "\0"));
	}

	public void AddRelocPtr(string type, object obj)
	{
		relocPtrs.Add(new RelocPtr(type, obj));
	}

	public void WriteRelocPtr(string type, object obj, NativeWriter writer)
	{
		FindRelocPtr(type, obj).Offset = writer.BaseStream.Position;
		writer.Write(16045690984833335023uL);
	}

	public void AddRelocArray(string type, int count, object arrayObj)
	{
		relocPtrs.Add(new RelocArray(type, count, arrayObj));
	}

	public void WriteRelocArray(string type, object arrayObj, NativeWriter writer)
	{
		RelocArray relocArray = FindRelocPtr(type, arrayObj) as RelocArray;
		relocArray.Offset = writer.BaseStream.Position + 4;
		writer.WriteInt32LittleEndian(relocArray.Count);
		writer.Write(16045690984833335023uL);
	}

	public void AddOffset(string type, object data, NativeWriter writer)
	{
		RelocPtr relocPtr = FindRelocPtr(type, data);
		if (relocPtr != null)
		{
			relocPtr.DataOffset = writer.BaseStream.Position;
		}
	}

	public void WriteStrings(NativeWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		foreach (RelocPtr key in strings.Keys)
		{
			key.DataOffset = writer.BaseStream.Position;
			writer.WriteFixedSizedString(strings[key], strings[key].Length);
		}
	}

	public void FixupRelocPtrs(NativeWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		foreach (RelocPtr relocPtr in relocPtrs)
		{
			writer.BaseStream.Position = relocPtr.Offset;
			writer.WriteInt64LittleEndian(relocPtr.DataOffset);
		}
	}

	public void WriteRelocTable(NativeWriter writer)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		writer.BaseStream.Position = writer.BaseStream.Length;
		foreach (RelocPtr relocPtr in relocPtrs)
		{
			writer.WriteUInt32LittleEndian((uint)relocPtr.Offset);
		}
	}

	private RelocPtr FindRelocPtr(string type, object obj)
	{
		foreach (RelocPtr relocPtr in relocPtrs)
		{
			if (relocPtr.Type == type && relocPtr.Data.Equals(obj))
			{
				return relocPtr;
			}
		}
		return null;
	}
}
