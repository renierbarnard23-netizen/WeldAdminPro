using System;
using System.IO;

namespace WeldAdminPro.Data
{
	public static class DatabasePath
	{
		private const string DatabaseFileName = "weldadmin.db";

		// 🔒 Cache the resolved path for the lifetime of the app
		private static readonly string _databasePath = BuildPath();

		public static string Get()
		{
			return _databasePath;
		}

		private static string BuildPath()
		{
			var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			var appDir = Path.Combine(baseDir, "WeldAdminPro");

			if (!Directory.Exists(appDir))
				Directory.CreateDirectory(appDir);

			return Path.Combine(appDir, DatabaseFileName);
		}
	}
}
