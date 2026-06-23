namespace WeldAdminPro.Core.Analytics.Procurement
{
	public class ProcurementSuggestion
	{
		public string ItemCode { get; set; } = "";

		public string Description { get; set; } = "";

		public decimal CurrentStock { get; set; }

		public decimal RequiredQuantity { get; set; }

		public decimal SuggestedOrderQuantity { get; set; }

		public int PriorityScore { get; set; }

		public string Reason { get; set; } = "";
	}
}