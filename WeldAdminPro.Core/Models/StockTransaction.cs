using System;

namespace WeldAdminPro.Core.Models
{
    public class StockTransaction
    {
        public Guid Id { get; set; }
        public Guid StockItemId { get; set; }
        public DateTime TransactionDate { get; set; }
        public int Quantity { get; set; }      // +IN / -OUT
        public string Type { get; set; } = ""; // IN or OUT
        public string Reference { get; set; } = "";
	public string ItemCode { get; set; } = "";
	public string ItemDescription { get; set; } = "";
	public int RunningBalance { get; set; }
    }
}
