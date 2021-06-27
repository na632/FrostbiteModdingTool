using FIFAModdingUI;
using FIFAModdingUI.Windows;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace FMT.Windows
{
    public class FIFA20Editor : FIFA21Editor
    {
        public FIFA20Editor(Window owner) : base(owner)
        {

        }

        public override string LastGameLocation => App.ApplicationDirectory + "FIFA20LastLocation.json";
    }
}
