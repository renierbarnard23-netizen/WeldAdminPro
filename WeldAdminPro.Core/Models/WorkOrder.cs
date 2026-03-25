using System;

namespace WeldAdminPro.Core.Models
{
	public class WorkOrder
	{
		public Guid Id { get; set; }

		public Guid ProjectId { get; set; }

		public string WorkOrderNumber { get; set; } = "";
		public string Description { get; set; } = "";
		public string ProjectName { get; set; } = "";

		public DateTime StartDate { get; set; } = DateTime.Today;
		public double EstimatedHours { get; set; }
		public WorkOrderStatus Status { get; set; }

		public DateTime CreatedOn { get; set; }

		public DateTime? CompletedOn { get; set; }
		public DateTime? PlannedStartDate { get; set; }
		public DateTime? DueDate { get; set; }
		public int Priority { get; set; }
		public List<Guid> DependencyIds { get; set; } = new();
		public DateTime? ActualStartTime { get; set; }
		public DateTime? ActualEndTime { get; set; }
		public double ActualHours { get; set; }
		public bool IsPaused { get; set; }
		public WorkOrderType Type { get; set; }
		public string? BlockReason { get; set; }
	}

	public enum WorkOrderStatus
	{
		Ready = 1,
		InProduction = 2,
		Paused = 3,
		Completed = 4
	}

	public enum WorkOrderType
	{
		Production,   // consumes materials
		Procurement,  // creates or orders materials
		Internal      // optional (machining, cutting, etc.)
	}
}