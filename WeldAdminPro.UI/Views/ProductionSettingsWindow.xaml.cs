using System.Windows;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.Views
{
	public partial class ProductionSettingsWindow : Window
	{
		private readonly ProductionSettingsRepository _repo =
			new ProductionSettingsRepository();

		public ProductionSettingsWindow()
		{
			InitializeComponent();

			var settings = _repo.Get();

			WorkersBox.Text = settings.Workers.ToString();
			HoursBox.Text = settings.HoursPerDay.ToString();
			OvertimeBox.Text = settings.OvertimeHours.ToString();
			ShiftsBox.Text = settings.Shifts.ToString();
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			var settings = new ProductionSettings
			{
				Workers = int.Parse(WorkersBox.Text),
				HoursPerDay = double.Parse(HoursBox.Text),
				OvertimeHours = double.Parse(OvertimeBox.Text),
				Shifts = int.Parse(ShiftsBox.Text)
			};

			_repo.Save(settings);

			MessageBox.Show("Production settings saved.");

			Close();
		}
	}
}