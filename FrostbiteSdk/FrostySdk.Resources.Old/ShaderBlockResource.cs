using FrostySdk.IO;

namespace FrostySdk.Resources.Old
{
	public class ShaderBlockResource
	{
		public ulong Hash;

		public ulong ResourceId;

		public ShaderBlockResource(NativeReader reader)
		{
			Hash = reader.ReadULong();
		}

		public virtual Resources.ShaderBlockResource Convert()
		{
			return null;
		}
	}
}
