using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxObject : FbxNative, IDisposable
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

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?Destroy@FbxObject@fbxsdk@@QEAAX_N@Z")]
        protected static extern IntPtr DestroyInternal(IntPtr InHandle, bool pRecursive);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetName@FbxObject@fbxsdk@@QEBAPEBDXZ")]
        protected static extern IntPtr GetNameInternal(IntPtr InHandle);

        [DllImport("ThirdParty/libfbxsdk", CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetName@FbxObject@fbxsdk@@QEAAXPEBD@Z")]
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
            FbxObject fbxObject = obj as FbxObject;
            if (fbxObject is not null)
            {
                return pHandle == fbxObject.Handle;
            }
            return false;
        }

        public static bool operator ==(FbxObject a, FbxObject b)
        {
            return a?.Equals(b) ?? (b is null);
        }

        public static bool operator !=(FbxObject a, FbxObject b)
        {
            if (a is null)
            {
                return b is not null;
            }
            return !a.Equals(b);
        }
    }
}
