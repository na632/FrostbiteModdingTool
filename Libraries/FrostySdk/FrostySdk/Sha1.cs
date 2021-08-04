using System;
using System.Globalization;
using System.Windows.Markup;

namespace FrostySdk
{
	public struct Sha1
	{
		public static readonly Sha1 Zero;

		private uint a;

		private uint b;

		private uint c;

		private uint d;

		private uint e;

		public Sha1(byte[] bytes)
		{
			if (bytes.Length < 20)
			{
				throw new ArgumentException("Input buffer is too small");
			}
			a = (uint)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24));
			b = (uint)(bytes[4] | (bytes[5] << 8) | (bytes[6] << 16) | (bytes[7] << 24));
			c = (uint)(bytes[8] | (bytes[9] << 8) | (bytes[10] << 16) | (bytes[11] << 24));
			d = (uint)(bytes[12] | (bytes[13] << 8) | (bytes[14] << 16) | (bytes[15] << 24));
			e = (uint)(bytes[16] | (bytes[17] << 8) | (bytes[18] << 16) | (bytes[19] << 24));
		}

		public Sha1(string text)
		{
			byte[] array = new byte[text.Length / 2];
			for (int i = 0; i < text.Length; i += 2)
			{
				array[i / 2] = byte.Parse(text.Substring(i, 2), NumberStyles.AllowHexSpecifier);
			}
			if (array.Length < 20)
			{
				throw new ArgumentException("Input buffer is too small");
			}
			a = (uint)(array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24));
			b = (uint)(array[4] | (array[5] << 8) | (array[6] << 16) | (array[7] << 24));
			c = (uint)(array[8] | (array[9] << 8) | (array[10] << 16) | (array[11] << 24));
			d = (uint)(array[12] | (array[13] << 8) | (array[14] << 16) | (array[15] << 24));
			e = (uint)(array[16] | (array[17] << 8) | (array[18] << 16) | (array[19] << 24));
		}

		public static bool operator ==(Sha1 A, Sha1 B)
		{
			if ((object)A != (object)B)
			{
				if ((object)A != (object)B)
				{
					return A.Equals(B);
				}
				return false;
			}
			return true;
		}

		public static bool operator !=(Sha1 A, Sha1 B)
		{
			return !(A == B);
		}

		public byte[] ToByteArray()
		{
			return new byte[20]
			{
				(byte)(a & 0xFF),
				(byte)((a >> 8) & 0xFF),
				(byte)((a >> 16) & 0xFF),
				(byte)((a >> 24) & 0xFF),
				(byte)(b & 0xFF),
				(byte)((b >> 8) & 0xFF),
				(byte)((b >> 16) & 0xFF),
				(byte)((b >> 24) & 0xFF),
				(byte)(c & 0xFF),
				(byte)((c >> 8) & 0xFF),
				(byte)((c >> 16) & 0xFF),
				(byte)((c >> 24) & 0xFF),
				(byte)(d & 0xFF),
				(byte)((d >> 8) & 0xFF),
				(byte)((d >> 16) & 0xFF),
				(byte)((d >> 24) & 0xFF),
				(byte)(e & 0xFF),
				(byte)((e >> 8) & 0xFF),
				(byte)((e >> 16) & 0xFF),
				(byte)((e >> 24) & 0xFF)
			};
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != typeof(Sha1))
			{
				return false;
			}
			Sha1 sha = (Sha1)obj;
			if (a != sha.a)
			{
				return false;
			}
			if (b != sha.b)
			{
				return false;
			}
			if (c != sha.c)
			{
				return false;
			}
			if (d != sha.d)
			{
				return false;
			}
			if (e != sha.e)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
            return unchecked(((((((((-2128831035 * 16777619) ^ a.GetHashCode()) * 16777619) ^ b.GetHashCode()) * 16777619) ^ c.GetHashCode()) * 16777619) ^ d.GetHashCode()) * 16777619) ^ e.GetHashCode();
            //return Convert.ToInt32(((((((((-21288 * 16777) ^ a.GetHashCode()) * 16777619) ^ b.GetHashCode()) * 16777619) ^ c.GetHashCode()) * 16777619) ^ d.GetHashCode()) * 16777619) ^ e.GetHashCode();
        }

		static Random RandomByteCreator = new Random();

		public static Sha1 Create()
		{
			byte[] data = new byte[256];
			RandomByteCreator.NextBytes(data);
			return Create(data);
		}

		public static Sha1 Create(byte[] data)
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			var cryptSha1 = System.Security.Cryptography.SHA1.Create();
			var hashed = cryptSha1.ComputeHash(data);

			// THIS IS NOT USING .NET 5. May not work!!!!
			return new Sha1(hashed);
		}

		public override string ToString()
		{
			string text = "";
			uint[] array = new uint[5]
			{
				a,
				b,
				c,
				d,
				e
			};
			for (int i = 0; i < 5; i++)
			{
				text = text + ((byte)(array[i] & 0xFF)).ToString("x2") + ((byte)(array[i] >> 8)).ToString("x2") + ((byte)(array[i] >> 16)).ToString("x2") + ((byte)(array[i] >> 24)).ToString("x2");
			}
			return text;
		}
	}
}
