using System.Collections.Generic;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionBlock
	{
		public string WorkOrderNumber { get; set; } = "";

		public List<string> BlockingItems { get; set; } = new();
		public string BlockingItemsText => string.Join(", ", BlockingItems);

		public double TotalShortage { get; set; }
	}
}