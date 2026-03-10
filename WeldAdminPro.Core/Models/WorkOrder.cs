using System;

namespace WeldAdminPro.Core.Models
{
	public class WorkOrder
	{
		public Guid Id { get; set; }

		public Guid ProjectId { get; set; }

		public string WorkOrderNumber { get; set; } = "";
		public string Description { get; set; } = "";

		public DateTime StartDate { get; set; } = DateTime.Today;
		public double EstimatedHours { get; set; }
		public WorkOrderStatus Status { get; set; }

		public DateTime CreatedOn { get; set; }

		public DateTime? CompletedOn { get; set; }
		public DateTime? PlannedStartDate { get; set; }
		public DateTime? DueDate { get; set; }
		public int Priority { get; set; }

		public DateTime? ActualStartTime { get; set; }

		public DateTime? ActualEndTime { get; set; }

		public double ActualHours { get; set; }

		public bool IsPaused { get; set; }
	}

	public enum WorkOrderStatus
	{
		Open,
		Ready,
		InProduction,
		Completed
	}
}