namespace WeldAdminPro.Core.Analytics.Executive
{
    public class ProjectComplianceDisplay
    {
        public int JobNumber { get; set; }

        public string ProjectName { get; set; } = "";

        public string Client { get; set; } = "";

        public double WpsPercent { get; set; }

        public double DocPercent { get; set; }

        public bool IsCompliant { get; set; }

        public int IssueCount { get; set; }

        public int RiskScore { get; set; }

        public string RiskLevel { get; set; } = "";

        public string RiskColor { get; set; } = "";

        public int ComplianceScore { get; set; }

        public int FinancialScore { get; set; }

        public string StatusText =>
            IsCompliant
                ? "✔ COMPLIANT"
                : "✖ NON-COMPLIANT";

        public string RiskDisplay =>
            $"{RiskLevel} ({RiskScore})";
    }
}