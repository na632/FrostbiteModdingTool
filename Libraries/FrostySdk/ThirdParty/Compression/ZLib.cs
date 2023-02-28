using System;
using System.Runtime.InteropServices;

namespace FrostySdk
{
    internal static class ZLib
    {
        public struct ZStream
        {
            public IntPtr next_in;

            public uint avail_in;

            public uint total_in;

            public IntPtr next_out;

            public uint avail_out;

            public uint total_out;

            public IntPtr msg;

            public IntPtr state;

            public IntPtr zalloc;

            public IntPtr zfree;

            public IntPtr opaque;

            public int data_type;

            public uint adler;

            public uint reserved;
        }

        public const string DllName = "thirdparty/zlibwapi.dll";

        public const CallingConvention Cdecl = CallingConvention.Cdecl;

        public const int Z_FINISH = 4;

        [DllImport("thirdparty/zlibwapi.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "inflate")]
        public static extern int Inflate(IntPtr strm, int flush);

        [DllImport("thirdparty/zlibwapi.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "inflateEnd")]
        public static extern int InflateEnd(IntPtr strm);

        [DllImport("thirdparty/zlibwapi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "inflateInit_")]
        public static extern int InflateInit(IntPtr strm, [MarshalAs(UnmanagedType.LPStr)] string version, int stream_size);

        [DllImport("thirdparty/zlibwapi.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "deflateInit_")]
        public static extern int DeflateInit(IntPtr strm, int level, [MarshalAs(UnmanagedType.LPStr)] string version, int stream_size);

        [DllImport("thirdparty/zlibwapi.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "deflate")]
        public static extern int Deflate(IntPtr strm, int flush);

        [DllImport("thirdparty/zlibwapi.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "deflateEnd")]
        public static extern int DeflateEnd(IntPtr strm);

        [DllImport("thirdparty/zlibwapi.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "deflateBound")]
        public static extern int DeflateBound(IntPtr strm, ulong sourceLen);
    }
}
