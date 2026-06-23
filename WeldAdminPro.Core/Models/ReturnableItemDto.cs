namespace WeldAdminPro.Core.Models
{
    public class ReturnableItemDto
    {
        public Guid StockItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
    }
}