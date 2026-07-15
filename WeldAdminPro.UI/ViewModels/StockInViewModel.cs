using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services.Inventory;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class StockInViewModel : ObservableObject
	{
        private readonly StockApplicationService _stockService;
        private readonly ProjectRepository _projectRepository;

		public ObservableCollection<StockItem> StockItems { get; }
		public ObservableCollection<Project> Projects { get; }

		// =====================================================
		// OBSERVABLE PROPERTIES
		// =====================================================

		[ObservableProperty]
		private StockItem? selectedStockItem;

		[ObservableProperty]
		private Project? selectedProject;

		[ObservableProperty]
		private int quantity;

		[ObservableProperty]
		private decimal unitCost;

		[ObservableProperty]
		private string reference = string.Empty;

		// =====================================================
		// CONSTRUCTOR
		// =====================================================

		public event Action? RequestClose;

		public StockInViewModel()
		{
			_stockService = new StockApplicationService();
			_projectRepository = new ProjectRepository();

			StockItems = new ObservableCollection<StockItem>(_stockService.GetStockItems());
			Projects = new ObservableCollection<Project>(_projectRepository.GetAll());
		}

		// =====================================================
		// COMMANDS
		// =====================================================

		[RelayCommand]
		private void Save()
		{
			if (SelectedStockItem == null)
				return;

			if (Quantity <= 0)
				return;

			if (UnitCost <= 0)
				return;

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

			_stockService.ReceiveStock(tx);

			RequestClose?.Invoke();
		}

		[RelayCommand]
		private void Cancel()
		{
			RequestClose?.Invoke();
		}
	}
}
