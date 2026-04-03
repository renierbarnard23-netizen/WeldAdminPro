namespace WeldAdminPro.Core.Analytics.Production
{
	public class MaterialReservation
	{
		public string WorkOrderNumber { get; set; } = "";

		public string ItemCode { get; set; } = "";

		public double RequiredQuantity { get; set; }

		public double ReservedQuantity { get; set; }

		public double AvailableStock { get; set; }

		public bool ReservationSuccessful { get; set; }

		public string Reason { get; set; } = "";
	}
}