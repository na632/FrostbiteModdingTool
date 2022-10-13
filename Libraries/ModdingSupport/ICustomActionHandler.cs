using FrostySdk.Managers;
using System.IO;

namespace Frosty.ModSupport.Handlers
{
	public interface ICustomActionHandler
	{
		object Load(object existing, byte[] newData);

		AssetEntry Modify(AssetEntry origEntry, Stream baseStream, object data, out byte[] outData);
	}
}
