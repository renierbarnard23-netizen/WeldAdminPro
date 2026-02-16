using System.Collections.ObjectModel;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Core.Services;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.UI.ViewModels
{
	public class SmartReorderPlannerViewModel
	{
		private readonly StockRepository _stockRepository;
		private readonly SmartReorderPlannerService _plannerService;

		public ObservableCollection<StockItem> ReorderItems { get; set; }

		public SmartReorderPlannerViewModel()
		{
			_stockRepository = new StockRepository();
			_plannerService = new SmartReorderPlannerService();

			var allItems = _stockRepository.GetAll();
			var reorderList = _plannerService.GetReorderItems(allItems);

			ReorderItems = new ObservableCollection<StockItem>(reorderList);
		}
	}
}
