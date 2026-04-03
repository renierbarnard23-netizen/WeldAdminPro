namespace WeldAdminPro.Core.Analytics.Executive
{
	public class ProjectProfitabilityStat
	{
		public string ProjectName { get; set; } = "";

		public decimal Revenue { get; set; }

		public decimal MaterialCost { get; set; }

		public decimal Profit => Revenue - MaterialCost;

		public double Margin =>
			Revenue == 0 ? 0 :
			(double)(Profit / Revenue * 100);
	}
}