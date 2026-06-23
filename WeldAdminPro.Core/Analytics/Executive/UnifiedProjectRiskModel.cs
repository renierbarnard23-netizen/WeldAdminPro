namespace WeldAdminPro.Core.Analytics.Executive
{
    public class UnifiedProjectRiskModel
    {
        public int Score { get; set; }           // 0–100
        public string Level { get; set; } = "";  // LOW / MEDIUM / HIGH
        public string Color { get; set; } = "";  // Green / Orange / Red

        // Breakdown (useful for UI / debugging)
        public int ComplianceScore { get; set; }
        public int FinancialScore { get; set; }

        public string Display =>
            $"{Level} ({Score})";
    }
}