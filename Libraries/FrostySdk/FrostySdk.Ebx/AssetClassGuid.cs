using System;

namespace FrostySdk.Ebx
{
    public struct AssetClassGuid
    {
        private Guid exportedGuid;

        private int internalId;

        private bool isExported;

        public Guid ExportedGuid => exportedGuid;

        public int InternalId => internalId;

        public bool IsExported => isExported;

        public AssetClassGuid(Guid inGuid, int inId)
        {
            exportedGuid = inGuid;
            internalId = inId;
            isExported = (inGuid != Guid.Empty);
        }

        public AssetClassGuid(int inId)
        {
            exportedGuid = Guid.Empty;
            internalId = inId;
            isExported = false;
        }

        public static bool operator ==(AssetClassGuid A, object B)
        {
            return A.Equals(B);
        }

        public static bool operator !=(AssetClassGuid A, object B)
        {
            return !A.Equals(B);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is AssetClassGuid)
            {
                AssetClassGuid assetClassGuid = (AssetClassGuid)obj;
                if (isExported == assetClassGuid.isExported && exportedGuid == assetClassGuid.exportedGuid)
                {
                    return internalId == assetClassGuid.internalId;
                }
                return false;
            }
            if (obj is Guid)
            {
                Guid a = (Guid)obj;
                if (isExported)
                {
                    return a == exportedGuid;
                }
                return false;
            }
            if (obj is int)
            {
                int num = (int)obj;
                return internalId == num;
            }
            return false;
        }

        public override int GetHashCode()
        {
            //return Convert.ToInt32(((((-2128831035L * 16777619L) ^ exportedGuid.GetHashCode()) * 16777619) ^ internalId.GetHashCode()) * 16777619) ^ isExported.GetHashCode();
            return 0;
        }

        public override string ToString()
        {
            if (isExported)
            {
                return exportedGuid.ToString();
            }
            return "00000000-0000-0000-0000-" + internalId.ToString("x12");
        }
    }
}
