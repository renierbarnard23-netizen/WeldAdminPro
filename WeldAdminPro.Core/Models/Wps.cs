using System;

namespace WeldAdminPro.Core.Models
{
    public class Wps
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProjectId { get; set; }
        public string WpsNumber { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string BaseMaterial { get; set; } = string.Empty;
        public string FillerMaterial { get; set; } = string.Empty;
        public string Process { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Preheat { get; set; } = string.Empty;
        public string PostHeat { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
    }
}
