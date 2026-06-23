namespace WeldAdminPro.Core.Analytics.Executive
{
	public class OperationalAlert
	{
		public string Message { get; set; } = "";

		public string Severity { get; set; } = "Info";

		public DateTime Timestamp { get; set; } = DateTime.Now;
	}
}