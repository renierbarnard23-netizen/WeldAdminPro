using System;
using System.Collections.Generic;

namespace WeldAdminPro.Core.Models
{
    public class Project
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ProjectNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string DatabookVersion { get; set; } = string.Empty;
        public ICollection<WeldAdminPro.Core.Models.Document> Documents { get; set; } = new List<WeldAdminPro.Core.Models.Document>();
    }
}
