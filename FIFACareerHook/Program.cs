using RGiesecke.DllExport;
using System;
using System.IO;

namespace FIFACareerHook
{
    public class Program
    {
        [DllExport("DllMain")]
        static void Main(string[] args)
        {
            File.WriteAllText("test.txt", "TEST c#");
        }

        static int _number;
        static Program()
        {
            File.WriteAllText("test.txt", "TEST c#");
            _number = 5;
        }
    }
}
