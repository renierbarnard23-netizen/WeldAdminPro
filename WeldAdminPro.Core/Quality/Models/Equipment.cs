using System;

namespace WeldAdminPro.Core.Quality.Models
{
    public class Equipment
    {
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }

        public string EquipmentNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public bool CalibrationRequired { get; set; } = true;
        public int CalibrationIntervalDays { get; set; } = 365;

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }
    }
}
