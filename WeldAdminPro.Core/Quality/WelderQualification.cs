using WeldAdminPro.Core.Quality;

namespace WeldAdminPro.Core.Quality
{
    public class WelderQualification
    {
        public int Id { get; set; }

        public string WelderName { get; set; } = string.Empty;

        public string Process { get; set; } = string.Empty; // SMAW, GTAW
        public string Position { get; set; } = string.Empty; // 1G, 2G, 6G
        public DateTime QualificationDate { get; set; }
        public DateTime ExpiryDate { get; set; }              
        public string MaterialGroup { get; set; } = string.Empty;    // P-No / Group 
        public double ThicknessMin { get; set; }
        public double ThicknessMax { get; set; }
        public bool IsActive { get; set; } = true;

        public string PNumber { get; set; } = "";
        public string FNumber { get; set; } = "";

        public double MinThickness { get; set; }
        public double MaxThickness { get; set; }

        public double Diameter { get; set; }
    }
}