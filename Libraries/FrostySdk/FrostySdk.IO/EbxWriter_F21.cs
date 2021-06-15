using FrostySdk;
using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.FrostySdk.IO;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FrostySdk.IO
{
	public class EbxWriter_F21 : EbxWriterV3
	{
		public EbxWriter_F21(Stream inStream, EbxWriteFlags inFlags = EbxWriteFlags.IncludeTransient, bool leaveOpen = false)
			: base(inStream, inFlags)
		{
		}
	}
}