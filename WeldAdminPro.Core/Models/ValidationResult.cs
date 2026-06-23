namespace WeldAdminPro.Core.Models
{
    public class ValidationResult
    {
        public string Message { get; set; }
        public string Severity { get; set; }

        public ValidationResult(string message, string severity)
        {
            Message = message;
            Severity = severity;
        }
    }
}