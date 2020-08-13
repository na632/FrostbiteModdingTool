using Frosty.Hash;
using FrostySdk.Deobfuscators;
using FrostySdk.IO;
using FrostySdk.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FrostySdk
{
	public static class ProfilesLibrary
	{
		private struct Profile
		{
			public string Name;

			public string DisplayName;

			public int DataVersion;

			public string CacheName;

			public string Deobfuscator;

			public string AssetLoader;

			public List<FileSystemSource> Sources;

			public string SDKFilename;

			public byte[] Banner;

			public int EbxVersion;

			public bool RequiresKey;

			public bool MustAddChunks;

			public bool EnableExecution;

			public string DefaultDiffuse;

			public string DefaultNormals;

			public string DefaultMask;

			public string DefaultTint;

			public Dictionary<int, string> SharedBundles;

			public List<uint> IgnoredResTypes;
		}

		private static Profile effectiveProfile;

		private static string DeobfuscatorNamespace = typeof(NullDeobfuscator).Namespace;

		private static string AssetLoaderNamespace = typeof(AssetManager).FullName;

		private static byte[][] obfuscationKey = new byte[6][]
		{
			new byte[6]
			{
				70,
				84,
				118,
				33,
				55,
				84
			},
			new byte[6]
			{
				72,
				82,
				50,
				69,
				86,
				41
			},
			new byte[6]
			{
				75,
				90,
				79,
				82,
				54,
				42
			},
			new byte[6]
			{
				77,
				86,
				67,
				83,
				58,
				82
			},
			new byte[6]
			{
				80,
				93,
				70,
				90,
				84,
				45
			},
			new byte[6]
			{
				86,
				80,
				74,
				37,
				67,
				89
			}
		};

		public static string ProfileName => effectiveProfile.Name;

		public static string DisplayName => effectiveProfile.DisplayName;

		public static string CacheName => effectiveProfile.CacheName;

		public static Type Deobfuscator => Type.GetType(DeobfuscatorNamespace + "." + effectiveProfile.Deobfuscator);

		public static Type AssetLoader => Type.GetType(AssetLoaderNamespace + "+" + effectiveProfile.AssetLoader);

		public static int DataVersion => effectiveProfile.DataVersion;

		public static List<FileSystemSource> Sources => effectiveProfile.Sources;

		public static string SDKFilename => effectiveProfile.SDKFilename;

		public static byte[] Banner => effectiveProfile.Banner;

		public static int EbxVersion => effectiveProfile.EbxVersion;

		public static bool RequiresKey => effectiveProfile.RequiresKey;

		public static bool MustAddChunks => effectiveProfile.MustAddChunks;

		public static bool EnableExecution => effectiveProfile.EnableExecution;

		public static string DefaultDiffuse => effectiveProfile.DefaultDiffuse;

		public static string DefaultNormals => effectiveProfile.DefaultNormals;

		public static string DefaultMask => effectiveProfile.DefaultMask;

		public static string DefaultTint => effectiveProfile.DefaultTint;

		public static Dictionary<int, string> SharedBundles => effectiveProfile.SharedBundles;

		public static bool IsResTypeIgnored(ResourceType resType)
		{
			return effectiveProfile.IgnoredResTypes.Contains((uint)resType);
		}

		public static void DumpAllFrostyProfiles()
		{
			using (FileStream fileStream = new FileStream("FrostySdk.Profiles.bin", FileMode.Open))
			using (NativeReader nativeReader = new NativeReader(fileStream))
			{
				int num2 = nativeReader.ReadInt();

				string text = DecodeString(nativeReader);
				long num = nativeReader.ReadLong();

				if (num == -1)
				{
					return;
				}
				nativeReader.Position += num;
				Profile profile = default(Profile);
				profile.Name = text;
				profile.DisplayName = DecodeString(nativeReader);
				profile.DataVersion = nativeReader.ReadInt();
				profile.CacheName = DecodeString(nativeReader);
				profile.Deobfuscator = DecodeString(nativeReader);
				profile.AssetLoader = DecodeString(nativeReader);
				profile.Sources = new List<FileSystemSource>();
				profile.SharedBundles = new Dictionary<int, string>();
				profile.IgnoredResTypes = new List<uint>();
				int num4 = nativeReader.ReadInt();
				for (int j = 0; j < num4; j++)
				{
					FileSystemSource item = default(FileSystemSource);
					item.Path = DecodeString(nativeReader);
					item.SubDirs = (nativeReader.ReadByte() == 1);
					profile.Sources.Add(item);
				}
				profile.SDKFilename = DecodeString(nativeReader);
				profile.Banner = nativeReader.ReadBytes(nativeReader.ReadInt());
				profile.DefaultDiffuse = DecodeString(nativeReader);
				profile.DefaultNormals = DecodeString(nativeReader);
				profile.DefaultMask = DecodeString(nativeReader);
				profile.DefaultTint = DecodeString(nativeReader);
				int num5 = nativeReader.ReadInt();
				for (int k = 0; k < num5; k++)
				{
					string text2 = DecodeString(nativeReader);
					profile.SharedBundles.Add(Fnv1.HashString(text2.ToLower()), text2);
				}
				int num6 = nativeReader.ReadInt();
				for (int l = 0; l < num6; l++)
				{
					profile.IgnoredResTypes.Add(nativeReader.ReadUInt());
				}
				profile.MustAddChunks = (nativeReader.ReadByte() == 1);
				profile.EbxVersion = nativeReader.ReadByte();
				profile.RequiresKey = (nativeReader.ReadByte() == 1);
				profile.EnableExecution = (nativeReader.ReadByte() != 1);

				System.IO.File.WriteAllText(text + "Profile.json", JsonConvert.SerializeObject(profile));
			}
		}

		public static void DumpFrostyProfile(string profileKey)
		{
			using (FileStream fileStream = new FileStream("FrostySdk.Profiles.bin", FileMode.Open))
			using (NativeReader nativeReader = new NativeReader(fileStream))
			{
				long num = -1L;
				int num2 = nativeReader.ReadInt();
				for (int i = 0; i < num2; i++)
				{
					var text = DecodeString(nativeReader);
					long num3 = nativeReader.ReadLong();
					if (text.Equals("FIFA18", StringComparison.OrdinalIgnoreCase))
					{
						num = num3;
						profileKey = text;
					}
				}
				if (num == -1)
				{
					return;
				}
				nativeReader.Position += num;
				Profile profile = default(Profile);
				profile.Name = profileKey;
				profile.DisplayName = DecodeString(nativeReader);
				profile.DataVersion = nativeReader.ReadInt();
				profile.CacheName = DecodeString(nativeReader);
				profile.Deobfuscator = DecodeString(nativeReader);
				profile.AssetLoader = DecodeString(nativeReader);
				profile.Sources = new List<FileSystemSource>();
				profile.SharedBundles = new Dictionary<int, string>();
				profile.IgnoredResTypes = new List<uint>();
				int num4 = nativeReader.ReadInt();
				for (int j = 0; j < num4; j++)
				{
					FileSystemSource item = default(FileSystemSource);
					item.Path = DecodeString(nativeReader);
					item.SubDirs = (nativeReader.ReadByte() == 1);
					profile.Sources.Add(item);
				}
				profile.SDKFilename = DecodeString(nativeReader);
				profile.Banner = nativeReader.ReadBytes(nativeReader.ReadInt());
				profile.DefaultDiffuse = DecodeString(nativeReader);
				profile.DefaultNormals = DecodeString(nativeReader);
				profile.DefaultMask = DecodeString(nativeReader);
				profile.DefaultTint = DecodeString(nativeReader);
				int num5 = nativeReader.ReadInt();
				for (int k = 0; k < num5; k++)
				{
					string text2 = DecodeString(nativeReader);
					profile.SharedBundles.Add(Fnv1.HashString(text2.ToLower()), text2);
				}
				int num6 = nativeReader.ReadInt();
				for (int l = 0; l < num6; l++)
				{
					profile.IgnoredResTypes.Add(nativeReader.ReadUInt());
				}
				profile.MustAddChunks = (nativeReader.ReadByte() == 1);
				profile.EbxVersion = nativeReader.ReadByte();
				profile.RequiresKey = (nativeReader.ReadByte() == 1);
				profile.EnableExecution = (nativeReader.ReadByte() != 1);

				System.IO.File.WriteAllText(profileKey + "Profile.json", JsonConvert.SerializeObject(profile));
			}
		}

		public static bool Initialize(string profileKey)
		{
			if (File.Exists(profileKey + "Profile.json"))
			{
				Profile profile = default(Profile);
				profile = JsonConvert.DeserializeObject<Profile>(System.IO.File.ReadAllText(profileKey + "Profile.json"));
				effectiveProfile = profile;
				return true;
			}
			else
			{
				long num = -1L;
				using (FileStream fileStream = new FileStream("FrostySdk.Profiles.bin", FileMode.Open))
				using (NativeReader nativeReader = new NativeReader(fileStream))
				{
					int num2 = nativeReader.ReadInt();
					for (int i = 0; i < num2; i++)
					{
						string text = DecodeString(nativeReader);
						long num3 = nativeReader.ReadLong();
						if (text.Equals(profileKey, StringComparison.OrdinalIgnoreCase))
						{
							num = num3;
							profileKey = text;
						}
					}
					if (num == -1)
					{
						return false;
					}
					nativeReader.Position += num;
					Profile profile = default(Profile);
					profile.Name = profileKey;
					profile.DisplayName = DecodeString(nativeReader);
					profile.DataVersion = nativeReader.ReadInt();
					profile.CacheName = DecodeString(nativeReader);
					profile.Deobfuscator = DecodeString(nativeReader);
					profile.AssetLoader = DecodeString(nativeReader);
					profile.Sources = new List<FileSystemSource>();
					profile.SharedBundles = new Dictionary<int, string>();
					profile.IgnoredResTypes = new List<uint>();
					int num4 = nativeReader.ReadInt();
					for (int j = 0; j < num4; j++)
					{
						FileSystemSource item = default(FileSystemSource);
						item.Path = DecodeString(nativeReader);
						item.SubDirs = (nativeReader.ReadByte() == 1);
						profile.Sources.Add(item);
					}
					profile.SDKFilename = DecodeString(nativeReader);
					profile.Banner = nativeReader.ReadBytes(nativeReader.ReadInt());
					profile.DefaultDiffuse = DecodeString(nativeReader);
					profile.DefaultNormals = DecodeString(nativeReader);
					profile.DefaultMask = DecodeString(nativeReader);
					profile.DefaultTint = DecodeString(nativeReader);
					int num5 = nativeReader.ReadInt();
					for (int k = 0; k < num5; k++)
					{
						string text2 = DecodeString(nativeReader);
						profile.SharedBundles.Add(Fnv1.HashString(text2.ToLower()), text2);
					}
					int num6 = nativeReader.ReadInt();
					for (int l = 0; l < num6; l++)
					{
						profile.IgnoredResTypes.Add(nativeReader.ReadUInt());
					}
					profile.MustAddChunks = (nativeReader.ReadByte() == 1);
					profile.EbxVersion = nativeReader.ReadByte();
					profile.RequiresKey = (nativeReader.ReadByte() == 1);
					profile.EnableExecution = (nativeReader.ReadByte() != 1);

					System.IO.File.WriteAllText(profileKey + "Profile.json", JsonConvert.SerializeObject(profile));
					// Write out to json and reinit
					return Initialize(profileKey);
				}

			}

		}

		private static string DecodeString(NativeReader reader)
		{
			int num = reader.Read7BitEncodedInt();
			byte[] array = reader.ReadBytes(num);
			for (int i = 0; i < num; i++)
			{
				array[i] = (byte)(array[i] ^ obfuscationKey[i % obfuscationKey.Length][(i + obfuscationKey.Length * (0x1000 | i)) % obfuscationKey.Length]);
			}
			return Encoding.UTF8.GetString(array);
		}
	}
}
