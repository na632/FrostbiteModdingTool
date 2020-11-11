using SharpDX;
using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxLayerElementArray : FbxNative
	{
		public int Count => GetCountInternal(pHandle);

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?Add@FbxLayerElementArray@fbxsdk@@QEAAHPEBXW4EFbxType@2@@Z")]
		private static extern int AddInternal(IntPtr InHandle, IntPtr pItem, EFbxType pValueType);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetCount@FbxLayerElementArray@fbxsdk@@QEBAHXZ")]
		private static extern int GetCountInternal(IntPtr handle);

		[DllImport("thirdparty/libfbxsdk", EntryPoint = "?GetAt@FbxLayerElementArray@fbxsdk@@QEBA_NHPEAPEAXW4EFbxType@2@@Z")]
		private static extern bool GetAtInternal(IntPtr handle, int pIndex, IntPtr pItem, EFbxType pValueType);

		public FbxLayerElementArray(IntPtr InHandle)
			: base(InHandle)
		{
		}

		public int Add(double x, double y, double z, double w = 0.0)
		{
			IntPtr intPtr = FbxUtils.FbxMalloc(32uL);
			Marshal.WriteInt64(intPtr, 0, BitConverter.ToInt64(BitConverter.GetBytes(x), 0));
			Marshal.WriteInt64(intPtr, 8, BitConverter.ToInt64(BitConverter.GetBytes(y), 0));
			Marshal.WriteInt64(intPtr, 16, BitConverter.ToInt64(BitConverter.GetBytes(z), 0));
			Marshal.WriteInt64(intPtr, 24, BitConverter.ToInt64(BitConverter.GetBytes(w), 0));
			int result = AddInternal(pHandle, intPtr, EFbxType.eFbxDouble4);
			FbxUtils.FbxFree(intPtr);
			return result;
		}

		public int Add(double x, double y)
		{
			IntPtr intPtr = FbxUtils.FbxMalloc(16uL);
			Marshal.WriteInt64(intPtr, 0, BitConverter.ToInt64(BitConverter.GetBytes(x), 0));
			Marshal.WriteInt64(intPtr, 8, BitConverter.ToInt64(BitConverter.GetBytes(y), 0));
			int result = AddInternal(pHandle, intPtr, EFbxType.eFbxDouble2);
			FbxUtils.FbxFree(intPtr);
			return result;
		}

		public int Add(int a)
		{
			IntPtr intPtr = FbxUtils.FbxMalloc(4uL);
			Marshal.WriteInt32(intPtr, 0, a);
			int result = AddInternal(pHandle, intPtr, EFbxType.eFbxInt);
			FbxUtils.FbxFree(intPtr);
			return result;
		}

		public void GetAt(int index, out Vector4 outValue)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			outValue = default(Vector4);
			IntPtr at = GetAt(index, EFbxType.eFbxDouble4);
			outValue.X = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 0));
			outValue.Y = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 8));
			outValue.Z = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 16));
			outValue.W = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 24));
			FbxUtils.FbxFree(at);
		}

		public void GetAt(int index, out Vector3 outValue)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			outValue = default(Vector3);
			IntPtr at = GetAt(index, EFbxType.eFbxDouble3);
			outValue.X = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 0));
			outValue.Y = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 8));
			outValue.Z = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 16));
			FbxUtils.FbxFree(at);
		}

		public void GetAt(int index, out Vector2 outValue)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			outValue = default(Vector2);
			IntPtr at = GetAt(index, EFbxType.eFbxDouble2);
			outValue.X = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 0));
			outValue.Y = (float)BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 8));
			FbxUtils.FbxFree(at);
		}

		public void GetAt(int index, out ColorBGRA outValue)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			outValue = default(ColorBGRA);
			IntPtr at = GetAt(index, EFbxType.eFbxDouble4);
			outValue.R = (byte)(BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 0)) * 255.0);
			outValue.G = (byte)(BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 8)) * 255.0);
			outValue.B = (byte)(BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 16)) * 255.0);
			outValue.A = (byte)(BitConverter.Int64BitsToDouble(Marshal.ReadInt64(at, 24)) * 255.0);
			FbxUtils.FbxFree(at);
		}

		public void GetAt(int index, out int outValue)
		{
			IntPtr at = GetAt(index, EFbxType.eFbxInt);
			outValue = Marshal.ReadInt32(at);
			FbxUtils.FbxFree(at);
		}

		private unsafe IntPtr GetAt(int index, EFbxType type)
		{
			ulong size = 0uL;
			switch (type)
			{
			case EFbxType.eFbxDouble4:
				size = 32uL;
				break;
			case EFbxType.eFbxDouble3:
				size = 24uL;
				break;
			case EFbxType.eFbxDouble2:
				size = 16uL;
				break;
			case EFbxType.eFbxInt:
				size = 4uL;
				break;
			}
			IntPtr result = FbxUtils.FbxMalloc(size);
			GetAtInternal(pItem: new IntPtr(&result), handle: pHandle, pIndex: index, pValueType: type);
			IntPtr zero = IntPtr.Zero;
			return result;
		}
	}
}
