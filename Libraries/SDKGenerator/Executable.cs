using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FrostyEditor.IO
{
    public class Executable : IDisposable
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct IMAGE_DOS_HEADER
        {
            [FieldOffset(60)]
            public int e_lfanew;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct IMAGE_FILE_HEADER
        {
            [FieldOffset(0)]
            public ushort Machine;

            [FieldOffset(2)]
            public ushort NumberOfSections;

            [FieldOffset(4)]
            public uint TimeDateStamp;

            [FieldOffset(8)]
            public uint PointerToSymbolTable;

            [FieldOffset(12)]
            public uint NumberOfSymbols;

            [FieldOffset(16)]
            public ushort SizeOfOptionalHeader;

            [FieldOffset(18)]
            public ushort Characteristics;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct IMAGE_OPTIONAL_HEADER64
        {
            [FieldOffset(0)]
            public ushort Magic;

            [FieldOffset(24)]
            public ulong ImageBase;

            [FieldOffset(224)]
            public IMAGE_DATA_DIRECTORY DataDirectory;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct IMAGE_NT_HEADERS64
        {
            [FieldOffset(0)]
            public uint Signature;

            [FieldOffset(4)]
            public IMAGE_FILE_HEADER FileHeader;

            [FieldOffset(24)]
            public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct IMAGE_DATA_DIRECTORY
        {
            [FieldOffset(0)]
            public uint VirtualAddress;

            [FieldOffset(4)]
            public uint Size;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct IMAGE_SECTION_HEADER
        {
            [FieldOffset(0)]
            public ulong pName;

            [FieldOffset(8)]
            public uint Address;

            [FieldOffset(12)]
            public uint VirtualAddress;

            [FieldOffset(16)]
            public uint SizeOfRawData;

            [FieldOffset(20)]
            public uint PointerToRawData;

            [FieldOffset(24)]
            public uint PointerToRelocations;

            [FieldOffset(28)]
            public uint PointerToLinenumbers;

            [FieldOffset(32)]
            public ushort NumberOfRelocations;

            [FieldOffset(34)]
            public ushort NumberOfLinenumbers;

            [FieldOffset(36)]
            public uint Characteristics;

            public string Name => Encoding.ASCII.GetString(BitConverter.GetBytes(pName));

            public uint VirtualSize => Address;
        }

        private IMAGE_DOS_HEADER dosHeader;

        private IMAGE_NT_HEADERS64 ntHeaders;

        private List<IMAGE_SECTION_HEADER> sectionHeaders = new List<IMAGE_SECTION_HEADER>();

        private byte[] lrgBuffer;

        private unsafe byte* bufPtr = null;

        public unsafe Executable(string path)
        {
            new FileInfo(path);
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            lrgBuffer = new byte[fileStream.Length];
            fileStream.Read(lrgBuffer, 0, (int)fileStream.Length);
            fileStream.Flush();
            fileStream.Close();
            bufPtr = (byte*)(void*)Marshal.UnsafeAddrOfPinnedArrayElement(lrgBuffer, 0);
            dosHeader = *(IMAGE_DOS_HEADER*)bufPtr;
            ntHeaders = *(IMAGE_NT_HEADERS64*)(bufPtr + dosHeader.e_lfanew);
            for (int i = 0; i < ntHeaders.FileHeader.NumberOfSections; i++)
            {
                sectionHeaders.Add(*(IMAGE_SECTION_HEADER*)(bufPtr + dosHeader.e_lfanew + 24 + ntHeaders.FileHeader.SizeOfOptionalHeader + i * sizeof(IMAGE_SECTION_HEADER)));
            }
        }

        public unsafe void getBytes(long offset, byte[] buf, int count)
        {
            for (int i = 0; i < count; i++)
            {
                buf[i] = (bufPtr + offset)[i];
            }
        }

        public long getOffset(long offset)
        {
            return adjustOffset(offset);
        }

        private long adjustOffset(long offset)
        {
            for (int i = 0; i < sectionHeaders.Count; i++)
            {
                if ((ulong)offset >= ntHeaders.OptionalHeader.ImageBase + sectionHeaders[i].VirtualAddress && (ulong)offset < ntHeaders.OptionalHeader.ImageBase + sectionHeaders[i].VirtualAddress + sectionHeaders[i].VirtualSize)
                {
                    offset -= sectionHeaders[i].VirtualAddress - sectionHeaders[i].PointerToRawData;
                    offset -= (long)ntHeaders.OptionalHeader.ImageBase;
                    break;
                }
            }
            return offset;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected unsafe virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                sectionHeaders.Clear();
                lrgBuffer = null;
                bufPtr = null;
            }
        }
    }
}
