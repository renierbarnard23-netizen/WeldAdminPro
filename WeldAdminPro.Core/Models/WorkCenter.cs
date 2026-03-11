namespace WeldAdminPro.Core.Models
{
	public class WorkCenter
	{
		public Guid Id { get; set; }

		public string Name { get; set; } = "";

		public int Workers { get; set; }

		public double HoursPerDay { get; set; }

		public double OvertimeHours { get; set; }

		public double DailyCapacity =>
			Workers * (HoursPerDay + OvertimeHours);
	}
}