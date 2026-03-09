using System;

namespace WeldAdminPro.Core.Models
{
	public class WorkOrder
	{
		public Guid Id { get; set; }

		public Guid ProjectId { get; set; }

		public string WorkOrderNumber { get; set; }

		public string Description { get; set; }

		public WorkOrderStatus Status { get; set; }

		public DateTime CreatedOn { get; set; }

		public DateTime? CompletedOn { get; set; }
		public DateTime? PlannedStartDate { get; set; }
		public DateTime? DueDate { get; set; }
		public int Priority { get; set; }
	}

	public enum WorkOrderStatus
	{
		Open = 0,
		InProgress = 1,
		Completed = 2
	}
}