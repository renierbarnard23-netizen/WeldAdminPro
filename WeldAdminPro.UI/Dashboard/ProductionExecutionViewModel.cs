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

		public ObservableCollection<WorkOrder> ReadyWorkOrders { get; set; } = new();
		public ObservableCollection<WorkOrder> RunningWorkOrders { get; set; } = new();
		public ObservableCollection<WorkOrder> CompletedToday { get; set; } = new();

		public ProductionExecutionViewModel()
		{
			_repository = new WorkOrderRepository();
			_executionService = new WorkOrderExecutionService(_repository);

			Load();
		}

		private void Load()
		{
			var all = _repository.GetAll();

			ReadyWorkOrders =
				new ObservableCollection<WorkOrder>(
					all.Where(w => w.Status == WorkOrderStatus.Ready));

			RunningWorkOrders =
				new ObservableCollection<WorkOrder>(
					all.Where(w => w.Status == WorkOrderStatus.InProduction));

			CompletedToday =
				new ObservableCollection<WorkOrder>(
					all.Where(w =>
						w.Status == WorkOrderStatus.Completed &&
						w.CompletedOn?.Date == DateTime.Today));
		}

		[RelayCommand]
		private void Start(Guid id)
		{
			_executionService.StartWorkOrder(id);
			Load();
		}

		[RelayCommand]
		private void Complete(Guid id)
		{
			_executionService.CompleteWorkOrder(id);
			Load();
		}
	}
}