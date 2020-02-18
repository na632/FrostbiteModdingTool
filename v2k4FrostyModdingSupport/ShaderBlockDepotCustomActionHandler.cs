using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using System.IO;

namespace Frosty.ModSupport.Handlers
{
	[ActionHandler(2314150405u)]
	internal class ShaderBlockDepotCustomActionHandler : ICustomActionHandler
	{
		public object Load(object existing, byte[] newData)
		{
			ModifiedShaderBlockDepot modifiedShaderBlockDepot = (ModifiedShaderBlockDepot)ModifiedResource.Read(newData);
			ModifiedShaderBlockDepot modifiedShaderBlockDepot2 = (ModifiedShaderBlockDepot)existing;
			if (modifiedShaderBlockDepot2 == null)
			{
				return modifiedShaderBlockDepot;
			}
			modifiedShaderBlockDepot2.Merge(modifiedShaderBlockDepot);
			return modifiedShaderBlockDepot2;
		}

		public AssetEntry Modify(AssetEntry origEntry, Stream baseStream, object data, out byte[] outData)
		{
			ModifiedShaderBlockDepot modifiedData = data as ModifiedShaderBlockDepot;
			ShaderBlockDepot shaderBlockDepot = new ShaderBlockDepot();
			using (NativeReader reader = new NativeReader(baseStream))
			{
				shaderBlockDepot.Read(reader, null, origEntry as ResAssetEntry, modifiedData);
			}
			byte[] array = shaderBlockDepot.ToBytes();
			outData = Utils.CompressFile(array);
			return new ResAssetEntry
			{
				Sha1 = Utils.GenerateSha1(outData),
				OriginalSize = array.Length,
				Size = outData.Length,
				ResMeta = shaderBlockDepot.ResourceMeta
			};
		}
	}
}
