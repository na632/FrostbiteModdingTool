using FrostySdk.IO;
using System;

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

		public BoxedValueRef()
		{
		}

		public BoxedValueRef(object inval, EbxFieldType intype)
		{
			value = inval;
			type = intype;
		}

		public override string ToString()
		{
			if (value == null)
			{
				return "(null)";
			}
			return $"{type} 'value'";
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
