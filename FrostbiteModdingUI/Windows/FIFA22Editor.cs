using FIFAModdingUI;
using FIFAModdingUI.Windows;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace FMT.Windows
{
    public class FIFA22Editor : FIFA21Editor
    {
        public FIFA22Editor(Window owner) : base(owner)
        {

        }

        public override string LastGameLocation => App.ApplicationDirectory + "FIFA22LastLocation.json";
    }
}
