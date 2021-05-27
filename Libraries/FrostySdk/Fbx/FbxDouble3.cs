using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
	public class FbxDouble3
	{
		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "??0?$FbxVectorTemplate3@N@fbxsdk@@QEAA@NNN@Z")]
		private static extern void ConstructInternal(IntPtr handle, double pValue1, double pValue2, double pValue3);

		public static IntPtr Construct(Vector3 value)
		{
			IntPtr intPtr = FbxUtils.FbxMalloc(24uL);
			ConstructInternal(intPtr, value.X, value.Y, value.Z);
			return intPtr;
		}

		public unsafe static Vector3 Get(IntPtr inHandle)
		{
			float x = (float)(*(double*)inHandle.ToInt64());
			float num2 = (float)(*(double*)(inHandle.ToInt64() + 8));
			float num3 = (float)(*(double*)(inHandle.ToInt64() + 16));
			return new Vector3(x, num2, num3);
		}
	}
}
