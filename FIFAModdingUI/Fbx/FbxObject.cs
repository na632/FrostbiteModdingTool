using System;
using System.Runtime.InteropServices;

namespace FrostbiteModdingUI.Fbx
{
	internal class FbxObject : FbxNative, IDisposable
	{
		public string Name
		{
			get
			{
				return FbxUtils.IntPtrToString(GetNameInternal(base.Handle));
			}
			set
			{
				SetNameInternal(base.Handle, value);
			}
		}

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?Destroy@FbxObject@fbxsdk@@QEAAX_N@Z")]
		protected static extern IntPtr DestroyInternal(IntPtr InHandle, bool pRecursive);

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetName@FbxObject@fbxsdk@@QEBAPEBDXZ")]
		protected static extern IntPtr GetNameInternal(IntPtr InHandle);

		[DllImport("thirdparty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetName@FbxObject@fbxsdk@@QEAAXPEBD@Z")]
		protected static extern void SetNameInternal(IntPtr InHandle, [MarshalAs(UnmanagedType.LPStr)] string pName);

		internal FbxObject()
		{
		}

		public FbxObject(IntPtr InHandle)
			: base(InHandle)
		{
		}

		public void Dispose()
		{
			Dispose(bDisposing: true);
		}

		protected virtual void Dispose(bool bDisposing)
		{
			if (pHandle != IntPtr.Zero)
			{
				DestroyInternal(pHandle, pRecursive: false);
				pHandle = IntPtr.Zero;
			}
			if (bDisposing)
			{
				GC.SuppressFinalize(this);
			}
		}

		public override int GetHashCode()
		{
			return pHandle.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is FbxObject)
			{
				FbxObject fbxObject = (FbxObject)obj;
				return pHandle == fbxObject.Handle;
			}
			return false;
		}

		public static bool operator ==(FbxObject a, FbxObject b)
		{
			return a?.Equals(b) ?? ((object)b == null);
		}

		public static bool operator !=(FbxObject a, FbxObject b)
		{
			if ((object)a == null)
			{
				return (object)b != null;
			}
			return !a.Equals(b);
		}
	}
}
