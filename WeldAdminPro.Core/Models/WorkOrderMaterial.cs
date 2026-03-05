using System;

namespace WeldAdminPro.Core.Models
{
	public class WorkOrderMaterial
	{
		public Guid Id { get; set; }

		public Guid WorkOrderId { get; set; }

		public Guid ItemId { get; set; }

		public double Quantity { get; set; }

		public DateTime IssuedOn { get; set; }
	}
}