using System;

namespace FrostySdk.Ebx
{
	public struct CString
	{
		private string strValue;

		public CString(string value = "")
		{
			strValue = value;
		}

		public CString Sanitize()
		{
			return new CString(strValue.Trim('\v', '\r', '\n', '\t'));
		}

		public static implicit operator string(CString value)
		{
			if (value.strValue == null)
			{
				value.strValue = "";
			}
			return value.strValue;
		}

		public static implicit operator CString(string value)
		{
			return new CString(value);
		}

		public bool IsNull()
		{
			return strValue == null;
		}

		public override string ToString()
		{
			return strValue;
		}

		public override bool Equals(object obj)
		{
			if (obj is CString)
			{
				CString cString = (CString)obj;
				if (strValue == null)
				{
					if (cString.strValue != null && cString.strValue != "")
					{
						return false;
					}
					return true;
				}
				if (cString.strValue == null)
				{
					if (strValue != null && strValue != "")
					{
						return false;
					}
					return true;
				}
				return strValue.Equals(cString.strValue);
			}
			if (obj is string)
			{
				string text = (string)obj;
				if (strValue == null)
				{
					if (text != null && text != "")
					{
						return false;
					}
					return true;
				}
				if (text == null)
				{
					if (strValue != null && strValue != "")
					{
						return false;
					}
					return true;
				}
				return strValue.Equals(text);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return strValue.GetHashCode();
		}

		public bool Equals(object obj, StringComparison comparison)
		{
			if (obj is CString)
			{
				CString cString = (CString)obj;
				if (strValue == null)
				{
					if (cString.strValue != null && cString.strValue != "")
					{
						return false;
					}
					return true;
				}
				if (cString.strValue == null)
				{
					if (strValue != null && strValue != "")
					{
						return false;
					}
					return true;
				}
				return strValue.Equals(cString.strValue, comparison);
			}
			if (obj is string)
			{
				string text = (string)obj;
				if (strValue == null)
				{
					if (text != null && text != "")
					{
						return false;
					}
					return true;
				}
				if (text == null)
				{
					if (strValue != null && strValue != "")
					{
						return false;
					}
					return true;
				}
				return strValue.Equals(text, comparison);
			}
			return false;
		}
	}
}
