using System;
using System.Windows.Media;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public class WpsDisplay
    {
        public Guid Id { get; set; }

        public string WpsNumber { get; set; } = "";
        public string Process { get; set; } = "";
        public string MaterialGroup { get; set; } = "";

        // =====================================
        // ENGINEERING DATA
        // =====================================

        public string PNumber { get; set; } = "";
        public string FNumber { get; set; } = "";
        public string Position { get; set; } = "";
        public string JointType { get; set; } = "";

        public double Diameter { get; set; }

        public double ThicknessMin { get; set; }
        public double ThicknessMax { get; set; }

        public int Revision { get; set; }

        // =====================================
        // DISPLAY HELPERS
        // =====================================

        public string ThicknessDisplay =>
            $"{ThicknessMin:0.##} - {ThicknessMax:0.##} mm";

        public string DiameterDisplay =>
            Diameter == double.MaxValue
                ? "No Limit"
                : Diameter <= 0
                    ? "-"
                    : $"{Diameter:0.##} mm";

        // =====================================
        // PQR
        // =====================================

        public string PqrNumber { get; set; } = "";

        // =====================================
        // VALIDATION UI
        // =====================================

        public string ValidationStatus { get; set; } = "";
        public string StatusIcon { get; set; } = "";

        public Brush StatusColor { get; set; }
            = Brushes.Black;

        public string StatusMessage { get; set; } = "";
        public string ValidationMessage { get; set; } = "";
    }
}