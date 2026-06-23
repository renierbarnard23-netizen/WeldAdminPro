namespace WeldAdminPro.Core.Quality
{
    public class WelderQualification
    {
        public int Id { get; set; }

        public string WelderNumber { get; set; } = "";

        public string Process { get; set; } = "";

        public string MaterialGroup { get; set; } = "";

        public string Position { get; set; } = "";

        public DateTime InitialQualificationDate { get; set; }

        public DateTime? RenewalDate { get; set; }

        // ONLY THESE TWO
        public double ThicknessMin { get; set; }

        public double ThicknessMax { get; set; }

        public DateTime QualificationDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string PNumber { get; set; } = "";

        public string FNumber { get; set; } = "";

        public double Diameter { get; set; }

        public string ThicknessDisplay =>
            $"{ThicknessMin:N2} - {ThicknessMax:N2} mm";
    }
}