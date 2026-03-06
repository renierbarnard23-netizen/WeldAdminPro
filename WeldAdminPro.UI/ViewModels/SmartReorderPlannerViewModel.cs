using System.Collections.ObjectModel;
using WeldAdminPro.Core.Analytics.Executive;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;
using WeldAdminPro.Data.Services;

namespace WeldAdminPro.UI.ViewModels
{
	public class SmartReorderPlannerViewModel
	{
		private readonly StockRepository _stockRepository;
		private readonly SmartReorderPlannerService _plannerService;
		private readonly MaterialDemandForecastService _forecastService;

		public ObservableCollection<StockItem> ReorderItems { get; set; }

		public ObservableCollection<MaterialDemandForecast> ForecastItems { get; set; }

		public SmartReorderPlannerViewModel()
		{
			_stockRepository = new StockRepository();
			_plannerService = new SmartReorderPlannerService();
			_forecastService = new MaterialDemandForecastService();

			var allItems = _stockRepository.GetAll();

			// Traditional reorder logic
			var reorderList = _plannerService.GetReorderItems(allItems);
			ReorderItems = new ObservableCollection<StockItem>(reorderList);

			// NEW: Demand Forecast
			var forecast = _forecastService.GenerateForecast();

			ForecastItems = new ObservableCollection<MaterialDemandForecast>(forecast);
		}
	}
}