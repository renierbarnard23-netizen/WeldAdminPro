using System;
using System.Windows;
using WeldAdminPro.UI.ViewModels;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI.Views
{
    public partial class MainWindow : Window
    {
		public MainWindow()
		{
			InitializeComponent();

			DataContext = new MainViewModel(); // ✅ THIS WAS MISSING
		}

		private void Home_Click(object sender, RoutedEventArgs e)
        {
			((MainViewModel)DataContext).CurrentView = new HomeView();
		}

        private void Projects_Click(object sender, RoutedEventArgs e)
        {
			((MainViewModel)DataContext).CurrentView = new ProjectsView();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
			((MainViewModel)DataContext).CurrentView = new ReportsView();
        }
		private void ProductionSettings_Click(object sender, RoutedEventArgs e)
		{
			var window = new ProductionSettingsWindow();
			window.ShowDialog();
		}
		private void Stock_Click(object sender, RoutedEventArgs e)
	    {
			((MainViewModel)DataContext).CurrentView = new StockView();
	    }
        private void ProjectCosts_Click(object sender, RoutedEventArgs e)
		{
			((MainViewModel)DataContext).CurrentView = new ProjectCostDashboardView();
		}
		private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
		private void MaterialCostDrivers_Click(object sender, RoutedEventArgs e)
		{
			((MainViewModel)DataContext).CurrentView = new MaterialCostDriversView();
		}
		private void Profitability_Click(object sender, RoutedEventArgs e)
		{
			((MainViewModel)DataContext).CurrentView = new ProjectProfitabilityView();
		}
		private void ProjectRisk_Click(object sender, RoutedEventArgs e)
		{
			((MainViewModel)DataContext).CurrentView = new ExecutiveProjectRiskView();
		}
		private void MaterialTrends_Click(object sender, RoutedEventArgs e)
		{
			((MainViewModel)DataContext).CurrentView = new ExecutiveMaterialTrendsView();
		}
		private void StockForecast_Click(object sender, RoutedEventArgs e)
		{
			((MainViewModel)DataContext).CurrentView = new ExecutiveStockForecastView();
		}
		private void WorkOrders_Click(object sender, RoutedEventArgs e)
		{
			((MainViewModel)DataContext).CurrentView = new WorkOrdersView();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			Application.Current.Shutdown();
		}
	}
}
