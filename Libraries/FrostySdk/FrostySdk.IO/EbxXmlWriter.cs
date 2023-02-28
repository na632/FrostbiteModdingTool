using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace FrostySdk.IO
{
    public class EbxXmlWriter : IDisposable
    {
        internal class PropertyComparer : IComparer<PropertyInfo>
        {
            public int Compare(PropertyInfo x, PropertyInfo y)
            {
                if (x.MetadataToken >= y.MetadataToken)
                {
                    return 1;
                }
                return -1;
            }
        }

        private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public;

        private AssetManager am;

        private List<object> objs = new List<object>();

        private Stream stream;

        public EbxXmlWriter(Stream inStream, AssetManager inAm)
        {
            am = inAm;
            stream = inStream;
        }

        public void WriteObjects(IEnumerable<object> inObjs)
        {
            objs.Clear();
            objs.AddRange(inObjs);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (object obj in objs)
            {
                stringBuilder.Append(ClassToXml(obj, obj.GetType()));
            }
            string s = stringBuilder.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            stream.Write(bytes, 0, bytes.Length);
        }

        private string ClassToXml(object Obj, Type ObjType, int TabCount = 0)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num = ObjType.GetProperties().Length;
            PropertyInfo[] properties = ObjType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            Array.Sort(properties, new PropertyComparer());
            string text = "";
            FieldInfo field = ObjType.GetField("__Guid", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
            {
                text = " Guid=\"" + ((AssetClassGuid)field.GetValue(Obj)).ToString() + "\"";
            }
            if (num != 0 && (properties.Length != 0 || (ObjType.BaseType != typeof(object) && ObjType.BaseType != typeof(ValueType))))
            {
                stringBuilder.AppendLine("".PadLeft(TabCount) + "<" + ObjType.Name + text + ">");
                TabCount += 4;
                PropertyInfo[] array = properties;
                foreach (PropertyInfo propertyInfo in array)
                {
                    if (propertyInfo.GetCustomAttribute<IsTransientAttribute>() == null)
                    {
                        stringBuilder.Append("".PadLeft(TabCount) + "<" + propertyInfo.Name + "[AddInfo]>");
                        object value = propertyInfo.GetValue(Obj);
                        string AdditionalInfo = "";
                        stringBuilder.Append(FieldToXml(value, ref AdditionalInfo, TabCount));
                        stringBuilder.AppendLine("</" + propertyInfo.Name + ">");
                        stringBuilder = stringBuilder.Replace("[AddInfo]", AdditionalInfo);
                    }
                }
                TabCount -= 4;
                stringBuilder.AppendLine("".PadLeft(TabCount) + "</" + ObjType.Name + ">");
            }
            else
            {
                stringBuilder.AppendLine("".PadLeft(TabCount) + "<" + ObjType.Name + text + "/>");
            }
            return stringBuilder.ToString();
        }

        private string FieldToXml(object Value, ref string AdditionalInfo, int TabCount = 0)
        {
            Type type = Value.GetType();
            StringBuilder stringBuilder = new StringBuilder();
            if (type.Name == "List`1")
            {
                int num = (int)type.GetMethod("get_Count").Invoke(Value, null);
                AdditionalInfo = " Count=\"" + num + "\"";
                if (num > 0)
                {
                    stringBuilder.AppendLine();
                    TabCount += 4;
                    for (int i = 0; i < num; i++)
                    {
                        stringBuilder.Append("".PadLeft(TabCount) + "<member Index=\"" + i.ToString() + "\">");
                        object value = type.GetMethod("get_Item").Invoke(Value, new object[1]
                        {
                            i
                        });
                        string AdditionalInfo2 = "";
                        stringBuilder.Append(FieldToXml(value, ref AdditionalInfo2, TabCount));
                        stringBuilder.AppendLine("</member>");
                    }
                    TabCount -= 4;
                    stringBuilder.Append("".PadLeft(TabCount));
                }
            }
            else if (type.Namespace == "FrostySdk.Ebx" && type.BaseType != typeof(Enum))
            {
                if (type == typeof(CString))
                {
                    stringBuilder.Append(Value.ToString());
                }
                else if (type == typeof(ResourceRef))
                {
                    stringBuilder.Append(Value.ToString());
                }
                else if (type == typeof(FileRef))
                {
                    stringBuilder.Append(Value.ToString());
                }
                else if (type == typeof(TypeRef))
                {
                    stringBuilder.Append(Value.ToString());
                }
                else if (type == typeof(BoxedValueRef))
                {
                    stringBuilder.Append(Value.ToString());
                }
                else if (type == typeof(PointerRef))
                {
                    PointerRef pointerRef = (PointerRef)Value;
                    if (pointerRef.Type == PointerRefType.Internal)
                    {
                        Type type2 = pointerRef.Internal.GetType();
                        stringBuilder.Append(string.Concat(str3: ((AssetClassGuid)type2.GetField("__Guid", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pointerRef.Internal)).ToString(), str0: "[", str1: type2.Name, str2: "] "));
                    }
                    else if (pointerRef.Type == PointerRefType.External)
                    {
                        //EbxAssetEntry ebxEntry = am.GetEbxEntry(pointerRef.External.FileGuid);
                        //if (ebxEntry != null)
                        //{
                        //	stringBuilder.Append("[Ebx] " + ebxEntry.Name + " [" + pointerRef.External.ClassGuid + "]");
                        //}
                        //else
                        //{
                        //	stringBuilder.Append("[Ebx] BadRef " + pointerRef.External.FileGuid + "/" + pointerRef.External.ClassGuid);
                        //}
                    }
                    else
                    {
                        stringBuilder.Append("nullptr");
                    }
                }
                else
                {
                    TabCount += 4;
                    stringBuilder.AppendLine();
                    stringBuilder.Append(ClassToXml(Value, Value.GetType(), TabCount));
                    TabCount -= 4;
                    stringBuilder.Append("".PadLeft(TabCount));
                }
            }
            else if (type == typeof(byte))
            {
                stringBuilder.Append(((byte)Value).ToString("X2"));
            }
            else if (type == typeof(ushort))
            {
                stringBuilder.Append(((ushort)Value).ToString("X4"));
            }
            else if (type == typeof(uint))
            {
                uint hash = (uint)Value;
                string @string = Utils.GetString((int)hash);
                if (!@string.StartsWith("0x"))
                {
                    stringBuilder.Append(@string + " [" + hash.ToString("X8") + "]");
                }
                else
                {
                    stringBuilder.Append(@string);
                }
            }
            else if (type == typeof(int))
            {
                int hash2 = (int)Value;
                string string2 = Utils.GetString(hash2);
                if (!string2.StartsWith("0x"))
                {
                    stringBuilder.Append(string2 + " [" + hash2.ToString("X8") + "]");
                }
                else
                {
                    stringBuilder.Append(string2);
                }
            }
            else if (type == typeof(ulong))
            {
                stringBuilder.Append(((ulong)Value).ToString("X16"));
            }
            else if (type == typeof(float))
            {
                stringBuilder.Append(((float)Value).ToString());
            }
            else if (type == typeof(double))
            {
                stringBuilder.Append(((double)Value).ToString());
            }
            else
            {
                stringBuilder.Append(Value.ToString());
            }
            return stringBuilder.ToString();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Stream stream = this.stream;
                this.stream = null;
                stream?.Close();
            }
            this.stream = null;
        }
    }
}
