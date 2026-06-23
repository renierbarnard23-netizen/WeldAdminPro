using System.Collections.Generic;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionReadinessResult
	{
		public int TotalWorkOrders { get; set; }

		public int ReadyWorkOrders { get; set; }

		public int BlockedWorkOrders { get; set; }

		public double ReadinessPercentage { get; set; }

		public List<string> BlockedWorkOrderNumbers { get; set; } = new();
	}
}