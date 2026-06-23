namespace WeldAdminPro.Core.Quality.Models
{
    public class DefectParetoModel
    {
        public string Defect
        { get; set; }
                = "";

        public int Count
        { get; set; }

        public double Percentage
        { get; set; }

        public double CumulativePercentage
        { get; set; }
    }
}