using FrostySdk.IO;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FrostySdk.Resources
{
	public class LocalizedStringResource
	{
		private List<LocalizedString> strings = new List<LocalizedString>();

		public IEnumerable<KeyValuePair<uint, string>> Strings
		{
			get
			{
				for (int i = 0; i < strings.Count; i++)
				{
					yield return new KeyValuePair<uint, string>(strings[i].Id, strings[i].Value);
				}
			}
		}

		public LocalizedStringResource(Stream stream)
		{
			if (ProfilesLibrary.DataVersion != 20181207)
			{
				using (NativeReader nativeReader = new NativeReader(stream))
				{
					if (nativeReader.ReadUInt() != 3616227563u)
					{
						throw new InvalidDataException();
					}
					nativeReader.ReadUInt();
					uint num = nativeReader.ReadUInt();
					nativeReader.ReadUInt();
					nativeReader.ReadUInt();
					nativeReader.ReadUInt();
					uint nodeCount = nativeReader.ReadUInt();
					nativeReader.ReadUInt();
					uint num2 = nativeReader.ReadUInt();
					nativeReader.ReadUInt();
					uint[] array = new uint[3];
					uint[] array2 = new uint[3];
					for (int i = 0; i < 3; i++)
					{
						if (i != 2 || array[i - 1] != 0)
						{
							array[i] = nativeReader.ReadUInt();
							array2[i] = nativeReader.ReadUInt();
						}
					}
					int[] stringData = new int[num2];
					HuffmanNode rootNode = ReadNodes(nativeReader, nodeCount);
					ReadStringData(nativeReader, num2, stringData);
					nativeReader.Position = num;
					ReadStrings(nativeReader, rootNode, stringData);
				}
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

		private void ReadStringData(NativeReader reader, uint stringsCount, int[] stringData)
		{
			for (int i = 0; i < stringsCount; i++)
			{
				uint id = reader.ReadUInt();
				stringData[i] = reader.ReadInt();
				strings.Add(new LocalizedString
				{
					Id = id
				});
			}
		}

		private void ReadStrings(NativeReader reader, HuffmanNode rootNode, int[] stringData)
		{
			using (BitReader bitReader = new BitReader(new MemoryStream(reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position)))))
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < strings.Count; i++)
				{
					bitReader.SetPosition(stringData[i]);
					while (true)
					{
						HuffmanNode huffmanNode = rootNode;
						while (huffmanNode.left != null && huffmanNode.right != null)
						{
							huffmanNode = ((!bitReader.GetBit()) ? huffmanNode.left : huffmanNode.right);
						}
						if (huffmanNode.Letter == '\0')
						{
							break;
						}
						stringBuilder.Append(huffmanNode.Letter);
					}
					strings[i].Value = stringBuilder.ToString();
					stringBuilder.Clear();
				}
			}
		}
	}
}
