using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxDouble3
	{
		[DllImport("thirdparty/libfbxsdk", EntryPoint = "??0?$FbxVectorTemplate3@N@fbxsdk@@QEAA@NNN@Z")]
		private static extern void ConstructInternal(IntPtr handle, double pValue1, double pValue2, double pValue3);

		public static IntPtr Construct(Vector3 value)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			IntPtr intPtr = FbxUtils.FbxMalloc(24uL);
			ConstructInternal(intPtr, value.X, value.Y, value.Z);
			return intPtr;
		}

		public unsafe static Vector3 Get(IntPtr inHandle)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			float num = (float)(*(double*)inHandle.ToInt64());
			float num2 = (float)(*(double*)(inHandle.ToInt64() + 8));
			float num3 = (float)(*(double*)(inHandle.ToInt64() + 16));
			return new Vector3(num, num2, num3);
		}
	}
}
