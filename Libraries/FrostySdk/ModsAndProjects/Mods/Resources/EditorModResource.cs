using FMT.FileTools;
using FMT.FileTools.Modding;
using FrostySdk.IO;
using System.Globalization;
using Fnv1a = FMT.FileTools.Fnv1a;

namespace FrostySdk
{
	public class EditorModResource : BaseModResource
	{
		public virtual void Write(NativeWriter writer, uint writerVersion = 4u)
		{
			WriteResourceType(writer, Type);
			writer.Write(resourceIndex);
			if (resourceIndex != -1)
			{
				if(writerVersion >= 28u)
                    writer.WriteLengthPrefixedString(name);
                else
					writer.WriteNullTerminatedString(name);

				writer.Write(sha1);
				writer.Write(size);
				writer.Write(flags);
				writer.Write(handlerHash);

				if(string.IsNullOrEmpty(UserData))
					UserData = string.Empty;	

                if (writerVersion >= 28u)
                    writer.WriteLengthPrefixedString(UserData);
                else
                    writer.WriteNullTerminatedString(UserData);

				writer.Write(bundlesToModify.Count);
				foreach (int item in bundlesToModify)
				{
					writer.Write(item);
				}
				writer.Write(bundlesToAdd.Count);
				foreach (int item2 in bundlesToAdd)
				{
					writer.Write(item2);
				}
			}
		}

		protected void WriteResourceType(NativeWriter writer, ModResourceType resType, string customTypeName = "")
		{
			writer.Write((byte)resType);
			if (resType == ModResourceType.Custom)
			{
				writer.Write(Fnv1a.HashString(customTypeName));
			}
		}

		protected void AddBundle(string name, bool modify)
		{
			int result = 0;
			int item = Fnv1a.HashString(name.ToLower());
			if (name.Length == 8 && int.TryParse(name, NumberStyles.HexNumber, null, out result))
			{
				item = result;
			}
			if (modify)
			{
				bundlesToModify.Add(item);
			}
			else
			{
				bundlesToAdd.Add(item);
			}
		}
	}
}
