using System.Collections.Generic;

namespace WeldAdminPro.Core.Quality.Models
{
    public class DocumentComplianceResult
    {
        public bool IsCompliant { get; set; }

        public int MissingRequiredDocuments { get; set; }

        public List<string> Issues { get; set; }
            = new();
    }
}
