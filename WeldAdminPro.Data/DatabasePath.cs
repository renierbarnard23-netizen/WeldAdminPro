using System;
using System.IO;

namespace WeldAdminPro.Data
{
	public static class DatabasePath
	{
		public static string Get()
		{
			var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var appDir = Path.Combine(baseDir, "WeldAdminPro");

			if (!Directory.Exists(appDir))
				Directory.CreateDirectory(appDir);

			return Path.Combine(appDir, "weldadmin.db");
		}
	}
}
