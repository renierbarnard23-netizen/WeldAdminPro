using System;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Quality.Models
{
    public class CapaRecord
    {
        public Guid Id { get; set; }

        public Guid NcrId { get; set; }

        public string CapaNumber { get; set; }
            = string.Empty;

        public string Title { get; set; }
            = string.Empty;

        public string RootCause { get; set; }
            = string.Empty;

        public string CorrectiveAction { get; set; }
            = string.Empty;

        public string PreventiveAction { get; set; }
            = string.Empty;

        public string AssignedTo { get; set; }
            = string.Empty;

        public DateTime DueDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public string CreatedBy { get; set; }
            = string.Empty;

        public string VerifiedBy { get; set; }
            = string.Empty;

        public DateTime? VerifiedDate { get; set; }

        public bool IsEffective { get; set; }

        public CapaPriority Priority { get; set; }

        public CapaStatus Status { get; set; }
    }
}