using System;

namespace WeldAdminPro.Core.Quality.Models
{
    public class InspectionTestPlanExecution
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid InspectionTestPlanItemId { get; set; }

        public string ExecutionType { get; set; } = string.Empty;

        public string ExecutionRecordId { get; set; } = string.Empty;

        public DateTime LinkedOn { get; set; } = DateTime.UtcNow;

        public string? LinkedBy { get; set; }

        public string? Notes { get; set; }
    }
}
