using System;

namespace FrostySdk.Ebx
{
    public struct ResourceRef
    {
        public static ResourceRef Zero = new ResourceRef(0uL);

        private ulong resourceId;

        public ResourceRef(ulong value)
        {
            resourceId = value;
        }

        public static implicit operator ulong(ResourceRef value)
        {
            return value.resourceId;
        }

        public static implicit operator ResourceRef(ulong value)
        {
            return new ResourceRef(value);
        }

        public override bool Equals(object obj)
        {
            if (obj is ResourceRef)
            {
                ResourceRef resourceRef = (ResourceRef)obj;
                return resourceId == resourceRef.resourceId;
            }
            if (obj is ulong)
            {
                ulong num = (ulong)obj;
                return resourceId == num;
            }
            return false;
        }

        public static bool operator ==(ResourceRef a, ResourceRef b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ResourceRef a, ResourceRef b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(-2128831035L * 16777619) ^ resourceId.GetHashCode();
        }

        public override string ToString()
        {
            return resourceId.ToString("X16");
        }
    }
}
