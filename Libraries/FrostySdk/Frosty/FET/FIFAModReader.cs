using System;
using System.IO;
using System.Reflection;
using FrostySdk.IO;
using Modding.Categories;
using FrostySdk;
using FrostySdk.Managers;
using Standart.Hash.xxHash;
using FrostySdk.Frosty.FET;
using FrostbiteSdk.Frosty.Abstract;

namespace FrostySdk
{
	public class FIFAModReader : BaseModReader
	{
		private class BundleResource : BaseModResource
		{
			private int superBundleName;

			public override ModResourceType Type => ModResourceType.Bundle;

			public override void Read(NativeReader reader)
			{
				base.Read(reader);
				name = reader.ReadNullTerminatedString();
				superBundleName = reader.ReadInt32LittleEndian();
			}

			public override void FillAssetEntry(object entry)
			{
				BundleEntry obj = (BundleEntry)entry;
				obj.Name = name;
				obj.SuperBundleId = superBundleName;
			}
		}

		public const int MinSupportedVersion = 4;

		private readonly long dataOffset;

		private readonly int dataCount;

		public bool IsValid
		{
			get;
		}

		public int GameVersion
		{
			get;
		}

		public uint Version
		{
			get;
		}

		public bool HasChecksums { get; }

		private readonly long startOfHeaderChecksumDataPosition;

		private readonly long endOfHeaderOffset;

		private readonly ulong headerChecksum;


		public FIFAModReader(Stream stream): base(stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			ulong header = ReadUInt64LittleEndian();
			if (header != 72155812747760198L && header != 5498700893333637446L)
			{
				return;
			}
			Version = ReadUInt32LittleEndian();
			if (Version <= 10 && Version >= 4)
			{
				if (Version >= 10)
				{
					HasChecksums = ReadBoolean();
					endOfHeaderOffset = ReadInt64LittleEndian();
					headerChecksum = ReadUInt64LittleEndian();
					startOfHeaderChecksumDataPosition = base.Position;
				}
				dataOffset = ReadInt64LittleEndian();
				dataCount = ReadInt32LittleEndian();
				var gn = ReadLengthPrefixedString();
				GameVersion = ReadInt32LittleEndian();
				IsValid = true;
			}
		}

		public bool CheckChecksums()
		{
			if (!HasChecksums)
			{
				throw new InvalidOperationException("The mod does not contain checksums.");
			}
			long startingPosition = base.Position;
			try
			{
				ulong headerChecksum = xxHash64.ComputeHash(new SubStream(base.BaseStream, startOfHeaderChecksumDataPosition, endOfHeaderOffset - startOfHeaderChecksumDataPosition), 32768, 0uL);
				if (this.headerChecksum != headerChecksum)
				{
					return false;
				}
				base.Position = endOfHeaderOffset;
				ulong num = ReadUInt64LittleEndian();
				long offset = endOfHeaderOffset + 8;
				ulong calculatedDataChecksum = xxHash64.ComputeHash(new SubStream(base.BaseStream, offset, base.Length - offset), 32768, 0uL);
				return num == calculatedDataChecksum;
			}
			finally
			{
				base.Position = startingPosition;
			}
		}


		public FIFAModDetails ReadModDetails()
		{
			string inTitle = ReadLengthPrefixedString();
			string author = ReadLengthPrefixedString();
			ModMainCategory mainCategory = ModMainCategory.Custom;
			byte subCategoryId = 0;
			if (Version >= 6)
			{
				mainCategory = (ModMainCategory)ReadByte();
				subCategoryId = ReadByte();
			}
			string customCategory = ReadLengthPrefixedString();
			string customSubCategory = null;
			if (Version >= 7)
			{
				customSubCategory = ReadLengthPrefixedString();
			}
			if (Version < 6)
			{
				if (Enum.TryParse<ModMainCategory>(customCategory, ignoreCase: true, out var parsedMainCategory))
				{
					mainCategory = parsedMainCategory;
					subCategoryId = 0;
				}
				else
				{
					mainCategory = ModMainCategory.Custom;
					subCategoryId = 0;
				}
			}
			Enum subCategory = ModCustomSubCategory.Custom;
			string userVersion = ReadLengthPrefixedString();
			string description = ReadLengthPrefixedString();
			string outOfDateModWebsiteLink = ((Version >= 8) ? ReadLengthPrefixedString() : string.Empty);
			string discordServerLink = ((Version >= 8) ? ReadLengthPrefixedString() : string.Empty);
			string patreonLink = ((Version >= 8) ? ReadLengthPrefixedString() : string.Empty);
			string twitterLink = ((Version >= 8) ? ReadLengthPrefixedString() : string.Empty);
			string youTubeLink = ((Version >= 8) ? ReadLengthPrefixedString() : string.Empty);
			string instagramLink = ((Version >= 8) ? ReadLengthPrefixedString() : string.Empty);
			string facebookLink = ((Version >= 8) ? ReadLengthPrefixedString() : string.Empty);
			string customLink = ((Version >= 8) ? ReadLengthPrefixedString() : string.Empty);
			uint screenshotsCount = ((Version < 5) ? 4u : ReadUInt32LittleEndian());
			return new FIFAModDetails(inTitle: inTitle, inAuthor: author, mainCategory: mainCategory, subCategory: subCategory, customCategory: customCategory, secondCustomCategory: customSubCategory, inVersion: userVersion, inDescription: description)
			{
				OutOfDateModWebsiteLink = outOfDateModWebsiteLink,
				DiscordServerLink = discordServerLink,
				PatreonLink = patreonLink,
				TwitterLink = twitterLink,
				YouTubeLink = youTubeLink,
				InstagramLink = instagramLink,
				FacebookLink = facebookLink,
				CustomLink = customLink,
				ScreenshotsCount = screenshotsCount
			};
		}

		public BaseModResource[] ReadResources()
		{
			if (Version >= 10)
			{
				base.Position = endOfHeaderOffset + 8;
			}
			int num = ReadInt32LittleEndian();
			BaseModResource[] array = new BaseModResource[num];
			for (int i = 0; i < num; i++)
			{
				switch (ReadByte())
				{
					case 0:
						array[i] = new EmbeddedResource();
						break;
					case 1:
						array[i] = new EbxResource();
						break;
					case 2:
						array[i] = new ResResource();
						break;
					case 3:
						array[i] = new ChunkResource();
						break;
					case 5:
						array[i] = new BundleResource();
						break;
				}
				array[i].Read(this);
			}
			return array;

		}

		public byte[] GetResourceData(BaseModResource resource)
		{
			if (resource == null)
			{
				throw new ArgumentNullException("resource");
			}
			if (resource.ResourceIndex == -1)
			{
				return null;
			}
			base.Position = dataOffset + resource.ResourceIndex * 16;
			long num = ReadInt64LittleEndian();
			long num2 = ReadInt64LittleEndian();
			base.Position = dataOffset + dataCount * 16 + num;
			return ReadBytes((int)num2);
		}
	}
}
