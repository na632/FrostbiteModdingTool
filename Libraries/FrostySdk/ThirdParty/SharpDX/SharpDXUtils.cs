using System;
using System.Numerics;

namespace FrostbiteSdk
{
    public static class SharpDXUtils
    {
        public static Matrix4x4 FromQuaternion(Quaternion quat)
        {
            float num = 2f * quat.X * quat.X;
            float num2 = 2f * quat.X * quat.Y;
            float num3 = 2f * quat.X * quat.W;
            float num4 = 2f * quat.Y * quat.Y;
            float num5 = 2f * quat.Y * quat.Z;
            float num6 = 2f * quat.Y * quat.W;
            float num7 = 2f * quat.Z * quat.X;
            float num8 = 2f * quat.Z * quat.Z;
            float num9 = 2f * quat.Z * quat.W;
            Matrix4x4 result = default(Matrix4x4);
            result.M11 = (float)(1.0 - ((double)num4 + (double)num8));
            result.M12 = num2 + num9;
            result.M13 = num7 - num6;
            result.M14 = 0f;
            result.M21 = num2 - num9;
            result.M22 = (float)(1.0 - ((double)num8 + (double)num));
            result.M23 = num5 + num3;
            result.M24 = 0f;
            result.M31 = num7 + num6;
            result.M32 = num5 - num3;
            result.M33 = (float)(1.0 - ((double)num4 + (double)num));
            result.M34 = 0f;
            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1f;
            return result;
        }

        public static Vector3 ExtractEulerAngles(Matrix4x4 m)
        {
            float num = (float)Math.PI / 180f;
            Vector3 result = default(Vector3);
            float num2 = (float)Math.Sqrt(m.M11 * (double)m.M11 + m.M12 * (double)m.M12);
            if ((double)num2 > 0.001)
            {
                result.X = (float)Math.Atan2(m.M23, m.M33);
                result.Y = (float)Math.Atan2(0.0 - m.M13, num2);
                result.Z = (float)Math.Atan2(m.M12, m.M11);
            }
            else
            {
                result.X = (float)Math.Atan2(0.0 - m.M32, m.M22);
                result.Y = (float)Math.Atan2(0.0 - m.M13, num2);
                result.Z = 0f;
            }
            result.X /= num;
            result.Y /= num;
            result.Z /= num;
            return result;
        }

        public static Quaternion CreateFromEulerAngles(float x, float y, float z)
        {
            float num = (float)Math.Sin((double)x * 0.5);
            float num2 = (float)Math.Cos((double)x * 0.5);
            float num3 = (float)Math.Sin((double)y * 0.5);
            float num4 = (float)Math.Cos((double)y * 0.5);
            float num5 = (float)Math.Sin((double)z * 0.5);
            float num6 = (float)Math.Cos((double)z * 0.5);
            Quaternion result = default(Quaternion);
            result.X = (float)((double)num * (double)num4 * (double)num6 - (double)num2 * (double)num3 * (double)num5);
            result.Y = (float)((double)num2 * (double)num3 * (double)num6 + (double)num * (double)num4 * (double)num5);
            result.Z = (float)((double)num2 * (double)num4 * (double)num5 - (double)num * (double)num3 * (double)num6);
            result.W = (float)((double)num2 * (double)num4 * (double)num6 + (double)num * (double)num3 * (double)num5);
            return result;
        }

        public static Matrix4x4 FromLinearTransform(dynamic transform)
        {
            return new Matrix4x4(transform.right.x, transform.right.y, transform.right.z, 0f, transform.up.x, transform.up.y, transform.up.z, 0f, transform.forward.x, transform.forward.y, transform.forward.z, 0f, transform.trans.x, transform.trans.y, transform.trans.z, 1f);
        }

        public static Vector3 FromVec3(dynamic vec)
        {
            return new Vector3(vec.x, vec.y, vec.z);
        }

        public static Vector4 FromVec4(dynamic vec)
        {
            return new Vector4(vec.x, vec.y, vec.z, vec.w);
        }
    }

}
