public class SimulationResult
{
	public string WorkOrderNumber { get; set; } = "";

	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }

	public DateTime Deadline { get; set; }

	public bool IsLate => EndDate > Deadline;

	public int DelayDays => IsLate
		? (EndDate - Deadline).Days
		: 0;
}