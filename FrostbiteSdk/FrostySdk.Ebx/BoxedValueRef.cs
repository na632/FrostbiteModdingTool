using System;

namespace FrostySdk.Ebx
{
	public class BoxedValueRef
	{
		private int value;

		private byte[] data;

		public BoxedValueRef()
		{
			value = -1;
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
			return value.value;
		}

		public static implicit operator BoxedValueRef(int inval)
		{
			return new BoxedValueRef(inval);
		}

		public override string ToString()
		{
			string str = (value != -1) ? BitConverter.ToString(data) : "(null)";
			return "BoxedValueRef '" + str + "'";
		}
	}
}
