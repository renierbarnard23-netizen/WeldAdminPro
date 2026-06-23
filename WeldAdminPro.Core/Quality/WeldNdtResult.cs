using System;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;

namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldNdtResult
    {
        public Guid Id { get; set; }

        public Guid WeldId { get; set; }

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
    }
}