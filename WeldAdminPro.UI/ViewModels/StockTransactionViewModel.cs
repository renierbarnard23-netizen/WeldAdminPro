using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Globalization;
using System.Windows;
using System.Collections.ObjectModel;          // ✅ ADDED
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class StockTransactionViewModel : ObservableObject
	{
		private readonly StockRepository _repo;
		private readonly ProjectRepository _projectRepo;   // ✅ ADDED
		private readonly bool _isStockIn;

		public StockItem Item { get; }

		// ✅ ADDED: Project support
		public ObservableCollection<Project> Projects { get; } = new();

		[ObservableProperty]
		private Project? selectedProject;

		[ObservableProperty]
		private string quantityText = string.Empty;

		[ObservableProperty]
		private string reference = string.Empty;

		public string Title => _isStockIn ? "Stock IN" : "Stock OUT";

		public IRelayCommand SaveCommand { get; }
		public IRelayCommand CancelCommand { get; }

		public event Action? TransactionCompleted;
		public event Action? RequestClose;

		public StockTransactionViewModel(StockItem item, bool isStockIn)
		{
			Item = item;
			_isStockIn = isStockIn;
			_repo = new StockRepository();
			_projectRepo = new ProjectRepository();   // ✅ ADDED

			LoadProjects();                            // ✅ ADDED

			SaveCommand = new RelayCommand(Save);
			CancelCommand = new RelayCommand(() => RequestClose?.Invoke());
		}

		// ✅ ADDED
		private void LoadProjects()
		{
			Projects.Clear();

			foreach (var project in _projectRepo.GetAll())
				Projects.Add(project);
		}

		private void Save()
		{
			if (!int.TryParse(
				QuantityText,
				NumberStyles.Integer,
				CultureInfo.InvariantCulture,
				out var quantity) || quantity <= 0)
			{
				MessageBox.Show("Please enter a valid quantity greater than zero.");
				return;
			}

			// 🔐 Stock OUT safety
			if (!_isStockIn && quantity > Item.Quantity)
			{
				MessageBox.Show("Cannot stock out more than the available quantity.");
				return;
			}

			// ✅ ADDED: Require project for Stock OUT
			if (!_isStockIn && SelectedProject == null)
			{
				MessageBox.Show("Please select a project for Stock OUT.");
				return;
			}

			var tx = new StockTransaction
			{
				Id = Guid.NewGuid(),
				StockItemId = Item.Id,
				ProjectId = !_isStockIn ? SelectedProject?.Id : null,   // ✅ ADDED
				TransactionDate = DateTime.Now,
				Quantity = quantity,
				Type = _isStockIn ? "IN" : "OUT",
				Reference = Reference
			};

			_repo.AddTransaction(tx);

			TransactionCompleted?.Invoke();
			RequestClose?.Invoke();
		}
	}
}
