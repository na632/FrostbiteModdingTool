using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
    public class FbxNode : FbxObject
    {
        public enum EPivotSet
        {
            eSourcePivot,
            eDestinationPivot
        }

        private IntPtr lclTranslation;

        private IntPtr lclRotation;

        private IntPtr lclScaling;

        private IntPtr visibility;

        public Vector3 LclTranslation
        {
            get
            {
                return FbxProperty.GetDouble3(lclTranslation);
            }
            set
            {
                FbxProperty.Set(lclTranslation, value);
            }
        }

        public Vector3 LclRotation
        {
            get
            {
                return FbxProperty.GetDouble3(lclRotation);
            }
            set
            {
                FbxProperty.Set(lclRotation, value);
            }
        }

        public Vector3 LclScaling
        {
            get
            {
                return FbxProperty.GetDouble3(lclScaling);
            }
            set
            {
                FbxProperty.Set(lclScaling, value);
            }
        }

        public double Visibility
        {
            get
            {
                return FbxProperty.GetDouble(visibility);
            }
            set
            {
                FbxProperty.Set(visibility, value);
            }
        }

        public int ChildCount => GetChildCountInternal(pHandle, pRecursive: false);

        public int NodeAttributeCount => GetNodeAttributeCountInternal(pHandle);

        public IEnumerable<FbxNode> Children
        {
            get
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    yield return GetChild(i);
                }
            }
        }

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxNode@fbxsdk@@SAPEAV12@PEAVFbxManager@2@PEBD@Z")]
        private static extern IntPtr CreateFromManager(IntPtr pManager, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?Create@FbxNode@fbxsdk@@SAPEAV12@PEAVFbxObject@2@PEBD@Z")]
        private static extern IntPtr CreateFromObject(IntPtr pObject, [MarshalAs(UnmanagedType.LPStr)] string pName);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?SetNodeAttribute@FbxNode@fbxsdk@@QEAAPEAVFbxNodeAttribute@2@PEAV32@@Z")]
        private static extern IntPtr SetNodeAttributeInternal(IntPtr InHandle, IntPtr pNodeAttribute);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?AddChild@FbxNode@fbxsdk@@QEAA_NPEAV12@@Z")]
        private static extern bool AddChildInternal(IntPtr InHandle, IntPtr pNode);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetNodeAttribute@FbxNode@fbxsdk@@QEAAPEAVFbxNodeAttribute@2@XZ")]
        private static extern IntPtr GetNodeAttributeInternal(IntPtr inHandle);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetNodeAttributeCount@FbxNode@fbxsdk@@QEBAHXZ")]
        private static extern int GetNodeAttributeCountInternal(IntPtr handle);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetNodeAttributeByIndex@FbxNode@fbxsdk@@QEAAPEAVFbxNodeAttribute@2@H@Z")]
        private static extern IntPtr GetNodeAttributeByIndexInternal(IntPtr handle, int pIndex);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?EvaluateGlobalTransform@FbxNode@fbxsdk@@QEAAAEAVFbxAMatrix@2@VFbxTime@2@W4EPivotSet@12@_N2@Z")]
        private static extern IntPtr EvaluateGlobalTransformInternal(IntPtr inHandle, IntPtr pTime, EPivotSet pPivotSet, bool pApplyTarget, bool pForceEval);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetParent@FbxNode@fbxsdk@@QEAAPEAV12@XZ")]
        private static extern IntPtr GetParentInternal(IntPtr pHandle);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetChildCount@FbxNode@fbxsdk@@QEBAH_N@Z")]
        private static extern int GetChildCountInternal(IntPtr handle, bool pRecursive);

        [DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetChild@FbxNode@fbxsdk@@QEAAPEAV12@H@Z")]
        private static extern IntPtr GetChildInternal(IntPtr handle, int pIndex);

        public FbxNode(FbxManager Manager, string pName)
        {
            pHandle = CreateFromManager(Manager.Handle, pName);
            lclTranslation = pHandle + 120;
            lclRotation = pHandle + 136;
            lclScaling = pHandle + 152;
            visibility = pHandle + 168;
        }

        public FbxNode(IntPtr InHandle)
            : base(InHandle)
        {
            lclTranslation = pHandle + 120;
            lclRotation = pHandle + 136;
            lclScaling = pHandle + 152;
            visibility = pHandle + 168;
        }

        public FbxNode(FbxObject Object, string pName)
        {
            pHandle = CreateFromObject(Object.Handle, pName);
            lclTranslation = pHandle + 120;
            lclRotation = pHandle + 136;
            lclScaling = pHandle + 152;
            visibility = pHandle + 168;
        }

        public FbxNodeAttribute SetNodeAttribute(FbxNodeAttribute pNodeAttribute)
        {
            IntPtr intPtr = SetNodeAttributeInternal(pHandle, pNodeAttribute.Handle);
            if (intPtr == IntPtr.Zero)
            {
                return null;
            }
            return new FbxNodeAttribute(intPtr);
        }

        public FbxNodeAttribute GetNodeAttribute(FbxNodeAttribute.EType type)
        {
            for (int i = 0; i < NodeAttributeCount; i++)
            {
                IntPtr nodeAttributeByIndexInternal = GetNodeAttributeByIndexInternal(pHandle, i);
                if (nodeAttributeByIndexInternal != IntPtr.Zero)
                {
                    FbxNodeAttribute fbxNodeAttribute = new FbxNodeAttribute(nodeAttributeByIndexInternal);
                    if (fbxNodeAttribute.AttributeType == type)
                    {
                        return fbxNodeAttribute;
                    }
                }
            }
            return null;
        }

        public bool AddChild(FbxNode pNode)
        {
            return AddChildInternal(pHandle, pNode.Handle);
        }

        public FbxAMatrix EvaluateGlobalTransform(FbxTime time = null, EPivotSet pivotSet = EPivotSet.eSourcePivot, bool applyTarget = false, bool forceEval = false)
        {
            if (time == null)
            {
                time = FbxTime.FBXSDK_TIME_INFINITE;
            }
            return new FbxAMatrix(EvaluateGlobalTransformInternal(pHandle, time.Handle, pivotSet, applyTarget, forceEval));
        }

        public FbxNode GetParent()
        {
            IntPtr parentInternal = GetParentInternal(pHandle);
            if (parentInternal == IntPtr.Zero)
            {
                return null;
            }
            return new FbxNode(parentInternal);
        }

        public FbxNode GetChild(int index)
        {
            IntPtr childInternal = GetChildInternal(pHandle, index);
            if (childInternal == IntPtr.Zero)
            {
                return null;
            }
            return new FbxNode(childInternal);
        }
    }
}
