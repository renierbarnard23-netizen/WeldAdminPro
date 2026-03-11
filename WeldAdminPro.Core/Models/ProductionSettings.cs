namespace WeldAdminPro.Core.Models
{
	public class ProductionSettings
	{
		public int Workers { get; set; } = 4;

		public double HoursPerDay { get; set; } = 8;

		public double OvertimeHours { get; set; } = 0;

		public int Shifts { get; set; } = 1;

		public double DailyCapacity =>
			Workers * (HoursPerDay + OvertimeHours) * Shifts;
	}
}