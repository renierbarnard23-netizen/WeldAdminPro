namespace WeldAdminPro.Core.Analytics.Production
{
	public class BillOfMaterialItem
	{
		public int Id { get; set; }

		public int BillOfMaterialId { get; set; }

		public string ItemCode { get; set; } = "";

		public string Description { get; set; } = "";

		public int QuantityRequired { get; set; }
	}
}