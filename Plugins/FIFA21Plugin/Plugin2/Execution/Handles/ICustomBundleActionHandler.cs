using System.IO;
using FrostySdk.Managers;

namespace FIFA21Plugin.Plugin2.Handlers
{
	public interface ICustomBundleActionHandler
	{
		object Load(object existing, byte[] newData);

		AssetEntry Modify(AssetEntry origEntry, Stream baseStream, object data, out byte[] outData);
	}
}
