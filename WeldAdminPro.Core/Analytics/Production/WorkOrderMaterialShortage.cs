using System;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class WorkOrderMaterialShortage
	{
		public Guid WorkOrderId { get; set; }

		public string WorkOrderNumber { get; set; } = "";

		public Guid StockItemId { get; set; }

		public string ItemCode { get; set; } = "";

		public string ItemName { get; set; } = "";

		public decimal RequiredQuantity { get; set; }

		public decimal AvailableQuantity { get; set; }

		public decimal ShortageQuantity { get; set; }
	}
}