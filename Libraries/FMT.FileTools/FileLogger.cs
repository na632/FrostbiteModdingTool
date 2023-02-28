using System;
using System.IO;

namespace FMT.FileTools
{
    public class FileLogger
    {
        public static DateTime LoggerStartDateTime { get; } = DateTime.Now;

        public static FileLogger Instance { get; } = new FileLogger();

        public static string GetFileLoggerPath()
        {
            var fmtLogPath = $"FMT.Log.{LoggerStartDateTime.ToString("yyyy-MM-dd.HH-mm")}.log";
            return fmtLogPath;
        }

        public FileLogger()
        {
            if (!File.Exists(GetFileLoggerPath()))
                File.WriteAllText(GetFileLoggerPath(), "");
        }

        public static void WriteLine(string text, bool prefixDateTime = true)
        {
            using (var nw = new NativeWriter(new FileStream(GetFileLoggerPath(), FileMode.Open)))
            {
                nw.Position = nw.Length;
                if (prefixDateTime)
                    nw.WriteLine($"[{DateTime.Now.ToString()}]: {text}");
                else
                    nw.WriteLine(text);
            }
        }
    }
}
