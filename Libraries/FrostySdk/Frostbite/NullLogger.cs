﻿using FrostySdk.Interfaces;
using System.Diagnostics;

namespace FrostySdk.Frostbite
{
    public class NullLogger : ILogger
    {
        public void Log(string text, params object[] vars)
        {
            Debug.WriteLine(text);
        }

        public void LogError(string text, params object[] vars)
        {
            Debug.WriteLine(text);
        }

        public void LogWarning(string text, params object[] vars)
        {
            Debug.WriteLine(text);
        }
    }
}
