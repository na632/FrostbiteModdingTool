using System;

namespace FrostySdk.Ebx
{
	public class TypeRef
	{
		private Guid typeGuid;

		private string typeName;

		public TypeRef()
		{
			typeName = "";
		}

		public TypeRef(string value)
		{
			typeName = value;

		}

		public TypeRef(Guid guid)
		{
			this.typeGuid = guid;
			this.typeName = TypeLibrary.Reflection.LookupType(guid);
		}

		public static implicit operator string(TypeRef value)
		{
			return value.typeName;
		}

		public static implicit operator TypeRef(string value)
		{
			return new TypeRef(value);
		}

		public static implicit operator TypeRef(Guid guid)
		{
			return new TypeRef(guid);
		}

		public override string ToString()
		{
			return "TypeRef '" + ((typeName != "") ? typeName : "(null)") + "'";
		}
	}
}
