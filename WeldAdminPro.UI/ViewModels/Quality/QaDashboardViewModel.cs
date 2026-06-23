using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Reporting.Enums;
using WeldAdminPro.Core.Reporting.Services;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels.Quality
{
    public partial class QaDashboardViewModel
    : ObservableObject
    {
        private readonly IWeldService
        _weldService;

        private readonly RepairRepository
        _repairRepository;

        private readonly IProjectContextService
            _projectContextService;

        private readonly QaDashboardAnalyticsService
            _analyticsService;

        private readonly TurnoverReadinessEngine
            _turnoverEngine;

        private readonly DocumentComplianceEngine
            _documentComplianceEngine;

        private readonly DocumentVaultRepository
            _documentVaultRepository;

        private readonly TurnoverPackageBuilder
            _packageBuilder;

        private readonly TurnoverExportService
            _exportService;

        private readonly DocumentVaultRepository
            _documentRepository;

        private readonly NdtRepository
            _ndtRepository;

        [ObservableProperty]
        private QaDashboardMetrics metrics
            = new();

        [ObservableProperty]
        private List<WorkflowDistributionItem>
            workflowDistribution
            = new();

        [ObservableProperty]
        private List<RepairAgingItem>
            repairAging
            = new();

        public QaDashboardViewModel(
            IWeldService weldService)
        {
            _weldService =
                weldService;

            _projectContextService =
                App.ProjectContextService;

            _projectContextService.ProjectChanged
                += OnProjectChanged;

            _repairRepository =
                new RepairRepository(
                    DatabasePath.GetConnectionString());

            _analyticsService =
                new QaDashboardAnalyticsService();

            _turnoverEngine =
                new TurnoverReadinessEngine();

            _documentComplianceEngine =
                new DocumentComplianceEngine();

            _documentVaultRepository =
                new DocumentVaultRepository(
                    DatabasePath.GetConnectionString());

            _packageBuilder =
                new TurnoverPackageBuilder();

            _exportService =
                new TurnoverExportService();

            _documentRepository =
                new DocumentVaultRepository(
                DatabasePath.GetConnectionString());

            _ndtRepository =
                new NdtRepository(
                DatabasePath.GetConnectionString());

            try
            {
                _ = LoadCurrentProjectData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                ex.Message,
                "QA Dashboard Error");
            }


        }

        private async Task LoadCurrentProjectData()
        {
            try
            {
                var project =
                _projectContextService.CurrentProject;

    if (project == null)
                    return;

                var welds =
                    await _weldService
                        .GetByProjectAsync(
                            project.Id);

                var repairs =
                    new List<RepairRecord>();

                foreach (var weld in welds)
                {
                    repairs.AddRange(
                        _repairRepository.GetByWeld(
                            weld.Id));
                }

                Metrics =
                    _analyticsService.Generate(
                        welds.ToList(),
                        repairs);

                WorkflowDistribution =
                    _analyticsService
                        .GetWorkflowDistribution(
                            welds.ToList());

                RepairAging =
                    _analyticsService
                        .GetRepairAging(
                            repairs);

                var turnoverResult =
                    _turnoverEngine.Evaluate(
                        welds.ToList(),
                        repairs,
                        0);

                Metrics.TurnoverReady =
                    turnoverResult.IsReady;

                Metrics.BlockingIssues =
                    turnoverResult.BlockingIssues.Count;

                var documents =
                    _documentVaultRepository.GetAll();

                var documentCompliance =
                    _documentComplianceEngine.Evaluate(
                        documents.ToList(),
                        GetDefaultRequirements());

                Metrics.BlockingIssues +=
                    documentCompliance
                        .MissingRequiredDocuments;

                if (!documentCompliance.IsCompliant)
                {
                    Metrics.TurnoverReady = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "QA Dashboard Error");
            }
}


        private List<DocumentRequirement>
GetDefaultRequirements()
        {
            return new List<DocumentRequirement>
{
new()
{
Category =
DocumentCategoryType.WPS,

        Description =
            "Approved WPS required",

        IsRequired = true,

        MinimumRequired = 1
    },

    new()
    {
        Category =
            DocumentCategoryType.WPQR,

        Description =
            "Approved WPQR required",

        IsRequired = true,

        MinimumRequired = 1
    },

    new()
    {
        Category =
            DocumentCategoryType.NDT,

        Description =
            "Approved NDT reports required",

        IsRequired = true,

        MinimumRequired = 1
    }
};
}
        [RelayCommand]
        private async Task ExportTurnoverPackage()
        {
            var project =
            _projectContextService.CurrentProject;

        if (project == null)
            {
                MessageBox.Show(
                    "No active project selected.");

                return;
            }

            var welds =
                await _weldService
                    .GetByProjectAsync(
                        project.Id);

            var repairs =
                new List<RepairRecord>();

            foreach (var weld in welds)
            {
                repairs.AddRange(
                    _repairRepository.GetByWeld(
                        weld.Id));
            }

            var documents =
                _documentRepository
                    .GetByProject(
                        project.Id);

            var ndtResults =
                _ndtRepository.GetByProject(
                welds.Select(x => x.Id)
                .ToList());



            var package =
            _packageBuilder.Build(
            project,
            welds.ToList(),
            repairs,
            documents,
            ndtResults);


            var dialog =
                new OpenFolderDialog();

            if (dialog.ShowDialog() != true)
                return;

            var exportPath =
                _exportService.Export(
                    package,
                    dialog.FolderName);

            MessageBox.Show(
                $"Turnover package exported to:\n{exportPath}");
}


        private void OnProjectChanged(
            Project? project)
        {
            _ = LoadCurrentProjectData();

        }
    }
}
