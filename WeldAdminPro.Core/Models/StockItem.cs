using System;

namespace WeldAdminPro.Core.Models
{
    public class StockItem
    {
        public Guid Id { get; set; }

        public string ItemCode { get; set; } = "";   // ✅ REQUIRED
        public string ItemName { get; set; } = "";
        public string Description { get; set; } = "";

        public int Quantity { get; set; }
        public string Unit { get; set; } = "";
    }
}
