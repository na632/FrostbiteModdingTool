using System;
using System.IO;
using System.Reflection;

namespace FrostySdk.Frosty.FET
{
    public static class FETUtil
    {

    }

    public static class EnumHelper
    {
        public static string EnumToString(Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString(), BindingFlags.Static | BindingFlags.Public);
            return fieldInfo.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>(inherit: false)?.Description ?? fieldInfo.Name;
        }
    }

    public class SubStream : Stream
    {
        private readonly long length;

        private Stream baseStream;

        private long position;

        public override long Length
        {
            get
            {
                CheckDisposed();
                return length;
            }
        }

        public override bool CanRead
        {
            get
            {
                CheckDisposed();
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                CheckDisposed();
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                CheckDisposed();
                return false;
            }
        }

        public override long Position
        {
            get
            {
                CheckDisposed();
                return position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public SubStream(Stream baseStream, long offset, long length)
        {
            if (baseStream == null)
            {
                throw new ArgumentNullException("baseStream");
            }
            if (!baseStream.CanRead)
            {
                throw new ArgumentException("can't read base stream");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            this.baseStream = baseStream;
            this.length = length;
            if (baseStream.CanSeek)
            {
                baseStream.Seek(offset, SeekOrigin.Begin);
                return;
            }
            byte[] buffer = new byte[512];
            while (offset > 0)
            {
                int read = baseStream.Read(buffer, 0, (int)((offset < 512) ? offset : 512));
                offset -= read;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            long remaining = length - position;
            if (remaining <= 0)
            {
                return 0;
            }
            if (remaining < count)
            {
                count = (int)remaining;
            }
            int read = baseStream.Read(buffer, offset, count);
            position += read;
            return read;
        }

        private void CheckDisposed()
        {
            if (baseStream == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            CheckDisposed();
            baseStream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && baseStream != null)
            {
                try
                {
                    baseStream.Dispose();
                }
                catch
                {
                }
                baseStream = null;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }

}
