using System;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldNdtResult
    {
        public Guid Id { get; set; }

        public Guid WeldId { get; set; }

        public Guid? EquipmentId { get; set; }

        public Guid? CalibrationRecordId { get; set; }

        public string EquipmentNumberSnapshot { get; set; } = string.Empty;
        public string EquipmentSerialNumberSnapshot { get; set; } = string.Empty;
        public string CalibrationCertificateSnapshot { get; set; } = string.Empty;

        public NdtMethodType NdtMethod { get; set; }

        public NdtResultType Result { get; set; }

        public DateTime InspectionDate { get; set; } = DateTime.Now;

        public string InspectorName { get; set; } = string.Empty;

        public string ReportNumber { get; set; } = string.Empty;

        public string AcceptanceCriteria { get; set; } = string.Empty;

        public string Remarks { get; set; } = string.Empty;

        public bool RequiresRepair { get; set; }

        public int RepairCycle { get; set; }

        public bool IsReinspection { get; set; }

        // W18: one NDT examination may use multiple controlled equipment items.
        // EquipmentId remains the primary/legacy equipment reference for compatibility.
        public List<Guid> EquipmentIds { get; set; } = new();

        public List<WeldNdtEquipmentUsage> EquipmentUsages { get; set; } = new();
    }
}