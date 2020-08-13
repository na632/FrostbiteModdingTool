using Frosty.Hash;
using FrostySdk.Attributes;
using FrostySdk.IO;
using System;
using System.IO;
using System.Reflection;

namespace FrostySdk.Resources
{
	public class ParameterEntry
	{
		public uint NameHash;

		public uint TypeHash;

		public ushort Used;

		public byte[] Value;

		private ulong parameterHash;

		public ParameterEntry(string name, object value)
		{
			NameHash = (uint)Fnv1.HashString(name.ToLower());
			Type type = value.GetType();
			Type type2 = null;
			type2 = ((value is bool) ? TypeLibrary.GetType((uint)Fnv1.HashString("Boolean")) : ((value is uint) ? TypeLibrary.GetType((uint)Fnv1.HashString("Uint32")) : ((value is float[]) ? TypeLibrary.GetType((uint)Fnv1.HashString("Vec")) : ((!(value is Guid)) ? type : TypeLibrary.GetType((uint)Fnv1.HashString("ITexture"))))));
			TypeHash = (uint)Fnv1.HashString(type2.Name);
			Used = 1;
			parameterHash = CalculateHash(name, type2);
			SetValue(value);
		}

		public ParameterEntry(NativeReader reader)
		{
			parameterHash = reader.ReadULong();
			TypeHash = reader.ReadUInt();
			Used = reader.ReadUShort();
			NameHash = (uint)((uint)(reader.ReadUShort() << 16) | ((parameterHash >> 48) & 0xFFFF));
			int count = reader.ReadInt();
			if (TypeHash == 2903162835u)
			{
				count = 16;
			}
			Value = reader.ReadBytes(count);
		}

		public object GetValue()
		{
			object result = null;
			switch (TypeHash)
			{
			case 2520298017u:
				result = BitConverter.ToBoolean(Value, 0);
				break;
			case 193460885u:
				result = new float[4]
				{
					BitConverter.ToSingle(Value, 0),
					BitConverter.ToSingle(Value, 4),
					BitConverter.ToSingle(Value, 8),
					BitConverter.ToSingle(Value, 12)
				};
				break;
			case 2965126178u:
				result = BitConverter.ToUInt32(Value, 0);
				break;
			}
			return result;
		}

		public void SetValue(object value)
		{
			switch (TypeHash)
			{
			case 2520298017u:
				Value = new byte[1]
				{
					(byte)(((bool)value) ? 1 : 0)
				};
				break;
			case 193460885u:
			{
				using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream()))
				{
					float[] array = (float[])value;
					nativeWriter.Write(array[0]);
					nativeWriter.Write(array[1]);
					nativeWriter.Write(array[2]);
					nativeWriter.Write(array[3]);
					Value = ((MemoryStream)nativeWriter.BaseStream).ToArray();
				}
				break;
			}
			case 2965126178u:
				Value = BitConverter.GetBytes((uint)value);
				break;
			case 2903162835u:
				Value = ((Guid)value).ToByteArray();
				break;
			case 220002843u:
				Value = new byte[1]
				{
					(byte)value
				};
				break;
			default:
				Value = BitConverter.GetBytes((int)value);
				break;
			}
		}

		public byte[] ToBytes()
		{
			using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream()))
			{
				nativeWriter.Write(parameterHash);
				nativeWriter.Write(TypeHash);
				nativeWriter.Write(Used);
				nativeWriter.Write((ushort)(NameHash >> 16));
				nativeWriter.Write((TypeHash == 2903162835u) ? 1 : Value.Length);
				nativeWriter.Write(Value);
				return ((MemoryStream)nativeWriter.BaseStream).ToArray();
			}
		}

		private ulong CalculateHash(string name, Type type)
		{
			string name2 = type.Name;
			string @namespace = type.GetCustomAttribute<EbxClassMetaAttribute>().Namespace;
			byte[] data = null;
			using (NativeWriter nativeWriter = new NativeWriter(new MemoryStream()))
			{
				nativeWriter.Write(1);
				nativeWriter.Write(name.Length);
				nativeWriter.Write(name2.Length);
				nativeWriter.Write(@namespace.Length);
				nativeWriter.WriteFixedSizedString(name, name.Length);
				nativeWriter.WriteFixedSizedString(name2, name2.Length);
				nativeWriter.WriteFixedSizedString(@namespace, @namespace.Length);
				data = ((MemoryStream)nativeWriter.BaseStream).ToArray();
			}
			return (ulong)((long)(CityHash.Hash64(data) & 0xFFFFFFFFFFFF) | ((long)Fnv1.HashString(name.ToLower()) << 48));
		}
	}
}
