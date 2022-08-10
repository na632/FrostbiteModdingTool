using System;
using System.Runtime.InteropServices;

namespace FrostbiteSdk
{
	public class FbxSystemUnit : FbxNative
	{
		private static FbxSystemUnit mMillimeters;

		private static FbxSystemUnit mCentimeters;

		private static FbxSystemUnit mMeters;

		private static FbxSystemUnit mKilometers;

		public double ScaleFactor => GetScaleFactorInternal(pHandle);

		public static FbxSystemUnit Millimeters => GetStaticValue("?mm@FbxSystemUnit@fbxsdk@@2V12@B", ref mMillimeters);

		public static FbxSystemUnit Centimeters => GetStaticValue("?cm@FbxSystemUnit@fbxsdk@@2V12@B", ref mCentimeters);

		public static FbxSystemUnit Meters => GetStaticValue("?m@FbxSystemUnit@fbxsdk@@2V12@B", ref mMeters);

		public static FbxSystemUnit Kilometers => GetStaticValue("?km@FbxSystemUnit@fbxsdk@@2V12@B", ref mKilometers);

		[DllImport("ThirdParty/libfbxsdk", EntryPoint = "?GetScaleFactor@FbxSystemUnit@fbxsdk@@QEBANXZ")]
		private static extern double GetScaleFactorInternal(IntPtr InHandle);

		public FbxSystemUnit()
		{
		}

		public FbxSystemUnit(IntPtr InHandle)
			: base(InHandle)
		{
		}

		private static FbxSystemUnit GetStaticValue(string Sig, ref FbxSystemUnit OutUnit)
		{
			if (OutUnit == null)
			{
				IntPtr hModule = Native.LoadLibrary("libfbxsdk.dll");
				try
				{
					OutUnit = new FbxSystemUnit(Native.GetProcAddress(hModule, Sig));
				}
				finally
				{
					Native.FreeLibrary(hModule);
				}
			}
			return OutUnit;
		}
	}
}
