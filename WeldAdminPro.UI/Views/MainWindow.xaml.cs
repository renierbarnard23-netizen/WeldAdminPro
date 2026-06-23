using System;
using System.Windows;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Services;
using WeldAdminPro.UI.ViewModels;
using WeldAdminPro.UI.ViewModels.Quality;
using WeldAdminPro.UI.Views.Production;
using WeldAdminPro.UI.Views.Quality;

namespace WeldAdminPro.UI.Views
{
    public partial class MainWindow : Window
    {
        // =====================================
        // MAIN VIEWMODEL
        // =====================================

        public MainViewModel Vm
        {
            get;
            private set;
        }

        // =====================================
        // SECURITY / VISIBILITY VIEWMODEL
        // =====================================

        public CurrentUserPermissionsViewModel Permissions
        {
            get;
            private set;
        } = new();

        // =====================================
        // CONSTRUCTOR
        // =====================================

        public MainWindow()
        {
            InitializeComponent();

            Vm =
                new MainViewModel();

            RefreshSecurity();

            _homeViewModel =
                new HomeViewModel();

            _homeView =
                new HomeView();

            _homeView.DataContext =
                _homeViewModel;

            Vm.CurrentView =
                _homeView;
        }

        // =====================================
        // SECURITY REFRESH
        // =====================================

        private void RefreshSecurity()
        {
            Permissions =
                new CurrentUserPermissionsViewModel();

            DataContext = null;
            DataContext = this;
        }

        // =====================================
        // HOME
        // =====================================

        private void Home_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                _homeView;
        }

        // =====================================
        // PROJECTS
        // =====================================

        private void Projects_Click(
            object sender,
            RoutedEventArgs e)
        {
            var view =
                new ProjectsView(
                    new ProjectsViewModel());

            if (view.DataContext
                is ProjectsViewModel pvm)
            {
                pvm.ProjectSelected =
                    project =>
                    {
                        Vm.SelectedProject =
                            project;

                        App.ProjectContextService
                            .SetCurrentProject(project);

                        MessageBox.Show(
                            $"Project Context Set:\n{project.ProjectName}");
                    };
            }

            Vm.CurrentView =
                view;
        }

        // =====================================
        // STOCK
        // =====================================

        private void Stock_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new StockView();
        }

        // =====================================
        // QUALITY
        // =====================================

        private void Wps_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new WpsView();
        }

        private void Pqr_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new PqrView();
        }

        private void Quality_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!SecurityAccessService.Require(
                PermissionService.HasPermission(
                    SystemPermission.AccessQuality)))
            {
                MessageBox.Show(
                    "You do not have permission to access Quality modules.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Vm.CurrentView =
                new QualityView();
        }

        private void WeldRegister_Click(
            object sender,
            RoutedEventArgs e)
        {
            var selectedProject =
                Vm.SelectedProject;

            if (selectedProject == null)
            {
                MessageBox.Show(
                    "Please select a project first.");

                return;
            }

            Vm.CurrentView =
                new WeldRegisterView(
                    selectedProject.Id);
        }

        private void Repairs_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new RepairManagementView();
        }

        private void NcrDashboard_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new NcrDashboardView();
        }

        private void TurnoverGovernance_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new TurnoverGovernanceView();
        }

        private void WelderQualifications_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new WelderQualificationView();
        }

        private void QaDashboard_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new QaDashboardView(
                    new QaDashboardViewModel(
                        App.WeldService));
        }

        private void ExecutiveQualityDashboard_Click(
            object sender,
                RoutedEventArgs e)
        {
            Vm.CurrentView =
                new QualityAnalyticsView();
        }

        // =====================================
        // TRACEABILITY
        // =====================================

        private void TraceabilityMatrix_Click(
            object sender,
            RoutedEventArgs e)
        {
            var window =
                new Window
                {
                    Title = "Weld Traceability Matrix",
                    Width = 1200,
                    Height = 700,
                    Content = new WeldTraceabilityView()
                };

            window.Show();
        }

        // =====================================
        // DOCUMENT VAULT
        // =====================================

        private void OpenDocumentVault_Click(
            object sender,
            RoutedEventArgs e)
        {
            var window =
                new DocumentVaultWindow();

            window.ShowDialog();
        }

        // =====================================
        // ANALYTICS
        // =====================================

        private void ProjectCosts_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new ProjectCostDashboardView();
        }

        private void MaterialCostDrivers_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new MaterialCostDriversView();
        }

        private void Profitability_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new ProjectProfitabilityView();
        }

        private void ProjectRisk_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new ExecutiveProjectRiskView();
        }

        private void MaterialTrends_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new ExecutiveMaterialTrendsView();
        }

        private void StockForecast_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new ExecutiveStockForecastView();
        }

        // =====================================
        // PRODUCTION
        // =====================================

        private void WorkOrders_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!SecurityAccessService.Require(
                PermissionService.HasPermission(
                    SystemPermission.AccessProduction)))
            {
                MessageBox.Show(
                    "You do not have permission to access Production modules.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Vm.CurrentView =
                new WorkOrdersView();
        }

        // =====================================
        // REPORTS
        // =====================================


        private void ProductionControlTower_Click(
            object sender,
            RoutedEventArgs e)
        {
            MainContent.Content =
                new ProductionControlTowerView();
        }

        private void Reports_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!SecurityAccessService.Require(
                PermissionService.HasPermission(
                    SystemPermission.AccessReports)))
            {
                MessageBox.Show(
                    "You do not have permission to access Reports.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Vm.CurrentView =
                new ReportsView();
        }

        // =====================================
        // SETTINGS
        // =====================================

        private void ProductionSettings_Click(
            object sender,
            RoutedEventArgs e)
        {
            var window =
                new ProductionSettingsWindow();

            window.ShowDialog();
        }

        // =====================================
        // USER MANAGEMENT
        // =====================================

        private void UserManagement_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!SecurityAccessService.Require(
                PermissionService.HasPermission(
                    SystemPermission.ManageUsers)))
            {
                MessageBox.Show(
                    "You do not have permission to access User Management.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var view =
                new UserManagementView();

            view.DataContext =
                new UserManagementViewModel();

            Vm.CurrentView =
                view;
        }

        private readonly HomeView _homeView;

        private readonly HomeViewModel _homeViewModel;

        // =====================================
        // AUDIT LOGS
        // =====================================

        private void AuditLogs_Click(
            object sender,
            RoutedEventArgs e)
        {
            Vm.CurrentView =
                new AuditLogView();
        }

        // =====================================
        // LOGOUT
        // =====================================

        private void Logout_Click(
            object sender,
            RoutedEventArgs e)
        {
            AuditService.Log(
                "LOGOUT",
                "Authentication",
                "User logged out");

            CurrentUserService.Logout();

            var login =
                new LoginWindow();

            var result =
                login.ShowDialog();

            if (result != true)
            {
                Close();
                return;
            }

            Vm =
                new MainViewModel();

            RefreshSecurity();

            Vm.CurrentView =
                _homeView;
        }

        private void OpenWeldControlTower_Click(
                object sender,
                RoutedEventArgs e)
        {
            Vm.CurrentView =
                new WeldControlTowerView();
        }

        // =====================================
        // EXIT
        // =====================================

        private void Exit_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(
            EventArgs e)
        {
            base.OnClosed(e);

            Application.Current.Shutdown();
        }
    }
}
