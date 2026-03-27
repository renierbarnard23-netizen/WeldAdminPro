using System;

namespace WeldAdminPro.Core.Models
{
	public class WorkOrderMaterialTrace
	{
		public string ItemCode { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public int Quantity { get; set; }

		public decimal UnitCost { get; set; }

		public decimal TotalCost => Quantity * UnitCost;

		public string? WorkOrderNumber { get; set; }
	}
}