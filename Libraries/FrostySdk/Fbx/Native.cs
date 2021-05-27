using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
	internal static class Native
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr LoadLibrary(string lpszLib);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr FreeLibrary(IntPtr hModule);
	}
}
