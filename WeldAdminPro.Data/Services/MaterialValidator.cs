using System;
using System.Collections.Generic;
using System.Linq;
using WeldAdminPro.Core.Models;
using WeldAdminPro.Data.Repositories;

namespace WeldAdminPro.Data.Services
{
	public class MaterialValidator
	{
		private readonly StockRepository _stockRepo;
		private readonly WorkOrderMaterialRepository _materialRepo;

		public MaterialValidator(
			StockRepository stockRepo,
			WorkOrderMaterialRepository materialRepo)
		{
			_stockRepo = stockRepo;
			_materialRepo = materialRepo;
		}

		public bool CanStart(WorkOrder workOrder, out string reason)
		{
			var materials = _materialRepo.GetByWorkOrder(workOrder.Id);

			if (materials == null || !materials.Any())
			{
				reason = "No materials assigned";
				return false;
			}

			foreach (var mat in materials)
			{
				var stock = _stockRepo.GetByItemCode(mat.ItemCode);

				if (stock == null)
				{
					reason = $"Stock item not found: {mat.ItemCode}";
					return false;
				}

				if (stock.Quantity < mat.RequiredQuantity)
				{
					reason = $"Insufficient stock for {mat.ItemCode}";
					return false;
				}

				if (stock.Quantity < 0)
				{
					reason = $"Negative stock detected for {mat.ItemCode}";
					return false;
				}
			}

			reason = string.Empty;
			return true;
		}
	}
}