using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using System.Diagnostics;

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
			Console.WriteLine("🔥 ViewModel Constructor");

			var repo = new WorkOrderRepository();
			var materialRepo = new WorkOrderMaterialRepository();
			var stockRepo = new StockRepository();

			var validator = new MaterialValidator(stockRepo, materialRepo);

			_executionService = new WorkOrderExecutionService(repo, materialRepo, validator);
			_repository = repo;
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
				var materials = new WorkOrderMaterialRepository()
					.GetByWorkOrderId(w.Id);

				// 🔥 AUTO TYPE (UI SAFETY)
				if (!materials.Any())
					w.Type = WorkOrderType.Procurement;
				else
					w.Type = WorkOrderType.Production;

				// 🔴 BLOCK REASON LOGIC
				if (w.Type == WorkOrderType.Production)
				{
					if (!materials.Any())
					{
						w.BlockReason = "No materials linked";
					}
					else
					{
						var validator = new MaterialValidator(
							new StockRepository(),
							new WorkOrderMaterialRepository());

						if (!validator.CanStart(w, out var reason))
							w.BlockReason = reason;
						else
							w.BlockReason = null;
					}
				}
				else
				{
					w.BlockReason = null;
				}
			}

			foreach (var w in all)
			{
				Debug.WriteLine($"{w.WorkOrderNumber} - Status: {w.Status}");
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
			Debug.WriteLine("🚀 RELAY START TRIGGERED");

			Debug.WriteLine($"🔥 SERVICE TYPE: {_executionService.GetType().FullName}");

			try
			{
				_executionService.StartWorkOrder(id);

				Debug.WriteLine("✅ Execution service completed");

				Load();
				ControlTower?.Load();
			}
			catch (Exception ex)
			{
				Debug.WriteLine("❌ ERROR IN START:");
				Debug.WriteLine(ex.ToString());
			}
		}

		public void StartWorkOrder(Guid id)
		{
			System.Diagnostics.Debug.WriteLine("🚀 StartWorkOrder METHOD CALLED");

			try
			{
				_executionService.StartWorkOrder(id);

				System.Diagnostics.Debug.WriteLine("✅ Execution completed");

				Load();
				ControlTower?.Load();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("❌ ERROR:");
				System.Diagnostics.Debug.WriteLine(ex.ToString());
			}
		}

		public void PauseWorkOrder(Guid id)
		{
			_executionService.PauseWorkOrder(id);

			Load();
			ControlTower?.Load();
		}

		public void CompleteWorkOrder(Guid id)
		{
			_executionService.CompleteWorkOrder(id);

			Load();
			ControlTower?.Load();
		}
	}
}