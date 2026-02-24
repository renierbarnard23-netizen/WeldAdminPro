namespace WeldAdminPro.Core.Enums
{
	public enum ItemInventoryCategory
	{
		Inactive,          // No movement, zero stock
		Stocked,           // Has stock, no movement this period
		ActiveConsumption, // Has outbound movement
		Replenished,       // Has inbound movement
		Balanced           // In and Out both occurred
	}
}