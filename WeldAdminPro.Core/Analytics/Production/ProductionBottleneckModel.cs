namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionBottleneckModel
	{
		public string WorkOrderNumber { get; set; } = "";
		public string BottleneckType { get; set; } = "";
		public string Description { get; set; } = "";
		public string Severity { get; set; } = "";
		public string SuggestedAction { get; set; } = "";
		public string Resource { get; set; } = "";
	}
}