using FrostySdk;
using FrostySdk.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FIFA21Plugin
{
    public class BinarySbWriter_M21
    {

        public void Write(DbObject dbObject, string filePath)
        {
            MemoryStream ms = new MemoryStream();
            using (NativeWriter nwMemory = new NativeWriter(ms, leaveOpen: true))
            {


                
            }


            byte[] bytesToWrite = null;
            using (var nr = new NativeReader(ms))
                bytesToWrite = nr.ReadToEnd();

            using (NativeWriter nwfile = new NativeWriter(new FileStream(filePath, FileMode.Open)))
            {
                nwfile.Position = nwfile.Length;
            }
        }
    }
}
