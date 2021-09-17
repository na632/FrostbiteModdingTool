using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FifaLibrary
{
	public class FifaUtil
	{
		private static uint[] c_LanguageHashtable = new uint[256]
		{
			0u, 1996959894u, 3993919788u, 2567524794u, 124634137u, 1886057615u, 3915621685u, 2657392035u, 249268274u, 2044508324u,
			3772115230u, 2547177864u, 162941995u, 2125561021u, 3887607047u, 2428444049u, 498536548u, 1789927666u, 4089016648u, 2227061214u,
			450548861u, 1843258603u, 4107580753u, 2211677639u, 325883990u, 1684777152u, 4251122042u, 2321926636u, 335633487u, 1661365465u,
			4195302755u, 2366115317u, 997073096u, 1281953886u, 3579855332u, 2724688242u, 1006888145u, 1258607687u, 3524101629u, 2768942443u,
			901097722u, 1119000684u, 3686517206u, 2898065728u, 853044451u, 1172266101u, 3705015759u, 2882616665u, 651767980u, 1373503546u,
			3369554304u, 3218104598u, 565507253u, 1454621731u, 3485111705u, 3099436303u, 671266974u, 1594198024u, 3322730930u, 2970347812u,
			795835527u, 1483230225u, 3244367275u, 3060149565u, 1994146192u, 31158534u, 2563907772u, 4023717930u, 1907459465u, 112637215u,
			2680153253u, 3904427059u, 2013776290u, 251722036u, 2517215374u, 3775830040u, 2137656763u, 141376813u, 2439277719u, 3865271297u,
			1802195444u, 476864866u, 2238001368u, 4066508878u, 1812370925u, 453092731u, 2181625025u, 4111451223u, 1706088902u, 314042704u,
			2344532202u, 4240017532u, 1658658271u, 366619977u, 2362670323u, 4224994405u, 1303535960u, 984961486u, 2747007092u, 3569037538u,
			1256170817u, 1037604311u, 2765210733u, 3554079995u, 1131014506u, 879679996u, 2909243462u, 3663771856u, 1141124467u, 855842277u,
			2852801631u, 3708648649u, 1342533948u, 654459306u, 3188396048u, 3373015174u, 1466479909u, 544179635u, 3110523913u, 3462522015u,
			1591671054u, 702138776u, 2966460450u, 3352799412u, 1504918807u, 783551873u, 3082640443u, 3233442989u, 3988292384u, 2596254646u,
			62317068u, 1957810842u, 3939845945u, 2647816111u, 81470997u, 1943803523u, 3814918930u, 2489596804u, 225274430u, 2053790376u,
			3826175755u, 2466906013u, 167816743u, 2097651377u, 4027552580u, 2265490386u, 503444072u, 1762050814u, 4150417245u, 2154129355u,
			426522225u, 1852507879u, 4275313526u, 2312317920u, 282753626u, 1742555852u, 4189708143u, 2394877945u, 397917763u, 1622183637u,
			3604390888u, 2714866558u, 953729732u, 1340076626u, 3518719985u, 2797360999u, 1068828381u, 1219638859u, 3624741850u, 2936675148u,
			906185462u, 1090812512u, 3747672003u, 2825379669u, 829329135u, 1181335161u, 3412177804u, 3160834842u, 628085408u, 1382605366u,
			3423369109u, 3138078467u, 570562233u, 1426400815u, 3317316542u, 2998733608u, 733239954u, 1555261956u, 3268935591u, 3050360625u,
			752459403u, 1541320221u, 2607071920u, 3965973030u, 1969922972u, 40735498u, 2617837225u, 3943577151u, 1913087877u, 83908371u,
			2512341634u, 3803740692u, 2075208622u, 213261112u, 2463272603u, 3855990285u, 2094854071u, 198958881u, 2262029012u, 4057260610u,
			1759359992u, 534414190u, 2176718541u, 4139329115u, 1873836001u, 414664567u, 2282248934u, 4279200368u, 1711684554u, 285281116u,
			2405801727u, 4167216745u, 1634467795u, 376229701u, 2685067896u, 3608007406u, 1308918612u, 956543938u, 2808555105u, 3495958263u,
			1231636301u, 1047427035u, 2932959818u, 3654703836u, 1088359270u, 936918000u, 2847714899u, 3736837829u, 1202900863u, 817233897u,
			3183342108u, 3401237130u, 1404277552u, 615818150u, 3134207493u, 3453421203u, 1423857449u, 601450431u, 3009837614u, 3294710456u,
			1567103746u, 711928724u, 3020668471u, 3272380065u, 1510334235u, 755167117u
		};

		private static CultureInfo s_Culture = new CultureInfo("en-GB");

		private static UTF8Encoding ue = new UTF8Encoding();

		public static DateTime ConvertToDate(string s)
		{
			DateTime result = default(DateTime);
			string text = s.Replace("\t", "");
			text = text.Trim();
			try
			{
				result = Convert.ToDateTime(text, s_Culture);
				return result;
			}
			catch
			{
				return result;
			}
		}

		public static short SwapEndian(short x)
		{
			byte b = (byte)((uint)x & 0xFFu);
			return (short)((byte)((x & 0xFF00) >> 8) + b * 256);
		}

		public static ushort SwapEndian(ushort x)
		{
			byte b = (byte)(x & 0xFFu);
			return (ushort)((byte)((x & 0xFF00) >> 8) + b * 256);
		}

		public static int SwapEndian(int x)
		{
			int num = x & 0xFF;
			x >>= 8;
			int num2 = num * 256 + (x & 0xFF);
			x >>= 8;
			int num3 = num2 * 256 + (x & 0xFF);
			x >>= 8;
			return num3 * 256 + (x & 0xFF);
		}

		public static uint SwapEndian(uint x)
		{
			uint num = x & 0xFF;
			x >>= 8;
			uint num2 = num * 256 + (x & 0xFF);
			x >>= 8;
			uint num3 = num2 * 256 + (x & 0xFF);
			x >>= 8;
			return num3 * 256 + (x & 0xFF);
		}

		public static long SwapEndian(long x)
		{
			long num = x & 0xFF;
			x >>= 8;
			long num2 = num * 256 + (x & 0xFF);
			x >>= 8;
			long num3 = num2 * 256 + (x & 0xFF);
			x >>= 8;
			long num4 = num3 * 256 + (x & 0xFF);
			x >>= 8;
			long num5 = num4 * 256 + (x & 0xFF);
			x >>= 8;
			long num6 = num5 * 256 + (x & 0xFF);
			x >>= 8;
			long num7 = num6 * 256 + (x & 0xFF);
			x >>= 8;
			return num7 * 256 + (x & 0xFF);
		}

		public static ulong SwapEndian(ulong x)
		{
			ulong num = x & 0xFF;
			x >>= 8;
			ulong num2 = num * 256 + (x & 0xFF);
			x >>= 8;
			ulong num3 = num2 * 256 + (x & 0xFF);
			x >>= 8;
			ulong num4 = num3 * 256 + (x & 0xFF);
			x >>= 8;
			ulong num5 = num4 * 256 + (x & 0xFF);
			x >>= 8;
			ulong num6 = num5 * 256 + (x & 0xFF);
			x >>= 8;
			ulong num7 = num6 * 256 + (x & 0xFF);
			x >>= 8;
			return num7 * 256 + (x & 0xFF);
		}

		public static string ReadNullTerminatedString(BinaryReader r)
		{
			char[] array = new char[256];
			int num = 0;
			byte b;
			while ((b = r.ReadByte()) != 0 && r.PeekChar() != -1)
			{
				array[num] = (char)b;
				num++;
				if (num == 256)
				{
					return null;
				}
			}
			return new string(array, 0, num);
		}

		public static string ReadNullTerminatedByteArray(BinaryReader r, int length)
		{
			char[] array = new char[length];
			int num = 0;
			for (int i = 0; i < length; i++)
			{
				if (r.PeekChar() == -1)
				{
					num = i;
					break;
				}
				byte b;
				if ((b = r.ReadByte()) == 0 && num == 0)
				{
					num = i;
				}
				array[i] = (char)b;
			}
			return new string(array, 0, num);
		}

		public static string ReadNullTerminatedString(BinaryReader r, int padding)
		{
			string text = ReadNullTerminatedString(r);
			int num = (text.Length + 1) % padding;
			if (num != 0)
			{
				r.ReadBytes(padding - num);
			}
			return text;
		}

		public static string ReadNullPaddedString(BinaryReader r, int length)
		{
			byte[] array = r.ReadBytes(length);
			int num = 0;
			for (num = 0; num < length && array[num] != 0; num++)
			{
			}
			if (num == 0)
			{
				return string.Empty;
			}
			return ue.GetString(array, 0, num);
		}

		public static void WriteNullPaddedString(BinaryWriter w, string str, int length)
		{
			if (str == null)
			{
				str = string.Empty;
			}
			byte[] bytes = ue.GetBytes(str);
			if (bytes.Length > length)
			{
				w.Write(bytes, 0, length);
				return;
			}
			w.Write(bytes);
			for (int i = bytes.Length; i < length; i++)
			{
				w.Write((byte)0);
			}
		}

		public static int ComputeAlignement(int v)
		{
			int num = 1;
			if (v == 0)
			{
				return 1;
			}
			for (int i = 0; i < 31; i++)
			{
				if ((v & num) != 0)
				{
					return (num + 1) / 2;
				}
				num = num * 2 + 1;
			}
			return 0;
		}

		public static int ComputeAlignementLong(long v)
		{
			int num = 1;
			if (v == 0L)
			{
				return 1;
			}
			for (int i = 0; i < 63; i++)
			{
				if ((v & num) != 0L)
				{
					return (num + 1) / 2;
				}
				num = num * 2 + 1;
			}
			return 0;
		}

		public static void WriteNullTerminatedString(BinaryWriter w, string s)
		{
			char[] array = s.ToCharArray(0, s.Length);
			for (int i = 0; i < array.Length; i++)
			{
				w.Write((byte)array[i]);
			}
			w.Write((byte)0);
		}

		public static void WriteNullTerminatedByteArray(BinaryWriter w, string s, int nBytes)
		{
			char[] array = s.ToCharArray(0, s.Length);
			for (int i = 0; i < nBytes; i++)
			{
				if (i < array.Length)
				{
					w.Write((byte)array[i]);
				}
				else
				{
					w.Write((byte)0);
				}
			}
		}

		public static int ComputeHash(string fileName)
		{
			int num = 4700322;
			int num2 = 0;
			char[] array = fileName.ToCharArray(0, fileName.Length);
			int num3 = array.Length;
			for (num2 = 0; num2 < num3; num2++)
			{
				num += array[num2];
				num *= 33;
			}
			return num;
		}

		public static int ComputeBucket(int hash, string extension)
		{
			extension.ToLower();
			int num;
			if (extension.Equals(".fsh"))
			{
				num = 0;
			}
			else if (extension.Equals(".jdi"))
			{
				num = 32;
			}
			else if (extension.Equals(".ini"))
			{
				num = 32;
			}
			else if (extension.Equals(".tvb"))
			{
				num = 64;
			}
			else if (extension.Equals(".irr"))
			{
				num = 64;
			}
			else if (extension.Equals(".loc"))
			{
				num = 96;
			}
			else if (extension.Equals(".cs"))
			{
				num = 96;
			}
			else if (extension.Equals(".shd"))
			{
				num = 128;
			}
			else if (extension.Equals(".txt"))
			{
				num = 128;
			}
			else if (extension.Equals(".dat"))
			{
				num = 128;
			}
			else if (extension.Equals(".hud"))
			{
				num = 128;
			}
			else if (extension.Equals(".ttf"))
			{
				num = 192;
			}
			else if (extension.Equals(".bin"))
			{
				num = 192;
			}
			else if (extension.Equals(".skn"))
			{
				num = 192;
			}
			else if (extension.Equals(".o"))
			{
				num = 224;
			}
			else if (extension.Equals(".big"))
			{
				num = 224;
			}
			else
			{
				if (!extension.Equals(".ebo"))
				{
					return 0;
				}
				num = 224;
			}
			return ((33 * hash + num) % 256) & 0xFF;
		}

		public static ulong ComputeBhHash(byte[] name, int length)
		{
			ulong num = 5381uL;
			for (int i = 0; i < length; i++)
			{
				int num2 = name[i];
				num = (num << 5) + num + (uint)num2;
			}
			return num;
		}

		public static ulong ComputeBhHash(string name)
		{
			ulong num = 5381uL;
			foreach (int num2 in name)
			{
				num = (num << 5) + num + (uint)num2;
			}
			return num;
		}

		public static int ComputeCrcDb11(byte[] bytes)
		{
			int num = -1;
			for (uint num2 = 0u; num2 < bytes.Length; num2++)
			{
				int num3 = 7;
				num ^= bytes[num2] << 24;
				do
				{
					if (num >= 0)
					{
						num *= 2;
					}
					else
					{
						num *= 2;
						num ^= 0x4C11DB7;
					}
					num3--;
				}
				while (num3 >= 0);
			}
			return num;
		}

		public static int ComputeCrcDb11(string text)
		{
			return ComputeCrcDb11(ue.GetBytes(text));
		}

		public static uint ComputeLanguageHash(string name)
		{
			byte[] bytes = ue.GetBytes(name);
			return EAHash(bytes, bytes.Length);
		}

		public static bool TryAllaCrc32(byte[] bytes, uint expected)
		{
			int num = bytes.Length;
			uint num2 = SwapEndian(expected);
			uint num3 = sdbm(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = RSHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = JSHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = PJWHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = ELFHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = BKDRHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = SDBMHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = DJBHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = DEKHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = BPHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = FNVHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = APHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = adler32(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = fletcher32(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = jenkins_one_at_a_time_hash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = (uint)ComputeBhHash(bytes, num);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			num3 = (uint)ComputeCrcDb11(bytes);
			if (num3 == expected || num3 == num2)
			{
				return true;
			}
			return false;
		}

		private static uint sdbm(byte[] str, int length)
		{
			uint num = 0u;
			for (int i = 0; i < length; i++)
			{
				byte num2 = str[i];
				uint num3 = num << 6;
				num3 += num << 16;
				num3 -= num;
				num = num2 + num3;
			}
			return num;
		}

		private static uint fletcher32(byte[] str, int len)
		{
			uint num = 0u;
			uint num2 = 0u;
			for (int i = 0; i < len; i++)
			{
				num += str[i];
				if (num >= 65535)
				{
					num -= 65535;
				}
				num2 += num;
				if (num2 >= 65535)
				{
					num2 -= 65535;
				}
			}
			return (num2 << 16) | num;
		}

		private static uint EAHash(byte[] str, int length)
		{
			uint num = 0u;
			for (int i = 0; i < length; i++)
			{
				uint num2 = str[i];
				num2 &= 0xDFu;
				num2 ^= num;
				num2 &= 0xFFu;
				num >>= 8;
				num ^= c_LanguageHashtable[num2];
			}
			return num ^ 0x80000000u;
		}

		private static uint RSHash(byte[] str, int length)
		{
			uint num = 378551u;
			uint num2 = 63689u;
			uint num3 = 0u;
			for (int i = 0; i < length; i++)
			{
				num3 = num3 * num2 + str[i];
				num2 *= num;
			}
			return num3;
		}

		private static uint JSHash(byte[] str, int length)
		{
			uint num = 1315423911u;
			for (int i = 0; i < length; i++)
			{
				num ^= (num << 5) + str[i] + (num >> 2);
			}
			return num;
		}

		private static uint PJWHash(byte[] str, int length)
		{
			uint num = 32u;
			uint num2 = num * 3 / 4u;
			uint num3 = num / 8u;
			uint num4 = (uint)(-1 << (int)(num - num3));
			uint num5 = 0u;
			uint num6 = 0u;
			for (int i = 0; i < length; i++)
			{
				num5 = (num5 << (int)num3) + str[i];
				if ((num6 = num5 & num4) != 0)
				{
					num5 = (num5 ^ (num6 >> (int)num2)) & ~num4;
				}
			}
			return num5;
		}

		private static uint ELFHash(byte[] str, int length)
		{
			uint num = 0u;
			uint num2 = 0u;
			for (int i = 0; i < length; i++)
			{
				num = (num << 4) + str[i];
				if ((num2 = num & 0xF0000000u) != 0)
				{
					num ^= num2 >> 24;
				}
				num &= ~num2;
			}
			return num;
		}

		private static uint BKDRHash(byte[] str, int length)
		{
			uint num = 131u;
			uint num2 = 0u;
			for (int i = 0; i < length; i++)
			{
				num2 = num2 * num + str[i];
			}
			return num2;
		}

		private static uint SDBMHash(byte[] str, int length)
		{
			uint num = 0u;
			for (int i = 0; i < length; i++)
			{
				num = str[i] + (num << 6) + (num << 16) - num;
			}
			return num;
		}

		private static uint DJBHash(byte[] str, int length)
		{
			uint num = 5381u;
			for (int i = 0; i < length; i++)
			{
				num = (num << 5) + num + str[i];
			}
			return num;
		}

		private static uint DEKHash(byte[] str, int length)
		{
			uint num = (uint)length;
			for (int i = 0; i < length; i++)
			{
				num = (num << 5) ^ (num >> 27) ^ str[i];
			}
			return num;
		}

		private static uint BPHash(byte[] str, int length)
		{
			uint num = 0u;
			for (int i = 0; i < length; i++)
			{
				num = (num << 7) ^ str[i];
			}
			return num;
		}

		private static uint FNVHash(byte[] str, int length)
		{
			uint num = 0u;
			num = 2166136261u;
			for (int i = 0; i < length; i++)
			{
				byte b = str[i];
				if (b >= 65 && b <= 90)
				{
					b = (byte)(b + 32);
				}
				num ^= b;
				num *= 16777619;
			}
			return num;
		}

		private static uint APHash(byte[] str, int length)
		{
			uint num = 2863311530u;
			for (int i = 0; i < length; i++)
			{
				num ^= (((i & 1) == 0) ? ((num << 7) ^ (str[i] * (num >> 3))) : (~((num << 11) + (str[i] ^ (num >> 5)))));
			}
			return num;
		}

		private static uint adler32(byte[] str, int length)
		{
			ulong num = 1uL;
			ulong num2 = 0uL;
			for (int i = 0; i < length; i++)
			{
				num = (num + str[i]) % 65521uL;
				num2 = (num2 + num) % 65521uL;
			}
			return (uint)((num2 << 16) | num);
		}

		private static uint jenkins_one_at_a_time_hash(byte[] str, int length)
		{
			uint num;
			uint num2 = (num = 0u);
			for (; num < length; num++)
			{
				num2 += str[num];
				num2 += num2 << 10;
				num2 ^= num2 >> 6;
			}
			num2 += num2 << 3;
			num2 ^= num2 >> 11;
			return num2 + (num2 << 15);
		}

		private static uint MurmurHash2(byte[] str, int length)
		{
			uint num = (uint)length;
			int num2 = 0;
			while (length >= 4)
			{
				uint num3 = str[num2];
				num3 = num3 * 256 + str[num2 + 1];
				num3 = num3 * 256 + str[num2 + 2];
				num3 = num3 * 256 + str[num2 + 3];
				num3 *= 1540483477;
				num3 ^= num3 >> 24;
				num3 *= 1540483477;
				num *= 1540483477;
				num ^= num3;
				num2 += 4;
				length -= 4;
			}
			if (length == 3)
			{
				num ^= (uint)(str[num2 + 2] << 16);
			}
			if (length >= 2)
			{
				num ^= (uint)(str[num2 + 1] << 8);
			}
			num ^= str[num2];
			num *= 1540483477;
			num ^= num >> 13;
			num *= 1540483477;
			return num ^ (num >> 15);
		}

		public static string ReadString(BinaryReader r, int offset)
		{
			long position = r.BaseStream.Position;
			r.BaseStream.Position = offset;
			int count = r.ReadInt16();
			string @string = ue.GetString(r.ReadBytes(count));
			r.BaseStream.Position = position;
			return @string;
		}

		public static string ReadStringAndMove(BinaryReader r)
		{
			int count = r.ReadInt16();
			return ue.GetString(r.ReadBytes(count));
		}

		public static string ConvertBytesToString(byte[] bytes)
		{
			return ue.GetString(bytes);
		}

		public static byte[] ConvertStringToBytes(string str)
		{
			return ue.GetBytes(str);
		}

		public static string ReadString(BinaryReader r, long offset, int length)
		{
			long position = r.BaseStream.Position;
			r.BaseStream.Position = offset;
			string @string = ue.GetString(r.ReadBytes(length));
			r.BaseStream.Position = position;
			return @string;
		}

		public static int WriteString(BinaryWriter w, int offset, string s)
		{
			long position = w.BaseStream.Position;
			w.BaseStream.Position = offset;
			if (s == null)
			{
				s = " ";
			}
			short num = (short)ue.GetByteCount(s);
			int num2 = num + 2;
			w.Write(num);
			w.Write(ue.GetBytes(s));
			if (((uint)num2 & 3u) != 0)
			{
				int num3 = 4 - (num2 & 3);
				for (int i = 0; i < num3; i++)
				{
					w.Write((byte)0);
				}
			}
			int result = (int)w.BaseStream.Position;
			w.BaseStream.Position = position;
			return result;
		}

		public static int StringSize(string s)
		{
			return RoundUp4((short)ue.GetByteCount(s) + 2);
		}

		public static int ComputeBitUsed(uint range)
		{
			if (range == 0)
			{
				return 1;
			}
			for (int num = 32; num > 0; num--)
			{
				uint num2 = (uint)(1 << num - 1);
				if ((range & num2) != 0)
				{
					return num;
				}
			}
			return 0;
		}

		public static int RoundUp4(int v)
		{
			return (v + 3) & -4;
		}

		public static int RoundUp(int v, int align)
		{
			return (v + (align - 1)) & ~(align - 1);
		}

		public static long RoundUp(long v, int align)
		{
			return (v + (align - 1)) & ~(align - 1);
		}

		public static bool CompareWildcardString(string pattern, string target)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			pattern = pattern.ToLower();
			target = target.ToLower();
			for (num2 = 0; num2 < target.Length; num2++)
			{
				if (num >= pattern.Length)
				{
					break;
				}
				switch (pattern[num])
				{
				case '?':
					num++;
					continue;
				case '*':
					if (num == pattern.Length - 1)
					{
						return true;
					}
					num++;
					num3 = 1;
					continue;
				case '/':
				case '\\':
					if (num3 == 0)
					{
						if ('\\' != target[num2] && '/' != target[num2])
						{
							return false;
						}
						num++;
					}
					else if ('\\' == target[num2] || '/' == target[num2])
					{
						num3 = 0;
						num++;
					}
					continue;
				}
				if (num3 == 0)
				{
					if (pattern[num] != target[num2])
					{
						return false;
					}
					num++;
				}
				else if (pattern[num] == target[num2])
				{
					num3 = 0;
					num++;
				}
			}
			if (num2 == target.Length && num == pattern.Length)
			{
				return true;
			}
			return false;
		}

		public static DateTime ConvertToDate(int gregorian)
		{
			DateTime result = new DateTime(1582, 10, 14, 12, 0, 0);
			if (gregorian < 0)
			{
				return result;
			}
			return result.AddDays(gregorian);
		}

		public static int ConvertFromDate(DateTime date)
		{
			DateTime dateTime = new DateTime(1582, 10, 14, 0, 0, 0);
			return (date - dateTime).Days;
		}

		public static string PadBlanks(string s, int len)
		{
			if (len > s.Length)
			{
				int num = len - s.Length;
				for (int i = 0; i < num; i++)
				{
					s = " " + s;
				}
			}
			return s;
		}

		public static int Limit(int val, int min, int max)
		{
			if (val < min)
			{
				return min;
			}
			if (val > max)
			{
				return max;
			}
			return val;
		}

		public static float ConvertToFloat(short float16Bit)
		{
			int num = (((float16Bit & 0x8000) == 0) ? 1 : (-1));
			int num2 = (float16Bit & 0x7C00) >> 10;
			int num3 = (float16Bit & 0x3FF) + 1024;
			if (num2 == 0 && num3 == 0)
			{
				return 0f;
			}
			if (num2 == 31)
			{
				return float.NaN;
			}
			num2 -= 15;
			float num4 = (float)Math.Pow(2.0, num2);
			return (float)(num * num3) / 1024f * num4;
		}

		public static ushort ConvertFloat16ToShort(float f)
		{
			if (f == 0f)
			{
				return 0;
			}
			int num = 1;
			if (f < 0f)
			{
				f = 0f - f;
				num = -1;
			}
			float num2 = f * 32768f;
			int num3 = 0;
			while ((double)num2 >= 2.0)
			{
				num2 /= 2f;
				num3++;
			}
			ushort num4 = 0;
			if (num < 0)
			{
				num4 = 32768;
			}
			if (num3 > 31)
			{
				num3 = 31;
			}
			num4 = (ushort)(num4 | (num3 << 10));
			ushort num5 = (ushort)((!(num2 >= 1f)) ? 1 : Convert.ToUInt16((double)(num2 - 1f) * 1024.0));
			return (ushort)(num4 | num5);
		}

		public static float SwapAndConvertToFloat(BinaryReader r)
		{
			byte[] array = new byte[4];
			array[3] = r.ReadByte();
			array[2] = r.ReadByte();
			array[1] = r.ReadByte();
			array[0] = r.ReadByte();
			MemoryStream memoryStream = new MemoryStream(array);
			float result = new BinaryReader(memoryStream).ReadSingle();
			memoryStream.Close();
			return result;
		}

		public static void SwapAndWriteFloat(BinaryWriter w, float f)
		{
			byte[] array = new byte[4];
			MemoryStream memoryStream = new MemoryStream(array);
			new BinaryWriter(memoryStream).Write(f);
			memoryStream.Close();
			w.Write(array[3]);
			w.Write(array[2]);
			w.Write(array[1]);
			w.Write(array[0]);
		}

		public static bool IsFileLocked(string filePath)
		{
			try
			{
				using (File.Open(filePath, FileMode.Open))
				{
				}
			}
			catch (IOException e)
			{
				int num = Marshal.GetHRForException(e) & 0xFFFF;
				return num == 32 || num == 33;
			}
			return false;
		}
	}
}
