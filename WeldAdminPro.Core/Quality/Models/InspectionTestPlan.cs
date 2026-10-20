using System;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Models
{
    public class InspectionTestPlan
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public string ItpNumber { get; set; } = string.Empty;

        public string Revision { get; set; } = "A";

        public InspectionTestPlanStatus Status { get; set; } =
            InspectionTestPlanStatus.Draft;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? PreparedBy { get; set; }

        public DateTime? PreparedOn { get; set; }

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedOn { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? SupersededOn { get; set; }

        public Guid? PreviousRevisionId { get; set; }

        public bool IsActive { get; set; }
    }
}
