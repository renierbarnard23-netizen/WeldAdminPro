using System;

namespace WeldAdminPro.Core.Analytics.Production
{
	public class ProductionCapacityForecast
	{
		public DateTime Date { get; set; }

		public double CapacityHours { get; set; }

		public double ScheduledHours { get; set; }

		public double LoadPercentage { get; set; }

		public bool IsOverloaded => LoadPercentage > 100;
	}
}