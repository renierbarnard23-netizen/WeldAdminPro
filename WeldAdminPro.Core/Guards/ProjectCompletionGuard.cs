using System;
using WeldAdminPro.Core.Models;

namespace WeldAdminPro.Core.Guards
{
	public static class ProjectCompletionGuard
	{
		public static void ValidateBeforeSave(Project project)
		{
			if (project == null)
				throw new ArgumentNullException(nameof(project));

			if (project.Status == ProjectStatus.Completed)
			{
				if (string.IsNullOrWhiteSpace(project.InvoiceNumber))
					throw new InvalidOperationException(
						"An invoice number is required before a project can be completed.");
			}
		}

		public static bool IsEditable(Project project)
{
        if (project == null) return false;

        // Active, On Hold and Completed projects remain editable.
        // Cancelled projects are permanently locked.
        return project.Status != ProjectStatus.Cancelled;
}
	}
}

