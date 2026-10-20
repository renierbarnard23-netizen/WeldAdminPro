using System;

namespace WeldAdminPro.Core.Quality.Models
{
    /// <summary>
    /// Immutable traceability snapshot for equipment used during one NDT examination.
    /// </summary>
    public class WeldNdtEquipmentUsage
    {
        public Guid Id { get; set; }
        public Guid WeldNdtResultId { get; set; }
        public Guid EquipmentId { get; set; }
        public Guid? CalibrationRecordId { get; set; }
        public string EquipmentNumberSnapshot { get; set; } = string.Empty;
        public string EquipmentSerialNumberSnapshot { get; set; } = string.Empty;
        public string CalibrationCertificateSnapshot { get; set; } = string.Empty;
    }
}
