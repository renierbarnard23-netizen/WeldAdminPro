using System;
using System.Collections.Generic;

namespace WeldAdminPro.Core.Models
{
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();   // ✅ GUID PRIMARY KEY

        public string ProjectNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;

        public string CreatedAt { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string DatabookVersion { get; set; } = string.Empty;

        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
