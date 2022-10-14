using MinHook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FIFA23Plugin
{
    public class FIFA23Plugin
    {
        [STAThread]
        public static void Main()
        {
            File.WriteAllText("_testInj.abc", "");
        }

        //[STAThread]
        //static void Main(string[] args)
        //{
        //    File.WriteAllText("_testInj.abc", "");
        //    //using (HookEngine engine = new HookEngine())
        //    //{
        //    //    //engine.
        //    //    //MessageBoxW_orig = engine.CreateHook("user32.dll", "MessageBoxW", new MessageBoxWDelegate(MessageBoxW_Detour));
        //    //    //engine.EnableHooks();

        //    //    ////Call the PInvoke import to test our hook is in place
        //    //    //MessageBoxW(IntPtr.Zero, "Text", "Caption", 0);
        //    //}
        //}
    }
}
