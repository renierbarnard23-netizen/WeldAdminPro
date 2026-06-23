using System;

namespace WeldAdminPro.Core.Quality.Models
{
    public class RepairAgingItem
    {
        public Guid RepairId { get; set; }

        public Guid WeldId { get; set; }

        public int RepairNumber { get; set; }

        public string Status { get; set; }
            = string.Empty;

        public int AgeDays { get; set; }

        public bool IsOverdue { get; set; }
    }
}
