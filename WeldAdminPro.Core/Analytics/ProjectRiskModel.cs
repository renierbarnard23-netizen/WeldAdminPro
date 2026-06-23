namespace WeldAdminPro.Core.Analytics
{
    public class ProjectRiskModel
    {
        public int Score { get; set; }           // 0–100
        public string Level { get; set; } = "";  // LOW / MEDIUM / HIGH
        public string Color { get; set; } = "";  // Green / Orange / Red

        public string Display =>
            $"{Level} ({Score})";
    }
}