using System;

namespace WeldAdminPro.Core.Models
{
    public class PqrRecord
    {
        public Guid Id { get; set; }

        public string PqrNumber { get; set; } = "";
        public int Revision { get; set; }

        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = "";

        public bool IsApproved { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? ApprovedBy { get; set; }

        public bool IsLocked { get; set; }

        // Snapshot of key technical data (keep it simple first)
        public string Standard { get; set; } = "";
        public double TestThickness { get; set; }
        public double QualifiedMinThickness { get; set; }
        public double QualifiedMaxThickness { get; set; }

        public string JointType { get; set; } = "";
        public string JointDesign { get; set; } = "";

        public int? BasePNo { get; set; }
        public int? FNo { get; set; }
        public int? ANo { get; set; }
    }
}