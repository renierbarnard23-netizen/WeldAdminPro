using System;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Models
{
    public class RepairRecord
    {
        public Guid Id { get; set; }
        public Guid WeldId { get; set; }

        
        // NCR that originated this repair.
        // Null is valid for legacy or manually created repairs.
        public Guid? NcrId { get; set; }
public int RepairNumber { get; set; }

        public string Reason { get; set; }
            = string.Empty;

        public string AuthorizedBy { get; set; }
            = string.Empty;

        public DateTime RequestedDate { get; set; }
            = DateTime.UtcNow;

        public DateTime? AuthorizedDate { get; set; }

        public string ExcavationMethod { get; set; }
            = string.Empty;

        public string RepairWpsNumber { get; set; }
            = string.Empty;

        public string RepairedByWelder { get; set; }
            = string.Empty;

        public string ReinspectionResult { get; set; }
            = string.Empty;

        public string Notes { get; set; }
            = string.Empty;

        public RepairStatus Status { get; set; }
            = RepairStatus.Requested;

        public DateTime? CompletedDate { get; set; }
    }

}

