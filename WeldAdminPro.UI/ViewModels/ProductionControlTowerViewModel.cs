using CommunityToolkit.Mvvm.ComponentModel;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public partial class ProductionControlTowerViewModel : ObservableObject
	{
		private readonly ProductionControlTowerService _service;

		[ObservableProperty]
		private int readyOrders;

		[ObservableProperty]
		private int runningOrders;

		[ObservableProperty]
		private int blockedOrders;

		[ObservableProperty]
		private int completedToday;

		[ObservableProperty]
		private double capacityLoad;

		[ObservableProperty]
		private int deadlineRisks;

		public ProductionControlTowerViewModel()
		{
			_service = new ProductionControlTowerService();
			Load();
		}

		public void Load()
		{
			ProductionControlTowerModel data = _service.GetControlTower();

			ReadyOrders = data.ReadyOrders;
			RunningOrders = data.RunningOrders;
			BlockedOrders = data.BlockedOrders;
			CompletedToday = data.CompletedToday;
			CapacityLoad = data.CapacityLoad;
			DeadlineRisks = data.DeadlineRisks;
		}

		public bool IsCapacityWarning => CapacityLoad >= 80 && CapacityLoad < 100;
		public bool IsCapacityCritical => CapacityLoad >= 100;
	}
}