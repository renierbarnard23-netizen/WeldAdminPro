namespace WeldAdminPro.Core.Services
{
    public class QualificationRangeResult
    {
        public double MinThickness { get; set; }
        public double MaxThickness { get; set; }

        public double MinDiameter { get; set; }
        public double MaxDiameter { get; set; }

        public string QualifiedPosition { get; set; } = "";

        public string Notes { get; set; } = "";
    }
}