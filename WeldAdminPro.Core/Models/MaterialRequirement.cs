namespace WeldAdminPro.Core.Models
{
	public class MaterialRequirement
	{
		public string MaterialCode { get; set; } = "";

		public double RequiredQuantity { get; set; }

		public double AvailableQuantity { get; set; }
	}
}