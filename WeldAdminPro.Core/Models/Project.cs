using System;

namespace WeldAdminPro.Core.Models
{
	public enum ProjectStatus
	{
		Active = 0,
		Completed = 1,
		Cancelled = 2,
		OnHold = 3,
	}

	public class Project
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public int JobNumber { get; set; }

		public string ProjectName { get; set; } = string.Empty;
		public string Client { get; set; } = string.Empty;
		public string ClientRepresentative { get; set; } = string.Empty;
				
		public string QuoteNumber { get; set; } = string.Empty;
		public string OrderNumber { get; set; } = string.Empty;

		public string Material { get; set; } = string.Empty;
		public string AssignedTo { get; set; } = string.Empty;

		public bool IsInvoiced { get; set; }
		public string? InvoiceNumber { get; set; }

		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		// ✅ Completion date (NEW)
		
		public ProjectStatus Status { get; set; } = ProjectStatus.Active;

		public DateTime CreatedOn { get; set; } = DateTime.Now;

		// Financial
		public decimal Budget { get; set; }          // Contract value
		public decimal ActualCost { get; set; }      // Auto calculated
		public decimal CommittedCost { get; set; }   // Allocated but not used

		// Lifecycle
		public DateTime? CompletedOn { get; set; }
		public DateTime? LastModifiedOn { get; set; }
		public bool IsArchived { get; set; }

	}
}
