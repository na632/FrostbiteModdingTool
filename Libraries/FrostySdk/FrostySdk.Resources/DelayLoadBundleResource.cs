using FrostySdk.IO;
using FrostySdk.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrostySdk.Resources
{
	public class DelayLoadBundleResource
	{
		private List<PartitionBundle> partitionBundles = new List<PartitionBundle>();

		private List<DelayLoadBundle> bundles = new List<DelayLoadBundle>();

		public DelayLoadBundleResource(Stream stream, AssetManager am)
		{
			using (NativeReader nativeReader = new NativeReader(stream))
			{
				List<int> list = new List<int>();
				List<byte> list2 = new List<byte>();
				List<int> list3 = new List<int>();
				List<Tuple<uint, uint>> list4 = new List<Tuple<uint, uint>>();
				for (int i = 0; i < 4; i++)
				{
					list4.Add(new Tuple<uint, uint>(nativeReader.ReadUInt(), nativeReader.ReadUInt()));
				}
				List<Tuple<uint, uint, uint>> list5 = new List<Tuple<uint, uint, uint>>();
				for (int j = 0; j < 2; j++)
				{
					list5.Add(new Tuple<uint, uint, uint>(nativeReader.ReadUInt(), nativeReader.ReadUInt(), nativeReader.ReadUInt()));
				}
				list4.Add(new Tuple<uint, uint>(nativeReader.ReadUInt(), nativeReader.ReadUInt()));
				nativeReader.ReadUInt();
				nativeReader.Position = list4[0].Item1;
				for (int k = 0; k < list4[0].Item2; k++)
				{
					int item = nativeReader.ReadInt();
					Guid inGuid = nativeReader.ReadGuid();
					uint inHash = nativeReader.ReadUInt();
					partitionBundles.Add(new PartitionBundle(inGuid, inHash));
					list.Add(item);
				}
				nativeReader.Position = list4[1].Item1;
				for (int l = 0; l < list4[1].Item2; l++)
				{
					byte item2 = nativeReader.ReadByte();
					int item3 = nativeReader.ReadInt();
					nativeReader.ReadBytes(3);
					int inUnk = nativeReader.ReadInt();
					uint inHash2 = nativeReader.ReadUInt();
					list2.Add(item2);
					list3.Add(item3);
					bundles.Add(new DelayLoadBundle(inUnk, inHash2));
				}
				nativeReader.Position = list4[2].Item1;
				List<ulong> list6 = new List<ulong>();
				for (int m = 0; m < list4[2].Item2; m++)
				{
					list6.Add(nativeReader.ReadULong());
				}
				nativeReader.Position = list4[3].Item1;
				List<uint> list7 = new List<uint>();
				for (int n = 0; n < list4[3].Item2; n++)
				{
					list7.Add(nativeReader.ReadUInt());
				}
				nativeReader.Position = list5[0].Item1;
				HuffmanNode stringRootNode = ReadNodes(nativeReader, list5[0].Item2);
				nativeReader.Position = list5[0].Item3;
				byte[] stringData = nativeReader.ReadBytes((int)(list5[1].Item1 - nativeReader.Position));
				nativeReader.Position = list5[1].Item1;
				HuffmanNode rootNode = ReadNodes(nativeReader, list5[1].Item2);
				nativeReader.Position = list5[1].Item3;
				List<string> list8 = ReadPaths(nativeReader, rootNode, (int)(list4[4].Item1 - nativeReader.Position), list2, list3, stringRootNode, stringData);
				for (int num = 0; num < bundles.Count; num++)
				{
					bundles[num].Name = list8[num];
				}
				for (int num2 = 0; num2 < partitionBundles.Count; num2++)
				{
					partitionBundles[num2].Bundle = bundles[list[num2]];
				}
				nativeReader.Position = list4[4].Item1;
				List<uint> list9 = new List<uint>();
				for (int num3 = 0; num3 < list4[4].Item2; num3++)
				{
					list9.Add(nativeReader.ReadUInt());
				}
			}
		}

		public IEnumerable<DelayLoadBundle> EnumerateBundles()
		{
			for (int i = 0; i < bundles.Count; i++)
			{
				yield return bundles[i];
			}
		}

		private HuffmanNode ReadNodes(NativeReader reader, uint nodeCount)
		{
			HuffmanNode result = null;
			HuffmanNode huffmanNode = null;
			HuffmanNode huffmanNode2 = null;
			List<HuffmanNode> list = new List<HuffmanNode>();
			int num = 0;
			for (int j = 0; j < nodeCount; j++)
			{
				HuffmanNode i = new HuffmanNode
				{
					value = reader.ReadUInt()
				};
				int num2 = list.FindIndex((HuffmanNode a) => a.value == i.value);
				if (num2 != -1)
				{
					i = list[num2];
				}
				if (huffmanNode == null)
				{
					huffmanNode = i;
				}
				else if (huffmanNode2 == null)
				{
					huffmanNode2 = i;
					if (num2 == -1)
					{
						list.Add(huffmanNode2);
					}
					i = new HuffmanNode
					{
						value = (uint)num++
					};
					i.left = huffmanNode;
					i.right = huffmanNode2;
					result = i;
					huffmanNode = null;
					huffmanNode2 = null;
					num2 = -1;
				}
				if (num2 == -1)
				{
					list.Add(i);
				}
			}
			return result;
		}

		private string ReadString(byte[] data, HuffmanNode rootNode, int offset)
		{
			using (BitReader bitReader = new BitReader(new MemoryStream(data)))
			{
				StringBuilder stringBuilder = new StringBuilder();
				bitReader.SetPosition(offset);
				do
				{
					HuffmanNode huffmanNode = rootNode;
					while (huffmanNode.left != null && huffmanNode.right != null)
					{
						bool bit = bitReader.GetBit();
						if (bitReader.EndOfStream)
						{
							break;
						}
						huffmanNode = ((!bit) ? huffmanNode.left : huffmanNode.right);
					}
					if (huffmanNode.Letter == '\0')
					{
						break;
					}
					stringBuilder.Append(huffmanNode.Letter);
				}
				while (!bitReader.EndOfStream);
				return stringBuilder.ToString();
			}
		}

		private List<string> ReadPaths(NativeReader reader, HuffmanNode rootNode, int length, List<byte> lengths, List<int> offsets, HuffmanNode stringRootNode, byte[] stringData)
		{
			List<string> list = new List<string>();
			byte[] buffer = reader.ReadBytes(length);
			int num = 0;
			using (BitReader bitReader = new BitReader(new MemoryStream(buffer)))
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < lengths.Count; i++)
				{
					bitReader.SetPosition(offsets[i]);
					num = 0;
					do
					{
						HuffmanNode huffmanNode = rootNode;
						while (huffmanNode.left != null && huffmanNode.right != null)
						{
							bool bit = bitReader.GetBit();
							if (bitReader.EndOfStream)
							{
								break;
							}
							huffmanNode = ((!bit) ? huffmanNode.left : huffmanNode.right);
						}
						stringBuilder.Append(ReadString(stringData, stringRootNode, (int)huffmanNode.Value) + "/");
						num++;
						if (num >= lengths[i])
						{
							list.Add(stringBuilder.ToString().Trim('/'));
							stringBuilder.Clear();
							break;
						}
					}
					while (!bitReader.EndOfStream);
				}
				return list;
			}
		}
	}
}
