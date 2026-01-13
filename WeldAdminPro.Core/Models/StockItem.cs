using System;

namespace WeldAdminPro.Core.Models
{
    public class StockItem
    {
        public Guid Id { get; set; }

        public string StockCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;

        public double MinimumLevel { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
