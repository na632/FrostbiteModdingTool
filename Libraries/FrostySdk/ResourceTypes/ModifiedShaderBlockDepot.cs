using FrostySdk.IO;
using System.Collections.Generic;

namespace FrostySdk.Resources
{
	public class ModifiedShaderBlockDepot : ModifiedResource
	{
		private List<ulong> hashes = new List<ulong>();

		private List<ShaderBlockResource> resources = new List<ShaderBlockResource>();

		internal override string ModifiedType => "ModifiedShaderBlockDepot";

		public void Merge(ModifiedShaderBlockDepot newMsbd)
		{
			for (int i = 0; i < hashes.Count; i++)
			{
				if (newMsbd.ContainsHash(hashes[i]))
				{
					resources[i] = newMsbd.GetResource(hashes[i]);
				}
			}
			for (int j = 0; j < newMsbd.hashes.Count; j++)
			{
				if (!ContainsHash(newMsbd.hashes[j]))
				{
					AddResource(newMsbd.hashes[j], newMsbd.resources[j]);
				}
			}
		}

		internal bool ContainsHash(ulong hash)
		{
			return hashes.Contains(hash);
		}

		internal ShaderBlockResource GetResource(ulong hash)
		{
			return resources[hashes.IndexOf(hash)];
		}

		internal void AddResource(ulong hash, ShaderBlockResource resource)
		{
			int num = hashes.IndexOf(hash);
			if (num == -1)
			{
				hashes.Add(hash);
				resources.Add(null);
				num = resources.Count - 1;
			}
			resources[num] = resource;
		}

		internal override void SaveInternal(NativeWriter writer)
		{
			base.SaveInternal(writer);
			writer.Write(hashes.Count);
			writer.WritePadding(16);
			for (int i = 0; i < hashes.Count; i++)
			{
				writer.Write(0L);
				writer.Write(0L);
			}
			List<long> list = new List<long>();
			List<int> relocTable = new List<int>();
			for (int j = 0; j < hashes.Count; j++)
			{
				resources[j].Save(writer, relocTable, out long startOffset);
				list.Add(startOffset);
			}
			writer.BaseStream.Position = 16L;
			for (int k = 0; k < hashes.Count; k++)
			{
				writer.Write(list[k]);
				if (resources[k] is ShaderPersistentParamDbBlock)
				{
					writer.Write(1L);
				}
				else if (resources[k] is MeshParamDbBlock)
				{
					writer.Write(3L);
				}
			}
		}

		internal override void ReadInternal(NativeReader reader)
		{
			int num = reader.ReadInt();
			reader.Pad(16);
			List<long> list = new List<long>();
			for (int i = 0; i < num; i++)
			{
				list.Add(reader.ReadLong());
				long num2 = reader.ReadLong();
				ShaderBlockResource item = null;
				if ((ulong)num2 <= 4uL)
				{
					switch (num2)
					{
					case 0L:
						item = new ShaderBlockEntry();
						break;
					case 1L:
						item = new ShaderPersistentParamDbBlock();
						break;
					case 2L:
						item = new ShaderStaticParamDbBlock();
						break;
					case 3L:
						item = new MeshParamDbBlock();
						break;
					case 4L:
						item = new MeshVariationDbBlock();
						break;
					}
				}
				resources.Add(item);
			}
			for (int j = 0; j < resources.Count; j++)
			{
				reader.Position = list[j];
				resources[j].Read(reader, null);
				hashes.Add(resources[j].Hash);
			}
		}
	}
}
