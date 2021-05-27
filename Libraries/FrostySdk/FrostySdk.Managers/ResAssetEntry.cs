using FrostySdk.FrostySdk.Managers;
using System.Text;

namespace FrostySdk.Managers
{
	public class ResAssetEntry : AssetEntry, IAssetEntry
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

		public override string AssetType => "res";

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
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
