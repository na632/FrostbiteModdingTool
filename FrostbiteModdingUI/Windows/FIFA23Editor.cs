using FIFAModdingUI;
using FIFAModdingUI.Windows;
using FrostySdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace FMT.Windows
{
    public class FIFA23Editor : FIFA22Editor
    {
        public FIFA23Editor(Window owner) : base(owner)
        {

        }

        public override string LastGameLocation
        {
            get
            {
                var dir = ProfilesLibrary.GetModProfileParentDirectoryPath() + "\\FIFA23\\";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                return dir + "LastLocation.json";
            }
        }
    }
}
