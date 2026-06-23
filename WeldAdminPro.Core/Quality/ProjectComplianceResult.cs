using System.Collections.Generic;

namespace WeldAdminPro.Core.Quality
{
    public class ProjectComplianceResult
    {
        public int TotalWps { get; set; }
        public int CompliantWps { get; set; }

        // 🔥 ADD THIS (FIXES YOUR ERROR)
        public double DocumentCompliancePercent { get; set; }

        public bool IsCompliant =>
            (TotalWps == 0 || CompliantWps == TotalWps)
            && DocumentCompliancePercent == 100;

        public List<string> Issues { get; set; } = new();

        public double WpsCompliancePercentage =>
    TotalWps == 0 ? 100 : (double)CompliantWps / TotalWps * 100;
    }
}