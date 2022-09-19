using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostySdk.IO._2022.Readers
{
	public enum EbxTypeCategory : ushort
	{
		NotApplicable,
		Class,
		ValueType,
		PrimitiveType,
		ArrayType,
		EnumType,
		FunctionType,
		InterfaceType,
		DelegateType
	}
}



