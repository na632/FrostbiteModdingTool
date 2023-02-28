using System;
using System.IO;

namespace FrostbiteSdk.Extras
{

    public class MemoryUtils : IDisposable
    {
        public FastStream.FastMemoryWriter MemoryWriter;// = new FastStream.FastMemoryWriter();

        //public MemoryUtils()
        //{
        //    MemoryWriter = new FastStream.FastMemoryWriter();
        //}

        public void Write(byte[] data)
        {
            if (MemoryWriter == null)
                MemoryWriter = new FastStream.FastMemoryWriter();
            MemoryWriter.Write(data);
        }

        public MemoryStream GetMemoryStream()
        {
            if (MemoryWriter == null)
                MemoryWriter = new FastStream.FastMemoryWriter();

            return new MemoryStream(MemoryWriter.ToArray());
        }



        // To detect redundant calls
        private bool _disposed = false;

        ~MemoryUtils() => Dispose(false);

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            // Managed Resources
            if (disposing)
            {
                if (MemoryWriter != null)
                {
                    MemoryWriter.Close();
                    MemoryWriter.Dispose();
                    MemoryWriter = null;

                }
            }
        }
    }
}
