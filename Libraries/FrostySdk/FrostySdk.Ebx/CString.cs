using System;

namespace FrostySdk.Ebx
{
    public struct CString
    {
        public string StrValue { get; set; }

        public CString(string value = "")
        {
            StrValue = value;
        }

        public CString Sanitize()
        {
            return new CString(StrValue.Trim('\v', '\r', '\n', '\t'));
        }

        public static implicit operator string(CString value)
        {
            if (value.StrValue == null)
            {
                value.StrValue = "";
            }
            return value.StrValue;
        }

        public static implicit operator CString(string value)
        {
            return new CString(value);
        }

        public bool IsNull()
        {
            return StrValue == null;
        }

        public override string ToString()
        {
            return StrValue;
        }

        public override bool Equals(object obj)
        {
            if (obj is CString)
            {
                CString cString = (CString)obj;
                if (StrValue == null)
                {
                    if (cString.StrValue != null && cString.StrValue != "")
                    {
                        return false;
                    }
                    return true;
                }
                if (cString.StrValue == null)
                {
                    if (StrValue != null && StrValue != "")
                    {
                        return false;
                    }
                    return true;
                }
                return StrValue.Equals(cString.StrValue);
            }
            if (obj is string)
            {
                string text = (string)obj;
                if (StrValue == null)
                {
                    if (text != null && text != "")
                    {
                        return false;
                    }
                    return true;
                }
                if (text == null)
                {
                    if (StrValue != null && StrValue != "")
                    {
                        return false;
                    }
                    return true;
                }
                return StrValue.Equals(text);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return StrValue.GetHashCode();
        }

        public bool Equals(object obj, StringComparison comparison)
        {
            if (obj is CString)
            {
                CString cString = (CString)obj;
                if (StrValue == null)
                {
                    if (cString.StrValue != null && cString.StrValue != "")
                    {
                        return false;
                    }
                    return true;
                }
                if (cString.StrValue == null)
                {
                    if (StrValue != null && StrValue != "")
                    {
                        return false;
                    }
                    return true;
                }
                return StrValue.Equals(cString.StrValue, comparison);
            }
            if (obj is string)
            {
                string text = (string)obj;
                if (StrValue == null)
                {
                    if (text != null && text != "")
                    {
                        return false;
                    }
                    return true;
                }
                if (text == null)
                {
                    if (StrValue != null && StrValue != "")
                    {
                        return false;
                    }
                    return true;
                }
                return StrValue.Equals(text, comparison);
            }
            return false;
        }
    }
}
