using System;

namespace WeldAdminPro.UI.Models
{
    public class ReturnableItemDisplay
    {
        public Guid Id { get; set; }
        public string Display { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }
}