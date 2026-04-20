using System;
using System.Windows;
using WeldAdminPro.UI.ViewModels;
using WeldAdminPro.UI.Views;
using WeldAdminPro.UI.Views.Quality;

namespace WeldAdminPro.UI.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel Vm => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();

            // ✅ Set default view
            Vm.CurrentView = new HomeView();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new HomeView();
        }

        private void Projects_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new ProjectsView();
        }

        private void Stock_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new StockView();
        }

        private void Wps_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new WpsView();
        }

        private void Pqr_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new PqrView();
        }

        private void Quality_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new QualityView();
        }

        private void ProjectCosts_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new ProjectCostDashboardView();
        }

        private void MaterialCostDrivers_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new MaterialCostDriversView();
        }

        private void Profitability_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new ProjectProfitabilityView();
        }

        private void ProjectRisk_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new ExecutiveProjectRiskView();
        }

        private void MaterialTrends_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new ExecutiveMaterialTrendsView();
        }

        private void StockForecast_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new ExecutiveStockForecastView();
        }

        private void WorkOrders_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new WorkOrdersView();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            Vm.CurrentView = new ReportsView();
        }

        private void ProductionSettings_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProductionSettingsWindow();
            window.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}