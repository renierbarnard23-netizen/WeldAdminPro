

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
        

    }
}