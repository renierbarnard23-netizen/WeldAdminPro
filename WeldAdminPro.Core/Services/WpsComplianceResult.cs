namespace WeldAdminPro.Core.Services
{
    public class WpsComplianceResult
    {
        public bool IsCompliant { get; set; }

        public List<string> ValidationErrors { get; set; } = new();
        public List<string> EssentialFailures { get; set; } = new();
        public List<string> RangeFailures { get; set; } = new();

        public List<string> AllIssues =>
            ValidationErrors
                .Concat(EssentialFailures)
                .Concat(RangeFailures)
                .ToList();
    }
}