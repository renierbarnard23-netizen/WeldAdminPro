public class PlanningResult
{
	public List<PlannedWorkOrder> Sequence { get; set; } = new();

	public double TotalDelayHours { get; set; }

	public int LateJobs { get; set; }
}

public class PlannedWorkOrder
{
	public Guid WorkOrderId { get; set; }

	public string WorkOrderNumber { get; set; } = "";

	public DateTime Start { get; set; }

	public DateTime End { get; set; }

	public bool IsLate { get; set; }
}