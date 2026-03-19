using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels.Dashboard
{
	public partial class ProductionExecutionViewModel : ObservableObject
	{
		private readonly WorkOrderRepository _repository;
		private readonly WorkOrderExecutionService _executionService;

		[ObservableProperty]
		private ObservableCollection<WorkOrder> readyWorkOrders = new();

		[ObservableProperty]
		private ObservableCollection<WorkOrder> runningWorkOrders = new();

		[ObservableProperty]
		private ObservableCollection<WorkOrder> completedToday = new();

		public ProductionControlTowerViewModel? ControlTower { get; set; }

		public ProductionExecutionViewModel()
		{
			_repository = new WorkOrderRepository();
			_executionService = new WorkOrderExecutionService(
	_repository,
	new MaterialValidator(
		new StockRepository(),
		new WorkOrderMaterialRepository()
	)
);
		}
		public void Refresh()
		{
			Load();
		}

		public void Load()
		{
			var all = _repository.GetAll().ToList();

			foreach (var w in all)
			{
				Console.WriteLine($"{w.WorkOrderNumber} - Status: {w.Status}");
			}

			ReadyWorkOrders = new ObservableCollection<WorkOrder>(
	all.Where(w =>
		w.Status == WorkOrderStatus.Ready));

			RunningWorkOrders = new ObservableCollection<WorkOrder>(
				all.Where(w =>
					w.Status == WorkOrderStatus.InProduction));

			CompletedToday = new ObservableCollection<WorkOrder>(
				all.Where(w =>
					w.Status == WorkOrderStatus.Completed &&
					w.CompletedOn?.Date == DateTime.Today));

			OnPropertyChanged(nameof(ReadyWorkOrders));
			OnPropertyChanged(nameof(RunningWorkOrders));
			OnPropertyChanged(nameof(CompletedToday));
		}

		[RelayCommand]
		private void Start(Guid id)
		{
			_executionService.StartWorkOrder(id);

			Load(); // reload lists from DB
			ControlTower?.Load();

			Console.WriteLine($"Started work order {id}");
		}
	}
}