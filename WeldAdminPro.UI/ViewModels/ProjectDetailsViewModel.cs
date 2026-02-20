using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Guards;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ProjectDetailsViewModel : ObservableObject
	{
		private readonly IProjectRepository _projectRepository;
		private readonly ProjectStockUsageRepository _usageRepository;
		private readonly StockRepository _stockRepository;
		private readonly StockAvailabilityService _stockAvailability;
		private readonly FinancialService _financialService;
		private readonly StockProjectTransactionService _transactionService;

		public Project Project { get; }

		public IReadOnlyList<ProjectStatus> Statuses { get; }

		public ObservableCollection<StockItem> StockItems { get; }
		public ObservableCollection<ProjectStockUsage> IssuedStockHistory { get; }
		public ObservableCollection<ProjectStockSummary> ProjectStockSummary { get; }
		public ObservableCollection<StockTransaction> ProjectCostTransactions { get; }

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

		private ProjectStockUsage? _selectedIssuedUsage;
		public ProjectStockUsage? SelectedIssuedUsage
		{
			get => _selectedIssuedUsage;
			set
			{
				SetProperty(ref _selectedIssuedUsage, value);
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

		private decimal GetRemainingIssuedBalance(Guid stockItemId) =>
			IssuedStockHistory
				.Where(x => x.StockItemId == stockItemId)
				.Sum(x => x.Quantity);

		public decimal RemainingIssuedBalance =>
			SelectedIssuedUsage == null
				? 0
				: GetRemainingIssuedBalance(SelectedIssuedUsage.StockItemId);

		public IEnumerable<ProjectStockUsage> ReturnableIssuedItems =>
			IssuedStockHistory
				.GroupBy(x => x.StockItemId)
				.Select(g => g.First())
				.Where(x => GetRemainingIssuedBalance(x.StockItemId) > 0);

		public bool CanReturnStock =>
			IsPersisted &&
			IsEditable &&
			SelectedIssuedUsage != null &&
			ReturnQuantity > 0 &&
			ReturnQuantity <= RemainingIssuedBalance;

		// ================= CONSTRUCTOR =================

		public ProjectDetailsViewModel(Project project)
		{
			_projectRepository = new ProjectRepository();
			_usageRepository = new ProjectStockUsageRepository();
			_stockRepository = new StockRepository();
			_stockAvailability = new StockAvailabilityService();
			_financialService = new FinancialService();
			_transactionService = new StockProjectTransactionService();

			Project = project;

			if (Project.JobNumber == 0)
				Project.JobNumber = _projectRepository.GetNextJobNumber();

			Statuses = Enum.GetValues(typeof(ProjectStatus))
				.Cast<ProjectStatus>()
				.ToList();

			StockItems = new ObservableCollection<StockItem>(
				_stockRepository.GetAll());

			IssuedStockHistory = new ObservableCollection<ProjectStockUsage>();
			ProjectStockSummary = new ObservableCollection<ProjectStockSummary>();
			ProjectCostTransactions = new ObservableCollection<StockTransaction>();

			RefreshProjectData();
		}

		// ================= ISSUE STOCK =================

		[RelayCommand]
		private void IssueStock()
		{
			try
			{
				if (!CanIssueStock)
					return;

				_transactionService.IssueStock(
					Project,
					SelectedStockItem!,
					IssueQuantity,
					IssuedBy);

				IssueQuantity = 0;
				RefreshProjectData();
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(
					ex.Message,
					"Issue Failed",
					System.Windows.MessageBoxButton.OK,
					System.Windows.MessageBoxImage.Warning);
			}
		}

		// ================= RETURN STOCK =================

		[RelayCommand]
		private void ReturnStock()
		{
			try
			{
				if (!CanReturnStock || SelectedIssuedUsage == null)
					return;

				var stockItem = _stockRepository.GetById(SelectedIssuedUsage.StockItemId);
				if (stockItem == null)
					throw new InvalidOperationException("Stock item not found.");

				_transactionService.ReturnStock(
					Project,
					stockItem,
					ReturnQuantity,
					SelectedIssuedUsage.UnitCostAtIssue,
					SelectedIssuedUsage.IssuedBy ?? "");

				ReturnQuantity = 0;
				SelectedIssuedUsage = null;

				RefreshProjectData();
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(
					ex.Message,
					"Return Stock Error",
					System.Windows.MessageBoxButton.OK,
					System.Windows.MessageBoxImage.Warning);
			}
		}

		// ================= INVOICE RULES =================

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
			foreach (var t in _stockRepository.GetProjectTransactions(Project.Id))
				ProjectCostTransactions.Add(t);

			OnPropertyChanged(nameof(ReturnableIssuedItems));
			OnPropertyChanged(nameof(RemainingIssuedBalance));
			OnPropertyChanged(nameof(AvailableQuantity));
			OnPropertyChanged(nameof(CanIssueStock));
			OnPropertyChanged(nameof(CanReturnStock));
			OnPropertyChanged(nameof(Variance));
			OnPropertyChanged(nameof(MarginPercentage));
		}

		// ================= SAVE =================

		[RelayCommand]
		private void Save()
		{
			try
			{
				ApplyInvoiceRules();

				ProjectCompletionGuard.ValidateBeforeSave(Project);

				var existing = _projectRepository.GetById(Project.Id);

				if (existing == null)
					_projectRepository.Add(Project);
				else
					_projectRepository.Update(Project);

				RequestClose?.Invoke();
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(
					ex.Message,
					"Cannot Save Project",
					System.Windows.MessageBoxButton.OK,
					System.Windows.MessageBoxImage.Warning);
			}
		}

		[RelayCommand]
		private void Cancel() => RequestClose?.Invoke();
	}
}