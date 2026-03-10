using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Analytics.Production;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class MaterialReservationService
	{
		private readonly StockRepository _stockRepository;
		private readonly WorkOrderMaterialPlanningService _planningService;

		public MaterialReservationService()
		{
			_stockRepository = new StockRepository();
			_planningService = new WorkOrderMaterialPlanningService();
		}

		public List<MaterialReservation> GenerateReservations()
		{
			var plan = _planningService.BuildPlan();
			var stock = _stockRepository.GetAll();

			var reservations = new List<MaterialReservation>();

			foreach (var item in plan)
			{
				var stockItem = stock.FirstOrDefault(s => s.ItemCode == item.ItemCode);

				double available = stockItem?.Quantity ?? 0;

				double reserved = available >= item.RequiredQuantity
					? item.RequiredQuantity
					: available;

				reservations.Add(new MaterialReservation
				{
					WorkOrderNumber = item.WorkOrderNumber,
					ItemCode = item.ItemCode,
					RequiredQuantity = item.RequiredQuantity,
					ReservedQuantity = reserved,
					AvailableStock = available - reserved,
					ReservationSuccessful = reserved >= item.RequiredQuantity,
					Reason = reserved >= item.RequiredQuantity
						? "Reserved"
						: "Insufficient stock"
				});

				if (stockItem != null)
					stockItem.Quantity -= (int)reserved;
			}

			return reservations;
		}
	}
}