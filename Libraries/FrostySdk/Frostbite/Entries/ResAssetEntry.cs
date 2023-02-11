using FMT.FileTools;
using System.Text;

namespace FrostySdk.Managers
{
	public sealed class ResAssetEntry : AssetEntry, IAssetEntry
	{
        public ulong ResRid { get; set; }

        public uint ResType { get; set; }

		public byte[] ResMeta { get; set; }

		public override string Type
		{
			get
			{
				ResourceType resType = (ResourceType)ResType;
				return resType.ToString();
			}
		}

		public override string AssetType => EAssetType.res.ToString();

        public override string ToString()
        {
			return $"[ResAssetEntry][{Type}]{Filename}";
		}

        public override bool Equals(object obj)
        {
			if(obj is ResAssetEntry)
            {
				var other = obj as ResAssetEntry;
				if (Fnv1a.HashString(other.Name) == Fnv1a.HashString(Name))
					return true;

				if (other.Name.Equals(Name))
					return true;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
