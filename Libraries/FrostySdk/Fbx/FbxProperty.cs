using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
	public class FbxProperty
	{
		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Set@FbxProperty@fbxsdk@@IEAA_NPEBXAEBW4EFbxType@2@_N@Z")]
		private static extern void SetInternal(IntPtr InHandle, IntPtr pValue, ref EFbxType pValueType, bool pCheckForValueEquality);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Get@FbxProperty@fbxsdk@@IEBA_NPEAXAEBW4EFbxType@2@@Z")]
		private static extern bool GetInternal(IntPtr InHandle, ref IntPtr pValue, ref EFbxType pValueType);

		public static void Set(IntPtr inHandle, string value)
		{
			EFbxType pValueType = EFbxType.eFbxString;
			IntPtr intPtr = FbxString.Construct(value);
			SetInternal(inHandle, intPtr, ref pValueType, pCheckForValueEquality: true);
			FbxUtils.FbxFree(intPtr);
		}

		public static void Set(IntPtr inHandle, Vector3 value)
		{
			EFbxType pValueType = EFbxType.eFbxDouble3;
			IntPtr intPtr = FbxDouble3.Construct(value);
			SetInternal(inHandle, intPtr, ref pValueType, pCheckForValueEquality: true);
			FbxUtils.FbxFree(intPtr);
		}

		public unsafe static void Set(IntPtr inHandle, double value)
		{
			EFbxType pValueType = EFbxType.eFbxDouble;
			IntPtr intPtr = FbxUtils.FbxMalloc(8uL);
			Marshal.WriteInt64(intPtr, *(long*)(&value));
			SetInternal(inHandle, intPtr, ref pValueType, pCheckForValueEquality: true);
			FbxUtils.FbxFree(intPtr);
		}

		public static string GetString(IntPtr inHandle)
		{
			EFbxType pValueType = EFbxType.eFbxString;
			IntPtr pValue = IntPtr.Zero;
			GetInternal(inHandle, ref pValue, ref pValueType);
			return FbxUtils.IntPtrToString(pValue);
		}

		public unsafe static Vector3 GetDouble3(IntPtr inHandle)
		{
			EFbxType pValueType = EFbxType.eFbxDouble3;
			IntPtr pValue = IntPtr.Zero;
			GetInternal(inHandle, ref pValue, ref pValueType);
			return FbxDouble3.Get(new IntPtr(&pValue));
		}

		public unsafe static double GetDouble(IntPtr inHandle)
		{
			EFbxType pValueType = EFbxType.eFbxDouble;
			IntPtr pValue = IntPtr.Zero;
			GetInternal(inHandle, ref pValue, ref pValueType);
			return *(double*)(&pValue);
		}
	}
}
