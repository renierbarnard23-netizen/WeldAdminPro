namespace WeldAdminPro.Core.Services
{
    public class EssentialVariableResult
    {
        public string Code { get; set; } = "";        // QW-402
        public string Variable { get; set; } = "";    // "Joint Type"
        public string Message { get; set; } = "";
        public bool IsFailure { get; set; }
    }
}