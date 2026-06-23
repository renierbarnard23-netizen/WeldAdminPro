using System.Collections.Generic;

namespace WeldAdminPro.Core.Quality.Models
{
    public class WeldStateTransitionResult
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }
            = string.Empty;

        public List<string> BlockingIssues { get; set; }
            = new();
    }
}
