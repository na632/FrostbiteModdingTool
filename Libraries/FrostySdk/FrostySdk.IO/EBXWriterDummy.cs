using FMT.FileTools;
using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostySdk.IO
{
	/// <summary>
	/// The Dummy Writer Copies and Pastes the Original Data
	/// </summary>
    public class EBXWriterDummy : EbxBaseWriter
    {
		public EBXWriterDummy(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.None)
			: base(inStream, inFlags)
		{
			flags = inFlags;
		}

		public EBXWriterDummy(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.None, bool leaveOpen = false)
			: base(inStream, inFlags)
		{
			flags = inFlags;
		}

		public override void WriteAsset(EbxAsset asset)
		{
			if (asset.ParentEntry == null)
				throw new Exception("Dummy Asset Writer needs the original Stream to take data from!");

			var originalStream = AssetManager.Instance.GetEbxStream(asset.ParentEntry);
			if(originalStream != null)
            {
				using(NativeReader nrO =  new NativeReader(originalStream))
                {
					this.Write(nrO.ReadToEnd());
                }
            }


		}
	}
}
