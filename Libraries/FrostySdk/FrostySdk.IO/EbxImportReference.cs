using System;

namespace FrostySdk.IO
{
    public struct EbxImportReference
    {
        public Guid FileGuid;

        public Guid ClassGuid;

        public override string ToString()
        {
            return FileGuid.ToString() + "/" + ClassGuid.ToString();
        }

        public static bool operator ==(EbxImportReference A, EbxImportReference B)
        {
            return A.Equals(B);
        }

        public static bool operator !=(EbxImportReference A, EbxImportReference B)
        {
            return !A.Equals(B);
        }

        public override bool Equals(object obj)
        {
            if (obj is EbxImportReference)
            {
                EbxImportReference ebxImportReference = (EbxImportReference)obj;
                if (FileGuid == ebxImportReference.FileGuid)
                {
                    return ClassGuid == ebxImportReference.ClassGuid;
                }
                return false;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(((-2128831035L * 16777619) ^ FileGuid.GetHashCode()) * 16777619) ^ ClassGuid.GetHashCode();
        }
    }
}
