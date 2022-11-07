using Frosty.Hash;
using FrostySdk.IO;
using System.Globalization;

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

                if (writerVersion >= 28u)
                    writer.WriteLengthPrefixedString(userData);
                else
                    writer.WriteNullTerminatedString(userData);

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
				writer.Write(Fnv1.HashString(customTypeName));
			}
		}

		protected void AddBundle(string name, bool modify)
		{
			int result = 0;
			int item = Fnv1.HashString(name.ToLower());
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
