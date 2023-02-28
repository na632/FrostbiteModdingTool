using FrostySdk.IO;
using System;

namespace FrostySdk.Ebx
{
    public struct PointerRef
    {
        private EbxImportReference external;

        private object internalObj;

        public EbxImportReference External => external;

        public object Internal => internalObj;

        private PointerRefType type;

        public PointerRefType Type
        {
            get { return type; }
            set { type = value; }
        }

        public PointerRef(EbxImportReference externalRef)
        {
            external = externalRef;
            internalObj = null;
            type = PointerRefType.External;
        }

        public PointerRef(Guid guid)
        {
            external = new EbxImportReference
            {
                FileGuid = guid,
                ClassGuid = Guid.Empty
            };
            internalObj = null;
            type = ((guid != Guid.Empty) ? PointerRefType.External : PointerRefType.Null);
        }

        public PointerRef(object internalRef)
        {
            external = default(EbxImportReference);
            internalObj = internalRef;
            type = PointerRefType.Internal;
        }

        public static bool operator ==(PointerRef A, object B)
        {
            return A.Equals(B);
        }

        public static bool operator !=(PointerRef A, object B)
        {
            return !A.Equals(B);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is PointerRef)
            {
                PointerRef pointerRef = (PointerRef)obj;
                if (Type == pointerRef.Type && Internal == pointerRef.Internal)
                {
                    return External == pointerRef.External;
                }
                return false;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int num = -2128831035;
            num = ((num * 16777619) ^ Type.GetHashCode());
            if (Type == PointerRefType.Internal)
            {
                num = ((num * 16777619) ^ Internal.GetHashCode());
            }
            else if (Type == PointerRefType.External)
            {
                num = ((num * 16777619) ^ External.GetHashCode());
            }
            return num;
        }
    }
}
