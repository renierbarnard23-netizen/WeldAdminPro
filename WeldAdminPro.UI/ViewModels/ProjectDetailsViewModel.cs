using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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

		[ObservableProperty]
		private StockItem? selectedStockItem;

		[ObservableProperty]
		private decimal issueQuantity;

		[ObservableProperty]
		private string issuedBy = string.Empty;

		// =========================
		// 🔁 RETURN STOCK
		// =========================
		[ObservableProperty]
		private ProjectStockUsage? selectedIssuedUsage;

		[ObservableProperty]
		private decimal returnQuantity;

		public event Action? RequestClose;

		// =========================
		// EDIT LOCKING
		// =========================
		public bool IsEditable =>
			Project.Status != ProjectStatus.Completed &&
			Project.Status != ProjectStatus.Cancelled;

		// =========================
		// LIVE AVAILABLE STOCK
		// =========================
		public int AvailableQuantity =>
			SelectedStockItem == null
				? 0
				: _stockRepository.GetAvailableQuantity(SelectedStockItem.Id);

		public bool CanIssueStock =>
			IsEditable &&
			SelectedStockItem != null &&
			IssueQuantity > 0 &&
			IssueQuantity <= AvailableQuantity;

		// =========================
		// 🔑 REMAINING ISSUED BALANCE (AUTHORITATIVE)
		// =========================
		private decimal GetRemainingIssuedBalance(Guid stockItemId)
		{
			return IssuedStockHistory
				.Where(x => x.StockItemId == stockItemId)
				.Sum(x => x.Quantity);
		}

		public decimal RemainingIssuedBalance =>
			SelectedIssuedUsage == null
				? 0
				: GetRemainingIssuedBalance(SelectedIssuedUsage.StockItemId);

		public bool CanReturnStock =>
			IsEditable &&
			SelectedIssuedUsage != null &&
			ReturnQuantity > 0 &&
			ReturnQuantity <= RemainingIssuedBalance;

		// =========================
		// STATUS + INVOICE RULES
		// =========================
		public ProjectStatus Status
		{
			get => Project.Status;
			set
			{
				if (Project.Status == value)
					return;

				if (value == ProjectStatus.Completed)
				{
					if (!Project.IsInvoiced)
					{
						var confirm = MessageBox.Show(
							"This project is not marked as invoiced.\n\nMark it as invoiced before completion?",
							"Confirm Completion",
							MessageBoxButton.YesNo,
							MessageBoxImage.Question
						);

						if (confirm == MessageBoxResult.No)
						{
							OnPropertyChanged(nameof(Status));
							return;
						}

						Project.IsInvoiced = true;
					}

					if (string.IsNullOrWhiteSpace(Project.InvoiceNumber))
					{
						MessageBox.Show(
							"Invoice number is required before completing a project.",
							"Invoice Required",
							MessageBoxButton.OK,
							MessageBoxImage.Warning
						);
						OnPropertyChanged(nameof(Status));
						return;
					}

					Project.CompletedOn ??= DateTime.Now;
				}

				Project.Status = value;

				OnPropertyChanged(nameof(Status));
				OnPropertyChanged(nameof(IsEditable));
				OnPropertyChanged(nameof(CanIssueStock));
				OnPropertyChanged(nameof(CanReturnStock));
			}
		}

		// =========================
		// CONSTRUCTOR
		// =========================
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

			var stockLookup = StockItems.ToDictionary(s => s.Id, s => s.Description);

			var history = _usageRepository.GetByProjectId(Project.Id);
			foreach (var h in history)
			{
				h.Notes = stockLookup.TryGetValue(h.StockItemId, out var desc)
					? desc
					: h.StockItemId.ToString();
			}

			IssuedStockHistory = new ObservableCollection<ProjectStockUsage>(history);

			OnPropertyChanged(nameof(AvailableQuantity));
			OnPropertyChanged(nameof(CanIssueStock));
			OnPropertyChanged(nameof(CanReturnStock));
		}

		// =========================
		// PROPERTY CHANGE HOOKS
		// =========================
		partial void OnSelectedStockItemChanged(StockItem? value)
		{
			OnPropertyChanged(nameof(AvailableQuantity));
			OnPropertyChanged(nameof(CanIssueStock));
		}

		partial void OnIssueQuantityChanged(decimal value)
		{
			OnPropertyChanged(nameof(CanIssueStock));
		}

		partial void OnSelectedIssuedUsageChanged(ProjectStockUsage? value)
		{
			ReturnQuantity = 0;
			OnPropertyChanged(nameof(RemainingIssuedBalance));
			OnPropertyChanged(nameof(CanReturnStock));
		}

		partial void OnReturnQuantityChanged(decimal value)
		{
			OnPropertyChanged(nameof(CanReturnStock));
		}

		// =========================
		// SAVE
		// =========================
		[RelayCommand]
		private void Save()
		{
			if (Project.Status == ProjectStatus.Completed &&
				(!Project.IsInvoiced || string.IsNullOrWhiteSpace(Project.InvoiceNumber)))
			{
				MessageBox.Show(
					"Completed projects must remain invoiced with an invoice number.",
					"Invoice Lock",
					MessageBoxButton.OK,
					MessageBoxImage.Warning
				);
				return;
			}

			_projectRepository.Update(Project);
			RequestClose?.Invoke();
		}

		// =========================
		// ISSUE STOCK
		// =========================
		[RelayCommand]
		private void IssueStock()
		{
			if (!CanIssueStock)
			{
				MessageBox.Show(
					$"Insufficient stock.\n\nAvailable: {AvailableQuantity}",
					"Stock Validation",
					MessageBoxButton.OK,
					MessageBoxImage.Warning
				);
				return;
			}

			var usage = new ProjectStockUsage
			{
				Id = Guid.NewGuid(),
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
				StockItemId = SelectedStockItem.Id,
				Quantity = (int)IssueQuantity,
				Type = "OUT",
				TransactionDate = DateTime.UtcNow,
				Reference = $"Project {Project.JobNumber}"
			});

			IssuedStockHistory.Insert(0, usage);

			IssueQuantity = 0;
			IssuedBy = string.Empty;

			OnPropertyChanged(nameof(AvailableQuantity));
			OnPropertyChanged(nameof(CanIssueStock));
			OnPropertyChanged(nameof(CanReturnStock));
		}

		// =========================
		// 🔁 RETURN STOCK
		// =========================
		[RelayCommand]
		private void ReturnStock()
		{
			if (!CanReturnStock)
				return;

			var usage = new ProjectStockUsage
			{
				Id = Guid.NewGuid(),
				ProjectId = Project.Id,
				StockItemId = SelectedIssuedUsage!.StockItemId,
				Quantity = -ReturnQuantity,
				IssuedBy = IssuedBy,
				IssuedOn = DateTime.UtcNow,
				Notes = "RETURN"
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

			OnPropertyChanged(nameof(AvailableQuantity));
			OnPropertyChanged(nameof(CanReturnStock));
		}

		[RelayCommand]
		private void Cancel() => RequestClose?.Invoke();
	}
}
