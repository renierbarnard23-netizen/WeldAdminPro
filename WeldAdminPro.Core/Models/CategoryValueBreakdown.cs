namespace WeldAdminPro.Core.Models
{
	public class CategoryValueBreakdown
	{
		public string Category { get; set; } = string.Empty;

		public decimal TotalValue { get; set; }

		public int TotalUnits { get; set; }
	}
}
