using System;
using System.IO;

namespace Modding.Utilities
{
	public static class ModdingSupportHelper
	{
		public static bool TryCheckIsModdingSupported(string gamePath, out (bool supported, string format) result)
		{
			try
			{
				string root = Path.GetPathRoot(new FileInfo(gamePath).FullName);
				if (!string.IsNullOrEmpty(root))
				{
					DriveInfo driveInfo = new DriveInfo(root);
					string format = driveInfo.DriveFormat;
					if (format != null && format.Equals("NTFS", StringComparison.OrdinalIgnoreCase))
					{
						result = (true, format);
						return true;
					}
					result = (false, driveInfo.DriveFormat);
					return true;
				}
			}
			catch (Exception ex)
			{
				//Log.Logger.Warning(ex, "Failed to check if game directory is on a supported drive.");
			}
			result = default((bool, string));
			return false;
		}
	}
}
