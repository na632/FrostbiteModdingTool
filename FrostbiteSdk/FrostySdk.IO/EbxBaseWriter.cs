using System.IO;

namespace FrostySdk.IO
{
	public class EbxBaseWriter : NativeWriter
	{
		protected EbxWriteFlags flags;

		public EbxBaseWriter(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.None, bool leaveOpen = false)
			: base(inStream, leaveOpen)
		{
			flags = inFlags;
		}

		public virtual void WriteAsset(EbxAsset asset)
		{
		}
	}
}
