namespace WeldAdminPro.Core.Models
{
    public class FillerMaterial
    {
        public string Classification { get; set; } = "";
        public int FNumber { get; set; }
        public int ANumber { get; set; }

        public string SfaNumber { get; set; } = "";
        public string AwsClassification { get; set; } = "";
        public string FillerForm { get; set; } = "";
        public string FillerComposition { get; set; } = "";

        public double Pass1Size { get; set; }
        public double Pass2Size { get; set; }
        public double Pass3Size { get; set; }

        public string Display => $"{Classification} (F{FNumber} A{ANumber})";

    }
}