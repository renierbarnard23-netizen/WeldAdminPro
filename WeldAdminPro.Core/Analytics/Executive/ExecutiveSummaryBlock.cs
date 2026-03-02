using WeldAdminPro.Core.Enums;

namespace WeldAdminPro.Core.Analytics.Executive
{
	public class ExecutiveSummaryBlock
	{
		public string Title { get; set; } = string.Empty;
		public ExecutiveSeverityLevel Severity { get; set; }
		public string Paragraph { get; set; } = string.Empty;
	}
}