using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostySdk.IO
{
	[StructLayout(LayoutKind.Sequential, Size = 4)]
	public struct FourCC : IEquatable<FourCC>, IFormattable
	{
		public static readonly FourCC Empty = new FourCC(0);

		private uint value;

		public FourCC(string fourCC)
		{
			if (fourCC.Length != 4)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid length for FourCC(\"{0}\". Must be be 4 characters long ", new object[1] { fourCC }), "fourCC");
			}
			value = ((uint)fourCC[3] << 24) | ((uint)fourCC[2] << 16) | ((uint)fourCC[1] << 8) | fourCC[0];
		}

		public FourCC(char byte1, char byte2, char byte3, char byte4)
		{
			value = ((uint)byte4 << 24) | ((uint)byte3 << 16) | ((uint)byte2 << 8) | byte1;
		}

		public FourCC(uint fourCC)
		{
			value = fourCC;
		}

		public FourCC(int fourCC)
		{
			value = (uint)fourCC;
		}

		public static implicit operator uint(FourCC d)
		{
			return d.value;
		}

		public static implicit operator int(FourCC d)
		{
			return (int)d.value;
		}

		public static implicit operator FourCC(uint d)
		{
			return new FourCC(d);
		}

		public static implicit operator FourCC(int d)
		{
			return new FourCC(d);
		}

		public static implicit operator string(FourCC d)
		{
			return d.ToString();
		}

		public static implicit operator FourCC(string d)
		{
			return new FourCC(d);
		}

		public override string ToString()
		{
			return $"{new string(new char[4] { (char)(value & 0xFFu), (char)((value >> 8) & 0xFFu),(char)((value >> 16) & 0xFFu),(char)((value >> 24) & 0xFFu)})}";
		}

		public bool Equals(FourCC other)
		{
			return value == other.value;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is FourCC)
			{
				return Equals((FourCC)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)value;
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (string.IsNullOrEmpty(format))
			{
				format = "G";
			}
			if (formatProvider == null)
			{
				formatProvider = CultureInfo.CurrentCulture;
			}
			string text = format.ToUpperInvariant();
			if (!(text == "G"))
			{
				if (text == "I")
				{
					return value.ToString("X08", formatProvider);
				}
				return value.ToString(format, formatProvider);
			}
			return ToString();
		}

		public static bool operator ==(FourCC left, FourCC right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(FourCC left, FourCC right)
		{
			return !left.Equals(right);
		}
	}
}