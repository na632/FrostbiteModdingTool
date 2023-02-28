using FrostySdk.IO;
using FrostySdk.Resources;

namespace FMT.FileTools
{
    public static class ReaderExtensions
    {
        public static Vec3 ReadVec3(this NativeReader reader)
        {
            Vec3 result = default;
            result.x = reader.ReadFloat();
            result.y = reader.ReadFloat();
            result.z = reader.ReadFloat();
            result.pad = reader.ReadFloat();
            return result;
        }

        public static Vec2 ReadVec2(this NativeReader reader)
        {
            Vec2 result = default;
            result.x = reader.ReadFloat();
            result.y = reader.ReadFloat();
            return result;
        }

        public static AxisAlignedBox2 ReadAxisAlignedBox2(this NativeReader reader)
        {
            AxisAlignedBox2 result = default;
            result.min = reader.ReadVec2();
            result.max = reader.ReadVec2();
            return result;
        }

        public static AxisAlignedBox ReadAxisAlignedBox(this NativeReader reader)
        {
            AxisAlignedBox result = default;
            result.min = reader.ReadVec3();
            result.max = reader.ReadVec3();
            return result;
        }

        public static LinearTransform ReadLinearTransform(this NativeReader reader)
        {
            LinearTransform result = default;
            result.right = reader.ReadVec3();
            result.up = reader.ReadVec3();
            result.forward = reader.ReadVec3();
            result.trans = reader.ReadVec3();
            return result;
        }

        public static Sha1 ReadSha1(this NativeReader reader)
        {
            return new FMT.FileTools.Sha1(reader.ReadBytes(20));
        }

        //public static Sha1 ReadSha1(this DeobfuscatedReader reader)
        //{
        //    return new FMT.FileTools.Sha1(reader.ReadBytes(20));
        //}

        public static Sha1 ReadSha1(this DbReader reader)
        {
            return new FMT.FileTools.Sha1(reader.ReadBytes(20));
        }
    }
}
