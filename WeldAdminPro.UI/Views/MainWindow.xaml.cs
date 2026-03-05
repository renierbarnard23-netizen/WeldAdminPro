using System.Windows;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new ProjectsView();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new HomeView();
        }

        private void Projects_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ProjectsView();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ReportsView();
        }
	    private void Stock_Click(object sender, RoutedEventArgs e)
	    {
    	    MainContent.Content = new StockView();
	    }
        private void ProjectCosts_Click(object sender, RoutedEventArgs e)
		{
			MainContent.Content = new ProjectCostDashboardView();
		}
		private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
		private void MaterialCostDrivers_Click(object sender, RoutedEventArgs e)
		{
			MainContent.Content = new MaterialCostDriversView();
		}
		private void Profitability_Click(object sender, RoutedEventArgs e)
		{
			MainContent.Content = new ProjectProfitabilityView();
		}
		private void ProjectRisk_Click(object sender, RoutedEventArgs e)
		{
			MainContent.Content = new ExecutiveProjectRiskView();
		}
		private void MaterialTrends_Click(object sender, RoutedEventArgs e)
		{
			MainContent.Content = new ExecutiveMaterialTrendsView();
		}
		private void StockForecast_Click(object sender, RoutedEventArgs e)
		{
			MainContent.Content = new ExecutiveStockForecastView();
		}
		private void WorkOrders_Click(object sender, RoutedEventArgs e)
		{
			MainContent.Content = new WorkOrdersView();
		}
	}
}
