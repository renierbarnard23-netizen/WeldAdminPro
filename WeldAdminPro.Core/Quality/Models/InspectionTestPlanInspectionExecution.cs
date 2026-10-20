using System;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Models
{
    public class InspectionTestPlanInspectionExecution
    {
        public Guid Id { get; set; }

        public Guid InspectionTestPlanItemId { get; set; }

        public Guid WeldId { get; set; }

        public InspectionTestPlanInspectionExecutionStatus Status { get; set; }

        public InspectionTestPlanInspectionResult Result { get; set; }

        public DateTime? InspectionDate { get; set; }

        public string InspectorName { get; set; } = string.Empty;

        public string Findings { get; set; } = string.Empty;

        public string Remarks { get; set; } = string.Empty;

        public string ExecutedBy { get; set; } = string.Empty;

        public DateTime ExecutedOn { get; set; }

        public string? CompletedBy { get; set; }

        public DateTime? CompletedOn { get; set; }
    }
}
