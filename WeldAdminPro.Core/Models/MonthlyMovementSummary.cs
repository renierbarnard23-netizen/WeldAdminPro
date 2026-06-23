using System;

namespace WeldAdminPro.Core.Models
{
	public class MonthlyMovementSummary
	{
		public DateTime Month { get; set; }

		public int TotalIn { get; set; }
		public int TotalOut { get; set; }
		public int NetMovement => TotalIn - TotalOut;

		public decimal ValueIn { get; set; }
		public decimal ValueOut { get; set; }
		public decimal NetValue => ValueIn - ValueOut;
	}
}
