using FrostySdk.Interfaces;
using System.IO;

namespace FrostySdk.IO
{
	public class CatReader : NativeReader
	{
		public const string CatMagic = "NyanNyanNyanNyan";

		private uint entryCount;

		private uint patchCount;

		private uint encryptedCount;

		public uint ResourceCount => entryCount;

		public uint PatchCount => patchCount;

		public uint EncryptedCount => encryptedCount;

		public CatReader(Stream inStream, IDeobfuscator inDeobfuscator)
			: base(inStream, inDeobfuscator)
		{
			if (ReadSizedString(16) != "NyanNyanNyanNyan")
			{
				return;
			}
			entryCount = (uint)(Length - Position) / 32u;
			patchCount = 0u;
			encryptedCount = 0u;
			if (ProfileManager.DataVersion == 20131115 || ProfileManager.DataVersion == 20141118 || ProfileManager.DataVersion == 20141117 || ProfileManager.DataVersion == 20151103)
			{
				return;
			}
			entryCount = ReadUInt();
			patchCount = ReadUInt();
			if (ProfileManager.DataVersion == 20170321 || ProfileManager.DataVersion == 20160927 || ProfileManager.DataVersion == 20171117 || ProfileManager.DataVersion == 20170929 || ProfileManager.DataVersion == 20171110 || ProfileManager.DataVersion == 20180807 || ProfileManager.DataVersion == 20180628)
			{
				encryptedCount = ReadUInt();
				Position += 12L;
				if (ProfileManager.DataVersion == 20171117 || ProfileManager.DataVersion == 20171110 || ProfileManager.DataVersion == 20180807 || ProfileManager.DataVersion == 20180628)
				{
					encryptedCount = 0u;
				}
			}
		}

		public CatResourceEntry ReadResourceEntry()
		{
			CatResourceEntry result = default(CatResourceEntry);
			result.Sha1 = ReadSha1();
			result.Offset = ReadUInt();
			result.Size = ReadUInt();
			if (ProfileManager.DataVersion != 20131115 && ProfileManager.DataVersion != 20141118 && ProfileManager.DataVersion != 20141117 && ProfileManager.DataVersion != 20151103)
			{
				result.LogicalOffset = ReadUInt();
			}
			result.ArchiveIndex = (ReadInt() & 0xFF);
			return result;
		}

		public CatResourceEntry ReadEncryptedEntry()
		{
			CatResourceEntry result = default(CatResourceEntry);
			result.Sha1 = ReadSha1();
			result.Offset = ReadUInt();
			result.Size = ReadUInt();
			result.LogicalOffset = ReadUInt();
			result.ArchiveIndex = (ReadInt() & 0xFF);
			result.Unknown = ReadUInt();
			result.IsEncrypted = true;
			result.KeyId = ReadSizedString(8);
			result.UnknownData = ReadBytes(32);
			result.EncryptedSize = result.Size;
			result.Size += 16 - result.Size % 16u;
			return result;
		}

		public CatPatchEntry ReadPatchEntry()
		{
			CatPatchEntry result = default(CatPatchEntry);
			result.Sha1 = ReadSha1();
			result.BaseSha1 = ReadSha1();
			result.DeltaSha1 = ReadSha1();
			return result;
		}
	}
}
