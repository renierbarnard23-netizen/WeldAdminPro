using System;

namespace WeldAdminPro.Domain.Entities
{
    public class Weld
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public string WeldNumber { get; set; } = string.Empty;

        public string DrawingNumber { get; set; } = string.Empty;

        public string JointType { get; set; } = string.Empty;

        public string WpsNumber { get; set; } = string.Empty;

        public string WelderNumber { get; set; } = string.Empty;

        public string MaterialHeat1 { get; set; } = string.Empty;

        public string MaterialHeat2 { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public string NdtStatus { get; set; } = "Not Tested";

        public int RepairCount { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}