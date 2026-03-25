using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;
using System.Diagnostics;
using WeldAdminPro.Core.Execution;

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

		public ProductionExecutionViewModel(WorkOrderExecutionService executionService)
		{
			_executionService = executionService;
			_repository = new WorkOrderRepository();
		}
		//{
			//Console.WriteLine("🔥 ViewModel Constructor");

			//var repo = new WorkOrderRepository();
			//var materialRepo = new WorkOrderMaterialRepository();
			//var stockRepo = new StockRepository();

			//var validator = new MaterialValidator(stockRepo, materialRepo);

			//_executionService = new WorkOrderExecutionService(repo, materialRepo, validator);
			//_repository = repo;
		//}

		public void Refresh()
		{
			Load();
		}

		public void Load()
		{
			var all = _repository.GetAll().ToList();

			var engine = new BlockReasonEngine();
			var materialRepo = new WorkOrderMaterialRepository();
			var stockRepo = new StockRepository();

			foreach (var wo in all)
			{
				var materials = materialRepo.GetByWorkOrderId(wo.Id);

				// 🔹 AUTO TYPE
				wo.Type = materials.Any()
					? WorkOrderType.Production
					: WorkOrderType.Procurement;

				// 🔹 MAP MATERIALS → ENGINE FORMAT
				wo.MaterialRequirements = materials.Select(m =>
				{
					var stock = stockRepo.GetAll()
						.FirstOrDefault(s => s.ItemCode == m.ItemCode);

					return new MaterialRequirement
					{
						MaterialCode = m.ItemCode,
						RequiredQuantity = m.RequiredQuantity,
						AvailableQuantity = stock?.Quantity ?? 0
					};				

				// fallback (safety)
				if (stock == null)
					{
						stock = stockRepo.GetAll()
							.FirstOrDefault(s => s.ItemCode == m.ItemCode);
					}

					return new MaterialRequirement
					{
						MaterialCode = m.ItemCode, // use your actual code
						RequiredQuantity = m.RequiredQuantity,
						AvailableQuantity = stock?.Quantity ?? 0
					};
				}).ToList();

				// 🔹 ENGINE DECISION
				var result = engine.Evaluate(wo);

				wo.BlockReason = result.Reason;
				wo.BlockMessage = result.Message;

				Debug.WriteLine($"{wo.WorkOrderNumber} → {wo.BlockReason}");
			}

			ReadyWorkOrders = new ObservableCollection<WorkOrder>(
				all.Where(w => w.Status == WorkOrderStatus.Ready));

			RunningWorkOrders = new ObservableCollection<WorkOrder>(
				all.Where(w => w.Status == WorkOrderStatus.InProduction));

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

			try
			{
				_executionService.StartWorkOrder(id);

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
			try
			{
				_executionService.StartWorkOrder(id);

				Load();
				ControlTower?.Load();
			}
			catch (Exception ex)
			{
				Debug.WriteLine("❌ ERROR:");
				Debug.WriteLine(ex.ToString());
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