

namespace WeldAdminPro.Core.Quality
{
    public class Pqr
    {
        public Guid Id { get; set; }
        public string PqrNumber { get; set; } = string.Empty;
        public DateTime QualificationDate { get; set; }
        public string QualifiedBy { get; set; } = string.Empty;
        public double ThicknessTested { get; set; }
        public string Process { get; set; } = string.Empty;
        public string MaterialGroup { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;

        public string FillerMaterial { get; set; } = string.Empty;
        public string GasType { get; set; } = string.Empty;

        public string QualifiedPNumberRange { get; set; } = "";

        public double AmpsUsed { get; set; }
        public double VoltsUsed { get; set; }

        public double HeatInput { get; set; }

        public double Preheat { get; set; }
        public double Interpass { get; set; }
        public Guid? WpsId { get; set; }
        public bool PwhtPerformed { get; set; }
        public string PNumber { get; set; } = "";
        public double ThicknessQualifiedMin { get; set; }
        public double ThicknessQualifiedMax { get; set; }

        public string QualifiedRangeDisplay =>
    $"{ThicknessQualifiedMin:0.##} - {ThicknessQualifiedMax:0.##}";
        public string FNumber { get; set; } = "";

        public string QualifiedPosition { get; set; } = "";
        // Example: "1G", "2G", "5G", "6G"

        public string JointType { get; set; } = "";

        public double DiameterMin { get; set; }
        public double DiameterMax { get; set; }
        public string WpsReferenceNumber { get; set; } = "";
        public bool IsApproved { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? ApprovedBy { get; set; }
        public bool IsLocked { get; set; }
        public int Revision { get; set; }
        public bool IsActive { get; set; }

        // =========================
        // STANDARD + PROCESS
        // =========================
        public string Standard { get; set; } = "";

        // =========================
        // JOINT
        // =========================
        public string JointDesign { get; set; } = "";

        // =========================
        // JOINT VARIABLES
        // =========================
        public string SurfacePreparation { get; set; } = "";
        public double GrooveAngle { get; set; }
        public double RootFace { get; set; }
        public double RootGap { get; set; }
        public string Backing { get; set; } = "";
        public string BackGouging { get; set; } = "";
        public string JointDiagramPath { get; set; } = "";
        public string PassDiagramPath { get; set; } = "";
        public double GrooveRadius { get; set; }
        public double Misalignment { get; set; }
        public string BackingType { get; set; } = "";
        public string EdgePreparation { get; set; } = "";
        public string WeldingType { get; set; } = "";
        public string Progression { get; set; } = "";


        public string CurrentType { get; set; } = "";
        public double TravelSpeed { get; set; }
        public string Polarity { get; set; } = "";

        public string PreheatNotes { get; set; } = "";

        // =========================
        // Gas
        // =========================

        public string ShieldingGas { get; set; } = "";
        public double GasFlowRate { get; set; }

        public string BackingGas { get; set; } = "";
        public double BackingGasFlowRate { get; set; }
        public string PNumber2 { get; set; } = "";
    }
}