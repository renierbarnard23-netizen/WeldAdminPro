using System.Windows.Media;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public class WpsDisplay
    {
        public Guid Id { get; set; }

        public string WpsNumber { get; set; } = "";
        public string Process { get; set; } = "";
        public string MaterialGroup { get; set; } = "";

        // 🔥 NEW FIELDS (YOU MISSED THESE)
        public string PNumber { get; set; } = "";
        public string FNumber { get; set; } = "";
        public string Position { get; set; } = "";
        public string JointType { get; set; } = "";
        public double Diameter { get; set; }

        // Thickness
        public double ThicknessMin { get; set; }
        public double ThicknessMax { get; set; }

        public string ThicknessDisplay => $"{ThicknessMin} - {ThicknessMax}";

        // PQR
        public string PqrNumber { get; set; } = "";

        // Validation UI
        public string ValidationStatus { get; set; } = "";
        public string StatusIcon { get; set; } = "";
        public Brush StatusColor { get; set; } = Brushes.Black;
        public string StatusMessage { get; set; } = "";
        public string DiameterDisplay { get; set; } = "";
        public string ValidationMessage { get; set; } = "";
        public int Revision { get; set; }
    }
}