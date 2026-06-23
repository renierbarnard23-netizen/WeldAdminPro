using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WeldAdminPro.Core.Enums;
using WeldAdminPro.Core.Interfaces;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Quality.Enums;
using WeldAdminPro.Core.Quality.Models;
using WeldAdminPro.Core.Quality.Services;
using WeldAdminPro.Core.Reporting.Enums;
using WeldAdminPro.Core.Reporting.Models;
using WeldAdminPro.Core.Reporting.Services;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.UI.Views;
using WeldAdminPro.UI.Views.Quality;
using CommunityToolkit.Mvvm.Messaging;
using WeldAdminPro.Core.Events;

namespace WeldAdminPro.UI.ViewModels
{
    public partial class WeldRegisterViewModel : ObservableObject, IRecipient<WeldNavigationMessage>
    {
        private readonly IWeldService _weldService;

        private readonly WeldValidationService _validator;

        private readonly IWpsRepository _wpsRepository;

        private readonly PqrRepository _pqrRepository = new();

        private readonly WelderQualificationRepository _welderRepository = new();

        private readonly WeldNdtRepository _ndtRepository;

        private readonly WeldHistoryRepository _historyRepository;

        private readonly string _connectionString;

        private readonly WeldAnalyticsService _analyticsService = new();

        private readonly WeldAdminPro.Core.Reporting.Services
            .TurnoverPackageService
            _turnoverPackageService = new();

        private readonly DocumentVaultService _documentVaultService = new();
        private readonly IProjectContextService _projectContextService;

        private readonly IHistoryTrackingService _historyTrackingService;

        private readonly RepairRepository
            _repairRepository;

        private readonly RepairWorkflowService
            _repairWorkflowService;

        private readonly WeldWorkflowEngine
            _workflowEngine;

        private readonly WeldAdminPro.Core.Services
                .WeldBlockingAnalysisService
                _blockingAnalysisService = new();

        private readonly HoldPointRepository
            _holdPointRepository;

        private readonly HoldPointWorkflowService
            _holdPointWorkflowService;

        private readonly WeldAuthorizationService
            _authorizationService = new();

        [ObservableProperty]
        private ObservableCollection<Weld> welds = new();

        [ObservableProperty]
        private Weld? selectedWeld;

        [ObservableProperty]
        private WeldRepairAnalytics analytics
        = new();

        [ObservableProperty]
        private ObservableCollection<WeldBlockingDisplayItem>
        blockingItems = new();

        [ObservableProperty]
        private string searchText = "";

        [ObservableProperty]
        private ObservableCollection<string> workflowFilters = new();

        [ObservableProperty]
        private string selectedWorkflowFilter = "All";

        [ObservableProperty]
        private ObservableCollection<string> welderFilters = new();

        [ObservableProperty]
        private string selectedWelderFilter = "All";

        [ObservableProperty]
        private ObservableCollection<string> wpsFilters = new();

        [ObservableProperty]
        private string selectedWpsFilter = "All";

        [ObservableProperty]
        private bool showNcrOnly;

        [ObservableProperty]
        private ObservableCollection<Weld> filteredWelds = new();

        public ObservableCollection<WeldNdtResult>
            SelectedWeldNdtResults
        { get; }
            = new();

        public ObservableCollection<WeldHistoryEntry>
    SelectedWeldHistory
        {
            get;
        }
    = new();

        public WeldRegisterViewModel(
IWeldService weldService)
        {
            _weldService = weldService;

_projectContextService =
    App.ProjectContextService;

            _projectContextService.ProjectChanged
                += OnProjectChanged;

            _connectionString =
                $"Data Source={DatabasePath.Get()}";

            _wpsRepository =
                new WpsRepository();

            _ndtRepository =
                new WeldNdtRepository(
                    _connectionString);

            _historyRepository =
    new WeldHistoryRepository(
        _connectionString);

            // =====================================
            // WORKFLOW SERVICES
            // =====================================

            _repairRepository =
                new RepairRepository(
                    DatabasePath.GetConnectionString());

            _repairWorkflowService =
                new RepairWorkflowService();

            _workflowEngine =
                new WeldWorkflowEngine();

            _holdPointRepository =
                new HoldPointRepository(
                    DatabasePath.GetConnectionString());

            _holdPointWorkflowService =
                new HoldPointWorkflowService();

            _historyTrackingService =
                new HistoryTrackingService(
                    DatabasePath.GetConnectionString());

            _validator =
                new WeldValidationService();

            ExportDataBookCommand =
                new AsyncRelayCommand(
                    ExportDataBookAsync);

            ExportTurnoverPackageCommand =
                new AsyncRelayCommand(
                    ExportTurnoverPackageAsync);

            _= LoadCurrentProjectData();

}
        private async Task LoadCurrentProjectData()
        {
            var currentProject =
            _projectContextService.CurrentProject;

        if (currentProject == null)
            {
                Welds.Clear();
                FilteredWelds.Clear();
                BlockingItems.Clear();

                Analytics = new WeldRepairAnalytics();

                return;
            }

            var items =
                await _weldService
                    .GetByProjectAsync(
                        currentProject.Id);

            Welds =
                new ObservableCollection<Weld>(
                    items);

            Analytics =
                _analyticsService.Generate(
                    Welds.ToList());

            FilteredWelds = new ObservableCollection<Weld>(Welds);

            WorkflowFilters =
                new ObservableCollection<string>(
                    new[]
                    {
            "All"
                    }
                    .Concat(
                        Welds
                            .Select(x =>
                                x.WorkflowStatus.ToString())
                            .Distinct()
                            .OrderBy(x => x)));

            WelderFilters =
                new ObservableCollection<string>(
                    new[]
                    {
            "All"
                    }
                    .Concat(
                        Welds
                            .Select(x =>
                                x.WelderNumber)
                            .Where(x =>
                                !string.IsNullOrWhiteSpace(x))
                            .Distinct()
                            .OrderBy(x => x)));

            WpsFilters =
                new ObservableCollection<string>(
                    new[]
                    {
            "All"
                    }
                    .Concat(
                        Welds
                            .Select(x =>
                                x.WpsNumber)
                            .Where(x =>
                                !string.IsNullOrWhiteSpace(x))
                            .Distinct()
                            .OrderBy(x => x)));

            SelectedWorkflowFilter = "All";
            SelectedWelderFilter = "All";
            SelectedWpsFilter = "All";

            ApplyFilters();
            LoadBlockingAnalysis();

        }


        partial void OnSelectedWeldChanged(Weld? value)
        {
            LoadNdtHistory();

            LoadHistory();
        }

        private async void OnProjectChanged(Project? project)
        {
            await LoadCurrentProjectData();
        }

        private void LoadHistory()
        {
            SelectedWeldHistory.Clear();

            if (SelectedWeld == null)
                return;

            var history =
                _historyRepository.GetByWeld(
                    SelectedWeld.Id);

            foreach (var item in history)
            {
                SelectedWeldHistory.Add(item);
            }
        }

        private void LoadNdtHistory()
        {
            SelectedWeldNdtResults.Clear();

            if (SelectedWeld == null)
                return;

            var results =
                _ndtRepository.GetByWeld(
                    SelectedWeld.Id);

            foreach (var item in results)
            {
                SelectedWeldNdtResults.Add(item);
            }
        }

        private Wps? FindLatestWps(string? wpsNumber)
        {
            if (string.IsNullOrWhiteSpace(wpsNumber))
                return null;

            return _wpsRepository.GetAll()
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.WpsNumber) &&
                    x.WpsNumber.Trim()
                        .Equals(
                            wpsNumber.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Revision)
                .FirstOrDefault();
        }

        private void LoadBlockingAnalysis()
        {
            BlockingItems.Clear();

            foreach (var weld in Welds)
            {
                var repairs =
                    _repairRepository.GetByWeld(
                        weld.Id);

                var ndtResults =
                    _ndtRepository.GetByWeld(
                        weld.Id);

                bool hasAcceptedNdt =
                    ndtResults.Any(x =>
                        x.Result == NdtResultType.Accept);

                bool hasOpenRepairs =
                    repairs.Any(x =>
                        x.Status != RepairStatus.Closed);

                // =====================================
                // LOAD WPS
                // =====================================

                var selectedWps =
                    FindLatestWps(weld.WpsNumber);

                // =====================================
                // LOAD PQR
                // =====================================

                Pqr? pqr = null;

                if (selectedWps?.PqrId != null)
                {
                    pqr =
                        _pqrRepository.GetById(
                            selectedWps.PqrId.Value);
                }

                // =====================================
                // LOAD WELDER
                // =====================================

                var welder =
                    _welderRepository.GetAll()
                        .FirstOrDefault(x =>
                            x.WelderNumber ==
                            weld.WelderNumber);

                // =====================================
                // ANALYSIS
                // =====================================

                var result =
                    _blockingAnalysisService.Analyze(
                        weld,
                        selectedWps,
                        pqr,

                        welder?.Position ?? "",
                        welder?.PNumber ?? "",
                        welder?.QualificationDate
                            ?? DateTime.MinValue);

                weld.ReleaseReady =
                    result.IsReady;

                weld.BlockingCount =
                    result.Blockers.Count;

                weld.ReadinessSummary =
                    result.Blockers.Any()
                    ? string.Join(
                        ", ",
                        result.Blockers)
                    : "Ready";

                BlockingItems.Add(
                            new WeldBlockingDisplayItem
                            {
                                WeldNumber =
                            weld.WeldNumber,

                                WorkflowStatus =
                            weld.WorkflowStatus.ToString(),


                                IsBlocked =
                            !result.IsReady,

                                BlockingReasons =
                            result.Blockers.Any()
                            ? string.Join(
                                ", ",
                                result.Blockers)
                            : "Ready"
                            });
            } }

            private void ApplyFilters()
        {
            IEnumerable<Weld> query =
                Welds;

            // Search
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query =
                    query.Where(x =>
                        x.WeldNumber.Contains(
                            SearchText,
                            StringComparison.OrdinalIgnoreCase));
            }

            // Workflow
            if (SelectedWorkflowFilter != "All")
            {
                query =
                    query.Where(x =>
                        x.WorkflowStatus.ToString()
                            == SelectedWorkflowFilter);
            }

            // Welder
            if (SelectedWelderFilter != "All")
            {
                query =
                    query.Where(x =>
                        x.WelderNumber
                            == SelectedWelderFilter);
            }

            // WPS
            if (SelectedWpsFilter != "All")
            {
                query =
                    query.Where(x =>
                        x.WpsNumber
                            == SelectedWpsFilter);
            }

            // NCR
            if (ShowNcrOnly)
            {
                query =
                    query.Where(x =>
                        x.RequiresRepair);
            }

            FilteredWelds =
                new ObservableCollection<Weld>(
                    query);
        }

        partial void OnSearchTextChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedWorkflowFilterChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedWelderFilterChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnSelectedWpsFilterChanged(string value)
        {
            ApplyFilters();
        }

        partial void OnShowNcrOnlyChanged(bool value)
        {
            ApplyFilters();
        }

        public void Receive(WeldNavigationMessage message)
        {
            var weld =
                Welds
                    .FirstOrDefault(x =>
                        x.Id ==
                        message.WeldId);

            if (weld == null)
                return;

            SelectedWeld =
                weld;
        }

        public ICommand ExportDataBookCommand { get; }
        public ICommand ExportTurnoverPackageCommand { get; }

        [RelayCommand]
        public async Task LoadAsync()
        {
            await LoadCurrentProjectData();
        }

        [RelayCommand]
        public async Task AddWeldAsync()
        {
            var currentProject =
                _projectContextService.CurrentProject;
            
            if (currentProject == null)
            {
                MessageBox.Show(
                    "Please select a project first.");

                return;
            }

            var nextNumber =
                await _weldService
                    .GetNextWeldNumberAsync(
                        currentProject.Id);

            var window =
                new AddWeldWindow(
                    nextNumber);

            var result =
                window.ShowDialog();

            if (result != true
                || window.Weld == null)
            {
                return;
            }

            var weld =
                window.Weld;

            // =====================================
            // LOAD WPS
            // =====================================

            var selectedWps =
                FindLatestWps(weld.WpsNumber);

            if (selectedWps == null)
            {
                MessageBox.Show(
                    "Selected WPS not found.",
                    "Validation Error");

                return;
            }

            // =====================================
            // LOAD WELDER QUALIFICATION
            // =====================================

            var welderRepo =
                new WelderQualificationRepository();

            var qualification =
                welderRepo.GetAll()
                    .FirstOrDefault(x =>
                        x.WelderNumber == weld.WelderNumber
                        &&
                        x.Process.Trim().ToUpper()
                            == weld.Process.Trim().ToUpper()
                        &&
                        x.MaterialGroup.Trim().ToUpper()
                            == weld.MaterialGroup.Trim().ToUpper()
                        &&
                        x.Position.Trim().ToUpper()
                            == weld.Position.Trim().ToUpper());

            if (qualification == null)
            {
                MessageBox.Show(
                    "Welder qualification not found.",
                    "Validation Error");

                return;
            }

            // =====================================
            // VALIDATION
            // =====================================

            var validation =
                _validator.Validate(
                    qualification,
                    selectedWps,
                    weld.Thickness);

            if (!validation.IsValid)
            {
                MessageBox.Show(
                    string.Join(
                        Environment.NewLine,
                        validation.Errors),
                    "Validation Error");

                return;
            }
                        
            weld.ProjectId =
                currentProject.Id;

            weld.WorkflowStatus =
                WeldWorkflowStatus.NdtPending;

            // SAVE WELD FIRST
            await _weldService.AddAsync(weld);

            var traceabilityRepository =
    new WeldTraceabilityRepository(
        DatabasePath.GetConnectionString());

            traceabilityRepository.Add(
                new WeldTraceabilityRecord
                {
                    Id = Guid.NewGuid(),

                    WeldId = weld.Id,

                    WpsNumber =
                        weld.WpsNumber,

                    PqrNumber =
                        "AUTO-PQR",

                    WelderQualification =
                        weld.WelderNumber,

                    MaterialHeatNumber =
                        "HEAT-001",

                    ConsumableBatch =
                        "BATCH-001",

                    NdtReportNumber =
                        "",

                    ReleaseCertificate =
                        ""
                });

            // ===========================
            // QCP INSPECTION ENGINE
            // ===========================

            var qcpRepository =
                new QcpInspectionRuleRepository(
                    DatabasePath.GetConnectionString());

            var holdPointRepository =
                new HoldPointRepository(
                    DatabasePath.GetConnectionString());

            var rules =
                qcpRepository.GetAll();

            var engine =
                new QcpInspectionEngine();

            foreach (var rule in rules)
            {
                if (!engine.RequiresInspection(
                        weld,
                        rule))
                {
                    continue;
                }

                if (rule.RequiresHoldPoint)
                {
                    holdPointRepository.Add(
                        new WeldHoldPoint
                        {
                            Id = Guid.NewGuid(),

                            WeldId = weld.Id,

                            HoldPointType =
                                HoldPointType.NdtReview,

                            Category =
                                HoldPointCategory.Hold,

                            Status =
                                HoldPointStatus.Pending,

                            IsMandatory = true
                        });
                }
            }

            // RELOAD WELD FROM DATABASE
            var savedWeld =
                (await _weldService.GetByProjectAsync(
                    currentProject.Id))
                .FirstOrDefault(x =>
                    x.WeldNumber == weld.WeldNumber);

            if (savedWeld == null)
            {
                MessageBox.Show(
                    "Failed to reload weld.");

                return;
            }

            // CREATE HOLD POINTS
            
            var seeder =
                new HoldPointSeeder();

            var holdPoints =
                seeder.CreateDefault(
                    savedWeld.Id);

            foreach (var hp in holdPoints)
            {
                holdPointRepository.Add(hp);
            }


            // =====================================
            // HISTORY ENTRY
            // =====================================

            _historyTrackingService.Track(
                weld,
                "Weld Created",
                $"Weld {weld.WeldNumber} created.");

            await LoadAsync();

            MessageBox.Show(
                "Weld added successfully.");
        }

        [RelayCommand]
        private async Task AddNdtAsync()
        {
            if (SelectedWeld == null)
                return;

            var rules =
                new WeldLifecycleRuleService();

            if (!rules.CanAddNdt(
                    SelectedWeld,
                    out var error))
            {
                MessageBox.Show(error);

                return;
            }

            var window =
                new AddNdtWindow(
                    _connectionString,
                    SelectedWeld.Id);

            var result =
                window.ShowDialog();

            if (result == true
                && window.SavedResult != null)
            {
                var complianceService =
                    new WeldComplianceService();

                complianceService.ApplyNdtResult(
                    SelectedWeld,
                    window.SavedResult);

                await _weldService.UpdateAsync(
                    SelectedWeld);



                if (window.SavedResult.Result
                    == NdtResultType.Reject)
                {
                    SelectedWeld.WorkflowStatus =
                        WeldWorkflowStatus.RepairRequired;

                    _workflowEngine.MoveToRepair(
                        SelectedWeld,
                        out _);

                    var repairs =
                        _repairRepository.GetByWeld(
                            SelectedWeld.Id);

                    var repair =
                        new RepairRecord
                        {
                            Id = Guid.NewGuid(),

                            WeldId =
                                SelectedWeld.Id,

                            RepairNumber =
                                repairs.Count + 1,

                            Reason =
                                $"NDT rejection: {window.SavedResult.NdtMethod}",

                            RequestedDate =
                                DateTime.UtcNow,

                            Status =
                                RepairStatus.Requested
                        };

                    var ncrRepository =
                        new NcrRepository(
                        DatabasePath.GetConnectionString());

                    var ncr =
                    new NcrRecord
                    {
                        Id = Guid.NewGuid(),
			
			NcrNumber =
    			    $"NCR-{DateTime.Now:yyyy}-{DateTime.Now.Ticks % 10000:D4}",

                        WeldId =
                            SelectedWeld.Id,

                        WeldNumber =
                            SelectedWeld.WeldNumber,

                        Description =
                            $"NDT rejection on weld {SelectedWeld.WeldNumber}",

                        RootCause =
                            "Pending investigation",

                        CorrectiveAction =
                            "Repair required",

                        PreventiveAction =
                            "Review welding controls",

                        RaisedBy =
                            CurrentUserContext.Username,

                        RaisedDate =
                            DateTime.UtcNow,

                        AssignedTo =
                            CurrentUserContext.Username,

                        DueDate =
                            DateTime.UtcNow.AddDays(7),

                        Status =
                            NcrStatus.Open,

                        IsClosed = false
                    };

                    ncrRepository.Add(ncr);

                    var capaRepository =
                        new CapaRepository(
                        DatabasePath.GetConnectionString());

                    var capa =
                        new CapaRecord
                        {
                            Id = Guid.NewGuid(),

                            NcrId =
                                ncr.Id,

                            RootCause =
                                "Pending investigation",

                            CorrectiveAction =
                                "Repair weld",

                            PreventiveAction =
                                "Review welding parameters",

                            AssignedTo =
                                CurrentUserContext.Username,

                            DueDate =
                                DateTime.UtcNow.AddDays(7),

                            Status =
                                CapaStatus.Open,

                            IsEffective = false,

                            CreatedDate =
                                DateTime.UtcNow,

                            CreatedBy =
                                CurrentUserContext.Username,

                            Priority =
                                CapaPriority.High,

                            Title =
                                $"CAPA for weld {SelectedWeld.WeldNumber}"
                        };

                    capaRepository.Add(
                        capa);

                    _repairRepository.Add(repair);

                    _historyTrackingService.Track(
                        SelectedWeld,
                        "Repair Requested",
                        $"Repair #{repair.RepairNumber} requested.");
                }
                else
                {
                    // =====================================
                    // CLOSE OPEN REPAIRS
                    // =====================================

                    var repairs =
                        _repairRepository.GetByWeld(
                            SelectedWeld.Id);

                    foreach (var repair in repairs
                        .Where(x =>
                            x.Status != RepairStatus.Closed))
                    {
                        repair.Status =
                            RepairStatus.Closed;

                        repair.CompletedDate =
                            DateTime.UtcNow;

                        _repairRepository.Update(repair);
                    }

                    // =====================================
                    // WORKFLOW RESET
                    // =====================================

                    SelectedWeld.WorkflowStatus =
                        WeldWorkflowStatus.Accepted;

                    _workflowEngine.MarkAccepted(
                        SelectedWeld,
                        out _);



                    // =====================================
                    // HISTORY
                    // =====================================

                    _historyTrackingService.Track(
                        SelectedWeld,
                        "Repair Accepted",
                        "Repair verified successfully by NDT.");
                }

                await _weldService.UpdateAsync(SelectedWeld);

                // =====================================
                // HISTORY ENTRY
                // =====================================

                _historyTrackingService.Track(
                    SelectedWeld,
                    "NDT Added",
                    $"{window.SavedResult.NdtMethod} = {window.SavedResult.Result}");

                LoadNdtHistory();

                await LoadAsync();
            }
        }

        [RelayCommand]
        private async Task MarkRepairedAsync()
        {
            if (SelectedWeld == null)
                return;

            var complianceService =
                new WeldComplianceService();

            complianceService.MarkRepaired(
                SelectedWeld);

            await _weldService.UpdateAsync(
                SelectedWeld);

            // =====================================
            // HISTORY ENTRY
            // =====================================

            _historyTrackingService.Track(
                SelectedWeld,
                "Repair Completed",
                $"Repair completed for weld {SelectedWeld.WeldNumber}.");

            await LoadAsync();
        }

        [RelayCommand]
        private async Task ReleaseWeldAsync()
        {

            if (SelectedWeld == null) return;

            var holdPoints =
                _holdPointRepository.GetByWeld(
                SelectedWeld.Id);

            if (!_holdPointWorkflowService.CanAdvanceWorkflow(holdPoints, out var reasons))
            {
                MessageBox.Show(
                    "Cannot release weld.\n\n" +
                    string.Join(
                        "\n",
                        reasons),
                    "Hold Point Blocking",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // =====================================
            // LOAD REPAIR STATUS
            // =====================================

            var repairs =
                _repairRepository.GetByWeld(
                    SelectedWeld.Id);

            var hasOpenRepairs =
                repairs.Any(x =>
                    x.Status !=
                    RepairStatus.Closed);

            // =====================================
            // LOAD NDT RESULTS
            // =====================================

            var ndtResults =
                _ndtRepository.GetByWeld(
                    SelectedWeld.Id);

            var hasAcceptedNdt =
                ndtResults.Any(x =>
                    x.Result ==
                    NdtResultType.Accept);

            // =====================================
            // BUILD RELEASE CONTEXT
            // =====================================

            var context =
                new WeldReleaseContext
                {
                    HasApprovedWps = true,
                    HasQualifiedWelder = true,
                    HasAcceptedNdt = hasAcceptedNdt,
                    HasOpenRepairs = hasOpenRepairs,
                    HasMaterialTraceability = true,
                    HasValidConsumables = true,
                    HasCalibrationCompliance = true
                };

            // =====================================
            // BLOCKING ANALYSIS
            // =====================================

            // =====================================
            // LOAD WPS
            // =====================================

            var selectedWps =
                FindLatestWps(SelectedWeld.WpsNumber);

            if (selectedWps == null)
            {
                MessageBox.Show(
                    "WPS not found.",
                    "Release Blocked");

                return;
            }

            // =====================================
            // LOAD PQR
            // =====================================

            Pqr? linkedPqr = null;

            if (selectedWps.PqrId != null)
            {
                linkedPqr =
                    _pqrRepository.GetById(
                        selectedWps.PqrId.Value);
            }

            if (linkedPqr == null)
            {
                MessageBox.Show(
                    "Linked PQR not found.",
                    "Release Blocked");

                return;
            }

            // =====================================
            // LOAD WELDER
            // =====================================

            var selectedWelder =
                _welderRepository.GetAll()
                    .FirstOrDefault(x =>
                        x.WelderNumber ==
                        SelectedWeld.WelderNumber);

            if (selectedWelder == null)
            {
                MessageBox.Show(
                    "Welder qualification not found.",
                    "Release Blocked");

                return;
            }

            // =====================================
            // BLOCKING ANALYSIS
            // =====================================

            var blockingResult =
                _blockingAnalysisService.Analyze(
                    SelectedWeld,
                    selectedWps,
                    linkedPqr,

                    selectedWelder.Position,
                    selectedWelder.PNumber,
                    selectedWelder.QualificationDate);

            if (!blockingResult.IsReady)
            {
                MessageBox.Show(
                    string.Join(
                        Environment.NewLine,
                        blockingResult.Blockers),
                    "Release Blocked",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // =====================================
            // RELEASE WORKFLOW
            // =====================================


            if (!PermissionService.HasPermission(
    SystemPermission.ReleaseWeld))
            {
                MessageBox.Show(
                    "You do not have permission to release welds.",
                    "Access Denied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            // =====================================
            // PREVENT DOUBLE RELEASE
            // =====================================

            if (SelectedWeld.WorkflowStatus
                == WeldWorkflowStatus.Released)
            {
                MessageBox.Show(
                    "Weld already released.");

                return;
            }

            // =====================================
            // WORKFLOW TRANSITION
            // =====================================

            if (!_workflowEngine.ReleaseWeld(
                    SelectedWeld,
                    true,
                    true,
                    hasAcceptedNdt,
                    hasOpenRepairs,
                    out var workflowError))
            {
                MessageBox.Show(
                    workflowError);

                return;
            }
           
            // =====================================
            // APPLY RELEASE FLAGS
            // =====================================

            SelectedWeld.IsReleased = true;

            SelectedWeld.ReleasedBy =
                CurrentUserContext.Username;

            SelectedWeld.ReleasedDate =
                DateTime.UtcNow;

            // =====================================
            // SAVE
            // =====================================


            SelectedWeld.ReleaseReady = true;
            SelectedWeld.BlockingCount = 0;
            SelectedWeld.ReadinessSummary = "Released";

            await _weldService.UpdateAsync(
                SelectedWeld);


            // =====================================
            // AUDIT TRAIL
            // =====================================

            var auditService =
                new AuditTrailService(
                    DatabasePath.GetConnectionString());

            auditService.Log(
                new AuditTrailEntry
                {
                    UserName =
                        CurrentUserContext.Username,

                    Module =
                        "Weld Register",

                    Action =
                        "Released Weld",

                    EntityType =
                        "Weld",

                    EntityId =
                        SelectedWeld.Id.ToString(),

                    Details =
                        $"Weld {SelectedWeld.WeldNumber} released."
                });

            AuditService.Log(
                "RELEASE WELD",
                "Quality",
                $"Released weld: {SelectedWeld.WeldNumber}");


            // =====================================
            // HISTORY
            // =====================================

            _historyTrackingService.Track(
                SelectedWeld,
                "Weld Released",
                $"Weld {SelectedWeld.WeldNumber} released for turnover.");

            // =====================================
            // REFRESH
            // =====================================

            await LoadAsync();

            MessageBox.Show(
                "Weld successfully released.",
                "Release Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadBlockingAnalysis();
        }

        [RelayCommand]
        private void OpenTraceabilityEditor()
        {
            if (SelectedWeld == null)
            {
                MessageBox.Show(
                    "Select a weld first.");

                return;
            }

            var window =
                new Window
                {
                    Title = "Traceability Editor",

                    Width = 1200,
                    Height = 700,

                    Content =
                        new WeldTraceabilityEditorView(
                            SelectedWeld.Id)
                };

            window.ShowDialog();
        }

        [RelayCommand]
        private async Task CloseWeld()
        {
            if (SelectedWeld == null)
                return;

            var rules =
                new WeldLifecycleRuleService();

            if (!rules.CanClose(
                    SelectedWeld,
                    out var error))
            {
                MessageBox.Show(error);

                return;
            }

            SelectedWeld.WorkflowStatus =
                WeldWorkflowStatus.Closed;

            await _weldService
                .UpdateAsync(
                    SelectedWeld);

            // =====================================
            // HISTORY ENTRY
            // =====================================

            _historyTrackingService.Track(
                SelectedWeld,
                "Weld Closed",
                $"Weld {SelectedWeld.WeldNumber} closed.");

            await LoadAsync();

            MessageBox.Show(
                "Weld successfully closed.");
        }


        [RelayCommand]
        private void OpenTraceability()
        {
            if (SelectedWeld == null)
                return;

            var window =
                new Window
                {
                    Title = "Traceability Editor",

                    Width = 1200,
                    Height = 700,

                    Content =
                        new WeldTraceabilityEditorView(
                            SelectedWeld.Id)
                };

            window.ShowDialog();
        }

        [RelayCommand]
        private void OpenNcrManagement()
        {
            var window =
                new Window
                {
                    Title = "NCR Management",

                    Width = 1400,
                    Height = 800,

                    Content =
                        new NcrManagementView()
                };

            window.ShowDialog();
        }
        private async Task ExportDataBookAsync()
        {
            try
            {
                var welds =
                    Welds.ToList();

                var allNdt =
                    new List<WeldNdtResult>();

                var allHistory =
                    new List<WeldHistoryEntry>();

                foreach (var weld in welds)
                {
                    var ndt =
                        _ndtRepository.GetByWeld(weld.Id);

                    allNdt.AddRange(ndt);

                    var history =
                        _historyRepository.GetByWeld(weld.Id);

                    allHistory.AddRange(history);
                }

                var builder =
                    new WeldDataBookService();

                var revision =
                    new DataBookRevision
                {
                    Revision = "0",

                    PreparedBy =
                        Environment.UserName,

                    ApprovedBy =
                        "QA Manager",

                    ProjectNumber =
                        "BP-001",

                    DataBookNumber =
                        "DB-001",

                    DocumentTitle =
                        "Welding Quality Data Book",

                    ClientDocumentNumber =
                        "CLIENT-001",

                    RevisionDate =
                        DateTime.Now,

                    Notes =
                        "Initial Issue"
                };

                var book =
                    builder.Build(
                        new CompanyProfile
                        {
                            CompanyName =
                                "Tetracube Pty Ltd",

                            Address =
                                "South Africa",

                            Phone =
                                "+27820440396",

                            Email =
                                "renier@tetracube.co.za"
                        },
                        "Blow Pot",
                        "Demo Client",
                        welds,
                        allNdt,
                        allHistory,
                        revision);

                var desktop =
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.Desktop);

                var file =
                    Path.Combine(
                        desktop,
                        $"WeldDataBook_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                var exporter =
                    new PdfExportService();

                exporter.Export(book, file);

                MessageBox.Show(
                    $"PDF exported successfully:\n\n{file}",
                    "Export Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Export failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
            

        private async Task
                    ExportTurnoverPackageAsync()
                {
                    try
                {
                var welds =
                    Welds.ToList();

                var allNdt =
                    new List<WeldNdtResult>();

                var allHistory =
                    new List<WeldHistoryEntry>();

                foreach (var weld in welds)
                {
                    var ndt =
                        _ndtRepository.GetByWeld(
                            weld.Id);

                    allNdt.AddRange(ndt);

                    var history =
                        _historyRepository.GetByWeld(
                            weld.Id);

                    allHistory.AddRange(history);
                }

                // =====================================
                // BUILD DATABOOK
                // =====================================

                var builder =
                    new WeldDataBookService();

                var revision =
                    new DataBookRevision
                    {
                        Revision = "0",

                        PreparedBy =
                            Environment.UserName,

                        ApprovedBy =
                            "QA Manager",

                        ProjectNumber =
                            "BP-001",

                        DataBookNumber =
                            "DB-001",

                        DocumentTitle =
                            "Welding Quality Data Book",

                        ClientDocumentNumber =
                            "CLIENT-001",

                        RevisionDate =
                            DateTime.Now,

                        Notes =
                            "Initial Issue"
                    };

                var dataBook =
                    builder.Build(
                        new CompanyProfile
                        {
                            CompanyName =
                                "Tetracube Pty Ltd",

                            Address =
                                "South Africa",

                            Phone =
                                "+27820440396",

                            Email =
                                "renier@tetracube.co.za"
                        },
                        "Blow Pot",
                        "Demo Client",
                        welds,
                        allNdt,
                        allHistory,
                        revision);

                        // =====================================
                        // AUTO LOAD APPROVED DOCUMENTS
                        // =====================================

                        var vaultRepository =
                            new DocumentVaultRepository( 
                                DatabasePath.GetConnectionString());

var approvedWps =
    vaultRepository.GetApprovedByCategory(
        DocumentCategoryType.WPS);

                var approvedWpqr =
                    vaultRepository.GetApprovedByCategory(
                        DocumentCategoryType.WPQR);

                var approvedNdt =
                    vaultRepository.GetApprovedByCategory(
                        DocumentCategoryType.NDT);

                // =====================================
                // WPS
                // =====================================

                foreach (var doc in approvedWps)
                {
                    dataBook.Attachments.Add(
                        new DataBookAttachment
                        {
                            FileName =
                                doc.FileName,

                            FilePath =
                                doc.FilePath,

                            Category =
                                doc.Category.ToString(),

                            Description =
                                doc.Description,

                            DocumentNumber =
                                doc.DocumentNumber,

                            Title =
                                doc.Title,

                            Revision =
                                doc.Revision,

                            Status =
                                doc.Status
                        });
                }

                // =====================================
                // WPQR
                // =====================================

                foreach (var doc in approvedWpqr)
                {
                    dataBook.Attachments.Add(
                        new DataBookAttachment
                        {
                            FileName =
                                doc.FileName,

                            FilePath =
                                doc.FilePath,

                            Category =
                                doc.Category.ToString(),

                            Description =
                                doc.Description,

                            DocumentNumber =
                                doc.DocumentNumber,

                            Title =
                                doc.Title,

                            Revision =
                                doc.Revision,

                            Status =
                                doc.Status
                        });
                }

                // =====================================
                // NDT
                // =====================================

                foreach (var doc in approvedNdt)
                {
                    dataBook.Attachments.Add(
                        new DataBookAttachment
                        {
                            FileName =
                                doc.FileName,

                            FilePath =
                                doc.FilePath,

                            Category =
                                doc.Category.ToString(),

                            Description =
                                doc.Description,

                            DocumentNumber =
                                doc.DocumentNumber,

                            Title =
                                doc.Title,

                            Revision =
                                doc.Revision,

                            Status =
                                doc.Status
                        });
                }

                // =====================================
                // OUTPUT FOLDER
                // =====================================

                var folder =
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.Desktop),
                        "TurnoverPackages",
                        DateTime.Now.ToString(
                            "yyyyMMdd_HHmmss"));

                // =====================================
                // GENERATE PACKAGE
                // =====================================

                _turnoverPackageService.Generate(
                    dataBook,
                    folder);

                MessageBox.Show(
                    $"Turnover package exported successfully:\n\n{folder}",
                    "Export Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Export failed:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        
    }
}
