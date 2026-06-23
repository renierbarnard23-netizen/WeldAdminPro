using System;
using WeldAdminPro.Core.Quality.Enums;

namespace WeldAdminPro.Core.Models
{
    public class Weld
    {
        public Guid Id { get; set; }

        public Guid ProjectId { get; set; }

        public string WeldNumber { get; set; } = string.Empty;

        public string WeldType
        {
            get;
            set;
        } = string.Empty;

        public string DrawingNumber { get; set; } = string.Empty;

        public string JointType { get; set; } = string.Empty;

        public string WpsNumber { get; set; } = string.Empty;

        public string JointNumber { get; set; } = "";

        public string MaterialSpecification { get; set; } = "";

        public double Diameter { get; set; }

        public string WelderNumber { get; set; } = string.Empty;

        public string MaterialHeat1 { get; set; } = string.Empty;

        public string MaterialHeat2 { get; set; } = string.Empty;

        public WeldStatusType Status { get; set; } = WeldStatusType.Pending;

        public string NdtStatus { get; set; } = "Not Tested";

        public int RepairCount { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? NdtPendingDate
        {
            get;
            set;
        }

        public string Process { get; set; } = "";

        public string MaterialGroup { get; set; } = "";

        public string Position { get; set; } = "";

        public double Thickness { get; set; }

        public bool IsValid { get; set; }

        public string ValidationMessage { get; set; } = "";
        public int RepairCycle { get; set; }

        public bool RequiresRepair { get; set; }

        public DateTime? LastNdtDate { get; set; }

        public string? LastNdtResult { get; set; }

        public WeldWorkflowStatus WorkflowStatus { get; set; }
            = WeldWorkflowStatus.Draft;

        public bool ReleaseReady
        {
            get;
            set;
        }

        public bool TurnoverReady
        {
            get;
            set;
        }

        public int BlockingCount
        {
            get;
            set;
        }

        public string ReadinessSummary
        {
            get;
            set;
        } = string.Empty;

        public string ReleasedBy
        {
            get;
            set;
        } = string.Empty;

        public DateTime? ReleasedDate
        {
            get;
            set;
        }

        public bool IsReleased
        {
            get;
            set;
        }

        public WeldReleaseRole RequiredReleaseRole
        {
            get;
            set;
        }
            = WeldReleaseRole.QA;

        public string DefectType
        { get; set; }
        = "";

    }
}