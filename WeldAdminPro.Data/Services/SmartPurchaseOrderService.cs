using System;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class SmartPurchaseOrderService
	{
		private readonly StockRepository _stockRepository;
		private readonly PurchaseOrderRepository _poRepository;

		public SmartPurchaseOrderService()
		{
			_stockRepository = new StockRepository();
			_poRepository = new PurchaseOrderRepository();
		}

		// =====================================================
		// AUTO GENERATE PO FOR PROJECT
		// =====================================================
		public PurchaseOrder? GenerateAutoPO(Project project, string supplierName)
		{
			if (project == null)
				throw new ArgumentNullException(nameof(project));

			var stockItems = _stockRepository.GetAll();
			var availabilityService = new StockAvailabilityService();

			var po = new PurchaseOrder
			{
				Id = Guid.NewGuid(),
				ProjectId = project.Id,
				JobNumber = project.JobNumber,
				PONumber = _poRepository.GenerateNextPONumber(project.JobNumber),
				SupplierName = supplierName,
				CreatedDate = DateTime.UtcNow,
				Status = "Draft"
			};

			foreach (var item in stockItems)
			{
				// Skip items without Min/Max levels
				if (!item.MinLevel.HasValue || !item.MaxLevel.HasValue)
					continue;

				int available = availabilityService.GetAvailableQuantity(item.Id);

				// Only reorder if at or below minimum
				if (available <= (int)item.MinLevel.Value)
				{
					decimal suggestedDecimal = item.MaxLevel.Value - available;

					if (suggestedDecimal > 0)
					{
						int suggested = (int)Math.Ceiling(suggestedDecimal);

						var line = new PurchaseOrderLine
						{
							Id = Guid.NewGuid(),
							PurchaseOrderId = po.Id,
							StockItemId = item.Id,
							ItemCode = item.ItemCode,
							Description = item.Description,
							Quantity = suggested,
							UnitCost = item.AverageUnitCost
						};

						po.Lines.Add(line);
					}
				}
			}

			if (!po.Lines.Any())
				return null;

			return po;
		}
	}
}
