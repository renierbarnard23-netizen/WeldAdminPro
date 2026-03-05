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

		public ObservableCollection<StockTransaction> ReturnableIssuedItems { get; } = new();

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

		private StockTransaction? _selectedIssuedItem;
		public StockTransaction? SelectedIssuedItem
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
			SelectedIssuedItem == null
				? 0
				: SelectedIssuedItem.Quantity;

		public bool CanReturnStock =>
			IsPersisted &&
			IsEditable &&
			SelectedIssuedItem != null &&
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

		// ================= RETURN STOCK =================

		[RelayCommand]
		private void ReturnStock()
		{
			if (!CanReturnStock || SelectedIssuedItem == null)
				return;

			var stockItem = _stockRepository.GetById(SelectedIssuedItem.StockItemId);

			if (stockItem == null)
				throw new InvalidOperationException("Stock item not found.");

			_transactionService.ReturnStock(
				Project,
				stockItem,
				ReturnQuantity,
				SelectedIssuedItem.UnitCost,
				"Return");

			ReturnQuantity = 0;
			SelectedIssuedItem = null;

			RefreshProjectData();
		}

		// ================= LOAD RETURNABLE ITEMS =================

		private void LoadReturnableItems()
		{
			ReturnableIssuedItems.Clear();

			var issued = _stockRepository.GetReturnableItems(Project.Id);

			foreach (var item in issued)
				ReturnableIssuedItems.Add(item);
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

			LoadReturnableItems();

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
			ApplyInvoiceRules();

			ProjectCompletionGuard.ValidateBeforeSave(Project);

			var existing = _projectRepository.GetById(Project.Id);

			if (existing == null)
				_projectRepository.Add(Project);
			else
				_projectRepository.Update(Project);

			RequestClose?.Invoke();
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

		[RelayCommand]
		private void Cancel() => RequestClose?.Invoke();
	}
}