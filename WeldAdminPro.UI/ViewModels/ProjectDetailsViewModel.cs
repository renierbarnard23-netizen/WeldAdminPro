using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WeldAdminPro.Core.Guards;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Quality;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using WeldAdminPro.UI.Views;

namespace WeldAdminPro.UI.ViewModels
{
    
    // ✅ FIX: Local UI helper class (prevents missing type errors)
    public class ReturnableItemDisplay
    {
        public Guid Id { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string Display { get; set; } = string.Empty;

    }

    public partial class ProjectDetailsViewModel : ObservableObject
    {
        private readonly ProjectDocumentFileRepository _fileRepo = new();
        private readonly IProjectRepository _projectRepository;
        private readonly ProjectStockUsageRepository _usageRepository;
        private readonly StockAvailabilityService _stockAvailability;
        private readonly FinancialService _financialService;
        private readonly ProjectMaterialService _materialService;
        private readonly ProjectComplianceService _complianceService = new();
        private readonly ProjectDocumentRepository _docRepo = new();



        // ✅ FORCE correct model (prevents namespace confusion permanently)
        public WeldAdminPro.Core.Models.Project Project { get; }

        public IReadOnlyList<ProjectStatus> Statuses { get; }

        public ObservableCollection<StockItem> StockItems { get; }
        public ObservableCollection<ProjectStockUsage> IssuedStockHistory { get; }
        public ObservableCollection<ProjectStockSummary> ProjectStockSummary { get; }
        public ObservableCollection<StockTransaction> ProjectCostTransactions { get; }

        public ObservableCollection<ReturnableItemDisplay> ReturnableIssuedItems { get; } = new();

        public ObservableCollection<ProjectDocument> ProjectDocuments { get; } = new();

        public IEnumerable<IGrouping<string, ProjectDocument>>
            GroupedProjectDocuments =>
                ProjectDocuments
                    .GroupBy(x => x.Category)
                    .OrderBy(x => x.Key);

        public BitmapImage? WeldMapImage { get; set; }

        public ObservableCollection<WeldMapPoint> WeldPoints { get; set; }
        = new ObservableCollection<WeldMapPoint>();

        public event Action? RequestClose;

        public bool IsPersisted => Project.Id != Guid.Empty;

        public bool IsLocked =>
            Project.IsInvoiced ||
            Project.Status == ProjectStatus.Completed;

        public bool IsEditable =>
            ProjectCompletionGuard.IsEditable(Project) && !IsLocked;

        public bool CanSave => IsEditable;

        public decimal Variance =>
            _financialService.CalculateVariance(Project);

        public decimal MarginPercentage =>
            _financialService.CalculateMarginPercentage(Project);

        // ================= COMPLIANCE =================

        private ProjectComplianceResult _complianceResult = new();
        public ProjectComplianceResult ComplianceResult
        {
            get => _complianceResult;
            set
            {
                _complianceResult = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ComplianceStatus));
                OnPropertyChanged(nameof(ComplianceColor));
                OnPropertyChanged(nameof(WpsComplianceText));
            }
        }

        public string ComplianceStatus =>
    ComplianceResult?.IsCompliant == true
        ? "✔ PROJECT COMPLIANT"
        : "❌ PROJECT NOT COMPLIANT";

        public Brush ComplianceColor =>
    ComplianceResult?.IsCompliant == true
        ? Brushes.Green
        : Brushes.Red;

        public string ProjectStatusText =>
            ComplianceResult?.IsCompliant == true ? "COMPLIANT" : "NON-COMPLIANT";

        public string WpsComplianceText =>
            $"WPS Compliance: {(
                ComplianceResult.TotalWps == 0
                ? 100
                : (double)ComplianceResult.CompliantWps / ComplianceResult.TotalWps * 100
         ):F1}%";

        public string DocumentComplianceText =>
            $"Document Compliance: {ComplianceResult?.DocumentCompliancePercent:F1}%";

        // ================= ISSUE FIELDS =================

        private StockItem? _selectedStockItem;
        public StockItem? SelectedStockItem
        {
            get => _selectedStockItem;
            set
            {
                SetProperty(ref _selectedStockItem, value);
                OnPropertyChanged(nameof(AvailableQuantity));
                OnPropertyChanged(nameof(CanIssueStock));
            }
        }

        private decimal _issueQuantity;
        public decimal IssueQuantity
        {
            get => _issueQuantity;
            set
            {
                SetProperty(ref _issueQuantity, value);
                OnPropertyChanged(nameof(CanIssueStock));
            }
        }

        private string _issuedBy = string.Empty;
        public string IssuedBy
        {
            get => _issuedBy;
            set
            {
                SetProperty(ref _issuedBy, value);
                OnPropertyChanged(nameof(CanIssueStock));
            }
        }

        private ReturnableItemDisplay? _selectedIssuedItem;
        public ReturnableItemDisplay? SelectedIssuedItem
        {
            get => _selectedIssuedItem;
            set
            {
                SetProperty(ref _selectedIssuedItem, value);
                OnPropertyChanged(nameof(RemainingIssuedBalance));
                OnPropertyChanged(nameof(CanReturnStock));
            }
        }

        private decimal _returnQuantity;
        public decimal ReturnQuantity
        {
            get => _returnQuantity;
            set
            {
                SetProperty(ref _returnQuantity, value);
                OnPropertyChanged(nameof(CanReturnStock));
            }
        }

        public int AvailableQuantity =>
            SelectedStockItem == null
                ? 0
                : _stockAvailability.GetAvailableQuantity(SelectedStockItem.Id);

        public bool CanIssueStock =>
            IsPersisted &&
            IsEditable &&
            SelectedStockItem != null &&
            IssueQuantity > 0 &&
            _stockAvailability.CanIssue(SelectedStockItem.Id, IssueQuantity) &&
            !string.IsNullOrWhiteSpace(IssuedBy);

        public decimal RemainingIssuedBalance =>
            SelectedIssuedItem?.Quantity ?? 0;

        public bool CanReturnStock =>
            IsPersisted &&
            IsEditable &&
            SelectedIssuedItem != null &&
            ReturnQuantity > 0 &&
            ReturnQuantity <= RemainingIssuedBalance;

        // ================= CONSTRUCTOR =================

        public ProjectDetailsViewModel(WeldAdminPro.Core.Models.Project project)
        {
            _projectRepository = new ProjectRepository();
            _usageRepository = new ProjectStockUsageRepository();
            _stockAvailability = new StockAvailabilityService();
            _financialService = new FinancialService();
            _materialService = new ProjectMaterialService();

            Project = project;
            Project.LastModifiedOn = DateTime.Now;

            new ProjectDocumentService().InitializeProjectDocuments(Project.Id);

            Statuses = Enum.GetValues(typeof(ProjectStatus))
                .Cast<ProjectStatus>()
                .ToList();

            StockItems = new ObservableCollection<StockItem>(
                _materialService.GetStockItems());

            IssuedStockHistory = new ObservableCollection<ProjectStockUsage>();
            ProjectStockSummary = new ObservableCollection<ProjectStockSummary>();
            ProjectCostTransactions = new ObservableCollection<StockTransaction>();

            RefreshProjectData();
            
        }

        // ================= ISSUE STOCK =================

        [RelayCommand]
        private void IssueStock()
        {
            if (!CanIssueStock) return;

            _materialService.IssueMaterial(
                Project,
                SelectedStockItem!,
                IssueQuantity,
                IssuedBy);

            IssueQuantity = 0;
            RefreshProjectData();
        }

        // ================= RETURN STOCK =================

        [RelayCommand]
        private void ReturnStock()
        {
            if (!CanReturnStock || SelectedIssuedItem == null) return;

            var stockItem = StockItems.FirstOrDefault(x => x.Id == SelectedIssuedItem.Id);
            if (stockItem == null)
                throw new InvalidOperationException("Stock item not found.");

            _materialService.ReturnMaterial(
                Project,
                stockItem,
                ReturnQuantity,
                SelectedIssuedItem.UnitCost,
                "Return");

            ReturnQuantity = 0;
            SelectedIssuedItem = null;

            RefreshProjectData();
        }

        // ================= LOAD RETURNABLE =================

        private void LoadReturnableItems()
        {
            ReturnableIssuedItems.Clear();

            var issued = _materialService.GetReturnableItems(Project.Id);

            foreach (var item in issued)
            {
                var stock = StockItems.FirstOrDefault(s => s.Id == item.StockItemId);
                if (stock == null) continue;

                ReturnableIssuedItems.Add(new ReturnableItemDisplay
                {
                    Id = item.StockItemId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    Display = $"{stock.ItemCode} - {stock.Description} (Issued: {item.Quantity})"
                });
            }
        }

        private void AttachDocumentEvents( ProjectDocument doc)
        {
            doc.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName ==
                    nameof(ProjectDocument.IsRequired))
                {
                    doc.LastModifiedOn =
                        DateTime.Now;

                    _docRepo.Update(doc);

                    ComplianceResult =
                        _complianceService
                            .Evaluate(Project.Id);
                }
            };
        }

        // ================= REFRESH =================

        private void RefreshProjectData()
        {
            IssuedStockHistory.Clear();
            foreach (var u in _usageRepository.GetByProjectId(Project.Id))
                IssuedStockHistory.Add(u);

            ProjectStockSummary.Clear();
            foreach (var s in _usageRepository.GetProjectStockSummary(Project.Id))
                ProjectStockSummary.Add(s);

            ProjectCostTransactions.Clear();
            foreach (var t in _materialService.GetProjectTransactions(Project.Id))
                ProjectCostTransactions.Add(t);

            ProjectDocuments.Clear();

            var docs =
                _docRepo.GetByProject(Project.Id);

            var loadedDocs =
                new List<ProjectDocument>();

            foreach (var d in docs)
            {
                d.Files =
                    _fileRepo.GetByDocument(d.Id);

                d.IsUploaded =
                    d.Files.Any();

                if (d.Files.Any())
                {
                    d.UploadedDate =
                        d.Files.Max(x => x.UploadedOn);
                }

                AttachDocumentEvents(d);

                loadedDocs.Add(d);
            }

            ProjectDocuments.Clear();

            foreach (var d in loadedDocs)
            {
                if (string.IsNullOrWhiteSpace(d.Category))
                {
                    d.Category =
                        d.DocumentType switch
                        {
                            "Drawings" => "Engineering",
                            "Weld Map" => "Engineering",

                            "Method Statement" => "Quality",
                            "Quality Control Plan" => "Quality",
                            "WPS" => "Quality",
                            "PQR" => "Quality",
                            "WPQR" => "Quality",

                            "Inspection Reports" => "Inspection",
                            "NDT Reports" => "Inspection",

                            "Material Certificates" => "Certificates",
                            "Consumable Certificates" => "Certificates",

                            "Index" => "Turnover",
                            "Final Data Book" => "Turnover",

                            _ => "General"
                        };

                    _docRepo.Update(d);
                }

                ProjectDocuments.Add(d);
            }

            LoadReturnableItems();

            OnPropertyChanged(nameof(RemainingIssuedBalance));
            OnPropertyChanged(nameof(AvailableQuantity));
            OnPropertyChanged(nameof(CanIssueStock));
            OnPropertyChanged(nameof(CanReturnStock));
            OnPropertyChanged(nameof(Variance));
            OnPropertyChanged(nameof(MarginPercentage));
    

            ComplianceResult = _complianceService.Evaluate(Project.Id);
            OnPropertyChanged( nameof(GroupedProjectDocuments));
        }

        // ================= SAVE =================

        [RelayCommand]
        private void Save()
        {
            ApplyInvoiceRules();

            // 🔥 COMPLIANCE BLOCK
            if (Project.Status == ProjectStatus.Completed &&
                ComplianceResult?.IsCompliant != true)
            {
                System.Windows.MessageBox.Show(
                    "Project cannot be completed.\n\nResolve compliance issues first.",
                    "Compliance Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            ProjectCompletionGuard.ValidateBeforeSave(Project);


            Project.LastModifiedOn = DateTime.Now;

            var existing = _projectRepository.GetById(Project.Id);

                if (existing == null)
                    _projectRepository.Add(Project);
                else
                    _projectRepository.Update(Project);

            foreach (var doc in ProjectDocuments)
            {
                _docRepo.Update(doc);
            }

            RequestClose?.Invoke();
        }

        // ================= RULES =================

        public void ApplyInvoiceRules()
        {
            if (Project.IsInvoiced)
            {
                Project.Status = ProjectStatus.Completed;

                if (!Project.CompletedOn.HasValue)
                    Project.CompletedOn = DateTime.UtcNow;
            }

            OnPropertyChanged(nameof(IsLocked));
            OnPropertyChanged(nameof(IsEditable));
            OnPropertyChanged(nameof(CanIssueStock));
            OnPropertyChanged(nameof(CanReturnStock));
        }

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke();


        // ================= DOCUMENT ACTIONS =================

        [RelayCommand]
        private void UploadDocument(ProjectDocument doc)
        {
            if (doc == null)
                return;

            var dialog = new OpenFileDialog();

            if (dialog.ShowDialog() != true)
                return;

            var selectedFile = dialog.FileName;

            // ==========================================
            // ALWAYS CREATE FILE RECORD
            // ==========================================

            var file = new ProjectDocumentFile
            {
                Id = Guid.NewGuid(),
                ProjectDocumentId = doc.Id,
                FileName = Path.GetFileName(selectedFile),
                FilePath = selectedFile,
                UploadedOn = DateTime.Now,
                IsApproved = false
            };

            _fileRepo.Add(file);

            // ==========================================
            // SINGLE FILE DOCUMENTS
            // ==========================================

            if (!doc.AllowMultiple)
            {
                doc.FilePath = selectedFile;
            }

            // ==========================================
            // UPDATE UI STATE
            // ==========================================

            doc.Files.Add(file);

            doc.IsUploaded = true;
            doc.UploadedDate = DateTime.Now;

            doc.LastModifiedOn = DateTime.Now;

            _docRepo.Update(doc);

            RefreshProjectData();
            
        } 
       

        [RelayCommand]
        private void OpenDocument(ProjectDocument doc)
        {
            try
            {
                // =====================================================
                // MULTI FILE DOCUMENTS
                // =====================================================

                if (doc.AllowMultiple)
                {
                    var files = _fileRepo.GetByDocument(doc.Id);

                    if (files.Count == 0)
                    {
                        MessageBox.Show("No files uploaded.");
                        return;
                    }

                    foreach (var file in files)
                    {
                        if (File.Exists(file.FilePath))
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = file.FilePath,
                                UseShellExecute = true
                            });
                        }
                    }

                    return;
                }

                // =====================================================
                // SINGLE FILE DOCUMENTS
                // =====================================================

                if (string.IsNullOrWhiteSpace(doc.FilePath))
                {
                    MessageBox.Show("No file uploaded.");
                    return;
                }

                if (!File.Exists(doc.FilePath))
                {
                    MessageBox.Show("File not found.");
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = doc.FilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening file:\n{ex.Message}");
            }
        }

        [RelayCommand]
        private void ToggleApproval(ProjectDocument doc)
        {
            if (doc == null) return;

            // 🔥 RULE: cannot approve if not uploaded
            if (!doc.IsUploaded)
            {
                System.Windows.MessageBox.Show(
                    "Document must be uploaded before approval.",
                    "Invalid Action",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            doc.IsApproved = !doc.IsApproved;

            doc.LastModifiedOn = DateTime.Now;

            if (doc.IsApproved)
            {
                doc.ApprovedOn = DateTime.Now;
                doc.ApprovedBy = Environment.UserName;
            }
            else
            {
                doc.ApprovedOn = null;
                doc.ApprovedBy = "";
            }

            _docRepo.Update(doc);

            RefreshProjectData();
        }

        [RelayCommand]
        private void OpenWeldRegister()
        {
    var window = new Window
    {
        Title = $"Weld Register - {Project.ProjectName}",
        WindowState = WindowState.Maximized,
        WindowStartupLocation = WindowStartupLocation.CenterScreen,
        Content = new WeldRegisterView(Project.Id)
    };

    window.ShowDialog();
        }
    }
}