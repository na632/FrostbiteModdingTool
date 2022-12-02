using FMT.FileTools;
using FrostySdk.IO;
using System;
using System.IO;

namespace FrostySdk.Resources
{
	public class ModifiedResource
	{
		internal virtual string ModifiedType => "ModifiedResource";

		public byte[] Save()
		{
			using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream()))
			{
				nativeWriter.WriteNullTerminatedString(ModifiedType);
				using (NativeWriter nativeWriter2 = new NativeWriter(new MemoryStream()))
				{
					SaveInternal(nativeWriter2);
					nativeWriter.Write(((MemoryStream)nativeWriter2.BaseStream).ToArray());
				}
				return ((MemoryStream)nativeWriter.BaseStream).ToArray();
			}
		}

		internal virtual void SaveInternal(NativeWriter writer)
		{
		}

		public static ModifiedResource Read(byte[] buffer)
		{
			using (NativeReader nativeReader = new NativeReader(new MemoryStream(buffer)))
			{
				string str = nativeReader.ReadNullTerminatedString();
				using (NativeReader reader = new NativeReader(new MemoryStream(nativeReader.ReadToEnd())))
				{
					ModifiedResource obj = (ModifiedResource)Activator.CreateInstance(Type.GetType("FrostySdk.Resources." + str));
					obj.ReadInternal(reader);
					return obj;
				}
			}
		}

		internal virtual void ReadInternal(NativeReader reader)
		{
		}
	}
}
