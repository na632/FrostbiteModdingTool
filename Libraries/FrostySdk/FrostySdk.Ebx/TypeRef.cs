namespace FrostySdk.Ebx
{
	public class TypeRef
	{
		private string typeName;

		public TypeRef()
		{
			typeName = "";
		}

		public TypeRef(string value)
		{
			typeName = value;
		}

		public static implicit operator string(TypeRef value)
		{
			return value.typeName;
		}

		public static implicit operator TypeRef(string value)
		{
			return new TypeRef(value);
		}

		public override string ToString()
		{
			return "TypeRef '" + ((typeName != "") ? typeName : "(null)") + "'";
		}
	}
}
