using FrostbiteSdk;

namespace FrostyEditor.IO
{
    public class ExeReader : MemoryReader
    {
        private Executable theExe;

        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                position = theExe.getOffset(value);
            }
        }

        public ExeReader(Executable exe)
        {
            theExe = exe;
        }

        public override void Dispose()
        {
            theExe.Dispose();
        }

        protected override void FillBuffer(int numBytes)
        {
            theExe.getBytes(position, buffer, numBytes);
            position += numBytes;
        }
    }
}
