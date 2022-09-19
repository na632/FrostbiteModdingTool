using FrostySdk.IO;
using System;
using System.Runtime.CompilerServices;

namespace FrostySdk.Ebx
{
	public class BoxedValueRef
	{
        private object value;

        public object Value
        {
            get { return value; }
            set { this.value = value; }
        }

        private byte[] data;

        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

		private EbxFieldType type;

		public EbxFieldType Type => type;

		private EbxFieldType subType;

		public BoxedValueRef()
		{
		}

		public BoxedValueRef(object inval, EbxFieldType intype)
		{
			value = inval;
			type = intype;
		}

		public BoxedValueRef(object inval, EbxFieldType intype, EbxFieldType insubtype)
		{
			this.Value = inval;
			this.type = intype;
			this.subType = insubtype;
		}


		public override string ToString()
		{
			if (this.Value == null)
			{
				return "BoxedValueRef '(null)'";
			}
			string text;
			switch (this.Type)
			{
				case EbxFieldType.Array:
					text = "Array<" + this.EbxTypeToString(this.subType, this.Value.GetType().GenericTypeArguments[0]) + ">";
					break;
				case EbxFieldType.Enum:
					text = this.Value.GetType().Name;
					break;
				case EbxFieldType.Struct:
					text = this.Value.GetType().Name;
					break;
				case EbxFieldType.CString:
					text = "CString";
					break;
				default:
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
						defaultInterpolatedStringHandler.AppendFormatted(this.Type);
						text = defaultInterpolatedStringHandler.ToStringAndClear();
						break;
					}
			}
			string typeString = text;
			return "BoxedValueRef '" + typeString + "'";
		}

		private string EbxTypeToString(EbxFieldType typeToConvert, Type actualType)
		{
			switch (typeToConvert)
			{
				case EbxFieldType.Struct:
				case EbxFieldType.Enum:
					return actualType.Name;
				case EbxFieldType.CString:
					return "CString";
				default:
					return typeToConvert.ToString();
			}
		}
	


	public BoxedValueRef(int inval)
		{
			value = inval;
		}

		public BoxedValueRef(int inval, byte[] inData)
		{
			value = inval;
			data = inData;
		}

		public void SetData(byte[] inData)
		{
			data = inData;
		}

		public byte[] GetData()
		{
			return data;
		}

		public static implicit operator int(BoxedValueRef value)
		{
			return (int)value.Value;
		}

		public static implicit operator BoxedValueRef(int inval)
		{
			return new BoxedValueRef(inval);
		}

	}
}
