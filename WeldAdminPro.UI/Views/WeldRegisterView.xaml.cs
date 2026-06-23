using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.UI.ViewModels;
using WeldAdminPro.UI.Views.Quality;



namespace WeldAdminPro.UI.Views
{
    public partial class WeldRegisterView : UserControl
    {
        public WeldRegisterView(Guid projectId)
        {
            InitializeComponent();

            var connectionString =
                $"Data Source={DatabasePath.Get()}";

            var repository =
                new WeldRepository(connectionString);

            var service =
                new WeldService(repository);

            var vm =
                new WeldRegisterViewModel(
                    service);

            DataContext = vm;

            Loaded += async (_, _) =>
            {
                await vm.LoadAsync();
            };

            // =====================================
            // HOLD POINT PANEL LOADING
            // =====================================

            WeldGrid.SelectionChanged += (_, _) =>
            {
                if (vm.SelectedWeld != null)
                {
                    HoldPointHost.Content =
                        new HoldPointPanel(
                            vm.SelectedWeld.Id);
                }
            };
        }

        private void ViewHistory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.DataContext is not Weld weld)
                return;

            var window =
                new Window
                {
                    Title =
                        $"Weld History - {weld.WeldNumber}",

                    Width = 1000,
                    Height = 600,

                    Content =
                        new WeldHistoryView(
                            weld.Id)
                };

            window.ShowDialog();
        }

        private void WeldGrid_MouseDoubleClick(
                object sender,
                System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext
                is not WeldRegisterViewModel vm)
            {
                return;
            }

            if (vm.SelectedWeld == null)
                return;

            var dossierVm =
                new WeldDossierViewModel
            {
                WeldId =
                    vm.SelectedWeld.Id,

                WeldNumber =
                    vm.SelectedWeld.WeldNumber
            };

            var window =
                new WeldDossierWindow(
                    dossierVm);

            window.ShowDialog();
        }

            private void ExportProjectMrb_Click(
                object sender,
                RoutedEventArgs e)
        {
            if (DataContext
                is not WeldRegisterViewModel vm)
            {
                return;
            }

            var service =
                new ProjectMrbPdfService();

            var model =
                new ProjectMrbExportModel
                {
                    ProjectNumber = "PRJ-001",
                    ProjectName = "Demo Fabrication Project"
                };

            foreach (var weld in vm.Welds)
            {
                var dossier =
                    new WeldDossierExportModel
                    {
                        WeldNumber =
                            weld.WeldNumber
                    };

                dossier.NdtRows.Add(
                    new NdtHistoryRow
                    {
                        Method = "RT",
                        ReportNumber = "RT-001",
                        Result = weld.NdtStatus,
                        Technician = "QA Inspector",
                        Date =
                            System.DateTime.Now
                                .ToString("yyyy-MM-dd")
                    });

                dossier.NcrRows.Add(
                    new NcrHistoryRow
                    {
                        NcrNumber = "NCR-001",
                        Description = "Sample NCR",
                        Status = "Closed",
                        CorrectiveAction = "Repair completed",
                        Closed = "Yes"
                    });

                model.Welds.Add(dossier);
            }

            service.Export(model);
        }


        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}