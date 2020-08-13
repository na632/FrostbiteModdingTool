using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;

namespace FrostySdk.Resources
{
	public class ShaderBlockDepot : Resource
	{
		private List<ShaderBlockEntry> sbEntries = new List<ShaderBlockEntry>();

		private List<ShaderBlockResource> sbResources = new List<ShaderBlockResource>();

		public ulong ResourceId => resRid;

		public byte[] ResourceMeta => resMeta;

		public int ResourceCount => sbResources.Count;

		public int Count => sbEntries.Count;

		public override void Read(NativeReader reader, AssetManager am, ResAssetEntry entry, ModifiedResource modifiedData)
		{
			base.Read(reader, am, entry, modifiedData);
			ModifiedShaderBlockDepot modifiedShaderBlockDepot = modifiedData as ModifiedShaderBlockDepot;
			int num = BitConverter.ToInt32(resMeta, 12);
			List<long> list = new List<long>();
			for (int i = 0; i < num; i++)
			{
				list.Add(reader.ReadLong());
				long num2 = reader.ReadLong();
				ShaderBlockResource shaderBlockResource = null;
				if ((ulong)num2 <= 4uL)
				{
					switch (num2)
					{
					case 0L:
						shaderBlockResource = new ShaderBlockEntry();
						sbEntries.Add(shaderBlockResource as ShaderBlockEntry);
						break;
					case 1L:
						shaderBlockResource = new ShaderPersistentParamDbBlock();
						break;
					case 2L:
						shaderBlockResource = new ShaderStaticParamDbBlock();
						break;
					case 3L:
						shaderBlockResource = new MeshParamDbBlock();
						break;
					case 4L:
						shaderBlockResource = new MeshVariationDbBlock();
						break;
					}
				}
				sbResources.Add(shaderBlockResource);
			}
			for (int j = 0; j < list.Count; j++)
			{
				reader.Position = list[j];
				sbResources[j].Read(reader, sbResources);
				if (modifiedShaderBlockDepot != null && modifiedShaderBlockDepot.ContainsHash(sbResources[j].Hash))
				{
					sbResources[j] = modifiedShaderBlockDepot.GetResource(sbResources[j].Hash);
				}
				sbResources[j].Index = j;
			}
		}

		internal override ModifiedResource Save()
		{
			ModifiedShaderBlockDepot modifiedShaderBlockDepot = new ModifiedShaderBlockDepot();
			foreach (ShaderBlockResource sbResource in sbResources)
			{
				if (sbResource.IsModified)
				{
					modifiedShaderBlockDepot.AddResource(sbResource.Hash, sbResource);
				}
			}
			return modifiedShaderBlockDepot;
		}

		public ShaderBlockResource GetResource(int index)
		{
			if (index >= sbResources.Count)
			{
				return null;
			}
			return sbResources[index];
		}

		public bool ReplaceResource(ShaderBlockResource newResource)
		{
			for (int i = 0; i < sbResources.Count; i++)
			{
				if (sbResources[i].Hash == newResource.Hash)
				{
					newResource.Index = sbResources[i].Index;
					sbResources[i] = newResource;
					return true;
				}
			}
			return false;
		}

		public ShaderBlockEntry GetSectionEntry(int lodIndex)
		{
			if (lodIndex >= sbEntries.Count)
			{
				return null;
			}
			return sbEntries[lodIndex];
		}

		public unsafe byte[] ToBytes()
		{
			using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream()))
			{
				for (int i = 0; i < sbResources.Count; i++)
				{
					nativeWriter.Write(0L);
					nativeWriter.Write(0L);
				}
				List<long> list = new List<long>();
				List<int> list2 = new List<int>();
				for (int j = 0; j < sbResources.Count; j++)
				{
					sbResources[j].Save(nativeWriter, list2, out long startOffset);
					list.Add(startOffset);
					list2.Add(j * 16);
				}
				nativeWriter.WritePadding(16);
				nativeWriter.BaseStream.Position = 0L;
				for (int k = 0; k < list.Count; k++)
				{
					nativeWriter.Write(list[k]);
					if (sbResources[k] is ShaderBlockEntry)
					{
						nativeWriter.Write(0L);
					}
					else if (sbResources[k] is ShaderPersistentParamDbBlock)
					{
						nativeWriter.Write(1L);
					}
					else if (sbResources[k] is ShaderStaticParamDbBlock)
					{
						nativeWriter.Write(2L);
					}
					else if (sbResources[k] is MeshParamDbBlock)
					{
						nativeWriter.Write(3L);
					}
					else if (sbResources[k] is MeshVariationDbBlock)
					{
						nativeWriter.Write(4L);
					}
				}
				try
				{
					fixed (byte* ptr = &resMeta[0])
					{
						*(int*)(ptr + 4) = (int)nativeWriter.BaseStream.Length;
						*(int*)(ptr + 8) = list2.Count * 4;
					}
				}
				finally
				{
				}
				nativeWriter.BaseStream.Position = nativeWriter.BaseStream.Length;
				for (int l = 0; l < list2.Count; l++)
				{
					nativeWriter.Write(list2[l]);
				}
				return ((MemoryStream)nativeWriter.BaseStream).ToArray();
			}
		}
	}
}
