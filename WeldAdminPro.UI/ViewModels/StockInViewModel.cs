using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class StockInViewModel : ObservableObject
	{
		private readonly StockRepository _repository;
		private readonly ProjectRepository _projectRepository;

		public ObservableCollection<StockItem> StockItems { get; }
		public ObservableCollection<Project> Projects { get; }

		[ObservableProperty]
		private StockItem? selectedStockItem;

		partial void OnSelectedStockItemChanged(StockItem? value)
		{
			if (value != null)
				UnitCost = value.AverageUnitCost;
		}

		[ObservableProperty]
		private Project? selectedProject;

		[ObservableProperty]
		private int quantity;

		[ObservableProperty]
		private decimal unitCost;

		[ObservableProperty]
		private string reference = string.Empty;

		public event Action? RequestClose;

		public StockInViewModel()
		{
			_repository = new StockRepository();
			_projectRepository = new ProjectRepository();

			StockItems = new ObservableCollection<StockItem>(_repository.GetAll());
			Projects = new ObservableCollection<Project>(_projectRepository.GetAll());
		}

		[RelayCommand]
		private void Save()
		{
			if (SelectedStockItem == null)
				return;

			if (Quantity <= 0)
				return;

			if (UnitCost <= 0)
				return;

			// Weighted average calculation
			var currentQty = SelectedStockItem.Quantity;
			var currentAvg = SelectedStockItem.AverageUnitCost;

			var newTotalQty = currentQty + Quantity;

			var newAverage =
				newTotalQty == 0
				? UnitCost
				: ((currentQty * currentAvg) + (Quantity * UnitCost)) / newTotalQty;

			var tx = new StockTransaction
			{
				Id = Guid.NewGuid(),
				StockItemId = SelectedStockItem.Id,
				ProjectId = SelectedProject?.Id,
				Quantity = Quantity,
				Type = "IN",
				UnitCost = UnitCost,
				TransactionDate = DateTime.UtcNow,
				Reference = Reference
			};

			_repository.AddTransaction(tx);

			// Update stock average cost
			SelectedStockItem.AverageUnitCost = Math.Round(newAverage, 2);
			SelectedStockItem.Quantity = newTotalQty;

			_repository.Update(SelectedStockItem);

			RequestClose?.Invoke();
		}

		[RelayCommand]
		private void Cancel()
		{
			RequestClose?.Invoke();
		}
	}
}
