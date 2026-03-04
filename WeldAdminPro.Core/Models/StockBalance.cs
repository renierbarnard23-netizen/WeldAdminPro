public class StockBalance
{
	public Guid Id { get; set; }

	public Guid StockItemId { get; set; }

	public Guid LocationId { get; set; }

	public int Quantity { get; set; }
}