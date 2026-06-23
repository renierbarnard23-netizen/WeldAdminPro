namespace WeldAdminPro.Core.Models
{
    public class CategoryStockSummary
    {
        public string Category { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public int TotalQuantity { get; set; }
    }
}
