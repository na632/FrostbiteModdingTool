using System;
using System.Runtime.InteropServices;

namespace FrostySdk
{
    internal static class LZ4
    {
        [DllImport("thirdparty/liblz4.so.1.8.0.dll", EntryPoint = "LZ4_decompress_fast")]
        public static extern int Decompress(IntPtr src, IntPtr dst, int outputSize);

        [DllImport("thirdparty/liblz4.so.1.8.0.dll", EntryPoint = "LZ4_compressBound")]
        public static extern int CompressBound(int inputSize);

        [DllImport("thirdparty/liblz4.so.1.8.0.dll", EntryPoint = "LZ4_compress_default")]
        public static extern int Compress(IntPtr src, IntPtr dst, int sourceSize, int maxDestSize);
    }
}
