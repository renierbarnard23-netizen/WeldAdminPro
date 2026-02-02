using System;

namespace WeldAdminPro.Core.Models
{
	public enum ProjectStatus
	{
		Planned = 0,
		Active = 1,
		OnHold = 2,
		Completed = 3,
		Cancelled = 4
	}

	public class Project
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public int JobNumber { get; set; }

		public string ProjectName { get; set; } = string.Empty;

		public string Client { get; set; } = string.Empty;

		public string ClientRepresentative { get; set; } = string.Empty;

		public decimal Amount { get; set; }

		public string QuoteNumber { get; set; } = string.Empty;

		public string OrderNumber { get; set; } = string.Empty;

		public string Material { get; set; } = string.Empty;

		public string AssignedTo { get; set; } = string.Empty;

		// ✅ NEW — Phase 2
		public ProjectStatus Status { get; set; } = ProjectStatus.Planned;

		public bool IsInvoiced { get; set; }

		public string? InvoiceNumber { get; set; }

		public DateTime? StartDate { get; set; }

		public DateTime? EndDate { get; set; }

		public DateTime CreatedOn { get; set; } = DateTime.Now;
		public int StatusSortOrder =>
	Status switch
	{
		ProjectStatus.Active => 0,
		ProjectStatus.Planned => 1,
		ProjectStatus.OnHold => 2,
		ProjectStatus.Completed => 3,
		ProjectStatus.Cancelled => 4,
		_ => 99
	};

	}
}
