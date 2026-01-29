using System;

namespace WeldAdminPro.Core.Models
{
	public class DuplicateStockItem
	{
		public Guid Id { get; set; }

		public string ItemCode { get; set; } = "";

		public string Description { get; set; } = "";

		public int Quantity { get; set; }

		public string Unit { get; set; } = "";

		public string Category { get; set; } = "Uncategorised";
	}
}
