using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

		public Project Project { get; }

		public IReadOnlyList<ProjectStatus> Statuses { get; }

		public ObservableCollection<StockItem> StockItems { get; }
		public ObservableCollection<ProjectStockUsage> IssuedStockHistory { get; }
		public ObservableCollection<ProjectStockSummary> ProjectStockSummary { get; }

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

		public event Action? RequestClose;

		public bool IsEditable =>
			ProjectCompletionGuard.IsEditable(Project);

		public bool CanSave => IsEditable;

		public int AvailableQuantity =>
			SelectedStockItem == null
				? 0
				: _stockAvailability.GetAvailableQuantity(SelectedStockItem.Id);

		public bool CanIssueStock =>
			IsEditable &&
			SelectedStockItem != null &&
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
			IsEditable &&
			SelectedIssuedUsage != null &&
			ReturnQuantity > 0 &&
			ReturnQuantity <= RemainingIssuedBalance;

		public ProjectDetailsViewModel(Project project)
		{
			Project = project;

			_projectRepository = new ProjectRepository();
			_usageRepository = new ProjectStockUsageRepository();
			_stockRepository = new StockRepository();
			_stockAvailability = new StockAvailabilityService();

			Statuses = Enum.GetValues(typeof(ProjectStatus))
				.Cast<ProjectStatus>()
				.ToList();

			StockItems = new ObservableCollection<StockItem>(
				_stockRepository.GetAll());

			IssuedStockHistory = new ObservableCollection<ProjectStockUsage>(
				_usageRepository.GetByProjectId(Project.Id));

			ProjectStockSummary = new ObservableCollection<ProjectStockSummary>(
				_usageRepository.GetProjectStockSummary(Project.Id));
		}

		private void RefreshSummary()
		{
			ProjectStockSummary.Clear();
			foreach (var row in _usageRepository.GetProjectStockSummary(Project.Id))
				ProjectStockSummary.Add(row);

			OnPropertyChanged(nameof(ReturnableIssuedItems));
			OnPropertyChanged(nameof(RemainingIssuedBalance));
			OnPropertyChanged(nameof(CanReturnStock));
			OnPropertyChanged(nameof(AvailableQuantity));
		}

		[RelayCommand]
		private void IssueStock()
		{
			if (!CanIssueStock || SelectedStockItem == null) return;

			try
			{
				// 1️⃣ Get cost snapshot
				var stockItem = _stockRepository
					.GetAll()
					.First(x => x.Id == SelectedStockItem.Id);

				decimal unitCost = stockItem.AverageUnitCost;
				decimal totalCost = IssueQuantity * unitCost;

				// 2️⃣ Record usage
				var usage = new ProjectStockUsage
				{
					ProjectId = Project.Id,
					StockItemId = SelectedStockItem.Id,
					Quantity = IssueQuantity,
					IssuedBy = IssuedBy,
					IssuedOn = DateTime.UtcNow,
					Notes = SelectedStockItem.Description
				};

				_usageRepository.Add(usage);

				// 3️⃣ Deduct stock (cost-aware)
				_stockRepository.AddTransaction(new StockTransaction
				{
					Id = Guid.NewGuid(),
					StockItemId = usage.StockItemId,
					Quantity = (int)IssueQuantity,
					Type = "OUT",
					UnitCost = unitCost,
					TransactionDate = DateTime.UtcNow,
					Reference = $"Project {Project.JobNumber}"
				});

				// 4️⃣ Update project costing
				Project.CommittedCost += totalCost;
				Project.ActualCost += totalCost;

				_projectRepository.Update(Project);

				IssuedStockHistory.Insert(0, usage);

				IssueQuantity = 0;
				IssuedBy = string.Empty;

				RefreshSummary();
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(
					ex.Message,
					"Stock Issue Failed",
					System.Windows.MessageBoxButton.OK,
					System.Windows.MessageBoxImage.Warning);
			}
		}


		[RelayCommand]
		private void ReturnStock()
		{
			if (!CanReturnStock || SelectedIssuedUsage == null) return;

			try
			{
				var stockItem = _stockRepository
					.GetAll()
					.First(x => x.Id == SelectedIssuedUsage.StockItemId);

				decimal unitCost = stockItem.AverageUnitCost;
				decimal totalCost = ReturnQuantity * unitCost;

				var usage = new ProjectStockUsage
				{
					ProjectId = Project.Id,
					StockItemId = SelectedIssuedUsage.StockItemId,
					Quantity = -ReturnQuantity,
					IssuedBy = IssuedBy,
					IssuedOn = DateTime.UtcNow,
					Notes = SelectedIssuedUsage.Notes
				};

				_usageRepository.Add(usage);

				_stockRepository.AddTransaction(new StockTransaction
				{
					Id = Guid.NewGuid(),
					StockItemId = usage.StockItemId,
					Quantity = (int)ReturnQuantity,
					Type = "IN",
					UnitCost = unitCost,
					TransactionDate = DateTime.UtcNow,
					Reference = $"Return from Project {Project.JobNumber}"
				});

				// Reduce project cost
				Project.CommittedCost -= totalCost;
				Project.ActualCost -= totalCost;

				_projectRepository.Update(Project);

				IssuedStockHistory.Insert(0, usage);

				ReturnQuantity = 0;
				SelectedIssuedUsage = null;

				RefreshSummary();
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(
					ex.Message,
					"Stock Return Failed",
					System.Windows.MessageBoxButton.OK,
					System.Windows.MessageBoxImage.Warning);
			}
		}


		[RelayCommand]
		private void Save()
		{
			try
			{
				ProjectCompletionGuard.ValidateBeforeSave(Project);
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
