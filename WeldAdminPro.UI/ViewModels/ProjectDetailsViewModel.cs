using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ProjectDetailsViewModel : ObservableObject
	{
		private readonly IProjectRepository _projectRepository;
		private readonly StockRepository _stockRepository;
		private readonly ProjectStockUsageRepository _usageRepository;

		public Project Project { get; }

		public IReadOnlyList<ProjectStatus> Statuses { get; }

		public ObservableCollection<StockItem> StockItems { get; }
		public ObservableCollection<ProjectStockUsage> IssuedStockHistory { get; }
		public ObservableCollection<ProjectStockSummary> ProjectStockSummary { get; }

		[ObservableProperty] private StockItem? selectedStockItem;
		[ObservableProperty] private decimal issueQuantity;
		[ObservableProperty] private string issuedBy = string.Empty;
		[ObservableProperty] private ProjectStockUsage? selectedIssuedUsage;
		[ObservableProperty] private decimal returnQuantity;

		public event Action? RequestClose;

		public bool IsEditable =>
			Project.Status != ProjectStatus.Completed &&
			Project.Status != ProjectStatus.Cancelled;

		public int AvailableQuantity =>
			SelectedStockItem == null
				? 0
				: _stockRepository.GetAvailableQuantity(SelectedStockItem.Id);

		public bool CanIssueStock =>
			IsEditable &&
			SelectedStockItem != null &&
			IssueQuantity > 0 &&
			IssueQuantity <= AvailableQuantity &&
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
			_stockRepository = new StockRepository();
			_usageRepository = new ProjectStockUsageRepository();

			Statuses = Enum.GetValues(typeof(ProjectStatus))
				.Cast<ProjectStatus>()
				.ToList();

			StockItems = new ObservableCollection<StockItem>(_stockRepository.GetAll());

			IssuedStockHistory = new ObservableCollection<ProjectStockUsage>(
				_usageRepository.GetByProjectId(Project.Id));

			ProjectStockSummary = new ObservableCollection<ProjectStockSummary>(
				_usageRepository.GetProjectStockSummary(Project.Id));
		}

		partial void OnSelectedStockItemChanged(StockItem? value)
		{
			OnPropertyChanged(nameof(AvailableQuantity));
			OnPropertyChanged(nameof(CanIssueStock));
		}

		partial void OnIssueQuantityChanged(decimal value)
		{
			OnPropertyChanged(nameof(CanIssueStock));
		}

		partial void OnIssuedByChanged(string value)
		{
			OnPropertyChanged(nameof(CanIssueStock));
		}

		partial void OnSelectedIssuedUsageChanged(ProjectStockUsage? value)
		{
			OnPropertyChanged(nameof(RemainingIssuedBalance));
			OnPropertyChanged(nameof(CanReturnStock));
		}

		partial void OnReturnQuantityChanged(decimal value)
		{
			OnPropertyChanged(nameof(CanReturnStock));
		}

		private void RefreshSummary()
		{
			ProjectStockSummary.Clear();
			foreach (var row in _usageRepository.GetProjectStockSummary(Project.Id))
				ProjectStockSummary.Add(row);

			OnPropertyChanged(nameof(RemainingIssuedBalance));
			OnPropertyChanged(nameof(CanReturnStock));
		}

		[RelayCommand]
		private void IssueStock()
		{
			if (!CanIssueStock) return;

			var usage = new ProjectStockUsage
			{
				ProjectId = Project.Id,
				StockItemId = SelectedStockItem!.Id,
				Quantity = IssueQuantity,
				IssuedBy = IssuedBy,
				IssuedOn = DateTime.UtcNow,
				Notes = SelectedStockItem.Description
			};

			_usageRepository.Add(usage);

			_stockRepository.AddTransaction(new StockTransaction
			{
				Id = Guid.NewGuid(),
				StockItemId = usage.StockItemId,
				Quantity = (int)IssueQuantity,
				Type = "OUT",
				TransactionDate = DateTime.UtcNow,
				Reference = $"Project {Project.JobNumber}"
			});

			IssuedStockHistory.Insert(0, usage);

			IssueQuantity = 0;
			IssuedBy = string.Empty;

			RefreshSummary();
			OnPropertyChanged(nameof(AvailableQuantity));
			OnPropertyChanged(nameof(CanIssueStock));
		}

		[RelayCommand]
		private void ReturnStock()
		{
			if (!CanReturnStock || SelectedIssuedUsage == null) return;

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
				TransactionDate = DateTime.UtcNow,
				Reference = $"Return from Project {Project.JobNumber}"
			});

			IssuedStockHistory.Insert(0, usage);

			ReturnQuantity = 0;
			SelectedIssuedUsage = null;

			RefreshSummary();
			OnPropertyChanged(nameof(AvailableQuantity));
		}

		[RelayCommand]
		private void Save()
		{
			_projectRepository.Update(Project);
			RequestClose?.Invoke();
		}

		[RelayCommand]
		private void Cancel() => RequestClose?.Invoke();
	}
}
