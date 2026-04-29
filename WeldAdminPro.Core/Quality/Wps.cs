namespace WeldAdminPro.Core.Quality
{
    public class Wps
    {
        public Guid Id { get; set; }

        public string WpsNumber { get; set; } = "";
        public string Process { get; set; } = "";

        public double ThicknessMin { get; set; }
        public double ThicknessMax { get; set; }

        public double Diameter { get; set; }

        public string FillerMaterial { get; set; } = "";
        public string GasType { get; set; } = "";

        public double AmpsMin { get; set; }
        public double AmpsMax { get; set; }

        public double VoltsMin { get; set; }
        public double VoltsMax { get; set; }

        public double HeatInputMin { get; set; }
        public double HeatInputMax { get; set; }

        public double PreheatMin { get; set; }
        public double PreheatMax { get; set; }


        public double? InterpassMax { get; set; }
        public bool PwhtRequired { get; set; }

        public Guid? PqrId { get; set; }
        public string? LinkedPqrNumber { get; set; }

        public string? PNumber { get; set; }
        public string? FNumber { get; set; }
        public string? Position { get; set; }
        public string MaterialGroup { get; set; } = "";

        public string? JointType { get; set; }
        public string JointDesign { get; set; } = "";

        public string Backing { get; set; } = "";
        public string BackingType { get; set; } = "";

        public bool IsApproved { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string? ApprovedBy { get; set; }

        public int Revision { get; set; }
        public bool IsActive { get; set; }

        // 🔥 Compliance
        public bool IsCompliant { get; set; }
        public string ValidationStatus { get; set; } = "";

        // 🔥 Essential Variables
        public string CurrentType { get; set; } = "";
        public string Polarity { get; set; } = "";
        public string Progression { get; set; } = "";

        // 🔥 Visual Traceability
        public string JointDiagramPath { get; set; } = "";
        public string PassDiagramPath { get; set; } = "";

        // 🔥 Audit
        public string QualifiedThicknessRange { get; set; } = "";
    }
}