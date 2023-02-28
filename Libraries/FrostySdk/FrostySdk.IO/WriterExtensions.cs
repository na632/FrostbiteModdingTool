using FrostySdk.Resources;

namespace FMT.FileTools
{
    public static class WriterExtensions
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

        public static void Write(this NativeWriter writer, Sha1 sha1)
        {
            writer.Write(sha1.ToByteArray());
        }

        public static void WriteVector3(this NativeWriter writer, Vec3 vec)
        {
            writer.Write(vec.x);
            writer.Write(vec.y);
            writer.Write(vec.z);
            writer.Write(vec.pad);
        }

        public static void WriteAxisAlignedBox(this NativeWriter writer, AxisAlignedBox aab)
        {
            writer.WriteVector3(aab.min);
            writer.WriteVector3(aab.max);
        }

        public static void WriteLinearTransform(this NativeWriter writer, LinearTransform lt)
        {
            writer.WriteVector3(lt.right);
            writer.WriteVector3(lt.up);
            writer.WriteVector3(lt.forward);
            writer.WriteVector3(lt.trans);
        }
    }
}
