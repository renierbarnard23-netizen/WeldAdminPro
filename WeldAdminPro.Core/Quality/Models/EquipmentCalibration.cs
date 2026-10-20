using System;

namespace WeldAdminPro.Core.Quality.Models
{
    public class EquipmentCalibration
    {
        public Guid Id { get; set; }

        public Guid EquipmentId { get; set; }
        public Guid CompanyId { get; set; }

        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime CalibrationDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.Date.AddYears(1);

        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovedDate { get; set; }

        public string Provider { get; set; } = string.Empty;
        public string Standard { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public Guid? DocumentVaultFileId { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
