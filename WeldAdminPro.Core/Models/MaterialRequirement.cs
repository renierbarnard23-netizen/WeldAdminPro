using System;

namespace WeldAdminPro.Core.Models
{
	public class MaterialRequirement
	{
		public Guid Id { get; set; }

		public Guid WorkOrderId { get; set; }

		public string ItemCode { get; set; } = "";

		public double RequiredQuantity { get; set; }
	}
}