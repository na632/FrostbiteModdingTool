using FrostySdk.Managers;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace FrostySdk.IO
{
	public class CasReader2 : NativeReader
	{
		private Stream deltaStream;

		public CasReader2(Stream inBaseStream, Stream inDeltaStream = null)
			: base(inBaseStream)
		{
			deltaStream = inDeltaStream;
		}

		public CasReader2(Stream inStream, byte[] inEncryptionKey, uint actualSize)
			: base(DecryptStream(inStream, inEncryptionKey, actualSize))
		{
		}

		public CasReader2(Stream inBaseStream, byte[] inBaseEncryptionKey, uint actualBaseSize, Stream inDeltaStream, byte[] inDeltaEncryptionKey, uint actualPatchSize)
			: this(DecryptStream(inBaseStream, inBaseEncryptionKey, actualBaseSize), DecryptStream(inDeltaStream, inDeltaEncryptionKey, actualPatchSize))
		{
		}

		public CasReader2(Stream inBaseStream, byte[] inBaseEncryptionKey, uint actualBaseSize, Stream inDeltaStream)
			: this(DecryptStream(inBaseStream, inBaseEncryptionKey, actualBaseSize), inDeltaStream)
		{
		}

		public byte[] Read()
		{
			MemoryStream memoryStream = new MemoryStream();
			if (deltaStream != null)
			{
				byte[] array = ReadPatched();
				memoryStream.Write(array, 0, array.Length);
			}
			if (stream != null)
			{
				while (Position < Length)
				{
					byte[] array2 = ReadBlock();
					if (array2 == null)
					{
						break;
					}
					memoryStream.Write(array2, 0, array2.Length);
				}
			}
			return memoryStream.ToArray();
		}

		public byte[] ReadBlock()
		{
			int num = ReadInt(Endian.Little);
			ushort num2 = ReadUShort();
			int num3 = ReadUShort(Endian.Little);
			int num4 = (num2 & 0xFF00) >> 8;
			bool useDictionary = false;
			byte[] result = null;
			if ((num4 & 0xF) != 0)
			{
				num3 = ((num4 & 0xF) << 16) + num3;
			}
			if ((num & 4278190080u) != 0L)
			{
				num &= 0xFFFFFF;
				useDictionary = true;
			}
			bool unobfuscate = ((num2 >> 7) & 1) != 0 && ProfilesLibrary.DataVersion == 20180914;
			var swR = (ushort)(num2 & 0x7F);
			switch (swR)
			{
                case 0:
                    result = ReadUncompressed(num3, unobfuscate);
                    break;
                case 2:
                    result = DecompressBlockZLib(num3, num);
                    break;
                case 15:
                    result = DecompressBlockZStd(num3, num, useDictionary, unobfuscate);
                    break;
                case 9:
                    result = DecompressBlockLZ4(num3, num);
                    break;
					// added by paul
				case 12:
					result = ReadUncompressed(num3, unobfuscate); //  DecompressOodle(num3, num, unobfuscate); //  ReadUncompressed(num3, unobfuscate);
					break;
				case 17:
                    result = DecompressOodle(num3, num, unobfuscate);
                    break;
                case 21:
                    result = DecompressOodle(num3, num, unobfuscate);
                    break;
            }
            return result;
		}

		private byte[] ReadUncompressed(int bufferSize, bool unobfuscate)
		{
			byte[] array = ReadBytes(bufferSize);
			if (unobfuscate)
			{
				byte[] key = KeyManager.Instance.GetKey("Key3");
				for (int i = 0; i < bufferSize; i++)
				{
					array[i] ^= key[i & 0x3FFF];
				}
			}
			return array;
		}

		private byte[] DecompressOodle(int bufferSize, int decompressedSize, bool unobfuscate)
		{
			byte[] array = ReadBytes(bufferSize);
			if (unobfuscate)
			{
				byte[] key = KeyManager.Instance.GetKey("Key3");
				for (int i = 0; i < bufferSize; i++)
				{
					array[i] ^= key[i & 0x3FFF];
				}
			}
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			byte[] array2 = new byte[decompressedSize];
			GCHandle gCHandle2 = GCHandle.Alloc(array2, GCHandleType.Pinned);
			Oodle.Decompress(gCHandle.AddrOfPinnedObject(), array.Length, gCHandle2.AddrOfPinnedObject(), array2.Length, 0, 0, 0L, 0L, 0L, 0L, 0L, 0L, 0L);
			gCHandle.Free();
			gCHandle2.Free();
			return array2;
		}

		private byte[] DecompressBlockZStd(int bufferSize, int decompressedSize, bool useDictionary, bool unobfuscate)
		{
			byte[] array = ReadBytes(bufferSize);
			if (unobfuscate)
			{
				byte[] key = KeyManager.Instance.GetKey("Key3");
				for (int i = 0; i < bufferSize; i++)
				{
					array[i] ^= key[i & 0x3FFF];
				}
			}
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			byte[] array2 = new byte[decompressedSize];
			GCHandle gCHandle2 = GCHandle.Alloc(array2, GCHandleType.Pinned);
			ulong num = 0uL;
			if (useDictionary)
			{
				byte[] dictionary = ZStd.GetDictionary();
				IntPtr intPtr = ZStd.Create();
				GCHandle gCHandle3 = GCHandle.Alloc(dictionary, GCHandleType.Pinned);
				if (gCHandle3 != null)
				{
					IntPtr dict = ZStd.CreateDigestedDict(gCHandle3.AddrOfPinnedObject(), dictionary.Length);
					num = ZStd.DecompressUsingDict(intPtr, gCHandle2.AddrOfPinnedObject(), (ulong)array2.Length, gCHandle.AddrOfPinnedObject(), (ulong)array.Length, dict);
					gCHandle3.Free();
					ZStd.FreeDigestedDict(dict);
				}
				ZStd.Free(intPtr);
			}
			else
			{
				num = ZStd.Decompress(gCHandle2.AddrOfPinnedObject(), (ulong)array2.Length, gCHandle.AddrOfPinnedObject(), (ulong)array.Length);
			}
			if (ZStd.IsError(num))
			{
				throw new InvalidDataException(Marshal.PtrToStringAnsi(ZStd.GetErrorName(num)));
			}
			gCHandle.Free();
			gCHandle2.Free();
			return array2;
		}

		private byte[] DecompressBlockZLib(int bufferSize, int decompressedSize)
		{
			byte[] array = ReadBytes(bufferSize);
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			byte[] array2 = new byte[decompressedSize];
			GCHandle gCHandle2 = GCHandle.Alloc(array2, GCHandleType.Pinned);
			ZLib.ZStream structure = default(ZLib.ZStream);
			structure.avail_in = (uint)array.Length;
			structure.avail_out = (uint)array2.Length;
			structure.next_in = gCHandle.AddrOfPinnedObject();
			structure.next_out = gCHandle2.AddrOfPinnedObject();
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
			Marshal.StructureToPtr(structure, intPtr, fDeleteOld: true);
			ZLib.InflateInit(intPtr, "1.2.11", Marshal.SizeOf<ZLib.ZStream>());
			ZLib.Inflate(intPtr, 0);
			ZLib.InflateEnd(intPtr);
			gCHandle.Free();
			gCHandle2.Free();
			Marshal.FreeHGlobal(intPtr);
			return array2;
		}

		private byte[] DecompressBlockLZ4(int bufferSize, int decompressedSize)
		{
			GCHandle gCHandle = GCHandle.Alloc(ReadBytes(bufferSize), GCHandleType.Pinned);
			byte[] array = new byte[decompressedSize];
			GCHandle gCHandle2 = GCHandle.Alloc(array, GCHandleType.Pinned);
			LZ4.Decompress(gCHandle.AddrOfPinnedObject(), gCHandle2.AddrOfPinnedObject(), array.Length);
			gCHandle.Free();
			gCHandle2.Free();
			return array;
		}

		private byte[] ReadPatched()
		{
			MemoryStream memoryStream = new MemoryStream();
			using (CasReader casReader = new CasReader(deltaStream))
			{
				while (casReader.Position < casReader.Length)
				{
					uint num = casReader.ReadUInt(Endian.Big);
					switch (((int)num & -268435456) >> 28)
					{
					case 0:
					{
						int num8 = (int)(num & 0xFFFFFFF);
						while (num8-- > 0)
						{
							byte[] array3 = ReadBlock();
							memoryStream.Write(array3, 0, array3.Length);
						}
						break;
					}
					case 1:
					{
						int num3 = (int)(num & 0xFFFFFFF);
						using (MemoryStream memoryStream2 = new MemoryStream(ReadBlock()))
						{
							byte[] array = null;
							while (num3-- > 0)
							{
								ushort num4 = casReader.ReadUShort(Endian.Big);
								int num5 = casReader.ReadUShort(Endian.Big);
								int num6 = (int)((int)num4 - memoryStream2.Position);
								array = new byte[num6];
								memoryStream2.Read(array, 0, num6);
								memoryStream.Write(array, 0, num6);
								array = casReader.ReadBlock();
								if (array != null)
								{
									memoryStream.Write(array, 0, array.Length);
								}
								memoryStream2.Position += num5;
							}
							array = new byte[(int)(memoryStream2.Length - memoryStream2.Position)];
							memoryStream2.Read(array, 0, array.Length);
							memoryStream.Write(array, 0, array.Length);
						}
						break;
					}
					case 2:
					{
						using (MemoryStream memoryStream3 = new MemoryStream(ReadBlock()))
						{
							int num9 = (int)(num & 0xFFFFFFF);
							int num10 = casReader.ReadUShort(Endian.Big) + 1;
							byte[] array4 = new byte[num10];
							long position = casReader.Position;
							int num11 = 0;
							while (casReader.Position - position < num9)
							{
								ushort num12 = casReader.ReadUShort(Endian.Big);
								int num13 = casReader.ReadByte();
								int numBytes = casReader.ReadByte();
								num11 += memoryStream3.Read(array4, num11, (int)(num12 - memoryStream3.Position));
								memoryStream3.Position += num13;
								num11 += casReader.Read(array4, num11, numBytes);
							}
							memoryStream3.Read(array4, num11, num10 - num11);
							memoryStream.Write(array4, 0, array4.Length);
						}
						break;
					}
					case 3:
					{
						int num7 = (int)(num & 0xFFFFFFF);
						while (num7-- > 0)
						{
							byte[] array2 = casReader.ReadBlock();
							memoryStream.Write(array2, 0, array2.Length);
						}
						break;
					}
					case 4:
					{
						int num2 = (int)(num & 0xFFFFFFF);
						while (num2-- > 0)
						{
							ReadBlock();
						}
						break;
					}
					}
				}
			}
			deltaStream = null;
			return memoryStream.ToArray();
		}

		private static Stream DecryptStream(Stream inStream, byte[] encryptionKey, uint actualSize)
		{
			if (encryptionKey == null)
			{
				return inStream;
			}
			int num = (int)inStream.Length;
			byte[] array = new byte[num];
			inStream.Read(array, 0, num);
			using (Aes aes = Aes.Create())
			{
				aes.Key = encryptionKey;
				aes.IV = encryptionKey;
				ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
				using (MemoryStream stream = new MemoryStream(array))
				{
					using (CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read))
					{
						cryptoStream.Read(array, 0, num);
					}
				}
			}
			inStream.Dispose();
			Array.Resize(ref array, (int)actualSize);
			return new MemoryStream(array);
		}
	}
}
