using Frosty.Hash;
using FrostySdk.FrostySdk.Managers;
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
			StringBuilder sb = new StringBuilder();
			sb.Append(Type);
			sb.Append(" - ");
			sb.Append(Filename);

			if(sb.Length > 0 )
            {
				return sb.ToString();
            }
			return base.ToString();
        }

        public override bool Equals(object obj)
        {
			if(obj is ResAssetEntry)
            {
				var other = obj as ResAssetEntry;
				if (Fnv1.HashString(other.Name) == Fnv1.HashString(Name))
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
