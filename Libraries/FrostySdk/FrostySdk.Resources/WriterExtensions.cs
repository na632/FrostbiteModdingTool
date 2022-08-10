using FrostySdk.IO;

namespace FrostySdk.Resources
{
	internal static class WriterExtensions
	{
		public static void Write(this NativeWriter writer, Vec3 vec)
		{
			writer.Write(vec.x);
			writer.Write(vec.y);
			writer.Write(vec.z);
			writer.Write(vec.pad);
		}

		public static void Write(this NativeWriter writer, AxisAlignedBox aab)
		{
			writer.Write(aab.min);
			writer.Write(aab.max);
		}

		public static void Write(this NativeWriter writer, LinearTransform lt)
		{
			writer.Write(lt.right);
			writer.Write(lt.up);
			writer.Write(lt.forward);
			writer.Write(lt.trans);
		}
	}
}
